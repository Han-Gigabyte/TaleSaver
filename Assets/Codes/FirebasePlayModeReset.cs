using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Firebase.Auth;

[InitializeOnLoad]
public static class FirebasePlayModeReset
{
    static FirebasePlayModeReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("?? ������ �÷��� ��� ���� �� Firebase �α׾ƿ� ����");
            FirebaseAuth.DefaultInstance?.SignOut();
        }
    }
}