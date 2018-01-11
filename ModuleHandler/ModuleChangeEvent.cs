using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DevQuarterLibraries
{
    internal class ModuleEventArgs : RenamedEventArgs
    {
        internal ModuleEventArgs(WatcherChangeTypes watcherChangeTypes, string directory, string filename, string oldname) : base(watcherChangeTypes, directory, filename, oldname)
        {
            this.Target = ModuleLoader.ModuleList.Single(x => x.Path == this.OldFullPath);
        }
        
        internal Module Target { get; private set; }
    }

    internal class ModuleWatcher : FileSystemWatcher
    {
        internal ModuleWatcher(string path) : base(path)
        {
            base.Changed += OnChanged;
            base.Renamed += OnRenamed;
            base.Deleted += OnDeleted;
        }

        internal new event EventHandler<ModuleEventArgs> Changed;
        internal new event EventHandler<ModuleEventArgs> Renamed;
        internal new event EventHandler<ModuleEventArgs> Deleted;

        internal void OnChanged(object sender, FileSystemEventArgs e)
        {
            if(e.FullPath.EndsWith(".module.dll"))
                Changed?.Invoke(null, new ModuleEventArgs(e.ChangeType, e.FullPath.Replace(e.Name, String.Empty), e.Name, e.Name));
        }

        internal void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (e.FullPath.EndsWith(".module.dll"))
                Renamed?.Invoke(null, new ModuleEventArgs(e.ChangeType, e.FullPath.Replace(e.Name, String.Empty), e.Name, e.OldName));
        }

        internal void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.EndsWith(".module.dll"))
                Deleted?.Invoke(null, new ModuleEventArgs(e.ChangeType, e.FullPath.Replace(e.Name, String.Empty), null, e.Name));
        }
    }
}
