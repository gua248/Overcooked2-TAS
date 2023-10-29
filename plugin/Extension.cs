using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OC2TAS.Extension
{
    public static class PlayerControlsExtension
    {
        private static readonly MethodInfo methodInfoPlayerControlsUpdate = AccessTools.Method(typeof(PlayerControls), "Update");
        public static void Update(this PlayerControls instance)
        {
            methodInfoPlayerControlsUpdate.Invoke(instance, null);
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
}
