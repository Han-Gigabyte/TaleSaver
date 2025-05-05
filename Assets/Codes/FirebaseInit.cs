using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Auth;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        Debug.Log("?? FirebaseInit Start() ���۵�");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.LogEvent("unity_start");
                Debug.Log("? Firebase �ʱ�ȭ �Ϸ�!");

                FirebaseAuth.DefaultInstance.SignOut();
                Debug.Log("?? Firebase �ڵ� �α׾ƿ� ����");

                // ?? logout �ݿ� ��ٸ� �� �ʱ�ȭ
                StartCoroutine(WaitForLogoutThenInitAuth());
            }
            else
            {
                Debug.LogError($"? Firebase �ʱ�ȭ ����: {dependencyStatus}");
            }
        });
    }

    private IEnumerator WaitForLogoutThenInitAuth()
    {
        float timeout = 5f;
        float timer = 0f;

        // ?? SignOut()�� �񵿱�� CurrentUser�� null�� �� ������ ��ٸ�
        while (FirebaseAuth.DefaultInstance.CurrentUser != null && timer < timeout)
        {
            Debug.Log($"? �α׾ƿ� ��� ��... UID: {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
            timer += Time.deltaTime;
            yield return null;
        }

        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("? �α׾ƿ� �ݿ� �Ϸ�. �ʱ�ȭ�� �����մϴ�.");
        }
        else
        {
            Debug.LogWarning("?? �α׾ƿ� �ݿ����� ����. �ð� �ʰ��� ���� �����մϴ�.");
        }

        StartCoroutine(WaitAndInitAuth());  // ���� �ʱ�ȭ �ڷ�ƾ ����
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
