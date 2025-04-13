using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerDetailUI : MonoBehaviour
{
    public Text playerIdText;
    public Text playerCharacterText;

    public Text statAgilityText;
    public Text statHealthText;
    public Text statPowerText;
    public Text statLuckText;

    public GameObject itemPrefab;         // Inspector���� ������ ����
    public Transform itemContainer;       // itemPrefab���� �� �θ� (ex: ItemContent)

    public void ClearItems()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }

    [System.Serializable]
    public class ItemSlot
    {
        public Image itemImage;
        public Text itemNameText;
    }

    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    public void UpdateDetailUI(Dictionary<string, object> data)
    {
        playerIdText.text = data.ContainsKey("playerId") ? data["playerId"].ToString() : "Unknown";
        playerCharacterText.text = data.ContainsKey("character") ? data["character"].ToString() : "Unknown";



        if (data.TryGetValue("stats", out object statsObj) && statsObj is Dictionary<string, object> stats)
        {
            statAgilityText.text = $"��ø {GetInt(stats, "��ø")}.Lv";
            statHealthText.text = $"���� {GetInt(stats, "�����")}.Lv";
            statPowerText.text = $"�Ŀ� {GetInt(stats, "�Ŀ�")}.Lv";
            statLuckText.text = $"��� {GetInt(stats, "���")}.Lv";
        }

        ClearItems();

        if (data.TryGetValue("item", out object itemObj) && itemObj is Dictionary<string, object> itemMap)
        {
            foreach (var entry in itemMap)
            {
                var itemData = entry.Value as Dictionary<string, object>;
                if (itemData != null)
                {
                    // �α� 1: ������ Ű + name, image
                    string itemName = itemData.ContainsKey("name") ? itemData["name"].ToString() : "�̸� ����";
                    string imageUrl = itemData.ContainsKey("image") ? itemData["image"].ToString() : "(����)";
                    Debug.Log($"[ ������ �ε�] key={entry.Key}, name={itemName}, image={imageUrl}");

                    GameObject itemGO = Instantiate(itemPrefab, itemContainer);
                    var itemImage = itemGO.transform.Find("ItemImage")?.GetComponent<Image>();
                    var itemText = itemGO.transform.Find("ItemName")?.GetComponent<Text>();

                    if (itemText != null)
                    {
                        //itemText.text = itemData.ContainsKey("name") ? itemData["name"].ToString() : "�̸� ����";
                        itemText.text = itemName;
                        itemText.color = Color.black; // Ȥ�� �����ұ�� ���� ����
                        Debug.Log($"[�ؽ�Ʈ ������] {itemName}");
                        Debug.Log($"[�ؽ�Ʈ ��ġ] anchoredPos={itemText.rectTransform.anchoredPosition}, size={itemText.rectTransform.sizeDelta}");

                    }


                    else
                    {
                        Debug.LogError($"[itemText == null] �� 'ItemName' ������Ʈ�� ã�� ���߽��ϴ�.");
                    }

                    //if (itemImage != null)
                    //{
                    //    if (!string.IsNullOrEmpty(imageUrl))
                    //    {
                    //        StartCoroutine(LoadItemImage(imageUrl, itemImage));
                    //    }
                    //    else
                    //    {
                    //        Debug.Log($"[�̹��� ����] name={itemName}");
                    //        // image.sprite = defaultSprite;
                    //    }
                    //}

                    //else
                    {
                        //Debug.Log($"image �ʵ� ����. name={itemText?.text}");
                        // image.sprite = defaultSprite; // �⺻ �̹��� ���� ����
                    }
                }
            }
        }
    }


    private int GetInt(Dictionary<string, object> dict, string key)
    {
        return dict.ContainsKey(key) ? int.Parse(dict[key].ToString()) : 0;
    }

    //url ���
    //private IEnumerator LoadItemImage(string url, Image image)
    //{
    //    UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
    //    yield return req.SendWebRequest();

    //    if (req.result == UnityWebRequest.Result.Success)
    //    {
    //        Texture2D tex = DownloadHandlerTexture.GetContent(req);
    //        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    //    }
    //    else
    //    {
    //        Debug.LogError("�̹��� �ε� ����: " + req.error);
    //    }
    //}
}
