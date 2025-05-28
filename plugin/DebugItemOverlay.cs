/*
 * https://github.com/hpmv/overcooked-supercharged/blob/master/patch/DebugItemOverlay.cs
 */

using HarmonyLib;
using OC2TAS.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Team17.Online.Multiplayer.Messaging;
using UnityEngine;

namespace OC2TAS
{
    public static class DebugItemOverlay
    {
        private static List<DebugItemOverlayProvider> providers = new List<DebugItemOverlayProvider>();
        private static GUIStyle style;
        private static KeyCode debugKey = KeyCode.F8;
        public static DebugType debugType = DebugType.None;
        
        public enum DebugType
        {
            None,
            Basic,
            All,
            Player,
            Item,
            Progress,
            Count
        }

        public static void LateUpdate()
        {
            if (Input.GetKeyDown(debugKey))
            {
                debugType++;
                if (debugType == DebugType.Count) debugType = DebugType.None;
            }
        }

        static FieldInfo fieldInfo_m_LeftOverTime = AccessTools.Field(typeof(ClientPlayerControlsImpl_Default), "m_LeftOverTime");
        static FieldInfo fieldInfo_m_lastPickupTimestamp = AccessTools.Field(typeof(ClientPlayerControlsImpl_Default), "m_lastPickupTimestamp");
        static FieldInfo fieldInfo_m_dashTimer= AccessTools.Field(typeof(ClientPlayerControlsImpl_Default), "m_dashTimer");

        public static void Awake()
        {
            //AddProviderFor<ClientPlayerControlsImpl_Default>("time", DebugType.Player, c =>
            //{
            //    return string.Format("{0:F4}", (float)fieldInfo_m_LeftOverTime.GetValue(c));
            //});m_dashTimer
            AddProviderFor<PlayerControls>("pos", DebugType.Player, c =>
            {
                Vector3 position = c.transform.position;
                return string.Format("\n{0:F4}\n{1:F4}\n{2:F4}", position.x, position.y, position.z);
            });
            AddProviderFor<ClientPlayerControlsImpl_Default>("cd", DebugType.Player, c =>
            {
                float t_pickup = (float)fieldInfo_m_lastPickupTimestamp.GetValue(c) - Time.time;
                float t_dash = (float)fieldInfo_m_dashTimer.GetValue(c) + 0.1f - 0.02f;
                return string.Format("p{0:.00},d{1:.00}", Mathf.Max(t_pickup, 0f), Mathf.Max(t_dash, 0f));
            });
            AddProviderFor<PlayerControls>("name", DebugType.Player, c =>
            {
                return c.name;
            });
            AddProviderFor<ServerPhysicalAttachment>("pos", DebugType.Item, c =>
            {
                Vector3 position = c.transform.position;
                return string.Format("\n{0:F4}\n{1:F4}\n{2:F4}", position.x, position.y, position.z);
            });
            AddProviderFor<ServerPhysicalAttachment>("name", DebugType.Item, c =>
            {
                return c.AccessRigidbody()?.name ?? c.name;
            });
            AddProviderFor<ServerCookingHandler>("progress", DebugType.Progress, c =>
            {
                return string.Format("{0:F2}", (c.GetServerUpdate() as CookingStateMessage).m_cookingProgress);
            });
            AddProviderFor<ServerMixingHandler>("progress", DebugType.Progress, c =>
            {
                return string.Format("{0:F2}", (c.GetServerUpdate() as MixingStateMessage).m_mixingProgress);
            });
            AddProviderFor<ServerPlateReturnStation>("progress", DebugType.Progress, c =>
            {
                var controller = GameUtils.GetFlowController() as ServerKitchenFlowControllerBase;
                if (controller == null) return "";
                List<float> timers = controller.get_m_plateReturnController().get_m_timers(c);
                return string.Join(" ", timers.Select(x => x.ToString("F2")).ToArray());
            });
            AddProviderFor<ServerWashingStation>("progress", DebugType.Progress, c =>
            {
                return string.Format("{0:F2}", c.get_m_cleaningTimer());

            });
            AddProviderFor<ServerHeatedStation>("heat", DebugType.Progress, c =>
            {
                return c.isActiveAndEnabled ? string.Format("{0:F3}", c.get_m_heatValue()) : null;
            });
            // AddProviderFor<PlayerControls>("grav", c => c.m_bApplyGravity ? "ON" : "OFF");
            // AddProviderFor<Rigidbody>("kin", c => c.GetComponent<Rigidbody>().isKinematic ? "ON" : "OFF");
            // AddProviderFor<GroundCast>("gnorm", c =>
            // {
            //     return string.Format("{0:0.###}", c.GetGroundNormal());
            // });
            // AddProviderFor<GroundCast>("gmask", c =>
            // {
            //     return "" + (int)c.GetGroundLayer();
            // });
            // AddProviderFor<PlayerControls>("gstr", c => "" + c.Movement.GravityStrength);
            // AddProviderFor<PlayerControls>("scale", c =>
            // {
            //     return string.Format("{0:0.###}", c.transform.localScale);
            // });
            // AddProviderFor<PlayerControls>("parent", c =>
            // {
            //     return c.transform.parent?.name ?? "null";
            // });
            // AddProviderFor<CannonCosmeticDecisions>("transDur", c =>
            // {
            //     var info = c.m_cannonAnimator.GetAnimatorTransitionInfo(0);
            //     return "" + info.duration;
            // });
            // AddProviderFor<CannonCosmeticDecisions>("transTime", c =>
            // {
            //     var info = c.m_cannonAnimator.GetAnimatorTransitionInfo(0);
            //     return "" + info.normalizedTime;
            // });
            // AddProviderFor<CannonCosmeticDecisions>("transName", c =>
            // {
            //     var info = c.m_cannonAnimator.GetAnimatorTransitionInfo(0);
            //     return "" + info.nameHash;
            // });
            // AddProviderFor<CannonCosmeticDecisions>("stateTime", c =>
            // {
            //     var info = c.m_cannonAnimator.GetCurrentAnimatorStateInfo(0);
            //     return "" + info.normalizedTime;
            // });
            // AddProviderFor<CannonCosmeticDecisions>("stateName", c =>
            // {
            //     var info = c.m_cannonAnimator.GetCurrentAnimatorStateInfo(0);
            //     return "" + info.nameHash;
            // });
            // AddProviderFor<CannonCosmeticDecisions>("nextStateTime", c =>
            // {
            //     var info = c.m_cannonAnimator.GetNextAnimatorStateInfo(0);
            //     return "" + info.normalizedTime;
            // });
            // AddProviderFor<CannonCosmeticDecisions>("nextStateName", c =>
            // {
            //     var info = c.m_cannonAnimator.GetNextAnimatorStateInfo(0);
            //     return "" + info.nameHash;
            // });
            // AddProviderFor<ServerPilotRotation>("spr.startAngle", c =>
            // {
            //     return "" + c.m_startAngle();
            // });
            // AddProviderFor<ServerPilotRotation>("spr.angle", c =>
            // {
            //     return "" + c.m_angle();
            // });
            // AddProviderFor<PilotRotation>("pr.euler.y", c =>
            // {
            //     return "" + c.transform.eulerAngles.y;
            // });
            // AddProviderFor<PlayerControls>("buttons", c =>
            // {
            //     var controlScheme = c.ControlScheme;
            //     if (controlScheme == null)
            //     {
            //         return "?";
            //     }
            //     var str = "";
            //     if (controlScheme.m_pickupButton is TASLogicalButton)
            //     {
            //         if (controlScheme.m_pickupButton.JustPressed())
            //         {
            //             str += "P";
            //         } else if (controlScheme.m_pickupButton.JustReleased())
            //         {
            //             str += "-";
            //         } else
            //         {
            //             str += " ";
            //         }
            //         if (controlScheme.m_pickupButton.IsDown())
            //         {
            //             str += "P ";
            //         } else
            //         {
            //             str += "  ";
            //         }
            //     } else
            //     {
            //         str += "?? ";
            //     }
            //     if (controlScheme.m_worksurfaceUseButton is TASLogicalButton)
            //     {
            //         if (controlScheme.m_worksurfaceUseButton.JustPressed())
            //         {
            //             str += "U";
            //         }
            //         else if (controlScheme.m_worksurfaceUseButton.JustReleased())
            //         {
            //             str += "-";
            //         }
            //         else
            //         {
            //             str += " ";
            //         }
            //         if (controlScheme.m_worksurfaceUseButton.IsDown())
            //         {
            //             str += "U ";
            //         }
            //         else
            //         {
            //             str += "  ";
            //         }
            //     }
            //     else
            //     {
            //         str += "?? ";
            //     }
            //     if (controlScheme.m_dashButton is TASLogicalButton)
            //     {
            //         if (controlScheme.m_dashButton.JustPressed())
            //         {
            //             str += "D";
            //         }
            //         else if (controlScheme.m_dashButton.JustReleased())
            //         {
            //             str += "-";
            //         }
            //         else
            //         {
            //             str += " ";
            //         }
            //         if (controlScheme.m_dashButton.IsDown())
            //         {
            //             str += "D";
            //         }
            //         else
            //         {
            //             str += " ";
            //         }
            //     }
            //     else
            //     {
            //         str += "??";
            //     }
            //     return str;
            // });
            // AddProviderFor<PlayerControls>("axes", c =>
            // {
            //     var controlScheme = c.ControlScheme;
            //     if (controlScheme == null)
            //     {
            //         return "?";
            //     }
            //     var str = "";
            //     if (controlScheme.m_moveX is TASLogicalValue)
            //     {
            //         str += "X:" + string.Format("{0:0.###}", controlScheme.m_moveX.GetValue()) + " ";
            //     }
            //     else
            //     {
            //         str += "X:? ";
            //     }
            //     if (controlScheme.m_moveY is TASLogicalValue)
            //     {
            //         str += "Y:" + string.Format("{0:0.###}", controlScheme.m_moveY.GetValue());
            //     }
            //     else
            //     {
            //         str += "Y:?";
            //     }
            //     return str;
            // });
            // AddProviderFor<PlayerControls>("canpress", c =>
            // {
            //     return c.CanButtonBePressed() ? "YES" : "NO";
            // });
            // AddProviderFor<ServerPlayerAttachmentCarrier>("carry", c =>
            // {
            //     return c.InspectCarriedItem()?.name ?? "";
            // });
            // AddProviderFor<PlayerControls>("pickup", c =>
            // {
            //     var handlePickup = c.CurrentInteractionObjects.m_iHandlePickup;
            //     if (handlePickup == null)
            //     {
            //         return "";
            //     }
            //     var canHandlePickup = " (NO)";
            //     if (handlePickup.CanHandlePickup(c.GetComponent<ICarrier>()))
            //     {
            //         canHandlePickup = " (YES)";
            //     }
            //     var pickup = c.CurrentInteractionObjects.m_TheOriginalHandlePickup;
            //     if (pickup == null)
            //     {
            //         return "";
            //     }
            //     var entityId = EntitySerialisationRegistry.GetEntry(pickup)?.m_Header?.m_uEntityID ?? 0;
            //     return pickup.name + " (" + entityId + ")" + canHandlePickup;
            // });
            // AddProviderFor<PlayerControls>("interact", c =>
            // {
            //     var obj = c.CurrentInteractionObjects.m_interactable;
            //     if (obj == null)
            //     {
            //         return "";
            //     }
            //     var entityId = EntitySerialisationRegistry.GetEntry(obj.gameObject)?.m_Header?.m_uEntityID ?? 0;
            //     return obj.name + " (" + entityId + ")";
            // });
            // AddProviderFor<ServerPhysicalAttachment>("attach.free", c =>
            // {
            //     return "" + (c.AccessRigidbody().transform == c.AccessGameObject().transform.parent);
            // });
            // AddProviderFor<ServerPhysicalAttachment>("attach.container.loc", c =>
            // {
            //     return "" + string.Format("{0:0.###}", c.AccessRigidbody()?.transform?.position ?? Vector3.zero);
            // });
            // AddProviderFor<ServerPhysicalAttachment>("attach.loc", c =>
            // {
            //     return "" + string.Format("{0:0.###}", c.transform.position);
            // });
            // AddProviderFor<ServerPhysicalAttachment>("attach.ancestry", c =>
            // {
            //     List<string> ancestry = new List<string>();
            //     for (var parent = c.AccessGameObject().transform.parent; parent != null; parent = parent.parent)
            //     {
            //         ancestry.Add(parent.name);
            //     }
            //     return string.Join(" -> ", ancestry.ToArray());
            // });
            // AddProviderFor<ServerThrowableItem>("flying", c =>
            // {
            //     return "" + c.IsFlying();
            // });
            // AddProviderFor<ServerIngredientContainer>("isc", c =>
            // {
            //     return "" + c.m_contents().Count;
            // });
            // AddProviderFor<ClientIngredientContainer>("icc", c =>
            // {
            //     return "" + c.m_contents().Count;
            // });
            // AddProviderFor<PhysicalAttachment>("id", c =>
            // {
            //     return "" + EntitySerialisationRegistry.GetEntry(c.gameObject).m_Header.m_uEntityID;
            // });
            // AddProviderFor<ServerPickupItemSwitcher>("pickup#", c =>
            // {
            //     return "" + c.GetCurrentItemPrefabIndex();
            // });
            // AddProviderFor<ServerPlacementItemSwitcher>("place#", c =>
            // {
            //     return "" + c.GetCurrentItemPrefabIndex();
            // });
            // AddProviderFor<ClientKitchenFlowControllerBase>("recipe", c =>
            // {
            //     var s = "";
            //     var gui = c.GetMonitorForTeam(TeamID.One).OrdersController.GetGUI();
            //     foreach (var widget in gui.GetActiveWidgets())
            //     {
            //         var animator = widget.m_widget.GetAnimator();
            //         var info = animator.GetCurrentAnimatorStateInfo(1);
            //         s += $"{info.nameHash}/{info.normalizedTime} ";
            //     }
            //     return s;
            // });
        }

        public static void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle();
                style.normal.textColor = Color.black;
                style.normal.background = Texture2D.whiteTexture;
                style.fontSize = 10;
                style.font = Font.CreateDynamicFontFromOSFont("Courier New", 10);
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.UpperLeft;
                style.padding = new RectOffset(4, 4, 1, 1);
            }

            if (debugType == DebugType.None || debugType == DebugType.Basic ||
                EntitySerialisationRegistry.m_EntitiesList?._items == null)
            {
                return;
            }

            EntitySerialisationRegistry.m_EntitiesList.ForEach(entry => LabelGameObject(entry.m_GameObject));
            
            //int activeCount = GridManager.GetActiveCount();
            //for (int i = 0; i < activeCount; i++)
            //{
            //    GridManager active = GridManager.GetActive(i);
            //    if (active is QuadGridManager)
            //        active.get_m_gridOccupancy().Values.ToList().ForEach(go => LabelGameObject(go));
            //}
        }

        private static void LabelGameObject(GameObject gameObject)
        {
            var screenPos = Camera.main.WorldToScreenPoint(gameObject.transform.position).XY();
            var curPos = screenPos + new Vector2(-50, 50);
            foreach (var provider in providers)
            {
                if (provider.type != debugType && debugType != DebugType.All)
                {
                    continue;
                }
                var data = provider.provider(gameObject);
                if (data == null)
                {
                    continue;
                }
                var guiContent = new GUIContent(provider.name + ": " + data);
                var labelSize = style.CalcSize(guiContent);
                GUI.Label(new Rect(curPos.x, Screen.height - curPos.y - labelSize.y, labelSize.x, labelSize.y), guiContent, style);
                curPos.y += labelSize.y;
            }
        }

        public static void Destroy()
        {
            providers.Clear();
            style = null;
        }

        public static void AddProvider(string name, DebugType type, DebugItemOverlayDataProvider provider)
        {
            providers.Add(new DebugItemOverlayProvider()
            {
                name = name,
                provider = provider,
                type = type
            });
        }

        public static void AddProviderFor<T>(string name, DebugType type, DebugItemOverlayDataProviderFor<T> provider)
            where T : Component
        {
            var wrapped = new DebugItemOverlayDataProvider((o) =>
            {
                var component = o.GetComponent<T>();
                if (component == null)
                {
                    return null;
                }
                return provider(component);
            });
            AddProvider(name, type, wrapped);
        }
    }

    public delegate string DebugItemOverlayDataProvider(GameObject item);
    public delegate string DebugItemOverlayDataProviderFor<T>(T component) where T : Component;

    class DebugItemOverlayProvider
    {
        public DebugItemOverlayDataProvider provider;
        public string name;
        public DebugItemOverlay.DebugType type;
    }
}