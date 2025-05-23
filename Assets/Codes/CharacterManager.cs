using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using Firebase.Extensions;

public static class FirebaseTaskExtensions
{
    public static IEnumerator AsCoroutine(this Task task)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            Debug.LogError(task.Exception);
    }
}

public class CharacterManager : MonoBehaviour
{
    [Header("Character Data Info")]
    public CharacterData[] characters;        // 캐릭터 데이터 배열
    public Image characterImage;              // 캐릭터 이미지
    public Text characterNameText;            // 캐릭터 이름 텍스트
    public Text descriptionText;              // 캐릭터 설명 텍스트
    public Text levelText;                    // 캐릭터 레벨 텍스트
    public Text unlockConditionText;          // 캐릭터 해금조건 텍스트
    public Text requireLevelupText;
    public Text MachineText;
    public Text ableupgrade;

    // 현재 선택된 캐릭터의 스프라이트를 저장할 변수
    private Sprite characterSprite;
    public RuntimeAnimatorController characterAnimator;  // 선택된 캐릭터 애니메이터



    [Header("Panel Info")]
    public GameObject characterInfoPanel;     // 캐릭터 정보 패널
    public GameObject upgradePanel;           // 업그레이드 패널
    public GameObject ExplainPanel;           //설명창 패널

    [Header("Upgrade Info")]
    public Image upgradeCharacterImage;
    public Text upgradeNameText;
    public Text upgradeLevelText;
    public Text vitalityText;
    public Text powerText;
    public Text agilityText;
    public Text luckText;

    [Header("캐릭터 스탯 및 스킬 디버그")]
    [SerializeField] private string selectedCharacterName;
    [SerializeField] private int selectedPowerLevel;
    [SerializeField] private float damageMultiplier;
    [System.Serializable]
    public class SkillDebugInfo
    {
        public string skillName;
        public int baseDamage;
        public int calculatedDamage;

        public SkillDebugInfo(string name, int baseDmg, int calcDmg)
        {
            skillName = name;
            baseDamage = baseDmg;
            calculatedDamage = calcDmg;
        }
    }
    [SerializeField] private List<SkillDebugInfo> skillDebugInfos = new List<SkillDebugInfo>();

    [Header("Upgrade Buttons")]
    public Button vitalityUpgradeButton; // Vitality 업그레이드 버튼
    public Button powerUpgradeButton;     // Power 업그레이드 버튼
    public Button agilityUpgradeButton;   // Agility 업그레이드 버튼
    public Button luckUpgradeButton;      // Luck 업그레이드 버튼

    [Header("Buttons")]
    public Transform characterContainer;  // 캐릭터 버튼들이 들어갈 부모 컨테이너
    public Button backButton;
    public Button selectButton;
    public Button upgradeButton;
    public Button unlockButton;
    public Button closeUpgradeButton;         // 업그레이드 창 닫기 버튼
    public Button levelupButton;
    public Button ExplainButton;
    public Button ExitExplainButton;

    private int currentCharacterIndex = 0;
    private List<Button> characterButtons = new List<Button>(); // 동적 버튼 리스트

    private float[] skillCooldownTimers; // 스킬 쿨타임 타이머

    private SkillManager skillManager;
    private int currentHealth;
    private int maxHealth;

    // 각 캐릭터의 최대 체력을 저장하는 배열
    public bool isDataLoaded = false; // 데이터 로드 완료 여부
    private bool firebaseDataLoaded = false;

    private void Start()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i] = Instantiate(characters[i]);
            characters[i].characterManager = this; 
        }
        characterInfoPanel.SetActive(false);
        upgradePanel.SetActive(false);

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("No character data found!");
            return;
        }

        StartCoroutine(FirebaseAuthManager.Instance.WaitUntilUserIsReady(() =>
        {
            StartCoroutine(LoadUnlockedCharactersFromFirebase(() =>
            {
                StartCoroutine(LoadCharacterStatsFromFirebase());//캐릭터 스텟 firebase에서 불러오기
            }));
        }));

        // 캐릭터 데이터 초기화
        foreach (var character in characters)
        {
            int index = Array.IndexOf(characters, character);

            character.level = PlayerPrefs.GetInt("CharacterLevel_" + index, 1);
            character.vitality = PlayerPrefs.GetInt("CharacterVitality_" + index, 0);
            character.power = PlayerPrefs.GetInt("CharacterPower_" + index, 0);
            character.agility = PlayerPrefs.GetInt("CharacterAgility_" + index, 0);
            character.luck = PlayerPrefs.GetInt("CharacterLuck_" + index, 0);
            // 캐릭터 잠금 상태 로드
            //character.isUnlocked = PlayerPrefs.GetInt("CharacterUnlocked_" + index, index == 1 ? 1 : 0) == 1;
        }

        unlockButton.onClick.AddListener(() => TryUnlockCharacterFirebase(currentCharacterIndex));
        levelupButton.onClick.AddListener(() => TryLevelUpCharacterFirebase(currentCharacterIndex));
        backButton.onClick.AddListener(HideCharacterInfo);
        selectButton.onClick.AddListener(OnSelectButtonClick);
        upgradeButton.onClick.AddListener(ShowUpgradePanel);
        closeUpgradeButton.onClick.AddListener(CloseUpgradePanel);

        Button[] existingButtons = characterContainer.GetComponentsInChildren<Button>();

        if (existingButtons.Length != characters.Length)
        {
            Debug.LogError($"⚠️ 버튼 개수({existingButtons.Length})와 캐릭터 개수({characters.Length})가 맞지 않습니다!");
            return;
        }

        characterButtons = new List<Button>();

        for (int i = 0; i < existingButtons.Length; i++)
        {
            int index = i;
            Button button = existingButtons[i];

            Text buttonText = button.GetComponentInChildren<Text>(); // 버튼의 텍스트
            Image buttonImage = button.GetComponent<Image>(); // 버튼 자체의 이미지

            if (buttonText != null)
            {
                buttonText.text = characters[index].characterName;
            }

            // 캐릭터가 잠겨 있으면 버튼을 어둡게 처리
            if (!characters[index].isUnlocked)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 어두운 색상 적용
            }

            // 버튼 클릭 이벤트 추가
            button.onClick.AddListener(() => ShowCharacterInfo(index));

            // 리스트에 추가
            characterButtons.Add(button);
        }

        // CharacterSelectionData에 스프라이트 설정 요청
        CharacterSelectionData.Instance.SetDefaultCharacterSprite(this);

        // 스킬 쿨타임 타이머 초기화
        skillCooldownTimers = new float[4];

        // SkillManager를 GameObject에 추가
        skillManager = gameObject.AddComponent<SkillManager>();

        maxHealth = GameManager.Instance.GetCurrentMaxHealth();
        currentHealth = maxHealth;

        // GameManager에 현재 캐릭터 설정
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentCharacter(characters[currentCharacterIndex]);
        }
    }
    public void OnClickExplain()
    {
        ExplainPanel.SetActive(true);
    }
    public void ExitExplain()
    {
        ExplainPanel.SetActive(false);
    }


    public void OnClickUnlockButton(int index)
    {
        StartCoroutine(FirebaseAuthManager.Instance.WaitUntilUserIsReady(() =>
        {
            Debug.Log("✅ 해금 시도 시작!");
            TryUnlockCharacterFirebase(index);
        }));
    }

    private IEnumerator WaitAndUnlock(int index)
    {
        float timeout = 5f;
        float timer = 0f;

        while (!FirebaseAuthManager.Instance.IsLoggedIn())
        {
            Debug.Log("⏳ 로그인 기다리는 중...");
            timer += Time.deltaTime;

            if (timer > timeout)
            {
                Debug.LogError("❌ 로그인 준비 시간 초과. 해금 중단");
                yield break;
            }

            yield return null;
        }

        Debug.Log("✅ 로그인 완료됨, 해금 시작!");
        TryUnlockCharacterFirebase(index);
    }
    private void Update()
    {
        // 디버그 정보 업데이트
        UpdateDebugInfo();

        // 키보드의 키를 눌렀을 때 현재 선택된 캐릭터의 레벨을 증가
        if (Input.GetKeyDown(KeyCode.BackQuote)) IncreaseCharacterLevel(); // 키는 BackQuote로 표현
        // 스킬 쿨타임 타이머 업데이트
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillCooldownTimers[i] -= Time.deltaTime;
            }
        }

        // 키 입력 감지 및 스킬 사용
        if (Input.GetKeyDown(KeyCode.T))
        {
            UseSkill(0);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            UseSkill(1);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseSkill(2);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UseSkill(3);
        }
    }

    private void UseSkill(int skillIndex)
    {
        CharacterData character = GameManager.Instance.CurrentCharacter;
        if (character.skills != null && skillIndex >= 0 && skillIndex < character.skills.Length)
        {
            CharacterSkill skill = character.skills[skillIndex];

            // 스킬 쿨타임 확인
            if (skillCooldownTimers[skillIndex] <= 0)
            {
                // STR 레벨에 따른 데미지 로그 추가
                Debug.Log($"캐릭터 '{character.characterName}'의 스킬 '{skill.skillName}' 사용 - 기본 데미지: {skill.skillDamage}, STR 레벨: {character.power}");

                skillManager.UseSkill(skill, transform, character); // character는 현재 선택된 캐릭터

                // 쿨타임 설정
                skillCooldownTimers[skillIndex] = skill.skillCooldown;
            }
            else
            {
                Debug.Log($"Skill {skill.skillName} is on cooldown for {skillCooldownTimers[skillIndex]:F1} more seconds.");
            }
        }
        else
        {
            Debug.LogWarning("Invalid skill index!");
        }
    }

    private void IncreaseCharacterLevel()
    {
        CharacterData character = characters[currentCharacterIndex];
        character.level++; // 레벨 증가
        SaveCharacterStats(); // 변경된 레벨 저장
        LoadCharacter(currentCharacterIndex); // 캐릭터 정보 다시 로드

        LoadUpgradePanel();
    }

    // Select 버튼 클릭
    public void OnSelectButtonClick()
    {
        //위치 랜덤 지정
        // GameManager.Instance.location = UnityEngine.Random.Range(0,6);
        // while(GameManager.Instance.location==4){
        //     GameManager.Instance.location = UnityEngine.Random.Range(0,6);
        // }

        GameManager.Instance.location = 5;
        GameManager.Instance.chapter = 1;
        GameManager.Instance.stage = 1;

        // 선택된 캐릭터의 데이터를 CharacterSelectionData에 저장
        CharacterData selectedCharacter = characters[currentCharacterIndex];
        if (selectedCharacter == null)
        {
            Debug.LogError("Selected character is null!");
            return; // 캐릭터가 null인 경우 메서드 종료
        }

        if (characterImage.sprite == null)
        {
            Debug.LogError("Selected character sprite is null!"); // 스프라이트가 null인 경우 오류 로그
            return; // 스프라이트가 null인 경우 메서드 종료
        }

        if (selectedCharacter.animatorController == null)
        {
            Debug.LogError("Selected character animator is null!"); // 스프라이트가 null인 경우 오류 로그
            return; // 애니메이터가 null인 경우 메서드 종료
        }

        if (CharacterSelectionData.Instance != null)
        {
            CharacterSelectionData.Instance.selectedCharacterSprite = characterSprite;
            CharacterSelectionData.Instance.selectedCharacterAnimator = characterAnimator;
        }


        CharacterSelectionData.Instance.selectedCharacterSprite = characterImage.sprite;
        CharacterSelectionData.Instance.selectedCharacterAnimator = selectedCharacter.animatorController;
        CharacterSelectionData.Instance.selectedCharacterData = selectedCharacter; // 선택된 캐릭터 데이터 저장
        SceneManager.LoadScene("GameScene");
    }

    private void UseCharacterSkills(CharacterData character)
    {
        foreach (var skill in character.skills)
        {
            // 각 스킬 사용 로직
            Debug.Log($"Using skill: {skill.skillName} with damage: {skill.skillDamage}");
            // 여기서 각 스킬을 실제로 사용하는 로직을 구현할 수 있습니다.
        }
    }

    public void ShowCharacterInfo(int index)
    {
        CharacterData character = characters[currentCharacterIndex];
        characterInfoPanel.SetActive(true);
        SetCharacterButtonsInteractable(false);
        LoadCharacter(index);

        if (characters[index].isUnlocked)
        {
            selectButton.interactable = true;
            upgradeButton.interactable = true;
            unlockButton.gameObject.SetActive(false); // 해금 버튼 숨김
            unlockConditionText.text = $"해금 완료!!";

        }
        else
        {
            selectButton.interactable = false;
            upgradeButton.interactable = false;
            unlockButton.gameObject.SetActive(true); // 해금 버튼 표시TryUnlockCharacter
            unlockConditionText.text = $"필요한 기계조각 : {character.requiredmachineparts}\n필요한 페이지 : {character.requiredstorybookpages}";

        }
    }


    public void HideCharacterInfo()
    {
        characterInfoPanel.SetActive(false);
        SetCharacterButtonsInteractable(true);
    }

    public void TryUnlockCharacterFirebase(int index)
    {
        StartCoroutine(HandleFirebaseUnlock(index));
    }
    public void TryLevelUpCharacterFirebase(int index)
    {
        StartCoroutine(HandleFirebaseLevelUp(index));
    }
    private IEnumerator HandleFirebaseLevelUp(int index)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("❌ 로그인 정보 없음: CurrentUser is null");
            yield break;
        }
        string uid = user.UserId;
        var db = FirebaseFirestore.DefaultInstance;
        var goodsRef = db.Collection("goods").Document(uid);
        Debug.Log("레벨업 시도");

        var task = goodsRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        if (!task.Result.Exists)
        {
            Debug.LogError("❌ Firebase에 goods 데이터가 존재하지 않습니다.");
            yield break;
        }

        var data = task.Result.ToDictionary();
        int machineParts = Convert.ToInt32(data["machineparts"]);

        CharacterData character = characters[index];
        int intResult = Mathf.FloorToInt(Mathf.Sqrt(character.level));

        if (machineParts < intResult)
        {
            Debug.Log("재화 부족으로 업그레이드 불가");
            yield break;
        }

        // ✅ 재화 차감
        machineParts -= intResult;

        // ✅ Firestore에 재화 업데이트
        Dictionary<string, object> updateData = new()
    {
        { "machineparts", machineParts }
    };

        yield return goodsRef.SetAsync(updateData).AsCoroutine();
        SaveCharacterStats();
        // ✅ Firebase에 해금 상태 저장 (별도 컬렉션)
        // ✅ 로컬 상태 반영
        Increaselevel();
        requireLevelupText.text = "필요량 : " + intResult;
        LobbyUI.Instance.machinePartsText.text = machineParts.ToString();
        MachineText.text = "기계부품 보유량 : "+ LobbyUI.Instance.machinePartsText.text;
        PlayerPrefs.Save();
        Debug.Log("레벨업 성공");
    }

    private IEnumerator HandleFirebaseUnlock(int index)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("❌ 로그인 정보 없음: CurrentUser is null");
            yield break;
        }

        string uid = user.UserId;
        var db = FirebaseFirestore.DefaultInstance;
        var goodsRef = db.Collection("goods").Document(uid);

        var task = goodsRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (!task.Result.Exists)
        {
            Debug.LogError("❌ Firebase에 goods 데이터가 존재하지 않습니다.");
            yield break;
        }

        var data = task.Result.ToDictionary();
        int storybookPages = Convert.ToInt32(data["storybookpages"]);
        int machineParts = Convert.ToInt32(data["machineparts"]);

        CharacterData character = characters[index];

        if (storybookPages < character.requiredstorybookpages || machineParts < character.requiredmachineparts)
        {
            Debug.Log("재화 부족으로 해금 불가");
            yield break;
        }

        // ✅ 재화 차감
        storybookPages -= character.requiredstorybookpages;
        machineParts -= character.requiredmachineparts;

        // ✅ Firestore에 재화 업데이트
        Dictionary<string, object> updateData = new()
    {
        { "storybookpages", storybookPages },
        { "machineparts", machineParts }
    };

        yield return goodsRef.SetAsync(updateData).AsCoroutine();

        // ✅ Firebase에 해금 상태 저장 (별도 컬렉션)
        var unlockRef = db.Collection("unlockedCharacters").Document(uid);
        Dictionary<string, object> unlockData = new()
    {
        { $"char_{character.characterName}", true }
    };
        yield return unlockRef.SetAsync(unlockData, SetOptions.MergeAll).AsCoroutine();

        // ✅ 로컬 상태 반영
        character.isUnlocked = true;
        PlayerPrefs.SetInt("CharacterUnlocked_" + index, 1);
        PlayerPrefs.Save();

        Button[] buttons = characterContainer.GetComponentsInChildren<Button>();
        if (index >= 0 && index < buttons.Length)
        {
            Image img = buttons[index].GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.white; // 밝게 표시
            }
        }

        ShowCharacterInfo(index);
        Debug.Log($"🎉 캐릭터 {character.characterName} 해금 완료");
    }

    public void LoadCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError("Character index out of range!");
            return;
        }

        currentCharacterIndex = index;
        CharacterData character = characters[index];
        characterImage.sprite = character.characterSprite;
        characterImage.color = character.isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

        if (character == null)
        {
            Debug.LogError("CharacterData is null for index: " + index);
            return;
        }

        // PlayerPrefs에서 캐릭터 속성 로드
        character.level = PlayerPrefs.GetInt("CharacterLevel_" + currentCharacterIndex, 1);
        character.vitality = PlayerPrefs.GetInt("CharacterVitality_" + currentCharacterIndex, 0);
        character.power = PlayerPrefs.GetInt("CharacterPower_" + currentCharacterIndex, 0);
        character.agility = PlayerPrefs.GetInt("CharacterAgility_" + currentCharacterIndex, 0);
        character.luck = PlayerPrefs.GetInt("CharacterLuck_" + currentCharacterIndex, 0);
        character.isUnlocked = PlayerPrefs.GetInt("CharacterUnlocked_" + currentCharacterIndex, currentCharacterIndex == 1 ? 1 : 0) == 1;

        // 스킬 로드
        LoadCharacterSkills(character);

        CharacterSelectionData.Instance.selectedCharacterSprite = character.characterSprite;
        characterImage.sprite = character.characterSprite;
        characterNameText.text = character.characterName;
        descriptionText.text = character.description;
        levelText.text = "Level: " + character.level;

        // 업그레이드 창에 동일하게 표시
        upgradeNameText.text = character.characterName;
        upgradeLevelText.text = "Level: " + character.level;
        vitalityText.text = "VIT: " + character.vitality;
        powerText.text = "POW: " + character.power;
        agilityText.text = "AGI: " + character.agility;
        luckText.text = "LUK: " + character.luck;
        
        currentCharacterIndex = index;

        // 레벨 값의 -1과 비교
        int totalIncrease = character.vitality + character.power + character.agility + character.luck;

        if (totalIncrease == character.level - 1)
        {
            // 모든 버튼 비활성화
            vitalityUpgradeButton.interactable = false;
            powerUpgradeButton.interactable = false;
            agilityUpgradeButton.interactable = false;
            luckUpgradeButton.interactable = false;
        }
        else if (totalIncrease < character.level - 1)
        {
            // 각 속성의 증가량이 5인 경우 해당 버튼 비활성화
            vitalityUpgradeButton.interactable = character.vitality < 5;
            powerUpgradeButton.interactable = character.power < 5;
            agilityUpgradeButton.interactable = character.agility < 5;
            luckUpgradeButton.interactable = character.luck < 5;
        }

        Debug.Log($"Total Increase: {totalIncrease}, Level: {character.level}, Vitality: {character.vitality}, Power: {character.power}, Agility: {character.agility}, Luck: {character.luck}");
    }

    private void LoadCharacterSkills(CharacterData character)
    {
        if (character.skills != null && character.skills.Length > 0)
        {
            foreach (var skill in character.skills)
            {
                if (skill == null || skill.skillName == "none") // 스킬이 null이거나 "none"인 경우
                {
                    Debug.LogWarning("Skill is null or not implemented, setting to null.");
                    // 스킬을 null로 설정
                    continue; // 다음 스킬로 넘어감
                }

                Debug.Log($"Loaded skill: {skill.skillName}");
            }
        }
        else
        {
            Debug.LogWarning("No skills found for this character.");
        }
    }

    public void ShowUpgradePanel()
    {
        characterInfoPanel.SetActive(false); // 캐릭터 정보 창 숨기기
        upgradePanel.SetActive(true);
        SetCharacterButtonsInteractable(false);
        LoadUpgradePanel();
    }

    public void LoadUpgradePanel()
    {
        Debug.Log($"Current Character Index: {currentCharacterIndex}");
        Debug.Log($"Characters Array Length: {characters.Length}");

        if (currentCharacterIndex < 0 || currentCharacterIndex >= characters.Length)
        {
            Debug.LogError("Current character index is out of bounds!");
            return;
        }

        CharacterData character = characters[currentCharacterIndex];

        if (character == null)
        {
            Debug.LogError("Character data is null!");
            return;
        }

        // UI 요소가 null인지 확인
        if (upgradeCharacterImage == null || upgradeNameText == null || upgradeLevelText == null ||
            vitalityText == null || powerText == null || agilityText == null || luckText == null)
        {
            Debug.LogError("One or more UI elements are not assigned!");
            return;
        }

        // 캐릭터 정보 로드
        upgradeCharacterImage.sprite = character.characterSprite;
        upgradeNameText.text = character.characterName;
        upgradeLevelText.text = "Level: " + character.level;
        vitalityText.text = "VIT: " + character.vitality;
        powerText.text = "POW: " + character.power;
        agilityText.text = "AGI: " + character.agility;
        luckText.text = "LUK: " + character.luck;
        requireLevelupText.text = "필요량 : " +Mathf.FloorToInt(Mathf.Sqrt(character.level));
        MachineText.text = "기계부품 보유량 : "+ LobbyUI.Instance.machinePartsText.text;
        

        // 레벨 값의 -1과 비교
        int totalIncrease = character.vitality + character.power + character.agility + character.luck;
        ableupgrade.text = "업그레이드 가능치 : " + (character.level -1 -totalIncrease).ToString();
        if (totalIncrease == character.level - 1)
        {
            // 모든 버튼 비활성화
            vitalityUpgradeButton.interactable = false;
            powerUpgradeButton.interactable = false;
            agilityUpgradeButton.interactable = false;
            luckUpgradeButton.interactable = false;
        }
        else if (totalIncrease < character.level - 1)
        {
            // 각 속성의 증가량이 5인 경우 해당 버튼 비활성화
            vitalityUpgradeButton.interactable = character.vitality < 5;
            powerUpgradeButton.interactable = character.power < 5;
            agilityUpgradeButton.interactable = character.agility < 5;
            luckUpgradeButton.interactable = character.luck < 5;
        }

        Debug.Log($"Total Increase: {totalIncrease}, Level: {character.level}, Vitality: {character.vitality}, Power: {character.power}, Agility: {character.agility}, Luck: {character.luck}");
    }
    public void Increaselevel()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.vitality < 999)
        {
            character.level++;
            levelText.text = "LV. " + character.level;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }
        

        LoadUpgradePanel();
    }

    public void IncreaseVitality()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.vitality < 5)
        {
            character.vitality++;
            vitalityText.text = "VIT: " + character.vitality;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }


    public void IncreasePower()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.power < 5)
        {
            character.power++;
            powerText.text = "POW: " + character.power;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }

    public void IncreaseAgility()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.agility < 5)
        {
            character.agility++;
            agilityText.text = "AGI: " + character.agility;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }

    public void IncreaseLuck()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character.luck < 5)
        {
            character.luck++;
            luckText.text = "LUK: " + character.luck;
            SaveCharacterStats(); // 캐릭터 속성 저장
        }

        LoadUpgradePanel();
    }


    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);       // 업그레이드 창 숨기기
        characterInfoPanel.SetActive(true);  // 캐릭터 정보 창 다시 표시
        LoadCharacter(currentCharacterIndex); // 현재 선택된 캐릭터 정보 다시 로드
    }

    private void SetCharacterButtonsInteractable(bool state)
    {
        foreach (Button button in characterButtons)
        {
            button.interactable = state;
        }
    }


    // 캐릭터 속성을 PlayerPrefs에 저장하는 메서드
    public void SaveCharacterStats(bool saveToFirebase = true)
    {

        CharacterData character = characters[currentCharacterIndex];
        PlayerPrefs.SetInt("CharacterLevel_" + currentCharacterIndex, character.level);
        PlayerPrefs.SetInt("CharacterVitality_" + currentCharacterIndex, character.vitality);
        PlayerPrefs.SetInt("CharacterPower_" + currentCharacterIndex, character.power);
        PlayerPrefs.SetInt("CharacterAgility_" + currentCharacterIndex, character.agility);
        PlayerPrefs.SetInt("CharacterLuck_" + currentCharacterIndex, character.luck);
        PlayerPrefs.Save();

        if (saveToFirebase)
        {
            SaveCharacterStatsToFirebase();
        }
    }


    public IEnumerator LoadUnlockedCharactersFromFirebase(Action onComplete)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("❌ 로그인 정보 없음: CurrentUser is null");
            yield break;
        }

        string uid = user.UserId;
        var db = FirebaseFirestore.DefaultInstance;
        var unlockRef = db.Collection("unlockedCharacters").Document(uid);

        var task = unlockRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (!task.Result.Exists)
        {
            Debug.LogWarning("🔐 Firebase에 해금 캐릭터 정보 없음.");

            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].isUnlocked = (i == 1); // 견우(index==1)만 해금
                PlayerPrefs.SetInt("CharacterUnlocked_" + i, characters[i].isUnlocked ? 1 : 0);

                if (i < characterButtons.Count)
                {
                    Image img = characterButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = characters[i].isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                }
            }
            PlayerPrefs.Save();

            onComplete?.Invoke();
            yield break;
        }

        var data = task.Result.ToDictionary();

        for (int i = 0; i < characters.Length; i++)
        {
            string key = $"char_{characters[i].characterName}";
            if (data.ContainsKey(key) && data[key] is bool unlocked && unlocked)
            {
                characters[i].isUnlocked = true;
            }
            else
            {
                characters[i].isUnlocked = (i == 1); // 견우만 기본 해금
            }

            PlayerPrefs.SetInt("CharacterUnlocked_" + i, characters[i].isUnlocked ? 1 : 0);

            if (i < characterButtons.Count)
            {
                Image img = characterButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = characters[i].isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
        }
        PlayerPrefs.Save();
        Debug.Log("✅ 해금 캐릭터 정보 로딩 완료");
        onComplete?.Invoke();
    }

    /*private void SetCharacterSkills(CharacterData character)
    {
        if (character.characterName == "Gyeonu")
        {
            character.skills = new CharacterSkill[4];

            character.skills[0] = new CharacterSkill
            {
                skillName = "Fireball",
                skillDamage = 20,
                skillCooldown = 5,
                effectRadius = 3.0f,
                effectType = CharacterSkill.EffectType.Damage,
                effectValue = 20
            };

            character.skills[1] = new CharacterSkill
            {
                skillName = "Heal",
                skillDamage = 0,
                skillCooldown = 10,
                effectType = CharacterSkill.EffectType.Heal,
                effectValue = 15
            };

            character.skills[2] = new CharacterSkill
            {
                skillName = "Speed Boost",
                skillDamage = 0,
                skillCooldown = 8,
                effectType = CharacterSkill.EffectType.Buff,
                effectValue = 5
            };

            character.skills[3] = new CharacterSkill
            {
                skillName = "Poison",
                skillDamage = 10,
                skillCooldown = 6,
                effectType = CharacterSkill.EffectType.Debuff,
                effectValue = 10
            };
        }
    }*/

    // 인스펙터용 디버그 정보 업데이트
    private void UpdateDebugInfo()
    {
        CharacterData character = characters[currentCharacterIndex];
        if (character != null)
        {
            selectedCharacterName = character.characterName;
            selectedPowerLevel = character.power;
            damageMultiplier = 1 + (selectedPowerLevel * 0.1f);

            // 스킬 데미지 정보 업데이트
            skillDebugInfos.Clear();

            if (character.skills != null)
            {
                foreach (CharacterSkill skill in character.skills)
                {
                    if (skill != null)
                    {
                        int baseDamage = skill.skillDamage;
                        int calculatedDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
                        skillDebugInfos.Add(new SkillDebugInfo(skill.skillName, baseDamage, calculatedDamage));
                    }
                }
            }
        }
    }
    //캐릭터 스텟 firebase에 저장하기 
    private void SaveCharacterStatsToFirebase()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("❌ Firebase 로그인 정보 없음");
            return;
        }

        string uid = user.UserId;
        CharacterData character = characters[currentCharacterIndex];
        string charKey = $"char_{character.characterName}";

        Dictionary<string, object> statData = new()
    {
        { "level", character.level },
        { "vitality", character.vitality },
        { "power", character.power },
        { "agility", character.agility },
        { "luck", character.luck }       
    };

        var docRef = FirebaseFirestore.DefaultInstance
            .Collection("characterStats")
            .Document(uid);

        Dictionary<string, object> update = new()
    {
        { charKey, statData }
    };

        docRef.SetAsync(update, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"✅ Firebase에 캐릭터 {charKey} 스탯 저장 완료");
            }
            else
            {
                Debug.LogError($"❌ Firebase 저장 실패: {task.Exception?.Message}");
            }
        });
    }
    // 캐릭터 스텟 firebase에서 불러오기
    private IEnumerator LoadCharacterStatsFromFirebase()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("❌ 로그인 안됨");
            yield break;

        }

        string uid = user.UserId;
        var docRef = FirebaseFirestore.DefaultInstance
            .Collection("characterStats")
            .Document(uid);

        var task = docRef.GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (!task.Result.Exists)
        {
            Debug.Log("🔸 Firebase에 캐릭터 스탯 문서 없음");
            yield break;
        }

        var data = task.Result.ToDictionary();

        for (int i = 0; i < characters.Length; i++)
        {
            string charKey = $"char_{characters[i].characterName}";
            if (data.TryGetValue(charKey, out object value) && value is Dictionary<string, object> stats)
            {
                characters[i].level = Convert.ToInt32(stats["level"]);
                characters[i].vitality = Convert.ToInt32(stats["vitality"]);
                characters[i].power = Convert.ToInt32(stats["power"]);
                characters[i].agility = Convert.ToInt32(stats["agility"]);
                characters[i].luck = Convert.ToInt32(stats["luck"]);

                // ✅ 추가

                // PlayerPrefs 동기화
                PlayerPrefs.SetInt("CharacterLevel_" + i, characters[i].level);
                PlayerPrefs.SetInt("CharacterVitality_" + i, characters[i].vitality);
                PlayerPrefs.SetInt("CharacterPower_" + i, characters[i].power);
                PlayerPrefs.SetInt("CharacterAgility_" + i, characters[i].agility);
                PlayerPrefs.SetInt("CharacterLuck_" + i, characters[i].luck);
                }
            }

        PlayerPrefs.Save();
        firebaseDataLoaded = true;

        Debug.Log("✅ Firebase에서 캐릭터 스탯 불러오기 + PlayerPrefs 동기화 완료");
    }
    private IEnumerator LoadCharacterStatsFromFirebaseThenInit()
    {
        yield return StartCoroutine(LoadCharacterStatsFromFirebase());

        for (int index = 0; index < characters.Length; index++)
        {
            characters[index].level = PlayerPrefs.GetInt("CharacterLevel_" + index, 1);
            characters[index].vitality = PlayerPrefs.GetInt("CharacterVitality_" + index, 0);
            characters[index].power = PlayerPrefs.GetInt("CharacterPower_" + index, 0);
            characters[index].agility = PlayerPrefs.GetInt("CharacterAgility_" + index, 0);
            characters[index].luck = PlayerPrefs.GetInt("CharacterLuck_" + index, 0);
        }

        LoadCharacter(0); // 최종 UI 반영
    }

}