using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using Clean_BaseLib_TestLib;

namespace Clean_BaseLib_Tests
{
    [TestClass]
    public class TestClass_ex0000
    {
        [TestMethod]
        public async void Execute_CaseFunction_Exception()
        {
            try
            {
                ExeSysTestProcess testProcess = new ExeSysTestProcess
                {
                    argsIn = "0000", 
                    testFunction = ex0000_ExecutionSystem.testCaseFunctionException, 
                    msTimeOut = 5000 
                };
                Assert.IsTrue(await testProcess.TryTest());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
