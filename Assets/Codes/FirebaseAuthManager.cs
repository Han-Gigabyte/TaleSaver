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

    void Awake()
    {
        Debug.Log("?? FirebaseAuthManager Awake() �����");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("? FirebaseAuthManager Singleton ��� �Ϸ�");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // FirebaseInit.cs���� �ʱ�ȭ ���� ȣ���� ��
    public void OnFirebaseInitialized()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        isFirebaseReady = true;

        Debug.Log("? OnFirebaseInitialized ȣ���, Firebase �غ� �Ϸ�!");
    }

    public void SignUp(string email, string password)
    {
        Debug.Log($"?? SignUp ȣ��� - isFirebaseReady = {isFirebaseReady}");

        if (!isFirebaseReady)
        {
            Debug.LogError("? Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            return;
        }

        if (!isFirebaseReady)
        {
            Debug.LogError("? Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("? �̸��� �Ǵ� ��й�ȣ�� ��� �ֽ��ϴ�.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser newUser = task.Result.User;  // ? ���� ������
                Debug.Log($"? ȸ������ ����: {newUser.Email} (UID: {newUser.UserId})");

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
                        Debug.Log($"? Firestore�� ����� ���� ���� �Ϸ�: {fullEmail}");
                    else
                        Debug.LogError($"? Firestore ���� ����: {docTask.Exception?.Message}");
                });
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
                                Debug.LogError("? �̹� ��� ���� �̸����Դϴ�.");
                                break;
                            case AuthError.WeakPassword:
                                Debug.LogError("? ��й�ȣ�� �ʹ� ���մϴ�.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError("? �̸��� ������ �߸��Ǿ����ϴ�.");
                                break;
                            default:
                                Debug.LogError($"? ��Ÿ ȸ������ ����: {errorCode} - {fbEx.Message}");
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"? ȸ������ �� �� �� ���� ����: {task.Exception.Message}");
                    }
                }
            }
        });
    }

    public void Login(string email, string password)
    {
        if (!isFirebaseReady)
        {
            Debug.LogError("? Firebase �ʱ�ȭ�� �Ϸ���� �ʾҽ��ϴ�.");
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("? �̸��� �Ǵ� ��й�ȣ�� ��� �ֽ��ϴ�.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = task.Result.User;  // ? ���� ������
                Debug.Log($"? �α��� ����: {user.Email} (UID: {user.UserId})");
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
                                Debug.LogError("? ��й�ȣ�� Ʋ�Ƚ��ϴ�.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError("? �̸��� ������ �߸��Ǿ����ϴ�.");
                                break;
                            case AuthError.UserNotFound:
                                Debug.LogError("? �ش� �̸��Ϸ� ���Ե� ����ڰ� �����ϴ�.");
                                break;
                            default:
                                Debug.LogError($"? ��Ÿ �α��� ����: {errorCode} - {fbEx.Message}");
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"? �α��� �� �� �� ���� ����: {task.Exception.Message}");
                    }
                }
            }
        });
    }
}
