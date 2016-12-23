using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
//using log4net;

namespace DotNet.Config
{
    /// <summary>
    /// This class uses a java style .properties file to load, apply ('glue'), and save settings. 
    /// 
    /// I've found this approach and helper class is far easier than using app.config for several reasons:
    /// 
    /// 1) Non-technical folks have no problem editing .ini "name=value" style files, but they struggle with XML
    /// 2) Values with quotes in xml are a headache - you have to use '&quot;' which gets messy
    /// 3) If you're writing a packaged dll, it needs to use the entry point's app.config which we don't control. 
    ///    Using this approach, you can ship a 'MyDll.config' 
    /// 4) The 'glueOn' approach is really handy. It'll just stick values from the .properties file onto your class,
    ///    regardless of whether they're public, private, or static. If you use a 'private static string _name' convention,
    ///    it'll even take 'name=value' from the properties file (no underscore) and apply it
    /// 5) Saving settings back to app.config is a royal headache. Here, you just call Save(name, value) and it's done
    /// 
    /// This class also takes into account loading the properties file from the same directory as the executing assembly,
    /// even when installed as a service, so you don't run into the issue of the current path being sys32 or whatever
    /// execution location services are started from.
    /// </summary>

    public class AppSettings
    {
        //The default config file name is 'config.properties'. 
        private static string defaultConfigFileName = "config.properties";
        private static string defaultConfigFileFullPath = Path.Combine(GetAssemblyDirectory(), defaultConfigFileName);

        private static Dictionary<string, string> appSettings;
        //private static ILog log = LogManager.GetLogger(typeof(AppSettings));

        //used for unit testing to confirm we only do the work once
        public static int CacheCount = 0;

        /// <summary>
        /// Uses the default 'config.properties' file
        /// </summary>
        public static void GlueOnto(object o)
        {
            GlueOnto(o, defaultConfigFileFullPath);
        }

        public static Dictionary<string, string> Retrieve()
        {
            return Retrieve(defaultConfigFileFullPath);
        }

        public static Dictionary<string, string> Retrieve(string configFile)
        {

            try
            {
                return _Retrieve(configFile);
            }
            catch (ArgumentException ex)
            {
                string message = "Key already exists: check " + configFile + " for duplicate settings. You may have accidentally commented/uncommented one so it appears twice";
                throw new ArgumentException(message);
            }
        }

        private static Dictionary<string, string> _Retrieve(string configFile)
        {
            //NOTE that if we need to use different config files at once this will need to be updated to 
            //check it's the same config file
            if (appSettings != null)
                return appSettings;

            ++CacheCount;

            if (!File.Exists(configFile))
            {
                configFile = Path.Combine(GetAssemblyDirectory(), configFile);
                if (!File.Exists(configFile))
                {
                    string message = "> Failed to locate config file " + configFile;
                    //log.Error(message);
                    throw new FileNotFoundException(message);
                }
            }

            var lines = File.ReadAllLines(configFile)
                .Where(line => !line.TrimStart().StartsWith("#"))
                .ToList();

            var temp = new Dictionary<string, string>();
            string n = null;
            string v = null;

            const string nameValueSettingLinePattern = @"^[^=\s]+\s*=\s*[^\s]+";
            for (int i = 0; i < lines.Count; i++)
            {
				string line = lines[i].Replace("\t", "    ");

                if (Regex.IsMatch(line, nameValueSettingLinePattern))
                {
                    var s = line.Split('=');
                    n = s[0].Trim();
                    v = line.Substring(line.IndexOf('=') + 1);

                    //allow for comments on the same line following the value like name=value #this is a comment
                    //2015-10-01: don't allow comments on the same line because it interferes with 
                    //cases like colors.one = #FF0000;
                    //v = Regex.Replace(v, @"\s+#.*$", "");
                }
                else if (n != null && v != null && Regex.IsMatch(line, @"^\s{3,}[^ ]"))//multi-liners must indent at least 3 spaces
                {
                    /*  a multi-line value like:
                     * name=hello there
                     *      how are you today ?
                     */
                    //v += Environment.NewLine + line.Trim(); //preserve line breaks!
                    v += " " + line.Trim(); //do NOT preserve line breaks!
                }
               
                if( (n != null && v != null) && 
                    (i == lines.Count - 1  //we're at the end
                        || Regex.IsMatch(lines[i+1], nameValueSettingLinePattern) //or the next line marks a new pair 
                        || String.IsNullOrWhiteSpace(lines[i+1]) //or the next line is empty 
                    ))
                {
                    if (!temp.ContainsKey(n))
                    {
                        temp.Add(n, v);
                    }
                    else
                    {
                        throw new ArgumentException("Your config file contains multiple entires for " + n);
                    }

                    n = null;
                    v = null;
                }

            }

            var nameValuePairs = new Dictionary<string, string>();

            foreach (var pair in temp)
            {
                string name = pair.Key;
                string value = pair.Value;

                if (value.Contains("$"))
                {
                    //do we have some setting that can resolve this ?
                    foreach (var pair2 in temp)
                    {
                        if (value.Contains("$" + pair2.Key))
                        {
                            value = value.Replace("$" + pair2.Key, pair2.Value);
                        }
                    };
                }

                if (value.Contains("$"))
                {
                    value = value.Replace("$PATH", GetAssemblyDirectory())
                                 .Replace("$TIMESTAMP", DateTime.Now.ToString("yyyyMMdd"));
                }
                
                //cleanup multiline stuff
                value  = Regex.Replace(value, "[\n\r\t]", " ");

                nameValuePairs.Add(name, value.Trim());
            }

            appSettings = nameValuePairs;

            return nameValuePairs;
        }

        public static void GlueOnto(object o, string configFile)
        {
            var appSettings = _Retrieve(configFile);

            BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Public
                             | BindingFlags.SetField | BindingFlags.Static
                             | BindingFlags.Instance | BindingFlags.SetProperty
                             | BindingFlags.NonPublic;

            foreach (var pair in appSettings)
            {
                string name = pair.Key;
                string value = pair.Value;

                var fieldInfo = o.GetType().GetField(name, flags);
                if (fieldInfo == null)
                {
                    name = "_" + name; // try the _variable naming convention
                    fieldInfo = o.GetType().GetField(name, flags);
                }
                if(fieldInfo == null && name.Contains("."))
                {
                    //might be a list
                    string listName = pair.Key.Split('.')[0];
                    fieldInfo = o.GetType().GetField(listName, flags);
                }

                if (fieldInfo == null)
                {
                    /*
                     * Not im use: we're not being strict about settings and properties having to match up
                    string errorMessage = "Your config file had a setting for " + line + " but no member variable was found " +
                                           " in " + o.GetType().Name +
                                           " to glue this on to. Either comment out the line with '#' or add a member variable. " +
                                           "Unable to proceed.";
                    log.Debug(errorMessage);
                    throw new MissingMemberException(errorMessage);
                    */
                    continue;
                }

                Type ft = fieldInfo.FieldType;

                if (ft == typeof(double))
                    fieldInfo.SetValue(o, Convert.ToDouble(value));
                else if (ft == typeof(int))
                    fieldInfo.SetValue(o, Convert.ToInt32(value));
                else if (ft == typeof(bool))
                    fieldInfo.SetValue(o, Convert.ToBoolean(value));
                else if (ft.BaseType == typeof(Enum))
                    fieldInfo.SetValue(o, Enum.Parse(ft, value));
                else if (ft.BaseType == typeof(DateTime))
                    fieldInfo.SetValue(o, DateTime.Parse(value));
                else if (ft.FullName.StartsWith("System.Collections.Generic.List"))
                {
                    if (fieldInfo.GetValue(o) == null)
                    {
                        object fieldInstance = Activator.CreateInstance(ft);
                        object newType = Convert.ChangeType(fieldInstance, fieldInfo.FieldType);
                        fieldInfo.SetValue(o, newType);
                    }
                    var field = fieldInfo.GetValue(o);
                    var genericListType = fieldInfo.FieldType.UnderlyingSystemType.GenericTypeArguments[0].UnderlyingSystemType;
                    
                    object castValue;
                    //support for lists of enums (TODO: I think we can do this w/o the Enum parse)
                    if (genericListType.BaseType == typeof(Enum))
                        castValue = Enum.Parse(genericListType, value);
                    else 
                        castValue = Convert.ChangeType(value, genericListType);

                    var addMethod = fieldInfo.FieldType.GetMethod("Add");
                    addMethod.Invoke(field, new object[] { castValue });
                }
                else
                    fieldInfo.SetValue(o, value);

                //log.Info("> " + name + "=" + value);
            }

        }

        /// <summary>
        /// NOT WELL TESTED.
        /// </summary>
        public static void Save(String name, String value)
        {
            //NOTE: Loading up the file each time we want to save one setting isn't the most efficient thing in the world,
            //but since we'll only ever be saving a couple settings, it's ok for the time being


            if (!File.Exists(defaultConfigFileFullPath))
            {
                string message = "AppsettingSave - Failed to find config file using full assembly path '" + defaultConfigFileFullPath + "'";
                //log.Error(message);
                throw new FileNotFoundException(message);
            }

            var lines = File.ReadAllLines(defaultConfigFileFullPath);

            bool wasUpdated = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(name + "="))
                {
                    lines[i] = name + "=" + value;
                    wasUpdated = true;
                    break;
                }
            }

            if (wasUpdated)
            {
                //log.Info("trying to commit config file:" + lines);
                File.WriteAllLines(defaultConfigFileFullPath, lines);
               
            }
        }

        public static string GetAssemblyDirectory()
        {
            //TODO: AppDomain.CurrentDomain.BaseDirectory;?
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

    }
}
