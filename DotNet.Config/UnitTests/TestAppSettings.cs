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
        private string test1;
        private string _test1;
        private string test2;
        private string test3;
        private string test4;
        private string test5;
        private string test6;
        private string test7;
        private int test8;
        private DateTime test9;

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
            Assert.AreEqual(_test1, "someValue");
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
            Assert.AreEqual(test5, "hello someValue");
        }

        [Test]
        public void Test_Same_Line_Comments_Removed()
        {
            Assert.AreEqual(test6, "Here is a setting");
        }

        [Test]
        public void Test_Multi_Line_String()
        {
            //multiline strings in the config are set as single line values (breaks removed)
            Assert.AreEqual(test7, "Select a,b,c,d from tableA as A  join tableB as B on A.foo = B.foo where something=something" );
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
        public void Test_It_Is_Caching()
        {
            for (int i = 0; i < 10; i++)
            {
                var test = AppSettings.Retrieve()["test1"];
            }

            Assert.AreEqual(AppSettings.CacheCount, 1);
        }

    }
}
