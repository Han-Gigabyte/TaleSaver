using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        // Firebase ���� �ν��Ͻ� ��������
        auth = FirebaseAuth.DefaultInstance;
    }

    // ?? ȸ������ �Լ�
    public void SignUp(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("? ȸ������ ����: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("? ȸ������ ����: " + newUser.Email);
        });
    }

    // ?? �α��� �Լ�
    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("? �α��� ����: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log("? �α��� ����: " + user.Email);
        });
    }
}