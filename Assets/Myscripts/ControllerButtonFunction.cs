using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using proto.CollectMsg;

public class ControllerButtonFunction : MonoBehaviour
{
    public UnityEngine.XR.InputDevice leftHandDevice;

    private void OnEnable()
    {
        GetLeftHand();
    }

    public void GetLeftHand()
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

    public void VibrationTrigger()
    {
        UnityEngine.XR.HapticCapabilities capabilities;
        if(leftHandDevice.TryGetHapticCapabilities(out capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                float amplitude = 0.8f;
                uint channel = 0;
                float duration = 0.2f;
                leftHandDevice.SendHapticImpulse(channel, amplitude, duration);
            }
        }
    }

    public void SendTimestamp()
    {
        //按下按钮发送时间戳到服务器
        MsgTimeUpByUser msg = new MsgTimeUpByUser();
        string id = PhotonNetwork.LocalPlayer.NickName;
        long timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        msg.userid = id;
        msg.timestamp = timestamp;
        //Debug.Log(id + timestamp.ToString());
        NetManager.Send(msg);
        VibrationTrigger();
    }
}
