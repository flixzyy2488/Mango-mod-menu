using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace ModMenuPatch.HarmonyPatches
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    internal class MenuPatch
    {
        public static bool ResetSpeed = false;

        static string[] buttons = new string[] { "Toggle Super Monke", "Toggle Tag Gun", "Toggle Speed Boost", "Tag All", "Turn Off Tag Freeze", "Toggle Beacon", "Platform Monke" };
        static bool?[] buttonsActive = new bool?[] { false, false, false, false, false, false, false };

        //static string[] buttons = new string[] { "Toggle Super Monke", "Toggle Tag Gun", "Toggle Speed Boost", "Tag All", "Turn Off Tag Freeze", "Toggle Beacon", "+10 speed", "-10 speed" };
        //static bool?[] buttonsActive = new bool?[] { false, false, false, false, false, false, false, false };

        //static string[] buttons = new string[] { "Toggle Super Monke", "Toggle Tag Gun", "Toggle Speed Boost", "Tag All", "Turn Off Tag Freeze", "Toggle Beacon" };
        //static bool?[] buttonsActive = new bool?[] { false, false, false, false, false, false };
        static bool gripDown;
        static GameObject menu = null;
        static GameObject canvasObj = null;
        static GameObject reference = null;
        public static int framePressCooldown = 0;
        static GameObject pointer = null;
        static bool gravityToggled = false;
        static bool flying = false;
        static int btnCooldown = 0;

        static int speedPlusCooldown = 0;
        static int speedMinusCooldown = 0;

        static float? maxJumpSpeed = null;

        private static void Prefix()
        {
            if (ModMenuPatch.allowSpaceMonke)
            {

            }
            else
            {

            }

            try
            {
                if (maxJumpSpeed == null)
                {
                    maxJumpSpeed = GorillaLocomotion.Player.Instance.maxJumpSpeed;
                }

                List<UnityEngine.XR.InputDevice> list = new List<UnityEngine.XR.InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
                list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out gripDown);

                if (gripDown && menu == null)
                {
                    Draw();
                    if (reference == null)
                    {
                        reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        GameObject.Destroy(reference.GetComponent<MeshRenderer>());
                        reference.transform.parent = GorillaLocomotion.Player.Instance.rightHandTransform;
                        reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                        reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    }
                }
                else if (!gripDown && menu != null)
                {
                    GameObject.Destroy(menu);
                    menu = null;
                    GameObject.Destroy(reference);
                    reference = null;
                }

                if (gripDown && menu != null)
                {
                    menu.transform.position = GorillaLocomotion.Player.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaLocomotion.Player.Instance.leftHandTransform.rotation;
                }

                if (buttonsActive[0] == true)
                {
                    bool primaryDown = false;
                    bool secondaryDown = false;
                    list = new List<UnityEngine.XR.InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
                    list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryDown);
                    list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryDown);

                    if (primaryDown)
                    {
                        GorillaLocomotion.Player.Instance.transform.position += (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * 12f;
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        if (!flying)
                        {
                            flying = true;
                        }
                    }
                    else if (flying)
                    {
                        GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = (GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime) * 36f; //12f;
                        flying = false;
                    }

                    if (secondaryDown)
                    {
                        if (!gravityToggled && GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity == true)
                        {
                            GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = false;
                            gravityToggled = true;
                        }
                        else if (!gravityToggled && GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity == false)
                        {
                            GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = true;
                            gravityToggled = true;
                        }
                    }
                    else
                    {
                        gravityToggled = false;
                    }
                }

                if (buttonsActive[1] == true)
                {
                    bool flag = false;
                    bool flag2 = false;
                    list = new List<UnityEngine.XR.InputDevice>();
                    InputDevices.GetDevices(list);
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
                    list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out flag);
                    list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out flag2);
                    if (flag2)
                    {
                        RaycastHit hitInfo;
                        Physics.Raycast(GorillaLocomotion.Player.Instance.rightHandTransform.position - GorillaLocomotion.Player.Instance.rightHandTransform.up, -GorillaLocomotion.Player.Instance.rightHandTransform.up, out hitInfo);
                        if (pointer == null)
                        {
                            pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            GameObject.Destroy(pointer.GetComponent<Rigidbody>());
                            GameObject.Destroy(pointer.GetComponent<SphereCollider>());
                            pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        }
                        pointer.transform.position = hitInfo.point;

                        Photon.Realtime.Player player;
                        bool taggable = GorillaTagger.Instance.TryToTag(hitInfo, true, out player);
                        if (flag && !taggable)
                        {
                            pointer.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                        }
                        else if (!flag && taggable)
                        {
                            pointer.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                        }
                        else if (flag && taggable)
                        {
                            pointer.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                            PhotonView.Get(GorillaTagManager.instance.GetComponent<GorillaGameManager>()).RPC("ReportTagRPC", RpcTarget.MasterClient, new object[]
                            {
                                player
                            });
                        }
                    }
                    else
                    {
                        GameObject.Destroy(pointer);
                        pointer = null;
                    }
                }

                if (buttonsActive[2] == true)
                {
                    //GorillaLocomotion.Player.Instance.maxJumpSpeed = 999f;
                    //GorillaLocomotion.Player.Instance.jumpMultiplier = 1.45f;

                    //GorillaLocomotion.Player.Instance.maxJumpSpeed = 50f;
                    //GorillaLocomotion.Player.Instance.jumpMultiplier = 1.15f;

                    //GorillaLocomotion.Player.Instance.maxJumpSpeed = 100f;
                    //GorillaLocomotion.Player.Instance.jumpMultiplier = 1.30f;

                    GorillaLocomotion.Player.Instance.maxJumpSpeed = ModMenuPatch.speedMultiplier.Value;
                    GorillaLocomotion.Player.Instance.jumpMultiplier = ModMenuPatch.jumpMultiplier.Value;
                }
                else
                {
                    //GorillaLocomotion.Player.Instance.maxJumpSpeed = (float)maxJumpSpeed;
                    //GorillaLocomotion.Player.Instance.jumpMultiplier = 1.1f;

                    GorillaLocomotion.Player.Instance.maxJumpSpeed = maxJumpSpeed.Value;
                    GorillaLocomotion.Player.Instance.jumpMultiplier = 1.15f;
                }

                if (buttonsActive[3] == true)
                {
                    if (btnCooldown == 0)
                    {
                        btnCooldown = Time.frameCount + 30;
                        foreach (var player in PhotonNetwork.PlayerList)
                        {
                            PhotonView.Get(GorillaTagManager.instance.GetComponent<GorillaGameManager>()).RPC("ReportTagRPC", RpcTarget.MasterClient, new object[]
                            {
                                player
                            });
                        }
                        GameObject.Destroy(menu);
                        menu = null;
                        Draw();
                    }
                }

                if (buttonsActive[4] == true)
                {
                    GorillaLocomotion.Player.Instance.disableMovement = false;
                }

                if (buttonsActive[5] == true)
                {
                    VRRig[] vrRigs = (VRRig[])GameObject.FindObjectsOfType(typeof(VRRig));
                    foreach (VRRig rig in vrRigs)
                    {
                        if (!rig.isOfflineVRRig && !rig.isMyPlayer && !rig.photonView.IsMine)
                        {
                            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            GameObject.Destroy(beacon.GetComponent<BoxCollider>());
                            GameObject.Destroy(beacon.GetComponent<Rigidbody>());
                            GameObject.Destroy(beacon.GetComponent<Collider>());
                            beacon.transform.rotation = Quaternion.identity;
                            beacon.transform.localScale = new Vector3(0.04f, 200f, 0.04f);
                            beacon.transform.position = rig.transform.position;
                            beacon.GetComponent<MeshRenderer>().material = rig.mainSkin.material;
                            GameObject.Destroy(beacon, Time.deltaTime);
                        }
                    }
                }

                if (buttonsActive[6] == true)
                {
                    ProcessPlatformMonke();
                }


                //if (buttonsActive[6] == true)
                //{
                //    if (speedPlusCooldown == 0)
                //    {
                //        speedPlusCooldown = Time.frameCount + 30;
                //        ModMenuPatch.speedMultiplier.Value += 10;
                //        buttons[6] = "+10 speed (" + ModMenuPatch.speedMultiplier.Value.ToString() + ")";
                //        buttons[7] = "-10 speed (" + ModMenuPatch.speedMultiplier.Value.ToString() + ")";
                //    }
                //}

                //if (buttonsActive[7] == true)
                //{
                //    if (speedMinusCooldown == 0)
                //    {
                //        speedMinusCooldown = Time.frameCount + 30;
                //        ModMenuPatch.speedMultiplier.Value -= 10;
                //        buttons[7] = "-10 speed (" + ModMenuPatch.speedMultiplier.Value.ToString() + ")";
                //        buttons[6] = "+10 speed (" + ModMenuPatch.speedMultiplier.Value.ToString() + ")";
                //    }
                //}

                if (btnCooldown > 0)
                {
                    if (Time.frameCount > btnCooldown)
                    {
                        btnCooldown = 0;
                        buttonsActive[3] = false;
                        GameObject.Destroy(menu);
                        menu = null;
                        Draw();
                    }
                }

                //if (speedPlusCooldown > 0)
                //{
                //    if (Time.frameCount > speedPlusCooldown)
                //    {
                //        speedPlusCooldown = 0;
                //        buttonsActive[6] = false;
                //        GameObject.Destroy(menu);
                //        menu = null;
                //        Draw();
                //    }
                //}

                //if (speedMinusCooldown > 0)
                //{
                //    if (Time.frameCount > speedMinusCooldown)
                //    {
                //        speedMinusCooldown = 0;
                //        buttonsActive[7] = false;
                //        GameObject.Destroy(menu);
                //        menu = null;
                //        Draw();
                //    }
                //}


                //if (Time.frameCount % 4000 == 0)
                //{
                //    verified = true; //CheckVerify();
                //}
            }
            catch (Exception e)
            {
                File.WriteAllText("vey-spacemonkemodmenu_error.log", e.ToString());
            }

        }



        #region platform monke stuff
        static Vector3 scale = new Vector3(0.0125f, 0.28f, 0.3825f);
        static bool gripDown_left;
        static bool gripDown_right;
        static bool once_left;
        static bool once_right;
        static bool once_left_false;
        static bool once_right_false;
        static bool once_networking;
        static GameObject[] jump_left_network = new GameObject[9999];
        static GameObject[] jump_right_network = new GameObject[9999];
        static GameObject jump_left_local = null;
        static GameObject jump_right_local = null;

        static void ProcessPlatformMonke()
        {
            if (!once_networking)
            {
                PhotonNetwork.NetworkingClient.EventReceived += PlatformNetwork;
                once_networking = true;
            }
            List<UnityEngine.XR.InputDevice> list = new List<UnityEngine.XR.InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
            list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripDown_left);
            InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
            list[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripDown_right);

            if (gripDown_right) //Right hand - I'm sorry for putting if statements within if statements... OR ELSE IT WOULN'T WORK :<
            {
                if (!once_right)
                {
                    if (jump_right_local == null)
                    {
                        jump_right_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_local.transform.localScale = scale;
                        jump_right_local.transform.position = new Vector3(0, (float)-0.0075, 0) + GorillaLocomotion.Player.Instance.rightHandTransform.position; //The reason for moving it down a little, is because on the right hand the cube would spawn ontop of the hand for some reason.
                        jump_right_local.transform.rotation = GorillaLocomotion.Player.Instance.rightHandTransform.rotation;

                        object[] right_form_1 = new object[] { new Vector3(0, (float)-0.0075, 0) + GorillaLocomotion.Player.Instance.rightHandTransform.position, GorillaLocomotion.Player.Instance.rightHandTransform.rotation };

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_photoncode, right_form_1, raiseEventOptions, SendOptions.SendReliable);

                        once_right = true;
                        once_right_false = false;
                    }
                }
            }
            else
            {
                if (!once_right_false)
                {
                    if (jump_right_local != null)
                    {
                        GameObject.Destroy(jump_right_local);
                        jump_right_local = null;

                        once_right = false;
                        once_right_false = true;

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_deletion, null, raiseEventOptions, SendOptions.SendReliable);
                    }
                }
            }

            if (gripDown_left) //Left hand - I'm sorry for putting if statements within if statements... OR ELSE IT WOULN'T WORK :<
            {
                if (!once_left)
                {
                    if (jump_left_local == null)
                    {
                        jump_left_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_local.transform.localScale = scale;
                        jump_left_local.transform.position = GorillaLocomotion.Player.Instance.leftHandTransform.position;
                        jump_left_local.transform.rotation = GorillaLocomotion.Player.Instance.leftHandTransform.rotation;

                        object[] left_form_1 = new object[] { GorillaLocomotion.Player.Instance.leftHandTransform.position, GorillaLocomotion.Player.Instance.leftHandTransform.rotation };

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_photoncode, left_form_1, raiseEventOptions, SendOptions.SendReliable);

                        once_left = true;
                        once_left_false = false;
                    }
                }
            }
            else
            {
                if (!once_left_false)
                {
                    if (jump_left_local != null)
                    {
                        GameObject.Destroy(jump_left_local);
                        jump_left_local = null;

                        once_left = false;
                        once_left_false = true;

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_deletion, null, raiseEventOptions, SendOptions.SendReliable);
                    }
                }
            }

            if (!PhotonNetwork.InRoom) //Worst way of doing it, but I'm not even bothered lol.
            {
                for (int i = 0; i < jump_right_network.Length; i++)
                {
                    GameObject.Destroy(jump_right_network[i]);
                }

                for (int i = 0; i < jump_left_network.Length; i++)
                {
                    GameObject.Destroy(jump_left_network[i]);
                }
            }

        }

        public enum PhotonEventCodes //Stores all the Photon codes, if you're making a mod do NOT use these photon values.
        {
            left_jump_photoncode = 69,
            right_jump_photoncode = 70,
            left_jump_deletion = 71,
            right_jump_deletion = 72
        }

        static private void PlatformNetwork(EventData eventData)
        {
            byte eventCode = eventData.Code;
            if (eventCode == (byte)PhotonEventCodes.left_jump_photoncode)
            {
                object[] data_left = (object[])eventData.CustomData; //Array
                jump_left_network[eventData.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jump_left_network[eventData.Sender].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                jump_left_network[eventData.Sender].transform.localScale = scale;
                jump_left_network[eventData.Sender].transform.position = (Vector3)data_left[0]; //Sets position.
                jump_left_network[eventData.Sender].transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
            }
            else if (eventCode == (byte)PhotonEventCodes.right_jump_photoncode)
            {
                object[] data_right = (object[])eventData.CustomData; //Array
                jump_right_network[eventData.Sender] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jump_right_network[eventData.Sender].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                jump_right_network[eventData.Sender].transform.localScale = scale;
                jump_right_network[eventData.Sender].transform.position = (Vector3)data_right[0]; //Sets position.
                jump_right_network[eventData.Sender].transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
            }
            else if (eventCode == (byte)PhotonEventCodes.left_jump_deletion)
            {
                GameObject.Destroy(jump_left_network[eventData.Sender]);
                jump_left_network[eventData.Sender] = null;
            }
            else if (eventCode == (byte)PhotonEventCodes.right_jump_deletion)
            {
                GameObject.Destroy(jump_right_network[eventData.Sender]);
                jump_right_network[eventData.Sender] = null;
            }
        }

        #endregion


        static void AddButton(float offset, string text)
        {
            GameObject newBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(newBtn.GetComponent<Rigidbody>());
            newBtn.GetComponent<BoxCollider>().isTrigger = true;
            newBtn.transform.parent = menu.transform;
            newBtn.transform.rotation = Quaternion.identity;
            newBtn.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            newBtn.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
            newBtn.AddComponent<BtnCollider>().relatedText = text;

            int index = -1;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (text == buttons[i])
                {
                    index = i;
                    break;
                }
            }

            if (buttonsActive[index] == false)
            {
                newBtn.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
            }
            else if (buttonsActive[index] == true)
            {
                newBtn.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
            }
            else
            {
                newBtn.GetComponent<Renderer>().material.SetColor("_Color", Color.grey);
            }

            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;
            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            title.text = text;
            title.fontSize = 1;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.sizeDelta = new Vector2(0.2f, 0.03f);
            titleTransform.localPosition = new Vector3(0.064f, 0f, 0.111f - (offset / 2.55f));
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void Draw()
        {
            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(menu.GetComponent<Rigidbody>());
            GameObject.Destroy(menu.GetComponent<BoxCollider>());
            GameObject.Destroy(menu.GetComponent<Renderer>());

            //menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);

            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(background.GetComponent<Rigidbody>());
            GameObject.Destroy(background.GetComponent<BoxCollider>());
            background.transform.parent = menu.transform;
            background.transform.rotation = Quaternion.identity;

            //background.transform.localScale = new Vector3(0.1f, 1f, 1f);
            background.transform.localScale = new Vector3(0.1f, 1f, 1.25f);

            background.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            background.transform.position = new Vector3(0.05f, 0f, 0f);

            canvasObj = new GameObject();
            canvasObj.transform.parent = menu.transform;
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScale = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScale.dynamicPixelsPerUnit = 1000;

            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;
            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            title.text = "Monke mod menu";
            title.fontSize = 1;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.sizeDelta = new Vector2(0.28f, 0.05f);
            titleTransform.position = new Vector3(0.06f, 0f, 0.175f);
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            for (int i = 0; i < buttons.Length; i++)
            {
                AddButton(i * 0.13f, buttons[i]);
            }
        }

        public static void Toggle(string relatedText)
        {
            int index = -1;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (relatedText == buttons[i])
                {
                    index = i;
                    break;
                }
            }

            if (buttonsActive[index] != null)
            {
                buttonsActive[index] = !buttonsActive[index];

                GameObject.Destroy(menu);
                menu = null;
                Draw();
            }
        }
    }

    class BtnCollider : MonoBehaviour
    {
        public string relatedText;

        private void OnTriggerEnter(Collider collider)
        {
            if (Time.frameCount >= MenuPatch.framePressCooldown + 30)
            {
                MenuPatch.Toggle(relatedText);
                MenuPatch.framePressCooldown = Time.frameCount;
            }
        }
    }
}
