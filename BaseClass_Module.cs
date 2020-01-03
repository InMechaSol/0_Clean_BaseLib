using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Clean_BaseLib
{
    /// <summary>
    /// BaseClass_Module abstract class is the fundamental executional element of the modular clean system.  
    /// </summary>
    /// <remarks>
    /// Modules know of, and therefore depend on, the module execuiton system on which they execute.  Modules
    /// inheriting from this class have entry point functions called by the execution system according to its 
    /// rules.
    /// </remarks>
    public abstract class BaseClass_Module
    {
        #region Configuration Policies of the Base Module Class
        Mod_ExecutionPolicy ExecutionPolicy { set; get; } = new Mod_ExecutionPolicy();
        #endregion

        #region Module Constructors
        /// <summary>
        /// Constructor from nothing
        /// </summary>
        public BaseClass_Module()
        {

        }
        /// <summary>
        /// Constructor from nothing, linking execution system
        /// </summary>
        /// <param name="modExeSys">Execution System to which this module belongs and from which it executes</param>
        public BaseClass_Module(BaseClass_ModuleExecutionSystem modExeSys)
        {
            modExeSys.linkModuleObj(this);
            ExeSysLink = modExeSys;
        }
        #endregion

        #region Module Properties
        public bool LoopBlocked { set { loop_blocked = true; } get { return loop_blocked; } }
        
        /// <summary>
        /// Indication of initialization status
        /// </summary>
        public bool IsInitialized { get { return initialized; } }
        /// <summary>
        /// Set and Get if the module threads should execute
        /// </summary>
        public bool ShouldExecute { get { return should_execute; } }
        public void PushModulePacket(BaseClass_Packet value) 
        {
            module_rx_queue.Enqueue(value);
        }
        public BaseClass_ModuleExecutionSystem ExeSysLink { get; protected set; }
        #endregion

        #region Module Entry Point Functions
        /// <summary>
        /// Entry Point Function - Main Initialiazation routine called in the initialization loop of the execution system.
        /// </summary>
        protected virtual void main_init()
        {
            ;
        }
        protected virtual void handle_exceptions()
        {
            ;
        }
        public void MainInit()
        {
            while (!exit_init)
            {
                try
                {
                    if (!initialized && should_execute)
                    {
                        handle_exceptions();
                        main_init();                        
                    }
                    count_excp_init = 0;
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                    ;
                }
                catch (ThreadAbortException)
                {
                    exit_init = true;
                }
                catch (Exception e)
                {
                    count_excp_init++;
                    module_exceptions.Enqueue(e);
                    ExeSysLink.PushModuleException(e);
                    if (count_excp_init > ExecutionPolicy.Threshhold_ConsecExcps)
                    {
                        exit_init = true;
                        Exception ept = new Exception("Base Module - MainInit exiting from consecutive exceptions, threshhold: " + ExecutionPolicy.Threshhold_ConsecExcps.ToString());
                        module_exceptions.Enqueue(ept);
                        ExeSysLink.PushModuleException(ept);

                    }
                }
            }
        }
        /// <summary>
        /// Entry Point Function - Main Cyclic routine called in the main loop of the execution system.
        /// </summary>
        protected virtual void main_loop()
        {
            ;
        }
        void RoutePackets()
        {
            ;// incoming packets trigger sync or async processing and response
        }
        public void MainLoop()
        {
            while (!exit_loop)
            {
                try
                {
                    if (initialized && should_execute)
                    {
                        RoutePackets();
                        main_loop();
                        loop_blocked = false;
                    }
                    count_excp_loop = 0;
                    Thread.Sleep(ExecutionPolicy.MS_Sleep);
                }
                catch (ThreadInterruptedException)
                {
                    ;
                }
                catch (ThreadAbortException)
                {
                    exit_loop = true;
                }
                catch (Exception e)
                {
                    count_excp_loop++;
                    initialized = false;
                    module_exceptions.Enqueue(e);
                    ExeSysLink.PushModuleException(e);
                    if (count_excp_loop > ExecutionPolicy.Threshhold_ConsecExcps)
                    {
                        exit_loop = true;
                        Exception ept = new Exception("Base Module - MainLoop exiting from consecutive exceptions, threshhold: " + ExecutionPolicy.Threshhold_ConsecExcps.ToString());
                        module_exceptions.Enqueue(ept);
                        ExeSysLink.PushModuleException(ept);

                    }
                }
            }
        }
        #endregion

        #region Module Helper Functions

        #endregion

        #region Module Data Members
        protected ConcurrentQueue<Exception>                    module_exceptions =         new ConcurrentQueue<Exception>();
        protected ConcurrentQueue<BaseClass_Packet>             module_rx_queue =           new ConcurrentQueue<BaseClass_Packet>();
        protected ConcurrentQueue<BaseClass_Packet>             module_tx_queue =           new ConcurrentQueue<BaseClass_Packet>();
        protected ConcurrentQueue<BaseClass_CommandResponse>    command_response_queue =    new ConcurrentQueue<BaseClass_CommandResponse>();
        protected bool                                          exit_init =                 false;
        protected bool                                          exit_loop =                 false;
        protected bool                                          initialized =               false;
        protected bool                                          should_execute =            true;
        protected int                                           count_excp_init =           0;
        protected int                                           count_excp_loop =           0;
        protected bool                                          loop_blocked =              false;
        #endregion

    }

    
}
