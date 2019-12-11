using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace Clean_BaseLib
{
    /// <summary>
    /// Abstract class defining core modular and clean concepts
    /// </summary>
    /// <remarks>
    /// The module execution system is responsible for executing module entry points according to data and execution policies.
    /// Data and execution policies provide the fundamental gaurantees for executional timing and data communication.
    /// # Requirements
    /// - Reusable, Common, Core IP
    /// - Cleanly Architected
    /// - Asynchrounous Event Driven Tasks Execution System
    /// - Cyclic Non-Blocking Main Loop
    /// - Robust Exception Handling
    /// - Data and Execution Gaurantees
    /// ## Clean Base Execution System
    /// Usage: 
    /// <code>
    /// BaseClass_Module_Execution_System myExeSys = new BaseClass_Module_Execution_System();
    /// myExeSys.Execute();
    /// </code>
    /// ### Configuration
    /// Core configuration is possible through constants of the module execution system.
    /// - Exception threshholds
    /// - Other...
    /// </remarks>
    public abstract class BaseClass_ModuleExecutionSystem
    {
        #region Configuration Constants of the Module Execution System
        /// <summary>
        /// Threshhold value of packet dequeue failures consider an exception
        /// </summary>
        public static int configured_failcount = 10;
        /// <summary>
        /// Time threshhold from start to next start of main loop
        /// </summary>
        public static int configured_ms_mainWD = 10;
        /// <summary>
        /// Time (ms) to sleep after each cyclic non-blocking loop of the execute function
        /// </summary>
        public static int configured_ms_exeLoopSleep = 10;
        #endregion

        #region Constructors of the Module Execution System
        /// <summary>
        /// Constructor from nothing
        /// </summary>
        public BaseClass_ModuleExecutionSystem()
        {
            ;
        }
        #endregion

        #region Properties of the Module Execution System
        /// <summary>
        /// Property exposing thread list
        /// </summary>
        public virtual List<Thread>                 Threads                         { set; get; }
        /// <summary>
        /// Property exposing module list
        /// </summary>
        public virtual List<BaseClass_Module>       CoreModules                     { set; get; }
        #endregion

        #region Data (Internal) of the Module Execution System
        List<Thread>                                corethreads =                   new List<Thread>();
        List<BaseClass_Module>                      coreModules =                   new List<BaseClass_Module>();
        Exception                                   e;
        bool                                        exit =                          false;
        int                                         state =                         0;
        ConcurrentQueue<Exception>                  applicationExceptionQueue =     new ConcurrentQueue<Exception>();
        ConcurrentQueue<BaseClass_Packet>           packetQueue  =                  new ConcurrentQueue<BaseClass_Packet>();
        DateTime                                    mainStartTime =                 DateTime.MinValue;
        TimeSpan                                    mainDuration;
        bool                                        stopExeSys =                    false;
        #endregion

        #region Main Entry Point of the Module Execution System (call this from the application)
        /// <summary>
        /// Main entry point of the execution system, of the application
        /// </summary>
        /// <remarks>
        /// This single function forms the entire application.
        /// Usage: 
        /// <code>
        /// // Create new Execution System Object from program main()
        /// BaseClass_Module_Execution_System myExeSys = new BaseClass_Module_Execution_System();
        /// // Call the Execute() function to immediately execute the full default application
        /// myExeSys.Execute();
        /// </code>
        /// This functions is called as the main application, it only returns when the application closes.
        /// Each iteration of the while loop is monitored and an exception is thrown if the data and execution
        /// policy has been violated
        /// </remarks>
        public void Execute()
        {            
            while (!exit)
            {
                try
                {                    
                    if (mainStartTime != DateTime.MinValue)
                    {
                        mainDuration = DateTime.Now - mainStartTime;
                        if ((mainDuration.TotalMilliseconds - configured_ms_exeLoopSleep) > configured_ms_mainWD)
                            throw new Exception("Execution System main loop duration exceeded configured threshold of: " + configured_ms_mainWD.ToString() + " (ms)");
                    }
                    mainStartTime = DateTime.Now;

                    switch (state)
                    {
                        case 0: HandleExceptions(); break;
                        case 1: Initialize(); break;
                        case 2: ManageThreads(); break;
                        case 3: RoutePackets(); break;
                        default: break;
                    }
                    Thread.Sleep(configured_ms_exeLoopSleep);
                }
                catch(Exception expct)
                {
                    e = expct;
                    state = 0;
                }
                
            }
        }
        #endregion

        #region Functions of the Module Execution System
        /// <summary>
        /// helper function to link module, add it to the list of exe modules
        /// </summary>
        /// <param name="module">Module to link</param>
        public void linkModuleObj(BaseClass_Module module)
        {
            if (CoreModules == null)
                CoreModules = new List<BaseClass_Module>();
            CoreModules.Add(module);
        }
        /// <summary>
        /// Execute Case Function to handle exceptions
        /// </summary>
        void HandleExceptions()
        {
            ;
        }
        /// <summary>
        /// Execute Case Function to handle (re)initialization the execution system
        /// </summary>
        void Initialize()
        {
            // determine type of initialization required (full, partial, custom, ... ??)
            

            // Full Initialization

            // Partial Initialization


        }
        /// <summary>
        /// Execute Case Function to shutdown and close the application
        /// </summary>
        void ApplicationClose()
        {
            exit = true;
            
            // Shutdown foreground threads

            // safe block until shutdown or timeout

            // exit now?
        }
        /// <summary>
        /// Static Function, Exception Handler
        /// </summary>
        /// <param name="e"></param>
        /// <param name="executionSystem"></param>
        public static void ApplicationExceptionHandler(Exception e, BaseClass_ModuleExecutionSystem executionSystem)
        {
            // Maybe just push exception to instance exceptoin queue???
            ;
        }
        /// <summary>
        /// Execute Case Function to manage child threads
        /// This function is cyclic and non-blocking.  Data and Execution gauarantees...
        /// </summary>
        /// <remarks>
        /// - Foreground threads require terminate logic, they must terminate before the application can close
        /// - Background threads do not require terminate logic, though cancalation is ideal
        /// - Barriers provide an interesting synchronization pattern
        /// - Exceptions handled at the thread level
        /// 
        /// # Each Module has at least two foreground threads
        /// - 1 Initiaiization
        /// - 2 Cyclic Non Blocking Loop
        ///
        /// Foreground threads have small execution space and route packets within Data and execution policy limits
        /// Foreground threads launch asynchrounous background threads to execute packet tasks</remarks>
        void ManageThreads()
        {
            if(stopExeSys)
            {
                bool allaborted = true;
                foreach (Thread thread in corethreads)
                {
                    if (thread.ThreadState != System.Threading.ThreadState.Aborted)
                    {
                        allaborted = false;
                        if (thread.ThreadState != System.Threading.ThreadState.AbortRequested)
                        {
                            thread.Abort();
                        }
                    }                    
                }
                if (allaborted)
                    ApplicationClose();
            }
            else
            {
                foreach (BaseClass_Module exeModule in coreModules)
                {
                    // Manage Initialization Thread (foreground)
                    if (!exeModule.IsInitialized && exeModule.ShouldExecute)
                    {
                        if (exeModule.InitThreadID >= corethreads.Count) 
                        {
                            exeModule.ShouldExecute = false;
                            throw new Exception("Execution System - Module Init Thread ID out of range (ID): " + exeModule.InitThreadID.ToString());
                        }
                        if (corethreads[exeModule.InitThreadID]==null)
                        {
                            exeModule.ShouldExecute = false;
                            throw new Exception("Execution System - Module Init Thread ID null (ID): " + exeModule.InitThreadID.ToString());
                        }
                        if (corethreads[exeModule.InitThreadID].ThreadState == System.Threading.ThreadState.Unstarted)
                        {
                            // Start the thread
                            corethreads[exeModule.InitThreadID].Start();
                        }
                        else if (corethreads[exeModule.InitThreadID].ThreadState != System.Threading.ThreadState.Running)
                        {
                            exeModule.ShouldExecute = false;
                            throw new Exception("Execution System - Module Init Thread not running: " + exeModule.ToString());
                        }
                    }
                    // Manage Main Loop Thread (foreground)
                    else if (exeModule.ShouldExecute)
                    {

                    }

                    
                }
            }            
        }
        /// <summary>
        /// Execute Case Function to route packets.
        /// This function is cyclic and non-blocking.  Data and Execution gauarantees...
        /// </summary>
        void RoutePackets()
        {
            // Chack Status of Packet Que
            if (!packetQueue.IsEmpty)
            {
                int failcount = 0;
                do  // Pop Packet(s) and Push them to Module Queue(s)
                {
                    BaseClass_Packet deQuePacket;
                    if(packetQueue.TryDequeue(out deQuePacket))
                    {
                        failcount = 0;
                        // Now Push deQuePacket to correct module queue
                    }
                    else
                    {
                        failcount++;
                        if (failcount > configured_failcount)
                            throw new Exception("Execution System failed to deQueue Packets, exceeded configured limit of consecutive deQueue failures.");
                    }
                } while (!packetQueue.IsEmpty);
            }           
        }
        #endregion
    }
}
