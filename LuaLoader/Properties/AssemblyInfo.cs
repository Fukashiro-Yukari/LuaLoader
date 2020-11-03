using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(LuaLoader.BuildInfo.Description)]
[assembly: AssemblyDescription(LuaLoader.BuildInfo.Description)]
[assembly: AssemblyCompany(LuaLoader.BuildInfo.Company)]
[assembly: AssemblyProduct(LuaLoader.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + LuaLoader.BuildInfo.Author)]
[assembly: AssemblyTrademark(LuaLoader.BuildInfo.Company)]
[assembly: AssemblyVersion(LuaLoader.BuildInfo.Version)]
[assembly: AssemblyFileVersion(LuaLoader.BuildInfo.Version)]
[assembly: MelonInfo(typeof(LuaLoader.LuaLoader), LuaLoader.BuildInfo.Name, LuaLoader.BuildInfo.Version, LuaLoader.BuildInfo.Author, LuaLoader.BuildInfo.DownloadLink)]


// Create and Setup a MelonGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonGameAttribute is found or any of the Values for any MelonGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonMame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null,null)]
