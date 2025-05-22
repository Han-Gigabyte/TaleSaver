using UnityEngine;
using TMPro;

public class OptionNoticeManager : MonoBehaviour
{
    public GameObject noticeImage; // Image ������Ʈ ��ü
    public TextMeshProUGUI noticeText;
    public float blinkSpeed = 2f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    private bool isBlinking = false;
    private Color originalColor;

    void Start()
    {
        // ���� ���� üũ
        if (!PlayerPrefs.HasKey("OptionNoticeShown"))
        {
            ShowNotice();
        }
        else
        {
            noticeImage.SetActive(false);
        }
    }

    void Update()
    {
        if (isBlinking)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));
            Color newColor = originalColor;
            newColor.a = alpha;
            noticeText.color = newColor;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // �ɼ�â ���� ���� ���⿡ �����ؾ� �� (���� OptionManager ��� ó��)
            HideNoticePermanently();
        }
    }

    void ShowNotice()
    {
        noticeImage.SetActive(true);
        originalColor = noticeText.color;
        isBlinking = true;
        noticeText.text = "�κ�� ���� �������� ESC�� ���� �ɼ�â�� ���� ���� �� �ֽ��ϴ�.";
    }

    public void HideNoticePermanently()
    {
        isBlinking = false;
        noticeImage.SetActive(false);
        PlayerPrefs.SetInt("OptionNoticeShown", 1); // �ٽ� �Ⱥ��̵��� ����
        PlayerPrefs.Save();
    }
}
