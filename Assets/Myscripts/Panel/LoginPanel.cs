using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using VRUiKits.Utils;


public class LoginPanel : BasePanel
{
    private GameObject keyboard;
    private UIKitInputField idInput;
    private UIKitInputField accountInput;
    private InvokeKeyboard idInputKeyboard;
    private InvokeKeyboard accountInputKeyboard;
    private Button loginBtn;
    private Button regBtn;
    private Button hideBtn;
    private UIKitInputField activeInput;
    private const string TargetSceneName = "OfficeLoggedIn";
    private const string TargetScenePath = "Assets/VR Office/Scenes/OfficeLoggedIn.unity";

    //初始化
    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer = PanelManager.Layer.Panel;
        keyboard = GameObject.Find("Manager").GetComponent<LobbyMain>().keyboard;
    }

    public override void OnShow(params object[] args)
    {
        //寻找组件
        idInput = skin.transform.Find("Contents/UserNameInputField").GetComponent<UIKitInputField>();
        accountInput = skin.transform.Find("Contents/PasswordInputField").GetComponent<UIKitInputField>();
        idInputKeyboard = skin.transform.Find("Contents/UserNameInputField").GetComponent<InvokeKeyboard>();
        accountInputKeyboard = skin.transform.Find("Contents/PasswordInputField").GetComponent<InvokeKeyboard>();
        loginBtn = skin.transform.Find("Contents/LoginButton").GetComponent<Button>();
        regBtn = skin.transform.Find("Contents/RegisterButton").GetComponent<Button>();
        hideBtn = skin.transform.Find("Contents/HideButton").GetComponent<Button>();

        //指定keyboard
        idInputKeyboard.keyboard = keyboard;
        accountInputKeyboard.keyboard = keyboard;

        idInput.text = "";
        accountInput.text = "";
        accountInput.contentType = UIKitInputField.ContentType.Standard;
        SetButtonText(loginBtn, "ENTER");
        regBtn.gameObject.SetActive(false);

        //监听
        loginBtn.onClick.AddListener(OnLoginClick);
        hideBtn.onClick.AddListener(OnHideClick);

        SelectInput(idInput);
    }

    public override void OnClose()
    {
    }

    private void Update()
    {
        if (idInput == null || accountInput == null)
        {
            return;
        }

        UpdateActiveInputFromEventSystem();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SelectInput(activeInput == idInput ? accountInput : idInput);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitLogin("hardware enter");
            return;
        }

        UIKitInputField target = activeInput ?? idInput;
        if (ConsumeVirtualKeyboardSubmit(target))
        {
            SubmitLogin("virtual keyboard enter");
            return;
        }

        string input = Input.inputString;
        for (int i = 0; i < input.Length; i++)
        {
            char character = input[i];
            if (character == '\b')
            {
                if (target.text.Length > 0)
                {
                    target.text = target.text.Substring(0, target.text.Length - 1);
                    target.ForceCaretUpdate();
                }
            }
            else if (character != '\n' && character != '\r' && !char.IsControl(character))
            {
                target.text += character;
                target.ForceCaretUpdate();
            }
        }
    }

    //当按下成功按钮
    public void OnLoginClick()
    {
        SubmitLogin("login button");
    }

    private void SubmitLogin(string source)
    {
        //用户名密码为空
        string userName = idInput.text.Trim();
        string account = accountInput.text.Trim();
        Debug.Log($"Login submit from {source}. UserName='{userName}', Account='{account}'");

        if (userName == "" || account == "")
        {
            PanelManager.Open<TipPanel>("User name and account cannot be empty!");
            Debug.LogWarning("Login blocked because user name or account is empty.");
            return;
        }

        if (keyboard != null)
        {
            keyboard.SetActive(false);
        }

        LobbyMain.playerId = userName;
        PlayerPrefs.SetString("LobbyUserName", userName);
        PlayerPrefs.SetString("LobbyAccount", account);
        PlayerPrefs.Save();

        LoadTargetScene();
    }

    //当按下注册按钮
    public void OnRegClick()
    {
        keyboard.SetActive(false);
        PanelManager.Open<RegisterPanel>();
        this.Close();
    }

    public void OnHideClick()
    {
        keyboard.SetActive(false);
    }

    private void SelectInput(UIKitInputField input)
    {
        activeInput = input;
        MobileKeyboardManager.Target = input;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(input.gameObject);
        }
    }

    private bool ConsumeVirtualKeyboardSubmit(UIKitInputField input)
    {
        if (input == null || string.IsNullOrEmpty(input.text))
        {
            return false;
        }

        string[] submitTokens = { "ENTER", "Enter", "enter", "RETURN", "Return", "return" };
        for (int i = 0; i < submitTokens.Length; i++)
        {
            string token = submitTokens[i];
            if (input.text.EndsWith(token))
            {
                input.text = input.text.Substring(0, input.text.Length - token.Length);
                input.ForceCaretUpdate();
                return true;
            }
        }

        return false;
    }

    private void LoadTargetScene()
    {
        bool canLoadByPath = Application.CanStreamedLevelBeLoaded(TargetScenePath);
        bool canLoadByName = Application.CanStreamedLevelBeLoaded(TargetSceneName);

        if (canLoadByPath)
        {
            Debug.Log($"Loading scene by path: {TargetScenePath}");
            SceneManager.LoadScene(TargetScenePath);
            return;
        }

        if (canLoadByName)
        {
            Debug.Log($"Loading scene by name: {TargetSceneName}");
            SceneManager.LoadScene(TargetSceneName);
            return;
        }

        string error = $"Cannot load scene '{TargetSceneName}'. Please make sure {TargetScenePath} is enabled in Build Settings.";
        Debug.LogError(error);
        PanelManager.Open<TipPanel>(error);
    }

    private void UpdateActiveInputFromEventSystem()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            return;
        }

        UIKitInputField selectedInput = EventSystem.current.currentSelectedGameObject.GetComponent<UIKitInputField>();
        if (selectedInput != null)
        {
            activeInput = selectedInput;
            MobileKeyboardManager.Target = selectedInput;
        }
    }

    private void SetButtonText(Button button, string text)
    {
        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = text;
        }
    }
}
