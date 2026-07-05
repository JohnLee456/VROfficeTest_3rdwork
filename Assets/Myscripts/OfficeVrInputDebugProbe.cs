using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using XRCommonUsages = UnityEngine.XR.CommonUsages;
using XRInputDevice = UnityEngine.XR.InputDevice;

public class OfficeVrInputDebugProbe : MonoBehaviour
{
    private const string ProbeName = "Office VR Input Debug Probe";
    private const float PressedThreshold = 0.5f;

    private static readonly List<XRInputDevice> XrDevices = new List<XRInputDevice>();

    private InputAction genericPrimaryAction;
    private InputAction genericSecondaryAction;
    private InputAction oculusButtonSouthAction;
    private InputAction oculusButtonEastAction;

    private bool previousAnyPrimary;
    private bool previousAnySecondary;
    private string previousPressedControls = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (!scene.IsValid() || scene.name != OfficeSceneSupport.OfficeLoggedIn)
        {
            return;
        }

        if (FindObjectOfType<OfficeVrInputDebugProbe>() != null)
        {
            return;
        }

        GameObject probe = new GameObject(ProbeName);
        probe.AddComponent<OfficeVrInputDebugProbe>();
    }

    private void OnEnable()
    {
        genericPrimaryAction = CreateButtonAction("Office Debug Right Generic Primary", "<XRController>{RightHand}/primaryButton");
        genericSecondaryAction = CreateButtonAction("Office Debug Right Generic Secondary", "<XRController>{RightHand}/secondaryButton");
        oculusButtonSouthAction = CreateButtonAction("Office Debug Right Oculus Button South", "<OculusTouchController>{RightHand}/buttonSouth");
        oculusButtonEastAction = CreateButtonAction("Office Debug Right Oculus Button East", "<OculusTouchController>{RightHand}/buttonEast");

        Debug.Log("OfficeVrInputDebugProbe active. Press right-hand A/B in OfficeLoggedIn to log XR and Input System button states.", this);
    }

    private void OnDisable()
    {
        DisposeAction(ref genericPrimaryAction);
        DisposeAction(ref genericSecondaryAction);
        DisposeAction(ref oculusButtonSouthAction);
        DisposeAction(ref oculusButtonEastAction);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != OfficeSceneSupport.OfficeLoggedIn)
        {
            return;
        }

        bool xrNodePrimary = TryGetXrNodeButton(XRCommonUsages.primaryButton, out string xrNodeDeviceName);
        bool xrNodeSecondary = TryGetXrNodeButton(XRCommonUsages.secondaryButton, out _);
        bool xrAnyPrimary = TryGetAnyRightHandButton(XRCommonUsages.primaryButton, out string xrAnyDeviceName);
        bool xrAnySecondary = TryGetAnyRightHandButton(XRCommonUsages.secondaryButton, out _);

        float genericPrimaryValue = ReadActionValue(genericPrimaryAction);
        float genericSecondaryValue = ReadActionValue(genericSecondaryAction);
        float oculusSouthValue = ReadActionValue(oculusButtonSouthAction);
        float oculusEastValue = ReadActionValue(oculusButtonEastAction);

        bool anyPrimary = xrNodePrimary ||
            xrAnyPrimary ||
            genericPrimaryValue > PressedThreshold ||
            oculusSouthValue > PressedThreshold;
        bool anySecondary = xrNodeSecondary ||
            xrAnySecondary ||
            genericSecondaryValue > PressedThreshold ||
            oculusEastValue > PressedThreshold;
        string pressedControls = GetPressedRightHandButtonControls();

        if (anyPrimary != previousAnyPrimary ||
            anySecondary != previousAnySecondary ||
            pressedControls != previousPressedControls)
        {
            Debug.Log(BuildLogMessage(
                xrNodeDeviceName,
                xrAnyDeviceName,
                xrNodePrimary,
                xrNodeSecondary,
                xrAnyPrimary,
                xrAnySecondary,
                genericPrimaryValue,
                genericSecondaryValue,
                oculusSouthValue,
                oculusEastValue,
                pressedControls), this);

            previousAnyPrimary = anyPrimary;
            previousAnySecondary = anySecondary;
            previousPressedControls = pressedControls;
        }
    }

    private static InputAction CreateButtonAction(string actionName, string binding)
    {
        InputAction action = new InputAction(actionName, InputActionType.Button);
        action.AddBinding(binding);
        action.Enable();
        return action;
    }

    private static void DisposeAction(ref InputAction action)
    {
        if (action == null)
        {
            return;
        }

        action.Disable();
        action.Dispose();
        action = null;
    }

    private static float ReadActionValue(InputAction action)
    {
        return action != null ? action.ReadValue<float>() : 0f;
    }

    private static bool TryGetXrNodeButton(InputFeatureUsage<bool> usage, out string deviceName)
    {
        XRInputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        deviceName = device.isValid ? device.name : "invalid";
        return device.isValid &&
            device.TryGetFeatureValue(usage, out bool pressed) &&
            pressed;
    }

    private static bool TryGetAnyRightHandButton(InputFeatureUsage<bool> usage, out string deviceName)
    {
        InputDeviceCharacteristics characteristics =
            InputDeviceCharacteristics.HeldInHand |
            InputDeviceCharacteristics.Controller |
            InputDeviceCharacteristics.Right;

        XrDevices.Clear();
        InputDevices.GetDevicesWithCharacteristics(characteristics, XrDevices);

        for (int i = 0; i < XrDevices.Count; i++)
        {
            XRInputDevice device = XrDevices[i];
            if (device.isValid &&
                device.TryGetFeatureValue(usage, out bool pressed) &&
                pressed)
            {
                deviceName = device.name;
                return true;
            }
        }

        deviceName = XrDevices.Count > 0 ? XrDevices[0].name : "none";
        return false;
    }

    private static string GetPressedRightHandButtonControls()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            UnityEngine.InputSystem.InputDevice device = InputSystem.devices[i];
            if (!IsLikelyRightHandDevice(device))
            {
                continue;
            }

            foreach (InputControl control in device.allControls)
            {
                ButtonControl button = control as ButtonControl;
                if (button == null || !button.isPressed)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(device.layout);
                builder.Append(":");
                builder.Append(button.path);
            }
        }

        return builder.Length > 0 ? builder.ToString() : "none";
    }

    private static bool IsLikelyRightHandDevice(UnityEngine.InputSystem.InputDevice device)
    {
        string text = (device.name + " " + device.displayName + " " + device.layout + " " + device.description).ToLowerInvariant();
        return text.Contains("right") &&
            (text.Contains("xr") || text.Contains("oculus") || text.Contains("touch") || text.Contains("controller"));
    }

    private static string BuildLogMessage(
        string xrNodeDeviceName,
        string xrAnyDeviceName,
        bool xrNodePrimary,
        bool xrNodeSecondary,
        bool xrAnyPrimary,
        bool xrAnySecondary,
        float genericPrimaryValue,
        float genericSecondaryValue,
        float oculusSouthValue,
        float oculusEastValue,
        string pressedControls)
    {
        return "OfficeVrInputDebugProbe RightHand A/B state | " +
            $"XRNode device={xrNodeDeviceName}, primary={xrNodePrimary}, secondary={xrNodeSecondary}; " +
            $"XR right-hand device={xrAnyDeviceName}, primary={xrAnyPrimary}, secondary={xrAnySecondary}; " +
            $"InputAction generic primary={genericPrimaryValue:0.00}, generic secondary={genericSecondaryValue:0.00}, " +
            $"oculus buttonSouth={oculusSouthValue:0.00}, oculus buttonEast={oculusEastValue:0.00}; " +
            $"pressed InputSystem controls={pressedControls}";
    }
}
