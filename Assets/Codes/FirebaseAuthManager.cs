
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using Firebase.Extensions;
using System.Collections;



public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseUser currentUser;
    public FirebaseUser CurrentUser => currentUser;
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

        if (currentUser != null)
            Debug.Log("���� �α��ε� UID: " + currentUser.UserId);
        else
            Debug.Log("���� �α��ε� ����� ����");

        Debug.Log("OnFirebaseInitialized ȣ���, Firebase �غ� �Ϸ�!");
        Debug.Log("���� �α��ε� UID: " + auth.CurrentUser?.UserId);
    }
    public bool IsLoggedIn()
    {
        if (!isFirebaseReady || currentUser == null)
        {
            Debug.LogWarning("IsLoggedIn(): Firebase ���ʱ�ȭ �Ǵ� currentUser�� null�Դϴ�.");
            return false;
        }
        return true;
    }

    public IEnumerator WaitForLoginReady(System.Action onReady)


    {
        float timer = 0f;
        float timeout = 5f;

        while (auth == null || auth.CurrentUser == null)
        {
            Debug.Log("? �α��� ���� ��ٸ��� ��...");
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("? �α��� �غ� �ð� �ʰ�!");
                yield break;
            }
            yield return null;
        }

        Debug.Log("? Firebase �α��� �غ� �Ϸ�!");
        onReady?.Invoke();
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
                currentUser = task.Result.User;
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
                    { "storybookpages", 0 },
                    { "machineparts", 0 }
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
                currentUser = task.Result.User;
                Debug.Log("? currentUser ���õ�: " + currentUser?.Email);
                Debug.Log("? IsLoggedIn(): " + IsLoggedIn());
                Debug.Log("? Auth.CurrentUser: " + auth?.CurrentUser?.Email);
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
                    try
                    {
                        var flatEx = task.Exception.Flatten();
                        if (flatEx.InnerExceptions.Count > 0)
                        {
                            FirebaseException fbEx = flatEx.InnerExceptions[0] as FirebaseException;

                            if (fbEx != null)
                            {
                                var errorCode = (AuthError)fbEx.ErrorCode;
                                switch (errorCode)
                                {
                                    case AuthError.EmailAlreadyInUse:
                                        Debug.LogWarning("�̹� ��� ���� �̸����Դϴ�.");
                                        callback(false, "�̹� ��� ���� �̸����Դϴ�.");
                                        break;
                                    case AuthError.WeakPassword:
                                        Debug.LogWarning("��й�ȣ�� �ʹ� ���մϴ�.");
                                        callback(false, "��й�ȣ�� �ʹ� ���մϴ�.");
                                        break;
                                    case AuthError.InvalidEmail:
                                        Debug.LogWarning("�̸��� ������ �߸��Ǿ����ϴ�.");
                                        callback(false, "�̸��� ������ �߸��Ǿ����ϴ�.");
                                        break;
                                    default:
                                        Debug.LogWarning($"��Ÿ ȸ������ ����: {errorCode}");
                                        callback(false, "ȸ������ �� ������ �߻��߽��ϴ�.");
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("�� �� ���� Firebase ����.");
                                callback(false, "ȸ������ �� �� �� ���� ������ �߻��߽��ϴ�.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("���� ó�� �� ���� �߻�: " + ex.Message);
                        callback(false, "ȸ������ �� �ý��� ������ �߻��߽��ϴ�.");
                    }
                }
            }
        });
    }
    public IEnumerator WaitUntilUserIsReady(Action onReady)
    {
        // currentUser�� null�̰ų� UID�� ��������� ��ٸ�
        yield return new WaitUntil(() =>
            FirebaseAuth.DefaultInstance.CurrentUser != null &&
            !string.IsNullOrEmpty(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
        );

        // �Ϸ�Ǹ� currentUser �����ϰ� �ݹ� ����
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("? �α��� �غ� �Ϸ�! UID: " + currentUser.UserId);
        onReady?.Invoke();
    }
}
