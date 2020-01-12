using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using Clean_BaseLib;
using Clean_BaseLib_TestLib;

namespace Clean_BaseLib_Tests
{
    [TestClass]
    public class TestClass_ex0000
    {
        [TestMethod]
        public async Task Execute_CaseFunction_Exception()
        {
            try
            {
                ExeSysTestProcess testProcess = new ExeSysTestProcess
                {
                    argsIn = "0000",
                    TestConversation = ex0000_ExecutionSystem.testCaseFunctionException,
                    DefaultSerializationType = BaseClass_Sys_ExecutionPolicy.default_serialization,
                    msTimeOut = 5000 
                };
                bool TestResult = await testProcess.TryTest();
                Assert.IsTrue(TestResult);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
