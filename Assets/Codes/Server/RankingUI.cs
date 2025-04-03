using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class RankingUI : MonoBehaviour
{
    public GameObject rankingScorePrefab;  // ������ (Inspector���� �Ҵ�)
    public Transform content;              //  Content�� Transform (��ũ�Ѻ� ��)
    private List<GameObject> rankingEntries = new List<GameObject>();  //  ������ UI ����Ʈ
    //private int initialPoolSize = 1; // �ʱ� ������ ���� (1��)

    //ó�� ������ �� ������ �̸� ����(������Ʈ Ǯ��)
    private void Awake()
    {
        GameObject entry = Instantiate(rankingScorePrefab, content);
        entry.SetActive(false); // ��Ȱ��ȭ
        rankingEntries.Add(entry);
    }

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

        //(1) �ʱ� ���� �����ϴ� RankingScore �������� ã�Ƽ� ����Ʈ�� �߰�
        if (rankingEntries.Count == 0)
        {
            foreach (Transform child in content) // Content�� ��� �ڽ� ��ü Ȯ��
            {
                rankingEntries.Add(child.gameObject);
            }
        }

        //(2) ���� UI �������� ��Ȱ��ȭ (���������� �ʵ���)
        foreach (GameObject entry in rankingEntries)
        {
            entry.SetActive(false);
        }
        Debug.Log("���� ��ŷ UI ��Ȱ��ȭ �Ϸ�");


        // �ʿ��� ��ŭ UI Ȱ��ȭ �Ǵ� ����
        for (int i = 0; i < rankingList.Count; i++)
        {
            GameObject entry;

            if (i < rankingEntries.Count)
            {
                entry = rankingEntries[i]; // ������ �ִ� ������ ����
                entry.SetActive(true);
            }
            else
            {
                entry = Instantiate(rankingScorePrefab, content); // �����ϸ� ���� ����
                rankingEntries.Add(entry);
            }

            // UI ��� �� ����
            RankingEntry entryScript = entry.GetComponent<RankingEntry>();
            entryScript.placeText.text = (i+1).ToString();
            entryScript.playerIDText.text = rankingList[i].playerID;
            entryScript.playcharacterText.text = rankingList[i].playcharacter;
            entryScript.cleartimeText.text = rankingList[i].clearTime;

            Debug.Log($"UI ������Ʈ: {rankingList[i].playerID}, {rankingList[i].playcharacter}, {rankingList[i].clearTime}");
        }
    }

}
