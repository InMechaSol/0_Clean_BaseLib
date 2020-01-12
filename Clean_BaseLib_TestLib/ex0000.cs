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
        public static List<BaseClass_CommandResponse> testCaseFunctionException;
        
    }
    public class ex0000_Module1 : BaseClass_Module
    {
        public ex0000_Module1(BaseClass_ModuleExecutionSystem exeSysIn) : base(exeSysIn) { }
    }

    
    
}
