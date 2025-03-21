using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float effectRadius = 17f; // effectRadius 변수 추가 (필요시 조정)
    private Vector3 fireballPosition; // Fireball 스킬 사용 시 위치 저장

    public void UseSkill(CharacterSkill skill, Transform characterTransform)
    {
        Debug.Log($"Using skill: {skill.skillName} with damage: {skill.skillDamage} or effect value: {skill.effectValue}");
        ApplySkillEffect(skill, characterTransform);
    }

    private void ApplySkillEffect(CharacterSkill skill, Transform characterTransform)
    {
        // 공통 변수 선언
        Vector3 cameraPosition;
        Vector3 forwardDirection;
        bool isFlipped;
        int enemyCount;

        switch (skill.skillName)
        {
            case "Fireball":
                // 카메라의 위치를 사용하여 현재 씬에서 Tag가 Enemy인 오브젝트 감지
                cameraPosition = Camera.main.transform.position; // 카메라의 위치 가져오기
                fireballPosition = cameraPosition; // Fireball 위치 저장
                
                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                enemyCount = 0;

                // 캐릭터가 바라보는 방향
                forwardDirection = characterTransform.forward;

                // flipX 상태 확인
                isFlipped = characterTransform.localScale.x < 0; // flipX가 되어있는지 확인

                // 범위 내의 적 찾기
                foreach (GameObject enemy in enemies)
                {
                    // 카메라 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - cameraPosition; // 적까지의 방향 벡터
                    float distance = directionToEnemy.magnitude; // 거리 계산

                    // 거리와 방향을 기준으로 양수와 음수로 판단
                    float dotProduct = Vector3.Dot(forwardDirection, directionToEnemy.normalized);

                    // flipX 상태에 따라 거리 인식 조정
                    if (distance <= skill.effectRadius && dotProduct > 0) // 범위 내에 있고, 바라보는 방향에 있는 경우
                    {
                        // flipX가 되어있으면 왼쪽 방향만 인식
                        if (isFlipped && directionToEnemy.x < 0)
                        {
                            enemyCount++;
                            if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                            {
                                meleeEnemy.TakeDamage(skill.skillDamage); // MeleeEnemy의 currentHealth 감소
                            }
                            else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                            {
                                rangedEnemy.TakeDamage(skill.skillDamage); // RangedEnemy의 currentHealth 감소
                            }
                        }
                        // flipX가 안되어있으면 오른쪽 방향만 인식
                        else if (!isFlipped && directionToEnemy.x > 0)
                        {
                            enemyCount++;
                            if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                            {
                                meleeEnemy.TakeDamage(skill.skillDamage); // MeleeEnemy의 currentHealth 감소
                            }
                            else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                            {
                                rangedEnemy.TakeDamage(skill.skillDamage); // RangedEnemy의 currentHealth 감소
                            }
                        }
                    }
                }

                Debug.Log($"Number of enemies hit by Fireball: {enemyCount}");
                break;

            case "Lightning":
                // Lightning 스킬 효과
                Debug.Log("Casting Lightning!");
                // Lightning 애니메이션 및 효과 적용
                break;

            case "Poison":
                // 카메라의 위치를 사용하여 현재 씬에서 Tag가 Enemy인 오브젝트 감지
                cameraPosition = Camera.main.transform.position; // 카메라의 위치 가져오기
                Vector3 poisonPosition = cameraPosition; // Poison 위치 저장
                
                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_poison = GameObject.FindGameObjectsWithTag("Enemy");
                enemyCount = 0;
                
                Debug.Log($"Found {enemies_poison.Length} enemies in the scene for Poison skill");

                // 캐릭터가 바라보는 방향
                forwardDirection = characterTransform.forward;

                // flipX 상태 확인
                isFlipped = characterTransform.localScale.x < 0; // flipX가 되어있는지 확인
                Debug.Log($"Character is flipped: {isFlipped}, Forward direction: {forwardDirection}");

                // 범위 내의 적 찾기
                foreach (GameObject enemy in enemies_poison)
                {
                    // 카메라 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - cameraPosition; // 적까지의 방향 벡터
                    float distance = directionToEnemy.magnitude; // 거리 계산

                    // 거리와 방향을 기준으로 양수와 음수로 판단
                    float dotProduct = Vector3.Dot(forwardDirection, directionToEnemy.normalized);
                    
                    Debug.Log($"Enemy: {enemy.name}, Distance: {distance}, Direction: {directionToEnemy}, DotProduct: {dotProduct}");

                    // flipX 상태에 따라 거리 인식 조정
                    if (distance <= skill.effectRadius && dotProduct > 0) // 범위 내에 있고, 바라보는 방향에 있는 경우
                    {
                        // flipX가 되어있으면 왼쪽 방향만 인식
                        if (isFlipped && directionToEnemy.x < 0)
                        {
                            enemyCount++;
                            Debug.Log($"Enemy {enemy.name} is in range and to the left of flipped character");
                            if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                            {
                                Debug.Log($"Starting poison effect on MeleeEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectMelee(skill, enemy, 5));
                            }
                            else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                            {
                                Debug.Log($"Starting poison effect on RangedEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectRanged(skill, enemy, 5));
                            }
                        }
                        // flipX가 안되어있으면 오른쪽 방향만 인식
                        else if (!isFlipped && directionToEnemy.x > 0)
                        {
                            enemyCount++;
                            Debug.Log($"Enemy {enemy.name} is in range and to the right of non-flipped character");
                            if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                            {
                                Debug.Log($"Starting poison effect on MeleeEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectMelee(skill, enemy, 5));
                            }
                            else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                            {
                                Debug.Log($"Starting poison effect on RangedEnemy: {enemy.name}");
                                StartCoroutine(ApplyPoisonEffectRanged(skill, enemy, 5));
                            }
                        }
                        else
                        {
                            Debug.Log($"Enemy {enemy.name} is in range but not in the correct direction. isFlipped: {isFlipped}, directionToEnemy.x: {directionToEnemy.x}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Enemy {enemy.name} is not in range or not in front of character. Distance: {distance}, DotProduct: {dotProduct}, EffectRadius: {skill.effectRadius}");
                    }
                }

                Debug.Log($"Number of enemies hit by Poison: {enemyCount}");
                break;
                
            default: // Heal 계열 스킬
                ApplyDefaultEffect(skill, characterTransform);
                break;
        }
    }

    private void ApplyDefaultEffect(CharacterSkill skill, Transform characterTransform)
    {
        switch (skill.effectType)
        {
            case CharacterSkill.EffectType.Damage:
            case CharacterSkill.EffectType.Debuff:
                // 유효 범위 내의 적 찾기
                Collider[] hitColliders = Physics.OverlapSphere(characterTransform.position, skill.effectRadius);
                foreach (var hitCollider in hitColliders)
                {
                    // 적에게 데미지 주는 로직
                    if (hitCollider.CompareTag("Enemy"))
                    {
                        Debug.Log($"Dealing {skill.effectValue} damage to {hitCollider.name}.");
                        // 적에게 데미지 적용 로직 추가
                    }
                }
                break;
            case CharacterSkill.EffectType.Heal:
                int healAmount = CalculateHealAmount(skill.effectValue);
                GameManager.Instance.ModifyHealth(healAmount);
                Debug.Log($"Healing for {healAmount} health.");
                break;
            case CharacterSkill.EffectType.Buff:
                // 캐릭터 버프 적용 로직
                Debug.Log($"Buffing character with {skill.effectValue}.");
                break;
            default:
                break;
        }
    }

    private IEnumerator ApplyPoisonEffectMelee(CharacterSkill skill, GameObject enemy, int effectTime)
    {
        // effectValue를 float로 변환
        float damagePerSecond;
        if (!float.TryParse(skill.effectValue, out damagePerSecond))
        {
            Debug.LogError("Invalid effect value for poison damage. Please check the value.");
            yield break; // 변환 실패 시 코루틴 종료
        }

        // 적이 MeleeEnemy 컴포넌트를 가지고 있는지 확인
        MeleeEnemy meleeEnemy = enemy.GetComponent<MeleeEnemy>();
        if (meleeEnemy == null)
        {
            Debug.LogError($"Enemy {enemy.name} does not have MeleeEnemy component.");
            yield break; // MeleeEnemy 컴포넌트가 없으면 코루틴 종료
        }

        Debug.Log($"Starting poison effect on MeleeEnemy: {enemy.name} for {effectTime} seconds with {damagePerSecond} damage per second");
        float elapsedTime = 0f; // 경과 시간

        // 독 데미지 적용 시작 시 현재 체력 기록
        float initialHealth = meleeEnemy.currentHealth;
        Debug.Log($"MeleeEnemy {enemy.name} initial health: {initialHealth}");

        while (elapsedTime < effectTime && enemy != null && enemy.activeInHierarchy)
        {
            // 적에게 독 데미지 적용
            meleeEnemy.TakeDamage(damagePerSecond);
            Debug.Log($"MeleeEnemy {enemy.name} took {damagePerSecond} poison damage. Remaining time: {effectTime - elapsedTime}s, Current health: {meleeEnemy.currentHealth}, Health change: {initialHealth - meleeEnemy.currentHealth}");

            elapsedTime += 1f; // 1초 경과
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        
        Debug.Log($"Poison effect on MeleeEnemy: {enemy.name} has ended. Total damage: {initialHealth - meleeEnemy.currentHealth}");
    }

    private IEnumerator ApplyPoisonEffectRanged(CharacterSkill skill, GameObject enemy, int effectTime)
    {
        // effectValue를 float로 변환
        float damagePerSecond;
        if (!float.TryParse(skill.effectValue, out damagePerSecond))
        {
            Debug.LogError("Invalid effect value for poison damage. Please check the value.");
            yield break; // 변환 실패 시 코루틴 종료
        }

        // 적이 RangedEnemy 컴포넌트를 가지고 있는지 확인
        RangedEnemy rangedEnemy = enemy.GetComponent<RangedEnemy>();
        if (rangedEnemy == null)
        {
            Debug.LogError($"Enemy {enemy.name} does not have RangedEnemy component.");
            yield break; // RangedEnemy 컴포넌트가 없으면 코루틴 종료
        }

        Debug.Log($"Starting poison effect on RangedEnemy: {enemy.name} for {effectTime} seconds with {damagePerSecond} damage per second");
        float elapsedTime = 0f; // 경과 시간

        // 독 데미지 적용 시작 시 현재 체력 기록
        float initialHealth = rangedEnemy.currentHealth;
        Debug.Log($"RangedEnemy {enemy.name} initial health: {initialHealth}");

        while (elapsedTime < effectTime && enemy != null && enemy.activeInHierarchy)
        {
            // 적에게 독 데미지 적용
            rangedEnemy.TakeDamage(damagePerSecond);
            Debug.Log($"RangedEnemy {enemy.name} took {damagePerSecond} poison damage. Remaining time: {effectTime - elapsedTime}s, Current health: {rangedEnemy.currentHealth}, Health change: {initialHealth - rangedEnemy.currentHealth}");

            elapsedTime += 1f; // 1초 경과
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        
        Debug.Log($"Poison effect on RangedEnemy: {enemy.name} has ended. Total damage: {initialHealth - rangedEnemy.currentHealth}");
    }

    private int CalculateHealAmount(string effectValue)
    {
        if (effectValue.EndsWith("%"))
        {
            // 퍼센트로 해석
            int percentage = int.Parse(effectValue.TrimEnd('%'));
            return (int)(GameManager.Instance.MaxHealth * (percentage / 100f));
        }
        else
        {
            // 정수로 해석
            return int.Parse(effectValue);
        }
    }

    private void OnDrawGizmos()
    {
        // 기즈모 색상 설정
        Gizmos.color = Color.blue;
        // 기즈모로 effectRadius 범위 그리기
        Gizmos.DrawWireSphere(fireballPosition, effectRadius); // Fireball 위치에 기즈모 그리기
    }
}
