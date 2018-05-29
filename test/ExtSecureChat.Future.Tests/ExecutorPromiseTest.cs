using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExtSecureChat.Future.Tests
{
    [TestClass]
    public class ExecutorPromiseTest
    {
        public string TestString = "Test";

        [TestMethod]
        public void TestExecutorPromiseReturnString()
        {
            string ret = String.Empty;

            var promise = new Promise((resolve, reject) =>
            {
                resolve(TestString);
            }).Then(res =>
            {
                ret = res;
            });

            promise.Wait();
            Assert.AreEqual(TestString, ret);
        }

        [TestMethod]
        public void TestExecutorPromiseCatch()
        {
            string errorMessage = String.Empty;

            var promise = new Promise((resolve, reject) =>
            {
                reject(TestString);
            }).Catch(err =>
            {
                errorMessage = err.Message;
            });

            promise.Wait();
            Assert.AreEqual(TestString, errorMessage);
        }
    }
}
