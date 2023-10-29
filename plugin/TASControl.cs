using System;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
// using Newtonsoft.Json.Linq;  // failed to load Newtonsoft.Json at runtime 
using OC2TAS.Extension;
using System.Text.RegularExpressions;
using Team17.Online;

namespace OC2TAS
{
    public class TASControl
    {
        private readonly KeyCode freezeToggleKey;
        private readonly KeyCode nextFrameKey;
        private readonly KeyCode replayKey;
        private readonly KeyCode videoKey;
        private readonly KeyCode GUIKey;
        private bool frozen;
        public ReplayState replayState;
        private int replayFrameCount;
        private int replayPeriod = 2;
        private bool video;
        private VideoRecorder videoRecorder;
        private int bindUIEmoteAt;
        private bool enableGUI;

        private float audioResumeTime;

        public TASScript script;
        private GamepadState[] last_state;
        private readonly GUIStyle guiStyle;

        private PlayerControls[] playerControls;
        private TASControlScheme[] tasControlSchemes;
        private ClientEmoteWheel[] emoteWheels;
        private ClientEmoteWheel[] UIemoteWheels;
        private ServerSessionInteractable[] serverSessionInteractables;
        private MultiplayerController multiplayerController;
        private Animator[] animators;

        public enum ReplayState
        {
            Init,
            InIntro,
            InLevel,
            FirstFrameAfterReplay,
            Over
        }

        public class GamepadState
        {
            public bool b_pickup;
            public bool b_interact;
            public bool b_dash;
            public bool b_emote;
            public float x;
            public float y;
            public GamepadState() { }
            public GamepadState(bool pickup, bool interact, bool dash, bool emote, float x, float y)
            {
                this.b_pickup = pickup;
                this.b_interact = interact;
                this.b_dash = dash;
                this.b_emote = emote;
                this.x = x;
                this.y = y;
            }
        }
        public class TASControlScheme
        {
            public TASLogicalButton useButton;
            public TASLogicalButton pickupButton;
            public TASLogicalButton dashButton;
            public TASLogicalValue xValue;
            public TASLogicalValue yValue;
            public TASLogicalButton emoteButton;
            public TASLogicalButton emoteOverRenableInputButton;
            public TASLogicalValue emoteXValue;
            public TASLogicalValue emoteYValue;
            public TASLogicalButton UIemoteButton;
            public TASLogicalButton UIemoteOverRenableInputButton;
            public TASLogicalValue UIemoteXValue;
            public TASLogicalValue UIemoteYValue;
        }
        public class PositionCorrection
        {
            public string objectName;
            public int frame;
            public Vector3 position;
            public PositionCorrection(string name, int frame, Vector3 position)
            {
                this.objectName = name;
                this.frame = frame;
                this.position = position;
            }
        }
        public class TASScript
        {
            public string level;
            public int[][] menu;
            public GamepadState[,] gamepadStates;
            public bool includeNull;
            public string author;
            public PositionCorrection[] positionCorrections;
        }

        public class TASLogicalButton : ILogicalButton
        {
            private int playerID;
            public ILogicalButton original;
            public bool down;
            public bool justPressed;
            public bool justReleased;

            public TASLogicalButton(int playerID, ILogicalButton original)
            {
                this.playerID = playerID;
                this.original = original;
            }

            public void ClaimPressEvent() { }
            public void ClaimReleaseEvent() { }
            public float GetHeldTimeLength() { return 0; }
            public void GetLogicTreeData(out AcyclicGraph<ILogicalElement, LogicalLinkInfo> _graph, out AcyclicGraph<ILogicalElement, LogicalLinkInfo>.Node _head)
            {
                original.GetLogicTreeData(out _graph, out _head);
            }
            public bool HasUnclaimedPressEvent() { return false; }
            public bool HasUnclaimedReleaseEvent() { return false; }
            public bool IsDown() { return down; }
            public bool JustPressed() { return justPressed; }
            public bool JustReleased() { return justReleased; }
        }

        public class TASLogicalValue : ILogicalValue
        {
            private int playerID;
            private PlayerControls playerControls;
            public ILogicalValue original;
            public float value;

            public TASLogicalValue(int playerID, PlayerControls playerControls, ILogicalValue original)
            {
                this.playerID = playerID;
                this.original = original;
                this.playerControls = playerControls;
            }

            public void GetLogicTreeData(out AcyclicGraph<ILogicalElement, LogicalLinkInfo> _graph, out AcyclicGraph<ILogicalElement, LogicalLinkInfo>.Node _head)
            {
                original.GetLogicTreeData(out _graph, out _head);
            }

            public float GetValue() 
            {
                if (playerControls == null)
                    return value;
                if (playerControls.CanButtonBePressed())
                    return value;
                return 0f;
            }
        }

        public TASControl()
        {
            freezeToggleKey = KeyCode.F9;
            replayKey = KeyCode.F10;
            nextFrameKey = KeyCode.F11;
            GUIKey = KeyCode.F8;
            videoKey = KeyCode.F1;
            playerControls = new PlayerControls[4];
            emoteWheels = new ClientEmoteWheel[4];
            UIemoteWheels = new ClientEmoteWheel[4];
            bindUIEmoteAt = -1;
            replayState = ReplayState.Init;

            Texture2D consoleBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            consoleBackground.SetPixel(0, 0, new Color(1, 1, 1, 0.4f));
            consoleBackground.Apply();
            guiStyle = new GUIStyle(GUIStyle.none);
            guiStyle.fontSize = 32;
            guiStyle.normal.textColor = Color.black;
            guiStyle.fontStyle = FontStyle.Bold;
            guiStyle.normal.background = consoleBackground;
        }

        public void OnSceneLoaded()
        {
            if (frozen) ToggleFreeze();
            replayFrameCount = 0;
            replayState = ReplayState.Init;
            if (video)
            {
                TASPlugin.audioRecorder.EndRecord();
                videoRecorder.Close();
            }
            video = false;
            bindUIEmoteAt = -1;
            playerControls = new PlayerControls[4];
        }

        public void LateUpdate()
        {
            if (Input.GetKeyDown(GUIKey))
                enableGUI = !enableGUI;

            if (video && (replayState == ReplayState.InIntro || replayState == ReplayState.InLevel))
            {
                if (replayFrameCount % replayPeriod == 0)
                {
                    audioResumeTime += 0.02f;
                    if (videoRecorder != null)
                        videoRecorder.AddFrame();
                }
                AudioListener.pause = TASPlugin.audioRecorder.totalTime > audioResumeTime;
            }

            if ((Input.GetKeyDown(replayKey) || Input.GetKeyDown(videoKey)) && 
                (replayState == ReplayState.InIntro || replayState == ReplayState.InLevel))
            {
                // abort replay
                EndReplay();
                replayState = ReplayState.Over;
                ToggleFreeze();  // unfreeze
                TASPlugin.pluginInstance.Log("Replay aborted");
                return;
            }

            if (replayState == ReplayState.InIntro)
            {
                if (replayFrameCount % replayPeriod == 0)
                {
                    if (UserSystemUtils.AreAllUsersInGameState(ServerUserSystem.m_Users, GameState.RanLevelIntro) 
                        || UserSystemUtils.AreAllUsersInGameState(ServerUserSystem.m_Users, GameState.InLevel))
                    {
                        replayFrameCount = 0;
                        replayState = ReplayState.InLevel;
                        return;
                    }
                    Time.timeScale = 1f;
                }
                if (replayFrameCount % replayPeriod == 1)
                    Time.timeScale = 0f;
                replayFrameCount++;
                return;
            }

            if (replayState == ReplayState.InLevel)
            {
                if (replayFrameCount / replayPeriod >= script.gamepadStates.GetLength(0))
                {
                    EndReplay();
                    replayState = script.includeNull ? ReplayState.FirstFrameAfterReplay : ReplayState.Over;
                    TASPlugin.pluginInstance.Log("Replay ended");
                    return;
                }

                if (replayFrameCount % replayPeriod == 0)
                {
                    if (replayFrameCount == replayPeriod * bindUIEmoteAt)
                        TryBindUIEmote();
                    multiplayerController.LateUpdate();
                    GamepadState[] state = new GamepadState[4];
                    for (int i = 0; i < 4; i++)
                    {
                        state[i] = script.gamepadStates[replayFrameCount / replayPeriod, i];
                        if (playerControls[i] != null)
                        {
                            tasControlSchemes[i].useButton.down = state[i].b_interact;
                            tasControlSchemes[i].useButton.justPressed= state[i].b_interact && !last_state[i].b_interact;
                            tasControlSchemes[i].useButton.justReleased = !state[i].b_interact && last_state[i].b_interact;
                            tasControlSchemes[i].pickupButton.justPressed = state[i].b_pickup && !last_state[i].b_pickup;
                            tasControlSchemes[i].dashButton.justPressed = state[i].b_dash && !last_state[i].b_dash;
                            tasControlSchemes[i].xValue.value = state[i].x;
                            tasControlSchemes[i].yValue.value = -state[i].y;
                            tasControlSchemes[i].emoteButton.down = state[i].b_emote;
                            tasControlSchemes[i].emoteButton.justPressed = state[i].b_emote && !last_state[i].b_emote;
                            tasControlSchemes[i].emoteOverRenableInputButton.down = state[i].b_pickup || state[i].b_interact || state[i].b_dash;
                            tasControlSchemes[i].emoteXValue.value = state[i].x;
                            tasControlSchemes[i].emoteYValue.value = -state[i].y;
                        }
                        if (UIemoteWheels[i] != null)
                        {
                            tasControlSchemes[i].UIemoteButton.down = state[i].b_emote;
                            tasControlSchemes[i].UIemoteButton.justPressed = state[i].b_emote && !last_state[i].b_emote;
                            tasControlSchemes[i].UIemoteOverRenableInputButton.down = state[i].b_pickup || state[i].b_interact || state[i].b_dash;
                            tasControlSchemes[i].UIemoteXValue.value = state[i].x;
                            tasControlSchemes[i].UIemoteYValue.value = -state[i].y;
                        }
                    }
                    last_state = state;

                    for (int i = 0; i < 4; i++)
                    {
                        if (playerControls[i] != null)
                        {
                            playerControls[i].Update();
                            emoteWheels[i].UpdateSynchronising();
                        }
                        if (UIemoteWheels[i] != null)
                            UIemoteWheels[i].Update();
                    }

                    foreach (var serverSessionInteractable in serverSessionInteractables)
                        if (serverSessionInteractable.enabled)
                            serverSessionInteractable.UpdateSynchronising();
                    for (int i = 0; i < 4; i++)
                    {
                        if (playerControls[i] != null)
                        {
                            tasControlSchemes[i].useButton.justPressed = false;
                            tasControlSchemes[i].useButton.justReleased = false;
                            tasControlSchemes[i].pickupButton.justPressed = false;
                            tasControlSchemes[i].emoteButton.justPressed = false;
                        }
                        if (UIemoteWheels[i] != null)
                            tasControlSchemes[i].UIemoteButton.justPressed = false;
                    }
                    multiplayerController.LateUpdate();

                    for (int i = 0; i < 4; i++)
                        if (playerControls[i] != null)
                            playerControls[i].Update();
                    multiplayerController.Update();
                    multiplayerController.LateUpdate();

                    for (int i = 0; i < 4; i++)
                        if (playerControls[i] != null)
                            playerControls[i].Update();
                    multiplayerController.Update();
                    multiplayerController.LateUpdate();

                    foreach (Animator animator in animators)
                        if (animator != null)
                            animator.Update(0f);

                    Time.timeScale = 1f;
                }
                if (replayFrameCount % replayPeriod == 1)
                {
                    Time.timeScale = 0f;
                    for (int i = 0; i < 4; i++)
                        if (playerControls[i] != null)
                            tasControlSchemes[i].dashButton.justPressed = false;
                }

                replayFrameCount++;
                if (replayFrameCount % replayPeriod == 0)
                    foreach (PositionCorrection pc in script.positionCorrections)
                        if (replayFrameCount == pc.frame * replayPeriod)
                        {
                            GameObject gameObject = GameObject.Find(pc.objectName);
                            if (gameObject != null)
                            {
                                Vector3 position = gameObject.transform.position;
                                Vector3 position_new = pc.position;
                                if (Vector3.Distance(position, position_new) > 0.0002)
                                {
                                    gameObject.transform.position = position_new;
                                    TASPlugin.pluginInstance.Log(String.Format("position correction at frame {0}, {1}", pc.frame, pc.objectName));
                                    TASPlugin.pluginInstance.Log(String.Format("  ({0:F4}, {1:F4}, {2:F4}) -> ({3:F4}, {4:F4}, {5:F4})", position.x, position.y, position.z, position_new.x, position_new.y, position_new.z));
                                }
                            }
                        }
                return;
            }
            
            if (frozen)  // frozen but not replaying
            {
                if (replayState == ReplayState.Init)
                    AudioListener.pause = true;
                Time.timeScale = 0f;
                if (Input.GetKeyDown(replayKey) && replayState == ReplayState.Init)
                    StartReplay(false);
                else if (Input.GetKeyDown(videoKey) && replayState == ReplayState.Init)
                    StartReplay(true);
                else if (Input.GetKeyDown(nextFrameKey))
                {
                    if (replayState == ReplayState.FirstFrameAfterReplay)
                        replayState = ReplayState.Over;
                    else
                    {
                        Time.timeScale = 1f;
                        if (replayState == ReplayState.Init)
                            AudioListener.pause = false;
                    }
                }
            }
            if (Input.GetKeyDown(freezeToggleKey)) ToggleFreeze();
        }

        public void OnDestroy() 
        { 
            if (frozen) ToggleFreeze();
            if (video)
            {
                TASPlugin.audioRecorder.EndRecord();
                videoRecorder.Close();
            }
        }

        public void OnGUI()
        {
            if (!enableGUI) return;
            string[] message = new string[4];
            for (int i = 0; i < 4; ++i)
            {
                Vector3 position = (playerControls[i] == null) ? Vector3.zero : playerControls[i].transform.position;
                message[i] = String.Format("P{0}, {1,10:F4}, {2,10:F4}, {3,10:F4}", i+1, position.x, position.y, position.z);
            }
            GUI.Label(
                new Rect(700, 20, 550, 200),
                String.Format(
                    "  Frame {0}\n  {1}\n  {2}\n  {3}\n  {4}",
                    replayState == ReplayState.InIntro ? 0 : replayFrameCount / replayPeriod, message[0], message[1], message[2], message[3]),
                guiStyle);
            //if (replayFrameCount / replayPeriod >= 8890 && replayFrameCount / replayPeriod <= 8920)
            //{
            //    GameObject gameObject = GameObject.Find("equipment_glass_01(Clone)_Rigidbody");
            //    if (gameObject != null)
            //    {
            //        GUI.Label(
            //            new Rect(700, 300, 550, 50),
            //            String.Format(
            //                "  {0,10:F4}, {1,10:F4}, {2,10:F4}",
            //                gameObject.transform.position.x,
            //                gameObject.transform.position.y,
            //                gameObject.transform.position.z),
            //            guiStyle);
            //    }
            //}
        }

        private static void CreateEmptyScript()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = dir + "\\replay.json";
            LevelConfigBase kitchenLevelConfigBase = GameUtils.GetLevelConfig();
            string content = String.Format("{{\n" +
                "  \"author\": \"\",\n" +
                "  \"level\": \"{0}\",\n" +
                "  \"menu\": [],\n" +
                "  \"pickup_flag\": [0,0,0,0],\n" +
                "  \"state\": \n  [\n" +
                "    [\n" +
                "      [null, false, false, false, 0.0, 0.0],\n" +
                "      [null, false, false, false, 0.0, 0.0],\n" +
                "      [null, false, false, false, 0.0, 0.0],\n" +
                "      [null, false, false, false, 0.0, 0.0]\n" +
                "    ]\n  ]\n}}", kitchenLevelConfigBase.name);
            File.WriteAllText(path, content);
            TASPlugin.pluginInstance.Log(String.Format("Empty script created for \"{0}\"", kitchenLevelConfigBase.name));
        }

        private static TASScript ParseScript()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = dir + "\\replay.json";
            if (!File.Exists(path))
            {
                CreateEmptyScript();
                return null;
            }
            try
            {
                string jsonString = File.ReadAllText(path);
                string s1 = Regex.Replace(jsonString, "\\s", "");

                TASScript script = new TASScript();
                script.level = Regex.Match(s1, "\"level\":\"(.*?)\"").Groups[1].Value;
                script.author = Regex.Match(s1, "\"author\":\"(.*?)\"").Groups[1].Value;

                string[] sMenu;
                Match mDynamicMenu = Regex.Match(s1, @"""menu"":\[\[(.*?)],?]");
                if (mDynamicMenu.Success)
                    sMenu = Regex.Split(mDynamicMenu.Groups[1].Value, @"],\[");
                else
                    sMenu = new string[] { Regex.Match(s1, @"""menu"":\[(.*?)]").Groups[1].Value };
                script.menu = new int[sMenu.Length][];
                for (int i = 0; i < sMenu.Length; i++)
                {
                    if (sMenu[i].Length == 0)
                        script.menu[i] = new int[0];
                    else
                        script.menu[i] = sMenu[i].Split(',').Select(x => int.Parse(x)).ToArray();
                }

                string s2 = Regex.Match(s1, @"""state"":\[\[\[(.*)]],?]").Groups[1].Value;
                string[] s3 = Regex.Split(s2, @"]],\[\[");
                GamepadState[,] gamepadStates = new GamepadState[s3.Length, 4];
                int flagNull = -1;
                for (int i = 0; i < s3.Length; ++i)
                {
                    if (s3[i].Contains("null"))
                    {
                        flagNull = i;
                        break;
                    }
                    string[] s4 = Regex.Split(s3[i], "],\\[");
                    for (int j = 0; j < 4; ++j)
                    {
                        string[] s5 = s4[j].Split(',');
                        gamepadStates[i, j] = new GamepadState(
                            bool.Parse(s5[0]),
                            bool.Parse(s5[1]),
                            bool.Parse(s5[2]),
                            bool.Parse(s5[3]),
                            float.Parse(s5[4]),
                            float.Parse(s5[5])
                        );
                    }
                }
                if (flagNull >= 0)
                {
                    script.includeNull = true;
                    GamepadState[,] gamepadStatesNew = new GamepadState[flagNull, 4];
                    for (int i = 0; i < flagNull; ++i)
                        for (int j = 0; j < 4; ++j)
                            gamepadStatesNew[i, j] = gamepadStates[i, j];
                    script.gamepadStates = gamepadStatesNew;
                }
                else 
                    script.gamepadStates = gamepadStates;

                Match m = Regex.Match(jsonString, @"""position_correction""\s*:\s*\[\s*\[(.*?)]\s*,?\s*]", RegexOptions.Singleline);
                if (m.Success)
                {
                    string[] s6 = Regex.Split(m.Groups[1].Value, @"]\s*,\s*\[");
                    script.positionCorrections = new PositionCorrection[s6.Length];
                    for (int i = 0; i < s6.Length; ++i)
                    {
                        string[] s7 = s6[i].Split(',');
                        script.positionCorrections[i] = new PositionCorrection(
                            Regex.Match(s7[0], @"""(.*)""").Groups[1].Value,
                            int.Parse(s7[1]), 
                            new Vector3(float.Parse(s7[2]), float.Parse(s7[3]), float.Parse(s7[4]))
                        );
                    }
                }
                else script.positionCorrections = null;
                return script;
            }
            catch (Exception ex)
            {
                TASPlugin.pluginInstance.Log(ex.ToString());
                return null;
            }
        }

        //private TASScript ParseScript()
        //{
        //    string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    string path = dir + "\\replay.json";
        //    if (!File.Exists(path)) return null;
        //    try
        //    {
        //        string jsonString = File.ReadAllText(path);
        //        JObject obj = JObject.Parse(jsonString);
        //        TASScript script = new TASScript();
        //        script.author = obj.Value<string>("author");
        //        script.level = obj.Value<string>("level");
        //        script.menu = ((JArray)obj["menu"]).Select(x => x.Value<int>()).ToArray();
        //        if (obj.Property("position_correction") == null)
        //            script.positionCorrections = null;
        //        else
        //        {
        //            JArray positionCorrections = (JArray)obj["position_correction"];
        //            script.positionCorrections = new PositionCorrection[positionCorrections.Count];
        //            for (int i = 0; i < positionCorrections.Count; i++)
        //            {
        //                JArray pc = (JArray)positionCorrections[i];
        //                script.positionCorrections[i] = new PositionCorrection(
        //                    pc[0].Value<string>(),
        //                    pc[1].Value<int>(),
        //                    new Vector3(pc[2].Value<float>(), pc[3].Value<float>(), pc[4].Value<float>())
        //                );
        //            }
        //        }

        //        JArray state = (JArray)obj["state"];
        //        GamepadState[,] gamepadStates = new GamepadState[state.Count, 4];
        //        int flagNull = -1;
        //        for (int i = 0; i < state.Count; ++i)
        //        {
        //            JArray frameState = (JArray)state[i];
        //            if (frameState.Any(x => x.Contains(null)))
        //            {
        //                flagNull = i;
        //                break;
        //            }
        //            for (int j = 0; j < 4; ++j)
        //            {
        //                JArray playerstate = (JArray)frameState[j];
        //                gamepadStates[i, j] = new GamepadState(
        //                    playerstate[0].Value<bool>(),
        //                    playerstate[1].Value<bool>(),
        //                    playerstate[2].Value<bool>(),
        //                    playerstate[3].Value<bool>(),
        //                    playerstate[4].Value<float>(),
        //                    playerstate[5].Value<float>()
        //                );
        //            }
        //        }
        //        if (flagNull >= 0)
        //        {
        //            firstFrameAfterReplay = true;
        //            GamepadState[,] gamepadStatesNew = new GamepadState[flagNull, 4];
        //            for (int i = 0; i < flagNull; ++i)
        //                for (int j = 0; j < 4; ++j)
        //                    gamepadStatesNew[i, j] = gamepadStates[i, j];
        //            script.gamepadStates = gamepadStatesNew;
        //        }
        //        else
        //        {
        //            script.gamepadStates = gamepadStates;
        //        }
        //        return script;
        //    }
        //    catch (Exception ex)
        //    {
        //        pluginInstance.Log(ex.ToString());
        //        return null;
        //    }
        //}

        private bool StartReplay(bool video)
        {
            script = ParseScript();
            if (script == null) return false;
            LevelConfigBase kitchenLevelConfigBase = GameUtils.GetLevelConfig();
            if (!kitchenLevelConfigBase.name.Equals(script.level))
            {
                TASPlugin.pluginInstance.Log(String.Format("Level mismatch: \"{0}\"", kitchenLevelConfigBase.name));
                return false;
            }

            GameObject[] players = new GameObject[4];
            for (int i = 0; i < PlayerIDProvider.s_AllProviders.Count; i++)
                players[(int)PlayerIDProvider.s_AllProviders._items[i].GetID()] = PlayerIDProvider.s_AllProviders._items[i].gameObject;
            for (int i = 0; i < 4; i++)
            {
                GameObject player = players[i];
                if (player != null)
                {
                    playerControls[i] = player.GetComponent<PlayerControls>();
                    playerControls[i].m_pickupDelay = 0.499f;
                    emoteWheels[i] = player.GetComponent<ClientEmoteWheel>();
                }
                else
                {
                    playerControls[i] = null;
                    emoteWheels[i] = null;
                }
            }
            if (playerControls[0] == null)
            {
                TASPlugin.pluginInstance.Log("No player found");
                return false;
            }

            if (!UserSystemUtils.AreAllUsersInGameState(ServerUserSystem.m_Users, GameState.RunLevelIntro))
            {
                TASPlugin.pluginInstance.Log("StartReplay at wrong gamestate");
                return false;
            }

            tasControlSchemes = new TASControlScheme[4].Select(x => new TASControlScheme()).ToArray();
            for (int i = 0; i < 4; i++)
                if (playerControls[i] != null)
                {
                    PlayerControls.ControlSchemeData controlScheme = playerControls[i].ControlScheme;
                    tasControlSchemes[i].useButton = new TASLogicalButton(i, controlScheme.m_worksurfaceUseButton);
                    tasControlSchemes[i].pickupButton = new TASLogicalButton(i, controlScheme.m_pickupButton);
                    tasControlSchemes[i].dashButton = new TASLogicalButton(i, controlScheme.m_dashButton);
                    tasControlSchemes[i].xValue = new TASLogicalValue(i, playerControls[i], controlScheme.m_moveX);
                    tasControlSchemes[i].yValue = new TASLogicalValue(i, playerControls[i], controlScheme.m_moveY);
                    controlScheme.m_worksurfaceUseButton = tasControlSchemes[i].useButton;
                    controlScheme.m_pickupButton = tasControlSchemes[i].pickupButton;
                    controlScheme.m_dashButton = tasControlSchemes[i].dashButton;
                    controlScheme.m_moveX = tasControlSchemes[i].xValue;
                    controlScheme.m_moveY = tasControlSchemes[i].yValue;
                    tasControlSchemes[i].emoteButton = new TASLogicalButton(i, emoteWheels[i].get_m_wheelButton());
                    emoteWheels[i].set_m_wheelButton(tasControlSchemes[i].emoteButton);
                    tasControlSchemes[i].emoteOverRenableInputButton = new TASLogicalButton(i, emoteWheels[i].get_m_renableInputButton());
                    emoteWheels[i].set_m_renableInputButton(tasControlSchemes[i].emoteOverRenableInputButton);
                    tasControlSchemes[i].emoteXValue = new TASLogicalValue(i, null, emoteWheels[i].get_m_xMovement());
                    emoteWheels[i].set_m_xMovement(tasControlSchemes[i].emoteXValue);
                    tasControlSchemes[i].emoteYValue = new TASLogicalValue(i, null, emoteWheels[i].get_m_yMovement());
                    emoteWheels[i].set_m_yMovement(tasControlSchemes[i].emoteYValue);
                }

            UIemoteWheels = new ClientEmoteWheel[4];

            serverSessionInteractables = GameObject.FindObjectsOfType<ServerSessionInteractable>();
            multiplayerController = GameUtils.RequireManager<MultiplayerController>();
            animators = GameObject.FindObjectsOfType<Animator>();

            last_state = new GamepadState[4].Select(g => new GamepadState()).ToArray();
            replayFrameCount = 0;
            replayState = ReplayState.InIntro;

            if (video)
            {
                this.video = true;
                videoRecorder = new VideoRecorder();
                replayPeriod = 5;
                TASPlugin.audioRecorder.StartRecord();
                audioResumeTime = 0f;
                TASPlugin.pluginInstance.Log(String.Format("Replay (video) script for level \"{0}\"", script.level));
            }
            else
            { 
                AudioListener.pause = false;
                TASPlugin.pluginInstance.Log(String.Format("Replay script for level \"{0}\"", script.level));
            }

            return true;
        }

        public void BindUIEmote()
        {
            bindUIEmoteAt = replayFrameCount / replayPeriod + 1;
        }

        private void TryBindUIEmote()
        {
            GameObject parent = GameObject.Find("UIPlayers");
            if (parent == null) return;
            ClientEmoteWheel[] wheels = parent.GetComponentsInChildren<ClientEmoteWheel>();
            
            foreach (ClientEmoteWheel wheel in wheels)
                UIemoteWheels[(int)wheel.GetComponent<EmoteWheel>().m_player] = wheel;
            for (int i = 0; i < 4; i++)
                if (UIemoteWheels[i] != null)
                {
                    tasControlSchemes[i].UIemoteButton = new TASLogicalButton(i, UIemoteWheels[i].get_m_wheelButton());
                    UIemoteWheels[i].set_m_wheelButton(tasControlSchemes[i].UIemoteButton);
                    tasControlSchemes[i].UIemoteOverRenableInputButton = new TASLogicalButton(i, UIemoteWheels[i].get_m_renableInputButton());
                    UIemoteWheels[i].set_m_renableInputButton(tasControlSchemes[i].UIemoteOverRenableInputButton);
                    tasControlSchemes[i].UIemoteXValue = new TASLogicalValue(i, null, UIemoteWheels[i].get_m_xMovement());
                    UIemoteWheels[i].set_m_xMovement(tasControlSchemes[i].UIemoteXValue);
                    tasControlSchemes[i].UIemoteYValue = new TASLogicalValue(i, null, UIemoteWheels[i].get_m_yMovement());
                    UIemoteWheels[i].set_m_yMovement(tasControlSchemes[i].UIemoteYValue);
                }
        }

        private void EndReplay()
        {
            // another replay requires restarting the level
            for (int i = 0; i < 4; i++)
            {
                if (playerControls[i] != null)
                {
                    PlayerControls.ControlSchemeData controlSchemeData = playerControls[i].ControlScheme;
                    controlSchemeData.m_worksurfaceUseButton = tasControlSchemes[i].useButton.original;
                    controlSchemeData.m_pickupButton = tasControlSchemes[i].pickupButton.original;
                    controlSchemeData.m_dashButton = tasControlSchemes[i].dashButton.original;
                    controlSchemeData.m_moveX = tasControlSchemes[i].xValue.original;
                    controlSchemeData.m_moveY = tasControlSchemes[i].yValue.original;

                    controlSchemeData.m_dashButton.JustPressed();
                    controlSchemeData.m_pickupButton.JustPressed();
                    controlSchemeData.m_worksurfaceUseButton.JustPressed();
                }
                if (emoteWheels[i] != null)
                {
                    emoteWheels[i].set_m_wheelButton(tasControlSchemes[i].emoteButton.original);
                    emoteWheels[i].get_m_wheelButton().JustPressed();
                    emoteWheels[i].set_m_renableInputButton(tasControlSchemes[i].emoteOverRenableInputButton.original);
                    emoteWheels[i].get_m_renableInputButton().JustPressed();
                    emoteWheels[i].set_m_xMovement(tasControlSchemes[i].emoteXValue.original);
                    emoteWheels[i].set_m_yMovement(tasControlSchemes[i].emoteYValue.original);
                }
                if (UIemoteWheels[i] != null)
                {
                    UIemoteWheels[i].set_m_wheelButton(tasControlSchemes[i].UIemoteButton.original);
                    UIemoteWheels[i].get_m_wheelButton().JustPressed();
                    UIemoteWheels[i].set_m_renableInputButton(tasControlSchemes[i].UIemoteOverRenableInputButton.original);
                    UIemoteWheels[i].get_m_renableInputButton().JustPressed();
                    UIemoteWheels[i].set_m_xMovement(tasControlSchemes[i].UIemoteXValue.original);
                    UIemoteWheels[i].set_m_yMovement(tasControlSchemes[i].UIemoteYValue.original);
                }
            }
            Time.timeScale = 0f;
            AudioListener.pause = false;
            replayPeriod = 2;
            if (video)
            {
                TASPlugin.audioRecorder.EndRecord();
                videoRecorder.Close();
            }
            video = false;
        }

        private void ToggleFreeze()
        {
            frozen = !frozen;
            if (frozen)
            {
                TASPlugin.pluginInstance.Log("Freezing");
                Time.timeScale = 0f;
                Time.captureFramerate = 50;
                if (replayState == ReplayState.Init)
                    AudioListener.pause = true;
            }
            else
            {
                TASPlugin.pluginInstance.Log("Unfreezing");
                Time.timeScale = 1f;
                Time.captureFramerate = 0;
                AudioListener.pause = false;
            }
        }
    }
}
