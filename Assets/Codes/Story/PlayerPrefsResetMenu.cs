using UnityEditor;
using UnityEngine;

public class PlayerPrefsResetMenu
{
    [MenuItem("Tools/�ʱ�ȭ/PlayerPrefs ��ü ����")]
    public static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs �ʱ�ȭ �Ϸ�");
    }
}
