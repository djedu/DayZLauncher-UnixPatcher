using DayZLauncher.UnixPatcher;
using DayZLauncher.UnixPatcher.Patches;

Common.WriteLine("NOTICE: This version will not enable settings saving, but mod installing should still work.", ConsoleColor.Yellow);
Common.WriteLine("Be sure to verify game files and delete compatdata for `221100` before continuing!", ConsoleColor.Red);

string? userInput;
if (args is null || args.Length < 1)
{
    Common.WriteLine("");
    Common.WriteLine("Enter full DayZ installation path and press ENTER");
    Console.Write("> ");
    userInput = Console.ReadLine()?.Trim() + '/';
}
else
{
    userInput = Path.GetDirectoryName(args[0].Trim() + '/');
}

if (string.IsNullOrWhiteSpace(userInput) || !Directory.Exists(userInput))
{
    Common.WriteLine("Must provide path to DayZ installation folder as argument!", ConsoleColor.Yellow);
    return;
}

var targetAssembly = $"{userInput}/Launcher/Utils.dll";
if (!File.Exists(targetAssembly))
{
    Common.WriteLine("Could not find 'Launcher/Utils.dll' in target folder!", ConsoleColor.Red);
    return;
}

var backupAssembly = Common.MoveFileToBackup(targetAssembly);
Common.WriteLine("Applying workshop mods fix...");
try
{
    var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory + '/');
    var utilsPatchPath = baseDirectory + "/DayZLauncher.UnixPatcher.Utils.dll";

    using (var patchedUtils = UtilsAssemblyPatcher.PatchAssembly(backupAssembly, utilsPatchPath))
    {
        patchedUtils.Write(targetAssembly);
        Common.WriteLine("Utils.dll patched!", ConsoleColor.Green);
    }

    File.Copy(utilsPatchPath, userInput + "/Launcher/DayZLauncher.UnixPatcher.Utils.dll", true);
    Common.WriteLine("UnixPatcher.Utils.dll deployed!", ConsoleColor.Green);
}
catch (Exception e)
{
    Common.WriteLine(e.Message);
    Common.WriteLine("Failed to write files into DayZ directory!", ConsoleColor.Red);
    Common.WriteLine("Workshop mods will not work!", ConsoleColor.Red);
    Common.WriteLine("Reverting changes...", ConsoleColor.Yellow);

    File.Delete(targetAssembly);
    File.Move(backupAssembly, targetAssembly);
}

Common.WriteLine("Checking unix path", ConsoleColor.Green);
// Check absolute unix path and write instructions to registry
// This probably won't work as run from unix..........
// var localInstall = "Z:\home\deck\.steam\steam\steamapps\common\DayZ"
// var externalType1 = "E:\steamapps\common\DayZ"
// var externalType2 = "E:\SteamLibrary\steamapps\common\DayZ"
// This might?
var localInstall = "/home/deck/.steam/steam/steamapps/common/DayZ";
var externalType1 = "/run/media/mmcblk0p1/steamapps/common/DayZ";
var externalType2 = "/run/media/mmcblk0p1/SteamLibrary/steamapps/common/DayZ";
if (!Directory.Exists(localInstall))
{
    Environment.SetEnvironmentVariable("DZPatch_WinDrive", "Z:", EnvironmentVariableTarget.Machine);
    Environment.SetEnvironmentVariable("DZPatch_UnixPath", string.Empty, EnvironmentVariableTarget.Machine);
    Common.Writeline($"Found DayZ directory in '{localInstall}'", ConsoleColor.Green);
}
else if (!Directory.Exists(externalType1))
{
    Environment.SetEnvironmentVariable("DZPatch_WinDrive", "E:", EnvironmentVariableTarget.Machine);
    Environment.SetEnvironmentVariable("DZPatch_UnixPath", "/run/media/mmcblk0p1", EnvironmentVariableTarget.Machine);
    Common.Writeline($"Found DayZ directory in '{externalType1}'", ConsoleColor.Green);
}
else if (!Directory.Exists(externalType2))
{
    Environment.SetEnvironmentVariable("DZPatch_WinDrive", "E:", EnvironmentVariableTarget.Machine);
    Environment.SetEnvironmentVariable("DZPatch_UnixPath", "/run/media/mmcblk0p1/SteamLibrary", EnvironmentVariableTarget.Machine);
    Common.Writeline($"Found DayZ directory in '{externalType2}'", ConsoleColor.Green);
}
else {
    Environment.SetEnvironmentVariable("DZPatch_WinDrive", "Z:", EnvironmentVariableTarget.Machine);
    Environment.SetEnvironmentVariable("DZPatch_UnixPath", string.Empty, EnvironmentVariableTarget.Machine);
    Common.Writeline($"Unable to find unix path! Setting to steam default ('{localInstall}')", ConsoleColor.Red);
    Common.Writeline("Patch may fail!", ConsoleColor.Red);
}

Common.WriteLine("Applying launcher settings fix...");
try
{
    LauncherConfigPatcher.ApplySettingsBandAid(userInput);
    await LauncherConfigPatcher.PatchLauncherConfigFile(userInput);
    LauncherConfigPatcher.RemoveOldUserConfig(userInput);
    Common.WriteLine("Settings fix applied!", ConsoleColor.Green);
}
catch
{
    Common.WriteLine("Failed to patch launcher settings", ConsoleColor.Red);
    Common.WriteLine("Launcher may continue to not save properly", ConsoleColor.Yellow);
}

Common.WriteLine("Patching finished!");