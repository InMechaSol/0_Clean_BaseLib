using System;
using System.Collections.Generic;
using System.Text;

namespace Clean_BaseLib
{
    public class BaseClass_DataExecutionPolicy
    {
    }

    public class BaseClass_ExecutionPolicy
    {

    }

    public class BaseClass_Sys_ExecutionPolicy
    {
        /// <summary>
        /// Threshhold value of packet dequeue failures consider an exception
        /// </summary>
        public static readonly int default_failcount = 10;
        public int DeQueueFailCount { protected set; get; } = default_failcount;
        /// <summary>
        /// Execution duration threshhold of combined MangageThreads() and RoutePackets() functions
        /// </summary>
        public static readonly int default_ms_exeDuration = 10;
        public int MS_ExeDuration { protected set; get; } = default_ms_exeDuration;
        /// <summary>
        /// Time (ms) to sleep after each cyclic non-blocking loop of the execute() function
        /// </summary>
        public static readonly int default_ms_exeLoopSleep = 100;
        public int MS_ExeLoopSleep { protected set; get; } = default_ms_exeLoopSleep;
        public int Loops_MainWDT { get { if (MS_ExeLoopSleep > 0) return (MS_ExeDuration / MS_ExeLoopSleep); else return (0); } }
        /// <summary>
        /// Consecutive exceptions threshhold
        /// </summary>
        public static readonly int default_consec_except_thresh = 10;
        public int Threshhold_ConsecExcps { protected set; get; } = default_consec_except_thresh;

        public bool LogExceptions { get; protected set; } = false;
        public bool AdvertiseExceptions { get; protected set; } = false;
        
        static BaseClass_Sys_ExecutionPolicy()
        {
            
        }
        
    }

    public class Mod_ExecutionPolicy
    {
        /// <summary>
        /// Default Module Sleep Time (ms)
        /// <seealso cref="MS_Sleep"/>
        /// </summary>
        public static readonly int default_ms_sleep = 100;
        /// <summary>
        /// Configured Module Sleep Time (ms)
        /// </summary>
        public int MS_Sleep { set; get; } = default_ms_sleep;
        /// <summary>
        /// Default Consecutive Module Exception Threshhold
        /// <seealso cref="Threshhold_ConsecExcps"/>
        /// </summary>
        public static readonly int default_threshhold_consec_excps = 10;
        /// <summary>
        /// Configured Consecutive Module Exception Threshhold
        /// </summary>
        public int Threshhold_ConsecExcps { set; get; } = default_threshhold_consec_excps;

        
    }


    #region Exceptions
    public class ExeSysException : Exception
    {
        public BaseClass_ModuleExecutionSystem  ExeSysLink      { get; protected set; } = null;
        public bool                             Acknowledged    { get; protected set; } = false;
        public bool                             Logged          { get; protected set; } = false;
        public bool                             Advertised      { get; protected set; } = false;
        public ExeSysException() : base() { }
        public ExeSysException(String msgIn) : base(msgIn) { }
        public ExeSysException(BaseClass_ModuleExecutionSystem exeSysIn, String msgIn) : base(msgIn) { ExeSysLink = exeSysIn; }
        public ExeSysException(String msgIn, Exception excpIn) : base(msgIn, excpIn) { }
        public ExeSysException(BaseClass_ModuleExecutionSystem exeSysIn, String msgIn, Exception excpIn) : base(msgIn, excpIn) { }
    }
    public class ModuleException : ExeSysException
    {
        public BaseClass_Module                 ModuleLink      { get; protected set; } = null;


        public ModuleException() : base() { }
        public ModuleException(String msgIn) : base(msgIn) { }
        public ModuleException(String msgIn, Exception excpIn) : base(msgIn, excpIn) { }
        public ModuleException(BaseClass_Module modIn, String msgIn) : base(msgIn) { ModuleLink = modIn; ExeSysLink = modIn.ExeSysLink; }
        public ModuleException(BaseClass_Module modIn, String msgIn, Exception excpIn) : base(msgIn, excpIn) { ModuleLink = modIn; ExeSysLink = modIn.ExeSysLink; }
    }


    #endregion
    
}
