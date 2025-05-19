using UnityEngine;
using TMPro;

public class BlinkAlarm : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private float alphaMin = 0.2f;
    private float alphaMax = 1f;
    private float blinkSpeed = 2f;
    private bool hasClicked = false;

    private void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Ŭ���ϸ� ��Ȱ��ȭ
        if (!hasClicked && Input.GetMouseButtonDown(0))
        {
            hasClicked = true;
            gameObject.SetActive(false);
            return;
        }

        // �����̴� ȿ��
        if (!hasClicked)
        {
            float alpha = Mathf.Lerp(alphaMin, alphaMax, Mathf.PingPong(Time.time * blinkSpeed, 1f));
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;
        }
    }
}
