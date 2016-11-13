using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using APIServerModule;
using RogyWatchCommon;

namespace APIServerTest
{
    /// <summary>
    /// Summary description for WSServer
    /// </summary>
    [TestClass]
    public class WSServer
    {
        public WSServer()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void WebSocketServer_Test()
        {
            var core = new APIServerCore() { Config = new Config() };
            APIServerExterior.StartWebSocketServer(core);
            Console.ReadLine();
            APIServerExterior.CloseWebSocket();
        }

        [TestMethod]
        public void ControlServerCore_Test()
        {
            var control = new APIServerCoreTest();
            Assert.AreEqual(control.Echo(new [] { "1", "2" }), "Hello 12");
            Assert.AreEqual(control.Invoke<IEnumerable<string>>("Echo 1 2").ToString(), "Hello 12");
        }
    }
}
