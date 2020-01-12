using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Clean_BaseLib;

namespace Clean_BaseLib_TestLib
{
    public class ExeSysTestProcess
    {
        public string argsIn { get; set; }
        public List<BaseClass_CommandResponse> TestConversation { set; get; }
        public SerializationType DefaultSerializationType { set; get; }
        public int msTimeOut { get; set; }

        public static readonly String titleString = "InMechaSol's Clean Base Lib - Example ";
        public static readonly string TestAppExePathString = "Clean_BaseLib_TestApplication.exe";

        protected bool firstStart = true;
        protected bool ExitTest = false;
        protected bool PassedTest = false;
        protected bool testTimedOut = false;

        protected System.Threading.Timer WD_Timer;

        protected List<List<byte>> StdOutArrays;
        protected List<byte> StandardOutputBytes;
        protected List<List<byte>> StdErrArrays;
        protected List<byte> StandardErrorBytes;
        protected List<List<byte>> StdInArrays;
        protected List<byte> StandardInputBytes;

        protected Stream StdInputStream;     
        protected Stream StdOutputStream;    
        protected Stream StdErrorStream;     

        public async Task<bool> TryTest()
        {
            int stateVar = 0;
            int testStep = 0;
            firstStart = true;
            ExitTest = false;
            PassedTest = false;
            testTimedOut = false;
            try
            {
                WD_Timer = new System.Threading.Timer(WD_TimerCallback, testTimedOut, msTimeOut, 10);
                StdOutArrays = new List<List<byte>>();
                StdErrArrays = new List<List<byte>>();
                StdInArrays = new List<List<byte>>();
                using (Process myProcess = new Process())
                {                   

                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.CreateNoWindow = true;

                    myProcess.StartInfo.StandardInputEncoding = Encoding.UTF8;
                    myProcess.StartInfo.RedirectStandardInput = true;
                    myProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    myProcess.StartInfo.RedirectStandardOutput = true;
                    myProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                    myProcess.StartInfo.RedirectStandardError = true;
                    
                    myProcess.StartInfo.FileName = TestAppExePathString;
                    myProcess.StartInfo.Arguments = argsIn;

                    
                    do
                    {
                        switch (stateVar)
                        {
                            case 0:
                                {
                                    if (myProcess.Start())
                                    {
                                        StdInputStream = myProcess.StandardInput.BaseStream;
                                        StdOutputStream = myProcess.StandardOutput.BaseStream;
                                        StdErrorStream = myProcess.StandardError.BaseStream;

                                        if (firstStart)
                                        {
                                            // close stream
                                            StdInputStream.Close();
                                            System.Threading.Thread.Sleep(BaseClass_Sys_ExecutionPolicy.default_msSleep_APIProc_PostInputDelay);

                                            await ReadStandardResponseAsync();

                                            // ensure starts with title string and ends with argIn string
                                            if (StdOutArrays.Count > 0)
                                            {
                                                if (!(Encoding.UTF8.GetString(StdOutArrays[0].ToArray())).StartsWith(titleString))
                                                    throw new Exception("Fail - Preamble received doesn't match title string.");
                                                if (!(Encoding.UTF8.GetString(StdOutArrays[0].ToArray())).EndsWith(argsIn))
                                                    throw new Exception("Fail - Preamble received doesn't match argument string.");

                                            }
                                            else if(StdErrArrays.Count>0)
                                                throw new Exception("Std Error: "+ Encoding.UTF8.GetString(StdErrArrays[0].ToArray()));

                                            firstStart = false;
                                            break;
                                        }

                                        // write test packet
                                        TestConversation[testStep].CommandPacket.ToByteStreamAndLog(DefaultSerializationType,StdInputStream,StdInArrays);

                                        // close stream
                                        StdInputStream.Close();
                                        System.Threading.Thread.Sleep(BaseClass_Sys_ExecutionPolicy.default_msSleep_APIProc_PostInputDelay);

                                        // read packets from standard streams
                                        // process response packet(s)
                                        // compare expected response packet to actual response packet
                                        foreach(BaseClass_Packet bc in await ReadStandardPacketsAsync(DefaultSerializationType))
                                        {
                                            if (testStep == 0)
                                                PassedTest = bc.Equals(TestConversation[testStep].ResponsePacket);
                                            else
                                                PassedTest &= bc.Equals(TestConversation[testStep].ResponsePacket);
                                        }
                                        if (++testStep >= TestConversation.Count)
                                            ExitTest = true;
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
                                        ExitPacket exPack = new ExitPacket
                                        {
                                            ShouldExitWillExit = true
                                        };
                                        exPack.ToByteStreamAndLog(DefaultSerializationType,StdInputStream,StdInArrays);

                                        // close stream
                                        StdInputStream.Close();
                                        System.Threading.Thread.Sleep(BaseClass_Sys_ExecutionPolicy.default_msSleep_APIProc_PostInputDelay);

                                        // read packets from standard streams
                                        // process response packet(s)
                                        // compare expected response packet to actual response packet
                                        bool gotexRsp = false;
                                        foreach (BaseClass_Packet bc in await ReadStandardPacketsAsync(DefaultSerializationType))
                                        {
                                            if (exPack.MatchesPacket(bc)) gotexRsp = true;
                                        }
                                        if(!gotexRsp)
                                            throw new Exception("Failed to Read Exit Response.");

                                        // wait for process to exit
                                        myProcess.WaitForExit(BaseClass_Sys_ExecutionPolicy.default_msSleep_APIProc_ExitDelay);

                                    }
                                    else
                                    {
                                        throw new Exception("Failed to Re-Start for Exit: " + myProcess.StartInfo.FileName);
                                    }
                                }
                                break;
                        }

                        if (testTimedOut)
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
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                WD_Timer.Dispose();
            }
        }

        private void WD_TimerCallback(object timerState)
        {
            testTimedOut = true;
        }

        async Task ReadStandardResponseAsync()
        {
            // read stderr async
            StandardErrorBytes = await BaseClass_Packet.ReadToEndAsync(StdErrorStream, BaseClass_Sys_ExecutionPolicy.default_msSleep_readLoops);
            if (StandardErrorBytes != null)
                if (StandardErrorBytes.Count>0)
                    StdErrArrays.Add(StandardErrorBytes);

            // read stdout async
            StandardOutputBytes = await BaseClass_Packet.ReadToEndAsync(StdOutputStream, BaseClass_Sys_ExecutionPolicy.default_msSleep_readLoops);
            if (StandardOutputBytes != null)
                if(StandardOutputBytes.Count>0)
                StdOutArrays.Add(StandardOutputBytes);
        }
        async Task<List<BaseClass_Packet>> ReadStandardPacketsAsync(SerializationType DeSerializationTypeSelect)
        {
            List<BaseClass_Packet> retList = null;
            List<BaseClass_Packet> outList = null;
            List<BaseClass_Packet> errList = null;

            // read stderr async
            StandardErrorBytes = await BaseClass_Packet.ReadToEndAsync(StdErrorStream, BaseClass_Sys_ExecutionPolicy.default_msSleep_readLoops);
            if (StandardErrorBytes != null)
            {
                StdErrArrays.Add(StandardErrorBytes);
                errList = BaseClass_Packet.ParsePackets(DeSerializationTypeSelect, StandardErrorBytes);
                if (errList != null)
                    retList = errList;
            }

            // read stdout async
            StandardOutputBytes = await BaseClass_Packet.ReadToEndAsync(StdOutputStream, BaseClass_Sys_ExecutionPolicy.default_msSleep_readLoops);
            if (StandardOutputBytes != null)
            {
                StdOutArrays.Add(StandardOutputBytes);
                outList = BaseClass_Packet.ParsePackets(DeSerializationTypeSelect, StandardOutputBytes);
                if (outList != null && retList != null)
                    retList.AddRange(outList);
                else if (outList != null)
                    retList = outList;
                     
            }
            return retList;
        }
    }

}
