using System;
using System.Text;
using System.IO;
using MelonLoader;
using MelonLoader.Preferences;
using LuaLoader.UI;
using LuaLoader.LuaClass;
using LuaLoader.Helpers;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Mono.CSharp;

#if CPP
using UnhollowerRuntimeLib;
#endif

namespace LuaLoader
{
    public static class BuildInfo
    {
        public const string Name = "Lua Loader"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Lua environment for MelonLoader"; // Description for the Mod.  (Set as null if none)
        public const string Author = "Kamishiro Kalina"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.2"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class LuaLoader : MelonMod
    {
        public static NLua.Lua lua;
        public static LuaLoader Instance;
        public static MelonPreferences_ReflectiveCategory Category;
        public static bool IsLoadMonoCSharp;

        public LuaLoader() : base()
        {
            Instance = this;
            Category = MelonPreferences.CreateCategory<Config>(BuildInfo.Name);
            Category.SetFilePath("Lua/LuaLoader.cfg");

            var path = Path.Combine(MelonUtils.BaseDirectory, "MelonLoader", "Managed", "Mono.CSharp.dll");

            if (File.Exists(path))
            {
                try
                {
                    Assembly.LoadFrom(path);
                    IsLoadMonoCSharp = true;
                }
                catch (Exception e)
                {
                    MelonLogger.Warning(e);
                }
            }
            else
            {
                MelonLogger.Warning("Assembly 'Mono.CSharp.dll' not found");
            }

            InputManager.Init();
            MelonLogger.Msg("Initialize lua environment");
            InitializationLua();
            LoadingLua();
            MelonLogger.Msg("Lua environment loaded complete");
        }

        private static AssemblyName ParseName(string fullName)
        {
            try
            {
                return new AssemblyName(fullName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override void OnPreSupportModule()
        {
            ForceUnlockCursor.Init();

            LuaCall("OnPreSupportModule");
        }

        public override void OnApplicationStart()
        {
            LuaCall("OnApplicationStart");
        }

        public override void OnApplicationLateStart()
        {
            LuaCall("OnApplicationLateStart");
        }

        public override void OnApplicationQuit()
        {
            LuaCall("OnApplicationQuit");
        }

        public override void OnPreferencesLoaded()
        {
            LuaCall("OnPreferencesLoaded");
        }

        public override void OnPreferencesLoaded(string filepath)
        {
            LuaCall("OnPreferencesLoaded", filepath);
        }

        public override void OnPreferencesSaved()
        {
            LuaCall("OnPreferencesSaved");
        }

        public override void OnPreferencesSaved(string filepath)
        {
            LuaCall("OnPreferencesSaved", filepath);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LuaCall("OnSceneWasInitialized", buildIndex, sceneName);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LuaCall("OnSceneWasLoaded", buildIndex, sceneName);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            LuaCall("OnSceneWasUnloaded", buildIndex, sceneName);
        }

        public override void OnUpdate()
        {
            if (Loader.ShowMouse)
            {
                ForceUnlockCursor.Update();
            }

            LuaCall("OnUpdate");
        }

        public override void OnLateUpdate()
        {
            LuaCall("OnLateUpdate");
        }

        public override void OnFixedUpdate()
        {
            LuaCall("OnFixedUpdate");
        }

        public override void OnGUI()
        {
            LuaCall("OnGUI");
        }

        public override void BONEWORKS_OnLoadingScreen()
        {
            LuaCall("BONEWORKS_OnLoadingScreen");
        }

        private static string dirpath = Directory.GetCurrentDirectory().Replace("\\", "/");
        private static string packagepath = "package.path = '" + dirpath + "/Lua/includes/modules/?.lua;" + dirpath + "/Lua/bin/?.dll;" + dirpath + "/Lua/includes/modules/?.luac'";
        private static string importloader = "import('LuaLoader', 'LuaLoader.LuaClass');import('LuaLoader', 'LuaLoader.Helpers')";
        private static bool loadingincludes;
        private static bool luaenvloaded;

        public static void LuaCall(string name)
        {
            lua.GetFunction("hook.Call").TryCall(name);
        }

        public static void LuaCall(string name,object arg1)
        {
            lua.GetFunction("hook.Call").TryCall(name, arg1);
        }

        public static void LuaCall(string name, object arg1, object arg2)
        {
            lua.GetFunction("hook.Call").TryCall(name, arg1, arg2);
        }

        public static void LuaCall(string name, object arg1, object arg2, object arg3)
        {
            lua.GetFunction("hook.Call").TryCall(name, arg1, arg2, arg3);
        }

        public static void LuaCall(string name, params object[] args)
        {
            if (args == null)
                args = new object[] { name };
            else
            {
                var objs = new object[args.Length + 1];

                objs[0] = name;

                for (var i = 0; i < args.Length; i++)
                    objs[i + 1] = args[i];

                args = objs;
            }

            lua.GetFunction("hook.Call").TryCall(args);
        }

        private static void LoadingLuaDir(string dir)
        {
            foreach (var f in Directory.GetFiles(dir, "*.lua"))
            {
                IncludeLuaFile(f);
            }
        }

        public void InitializationLua()
        {
            lua = new NLua.Lua();
            lua.State.Encoding = Encoding.UTF8;

#if CPP
            lua["CPP"] = true;
#endif

            lua.RegisterFunction("include", GetType().GetMethod(nameof(IncludeLuaFile)));
            lua.RegisterFunction("typeof", typeof(ReflectionHelpers).GetMethod(nameof(ReflectionHelpers.GetActualType)));
            lua.RegisterFunction("ctype", GetType().GetMethod(nameof(GetTypeName)));
            lua.RegisterFunction("cprint", GetType().GetMethod(nameof(LuaCPrint)));

            if (IsLoadMonoCSharp)
                lua.RegisterFunction("Evaluator", GetType().GetMethod(nameof(CreateEvaluator)));

            IncludeLuaFile("Lua/includes/luanet.lua");

            lua.DoString(packagepath);
            lua.DoString(importloader);
            lua.DoString(@"
local Msg = Loader.Msg
local table = table

function print(...)
    local r = {}

    for i = 1, select('#', ...) do table.insert(r, tostring(select(i, ...))) end

    if #r == 0 then table.insert(r, 'nil') end

    Msg(table.concat(r, '  '))
end
");

            loadingincludes = true;

            IncludeLuaFile("Lua/includes/init.lua");

            loadingincludes = false;
            luaenvloaded = true;
        }

        public static void LoadingLua(bool reload = false)
        {
            var path = "Lua/autorun";

            LoadingLuaDir(path);

#if DEBUG
            LuaCall("test");
#endif

            if (reload)
                return;

            void Include(object sender, FileSystemEventArgs args)
            {
                IncludeLuaFile(Path.Combine(path, Path.GetFileName(args.Name)));
            }

            var fileSystemWatcher = new FileSystemWatcher(path);
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            fileSystemWatcher.Filter = "*.lua";
            fileSystemWatcher.Changed += Include;
            fileSystemWatcher.Renamed += Include;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        public static void ReloadLua()
        {
            MelonLogger.Msg("Lua environment Reloading");
            LoadingLua(true);
            MelonLogger.Msg("Lua environment Reloading Complete");
        }

        public static object[] IncludeLuaFile(string name, bool isunsafe = false)
        {
            name = name.Replace("\\", "/");

            if (!isunsafe)
            {
                var ext = Path.GetExtension(name);

                if (ext != null && ext == string.Empty)
                    name = name + ".lua";

                if (loadingincludes && File.Exists("Lua/includes/" + name))
                    name = "Lua/includes/" + name;

                if (!name.StartsWith("Lua/"))
                    name = "Lua/" + name;
            }

#if DEBUG
            MelonLogger.Msg($"Loading Lua File: {name}");
#endif

            try
            {
                return lua.DoFile(name);
            }
            catch (Exception e)
            {
                LuaError(e);
            }

            return null;
        }

        public static void LuaCPrint(params object[] args)
        {
            if (args == null)
                args = new object[0];

            MelonLogger.Msg(makeString(args));
        }

        private static string makeString(object[] args)
        {
            if (args.Length == 0 || args[0] == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var text = args[0].ToString();

            if (text != null)
                sb.Append(text);

            for (int i = 1; i < args.Length; i++)
            {
                sb.Append("  ");
                if (args[i] != null)
                {
                    if (args[i] is NLua.ProxyType ntype)
                        args[i] = ntype.UnderlyingSystemType;

                    text = args[i].ToString();
                    if (text != null)
                        sb.Append(text);
                }
            }

            return sb.ToString();
        }

        public static string GetTypeName(object obj)
        {
            var type = ReflectionHelpers.GetActualType(obj);

            if (type == null) return null;

            return type.Name;
        }

        public static void AddMethodDynamically(TypeBuilder myTypeBld,
                                    string mthdName,
                                    Type[] mthdParams,
                                    Type returnType,
                                    string mthdAction)
        {
            MethodBuilder myMthdBld = myTypeBld.DefineMethod(
                                                    mthdName,
                                                    MethodAttributes.Public |
                                                    MethodAttributes.Static,
                                                    returnType,
                                                    mthdParams);
            ILGenerator ILout = myMthdBld.GetILGenerator();
            int numParams = mthdParams.Length;
            for (byte x = 0; x < numParams; x++)
            {
                ILout.Emit(OpCodes.Ldarg_S, x);
            }
            if (numParams > 1)
            {
                for (int y = 0; y < (numParams - 1); y++)
                {
                    switch (mthdAction)
                    {
                        case "A":
                            ILout.Emit(OpCodes.Add);
                            break;
                        case "M":
                            ILout.Emit(OpCodes.Mul);
                            break;
                        default:
                            ILout.Emit(OpCodes.Add);
                            break;
                    }
                }
            }
            ILout.Emit(OpCodes.Ret);
        }

        public static void LuaError(Exception e)
        {
            if (luaenvloaded)
            {
                try
                {
                    lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString(), e);
                }
                catch (Exception e2)
                {
                    MelonLogger.Error(e2.ToString());
                }
            }

            MelonLogger.Error(e.ToString());
        }

        public static void LuaError(object e)
        {
            if (luaenvloaded)
            {
                try
                {
                    lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.Error(e2.ToString());
                }
            }

            MelonLogger.Error(e.ToString());
        }

        public static Evaluator CreateEvaluator()
        {
            if (!IsLoadMonoCSharp)
            {
                MelonLogger.Warning("Assembly 'Mono.CSharp.dll' was not loaded");
                return null;
            }

            var ctx = CreateContext(new StreamReportPrinter(new LoggerTextWriter()));

            return new Evaluator(ctx);
        }

        private static CompilerContext CreateContext(ReportPrinter reportPrinter)
        {
            var settings = new CompilerSettings
            {
                Version = LanguageVersion.Experimental,
                GenerateDebugInfo = false,
                StdLib = true,
                Target = Target.Library
            };

            return new CompilerContext(settings, reportPrinter);
        }
    }

    internal class LoggerTextWriter : TextWriter
    {
        private readonly StringBuilder sb = new StringBuilder();

        public LoggerTextWriter() { }

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\n')
            {
                MelonLogger.Warning(sb.ToString());
                sb.Length = 0;
                return;
            }

            sb.Append(value);
        }
    }
}
