using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRUiKits.Utils;

public class SettingPanel : BasePanel
{
    private Button CloseBtn;
    private Button ApplyBtn;
    public DataCollector datacollector;
    private OptionsManager optionsManager;
    private SliderProgressBar sliderProgressBar;
    private string selectedValue;
    private float value;

    public override void OnInit()
    {
        skinPath = "SettingPanel";
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] args)
    {
        CloseBtn = skin.transform.Find("Contents/CloseBtn").GetComponent<Button>();
        ApplyBtn = skin.transform.Find("Contents/ApplyBtn").GetComponent<Button>();
        datacollector = skin.transform.Find("/VRRigDeviceBased").GetComponent<DataCollector>();
        optionsManager = skin.transform.Find("Contents/OptionsWithLabel").GetComponent<OptionsManager>();
        sliderProgressBar = skin.transform.Find("Contents/AdjustableBarWithLabel-L1").GetComponent<SliderProgressBar>();

        //获取当前Interval
        sliderProgressBar.Value = datacollector.GetInterval();
        if (datacollector.GetCollectType() == "Simple")
        {
            optionsManager.firstSelectedIndex = 0;
        }
        else
        {
            optionsManager.firstSelectedIndex = 1;
        }
        CloseBtn.onClick.AddListener(OnCloseClick);
        ApplyBtn.onClick.AddListener(OnApplyClick);
    }

    public void OnCloseClick()
    {
        Close();
    }

    public void OnApplyClick()
    {
        //获得当前设定的值
        string selectedValue = optionsManager.selectedValue;
        Debug.Log("selected Value: " + selectedValue);

        float value = sliderProgressBar.Value;
        Debug.Log("Bar Value: " + value);

        //调用datacollector中更改参数的函数
        datacollector.ChangeInterval(value);

        datacollector.ChangeCollectType(selectedValue);

        PanelManager.Open<TipPanel>("Apply Successfully!");
    }
}
