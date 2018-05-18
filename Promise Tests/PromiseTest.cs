using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExtSecureChat.Future.Tests
{
    [TestClass]
    public class PromiseTest
    {
        public string TestString = "Test";

        [TestMethod]
        public void ThenReturnString()
        {
            string ret = String.Empty;
            var promise = new Promise<string>(() =>
            {
                return TestString;
            }, 
            then: res =>
            {
                ret = res;
            });

            promise.Wait();
            Assert.AreEqual(TestString, ret);
        }

        [TestMethod]
        public void CatchError()
        {
            var promise = new Promise<string>(() =>
            {
                throw new Exception(TestString);
            },
            except: ex =>
            {
                Assert.AreEqual(TestString, ex.InnerException.Message);
            });

            promise.Wait();
        }
    }
}
