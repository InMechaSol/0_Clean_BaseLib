using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

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
            UserCmdResps = new ConcurrentQueue<BaseClass_CommandResponse>();

            linkModuleExeSys(modExeSys);
        }
        #endregion

        #region Module Entry Point Functions
        /// <summary>
        /// Entry Point Function - Main Initialiazation routine called in the initialization loop of the execution system.
        /// </summary>
        public virtual void mainInit()
        {
            ;
        }
        /// <summary>
        /// Entry Point Function - Main Cyclic routine called in the main loop of the execution system.
        /// </summary>
        public virtual void mainLoop()
        {
            ;
        }
        #endregion

        #region Module Helper Functions
        /// <summary>
        /// Function to link input execution system to this module object
        /// </summary>
        /// <param name="modExeSys"></param>
        protected virtual void linkModuleExeSys(BaseClass_ModuleExecutionSystem modExeSys)
        {
            moduleExecutionSystem = modExeSys;
            modExeSys.linkModuleObj(this);
        }
        #endregion

        #region Module Data Members

        ConcurrentQueue<BaseClass_CommandResponse> UserCmdResps;
        private BaseClass_ModuleExecutionSystem moduleExecutionSystem;


        #endregion

    }
}
