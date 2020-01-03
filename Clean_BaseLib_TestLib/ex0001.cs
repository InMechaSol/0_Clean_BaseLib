using System;
using Clean_BaseLib;

namespace Clean_BaseLib_TestLib
{
    public class ex0001_ExecutionSystem:BaseClass_ModuleExecutionSystem
    {

    }
    public class ex0001_Module1:BaseClass_Module
    {
        public ex0001_Module1(BaseClass_ModuleExecutionSystem exeSysIn) : base(exeSysIn) { }
    }
}
