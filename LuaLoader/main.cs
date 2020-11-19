using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using XLua;
using MelonLoader;
using LuaLoader.Config;
using LuaLoader.UI;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
#if CPP
using UnhollowerRuntimeLib;
#endif

namespace LuaLoader
{
    public static class BuildInfo
    {
        public const string Name = "Lua Loader"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Allow the game to run lua language"; // Description for the Mod.  (Set as null if none)
        public const string Author = "NepQ Neko"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.12"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    class Loader
    {
        public static void Print(string s)
        {
            MelonLogger.Log(s);
        }

        public static void Print2(string s)
        {
            Console.WriteLine("[Lua Print] " + s);
        }

        public static bool ShowMouse
        {
            get => m_showMouse;
            set => SetShowMouse(value);
        }

        private static bool m_showMouse;

        private static void SetShowMouse(bool show)
        {
            m_showMouse = show;
            ForceUnlockCursor.UpdateCursorControl();
        }

        public static object[] RunXLuaCode(string code, NLua.LuaTable table = null)
        {
            var xlua = new LuaEnv();

            try
            {
                object[] ret;

                xlua.DoString(LuaLoader.Instance.luacode4, "XLua Init");
                xlua.DoString(LuaLoader.Instance.luacode5, "XLua Init");
                xlua.DoString("nlua = {}", "XLua Init");
                xlua.DoString("require('main')", "XLua Init");

                if (table != null)
                {
                    var keys = table.Keys;
                    var vels = table.Values;
                    var keysa = new object[keys.Count];
                    var velsa = new object[vels.Count];

                    keys.CopyTo(keysa, 0);
                    vels.CopyTo(velsa, 0);

                    for (var i = 0; i < keysa.Length; i++)
                    {
                        var k = keysa[i];
                        var v = velsa[i];

                        xlua.Global.SetInPath("nlua." + k.ToString(), v);
                    }
                }

                ret = xlua.DoString(code, "XLua Run Lua Code");

                xlua.Dispose();

                return ret;
            }
            catch (Exception e)
            {
                try
                {
                    LuaLoader.lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }

            return null;
        }

        public static Assembly RunCSCode(string code, NLua.LuaTable range = null)
        {
            var par = new CompilerParameters() // [Error] System.PlatformNotSupportedException: Operation is not supported on this platform. (In Superliminal game)
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            par.ReferencedAssemblies.Add("system.dll");
            par.ReferencedAssemblies.Add("system.data.dll");
            par.ReferencedAssemblies.Add("system.xml.dll");

            if (range != null)
            {
                var ranges = new string[range.Values.Count];

                range.Values.CopyTo(ranges, 0);

                par.ReferencedAssemblies.AddRange(ranges);
            }

            MelonLogger.Log("test 3");

            var comp = new CSharpCodeProvider();

            MelonLogger.Log("test 4");
            MelonLogger.Log("test 5");

            var res = comp.CompileAssemblyFromSource(par, code);

            MelonLogger.Log("test 6");

            if (res.Errors.HasErrors)
            {
                var errors = new StringBuilder();

                foreach (CompilerError err in res.Errors)
                {
                    errors.Append($"{err.FileName}({err.Line},{err.Column}): error {err.ErrorNumber}: {err.ErrorText}");
                }

                try
                {
                    LuaLoader.lua.lua.GetFunction("hook.Call").Call("OnLuaError", errors.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(errors.ToString());
            }

            return res.CompiledAssembly;
        }

        public static Type GetType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.TryGetTypes())
                {
                    if (type.FullName == fullName)
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        //public static void HarmonyPatch(MethodBase method, LuaFunction prefix = null, LuaFunction postfix = null, LuaFunction tr = null)
        //{
        //    if (prefix != null)
        //    {
        //        LuaLoader.luaprefix.Add(prefix);
        //    }
            
        //    if (postfix != null)
        //    {
        //        LuaLoader.luapostfix.Add(postfix);
        //    }

        //    if (tr != null)
        //    {
        //        LuaLoader.luatrfix.Add(tr);
        //    }

        //    try
        //    {
        //        LuaLoader.harmony.Patch(method, new HarmonyMethod(typeof(LuaLoader).GetMethod(nameof(LuaLoader.LuaPatchPrefix))), new HarmonyMethod(typeof(LuaLoader).GetMethod(nameof(LuaLoader.LuaPatchPostfix))), new HarmonyMethod(typeof(LuaLoader).GetMethod(nameof(LuaLoader.LuaPatchTrfix))));
        //    }
        //    catch (Exception e)
        //    {
        //        MelonLogger.LogError(e.ToString());
        //    }
        //}
    }

    class ents
    {
        public static object FindObjectOfType(Type type)
        {
#if CPP
            var e = UnityEngine.Object.FindObjectOfType(Il2CppType.From(type));

            return e.Il2CppCast(type);
#else
            return UnityEngine.Object.FindObjectOfType(type);
#endif
        }

        public static object FindObjectsOfType(Type type)
        {
#if CPP
            var a = UnityEngine.Object.FindObjectsOfType(Il2CppType.From(type));
            var ret = new object[a.Length];

            for (var i = 0;i < ret.Length; i++)
            {
                ret[i] = a[i].Il2CppCast(type);
            }

            return ret;
#else
            return UnityEngine.Object.FindObjectsOfType(type);
#endif
        }
    }

    public class LuaLoader : MelonMod
    {
        public static LuaTask.LuaEnv lua;
        public static LuaLoader Instance;
        public string luacode = @"
            local old = Print

            function Print(...)
                local r = {}

                for i = 1,select('#',...) do
                    table.insert(r,tostring(select(i,...)))
                end

                if #r == 0 then
                    table.insert(r,'nil')
                end

                old(table.concat(r,'  '))
            end

            local old = Print2

            function Print2(...)
                local r = {}

                for i = 1,select('#',...) do
                    table.insert(r,tostring(select(i,...)))
                end

                if #r == 0 then
                    table.insert(r,'nil')
                end

                old(table.concat(r,'  '))
            end

            print = Print
         ";
        public string luacode2 = "import('LuaLoader');import('LuaLoader','LuaLoader.Config');import('LuaLoader','LuaLoader.Helpers')";
        public string luacode3 = "package.path = '" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/modules/?.lua;" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/bin/?.dll;" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/modules/?.luac'";
        public string luacode4 = "package.path = '" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/xlua/?.lua;" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/bin/?.dll;" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/xlua/?.luac'";
        public string luacode5 = @"
            function print(...)
                local r = {}
            
                for i = 1,select('#',...) do
                    table.insert(r,tostring(select(i,...)))
                end
            
                if #r == 0 then
                    table.insert(r,'nil')
                end
            
                CS.MelonLoader.MelonLogger.Log(table.concat(r,'  '))
            end
        ";
        private static HarmonyInstance harmony;
        //public static LuaFunction[] luaprefix;
        //public static LuaFunction[] luapostfix;
        //public static LuaFunction[] luatrfix;

        public override void OnApplicationStart()
        {
            Instance = this;
            harmony = HarmonyInstance.Create("LuaLoader.Patch");

            config.OnLoad();
            InputManager.Init();
            ForceUnlockCursor.Init();
            MelonLogger.Log("Lua Init");
            InitLua();
            LoadingLua();
            PostLua();
            MelonLogger.Log("Lua Loading Complete");

            PatchIt(typeof(Text));
            PatchIt(typeof(TextMesh));

            LuaCall("OnApplicationStart");

            //Loader.RunCSCode("");
        }

        private static void PatchIt(Type type)
        {
            try
            {
                harmony.Patch(type.GetProperty("text").GetGetMethod(), null, new HarmonyMethod(typeof(LuaLoader).GetMethod(nameof(StringHook), BindingFlags.NonPublic | BindingFlags.Static)));
                MelonLogger.Log($"Patched {type.Name}");
            }
            catch (Exception e)
            {
                MelonLogger.LogError($"FAIL! : Patched {type.Name}\n{e}");
            }
        }

        private static void StringHook(ref string __result)
        {
            try
            {
                var lr = lua.lua.GetFunction("hook.Call").Call("OnGUIGetString", __result);

                if (lr.Length > 0 && lr[0] != null)
                {
                    __result = (string)lr[0];
                }
            }
            catch (Exception e)
            {
                try
                {
                    lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }
        }

        //public static void LuaPatchPrefix(ref object __result, params object[] objs)
        //{
        //    foreach (var f in luaprefix)
        //    {
        //        var r = f.Call(__result, objs);

        //        if (r.Length > 0 && r[0] != null)
        //        {
        //            __result = r[0];

        //            break;
        //        }
        //    }
        //}

        //public static void LuaPatchPostfix(ref object __result, params object[] objs)
        //{
        //    foreach (var f in luapostfix)
        //    {
        //        var r = f.Call(__result, objs);

        //        if (r.Length > 0 && r[0] != null)
        //        {
        //            __result = r[0];

        //            break;
        //        }
        //    }
        //}

        //public static void LuaPatchTrfix(ref object __result, params object[] objs)
        //{
        //    foreach (var f in luatrfix)
        //    {
        //        var r = f.Call(__result, objs);

        //        if (r.Length > 0 && r[0] != null)
        //        {
        //            __result = r[0];

        //            break;
        //        }
        //    }
        //}

        public override void OnApplicationQuit()
        {
            LuaCall("OnApplicationQuit");
        }

        public override void OnLevelWasInitialized(int level)
        {
            try
            {
                lua.lua.GetFunction("hook.Call").Call("OnLevelWasInitialized", level);
            }
            catch (Exception e)
            {
                try
                {
                    lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }
        }

        public override void OnLevelWasLoaded(int level)
        {
            try
            {
                lua.lua.GetFunction("hook.Call").Call("OnLevelWasLoaded", level);
            }
            catch (Exception e)
            {
                try
                {
                    lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }
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

        public override void OnModSettingsApplied()
        {
            LuaCall("OnModSettingsApplied");
        }

        public override void VRChat_OnUiManagerInit()
        {
            LuaCall("VRChat_OnUiManagerInit");
        }

        private void LuaCall(string s)
        {
            try
            {
                lua.lua.GetFunction("hook.Call").Call(s);
            }
            catch (Exception e)
            {
                try
                {
                    lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }
        }

        private void LoadingLuaDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (var f in Directory.GetFiles(dir))
            {
                var fext = Path.GetExtension(f);

                if (fext != ".lua") continue;

                MelonLogger.Log("Lua Loading: " + f);

                try
                {
                    lua.DoFile(f);
                }
                catch (Exception e)
                {
                    try
                    {
                        lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                    }
                    catch (Exception e2)
                    {
                        MelonLogger.LogError(e2.ToString());
                    }

                    MelonLogger.LogError(e.ToString());
                }
            }
        }

        public void InitLua()
        {
            lua = new LuaTask.LuaEnv();
            //lua = new LuaEnv();
            lua.lua.State.Encoding = Encoding.UTF8;
            lua.lua["test"] = "test";
            lua.lua.LoadCLRPackage();
            lua.lua.RegisterFunction("Print", null, typeof(Loader).GetMethod("Print"));
            lua.lua.RegisterFunction("Print2", null, typeof(Loader).GetMethod("Print2"));

            var obj = new LuaLoader();

            lua.lua.RegisterFunction("ReloadLua", obj, obj.GetType().GetMethod("ReloadLua"));
            lua.lua.RegisterFunction("LoadLuaFile", obj, obj.GetType().GetMethod("LoadLuaFile"));
            lua.DoString(luacode2);
            lua.DoString(luacode3);
            lua.DoString(luacode);
        }

        public void PostLua()
        {

        }

        public void LoadingLua()
        {
            var luadir = "Mods/LuaLoader";

            if (!Directory.Exists(luadir))
            {
                Directory.CreateDirectory(luadir);
            }

            LoadingLuaDir("Mods/LuaLoader/libs");
            LoadingLuaDir("Mods/LuaLoader/autorun");
        }

        public void ReloadLua()
        {
            MelonLogger.Log("Lua Reloading");
            LoadingLua();
            MelonLogger.Log("Lua Reloading Complete");
        }

        public object[] LoadLuaFile(string name)
        {
            object[] result = null;

            try
            {
                var fext = Path.GetExtension(name);

                if (fext == ".lua")
                {
                    MelonLogger.Log("Lua Loading: " + name);

                    return lua.DoFile(name);
                }
            }
            catch (Exception e)
            {
                try
                {
                    lua.lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }

            return result;
        }
    }
}
