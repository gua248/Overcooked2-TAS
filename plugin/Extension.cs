using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OC2TAS.Extension
{
    public static class GridManagerExtension
    {
        private static readonly FieldInfo fieldInfo_m_gridOccupancy = AccessTools.Field(typeof(GridManager), "m_gridOccupancy");

        public static Dictionary<GridIndex, GameObject> get_m_gridOccupancy(this GridManager instance)
        {
            return (Dictionary<GridIndex, GameObject>)fieldInfo_m_gridOccupancy.GetValue(instance);
        }
    }

    public static class ServerHeatedStationExtension
    {
        private static readonly FieldInfo fieldInfo_m_heatValue = AccessTools.Field(typeof(ServerHeatedStation), "m_heatValue");

        public static float get_m_heatValue(this ServerHeatedStation instance)
        {
            return (float)fieldInfo_m_heatValue.GetValue(instance);
        }
    }

    public static class ServerWashingStationExtension
    {
        private static readonly FieldInfo fieldInfo_m_cleaningTimer = AccessTools.Field(typeof(ServerWashingStation), "m_cleaningTimer");

        public static float get_m_cleaningTimer(this ServerWashingStation instance)
        {
            return (float)fieldInfo_m_cleaningTimer.GetValue(instance);
        }
    }

    public static class PlateReturnControllerExtension
    {
        private static readonly FieldInfo fieldInfo_m_platesToReturn = AccessTools.Field(typeof(PlateReturnController), "m_platesToReturn");
        private static readonly FieldInfo fieldInfo_m_timer = AccessTools.Field(typeof(PlateReturnController).GetNestedType("PlatesPendingReturn", BindingFlags.Instance | BindingFlags.NonPublic), "m_timer");
        private static readonly FieldInfo fieldInfo_m_platingStepData = AccessTools.Field(typeof(PlateReturnController).GetNestedType("PlatesPendingReturn", BindingFlags.Instance | BindingFlags.NonPublic), "m_platingStepData");
        private static readonly MethodInfo methodInfo_FindBestReturnStation = AccessTools.Method(typeof(PlateReturnController), "FindBestReturnStation");
        
        public static List<float> get_m_timers(this PlateReturnController instance, ServerPlateReturnStation plateReturnStation)
        {
            var m_platesToReturn = fieldInfo_m_platesToReturn.GetValue(instance);
            object[] plates = m_platesToReturn.GetType().GetField("_items").GetValue(m_platesToReturn) as object[];
            var m_timers = new List<float>();
            foreach (object plate in plates)
                if (plate != null)
                {
                    var plateType = fieldInfo_m_platingStepData.GetValue(plate);
                    ServerPlateReturnStation station = (ServerPlateReturnStation)methodInfo_FindBestReturnStation.Invoke(instance, new object[] { plateType });
                    if (station == plateReturnStation)
                        m_timers.Add((float)fieldInfo_m_timer.GetValue(plate));
                }
            return m_timers;
        }
    }

    public static class ServerKitchenFlowControllerBaseExtension
    {
        private static readonly FieldInfo fieldInfo_m_plateReturnController = AccessTools.Field(typeof(ServerKitchenFlowControllerBase), "m_plateReturnController");

        public static PlateReturnController get_m_plateReturnController(this ServerKitchenFlowControllerBase instance)
        {
            return (PlateReturnController)fieldInfo_m_plateReturnController.GetValue(instance);
        }
    }

    public static class PlayerControlsExtension
    {
        private static readonly MethodInfo methodInfoPlayerControlsUpdate = AccessTools.Method(typeof(PlayerControls), "Update");

        public static void Update(this PlayerControls instance)
        {
            methodInfoPlayerControlsUpdate.Invoke(instance, null);
        }
    }

    public static class PlayerSwitchingManagerExtension
    {
        private static readonly MethodInfo methodInfoPlayerSwitchingManagerUpdate = AccessTools.Method(typeof(PlayerSwitchingManager), "Update");
        private static readonly FieldInfo fieldInfo_m_avatarSets = AccessTools.Field(typeof(PlayerSwitchingManager), "m_avatarSets");
        private static readonly FieldInfo fieldInfo_SwitchButtons = AccessTools.Field(AccessTools.TypeByName("PlayerSwitchingManager+AvatarSet"), "SwitchButtons");

        public static void Update(this PlayerSwitchingManager instance)
        {
            methodInfoPlayerSwitchingManagerUpdate.Invoke(instance, null);
        }

        public static ILogicalButton[] get_SwitchButton(this PlayerSwitchingManager instance)
        {
            var avatarSets = (IDictionary)fieldInfo_m_avatarSets.GetValue(instance);
            return (ILogicalButton[])fieldInfo_SwitchButtons.GetValue(avatarSets[(PlayerInputLookup.Player)0]);
        }
    }

    public static class ClientWorkstationExtension
    {
        private static readonly FieldInfo fieldInfo_m_chopPFXInstance = AccessTools.Field(typeof(ClientWorkstation), "m_chopPFXInstance");

        public static ParticleSystem get_m_chopPFXInstance(this ClientWorkstation instance)
        {
            return (ParticleSystem)fieldInfo_m_chopPFXInstance.GetValue(instance);
        }
    }

    public static class ClientEmoteWheelExtension
    {
        private static readonly FieldInfo fieldInfo_m_wheelButton = AccessTools.Field(typeof(ClientEmoteWheel), "m_wheelButton");
        private static readonly FieldInfo fieldInfo_m_renableInputButton = AccessTools.Field(typeof(ClientEmoteWheel), "m_renableInputButton");
        private static readonly FieldInfo fieldInfo_m_xMovement = AccessTools.Field(typeof(ClientEmoteWheel), "m_xMovement");
        private static readonly FieldInfo fieldInfo_m_yMovement = AccessTools.Field(typeof(ClientEmoteWheel), "m_yMovement");
        private static readonly MethodInfo methodInfoUpdate = AccessTools.Method(typeof(ClientEmoteWheel), "Update");
        
        public static ILogicalButton get_m_wheelButton(this ClientEmoteWheel instance)
        {
            return (ILogicalButton)fieldInfo_m_wheelButton.GetValue(instance);
        }
        
        public static void set_m_wheelButton(this ClientEmoteWheel instance, ILogicalButton button)
        {
            fieldInfo_m_wheelButton.SetValue(instance, button);
        }
        
        public static ILogicalButton get_m_renableInputButton(this ClientEmoteWheel instance)
        {
            return (ILogicalButton)fieldInfo_m_renableInputButton.GetValue(instance);
        }
        
        public static void set_m_renableInputButton(this ClientEmoteWheel instance, ILogicalButton button)
        {
            fieldInfo_m_renableInputButton.SetValue(instance, button);
        }
        
        public static ILogicalValue get_m_xMovement(this ClientEmoteWheel instance)
        {
            return (ILogicalValue)fieldInfo_m_xMovement.GetValue(instance);
        }
        
        public static void set_m_xMovement(this ClientEmoteWheel instance, ILogicalValue button)
        {
            fieldInfo_m_xMovement.SetValue(instance, button);
        }
        
        public static ILogicalValue get_m_yMovement(this ClientEmoteWheel instance)
        {
            return (ILogicalValue)fieldInfo_m_yMovement.GetValue(instance);
        }
        
        public static void set_m_yMovement(this ClientEmoteWheel instance, ILogicalValue button)
        {
            fieldInfo_m_yMovement.SetValue(instance, button);
        }
        
        public static void Update(this ClientEmoteWheel instance)
        {
            methodInfoUpdate.Invoke(instance, null);
        }
    }

    public static class ServerTriggerAnimatorSetVariableExtension
    {
        private static readonly FieldInfo fieldInfo_m_data = AccessTools.Field(typeof(ServerTriggerAnimatorSetVariable), "m_data");
        private static readonly FieldInfo fieldInfo_m_triggerAnimatorVariable = AccessTools.Field(typeof(ServerTriggerAnimatorSetVariable), "m_triggerAnimatorVariable");

        public static TriggerAnimatorVariableMessage get_m_data(this ServerTriggerAnimatorSetVariable instance)
        {
            return (TriggerAnimatorVariableMessage)fieldInfo_m_data.GetValue(instance);
        }

        public static TriggerAnimatorSetVariable get_m_triggerAnimatorVariable(this ServerTriggerAnimatorSetVariable instance)
        {
            return (TriggerAnimatorSetVariable)fieldInfo_m_triggerAnimatorVariable.GetValue(instance);
        }
    }

    public static class ServerProjectileSpawnerExtension
    {
        private static readonly FieldInfo fieldInfo_m_spawner = AccessTools.Field(typeof(ServerProjectileSpawner), "m_spawner");

        public static ProjectileSpawner get_m_spawner(this ServerProjectileSpawner instance)
        {
            return (ProjectileSpawner)fieldInfo_m_spawner.GetValue(instance);
        }
    }
}
