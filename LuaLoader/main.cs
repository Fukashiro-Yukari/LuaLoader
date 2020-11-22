using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using System.Collections.Generic;
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

            var comp = new CustomCSharpCodeGenerator();
            var res = comp.CompileAssemblyFromSource(par, code);

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

        public static object[] CreateArray(NLua.LuaTable t)
        {
            var rt = new object[t.Values.Count];

            t.Values.CopyTo(rt, 0);

            return rt;
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

    public class CustomCSharpCodeGenerator
    {
        private readonly IDictionary<string, string> _provOptions;

        internal CustomCSharpCodeGenerator()
        {
        }

        internal CustomCSharpCodeGenerator(IDictionary<string, string> providerOptions)
        {
            _provOptions = providerOptions;
        }

        private string FileExtension
        {
            get => ".cs";
        }

        public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            CompilerResults result;
            try
            {
                result = FromSource(options, source);
            }
            finally
            {
                try
                {
                    options.TempFiles.Delete();
                }
                finally
                {
                }
            }
            return result;
        }

        private CompilerResults FromSource(CompilerParameters options, string source)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            return FromSourceBatch(options, new string[]
            {
                source
            });
        }

        private CompilerResults FromSourceBatch(CompilerParameters options, string[] sources)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (sources == null)
            {
                throw new ArgumentNullException("sources");
            }
            string[] array = new string[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                string text = options.TempFiles.AddExtension(i + FileExtension);
                using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        streamWriter.Write(sources[i]);
                        streamWriter.Flush();
                    }
                }
                array[i] = text;
            }
            return this.FromFileBatch(options, array);
        }

        private CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (fileNames == null)
                throw new ArgumentNullException(nameof(fileNames));

            CompilerResults results = new CompilerResults(options.TempFiles);
            Process mcs = new Process();

            // FIXME: these lines had better be platform independent.
            if (Path.DirectorySeparatorChar == '\\')
            {
                mcs.StartInfo.FileName = Path.GetFullPath("Mods\\LuaLoader\\Mono\\bin\\mono.exe");
                mcs.StartInfo.Arguments = "\""+ Path.GetFullPath("Mods\\LuaLoader\\Mono\\lib\\mono\\4.5\\mcs.exe") + "\" ";
            }
            else
            {
                mcs.StartInfo.FileName = Path.GetFullPath("Mods\\LuaLoader\\Mono\\bin\\mcs");
            }

            mcs.StartInfo.Arguments += BuildArgs(options, fileNames, _provOptions);

            var stderr_completed = new ManualResetEvent(false);
            var stdout_completed = new ManualResetEvent(false);
            /*		       
                        string monoPath = Environment.GetEnvironmentVariable ("MONO_PATH");
                        if (monoPath != null)
                            monoPath = String.Empty;
                        string privateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
                        if (privateBinPath != null && privateBinPath.Length > 0)
                            monoPath = String.Format ("{0}:{1}", privateBinPath, monoPath);
                        if (monoPath.Length > 0) {
                            StringDictionary dict = mcs.StartInfo.EnvironmentVariables;
                            if (dict.ContainsKey ("MONO_PATH"))
                                dict ["MONO_PATH"] = monoPath;
                            else
                                dict.Add ("MONO_PATH", monoPath);
                        }
            */
            /*
			 * reset MONO_GC_PARAMS - we are invoking compiler possibly with another GC that
			 * may not handle some of the options causing compilation failure
			 */
            mcs.StartInfo.EnvironmentVariables.Remove("MONO_GC_PARAMS");
            mcs.StartInfo.CreateNoWindow = true;
            mcs.StartInfo.UseShellExecute = false;
            mcs.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            mcs.StartInfo.RedirectStandardOutput = true;
            mcs.StartInfo.RedirectStandardError = true;
            mcs.ErrorDataReceived += new DataReceivedEventHandler((sender, args) => {
                if (args.Data != null)
                    results.Output.Add(args.Data);
                else
                    stderr_completed.Set();
            });
            mcs.OutputDataReceived += new DataReceivedEventHandler((sender, args) => {
                if (args.Data == null)
                    stdout_completed.Set();
            });

            // Use same text decoder as mcs and not user set values in Console
            mcs.StartInfo.StandardOutputEncoding =
            mcs.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            try
            {
                mcs.Start();
            }
            catch (Exception e)
            {
                Win32Exception exc = e as Win32Exception;
                if (exc != null)
                {
                    throw new SystemException(String.Format("Error running {0}: {1}", mcs.StartInfo.FileName, exc.Message));
                }
                throw;
            }

            try
            {
                mcs.BeginOutputReadLine();
                mcs.BeginErrorReadLine();
                mcs.WaitForExit();

                results.NativeCompilerReturnValue = mcs.ExitCode;
            }
            finally
            {
                stderr_completed.WaitOne(TimeSpan.FromSeconds(30));
                stdout_completed.WaitOne(TimeSpan.FromSeconds(30));
                mcs.Close();
            }

            bool loadIt = true;
            foreach (string error_line in results.Output)
            {
                CompilerError error = CreateErrorFromString(error_line);
                if (error != null)
                {
                    results.Errors.Add(error);
                    if (!error.IsWarning)
                        loadIt = false;
                }
            }

            if (results.Output.Count > 0)
            {
                results.Output.Insert(0, mcs.StartInfo.FileName + " " + mcs.StartInfo.Arguments + Environment.NewLine);
            }

            if (loadIt)
            {
                if (!File.Exists(options.OutputAssembly))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string s in results.Output)
                        sb.Append(s + Environment.NewLine);

                    throw new Exception("Compiler failed to produce the assembly. Output: '" + sb.ToString() + "'");
                }

                if (options.GenerateInMemory)
                {
                    using (FileStream fs = File.OpenRead(options.OutputAssembly))
                    {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        results.CompiledAssembly = Assembly.Load(buffer, null);
                        fs.Close();
                    }
                }
                else
                {
                    // Avoid setting CompiledAssembly right now since the output might be a netmodule
                    results.PathToAssembly = options.OutputAssembly;
                }
            }
            else
            {
                results.CompiledAssembly = null;
            }

            return results;
        }

        private static string BuildArgs(CompilerParameters options, string[] fileNames, IDictionary<string, string> providerOptions)
        {
            StringBuilder args = new StringBuilder();
            if (options.GenerateExecutable)
                args.Append("/target:exe ");
            else
                args.Append("/target:library ");

            string privateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            if (privateBinPath != null && privateBinPath.Length > 0)
                args.AppendFormat("/lib:\"{0}\" ", privateBinPath);

            if (options.Win32Resource != null)
                args.AppendFormat("/win32res:\"{0}\" ",
                    options.Win32Resource);

            if (options.IncludeDebugInformation)
                args.Append("/debug+ /optimize- ");
            else
                args.Append("/debug- /optimize+ ");

            if (options.TreatWarningsAsErrors)
                args.Append("/warnaserror ");

            if (options.WarningLevel >= 0)
                args.AppendFormat("/warn:{0} ", options.WarningLevel);

            if (options.OutputAssembly == null || options.OutputAssembly.Length == 0)
            {
                string extension = (options.GenerateExecutable ? "exe" : "dll");
                options.OutputAssembly = GetTempFileNameWithExtension(options.TempFiles, extension,
                    !options.GenerateInMemory);
            }
            args.AppendFormat("/out:\"{0}\" ", options.OutputAssembly);

            foreach (string import in options.ReferencedAssemblies)
            {
                if (import == null || import.Length == 0)
                    continue;

                args.AppendFormat("/r:\"{0}\" ", import);
            }

            if (options.CompilerOptions != null)
            {
                args.Append(options.CompilerOptions);
                args.Append(" ");
            }

            foreach (string embeddedResource in options.EmbeddedResources)
            {
                args.AppendFormat("/resource:\"{0}\" ", embeddedResource);
            }

            foreach (string linkedResource in options.LinkedResources)
            {
                args.AppendFormat("/linkresource:\"{0}\" ", linkedResource);
            }

            if (providerOptions != null && providerOptions.Count > 0)
            {
                string langver;

                if (!providerOptions.TryGetValue("CompilerVersion", out langver))
                    langver = "3.5";

                if (langver.Length >= 1 && langver[0] == 'v')
                    langver = langver.Substring(1);

                switch (langver)
                {
                    case "2.0":
                        args.Append("/langversion:ISO-2 ");
                        break;

                    case "3.5":
                        // current default, omit the switch
                        break;
                }
            }

            args.Append("/noconfig ");

            args.Append(" -- ");
            foreach (string source in fileNames)
                args.AppendFormat("\"{0}\" ", source);
            return args.ToString();
        }

        // Keep in sync with mcs/class/Microsoft.Build.Utilities/Microsoft.Build.Utilities/ToolTask.cs
        const string ErrorRegexPattern = @"
			^
			(\s*(?<file>[^\(]+)                         # filename (optional)
			 (\((?<line>\d*)(,(?<column>\d*[\+]*))?\))? # line+column (optional)
			 :\s+)?
			(?<level>\w+)                               # error|warning
			\s+
			(?<number>[^:]*\d)                          # CS1234
			:
			\s*
			(?<message>.*)$";

        static readonly Regex RelatedSymbolsRegex = new Regex(
            @"
            \(Location\ of\ the\ symbol\ related\ to\ previous\ (warning|error)\)
			",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private static CompilerError CreateErrorFromString(string error_string)
        {
            if (error_string.StartsWith("BETA"))
                return null;

            if (error_string == null || error_string == "")
                return null;

            CompilerError error = new CompilerError();
            Regex reg = new Regex(ErrorRegexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
            Match match = reg.Match(error_string);
            if (!match.Success)
            {
                match = RelatedSymbolsRegex.Match(error_string);
                if (!match.Success)
                {
                    // We had some sort of runtime crash
                    error.ErrorText = error_string;
                    error.IsWarning = false;
                    error.ErrorNumber = "";
                    return error;
                }
                else
                {
                    // This line is a continuation of previous warning of error
                    return null;
                }
            }
            if (String.Empty != match.Result("${file}"))
                error.FileName = match.Result("${file}");
            if (String.Empty != match.Result("${line}"))
                error.Line = Int32.Parse(match.Result("${line}"));
            if (String.Empty != match.Result("${column}"))
                error.Column = Int32.Parse(match.Result("${column}").Trim('+'));

            string level = match.Result("${level}");
            if (level == "warning")
                error.IsWarning = true;
            else if (level != "error")
                return null; // error CS8028 will confuse the regex.

            error.ErrorNumber = match.Result("${number}");
            error.ErrorText = match.Result("${message}");
            return error;
        }

        private static string GetTempFileNameWithExtension(TempFileCollection temp_files, string extension, bool keepFile)
        {
            return temp_files.AddExtension(extension, keepFile);
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
            lua.lua.State.Encoding = Encoding.UTF8;
            lua.lua["test"] = "test";

#if CPP
            lua.lua["CPP"] = true;
#endif

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
