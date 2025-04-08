
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using Firebase.Extensions;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private bool isFirebaseReady = false;
    public FirebaseAuth Auth => auth;

    void Awake()
    {
        Debug.Log(" FirebaseAuthManager Awake() �����");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log(" FirebaseAuthManager Singleton ��� �Ϸ�");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnFirebaseInitialized()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        isFirebaseReady = true;

        Debug.Log(" OnFirebaseInitialized ȣ���, Firebase �غ� �Ϸ�!");
    }
    public bool IsLoggedIn()
{
    return auth != null && auth.CurrentUser != null;
}

    public void SignUp(string email, string password, Action<bool, string> callback)
    {
        Debug.Log($" SignUp ȣ��� - isFirebaseReady = {isFirebaseReady}");

        if (!isFirebaseReady)
        {
            Debug.LogError(" Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            callback(false, "Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError(" �̸��� �Ǵ� ��й�ȣ�� ��� �ֽ��ϴ�.");
            callback(false, "�̸��� �Ǵ� ��й�ȣ�� ��� �ֽ��ϴ�.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser newUser = task.Result.User;
                Debug.Log($" ȸ������ ����: {newUser.Email} (UID: {newUser.UserId})");

                string fullEmail = newUser.Email;
                string uid = newUser.UserId;
                string username = fullEmail.Split('@')[0];

                Dictionary<string, object> userData = new Dictionary<string, object>()
                {
                    { "userId", fullEmail },
                    { "username", username }
                };

                firestore.Collection("users").Document(uid).SetAsync(userData).ContinueWithOnMainThread(docTask =>
                {
                    if (docTask.IsCompletedSuccessfully)
                        Debug.Log($" Firestore�� ����� ���� ���� �Ϸ�: {fullEmail}");
                    else
                        Debug.LogError($" Firestore ���� ����: {docTask.Exception?.Message}");
                });
                Dictionary<string, object> goodsData = new Dictionary<string, object>()
                {
                    { "storybook page", 0 },
                    { "machine parts", 0 }
                };

                firestore.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(goodsTask =>
                {
                    if (goodsTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"�⺻ goods ������ ���� �Ϸ� for UID: {uid}");
                    }
                    else
                    {
                        Debug.LogError($"goods ���� ����: {goodsTask.Exception?.Message}");
                    }
                });

                callback(true, "ȸ������ ����");
            }
            else
            {
                if (task.Exception != null)
                {
                    Exception innerEx = task.Exception.Flatten().InnerExceptions[0];
                    FirebaseException fbEx = innerEx as FirebaseException;
                    if (fbEx != null)
                    {
                        var errorCode = (AuthError)fbEx.ErrorCode;
                        switch (errorCode)
                        {
                            case AuthError.EmailAlreadyInUse:
                                Debug.LogError(" �̹� ��� ���� �̸����Դϴ�.");
                                callback(false, "�̹� ��� ���� �̸����Դϴ�.");
                                break;
                            case AuthError.WeakPassword:
                                Debug.LogError(" ��й�ȣ�� �ʹ� ���մϴ�.");
                                callback(false, "��й�ȣ�� �ʹ� ���մϴ�.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError(" �̸��� ������ �߸��Ǿ����ϴ�.");
                                callback(false, "�̸��� ������ �߸��Ǿ����ϴ�.");
                                break;
                            default:
                                Debug.LogError($" ��Ÿ ȸ������ ����: {errorCode} - {fbEx.Message}");
                                callback(false, fbEx.Message);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($" ȸ������ �� �� �� ���� ����: {task.Exception.Message}");
                        callback(false, task.Exception.Message);
                    }
                }
            }
        });
    }

    public void Login(string email, string password, Action<bool, string> callback)
    {
        if (!isFirebaseReady)
        {
            Debug.LogError(" Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            callback(false, "Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError(" �̸��� �Ǵ� ��й�ȣ�� ��� �ֽ��ϴ�.");
            callback(false, "�̸��� �Ǵ� ��й�ȣ�� ��� �ֽ��ϴ�.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log($" �α��� ����: {user.Email} (UID: {user.UserId})");
                string uid = user.UserId;

                firestore.Collection("goods").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(goodsTask =>
                {
                    if (goodsTask.IsCompletedSuccessfully)
                    {
                        DocumentSnapshot snapshot = goodsTask.Result;
                        if (snapshot.Exists)
                        {
                            Dictionary<string, object> goodsData = snapshot.ToDictionary();
                            Debug.Log(" ������ goods ���� �ҷ����� ����");

                            foreach (var kvp in goodsData)
                            {
                                Debug.Log($"{kvp.Key} : {kvp.Value}");
                            }
                            GameDataManager.Instance.SetGoodsData(goodsData);
                        }
                        else
                        {
                            Debug.LogWarning(" �ش� UID�� goods ������ �������� �ʽ��ϴ�.");
                        }
                    }
                    else
                    {
                        Debug.LogError($" goods �ҷ����� ����: {goodsTask.Exception?.Message}");
                    }
                });
                callback(true, "�α��� ����");
            }
            else
            {
                if (task.Exception != null)
                {
                    Exception innerEx = task.Exception.Flatten().InnerExceptions[0];
                    FirebaseException fbEx = innerEx as FirebaseException;
                    if (fbEx != null)
                    {
                        var errorCode = (AuthError)fbEx.ErrorCode;
                        switch (errorCode)
                        {
                            case AuthError.WrongPassword:
                                Debug.LogError(" ��й�ȣ�� Ʋ�Ƚ��ϴ�.");
                                callback(false, "��й�ȣ�� Ʋ�Ƚ��ϴ�.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError(" �̸��� ������ �߸��Ǿ����ϴ�.");
                                callback(false, "�̸��� ������ �߸��Ǿ����ϴ�.");
                                break;
                            case AuthError.UserNotFound:
                                Debug.LogError(" �ش� �̸��Ϸ� ���Ե� ����ڰ� �����ϴ�.");
                                callback(false, "���Ե� ����ڰ� �����ϴ�.");
                                break;
                            default:
                                Debug.LogError($" ��Ÿ �α��� ����: {errorCode} - {fbEx.Message}");
                                callback(false, fbEx.Message);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"? �α��� �� �� �� ���� ����: {task.Exception.Message}");
                        callback(false, task.Exception.Message);
                    }
                }
            }
        });
    }
}
