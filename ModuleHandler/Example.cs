using System;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace DevQuarterLibraries
{
    public class Program
    {
        internal static void Main(string[] args)
        {
            Module[] modules = ModuleLoader.LoadModule(ModuleLoader.SelectModules.New, true);
            Console.WriteLine("\nPress any key to REALOAD and START again modules...\n");
            Console.ReadKey(true);
            ModuleLoader.ReloadModule(ref modules, true);
            Console.WriteLine("\nPress any key to STOP and REMOVE modules...\n");
            Console.ReadKey(true);
            ModuleLoader.RemoveModule(ref modules);
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }
    }
}
