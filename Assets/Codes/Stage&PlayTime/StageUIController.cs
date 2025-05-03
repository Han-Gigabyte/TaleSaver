using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageUIController : MonoBehaviour
{

    private static StageUIController instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // Canvas�� UIRoot ��ü�� �����ϱ� ���� root ����
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject); // �ߺ� ���� ����
        }
    }

    public Text NowStage;
    public Text PlayTime;

    private float playTime = 0f;

    private readonly string[] stages = {
        "Store", "Stage 1", "Stage 2", "Stage 3", "Stage 4",
        "Stage 5", "Stage 6", "Stage 7", "Stage 8",
        "Stage 9", "Boss"
    };



    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Store")
        {
            GameManager.Instance.Stage = 0;  // Store �ε���
        }
        else
        {
            var progress = SaveManager.Instance.LoadProgressData();
            if (progress != null)
            {
                GameManager.Instance.PlayTime = progress.playTime;
                GameManager.Instance.Stage = progress.stageIndex;
            }
        }


        playTime = GameManager.Instance.PlayTime;
        UpdateStageText();
    }

    void Update()
    {
        // ���������� Store�� �ƴϸ� �ð� ����
        if (GameManager.Instance.Stage != 0)
        {
            GameManager.Instance.PlayTime += Time.deltaTime;
            playTime = GameManager.Instance.PlayTime;
        }
        else
        {
            // Store������ ���� ����, ����� �ð� �״�� ǥ��
            playTime = GameManager.Instance.PlayTime;
        }

        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        PlayTime.text = $"Time: {minutes:00}:{seconds:00}";

        UpdateStageText();
    }

    public void NextStage()
    {
        GameManager.Instance.Stage++;
        SaveProgress();
    }

    private void UpdateStageText()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Boss Scene�̸� Boss�� ���� ǥ��
        if (sceneName == "BossStage")
        {
            NowStage.text = "Boss";
            return;
        }

        int stageIndex = GameManager.Instance.Stage;
        int chapter = GameManager.Instance.Chapter;

        string locationName = GetLocationName(MapManager.Instance.location);

        if (stageIndex == 0)
        {
            NowStage.text = $"{locationName} Store";
        }

        else if (stageIndex >= 1 && stageIndex <= 9)
        {
            NowStage.text = $"{locationName} {stageIndex}";
        }
        else
        {
            NowStage.text = "Unknown";
        }
    }

    private string GetLocationName(int location)
    {
        switch (location){
            case 0: return "Cave";
            case 1: return "Desert";
            case 2: return "Forest";
            case 3: return "Ice";
            case 4: return "Lab";
            case 5: return "Lava";
            default: return "Unknown";
        }
    }

    private void SaveProgress()
    {
        int stageIndex = GameManager.Instance.Stage;
        PlayerProgressData data = new PlayerProgressData(playTime, stageIndex);
        SaveManager.Instance.SaveProgressData(data);
    }
}
