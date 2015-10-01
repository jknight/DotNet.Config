using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.Config;

namespace DotNet.Config.SampleUsage
{
    class Program
    {
        public enum Color { Red, Blue, Green };

        #region Glue-on properties
        private int numberSetting;
        private string stringSetting;
        private Color enumSetting;
        private DateTime dateTimeSetting;
        #endregion

        static void Main(string[] args)
        {
            //you can get settings explicitly:
           Console.WriteLine(AppSettings.Retrieve("config.properties")["stringSetting"]);

           new Program().Init();

            Console.ReadLine();
        }

        public void Init()
        {
            //or use a shorthand to glue them on:
            AppSettings.GlueOnto(this);

            Console.WriteLine("numberSetting:" + numberSetting);
            Console.WriteLine("stringSetting:" + stringSetting);
            Console.WriteLine("enumSetting:" + enumSetting);
            Console.WriteLine("dateTimeSetting:" + dateTimeSetting);
        }
    }
}
