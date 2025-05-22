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
            // �г��� ��Ȱ��ȭ�Ǿ� ������ Ȱ��ȭ�ϰ�, �̹� ���������� ���� (��� ���)
            if (settingPanel != null)
                settingPanel.SetActive(!settingPanel.activeSelf);
        }
    }
}
