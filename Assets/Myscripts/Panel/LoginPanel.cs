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
        accountInput.gameObject.SetActive(false);
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
            SelectInput(idInput);
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
        string account = idInput.text.Trim();
        Debug.Log($"Login submit from {source}. Account='{account}'");

        if (!LoginAccountResolver.TryResolve(account, out LoginRoute route))
        {
            PanelManager.Open<TipPanel>("Account not found!");
            Debug.LogWarning($"Login blocked because account '{account}' was not found.");
            return;
        }

        if (keyboard != null)
        {
            keyboard.SetActive(false);
        }

        LoginSession.Apply(route);
        Debug.Log($"Login resolved account '{account}' to scene '{route.SceneName}', avatar '{route.AvatarName}'.");

        LobbyMain.playerId = route.AvatarName;
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
        string sceneName = LoginSceneTarget.SceneName;
        bool canLoadByName = Application.CanStreamedLevelBeLoaded(sceneName);

        if (canLoadByName)
        {
            LoginSceneTarget.Load();
            return;
        }

        string error = $"Cannot load scene '{sceneName}'. Please make sure it is enabled in Build Settings.";
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
