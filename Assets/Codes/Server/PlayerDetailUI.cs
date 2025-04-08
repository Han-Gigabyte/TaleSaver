using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json.Linq; // ��ġ �ʿ�!

public class PlayerDetailUI : MonoBehaviour
{
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI playerCharacterText;

    // Stats �ؽ�Ʈ (��ø, ����, �Ŀ�, ���)
    public TextMeshProUGUI statAgilityText;
    public TextMeshProUGUI statHealthText;
    public TextMeshProUGUI statPowerText;
    public TextMeshProUGUI statLuckText;

    // Item UI (5���� ����)
    [System.Serializable]
    public class ItemSlot
    {
        public Image itemImage;
        public TextMeshProUGUI itemNameText;
    }

    public List<ItemSlot> itemSlots = new List<ItemSlot>(); // Inspector�� 5�� ����

    // UI ������Ʈ
    public void UpdateDetailUI(string json)
    {
        JObject data = JObject.Parse(json);

        playerIdText.text = data["playerId"]?.ToString();

        JObject stats = (JObject)data["stats"];
        statAgilityText.text = $"��ø {stats["��ø"]}.Lv";
        statHealthText.text = $"���� {stats["�����"]}.Lv";
        statPowerText.text = $"�Ŀ� {stats["�Ŀ�"]}.Lv";
        statLuckText.text = $"��� {stats["���"]}.Lv";

        JObject items = (JObject)data["item"];
        int i = 0;
        foreach (var item in items)
        {
            if (i >= itemSlots.Count) break;
            var slot = itemSlots[i];
            JObject itemObj = (JObject)item.Value;
            slot.itemNameText.text = itemObj["name"]?.ToString();

            // �̹��� �ε� �ڷ�ƾ
            string imageUrl = itemObj["image"]?.ToString();
            StartCoroutine(LoadItemImage(imageUrl, slot.itemImage));
            i++;
        }
    }

    // �̹��� �ε� �Լ�
    private IEnumerator LoadItemImage(string url, Image image)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        }
        else
        {
            Debug.LogError(" �̹��� �ε� ����: " + req.error);
        }
    }
}
