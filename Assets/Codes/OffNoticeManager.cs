using UnityEngine;
using TMPro;

public class OffNoticeManager : MonoBehaviour
{
    public GameObject offNoticeImage;
    public GameObject settingsPanel;
    public TextMeshProUGUI offNoticeText;

    private bool shownOnce = false;
    private bool wasSettingsPanelActive = false;

    void Start()
    {
        if (PlayerPrefs.HasKey("OffNoticeShown"))
        {
            shownOnce = true;
            offNoticeImage.SetActive(false);
        }
    }

    void Update()
    {
        if (shownOnce) return;

        bool isNowActive = settingsPanel.activeInHierarchy;

        // ����â�� ���� �ִٰ� ���� ������ ����
        if (!wasSettingsPanelActive && isNowActive)
        {
            ShowNotice();
        }

        wasSettingsPanelActive = isNowActive;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideNoticePermanently();
        }
    }

    void ShowNotice()
    {
        offNoticeImage.SetActive(true);
        offNoticeText.text = "�κ�� ���� �������� ESC�� ���� �ɼ�â�� ��� �� �ֽ��ϴ�";
        shownOnce = true;
        PlayerPrefs.SetInt("OffNoticeShown", 1);
        PlayerPrefs.Save();
    }

    void HideNoticePermanently()
    {
        offNoticeImage.SetActive(false);
    }
}
