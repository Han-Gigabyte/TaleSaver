using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        Debug.Log("?? FirebaseInit Start() ���۵�");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("?? FirebaseApp.CheckAndFixDependenciesAsync �Ϸ��");

            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.LogEvent("unity_start");
                Debug.Log("? Firebase �ʱ�ȭ �Ϸ�!");

                // ���⼭ �ڷ�ƾ ����
                StartCoroutine(WaitAndInitAuth());
            }
            else
            {
                Debug.LogError($"? Firebase �ʱ�ȭ ����: {dependencyStatus}");
            }
        });
    }

    IEnumerator WaitAndInitAuth()
    {
        float timer = 0f;
        float timeout = 5f;

        while ((FirebaseAuthManager.Instance == null || !FirebaseAuthManager.Instance.isActiveAndEnabled) && timer < timeout)
        {
            Debug.Log($"? ��ٸ��� ��... FirebaseAuthManager.Instance == null ? {FirebaseAuthManager.Instance == null}");
            timer += Time.deltaTime;
            yield return null;
        }

        if (FirebaseAuthManager.Instance != null)
        {
            Debug.Log("? FirebaseAuthManager �ν��Ͻ� �߰ߵ�! �ʱ�ȭ ȣ��");
            FirebaseAuthManager.Instance.OnFirebaseInitialized();
        }
        else
        {
            Debug.LogError("? FirebaseAuthManager.Instance ������ null�Դϴ�. �ʱ�ȭ ����");
        }
    }
}
