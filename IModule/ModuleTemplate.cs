using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace DevQuarterLibraries
{
    public abstract class ModuleTemplate
    {
        protected ModuleTemplate(object sender = null)
        {

        }

        private string _moduleName = null;

        public string ModuleName
        {
            get { return _moduleName; }
            protected set  { _moduleName = value; }
        }

        abstract public void Start();
        abstract public void Reload();
        abstract public void Stop();
    }
}
