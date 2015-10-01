using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;

namespace DotNet.Config
{
    [TestFixture]
    public class TestAppSettings
    {
        public enum Size { small, medium, large };

        private string _test0;
        private string test1;
        private string test2;
        private string test3;
        private string test4;
        public string test5; //public or private - both work
        private string test6;
        private string test7;
        private int test8;
        private DateTime test9;
        private Size size; 
        private List<string> colors;
        private List<int> numbers;
        private List<Size> sizes;

        [SetUp]
        public void Setup()
        {
            AppSettings.GlueOnto(this, "../../config.properties");
        }

        [Test]
        public void TestSimple()
        {
            Assert.AreEqual(test1, "someValue");
        }

        [Test]
        public void TestUnderscore()
        {
            Assert.AreEqual(_test0, "test underscores");
        }

        [Test]
        public void Test_Equals_In_Value()
        {
            Assert.AreEqual(test2, "dsn=test");
        }

        [Test]
        public void Test_Timestamp_Substitution()
        {
            Assert.True(test3.Contains(DateTime.Now.Year.ToString()));
        }

        [Test]
        public void Test_Variable_Substitution()
        {
            Assert.AreEqual(test5, "Hello someValue");
        }


        [Test]
        public void Test_Multi_Line_String()
        {
            //multiline strings in the config are set as single line values (breaks removed)
            Assert.AreEqual(test7, "Select a,b,c,d from tableA as A  join tableB as B on A.foo = B.foo where something > 'something' and \"oh look some quotes and an ampersand &\"" );
        }

        [Test]
        public void Test_Cast_Int()
        {
            Assert.AreEqual(test8, 1234);
        }

        [Test]
        public void Test_Cast_DateTime()
        {
            Assert.AreEqual(new DateTime(2016, 3, 21), test9);
        }

        [Test]
        public void Test_Long_String()
        {
            Assert.IsTrue(this.test6.StartsWith("What, my young master")
                            &&
                          this.test6.EndsWith("Envenoms him that bears it!"));
        }
        
        [Test]
        public void Test_It_Is_Caching()
        {
            for (int i = 0; i < 10; i++)
            {
                var test = AppSettings.Retrieve()["test1"];
            }

            Assert.AreEqual(AppSettings.CacheCount, 1);
        }

        [Test]
        public void Test_Enum()
        {
            Assert.AreEqual(this.size, Size.medium);
        }

        [Test]
        public void Test_List_of_Strings()
        {
            string[] expected = new string[] { "#FF0000;", "#00FF00;", "#0000FF;" };
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(this.colors.ElementAt(i), expected[i]);
        }

        [Test]
        public void Test_List_of_Integers()
        {
            int[] expected = new int[] { 10, 20, 30, 40 };
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(this.numbers.ElementAt(i), expected[i]);
        }

        [Test]
        public void Test_List_of_Enums() 
        {
            Size[] expected = new Size[] { Size.small, Size.medium, Size.large };
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(this.sizes.ElementAt(i), expected[i]);
        }
    }
}
