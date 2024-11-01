using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class LoginPanel : MonoBehaviour
{
    public InputField emailInput;
    public InputField pwInput;
    
    public Button loginButton;
    public Button signUpUIOpenButton;
   
    
    [Header("회원가입 UI")]
    public GameObject signupUI;
    public InputField emailInputForSignup;
    public InputField pwInputForSignUp;
    public InputField confirmpwInput;
    public InputField nickNameInput;
    public Button cancelButton;
    public Button signUpButton;


    private void Awake()
    {
        loginButton.onClick.AddListener(LoginButtonClick); 
        signUpUIOpenButton.onClick.AddListener(signUpUIOpenButtonClick);
        signUpButton.onClick.AddListener(SignUpButtonClick);
        cancelButton.onClick.AddListener(CancelButtonClick);
    }

    

    private IEnumerator Start()
    {
        emailInput.interactable = false;
        pwInput.interactable = false;
        signUpButton.interactable = false;
        loginButton.interactable = false;

        yield return new WaitUntil(() => FirebaseManager.Instance.IsInitialized); 

        emailInput.interactable = true;
        pwInput.interactable = true;
        signUpButton.interactable = true;
        loginButton.interactable = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == emailInput.gameObject)
                EventSystem.current.SetSelectedGameObject(pwInput.gameObject);
        }
        if (Input.GetKeyDown(KeyCode.Return))
            LoginButtonClick();
    }

    private void OnEnable()
    {
        emailInput.interactable = true;
        loginButton.interactable = true;
    }

    public void LoginButtonClick()
    {
        loginButton.interactable = false;
        PanelManager.Instance.Dialog("로그인 중입니다...");
        FirebaseManager.Instance.Login(emailInput.text, pwInput.text, (user) =>
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.LocalPlayer.NickName = FirebaseManager.Instance.userName;
            loginButton.interactable = true;
            
        },
        (error) =>
        {
            PanelManager.Instance.Dialog("이메일 혹은 비밀번호를 확인해주세요.");
            loginButton.interactable = true;
        });
    }

    private void signUpUIOpenButtonClick()
    {
        signupUI.SetActive(true);
    }

   
    public void SignUpButtonClick()
    {
        signUpButton.interactable = false;
        if (!pwInputForSignUp.text.Equals(confirmpwInput.text))
        {
            PanelManager.Instance.Dialog("비밀번호 재입력이 비밀번호와 일치하지 않습니다.");
            pwInputForSignUp.text = string.Empty;
            confirmpwInput.text = string.Empty;
            signUpButton.interactable = true;
            return;
        }
        FirebaseManager.Instance.Signup(emailInputForSignup.text, pwInputForSignUp.text,nickNameInput.text, (user) =>
        {
            signUpButton.interactable = true;
            PanelManager.Instance.Dialog("회원가입이 완료되었습니다.");
            signupUI.SetActive(false);
        });
    }


    private void CancelButtonClick()
    {
        emailInputForSignup.text = string.Empty;
        pwInputForSignUp.text = string.Empty;
        confirmpwInput.text = string.Empty;
        nickNameInput.text = string.Empty;
        signupUI.SetActive(false);
    }
}
