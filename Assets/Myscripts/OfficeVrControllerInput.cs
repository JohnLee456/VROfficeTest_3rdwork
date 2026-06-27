using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using XRCommonUsages = UnityEngine.XR.CommonUsages;
using XRInputDevice = UnityEngine.XR.InputDevice;

public static class OfficeVrControllerInput
{
    private static readonly List<XRInputDevice> Devices = new List<XRInputDevice>();

    private static InputAction leftPrimaryButtonAction;
    private static InputAction leftSecondaryButtonAction;
    private static InputAction rightPrimaryButtonAction;
    private static InputAction rightSecondaryButtonAction;
    private static bool actionsInitialized;

    private static bool wasXPressed;
    private static bool wasYPressed;
    private static bool wasAPressed;
    private static bool wasBPressed;
    private static bool xPressedThisFrame;
    private static bool yPressedThisFrame;
    private static bool aPressedThisFrame;
    private static bool bPressedThisFrame;
    private static int sampledFrame = -1;

    public static bool GetXDown()
    {
        SampleButtons();
        return xPressedThisFrame;
    }

    public static bool GetYDown()
    {
        SampleButtons();
        return yPressedThisFrame;
    }

    public static bool GetADown()
    {
        SampleButtons();
        return aPressedThisFrame;
    }

    public static bool GetBDown()
    {
        SampleButtons();
        return bPressedThisFrame;
    }

    private static void SampleButtons()
    {
        if (sampledFrame == Time.frameCount)
        {
            return;
        }

        sampledFrame = Time.frameCount;
        EnsureInputActions();

        string sceneName = SceneManager.GetActiveScene().name;
        bool shouldReadLeftControllerUiButtons = OfficeSceneSupport.ShouldShowRuntimeUi(sceneName);
        if (!shouldReadLeftControllerUiButtons)
        {
            wasXPressed = false;
            wasYPressed = false;
            xPressedThisFrame = false;
            yPressedThisFrame = false;
        }

        bool shouldReadRightControllerButtons = sceneName == OfficeSceneSupport.OfficeLoggedIn ||
            sceneName == OfficeSceneSupport.OfficeLoggedInNoBot;
        if (!shouldReadRightControllerButtons)
        {
            wasAPressed = false;
            wasBPressed = false;
            aPressedThisFrame = false;
            bPressedThisFrame = false;
        }

        if (shouldReadLeftControllerUiButtons)
        {
            bool isXPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Left, XRNode.LeftHand, XRCommonUsages.primaryButton, leftPrimaryButtonAction);
            bool isYPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Left, XRNode.LeftHand, XRCommonUsages.secondaryButton, leftSecondaryButtonAction);

            xPressedThisFrame = isXPressed && !wasXPressed;
            yPressedThisFrame = isYPressed && !wasYPressed;

            wasXPressed = isXPressed;
            wasYPressed = isYPressed;
        }

        if (shouldReadRightControllerButtons)
        {
            bool isAPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Right, XRNode.RightHand, XRCommonUsages.primaryButton, rightPrimaryButtonAction);
            bool isBPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Right, XRNode.RightHand, XRCommonUsages.secondaryButton, rightSecondaryButtonAction);

            aPressedThisFrame = isAPressed && !wasAPressed;
            bPressedThisFrame = isBPressed && !wasBPressed;
            wasAPressed = isAPressed;
            wasBPressed = isBPressed;
        }
    }

    private static void EnsureInputActions()
    {
        if (actionsInitialized)
        {
            return;
        }

        leftPrimaryButtonAction = CreateButtonAction("Office Left Primary Button", "<XRController>{LeftHand}/primaryButton", "<OculusTouchController>{LeftHand}/buttonWest");
        leftSecondaryButtonAction = CreateButtonAction("Office Left Secondary Button", "<XRController>{LeftHand}/secondaryButton", "<OculusTouchController>{LeftHand}/buttonNorth");
        rightPrimaryButtonAction = CreateButtonAction("Office Right Primary Button", "<XRController>{RightHand}/primaryButton", "<OculusTouchController>{RightHand}/buttonSouth");
        rightSecondaryButtonAction = CreateButtonAction("Office Right Secondary Button", "<XRController>{RightHand}/secondaryButton", "<OculusTouchController>{RightHand}/buttonEast");
        actionsInitialized = true;
    }

    private static InputAction CreateButtonAction(string actionName, string genericBinding, string oculusBinding)
    {
        InputAction action = new InputAction(actionName, InputActionType.Button);
        action.AddBinding(genericBinding);
        action.AddBinding(oculusBinding);
        action.Enable();
        return action;
    }

    private static bool IsControllerButtonPressed(InputDeviceCharacteristics handedness, XRNode node, InputFeatureUsage<bool> button, InputAction action)
    {
        EnsureInputActions();

        if (action != null && action.ReadValue<float>() > 0.5f)
        {
            return true;
        }

        XRInputDevice nodeDevice = InputDevices.GetDeviceAtXRNode(node);
        if (nodeDevice.isValid &&
            nodeDevice.TryGetFeatureValue(button, out bool nodePressed) &&
            nodePressed)
        {
            return true;
        }

        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.HeldInHand |
            InputDeviceCharacteristics.Controller |
            handedness;

        Devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(characteristics, Devices);

        for (int i = 0; i < Devices.Count; i++)
        {
            XRInputDevice device = Devices[i];
            if (device.isValid &&
                device.TryGetFeatureValue(button, out bool isPressed) &&
                isPressed)
            {
                return true;
            }
        }

        return false;
    }
}
