using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class RankingUI : MonoBehaviour
{
    public GameObject rankingScorePrefab;  // ������ (Inspector���� �Ҵ�)
    public Transform content;              //  Content�� Transform (��ũ�Ѻ� ��)
    private List<GameObject> rankingEntries = new List<GameObject>();  //  ������ UI ����Ʈ



    // Update is called once per frame
    public void UpdateRankingUI(List<PlayerData> rankingList)
    {
        Debug.Log($"UpdateRankingUI ���� - ��ŷ ����: {rankingList.Count}");

        if (rankingScorePrefab == null)
        {
            Debug.LogError("rankingScorePrefab�� �������� ����");
            return;
        }
        if (content == null)
        {
            Debug.LogError("content�� �������� ����");
            return;
        }

        //�� ������ �����ϰ� ���� UI ����
        foreach (GameObject entry in rankingEntries)
        {
            Destroy(entry);
        }
        rankingEntries.Clear();
        Debug.Log("���� ��ŷ UI ����");


        foreach(var data in rankingList)
        {
            if (data == null)
            {
                Debug.LogError("PlayerData�� null�Դϴ�.");
                continue;
            }
            //������ ����
            GameObject newEntry = Instantiate(rankingScorePrefab, content);
            newEntry.transform.localScale = Vector3.one;

            //RankingEntry ������Ʈ ��������
            RankingEntry entryScript = newEntry.GetComponent<RankingEntry>();
            if (entryScript == null)
            {
                Debug.LogError("RankingEntry ��ũ��Ʈ�� ã�� �� �����ϴ�! �������� Ȯ���ϼ���.");
                continue;
            }

            // TextMeshProUGUI �������� ��� ����
            entryScript.playerIDText = newEntry.transform.Find("playerID")?.GetComponent<TextMeshProUGUI>();
            entryScript.playcharacterText = newEntry.transform.Find("playcharacter")?.GetComponent<TextMeshProUGUI>();
            entryScript.cleartimeText = newEntry.transform.Find("cleartime")?.GetComponent<TextMeshProUGUI>();


            if (entryScript.playerIDText == null || entryScript.playcharacterText == null || entryScript.cleartimeText == null)
            {
                Debug.LogError("UI ��Ҹ� ã�� �� ����: playerIDText, playcharacterText, cleartimeText �� �ϳ��� null");
                continue;
            }

            // �� ����
            entryScript.playerIDText.text = data.playerID;
            entryScript.playcharacterText.text = data.playcharacter;
            entryScript.cleartimeText.text = data.clearTime;


            rankingEntries.Add(newEntry);


        }
        Debug.Log("���� ��ŷ UI �ҷ����� ����");
    }
}
