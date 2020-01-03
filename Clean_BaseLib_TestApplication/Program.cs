using System;
using Clean_BaseLib_TestLib;

namespace Clean_BaseLib_TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            int exampleCase = 0;          
            try { exampleCase = Convert.ToInt32(args[0]); }
            catch { exampleCase = 0; }
            finally
            {
                Console.Title = ExeSysTestProcess.titleString + exampleCase.ToString("D4");
                Console.WriteLine(Console.Title);
                switch (exampleCase)
                {
                    case 0:
                        {
                            try
                            {
                                ex0000_ExecutionSystem exeSys = new ex0000_ExecutionSystem();
                                ex0000_Module1 module1 = new ex0000_Module1(exeSys);
                                exeSys.Execute();
                            }
                            catch(Exception e)
                            {
                                ex0000_ExecutionSystem.ApplicationExceptionHandler(e);
                            }                            
                        }
                        break;
                    case 1:
                        {
                            try
                            {
                                ex0001_ExecutionSystem exeSys = new ex0001_ExecutionSystem();
                                ex0001_Module1 module1 = new ex0001_Module1(exeSys);
                                exeSys.Execute();
                            }
                            catch (Exception e)
                            {
                                ex0001_ExecutionSystem.ApplicationExceptionHandler(e);
                            }
                        }
                        break;
                }                
            }
        }
    }
}
