using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardBootstrap : MonoBehaviour
{
    void Start()
    {
        if (RankingManager.Instance != null)
        {
            RankingManager.Instance.TryReconnectRankingUI(); // ? ScoreBoard ���� �� UI �翬��
        }
        else
        {
            Debug.LogWarning("? RankingManager.Instance�� �������� �ʽ��ϴ�.");
        }
    }
}