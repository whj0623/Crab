using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Photon.Realtime;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public FirebaseApp App { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseDatabase DB { get; private set; }
    public bool IsInitialized { get; private set; } = false;
    public DatabaseReference usersRef;
    internal UserData userData;
    public event Action<FirebaseUser> onLogin;
    public string userName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;
            DB = FirebaseDatabase.DefaultInstance;
            App = FirebaseApp.DefaultInstance;
            IsInitialized = true;
        }
        else
            Debug.LogWarning($"{status}");
    }

    public async void Login(string email, string pw, Action<FirebaseUser> callback = null,Action<Exception> errorCallback = null)
    {
        try
        {
            var result = await Auth.SignInWithEmailAndPasswordAsync(email, pw);
            FirebaseDatabase.GetInstance(App, "https://myfirebasetest-e49d0-default-rtdb.firebaseio.com/");
            usersRef = DB.GetReference($"users/{result.User.UserId}");
            if (usersRef == null)
            {
                Debug.LogError("DB.GetReference 호출 후 usersRef가 null입니다.");
                return;
            }
            DataSnapshot userDataValues = await usersRef.GetValueAsync();

            if (userDataValues.Exists)
            {
                var userName = userDataValues.Child("userName");
                if (userName.Exists)
                    this.userName = (string)userName.GetValue(false);
            }
            onLogin?.Invoke(result.User);
            callback?.Invoke(result.User);
        }
        catch (Exception ex)
        {
            errorCallback?.Invoke(ex);
        }

    }
    
    public async void Signup(string email, string pw, string Nickname, Action<FirebaseUser> callback = null)
    {
        try
        {
            var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, pw);
            usersRef = DB.GetReference($"users/{result.User.UserId}");
            UserData userData = new UserData(result.User.UserId,Nickname);
            string userDataJson = JsonConvert.SerializeObject(userData);
            await usersRef.SetRawJsonValueAsync(userDataJson);
            this.userData = userData;
            onLogin?.Invoke(result.User);
            callback?.Invoke(result.User);
        }
        catch (FirebaseException fe)
        {
            PanelManager.Instance.Dialog(fe.ToString());
        }
    }
}
