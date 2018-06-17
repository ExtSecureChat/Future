using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExtSecureChat.Future.Tests
{
    [TestClass]
    public class PromiseTest
    {
        public string TestString = "Test";

        [TestMethod]
        public void TestPromiseReturnString()
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
        public void TestPromiseCatch()
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
        public void TestPromiseCancel()
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

        [TestMethod]
        public void TestPromiseAll()
        {
            bool promise1Completed = false, promise2Completed = false;

            var promise1 = new Promise(() =>
            {
                return true;
            }).Then(res =>
            {
                promise1Completed = res;
            });

            var promise2 = new Promise(() =>
            {
                return true;
            }).Then(res =>
            {
                promise2Completed = res;
            });

            var finalPromise = Promise.All(promise1, promise2);
            finalPromise.Wait();
            Assert.AreEqual(true, finalPromise.Completed);
        }

        [TestMethod]
        public void TestPromiseRace()
        {
            var promise1 = new Promise(() =>
            {
                return "promise1";
            });

            var promise2 = new Promise(() =>
            {
                return "promise2";
            });

            var finalPromise = Promise.Race(promise1, promise2);
            Assert.AreEqual(true, finalPromise.Completed);
            Assert.IsNotNull(finalPromise.Result);
        }
    }
}
