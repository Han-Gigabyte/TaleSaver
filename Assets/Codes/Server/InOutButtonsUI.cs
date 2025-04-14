using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InOutButtonsUI : MonoBehaviour
{
    public Button toggleOnOffButton;
    public Button buttonX;

    private void Start()
    {
        if (toggleOnOffButton != null)
            toggleOnOffButton.onClick.AddListener(RestartGame);

        if (buttonX != null)
            buttonX.onClick.AddListener(QuitGame);
    }

    private void RestartGame()
    {
        Debug.Log("ToggleOnOff ��ư Ŭ���� - Lobby ������ �̵�");
        SceneManager.LoadScene("Lobby"); // Lobby �� �̸� ��Ȯ�� �Է�
    }

    public void QuitGame()
    {
        Debug.Log("���� ���� ��û��");

        if (Application.isEditor)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }

}
