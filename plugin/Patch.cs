using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using OC2TAS.Extension;
using System.Xml.Linq;

namespace OC2TAS
{
    public static class ClientTime2TimePatch
    {
        private static readonly MethodInfo methodInfoTime = AccessTools.PropertyGetter(typeof(Time), "time");
        private static readonly MethodInfo methodInfoDeltaTime = AccessTools.PropertyGetter(typeof(Time), "deltaTime");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TimeManager), "Update")]
        public static bool TimeManagerUpdatePatch() => false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AudioManager), "StartAudio", new Type[] {
            typeof(AudioSource), typeof(AudioManager.AudioGroup), typeof(AudioClip), typeof(ClipParameters), typeof(bool), typeof(int)
        })]
        public static void AudioManagerStartAudioPatch(ClipParameters _parameters, AudioSource __result)
        {
            float randomPitchVariance = _parameters.RandomPitchVariance;
            float num = _parameters.Pitch + UnityEngine.Random.Range(-randomPitchVariance, randomPitchVariance);
            __result.pitch = num;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioManager), "Update")]
        public static bool AudioManagerUpdatePatch() => !AudioListener.pause;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClientWorkstation), "StartSynchronising")]
        public static void ClientWorkstationStartSynchronisingPatch(ClientWorkstation __instance)
        {
            ParticleSystem m_chopPFXInstance = __instance.get_m_chopPFXInstance();
            if (m_chopPFXInstance != null)
            {
                ParticleSystem.MainModule mainModule = m_chopPFXInstance.main;
                mainModule.useUnscaledTime = false;
                ParticleSystem[] componentsInChildren = m_chopPFXInstance.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    mainModule = componentsInChildren[i].main;
                    mainModule.useUnscaledTime = false;
                }
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ConveyorPrediction), "Update")]
        public static IEnumerable<CodeInstruction> ConveyorPredictionUpdatePatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[15] = new CodeInstruction(OpCodes.Call, methodInfoDeltaTime);
            codes[41] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ServerConveyorStation), "RefreshStartConveying")]
        public static IEnumerable<CodeInstruction> ServerConveyorStationRefreshStartConveyingPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[36] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MaterialTimeController), "Update")]
        public static IEnumerable<CodeInstruction> MaterialTimeControllerUpdatePatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[6] = new CodeInstruction(OpCodes.Call, methodInfoDeltaTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ClientCookingRegion), "OnEnableChanged")]
        public static IEnumerable<CodeInstruction> ClientCookingRegionOnEnableChangedPatch(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = instructions.ToList();
            Label label1 = il.DefineLabel();
            Label label2 = il.DefineLabel();
            codes[24] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            codes[24].labels.Add(label1);
            codes[15] = new CodeInstruction(OpCodes.Bne_Un, label1);
            codes[37] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            codes[53] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            codes[53].labels.Add(label2);
            codes[48] = new CodeInstruction(OpCodes.Brtrue, label2);
            codes[66] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ServerLimitedQuantityItem), "GetDestructionScore")]
        public static IEnumerable<CodeInstruction> ServerLimitedQuantityItemGetDestructionScorePatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[0] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ServerLimitedQuantityItem), "Touch")]
        public static IEnumerable<CodeInstruction> ServerLimitedQuantityItemTouchPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[1] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ServerTimedQueue), "AdvanceQueue")]
        public static IEnumerable<CodeInstruction> ServerTimedQueueAdvanceQueuePatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[23] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            codes[68] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ServerTimedQueue), "OnTrigger")]
        public static IEnumerable<CodeInstruction> ServerTimedQueueOnTriggerPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[19] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ServerTimedQueue), "UpdateSynchronising")]
        public static IEnumerable<CodeInstruction> ServerTimedQueueUpdateSynchronisingPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[30] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ClientTimedQueue), "UpdateSynchronising")]
        public static IEnumerable<CodeInstruction> ClientTimedQueueUpdateSynchronisingPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[0] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            codes[79] = new CodeInstruction(OpCodes.Call, methodInfoDeltaTime);
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ClientPlayerControlsImpl_Default), "Update_Carry")]
        public static IEnumerable<CodeInstruction> ClientPlayerControlsImpl_DefaultUpdate_CarryPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            codes[30] = new CodeInstruction(OpCodes.Call, methodInfoTime);
            return codes.AsEnumerable();
        }
    }

    public static class UIPatch
    { 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ClientHeatedStationGUI), "StartSynchronising")]
        public static void ClientHeatedStationGUIStartSynchronisingPatch(Component synchronisedObject)
        {
            HeatedStationGUI heatedStationGUI = synchronisedObject as HeatedStationGUI;
            heatedStationGUI.m_Offset = heatedStationGUI.m_Offset.AddY(2f);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPlayerRootMenu), "Start")]
        public static void UIPlayerRootMenuStartPatch()
        {
            if (TASPlugin.tasControl.replayState == TASControl.ReplayState.InLevel)
                TASPlugin.tasControl.BindUIEmoteAtNextFrame();
        }
    }

    public static class RNGPatch
    {
        public class FixedMenuRoundData : RoundData
        {
            public static RecipeList.Entry[] GetNextRecipeFixed(RoundData roundData, RoundInstanceDataBase _data)
            {
                RoundData.RoundInstanceData instance = _data as RoundData.RoundInstanceData;
                if (roundData is ScriptedRoundData scriptedRoundData)
                    if (instance.RecipeCount < scriptedRoundData.m_manualOrder.Length)
                        return null;
                int menuCount = instance.CumulativeFrequencies.Collapse((int f, int total) => total + f);
                if (TASPlugin.tasControl.script.menu[0].Length <= menuCount)
                    return null;
                int menuIndex = TASPlugin.tasControl.script.menu[0][menuCount];
                instance.RecipeCount++;
                instance.CumulativeFrequencies[menuIndex]++;
                return new RecipeList.Entry[] { roundData.m_recipes.m_recipes[menuIndex] };
            }
        }

        public class FixedMenuDynamicRoundData : DynamicRoundData
        {
            public static RecipeList.Entry[] GetNextRecipeFixed(DynamicRoundData roundData, RoundInstanceDataBase _data)
            {
                DynamicRoundData.DynamicRoundInstanceData instance = _data as DynamicRoundData.DynamicRoundInstanceData;
                if (TASPlugin.tasControl.script.menu.Length <= instance.CurrentPhase)
                    return null;
                int menuCount = instance.CumulativeFrequencies.Collapse((int f, int total) => total + f);
                if (TASPlugin.tasControl.script.menu[instance.CurrentPhase].Length <= menuCount)
                    return null;
                int menuIndex = TASPlugin.tasControl.script.menu[instance.CurrentPhase][menuCount];
                DynamicRoundData.Phase phase = roundData.Phases[instance.CurrentPhase];
                instance.RecipeCount++;
                instance.CumulativeFrequencies[menuIndex]++;
                return new RecipeList.Entry[] { phase.Recipes.m_recipes[menuIndex] };
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundData), "GetNextRecipe")]
        public static bool RoundDataGetNextRecipePatch(RoundData __instance, ref RecipeList.Entry[] __result, RoundInstanceDataBase _data)
        {
            if (TASPlugin.tasControl.replayState != TASControl.ReplayState.Init)
            {
                __result = FixedMenuRoundData.GetNextRecipeFixed(__instance, _data);
                return __result == null;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DynamicRoundData), "GetNextRecipe")]
        public static bool DynamicRoundDataGetNextRecipePatch(DynamicRoundData __instance, ref RecipeList.Entry[] __result, RoundInstanceDataBase _data)
        {
            if (TASPlugin.tasControl.replayState != TASControl.ReplayState.Init)
            {
                __result = FixedMenuDynamicRoundData.GetNextRecipeFixed(__instance, _data);
                return __result == null;
            }
            return true;
        }
    }
}
