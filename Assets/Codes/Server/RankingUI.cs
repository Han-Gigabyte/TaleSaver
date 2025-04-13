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
        if (rankingScorePrefab == null || content == null)
        {
            Debug.LogError(" rankingScorePrefab �Ǵ� content�� ����Ǿ� ���� �ʽ��ϴ�.");
            return;
        }

        // ���ø������� �ϳ��� �̸� ������ (��Ȱ��ȭ)
        GameObject entry = Instantiate(rankingScorePrefab, content);
        entry.SetActive(false);
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

        // ���ο� UI ����
        foreach (var player in rankingList)
        {
            GameObject entry = Instantiate(rankingScorePrefab, content);
            entry.SetActive(true); // Ȱ��ȭ

            RankingEntry entryScript = entry.GetComponent<RankingEntry>();
            if (entryScript != null)
            {
                entryScript.SetPlayerEntry(
                    player.playerID,
                    player.playcharacter,
                    player.clearTime,
                    player.rank
                );
                Debug.Log($" UI �߰�: {player.playerID}, {player.playcharacter}, {player.clearTime}");
            }
            else
            {
                Debug.LogError("RankingEntry ��ũ��Ʈ�� �����տ� �پ����� �ʽ��ϴ�.");
            }

            rankingEntries.Add(entry);
        }
    }
}
