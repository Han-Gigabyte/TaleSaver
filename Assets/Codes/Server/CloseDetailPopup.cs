using UnityEngine;

public class CloseDetailPopup : MonoBehaviour
{
    public GameObject DetailPopupCanvas;

    public void ClosePopup()
    {
        if (DetailPopupCanvas != null)
        {
            DetailPopupCanvas.SetActive(false);
            Debug.Log("[DetailPopupCanvas ��Ȱ��ȭ��]");
        }
        else
        {
            Debug.LogError("detailPopupCanvas�� ������� �ʾҽ��ϴ�.");
        }
    }
}
