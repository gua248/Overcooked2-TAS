using BepInEx;
using HarmonyLib;
using System;
using UnityEngine.SceneManagement;

namespace OC2TAS
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("Overcooked2.exe")]
    public class TASPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "dev.gua.overcooked.tas";
        public const string PLUGIN_NAME = "Overcooked2 TAS Plugin";
        public const string PLUGIN_VERSION = "1.1";
        public static TASPlugin pluginInstance;
        public static TASControl tasControl;
        private static Harmony patcher;

        public void Awake()
        {
            pluginInstance = this;
            patcher = new Harmony("dev.gua.overcooked.tas");
            patcher.PatchAll(typeof(ClientTime2TimePatch));
            patcher.PatchAll(typeof(UIPatch));
            patcher.PatchAll(typeof(RNGPatch));
            foreach (var patched in Harmony.GetAllPatchedMethods())
                Console.WriteLine("Patched: " + patched.FullDescription());
            SceneManager.sceneLoaded += OnSceneLoaded;

            tasControl = new TASControl();
            DebugItemOverlay.Awake();
        }

        public void LateUpdate()
        {
            tasControl.LateUpdate();
            DebugItemOverlay.LateUpdate();
        }

        public void OnGUI()
        {
            tasControl.OnGUI();
            DebugItemOverlay.OnGUI();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            tasControl.OnSceneLoaded();
        }

        public void OnDestroy()
        {
            tasControl.OnDestroy();
            DebugItemOverlay.Destroy();
        }

        public static void Log(string msg) { pluginInstance.Logger.LogInfo(msg); }
    }
}
