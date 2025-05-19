using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryController : MonoBehaviour
{
    public Image storyImage;
    public TextMeshProUGUI storyText;
    public Button skipButton;

    public Sprite[] storySprites;
    [TextArea(2, 5)]
    public string[] storyLines = {
        "�ܰ������κ��� ���ƿ� ���� ������",
        "����� ��κ��� ����� �� �� ���� �Ǿ���.",
        "�ᱹ ���� ������� ��� �ٸ� �༺���� �̹��� ����� �����ߴ�.",

        "��Ը� �̹� �Ⱓ�̴�.",
        "�̹��ڵ��� ���� ���� �ϳ� �� ���� ������ Ÿ�� ������.",

        "�̹��� �� �� ���ڴ� �ĸ꿡 ���� ������ �ٶ󺻴�.",
        "����� ������ ���� �ӿ��� ���� ������ ���� �Ƿ�������.",

        "���� ������ �ٷ� �ƹ�Ÿ ������",
        "�ƹ�Ÿ�� �̿��� ������ ��ã�� ��ȹ�� �����.",
        "������ ������ �ƹ�Ÿ ���縦 ��µ� ������ ����ģ��.",

        "5��¥�� �Ƶ鿡�� ��ȭå�� �о��ִ� ���� ���ݴ´�.",
        "��ȭ �� ���ΰ����� ������ ���� ���̴�.",

        "�ƹ�Ÿ ���縦 ���� �ں���, ������ ������ �̾�����.",
        "�׷��� ��,",
        "���� ������ �ܰ��ε��� ��ô�� ���� ������ �ٿ����� ã�� �ȴ�.",

        "�ٷ� ������� �� �𸣴�, ������ ������ �ʴ� ������ �������̾���.",
        "����ؼ� ��� ���Ĺ��� ���������� ����� �ܰ� ������ �󵵰�",
        "�� �����ǿ��� �����ϰ� �������� Ȯ���ߴ�.",

        "�״� �ǽ��� ���� ���� �� �����Ҹ� ���� �ƹ�Ÿ�� �����߰ڴٰ� ���ߴ�.",
        "�� �����ǿ��� ������ �ܰ� ���� ������ ������,",
        "Ǫ���� ������ �� ��ã������."
    };

    public float typingSpeed = 0.05f;

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isFullTextShown = false;

    void Start()
    {
        skipButton.onClick.AddListener(SkipStory);
        ShowCurrentSlide();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Ÿ�� ȿ�� ���� ���� ��� ���
                StopAllCoroutines();
                storyText.text = storyLines[currentIndex];
                isTyping = false;
                isFullTextShown = true;
            }
            else if (isFullTextShown)
            {
                // �̹� ��ü ���ڰ� ���̸� ���� �����̵��
                currentIndex++;
                if (currentIndex < storyLines.Length)
                    ShowCurrentSlide();
                else
                    EndStory();
            }
        }
    }

    void ShowCurrentSlide()
    {
        storyImage.sprite = storySprites[currentIndex];
        storyText.text = "";
        StartCoroutine(TypeText(storyLines[currentIndex]));
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        isFullTextShown = false;

        foreach (char c in line)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isFullTextShown = true;
    }

    void SkipStory()
    {
        // ���ϴ� ������ �̵��ϰų� ���� ����
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void EndStory()
    {
        // ��� ���丮 ������ �� ó��
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
