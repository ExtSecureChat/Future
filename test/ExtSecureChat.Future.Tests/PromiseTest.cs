using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExtSecureChat.Future.Tests
{
    [TestClass]
    public class PromiseTest
    {
        public string TestString = "Test";

        [TestMethod]
        public void TestReturnString()
        {
            string ret = String.Empty;

            var promise = new Promise(() =>
            {
                return TestString;
            }).Then(res =>
            {
                ret = res;
            });

            promise.Wait();
            Assert.AreEqual(TestString, ret);
        }

        [TestMethod]
        public void TestCatchError()
        {
            string errorMessage = String.Empty;

            var promise = new Promise(() =>
            {
                throw new Exception(TestString);
            }).Catch(err =>
            {
                errorMessage = err.Message;
            });

            promise.Wait();
            Assert.AreEqual(TestString, errorMessage);
        }

        [TestMethod]
        public void TestCancel()
        {
            var promise = new Promise(() =>
            {
                while (true)
                {
                    // Infinite loop to cancel
                }
            });

            promise.Cancel();
            promise.Wait();
            Assert.AreEqual(true, promise.Cancelled);
        }
    }
}
