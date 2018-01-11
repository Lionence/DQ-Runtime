using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DevQuarterLibraries
{
    public class ExtensionModule : ModuleTemplate
    {
        public ExtensionModule() : this(null) { }

        public ExtensionModule(object sender = null) : base(sender)
        {
            ModuleName = "DQ Extension Module";
        }

        public override void Start()
        {
            Console.WriteLine(ModuleName + " is started");
            while (true)
            {
                Console.WriteLine("Printed by DevQuarter Extension Module.");
                Thread.Sleep(1000);
            }
        }

        public override void Reload()
        {
            Console.WriteLine(ModuleName + " is reloaded");
        }
        
        public override void Stop()
        {
            Console.WriteLine(ModuleName + " is stopped");
        }
    }
}
