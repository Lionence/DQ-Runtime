using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DevQuarterLibraries
{
    class ModuleLoader : MarshalByRefObject
    {
        public enum SelectModules
        {
            All,
            Necessery,
            Loaded,
            Started,
            New
        }
        
        public static List<Module> ModuleList = new List<Module>();

        public static bool CheckAssembly(string path, Type type)
        {
            var tempDomain = AppDomain.CreateDomain("AssemblyTest");
            byte[] assemblyArray = File.ReadAllBytes(path);
            var asm = Assembly.Load(assemblyArray);
            Type[] types = asm.GetTypes().Where(x => x.IsSubclassOf(asm.GetType("DevQuarterLibraries.ModuleTemplate"))).ToArray();
            if (types.Length > 0)
                return true;
            else return false;
        }

        public static string[] GetExistingModulesPath(SelectModules selectModules)
        {
            List<string> returnAble = new List<string>();

            string[] files = Directory.GetFiles(@"Modules\");
            foreach (var filePath in files)
                if (filePath.EndsWith(".module.dll"))
                {
                    if (selectModules == SelectModules.All)
                        returnAble.Add(filePath);

                    else if (selectModules == SelectModules.New)
                    {
                        if (!ModuleList.Exists(x => x.Path == filePath))
                            returnAble.Add(filePath);
                    }
                    
                    else if (selectModules == SelectModules.Started)
                    {
                        if (ModuleList.Find(x => x.Path == filePath).Status == Module.ModuleStatus.Started)
                            returnAble.Add(filePath);
                    }
                    
                    else if (selectModules == SelectModules.Loaded)
                    {
                        if (ModuleList.Find(x => x.Path == filePath).Status == Module.ModuleStatus.Loaded)
                            returnAble.Add(filePath);
                    }
                        
                    else if (selectModules == SelectModules.Necessery)
                    {
                        if (ModuleList.Find(x => x.Path == filePath).LastWriteTime != new FileInfo(filePath).LastWriteTime)
                            returnAble.Add(filePath);
                    }
                }
                else
                    Console.WriteLine("File in path \"" + filePath + "\" has not a valid module extension (.module.dll).");

            return returnAble.ToArray();
        }

        public static Module[] LoadModule(string[] filePaths, bool start = false)
        {
            try
            {
                if (filePaths.Length > 0)
                {
                    List<Module> ret = new List<Module>();
                    foreach (var filePath in filePaths)
                        ret.AddRange(LoadModule(filePath, start));
                    if (ret.Count == 0)
                        throw new Exception("None of the given files refering to any modules.");
                    return ret.ToArray();
                }
                else
                    throw new Exception("No file paths given.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static Module[] LoadModule(string filePath, bool start = false)
        {
            try
            {
                if (File.Exists(filePath) && filePath.EndsWith(".module.dll") && !ModuleList.Exists(x => x.Path == filePath))
                {
                    Assembly a = Assembly.Load(File.ReadAllBytes(filePath));
                    Type[] types = a.GetTypes().Where(x => x.IsSubclassOf(a.GetType("DevQuarterLibraries.ModuleTemplate"))).ToArray();
                    List<Module> modules = new List<Module>();
                    foreach (var t in types)
                    {
                        Module m = new Module(t, filePath, start);
                        modules.Add(m);
                        ModuleList.Add(m);
                    }
                    if (modules.Count == 0)
                        throw new Exception("Given file on path \"" + filePath + "\" is not Refering to modules.");
                    else
                        return modules.ToArray();
                }
                else
                    throw new Exception("Given file path \"" + filePath + "\" is not valid.");
            }
            catch
            {
                Console.WriteLine("Unknown error: Can't load module from \"{0}\".", filePath);
                return null;
            }
        }
        
        public static Module[] LoadModule(SelectModules selectModules, bool start = false)
        {
            try
            {
                if (selectModules == SelectModules.New)
                {
                    string[] files = Directory.GetFiles(@"Modules/");
                    System.Collections.Concurrent.ConcurrentBag<Module> ret = new System.Collections.Concurrent.ConcurrentBag<Module>();
                    Parallel.ForEach(files, (filePath) => 
                    {
                        if(filePath.EndsWith(".module.dll"))
                            Parallel.ForEach(LoadModule(filePath, start), (module) =>
                            {
                                ret.Add(module);
                            });
                    });     
                    if (ret.Count == 0)
                        throw new Exception("None of the module files marked as new are refering to any modules!");
                    return ret.ToArray();
                }
                else
                    throw new Exception("You can load only new or specified modules. You can also remove or reload them reloading them.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public static void ReloadModule(SelectModules selectModules, bool loadNull = false)
        {
            if(selectModules == SelectModules.New)
            {
                Console.WriteLine("You can not reload new moules because they are not loaded yet. In this case try with simply load them.");
                return;
            }
            string[] files = Directory.GetFiles("Modules/");
            foreach (var filePath in files)
            {
                if (filePath.EndsWith(".module.dll"))
                {
                    var module = ModuleList.Find(x => x.Path == filePath);
                    if (selectModules == SelectModules.All)
                        if (module == null && loadNull)
                            LoadModule(filePath);
                        else
                            ReloadModule(ref module);

                    else if (selectModules == SelectModules.Necessery)
                    {
                        if (module != null && module.LastWriteTime != new FileInfo(filePath).LastWriteTime)
                            ReloadModule(ref module);
                    }

                    else if (selectModules == SelectModules.Loaded)
                    {
                        if (module.Status == Module.ModuleStatus.Loaded)
                            ReloadModule(ref module);
                        else if (module.Status == Module.ModuleStatus.Stopping)
                        {
                            while (module.Status != Module.ModuleStatus.Loaded)
                                System.Threading.Thread.Sleep(500);
                            ReloadModule(ref module);
                        }
                    }
                    else if (selectModules == SelectModules.Started)
                    {
                        if (module.Status == Module.ModuleStatus.Started)
                            ReloadModule(ref module);
                        else if (module.Status == Module.ModuleStatus.Starting)
                        {
                            while (module.Status != Module.ModuleStatus.Started)
                                System.Threading.Thread.Sleep(500);
                            ReloadModule(ref module);
                        }
                    }
                }
                else
                    Console.WriteLine("File in path \"" + filePath + "\" has not a valid module extension (.module.dll).");
            }
        }
        
        
        public static void ReloadModule(ref Module module, bool start = false)
        {
            module.Reload(start, module.ConstructorParameters);
        }

        public static void ReloadModule(string filePath, bool start = false)
        {
            var module = ModuleList.Find(x => x.Path == filePath);
            module.Reload(start, module.ConstructorParameters);
        }

        public static void ReloadModule(ref Module[] modules, bool start = false)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                ReloadModule(ref modules[i], start);
            }
        }

        public static void RemoveModule(ref Module[] modules)
        {
            if(modules == null || modules.Length == 0)
                Console.WriteLine("No modules given.");
            else
                for (int i = 0; i < modules.Length; i++)
                    RemoveModule(ref modules[i]);
        }

        public static void RemoveModule(ref Module module)
        {
            if(module == null)
            {
                Console.WriteLine("Invalid value is given.");
                return;
            }
            if (module.Status != Module.ModuleStatus.Loaded)
            {
                module.Stop();
            }
            else if (module.Status == Module.ModuleStatus.Stopping)
            {
                while (module.Status != Module.ModuleStatus.Loaded)
                    System.Threading.Thread.Sleep(500);
                module.Stop();
            }
            ModuleList.Remove(module);
        }
    }

}
