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
        private int myInteger;
        private string firstName;
        private Color color;
        private DateTime dateTimeSetting;
        private List<string> myList;
        private List<int> sizes;
        #endregion

        static void Main(string[] args)
        {
            //you can get settings explicitly  without "glueing" them on.
            //This is useful in a static context.

            string firstName = AppSettings.Retrieve("config.properties")["firstName"];
            Console.WriteLine("Seting loaded from explicit config file name: " + firstName);

            //Or assuming the default "config.properties":
            string firstName2 = AppSettings.Retrieve()["firstName"];
            Console.WriteLine("Setting loaded from default config file:" + firstName2);

            new Program().Init();

            Console.ReadLine();
        }

        public void Init()
        {
            //or use a convention-over-configuration shorthand to glue them on:
            AppSettings.GlueOnto(this); //<-- That's it !`

            Console.WriteLine("myInteger:" + this.myInteger);
            Console.WriteLine("firstName:" + this.firstName);
            Console.WriteLine("color:" + this.color.ToString());
            Console.WriteLine("dateTimeSetting:" + this.dateTimeSetting);

            Console.WriteLine("A list of strings:");
            for(int i = 0; i < this.myList.Count; i++)
                Console.WriteLine(i + ") " + this.myList.ElementAt(i));

            Console.WriteLine("A list of integers:");
            for(int i = 0; i < this.sizes.Count; i++)
                Console.WriteLine(i + ") " + this.sizes.ElementAt(i));

        }
    }
}
