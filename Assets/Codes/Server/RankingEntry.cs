using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingEntry : MonoBehaviour
{
    public TextMeshProUGUI placeText;
    public TextMeshProUGUI playerIDText;
    public TextMeshProUGUI playcharacterText;
    public TextMeshProUGUI cleartimeText;
    public GameObject detailPopupCanvas;
    private string playerCharacter; // �߰�
    private Vector2 defaultPopupPosition;
private bool defaultPositionSet = false;

    public Button detailButton;

    private string playerId;

    public void SetPlayerEntry(string id, string character, string clearTime, int rank)
    {
        playerId = id;
        playerCharacter = character;  // �����ص�
        placeText.text = rank.ToString();
        playerIDText.text = id;
        playcharacterText.text = character;
        cleartimeText.text = clearTime;

        if (detailButton != null)
        {
            detailButton.onClick.RemoveAllListeners();
            detailButton.onClick.AddListener(OnDetailButtonClick);
        }
    }

    private void OnDetailButtonClick()
    {

        if (detailPopupCanvas != null)
        {
            detailPopupCanvas.SetActive(true);
            Debug.Log("[DetailPopupCanvas Ȱ��ȭ��]");
        }
        else
        {
            Debug.LogError("Inspector�� DetailPopupCanvas�� ������� �ʾҽ��ϴ�.");
        }

        PlayerDetailManager manager = FindObjectOfType<PlayerDetailManager>();
        if (manager != null)
        {
            manager.LoadPlayerDetail(playerId, playerCharacter);
        }
        else
        {
            Debug.LogError("PlayerDetailManager�� ã�� �� �����ϴ�.");
        }
    
    }

}
