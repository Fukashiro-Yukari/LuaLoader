using System;
using System.Text;
using System.IO;
using System.Reflection;
using NLua;
using MelonLoader;
using LuaLoader.Config;
using LuaLoader.UI;
using Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace LuaLoader
{
    public static class BuildInfo
    {
        public const string Name = "Lua Loader"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Allow the game to run lua language"; // Description for the Mod.  (Set as null if none)
        public const string Author = "NepQ Neko"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "0.1"; // Version of the Mod.  (MUST BE SET)
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
            Console.WriteLine("[Lua Print] "+s);
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

        public static Lua CreateLua()
        {
            var nlua = new Lua();

            nlua.State.Encoding = Encoding.UTF8;
            nlua["test"] = "test";
            nlua.LoadCLRPackage();
            nlua.RegisterFunction("Print", null, typeof(Loader).GetMethod("Print"));
            nlua.RegisterFunction("Print2", null, typeof(Loader).GetMethod("Print2"));

            var obj = new LuaLoader();

            nlua.RegisterFunction("ReloadLua", obj, obj.GetType().GetMethod("ReloadLua"));
            nlua.DoString(LuaLoader.Instance.luacode2);
            nlua.DoString(LuaLoader.Instance.luacode3);
            nlua.DoString(LuaLoader.Instance.luacode);

            return nlua;
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
    }

    class ents
    {
        public static UnityEngine.Object GetPlayer()
        {
            return UnityEngine.Object.FindObjectOfType<Player>();
        }

        public static UnityEngine.Object[] GetPlayers()
        {
            return UnityEngine.Object.FindObjectsOfType<Player>();
        }

        public static Type GetPlayerType()
        {
            return typeof(Player);
        }
    }

    public class LuaLoader : MelonMod
    {
        public static Lua lua;
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

            print = Print2
         ";
        public string luacode2 = "import('LuaLoader');import('LuaLoader','LuaLoader.Config')";
        public string luacode3 = "package.path = '" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/modules/?.lua;" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/dll/?.dll;" + Directory.GetCurrentDirectory().Replace("\\", "/") + "/Mods/LuaLoader/modules/?.luac'";
        private static HarmonyInstance harmony;

        public override void OnApplicationStart()
        {
            Instance = this;

            config.OnLoad();
            InputManager.Init();
            ForceUnlockCursor.Init();
            MelonLogger.Log("Lua Init");
            InitLua();
            LoadingLua();
            PostLua();
            MelonLogger.Log("Lua Loading Complete");

            harmony = HarmonyInstance.Create("LuaLoader.StringHook");
            PatchIt(typeof(Text));
            PatchIt(typeof(TextMesh));

            LuaCall("OnApplicationStart");
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
                MelonLogger.LogError($"FAIL! : Patched {type.Name}\n{e.ToString()}");
            }
        }

        private static void StringHook(ref string __result)
        {
            try
            {
                var lr = lua.GetFunction("hook.Call").Call("OnGUIGetString", __result);

                if (lr.Length > 0 && lr[0] != null)
                {
                    __result = (string)lr[0];
                }
            }
            catch (Exception e)
            {
                try
                {
                    lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
                }
                catch (Exception e2)
                {
                    MelonLogger.LogError(e2.ToString());
                }

                MelonLogger.LogError(e.ToString());
            }
        }

        public override void OnApplicationQuit()
        {
            LuaCall("OnApplicationQuit");
        }

        public override void OnLevelWasInitialized(int level)
        {
            try
            {
                lua.GetFunction("hook.Call").Call("OnLevelWasInitialized", level);
            }
            catch (Exception e)
            {
                try
                {
                    lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
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
                lua.GetFunction("hook.Call").Call("OnLevelWasLoaded", level);
            }
            catch (Exception e)
            {
                try
                {
                    lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
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
                lua.GetFunction("hook.Call").Call(s);
            }
            catch (Exception e)
            {
                try
                {
                    lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
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
                var ftype = f.Split('.');

                if (ftype.Length < 2 || ftype[1] != "lua") continue;

                MelonLogger.Log("Lua Loading: " + f);

                try
                {
                    lua.DoFile(f);
                }
                catch (Exception e)
                {
                    try
                    {
                        lua.GetFunction("hook.Call").Call("OnLuaError", e.ToString());
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
            lua = new Lua();
            //lua = new LuaEnv();
            lua.State.Encoding = Encoding.UTF8;
            lua["test"] = "test";
            lua.LoadCLRPackage();
            lua.RegisterFunction("Print", null, typeof(Loader).GetMethod("Print"));
            lua.RegisterFunction("Print2", null, typeof(Loader).GetMethod("Print2"));

            var obj = new LuaLoader();

            lua.RegisterFunction("ReloadLua", obj, obj.GetType().GetMethod("ReloadLua"));
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
    }
}
