using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
using System;

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
        if (goodsData.ContainsKey("storybookpages"))
            storybookPage = int.Parse(goodsData["storybookpages"].ToString());

        if (goodsData.ContainsKey("machineparts"))
            machineParts = int.Parse(goodsData["machineparts"].ToString());

        // Inventory���� �ݿ�
        InventoryManager.Instance.inventory.storybookpages = storybookPage;
        InventoryManager.Instance.inventory.machineparts = machineParts;
    }

    // ?? ���� �Լ�
    public void SaveGoodsToFirestore()
    {
        StartCoroutine(SaveGoodsWhenReady());
    }

    private IEnumerator SaveGoodsWhenReady()
    {
        yield return FirebaseAuthManager.Instance.WaitUntilUserIsReady(() =>
        {
            FirebaseUser user = FirebaseAuthManager.Instance.Auth.CurrentUser;

            if (user == null)
            {
                Debug.LogError("? Firestore ���� ����: user null");
                return;
            }

            string uid = user.UserId;
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            // ?? Firestore���� ���� ��ȭ �� �о����
            db.Collection("goods").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    int prevMachineParts = 0;
                    int prevStorybookPages = 0;

                    var snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ToDictionary();
                        if (data.ContainsKey("machineparts"))
                            prevMachineParts = Convert.ToInt32(data["machineparts"]);
                        if (data.ContainsKey("storybookpages"))
                            prevStorybookPages = Convert.ToInt32(data["storybookpages"]);
                    }

                    // ?? ������ ��� ���
                    int newMachineParts = prevMachineParts + machineParts;
                    int newStorybookPages = prevStorybookPages + storybookPage;

                    Dictionary<string, object> goodsData = new Dictionary<string, object>()
                {
                    { "machineparts", newMachineParts },
                    { "storybookpages", newStorybookPages }
                };

                    // ?? Firestore�� ����
                    db.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(saveTask =>
                    {
                        if (saveTask.IsCompletedSuccessfully)
                        {
                            Debug.Log($"? Firestore ���� ���� �Ϸ�: machineparts={newMachineParts}, storybookpages={newStorybookPages}");

                            // ?? ���� �� �ʱ�ȭ (�ߺ� ���� ����)
                            machineParts = 0;
                            storybookPage = 0;

                            // ? �κ��丮���� �ݿ�
                            InventoryManager.Instance.inventory.machineparts = 0;
                            InventoryManager.Instance.inventory.storybookpages = 0;
                        }
                        else
                        {
                            Debug.LogError($"? Firestore ���� ����: {saveTask.Exception?.Message}");
                        }
                    });
                }
                else
                {
                    Debug.LogError($"? Firestore ���� �� �ҷ����� ����: {task.Exception?.Message}");
                }
            });
        });
    }
}
