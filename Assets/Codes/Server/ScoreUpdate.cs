using Firebase;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;
using Firebase.Extensions;

public class FirebaseWebInit : MonoBehaviour
{
    void Start()
    {
        // Firebase 웹 API를 직접 설정
        AppOptions options = new AppOptions
        {
            ApiKey = "AIzaSyCTEXMlsJSGF7lIdVt-Hn9BITyx_8Pyu20",  // 🔹 Firebase 프로젝트의 웹 API 키
            ProjectId = "tale-saver", // 🔹 Firebase 프로젝트 ID
            AppId = "1:870744593385:web:3c65b1a42e1ec62fc27178" // 🔹 웹 앱 추가 시 제공된 App ID
        };

        FirebaseApp app = FirebaseApp.Create(options);
        FirebaseFirestore db = FirebaseFirestore.GetInstance(app);

        Debug.Log("웹 API 이용한 설정 완료");

        // 🔥 Firestore 데이터 테스트
        GetFirestoreData(db);
    }

    void GetFirestoreData(FirebaseFirestore db)
    {
        db.Collection("users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                foreach (var doc in task.Result.Documents)
                {
       
                    string userId = doc.GetValue<string>("userId");

                    // "username" 필드를 가져옴
                    string username = doc.GetValue<string>("username");

                    // 로그에 userId와 username 출력
                    Debug.Log($"UserId: {userId}, Username: {username}");
                }
            }

            else
            {
                Debug.LogError("데이터 불러오기 실패");
            }
        });
    }
}
