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

            // Create a new executor promise
            var promise = new Promise((resolve, reject) =>
            {
                // Resolve the promise with the variable TestString as result
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

            // Create a new executor promise
            var promise = new Promise(reject =>
            {
                // Reject the promise with the variable TestString as result
                reject(new Exception(TestString));
            }).Catch(err =>
            {
                errorMessage = err.Message;
            });

            promise.Wait();
            Assert.AreEqual(TestString, errorMessage);
        }

        [TestMethod]
        public void TestExecutorPromiseCancel()
        {
            var promise = new Promise(resolve =>
            {
                while (true)
                {
                    // Infinite loop to cancel
                }
                // This is required or visual studio says ambigous
                resolve();
            });

            promise.Cancel();
            promise.Wait();
            Assert.AreEqual(true, promise.Cancelled);
        }
    }
}
