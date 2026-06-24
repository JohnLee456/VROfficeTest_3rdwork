using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public static class OfficeVrControllerInput
{
    private static readonly List<InputDevice> Devices = new List<InputDevice>();

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

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != OfficeSceneSupport.OfficeLoggedIn)
        {
            wasXPressed = false;
            wasYPressed = false;
            xPressedThisFrame = false;
            yPressedThisFrame = false;
        }

        if (sceneName != OfficeSceneSupport.OfficeLoggedInNoBot)
        {
            wasAPressed = false;
            wasBPressed = false;
            aPressedThisFrame = false;
            bPressedThisFrame = false;
        }

        if (sceneName == OfficeSceneSupport.OfficeLoggedIn)
        {
            bool isXPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Left, CommonUsages.primaryButton);
            bool isYPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Left, CommonUsages.secondaryButton);

            xPressedThisFrame = isXPressed && !wasXPressed;
            yPressedThisFrame = isYPressed && !wasYPressed;

            wasXPressed = isXPressed;
            wasYPressed = isYPressed;
        }

        if (sceneName == OfficeSceneSupport.OfficeLoggedInNoBot)
        {
            bool isAPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Right, CommonUsages.primaryButton);
            bool isBPressed = IsControllerButtonPressed(InputDeviceCharacteristics.Right, CommonUsages.secondaryButton);

            aPressedThisFrame = isAPressed && !wasAPressed;
            bPressedThisFrame = isBPressed && !wasBPressed;
            wasAPressed = isAPressed;
            wasBPressed = isBPressed;
        }
    }

    private static bool IsControllerButtonPressed(InputDeviceCharacteristics handedness, InputFeatureUsage<bool> button)
    {
        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.HeldInHand |
            InputDeviceCharacteristics.Controller |
            handedness;

        Devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(characteristics, Devices);

        for (int i = 0; i < Devices.Count; i++)
        {
            InputDevice device = Devices[i];
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
