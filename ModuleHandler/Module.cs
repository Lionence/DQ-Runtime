using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DevQuarterLibraries
{
    public class Module
    {
        public Module(Type type, string path, bool start)
        {
            Status = ModuleStatus.Undefinied;
            this.Path = path;
            _type = type;
            Load(start);
        }

        static Module()
        {
            _moduleWatcher = new ModuleWatcher("Modules") { EnableRaisingEvents = true };
            _moduleWatcher.Changed += _fileWatcher_Changed;
            _moduleWatcher.Renamed += _fileWatcher_Changed;
            _moduleWatcher.Deleted += _fileWatcher_Changed;  //TODO: We will need it in the future
        }
        
        public enum ModuleStatus
        {
            Undefinied,
            Started,
            Starting,
            Stopping,
            Loaded,
            Loading,
            Reloading,
            Unloaded,
            Unloading
        }
        
        public object[] ConstructorParameters { get; internal set; }
        public string GetInfo
        {
            get
            {
                return "TODO";
            }
        }
        public ModuleStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
            }
        }

        internal string Path {
            get { return this._path;  }
            private set
            {
                _fileInfo = new FileInfo(value);
                if (_fileInfo.Exists)
                {
                    this._path = value;
                    this.LastWriteTime = _fileInfo.LastWriteTime;
                }
                else
                    throw new FileNotFoundException("Given path is not valid while creating new module", value);
            }
        }
        internal string ModuleName { get; private set; }
        internal DateTime LastWriteTime;

        private static ModuleWatcher _moduleWatcher;
        readonly private Type _type;
        private string _path;
        private object _instance;
        private ModuleStatus _status;
        private Thread _thread;
        private MethodInfo _method;
        private FileInfo _fileInfo;

        private static void _fileWatcher_Changed(object sender, ModuleEventArgs e)
        {
            Console.WriteLine("Changed someting: {0}", e.ChangeType);
            e.Target._fileInfo.Refresh();
            if (e.ChangeType == WatcherChangeTypes.Changed && e.Target.LastWriteTime != e.Target._fileInfo.LastWriteTime)
            {
                Console.WriteLine("DONE");
                e.Target.Reload(true, e.Target.ConstructorParameters);
            }
            else if (e.ChangeType == WatcherChangeTypes.Created)
            {
                ModuleLoader.LoadModule(e.FullPath, true);
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                if(e.Name.EndsWith(".module.dll"))
                {
                    e.Target.NotifyNameChanged(e.FullPath);
                }
                else
                {
                    Console.WriteLine("Invalid path format given \"{0}\". File name should end with \".module.dll\"!\n\tFilename is changed back to {1}.", e.Name, e.Target._fileInfo.Name);
                    _moduleWatcher.EnableRaisingEvents = false;
                    File.Move(e.FullPath, e.Target.Path);
                    _moduleWatcher.EnableRaisingEvents = true;
                }
                
            }
            // ELSE DO NOTHING - e.ChangeType == WatcherChangeTypes.Deleted
            // We'd not like to delete the module because it is possible that the user doesn't wants to. In cases, the file name could changle with the new version so we should keep it now.
            // TODO: Later we will implement to reload the module in a similar case like describer in the prvious sentence.
            /*
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                //TODO
            }
            */
        }

        public void Load(bool start = false)
        {
            if (Status == ModuleStatus.Unloading)
            {
                while (Status != ModuleStatus.Unloaded) ;
                Load(start);
            }
            else if (Status == ModuleStatus.Unloaded || Status == ModuleStatus.Undefinied)
            {
                try
                {
                    this.Status = ModuleStatus.Loading;
                    this._instance = Activator.CreateInstance(_type);
                    this._method = _type.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public);
                    
                    this.ModuleName = (string)_type.GetProperty("ModuleName").GetValue(_instance, null);
                    Console.WriteLine("Module from " + this.Path + " is successfully loaded.");
                    this.Status = ModuleStatus.Loaded;
                    if (start)
                        Start();
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Module from " + this.Path + " is failed to load. Path may be invalid.");
                    return;
                }
            }
            else
                Console.WriteLine("Can not load module on path \"" + Path + "\" because it is already loaded");
        }

        public void Start(params object[] parameters)
        {
            if(Status == ModuleStatus.Started)
            {
                Console.WriteLine("Can not start " + ModuleName + " because it is already started.");
                return;
            }
            else if (Status == ModuleStatus.Starting)
            {
                Console.WriteLine("Can not start " + ModuleName + " because it is just starting.");
                return;
            }
            else
            {
                Status = ModuleStatus.Starting;
                _thread = new Thread(new ThreadStart(() => { _type.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public).Invoke(_instance, parameters); }));
                _thread.Start();
                Status = ModuleStatus.Started;
            }
        }

        public void Reload(bool start = false, params object[] parameters)
        {
            if(Status != ModuleStatus.Reloading || Status != ModuleStatus.Undefinied || Status != ModuleStatus.Unloaded || Status != ModuleStatus.Unloading)
            {
                Stop();
                Status = ModuleStatus.Reloading;
                _type.GetMethod("Reload", BindingFlags.Instance | BindingFlags.Public).Invoke(_instance, parameters);
                if (start)
                    Start();
                Status = ModuleStatus.Started;
            }
            else if(Status == ModuleStatus.Reloading)
                Console.WriteLine("Can not reload " + ModuleName + " because it is just reloading now.");
            else if(Status == ModuleStatus.Unloading)
                Console.WriteLine("Can not reload " + ModuleName + " because it is just unloading.");
            else
                Console.WriteLine("Can not reload " + ModuleName + " because it is not loaded.");

        }

        public void Stop()
        {
            if(Status == ModuleStatus.Started || Status == ModuleStatus.Starting)
            {
                _type.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public).Invoke(_instance, null);
                Status = ModuleStatus.Stopping;
                if (_thread != null)
                {
                    _thread.Abort();
                    _thread = null;
                }
                Status = ModuleStatus.Loaded;
            }
            else
                Console.WriteLine("Can not stop " + ModuleName + " because it is not started.");
        }

        public void NotifyNameChanged(string newPath)
        {
            if(newPath != this.Path)
            {
                this._fileInfo.Refresh();
                if (!this._fileInfo.Exists)
                {
                    Console.WriteLine("File path of {0} is changed.", this.ModuleName);
                    var fi = new FileInfo(newPath);
                    if (fi.Exists && ModuleLoader.GetExistingModulesPath(ModuleLoader.SelectModules.All).Contains(newPath))
                    {
                        if (ModuleLoader.CheckAssembly(newPath, this._type))
                        {
                            this.Path = newPath;
                            Console.WriteLine("New file path of {0} is \"{1}\"", this.ModuleName, this.Path);
                        }
                        else
                        {
                            Console.WriteLine("Unexpected error: File on new path does does not contain {0}.", this.ModuleName);
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unexpected error: File on the new path is not exists.");
                        return;
                    }
                }
                else
                    Console.WriteLine(this._fileInfo.FullName + "Unexpected error: Old file path still kept or notification is false.");
            }
                
        }
    }
}
