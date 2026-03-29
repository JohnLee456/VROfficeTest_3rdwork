using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRInputRecord : MonoBehaviour
{
    public UnityEngine.XR.InputDevice leftHandDevice;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        getLeftHand();
    }

    public void allInputDeviceRecord()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach(var deviceA in inputDevices)
        {
            Debug.Log(string.Format("device name '{0}' device role '{1}'", deviceA.name, deviceA.characteristics.ToString()));
        }
    }

    public void getLeftHand()
    {
        var leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);

        foreach (var device in leftHandedControllers)
        {
            Debug.Log(string.Format("left device name '{0}'left device role '{1}'", device.name, device.characteristics.ToString()));
            leftHandDevice = device;
        }

    }

    public void updateInputDynamicRecord()
    {
        bool triggerValue;
        if(leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out triggerValue) && triggerValue)
        {
            Debug.Log("ˆÂ‰ºprimaryButtton");
        }

        else if (leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out triggerValue) && triggerValue)
        {
            Debug.Log("ˆÂ‰ºsecondaryButton");
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateInputDynamicRecord();
    }
}
