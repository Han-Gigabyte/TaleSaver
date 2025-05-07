using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;

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
            Dictionary<string, object> goodsData = new Dictionary<string, object>()
        {
            { "storybookpages", storybookPage },
            { "machineparts", machineParts }
        };

            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            db.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"? Firestore�� goods ���� �Ϸ�! machineparts: {machineParts}, storybook: {storybookPage}");
                }
                else
                {
                    Debug.LogError($"? Firestore ���� ����: {task.Exception?.Message}");
                }
            });
        });
    }

}
