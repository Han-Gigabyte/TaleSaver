using UnityEngine;

public class SettingPanelController : MonoBehaviour
{
    public GameObject settingPanel;  // �ν����Ϳ��� ����
    private bool isPanelOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingPanel();
        }
    }

    public void ToggleSettingPanel()
    {
        isPanelOpen = !isPanelOpen;
        settingPanel.SetActive(isPanelOpen);

        // ���� �Ͻ�����/�簳
        Time.timeScale = isPanelOpen ? 0f : 1f;

        if (isPanelOpen && BGMManager.instance != null)
        {
            BGMManager.instance.TryReconnectUI(); // �г��� ���� ���� ���� �õ�
        }
    }

    // �ݱ� ��ư���� ȣ���� �� �ֵ��� ���� �޼���
    public void ClosePanel()
    {
        isPanelOpen = false;
        settingPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
