using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanelManager : MonoBehaviour
{
    public GameObject settingPanel; // �ν����Ϳ��� ������ SettingPanel ������Ʈ

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel != null)
            {
                bool isOpening = !settingPanel.activeSelf;
                settingPanel.SetActive(isOpening);

                // �г��� �� �� UI �翬�� �õ�
                if (isOpening && BGMManager.instance != null)
                {
                    BGMManager.instance.TryReconnectUI();
                }

                // ���� �Ͻ�����/�簳�� ���� ����
                Time.timeScale = isOpening ? 0f : 1f;
            }
        }
    }
}