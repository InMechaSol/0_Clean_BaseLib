using System;
using Clean_BaseLib_Tests;

namespace Clean_BaseLib_API_TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            TestClass_ex0000 testCaseClass = new TestClass_ex0000();
            testCaseClass.Execute_CaseFunction_Exception();
        }
    }
}
