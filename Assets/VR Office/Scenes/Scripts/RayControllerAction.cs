using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace ChiliGames.VROffice
{
    public class RayControllerAction : MonoBehaviour
    {
        public ActionBasedController leftTeleportRay;
        public ActionBasedController rightTeleportRay;
        public float activationThreshold = 0.1f;
        public bool leftTeleportEnabled { get; set; } = true;
        public bool rightTeleportEnabled { get; set; } = true;
        private bool leftButtonPressedLastFrame = false;
        private bool rightButtonPressedLastFrame = false;
        public GameObject leftTeleportReticle;
        public GameObject rightTeleportReticle;

        private static readonly List<InputDevice> InputDevicesBuffer = new List<InputDevice>();

        void Start()
        {
            InitializeTeleportRay(leftTeleportRay);
            InitializeTeleportRay(rightTeleportRay);
        }

        void InitializeTeleportRay(ActionBasedController teleportRay)
        {
            if (!teleportRay) { return; }

            teleportRay.gameObject.SetActive(false);
        }

        void Update()
        {
            ManageTeleportRay(leftTeleportRay, XRNode.LeftHand, ref leftButtonPressedLastFrame, leftTeleportReticle, leftTeleportEnabled);
            ManageTeleportRay(rightTeleportRay, XRNode.RightHand, ref rightButtonPressedLastFrame, rightTeleportReticle, rightTeleportEnabled);
        }

        void ManageTeleportRay(ActionBasedController teleportRay, XRNode controllerNode, ref bool buttonPressedLastFrame, GameObject teleportReticle, bool teleportEnabled)
        {
            if (!teleportRay) { return; }

            bool isPressed = IsPrimaryThumbstickClicked(controllerNode);
            bool buttonJustPressed = isPressed && !buttonPressedLastFrame;

            if (!teleportEnabled)
            {
                if (teleportRay.gameObject.activeSelf)
                {
                    SetActiveNextFrame(teleportRay.gameObject, false);
                }

                buttonPressedLastFrame = isPressed;
                return;
            }

            if (buttonJustPressed)
            {
                bool shouldActivate = !teleportRay.gameObject.activeSelf;
                if (shouldActivate)
                {
                    teleportRay.gameObject.SetActive(true);
                    if (teleportReticle != null)
                    {
                        // This stops the reticle from appearing by the player's feet for 1 frame every time the teleport ray is activated.
                        teleportReticle.SetActive(false);
                    }
                }
                else
                {
                    // If we disable this object this frame, then the teleport will not work, so do it next frame.
                    SetActiveNextFrame(teleportRay.gameObject, false);
                }
            }

            buttonPressedLastFrame = isPressed;
        }

        private static bool IsPrimaryThumbstickClicked(XRNode node)
        {
            InputDevice nodeDevice = InputDevices.GetDeviceAtXRNode(node);
            if (nodeDevice.isValid &&
                nodeDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool nodeClicked) &&
                nodeClicked)
            {
                return true;
            }

            InputDeviceCharacteristics handedness = node == XRNode.LeftHand
                ? InputDeviceCharacteristics.Left
                : InputDeviceCharacteristics.Right;
            InputDeviceCharacteristics characteristics =
                InputDeviceCharacteristics.HeldInHand |
                InputDeviceCharacteristics.Controller |
                handedness;

            InputDevicesBuffer.Clear();
            InputDevices.GetDevicesWithCharacteristics(characteristics, InputDevicesBuffer);
            for (int i = 0; i < InputDevicesBuffer.Count; i++)
            {
                InputDevice device = InputDevicesBuffer[i];
                if (device.isValid &&
                    device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool isClicked) &&
                    isClicked)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetActiveNextFrame(GameObject gameObject, bool active)
        {
            StartCoroutine(SetActiveNextFrameHelper(gameObject, active));
        }

        IEnumerator SetActiveNextFrameHelper(GameObject gameObject, bool active)
        {
            yield return null;
            gameObject.SetActive(active);
        }
    }
}
