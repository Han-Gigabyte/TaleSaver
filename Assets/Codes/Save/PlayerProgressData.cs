using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

[System.Serializable]
public class PlayerProgressData
{
    public float playTime;   // �÷��� �ð� (�� ����)
    public int stageIndex;   // ���� �������� �ε���

    public PlayerProgressData(float playTime, int stageIndex)
    {
        this.playTime = playTime;
        this.stageIndex = stageIndex;
    }
}
