using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RankingUI : MonoBehaviour
{
    public GameObject rankingScorePrefab;  // ������ (Inspector���� �Ҵ�)
    public Transform content;              //  Content�� Transform (��ũ�Ѻ� ��)
    private List<GameObject> rankingEntries = new List<GameObject>();  //  ������ UI ����Ʈ

    private void Awake()
    {
        GameObject entry = Instantiate(rankingScorePrefab, content);
        entry.SetActive(false); // ��Ȱ��ȭ
        rankingEntries.Add(entry);
    }

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

        if (rankingEntries.Count == 0)
        {
            foreach (Transform child in content)
            {
                rankingEntries.Add(child.gameObject);
            }
        }

        foreach (GameObject entry in rankingEntries)
        {
            entry.SetActive(false);
        }
        Debug.Log("���� ��ŷ UI ��Ȱ��ȭ �Ϸ�");

        for (int i = 0; i < rankingList.Count; i++)
        {
            GameObject entry;

            if (i < rankingEntries.Count)
            {
                entry = rankingEntries[i];
                entry.SetActive(true);
            }
            else
            {
                entry = Instantiate(rankingScorePrefab, content);
                rankingEntries.Add(entry);
            }

            RankingEntry entryScript = entry.GetComponent<RankingEntry>();
            entryScript.SetPlayerEntry(
                rankingList[i].playerID,
                rankingList[i].playcharacter,
                rankingList[i].clearTime,
                rankingList[i].rank
            );

            Debug.Log($"UI ������Ʈ: {rankingList[i].playerID}, {rankingList[i].playcharacter}, {rankingList[i].clearTime}");
        }
    }
}
