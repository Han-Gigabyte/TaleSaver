using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public int storybookPage = 0;
    public int machineParts = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ?? �α��� �� ȣ��Ǵ� �Լ�
    public void SetGoodsData(Dictionary<string, object> goodsData)
    {
        if (goodsData.ContainsKey("storybook page"))  // Firestore�� ����� Ű �״�� ���
            storybookPage = int.Parse(goodsData["storybook page"].ToString());

        if (goodsData.ContainsKey("machine parts"))
            machineParts = int.Parse(goodsData["machine parts"].ToString());

        Debug.Log($"?? ���� �Ϸ�: storybookPage={storybookPage}, machineParts={machineParts}");
    }

    // ?? ���� �Լ�
    public void SaveGoodsToFirestore()
    {
        string uid = FirebaseAuthManager.Instance.Auth.CurrentUser?.UserId;

        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogError("? UID�� ��� �ֽ��ϴ�. �α��� ���� Ȯ�� �ʿ�");
            return;
        }

        Dictionary<string, object> goodsData = new Dictionary<string, object>()
        {
            { "storybook page", storybookPage },
            { "machine parts", machineParts }
        };

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        db.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("? Firestore�� goods ������ ���� �Ϸ�");
            }
            else
            {
                Debug.LogError($"? goods ���� ����: {task.Exception?.Message}");
            }
        });
    }
}
