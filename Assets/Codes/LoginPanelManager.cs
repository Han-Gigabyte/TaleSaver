
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
        if (loginWarningText == null)
        {
            Debug.LogError("?? loginWarningText�� null�Դϴ�! Inspector ���� Ȯ�� �ʿ�!");
            return;
        }

        if (authManager != null && authManager.IsLoggedIn())
        {
            loginWarningText.gameObject.SetActive(false);
        }
        else
        {
            loginWarningText.gameObject.SetActive(true);
        }
    }

    void Awake()
    {
        if (loginWarningText == null)
        {
            loginWarningText = GameObject.Find("LoginWarningText")?.GetComponent<TMP_Text>();
            if (loginWarningText == null)
            {
                Debug.LogError("?? LoginWarningText�� �ڵ����� ã�� �� �����ϴ�!");
            }
            else
            {
                Debug.Log("? LoginWarningText�� �ڵ忡�� �ڵ����� �����߽��ϴ�.");
            }
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
        string email = emailInput.text;
        string password = passwordInput.text;

        authManager.Login(email, password, (success, message) =>
        {
            if (success)
            {
                alertText.text = " �α��ο� �����Ͽ����ϴ�.";
                loginWarningText.gameObject.SetActive(false);
                Invoke(nameof(HideLoginPanel), 1.0f);
            }
            else
            {
                alertText.text = message;

                // ? ���� �� UI�� ��Ȯ�� �α��� �� ���·� �����ֱ�
                loginPanel.SetActive(true);
                gameStartButton.SetActive(false);
                gameLoginButton.SetActive(false);
                gameOptionButton.SetActive(false);
                gameExitButton.SetActive(false); 

                emailInput.interactable = true;
                passwordInput.interactable = true;

                // �ʿ��ϸ� ����Ŀ��
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
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

        // ?�Է� �ʵ�� ��ư ��� ��Ȱ��ȭ (�ߺ� Ŭ�� ������, ���� ����)
        emailInput.interactable = true;
        passwordInput.interactable = true;

        authManager.SignUp(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" ȸ������ ���� �ݹ�");
                alertText.text = " ȸ������ �Ǿ����ϴ�.";

                // ���� �� ���Ѵٸ� �α��� �г� �ڵ����� �ݰų� �ʱ�ȭ ����
                // HideLoginPanel();
            }
            else
            {
                Debug.LogError(" ȸ������ ���� �ݹ�: " + message);
                alertText.text = message;

                // ? ���� �� �Է� UI ����
                loginPanel.SetActive(true); // Ȥ�� �����ٸ� �ٽ� ǥ��
                emailInput.interactable = true;
                passwordInput.interactable = true;

                // ? �Է� �ʵ� �ڵ� ��Ŀ��
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
            }
        });
    }
}
