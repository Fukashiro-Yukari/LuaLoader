using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.IO;
using MelonLoader;
using LuaLoader.UI;

namespace LuaLoader.LuaClass
{
    class Loader
    {
        public static void Msg(object obj)
        {
            MelonLogger.Msg(obj);
        }

        public static void Warning(object obj)
        {
            MelonLogger.Warning(obj);
        }

        public static void Error(object obj)
        {
            MelonLogger.Error(obj);
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

        public static Config GetConfig()
        {
            var config = LuaLoader.Category.GetValue<Config>();

            if (config == null)
                return null;

            return config;
        }

        public static void ReloadLua()
        {
            LuaLoader.ReloadLua();
        }

        public static FileSystemWatcher CreateFileSystemWatcher(string path,NLua.LuaTable table)
        {
            var fileSystemWatcher = new FileSystemWatcher(path);
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            fileSystemWatcher.Filter = (string)(table["Filter"] != null ? table["Filter"] : "*.lua");

            if (table["Changed"] != null)
            {
                fileSystemWatcher.Changed += (sender, args) =>
                {
                    ((NLua.LuaFunction)table["Changed"]).TryCall(sender, args);
                };
            }

            if (table["Created"] != null)
            {
                fileSystemWatcher.Created += (sender, args) =>
                {
                    ((NLua.LuaFunction)table["Created"]).TryCall(sender, args);
                };
            }

            if (table["Deleted"] != null)
            {
                fileSystemWatcher.Deleted += (sender, args) =>
                {
                    ((NLua.LuaFunction)table["Deleted"]).TryCall(sender, args);
                };
            }

            if (table["Disposed"] != null)
            {
                fileSystemWatcher.Disposed += (sender, args) =>
                {
                    ((NLua.LuaFunction)table["Disposed"]).TryCall(sender, args);
                };
            }

            if (table["Error"] != null)
            {
                fileSystemWatcher.Error += (sender, args) =>
                {
                    ((NLua.LuaFunction)table["Error"]).TryCall(sender, args);
                };
            }

            if (table["Renamed"] != null)
            {
                fileSystemWatcher.Renamed += (sender, args) =>
                {
                    ((NLua.LuaFunction)table["Renamed"]).TryCall(sender, args);
                };
            }

            fileSystemWatcher.EnableRaisingEvents = true;

            return fileSystemWatcher;
        }
    }
}
