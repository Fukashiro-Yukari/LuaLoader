using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace LuaLoader.Config
{
    public class config
    {
        [XmlIgnore] public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(config));

        [XmlIgnore] private const string EXPLORER_FOLDER = @"Mods\LuaLoader";
        [XmlIgnore] private const string SETTINGS_PATH = EXPLORER_FOLDER + @"\config.xml";

        [XmlIgnore] public static config Instance;

        // Actual configs
        public KeyCode ReloadLuaKey = KeyCode.F2;
        public KeyCode ConsoleKey = KeyCode.F10;

        public static void OnLoad()
        {
            if (!Directory.Exists(EXPLORER_FOLDER))
            {
                Directory.CreateDirectory(EXPLORER_FOLDER);
            }

            if (LoadSettings()) return;

            Instance = new config();
            SaveSettings();
        }

        // returns true if settings successfully loaded
        public static bool LoadSettings()
        {
            if (!File.Exists(SETTINGS_PATH))
                return false;

            try
            {
                using (var file = File.OpenRead(SETTINGS_PATH))
                {
                    Instance = (config)Serializer.Deserialize(file);
                }
            }
            catch
            {
                return false;
            }

            return Instance != null;
        }

        public static void SaveSettings()
        {
            if (File.Exists(SETTINGS_PATH))
                File.Delete(SETTINGS_PATH);

            using (var file = File.Create(SETTINGS_PATH))
            {
                Serializer.Serialize(file, Instance);
            }
        }
    }
}
