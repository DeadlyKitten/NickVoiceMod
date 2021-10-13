using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using VoiceMod.Managers;
using BepInEx.Configuration;
using System.IO;

namespace VoiceMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static ConfigEntry<bool> PreloadAllClips;

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;

            var config = new ConfigFile(Path.Combine(Paths.ConfigPath, "VoiceMod.cfg"), true);
            PreloadAllClips = config.Bind<bool>("Settings", "Preload Clips on Game Start", false, "If you're having issues with lag, turn this setting on to prevent loading clip files during gameplay.");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            new VoicepackManager();
        }

        #region logging
        internal static void LogDebug(string message) => Instance.Log(message, LogLevel.Debug);
        internal static void LogInfo(string message) => Instance.Log(message, LogLevel.Info);
        internal static void LogWarning(string message) => Instance.Log(message, LogLevel.Warning);
        internal static void LogError(string message) => Instance.Log(message, LogLevel.Error);
        private void Log(string message, LogLevel logLevel) => Logger.Log(logLevel, message);
        #endregion
    }
}
