using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerDetailUI : MonoBehaviour
{
    [Header("Stat UI")]
    public Text agilityText;
    public Text healthText;
    public Text powerText;
    public Text luckText;

    [Header("Item UI")]
    public Transform itemContentParent; // ScrollView�� Content ������Ʈ
    public GameObject itemPrefab; // ������ UI ������

    private List<GameObject> currentItemObjects = new List<GameObject>();

    public void DisplayPlayerDetails(PlayerDetailData data)
    {
        //���� ǥ��
        agilityText.text = $"��ø {data.stats["��ø"]}.Lv";
        healthText.text = $"���� {data.stats["�����"]}.Lv";
        powerText.text = $"�Ŀ� {data.stats["�Ŀ�"]}.Lv";
        luckText.text = $"��� {data.stats["���"]}.Lv";

        //���� ������ UI ����
        foreach (var obj in currentItemObjects)
        {
            Destroy(obj);
            Debug.Log("���� ������ UI ����");
        }
        currentItemObjects.Clear();

        //���ο� ������ UI ����
        foreach (var item in data.items)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemContentParent);
            currentItemObjects.Add(itemObj);

            var nameText = itemObj.transform.Find("ItemNameText").GetComponent<Text>();
            var image = itemObj.transform.Find("Image").GetComponent<Image>();

            nameText.text = item.name;
            StartCoroutine(LoadImageFromURL(item.imageUrl, image));
        }
    }

    private System.Collections.IEnumerator LoadImageFromURL(string url, Image imageComponent)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                Debug.LogWarning($"�̹��� �ε� ����: {url}");
            }
        }
    }
}
