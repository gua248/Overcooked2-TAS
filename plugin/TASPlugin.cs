using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OC2TAS
{
    [BepInPlugin("dev.gua.overcooked.tas", "Overcooked2 TAS Plugin", "1.0")]
    [BepInProcess("Overcooked2.exe")]
    public class TASPlugin : BaseUnityPlugin
    {
        public static TASPlugin pluginInstance;
        public static TASControl tasControl;
        private static Harmony patcher;
        public static AudioRecorder audioRecorder;

        public void Awake()
        {
            pluginInstance = this;
            patcher = new Harmony("dev.gua.overcooked.tas");
            patcher.PatchAll(typeof(ClientTime2TimePatch));
            patcher.PatchAll(typeof(UIPatch));
            patcher.PatchAll(typeof(MenuPatch));
            foreach (var patched in Harmony.GetAllPatchedMethods())
                Console.WriteLine("Patched: " + patched.FullDescription());

            tasControl = new TASControl();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void LateUpdate()
        {
            tasControl.LateUpdate();
        }

        public void OnGUI()
        {
            tasControl.OnGUI();
        }
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            tasControl.OnSceneLoaded();
            AudioListener audioListener = GameObject.FindObjectOfType<AudioListener>();
            if (audioListener != null)
                if (audioListener.gameObject.GetComponent<AudioRecorder>() == null)
                    audioRecorder = audioListener.gameObject.AddComponent<AudioRecorder>();
        }

        public void OnDestroy()
        {
            tasControl.OnDestroy();
        }

        public void Log(String msg) { this.Logger.LogInfo(msg); }
    }
}
