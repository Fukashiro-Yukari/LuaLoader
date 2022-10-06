﻿using System;
using UnityEngine;
using LuaLoader.Helpers;
using LuaLoader.LuaClass;
using BF = System.Reflection.BindingFlags;
using HarmonyLib;

namespace LuaLoader.UI
{
    public class ForceUnlockCursor
    {
        public static bool Unlock
        {
            get => m_forceUnlock;
            set => SetForceUnlock(value);
        }
        private static bool m_forceUnlock;

        public static bool ShouldForceMouse => Loader.ShowMouse && Unlock;

        private static CursorLockMode m_lastLockMode;
        private static bool m_lastVisibleState;

        private static bool m_currentlySettingCursor = false;

        private static Type CursorType 
            => m_cursorType 
            ?? (m_cursorType = ReflectionHelpers.GetTypeByName("UnityEngine.Cursor"));
        private static Type m_cursorType;

        public static void Init()
        {
            try
            {
                if (CursorType == null)
                {
                    throw new Exception("Could not find Type 'UnityEngine.Cursor'!");
                }

                // Get current cursor state and enable cursor
                try
                {
                    m_lastLockMode = (CursorLockMode)typeof(Cursor).GetProperty("lockState", BF.Public | BF.Static).GetValue(null, null);
                    m_lastVisibleState = (bool)typeof(Cursor).GetProperty("visible", BF.Public | BF.Static).GetValue(null, null);
                }
                catch 
                {
                    m_lastLockMode = CursorLockMode.None;
                    m_lastVisibleState = true;
                }

                // Setup Harmony Patches
                TryPatch("lockState", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(Prefix_set_lockState))), true);
                TryPatch("lockState", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(Postfix_get_lockState))), false);

                TryPatch("visible", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(Prefix_set_visible))), true);
                TryPatch("visible", new HarmonyMethod(typeof(ForceUnlockCursor).GetMethod(nameof(Postfix_get_visible))), false);
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Warning($"Exception on CursorControl.Init! {e.GetType()}, {e.Message}");
            }

            Unlock = true;
        }

        private static void TryPatch(string property, HarmonyMethod patch, bool setter)
        {
            try
            {
                var harmony = LuaLoader.Instance.HarmonyInstance;
                var prop = typeof(Cursor).GetProperty(property);

                if (setter)
                {
                    // setter is prefix
                    harmony.Patch(prop.GetSetMethod(), prefix: patch);
                }
                else
                {
                    // getter is postfix
                    harmony.Patch(prop.GetGetMethod(), postfix: patch);
                }
            }
            catch (Exception e)
            {
                string s = setter ? "set_" : "get_" ;
                MelonLoader.MelonLogger.Warning($"Unable to patch Cursor.{s}{property}: {e.Message}");
            }
        }

        private static void SetForceUnlock(bool unlock)
        {
            m_forceUnlock = unlock;
            UpdateCursorControl();
        }

        public static void Update()
        {
            // Check Force-Unlock input
            if (InputManager.GetKeyDown(KeyCode.LeftAlt))
            {
                Unlock = !Unlock;
            }
        }

        public static void UpdateCursorControl()
        {
            try
            {
                m_currentlySettingCursor = true;
                if (ShouldForceMouse)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = m_lastLockMode;
                    Cursor.visible = m_lastVisibleState;
                }
                m_currentlySettingCursor = false;
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Msg($"Exception setting Cursor state: {e.GetType()}, {e.Message}");
            }
        }

        // Force mouse to stay unlocked and visible while UnlockMouse and ShowMenu are true.
        // Also keep track of when anything else tries to set Cursor state, this will be the
        // value that we set back to when we close the menu or disable force-unlock.

        [HarmonyPrefix]
        public static void Prefix_set_lockState(ref CursorLockMode value)
        {
            if (!m_currentlySettingCursor)
            {
                m_lastLockMode = value;

                if (ShouldForceMouse)
                {
                    value = CursorLockMode.None;
                }
            }
        }

        [HarmonyPrefix]
        public static void Prefix_set_visible(ref bool value)
        {
            if (!m_currentlySettingCursor)
            {
                m_lastVisibleState = value;

                if (ShouldForceMouse)
                {
                    value = true;
                }
            }
        }

        [HarmonyPrefix]
        public static void Postfix_get_lockState(ref CursorLockMode __result)
        {
            if (ShouldForceMouse)
            {
                __result = m_lastLockMode;
            }
        }

        [HarmonyPrefix]
        public static void Postfix_get_visible(ref bool __result)
        {
            if (ShouldForceMouse)
            {
                __result = m_lastVisibleState;
            }
        }
    }
}
