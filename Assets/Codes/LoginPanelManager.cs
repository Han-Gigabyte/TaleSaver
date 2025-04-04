
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;

public class LoginPanelManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject gameStartButton;
    public GameObject gameOptionButton;
    public GameObject gameLoginButton;
    public GameObject gameExitButton;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text alertText;
    public FirebaseAuthManager authManager;
    public TMP_Text loginWarningText;

    void Start()
    {
        // ���� ���� �� �α��� ���� Ȯ��
        if (authManager != null && authManager.IsLoggedIn())
        {
            loginWarningText.gameObject.SetActive(false);
        }
        else
        {
            loginWarningText.gameObject.SetActive(true);
        }
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        gameStartButton.SetActive(false);
        gameOptionButton.SetActive(false);
        gameLoginButton.SetActive(false);
        gameExitButton.SetActive(false);
        alertText.text = "";
    }

    public void HideLoginPanel()
    {
        loginPanel.SetActive(false);
        gameStartButton.SetActive(true);
        gameOptionButton.SetActive(true);
        gameLoginButton.SetActive(true);
        gameExitButton.SetActive(true);
    }

    public void OnClickLogin()
    {
        Debug.Log(" �α��� ��ư ����");

        string email = emailInput.text;
        string password = passwordInput.text;

        if (authManager == null)
        {
            Debug.LogError("? authManager�� null�Դϴ�! Inspector ���� ����");
            return;
        }

        authManager.Login(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" �α��� ���� �ݹ�");
                alertText.text = " �α��ο� �����Ͽ����ϴ�.";
                loginWarningText.gameObject.SetActive(false); // ? ��� �����
                Invoke(nameof(HideLoginPanel), 1.0f);
            }
            else
            {
                Debug.LogError(" �α��� ���� �ݹ�: " + message);
                alertText.text = " " + message;
            }
        });
    }

    public void OnClickSignup()
    {
        Debug.Log(" ȸ������ ��ư ����");

        string email = emailInput.text;
        string password = passwordInput.text;

        if (authManager == null)
        {
            Debug.LogError(" authManager�� null�Դϴ�! Inspector ���� ����");
            return;
        }

        authManager.SignUp(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" ȸ������ ���� �ݹ�");
                alertText.text = " ȸ������ �Ǿ����ϴ�.";
            }
            else
            {
                Debug.LogError(" ȸ������ ���� �ݹ�: " + message);
                alertText.text = "? " + message;
            }
        });
    }
}
