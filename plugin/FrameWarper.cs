using HarmonyLib;
using OC2TAS.Extension;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OC2TAS
{
    public static class FrameWarper  // for snt34-4p-p4
    {
        static ServerPlayerControlsImpl_Default[] players = new ServerPlayerControlsImpl_Default[4];
        static GameObject blender1, blender2, barbecue1, barbecue2, watergun;
        static KitchenFlowControllerBase flowController;

        static void RemoveOrder(int id)
        {
            var serverFlowController = flowController.GetComponent<ServerKitchenFlowControllerBase>();
            var plateStation = GameObject.Find("workstation_plate_station").GetComponent<ServerPlateStation>();
            OrderController.OrderID orderID = new OrderController.OrderID((uint)id);
            serverFlowController.GetMonitorForTeam(TeamID.One).OrdersController.RemoveOrder(orderID);
            AccessTools.Method(typeof(ServerKitchenFlowControllerBase), "SendDeliverySuccess")
                .Invoke(serverFlowController, new object[] { TeamID.One, plateStation, orderID, 1f, 0, true });
        }

        public static bool Warp(TASControl tas)
        {
            if (tas.script.level != "Beach_Special_4P") return false;
            if (tas.replayFrameCount == 13845 * tas.replayPeriod)
            {
                if (Time.timeScale > 0f)
                {
                    Time.timeScale = 0f;
                    return true;
                }
                return false;
            }
            if (tas.replayFrameCount > 13845 * tas.replayPeriod) return false;

            if (tas.replayFrameCount == 13620 * tas.replayPeriod)
            {
                GameObject.Find("Design/Animated Objects/AnimatedLayouts/Wind_Bottom_01").SetActive(false);
                GameObject.Find("Design/Animated Objects/AnimatedLayouts/Wind_Bottom_02").SetActive(false);
                GameObject.Find("Design/Animated Objects/AnimatedLayouts/Wind_Top_01").SetActive(false);
                GameObject.Find("Design/Animated Objects/AnimatedLayouts/Wind_Top_02").SetActive(false);
            }

            if (tas.replayFrameCount >= 13620 * tas.replayPeriod && tas.replayFrameCount <= 13625 * tas.replayPeriod)
            {
                RemoveOrder(tas.replayFrameCount / tas.replayPeriod - 13619);
            }
            
            if (tas.replayFrameCount == 13630 * tas.replayPeriod)
            {
                ServerTeamMonitor serverTeamMonitor = flowController.GetComponent<ServerKitchenFlowControllerBase>().GetMonitorForTeam(TeamID.One);
                serverTeamMonitor.Score.TotalBaseScore = 7904;
                serverTeamMonitor.Score.TotalMultiplier = 4;
                serverTeamMonitor.Score.TotalTimeExpireDeductions = 0;
                ClientKitchenFlowControllerBase clientFlowController = flowController.GetComponent<ClientKitchenFlowControllerBase>();
                ClientTeamMonitor clientTeamMonitor = clientFlowController.GetMonitorForTeam(TeamID.One);
                clientTeamMonitor.Score.TotalBaseScore = 7904;
                clientTeamMonitor.Score.TotalMultiplier = 4;
                clientTeamMonitor.Score.TotalTimeExpireDeductions = 0;
                AccessTools.Method(typeof(ClientKitchenFlowControllerBase), "UpdateScoreUI").Invoke(clientFlowController, new object[] { TeamID.One });
            }

            if (tas.replayFrameCount == 0)
            {
                for (int i = 0; i < players.Length; i++)
                    players[i] = GameObject.Find($"Player {i+1}").GetComponent<ServerPlayerControlsImpl_Default>();
                blender1 = GameObject.Find("utensil_blender_01 (1)");
                blender2 = GameObject.Find("utensil_blender_01 (2)");
                barbecue1 = GameObject.Find("utensil_skewer_01 (1)");
                barbecue2 = GameObject.Find("utensil_skewer_01 (2)");
                watergun = GameObject.Find("utensil_water_gun_01");
                flowController = GameUtils.RequestManager<KitchenFlowControllerBase>();
            }

            if (tas.replayFrameCount == 5 * tas.replayPeriod)
            {
                players[0].ReceivePickUpEvent(GameObject.Find("equipment_plate_01"));
                players[1].ReceivePickUpEvent(GameObject.Find("equipment_plate_01 (2)"));
                players[2].ReceivePickUpEvent(GameObject.Find("equipment_glass_01 (1)"));
                players[3].ReceivePickUpEvent(GameObject.Find("equipment_glass_01"));
            }
            
            if (tas.replayFrameCount == 10 * tas.replayPeriod)
            {
                players[0].ReceivePlaceEvent(GameObject.Find("workstation_plate_station"));
                players[1].ReceivePlaceEvent(GameObject.Find("workstation_plate_station"));
                players[2].ReceivePlaceEvent(GameObject.Find("workstation_plate_station"));
                players[3].ReceivePlaceEvent(GameObject.Find("countertop_02"));
            }

            if (tas.replayFrameCount == 15 * tas.replayPeriod)
            {
                players[0].ReceivePickUpEvent(watergun);
                players[1].ReceivePickUpEvent(GameObject.Find("utensil_bellows_01"));
            }

            if (tas.replayFrameCount == 20 * tas.replayPeriod)
            {
                players[0].ReceiveTakeEvent();
                players[1].ReceivePlaceEvent(GameObject.Find("Design/Work Surfaces/countertop_01 (1)"));
            }

            if (tas.replayFrameCount == 25 * tas.replayPeriod)
            {
                var plateReturnController = flowController.GetComponent<ServerKitchenFlowControllerBase>().get_m_plateReturnController();
                var platesToReturn = AccessTools.Field(typeof(PlateReturnController), "m_platesToReturn").GetValue(plateReturnController);
                object[] plates = platesToReturn.GetType().GetField("_items").GetValue(platesToReturn) as object[];
                FieldInfo fieldInfo_m_timer = typeof(PlateReturnController).GetNestedType("PlatesPendingReturn", BindingFlags.Instance | BindingFlags.NonPublic).GetField("m_timer");
                fieldInfo_m_timer.SetValue(plates[0], 276.76f);
                fieldInfo_m_timer.SetValue(plates[1], 283.26f);
                fieldInfo_m_timer.SetValue(plates[2], 283.22f);
            }

            if (tas.replayFrameCount == 30 * tas.replayPeriod)
            {
                var transform = watergun.GetComponent<ServerPhysicalAttachment>().AccessRigidbody().transform;
                transform.position = new Vector3(-6.1257f, 1.0000f, 13.6184f);
                transform.rotation = Quaternion.Euler(0, 270, 0);
                players[1].ReceivePickUpEvent(barbecue1);
                players[3].ReceivePickUpEvent(blender1);
                players[0].ReceivePickUpEvent(barbecue2);
                players[2].ReceivePickUpEvent(blender2);
            }

            if (tas.replayFrameCount == 40 * tas.replayPeriod)
            {
                players[2].ReceivePlaceEvent(GameObject.Find("countertop_01"));
                players[0].ReceivePlaceEvent(GameObject.Find("Design/Work Surfaces/countertop_02 (1)"));
                string[] dispensers = new string[] { "DispenserCrate 3 (1)", "DispenserCrate 3 (4)", "DispenserCrate 3 (5)", "DispenserCrate 3 (6)", "DispenserCrate 3 (7)" };
                var ingredients = dispensers.Select(s => 
                    GameObject.Find(s)
                    .GetComponent<PickupItemSpawner>().m_itemPrefab
                    .GetComponent<WorkableItem>().m_nextPrefab
                    .GetComponent<IngredientPropertiesComponent>().GetOrderComposition()).ToArray();
                barbecue1.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[0]);
                barbecue1.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[2]);
                barbecue1.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[3]);
                (barbecue1.GetComponent<ServerCookingHandler>().GetServerUpdate() as CookingStateMessage).m_cookingProgress = 4.36f;
                blender1.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[1]);
                blender1.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[1]);
                blender1.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[1]);
                (blender1.GetComponent<ServerMixingHandler>().GetServerUpdate() as MixingStateMessage).m_mixingProgress = 12.50f;
                barbecue2.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[4]);
                barbecue2.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[2]);
                barbecue2.GetComponent<ServerIngredientContainer>().AddIngredient(ingredients[3]);
                (barbecue2.GetComponent<ServerCookingHandler>().GetServerUpdate() as CookingStateMessage).m_cookingProgress = 7.82f;
            }

            if (tas.replayFrameCount == 45 * tas.replayPeriod)
            {
                players[0].transform.position = new Vector3(-1.9505f, 1.0000f, 14.5963f);
                players[0].transform.rotation = Quaternion.Euler(0, 315, 0);
                players[1].transform.position = new Vector3(10.9000f, 1.0000f, 14.6000f);
                players[1].transform.rotation = Quaternion.Euler(0, 0, 0);
                players[2].transform.position = new Vector3(-4.1286f, 1.0000f, 14.6000f);
                players[2].transform.rotation = Quaternion.Euler(0, 51.8027f, 0);
                players[3].transform.position = new Vector3(9.7143f, 1.0000f, 14.6000f);
                players[3].transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            int left = 100, right = 20;
            if (tas.replayFrameCount >= 100 * tas.replayPeriod &&
                tas.replayFrameCount <= (4500 - left) * tas.replayPeriod ||
                tas.replayFrameCount >= (4500 + right) * tas.replayPeriod &&
                tas.replayFrameCount <= (9000 - left) * tas.replayPeriod ||
                tas.replayFrameCount >= (9000 + right) * tas.replayPeriod &&
                tas.replayFrameCount <= (13500 - left) * tas.replayPeriod)
            {
                Time.timeScale = 20f;
                tas.replayFrameCount += 20 * tas.replayPeriod;
            }
            else
            {
                Time.timeScale = 1f;
                tas.replayFrameCount += tas.replayPeriod;
            }
            return true;
        }
    }
}
