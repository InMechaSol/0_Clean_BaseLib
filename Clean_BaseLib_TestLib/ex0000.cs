using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clean_BaseLib;
using System.Diagnostics;
using System.ComponentModel;

namespace Clean_BaseLib_TestLib
{
    public class ex0000_ExecutionSystem : BaseClass_ModuleExecutionSystem
    {
        public static void testCaseFunctionException(Process testProcess, out bool testPassed, out bool exitTest)
        {
            System.Threading.Thread.Sleep(100000);
            testPassed = true;
            exitTest = true;
        }
    }
    public class ex0000_Module1 : BaseClass_Module
    {
        public ex0000_Module1(BaseClass_ModuleExecutionSystem exeSysIn) : base(exeSysIn) { }
    }

    public class ExeSysTestProcess
    {
        public string argsIn { get; set; }
        public TestCase testFunction { get; set; }
        public int msTimeOut { get; set; }

        public static readonly String titleString = "InMechaSol's Clean Base Lib - Example ";
        public static readonly string TestAppExePathString = "Clean_BaseLib_TestApplication.exe";
        
        protected bool firstStart = true;
        protected bool ExitTest = false;
        protected bool PassedTest = false;
        protected bool testTimedOut = false;
        
        protected System.Threading.Timer WD_Timer;
        protected BackgroundWorker TestWorker;
        
        protected List<String> StdOutStrings;
        protected List<String> StdErrStrings;
        protected List<String> StdInStrings; 
        
        public async Task<bool> TryTest()
        {
            int stateVar = 0;
            firstStart = true;
            ExitTest = false;
            PassedTest = false;
            testTimedOut = false;
            try
            {
                WD_Timer = new System.Threading.Timer(WD_TimerCallback, testTimedOut, msTimeOut, 10);
                TestWorker = new BackgroundWorker();
                TestWorker.DoWork += TestWorker_DoWork;
                StdOutStrings = new List<String>();
                StdErrStrings = new List<String>();
                StdInStrings = new List<String>();
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.RedirectStandardInput = true;
                    myProcess.StandardInput.AutoFlush = true;
                    myProcess.StartInfo.RedirectStandardOutput = true;
                    myProcess.StartInfo.RedirectStandardError = true;

                    myProcess.StartInfo.FileName = TestAppExePathString;
                    myProcess.StartInfo.Arguments = argsIn;

                    do
                    {
                        switch(stateVar)
                        {
                            case 0:
                                {
                                    if (myProcess.Start())
                                    {
                                        if (firstStart)
                                        {
                                            // close stream
                                            myProcess.StandardInput.Close();
                                            System.Threading.Thread.Sleep(10);

                                            // read stdout for test preamble
                                            StdOutStrings.Add(await myProcess.StandardOutput.ReadToEndAsync());

                                            // ensure starts with title string and ends with argIn string
                                            if (!StdOutStrings[0].StartsWith(titleString))
                                                throw new Exception("Fail - Preamble received doesn't match title string.");
                                            if (!StdOutStrings[0].EndsWith(argsIn))
                                                throw new Exception("Fail - Preamble received doesn't match title string.");

                                            firstStart = false;
                                            break;
                                        }

                                        // write test packet
                                        StdInStrings.Add();
                                        myProcess.StandardInput.Write(StdInStrings[StdInStrings.Count-1]);

                                        // close stream
                                        myProcess.StandardInput.Close();
                                        System.Threading.Thread.Sleep(10);

                                        await ReadStandardResponseAsync(myProcess);

                                        // process response packet(s)
                                        // compare expected response packet to actual response packet
                                        PassedTest = ;
                                        ExitTest = ;
                                    }
                                    else if (firstStart)
                                    {
                                        throw new Exception("Failed to Start: " + myProcess.StartInfo.FileName + "\\" + myProcess.StartInfo.Arguments);
                                    }
                                    else
                                    {
                                        throw new Exception("Failed to Re-Start: " + myProcess.StartInfo.FileName);
                                    }

                                }
                                if (ExitTest) { goto case 1; }
                                else { break; }
                            case 1:
                                {
                                    if (myProcess.Start())
                                    {
                                        // write exit packet

                                        // close stream

                                        // read response
                                        await ReadStandardResponseAsync(myProcess);

                                        // process response packet

                                        // wait for process to exit
                                        myProcess.WaitForExit(100);

                                    }
                                    else
                                    {
                                        throw new Exception("Failed to Re-Start for Exit: " + myProcess.StartInfo.FileName);
                                    }
                                }
                                break;
                        }

                        if(testTimedOut)
                            throw new Exception("Test Timed Out!");

                    } while (!ExitTest);


                    if (!myProcess.HasExited)
                    {
                        myProcess.Kill();
                        throw new Exception("Test Process did not Exit!");
                    }

                }
                return PassedTest;
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                WD_Timer.Dispose();
                TestWorker.Dispose();
            }
        }

        private void TestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.
            testFunction(myProcess, out WD_State.PassedTest, out WD_State.ExitTest);
        }

        private void WD_TimerCallback(object timerState)
        {
            testTimedOut = true;           
        }

        async Task ReadStandardResponseAsync(Process myProcess)
        {
            bool noresponseout = true;
            bool noresponseerr = true;
            do
            {
                // read stderr async
                if (!myProcess.StandardError.EndOfStream)
                {
                    if (noresponseerr)
                    {
                        StdErrStrings.Add(await myProcess.StandardError.ReadToEndAsync());
                        noresponseerr = false;
                    }
                    else
                        StdErrStrings[StdErrStrings.Count - 1] += await myProcess.StandardError.ReadToEndAsync();


                }

                // read stdout async
                if (!myProcess.StandardOutput.EndOfStream)
                {
                    if (noresponseout)
                    {
                        StdOutStrings.Add(await myProcess.StandardOutput.ReadToEndAsync());
                        noresponseout = false;
                    }
                    else
                        StdOutStrings[StdOutStrings.Count - 1] += await myProcess.StandardOutput.ReadToEndAsync();
                }

                System.Threading.Thread.Sleep(10);

            } while ((noresponseout && noresponseerr) || (!myProcess.StandardError.EndOfStream || !myProcess.StandardOutput.EndOfStream));
        }
    }

    public delegate void TestCase(Process testProcess, out bool testPassed, out bool testExit);
  
    
}
