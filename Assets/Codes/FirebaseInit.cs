using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseAnalytics.LogEvent("unity_start");
                Debug.Log("? Firebase �ʱ�ȭ �Ϸ�!");
            }
            else
            {
                Debug.LogError("? Firebase �ʱ�ȭ ����: " + dependencyStatus);
            }
        });
    }
}