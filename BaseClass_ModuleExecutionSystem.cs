using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


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
        #region Configuration Settings of the Module Execution System
        public BaseClass_Sys_ExecutionPolicy ExecutionPolicy { protected set; get; } = new BaseClass_Sys_ExecutionPolicy();
        #endregion

        #region Constructors of the Module Execution System
        static BaseClass_ModuleExecutionSystem()
        {
            
        }
        /// <summary>
        /// Constructor from nothing
        /// </summary>
        public BaseClass_ModuleExecutionSystem()
        {
            exesys_module = new BaseClass_ExeSys_Module(this);
        }
        #endregion

        #region Properties of the Module Execution System
        /// <summary>
        /// Property exposing thread list
        /// </summary>
        public virtual List<Thread> Threads { set; get; }
        /// <summary>
        /// Property exposing module list
        /// </summary>
        public virtual List<BaseClass_Module> CoreModules { set; get; }
        public void PushModuleException(Exception exception)
        {
            applicationExceptionQueue.Enqueue(exception);
        }
        #endregion

        #region Data (Internal) of the Module Execution System
        List<Thread>                        corethreads =                   new List<Thread>();
        List<BaseClass_Module>              coreModules =                   new List<BaseClass_Module>();
        bool                                exit =                          false;
        int                                 start_state =                   0;
        int                                 count_excp_loop =               0;
        ConcurrentQueue<Exception>          applicationExceptionQueue =     new ConcurrentQueue<Exception>();
        List<ExeSysException>               exception_list =                new List<ExeSysException>();
        ConcurrentQueue<BaseClass_Packet>   packetQueue =                   new ConcurrentQueue<BaseClass_Packet>();
        DateTime                            executeStartTime =              DateTime.MinValue;
        TimeSpan                            executeDuration;
        bool                                stopExeSys =                    false;
        int                                 threadindex, moduleindex;
        BaseClass_ExeSys_Module             exesys_module;
        List<int>                           module_blocked_cycles_list =    new List<int>();
        bool                                initialized =                   false;
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
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (!exit)
            {
                try
                {
                    switch (start_state)
                    {
                        case 0: HandleExceptions(); goto case 1;
                        case 1: Initialize(); goto case 2;
                        case 2:
                            {
                                executeStartTime = DateTime.Now;
                                ManageThreads();                                 
                            }
                            goto case 3;
                        case 3:
                            { 
                                RoutePackets();                                
                                executeDuration = DateTime.Now - executeStartTime;
                                if (executeDuration.TotalMilliseconds > ExecutionPolicy.MS_ExeDuration)
                                    throw NewExecuteDurationException();                                
                            }
                            break;
                        default: break;
                    }
                    count_excp_loop = 0;
                    Thread.Sleep(ExecutionPolicy.MS_ExeLoopSleep);
                }
                catch (Exception expct)
                {
                    if (typeof(ModuleException).IsInstanceOfType(expct))
                        applicationExceptionQueue.Enqueue(expct);
                    
                    else if (typeof(ExeSysException).IsInstanceOfType(expct))
                        applicationExceptionQueue.Enqueue(expct);
                    
                    else
                        applicationExceptionQueue.Enqueue(NewExeSysException(expct));
                                       
                    start_state = 0;
                    count_excp_loop++;
                }
                finally
                {
                    // start next loop with 
                    // Manage Threads and Route Packets if initialized
                    if (initialized)
                        start_state = 2;

                    // start next loop with 
                    // Handle Exceptions if the Queue is not empty
                    if (applicationExceptionQueue.Count > 0)
                        start_state = 0;

                    // start next loop with 
                    // Close the application if too many consecutive exceptions
                    if (count_excp_loop > ExecutionPolicy.Threshhold_ConsecExcps)
                    {
                        applicationExceptionQueue.Enqueue(NewConsecutiveExcpsException());
                        start_state = 2;
                        stopExeSys = true; // this will initiate an application exit sequence
                    }
                }
            }
        }
        #endregion

        #region Exception Builders of the Module Execution System
        
        public ExeSysException NewConsecutiveExcpsException()
        {
            String msg = "Execution System - Execute() consecutive exception threshhold exceeded (threshhold): ";
            msg += ExecutionPolicy.Threshhold_ConsecExcps.ToString()+ " consecutive exceptions without successful code execution.";

            ExeSysException outException = new ExeSysException(this, msg);
            return outException;
        }
        public ExeSysException NewExecuteDurationException()
        {
            String msg = "Execution System - Execute() duration exceeded configured threshold of: ";
            msg += ExecutionPolicy.MS_ExeDuration.ToString() + " (ms).";

            ExeSysException outException = new ExeSysException(this, msg);
            return outException;
        }
        public ExeSysException NewExeSysException(Exception innerExcp)
        {
            String msg = "Execution System - Execute() caught an exception - see inner ";

            ExeSysException outException = new ExeSysException(this, msg, innerExcp);
            return outException;
        }
        public ExeSysException NewUnCaughtExeSysException(Exception innerExcp)
        {
            String msg = "Execution System - failed to catch an exception - see inner ";

            ExeSysException outException = new ExeSysException(this, msg, innerExcp);
            return outException;
        }
        public static ExeSysException NewApplicationException(Exception innerExcp)
        {
            String msg = "Application - caught an exception - see inner ";

            ExeSysException outException = new ExeSysException(null, msg, innerExcp);
            return outException;
        }
        #endregion

        #region Functions of the Module Execution System
        /// <summary>
        /// helper function to link module, add it to the list of exe modules
        /// </summary>
        /// <param name="module">Module to link</param>
        public void linkModuleObj(BaseClass_Module module)
        {    
            if (!coreModules.Contains(module))
            {
                coreModules.Add(module);
                module_blocked_cycles_list.Add(0);

                corethreads.Add(new Thread(new ThreadStart(module.MainInit)));
                corethreads[corethreads.Count - 1].Name = "init_" + coreModules.Count.ToString();
                corethreads[corethreads.Count - 1].Priority = ThreadPriority.AboveNormal;
                corethreads[corethreads.Count - 1].IsBackground = false;
                corethreads.Add(new Thread(new ThreadStart(module.MainLoop)));
                corethreads[corethreads.Count - 1].Name = "loop_" + coreModules.Count.ToString();
                corethreads[corethreads.Count - 1].Priority = ThreadPriority.Normal;
                corethreads[corethreads.Count - 1].IsBackground = false;

            }
            else
                throw new Exception("Execution System - Attempted duplicate module link: " + module.ToString());
        }
        /// <summary>
        /// Execute() Case Function to handle exceptions
        /// </summary>
        void HandleExceptions()
        {
            
            // at this point, all exceptions are in the list
            for(int i = exception_list.Count; i > 0; i--)
            {     
                // could be exesysexception, or moduleexception...
                if (typeof(ModuleException).IsInstanceOfType(exception_list[i]))
                {
                    ModuleException modException = (ModuleException)exception_list[i];
                    // let the module handle it...

                    
                }
                else if (typeof(ExeSysException).IsInstanceOfType(exception_list[i]))
                {
                    ExeSysException exeException = (ExeSysException)exception_list[i];
                    // basically need to decide to terminate or keep going and if to reset the initialized flag

                    
                }

                // log exceptions if logging enabled (through what file system module???)
                LogExceptions();

                // advertise it
                AdvertiseExceptions();
            }
        }
        /// <summary>
        /// Execute Case Function to handle (re)initialization the execution system
        /// </summary>
        void Initialize()
        {
            // determine type of initialization required (full, partial, custom, ... ??)

            // Full Initialization

            // Partial Initialization

            initialized = true;
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

            if(exception_list.Count>0)
            {
                ConsoleDumpExceptions();
            }
        }
        /// <summary>
        /// Push exception packet of correct serialized format out std err
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// According to the application program try-catch-finally pattern, this
        /// should only be executing if the application was unable to construct the 
        /// execution system object.  Therefore, there is no instance on which to execute
        /// module functions like user interface and exception handlers.  
        /// 
        /// We just have to give up...so please always program reliable well tested constructors for the
        /// execution system!
        /// </remarks>
        public static void ApplicationExceptionHandler(Exception e)
        {
            Console.Error.Write((new ExceptionPacket(NewApplicationException(e))).toJSON_array()); 
        }
        /// <summary>
        /// Fundamental Execute Case Function, Cyclic and Non-Blocking, Enforces Execution Policy
        /// </summary>
        /// <remarks>
        /// # Manage Module Threads
        /// Each module within the execution system is allocated two foreground threads, one for initialization (main init)
        /// and another for cyclic non-blocking loop (main loop).   An instance global private boolean variable, stopeExeSys, can
        /// be used to abort all threads and shutdown the application.  If its value is true, abort will occur followed by 
        /// application close.  Otherwise, this function will manage the two foreground module threads.
        /// 
        /// Modules can still utilize the process thread pool to perform async operations.  Modules can create their own threads
        /// and manage them locally.  Modules can start external processes and threads to manage the external processes.
        /// the two allocated foreground threads are the only threads controlled by the execution system.
        /// 
        /// # Main Initialization Thread
        /// This is a one-shot on-demand thread able to be restarted when (re)initialization becomes necessary.  Module's initialization
        /// entry points are executed here.  
        /// 
        /// # Main Loop Thread
        /// This is a cyclic non-blocking thread that is started and only aborted at application close.  This thread is responsible for
        /// execution of Modules' cyclic loop entry point functions.  It can be paused and restarted by two means:
        /// 1 setting the module.should_execute private data member false, from whithin module code
        /// 2 sending the pause packet which places the thread in waitsleepjoin state, enabling thread interrupt by this function
        /// 
        /// # Background Information on .NET core threads
        /// - Foreground threads require terminate logic, they must terminate before the application can close
        /// - Background threads do not require terminate logic, though cancalation is ideal
        /// - Barriers provide an interesting synchronization pattern
        /// - Exceptions handled at the thread level are aggregated and passed to calling thread
        /// - Foreground threads have small execution space and route packets within Data and execution policy limits
        /// - Foreground threads launch asynchrounous background threads to execute packet tasks
        /// </remarks>
        void ManageThreads()
        {
            if (stopExeSys) // Abort all threads of all modules if true
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
            else // Manage Init and Loop Threads of all modules otherwise
            {
                threadindex = 0;
                moduleindex = 0;
                foreach (Thread thread in corethreads)
                {
                    if (thread.ThreadState == System.Threading.ThreadState.Unstarted)
                    {
                        thread.Start();
                    }
                    else if (thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    {
                        // if its even, its inititialization thread
                        if ((threadindex % 2) == 0)
                        {
                            if (!coreModules[moduleindex].IsInitialized && coreModules[moduleindex].ShouldExecute)
                            {
                                thread.Interrupt();  // Run the Initialization Thread, single shot
                            }
                        }
                        else
                        {
                            // Watch Dog the main loop waitsleepjoin time, interrupt if necessary
                        }
                    }
                    else if (thread.ThreadState != System.Threading.ThreadState.Running)
                    {
                        if ((threadindex % 2) == 0)
                        {
                            if (coreModules[moduleindex].ShouldExecute)
                                throw new Exception("Execution System - Module Main Init not Running (ID): " + moduleindex.ToString() + " (Init Thread State): " + thread.ThreadState);
                        }
                        else
                        {
                            if (coreModules[moduleindex].ShouldExecute)
                                throw new Exception("Execution System - Module Main Loop not Running (ID): " + moduleindex.ToString() + " (Loop Thread State): " + thread.ThreadState);
                        }
                    }
                    else
                    {
                        // watch dog running threads to ensure they've not blocked
                        if ((threadindex % 2) == 0)
                        {
                            // init threads run until waitsleepjoin state
                        }
                        else
                        {
                            // loop threads run until momentary waitsleepjoin state then run again

                            // check to see if loop block has been reset
                            if (coreModules[moduleindex].LoopBlocked)
                                module_blocked_cycles_list[moduleindex]++;
                            else
                                module_blocked_cycles_list[moduleindex] = 0;
                                                       
                            // set it again
                            coreModules[moduleindex].LoopBlocked = true;

                            // this algormithm assumes the execution time of this thread insignificant compared
                            // to the wait time of this thread.  considering the impact of garbage collection and other 
                            // OS level operations, this seems an exceptable approximation.
                            if (module_blocked_cycles_list[moduleindex] > ExecutionPolicy.Loops_MainWDT)
                            {
                                throw new Exception("Execution System - Module Main Loop WDT (ID): " + moduleindex.ToString() + " (Loop Thread State): " + thread.ThreadState);
                            }
                        }

                    }
                    threadindex++;
                    if ((threadindex % 2) == 0)
                        moduleindex++;
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
                    if (packetQueue.TryDequeue(out deQuePacket))
                    {
                        failcount = 0;
                        // Now Push deQuePacket to correct module queue
                        coreModules[deQuePacket.ModuleID].PushModulePacket(deQuePacket);
                    }
                    else
                    {
                        failcount++;
                        if (failcount > ExecutionPolicy.DeQueueFailCount)
                            throw new Exception();
                    }
                } while (!packetQueue.IsEmpty);
            }

            // Now Route Exceptions
            // First DeQueue any new exceptions, transfer to local list as ExeSysException
            bool stopDeQueing = false;
            do
            {
                Exception quedException0;

                if (applicationExceptionQueue.TryDequeue(out quedException0))
                {
                    if (typeof(ModuleException).IsInstanceOfType(quedException0))
                    {
                        exception_list.Add((ModuleException)quedException0);
                    }
                    else if (typeof(ExeSysException).IsInstanceOfType(quedException0))
                    {
                        exception_list.Add((ExeSysException)quedException0);
                    }
                    else
                    {
                        exception_list.Add(NewUnCaughtExeSysException(quedException0));
                    }
                }
                else
                    stopDeQueing = true;


            } while (!stopDeQueing);

        }
        #endregion

        void LogExceptions()
        {
            if(ExecutionPolicy.LogExceptions)
            {
                // if a persistance module exists, log exceptions
            }
        }
        void AdvertiseExceptions()
        {
            if(ExecutionPolicy.AdvertiseExceptions)
            {
                // if a user module exists, advertise over it

                // otherwise, advetise to console.error
                ConsoleDumpExceptions();
            }
        }
        void ConsoleDumpExceptions()
        {
            foreach(ExeSysException ep in exception_list)
                Console.Error.Write((new ExceptionPacket(ep)).toJSON_array());
        }
        public void ResetExePolicy_Default()
        {
            ExecutionPolicy = null;
            ExecutionPolicy = new BaseClass_Sys_ExecutionPolicy();
        }
    }


    public class BaseClass_ExeSys_Module : BaseClass_Module
    {
        public BaseClass_ExeSys_Module(BaseClass_ModuleExecutionSystem exSystem) : base(exSystem)
        {

        }
    }
}
