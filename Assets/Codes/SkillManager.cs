using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 타격 이펙트 프리팹 로드
        hitEffectPrefab = Resources.Load<GameObject>("Prefabs/Particle/HitEffect");
        // Fireball 이펙트 프리팹 로드
        fireballEffectPrefab = Resources.Load<GameObject>("Prefabs/Particle/Fireball");
        // Ultimo 이펙트 프리팹 로드
        ultimoEffectPrefab = Resources.Load<GameObject>("Prefabs/Particle/Ultimo");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 fireballPosition; // Fireball 스킬 사용 시 위치 저장
    private Vector3 ultimoPosition; // Ultimo 스킬 사용 시 위치 저장
    private Vector3 baseAttackPosition; // BaseG 스킬 사용 시 위치 저장
    private Vector3 closestEnemyPosition; // 가장 가까운 적의 위치 저장
    private GameObject hitEffectPrefab; // 타격 이펙트 프리팹
    private GameObject fireballEffectPrefab; // Fireball 이펙트 프리팹

    private GameObject ultimoEffectPrefab; // Ultimo 이펙트 프리팹

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
            case "BaseG":
                // Player(Clone) 오브젝트 찾기
                GameObject playerObject = GameObject.Find("Player(Clone)");
                if (playerObject == null)
                {
                    Debug.LogError("Player(Clone) object not found!");
                    return;
                }

                baseAttackPosition = playerObject.transform.position; // 공격 위치 저장

                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                float closestDistance = float.MaxValue;
                GameObject closestEnemy = null;

                // Player(Clone) 오브젝트 찾기
                SpriteRenderer baseGSpriteRenderer = playerObject.GetComponent<SpriteRenderer>();
                isFlipped = baseGSpriteRenderer != null && baseGSpriteRenderer.flipX;
                Debug.Log($"Player found - flipX: {isFlipped}, SpriteRenderer: {(baseGSpriteRenderer != null ? "Found" : "Not Found")}");

                // 가장 가까운 적 찾기
                foreach (GameObject enemy in enemies)
                {
                    // 캐릭터 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - playerObject.transform.position; // 플레이어 기준으로 적까지의 방향 벡터
                    float distance = directionToEnemy.magnitude; // 거리 계산

                    // Y좌표 차이 계산
                    float yDifference = Mathf.Abs(enemy.transform.position.y - playerObject.transform.position.y);
                    float maxYDifference = 2f; // 최대 허용 Y좌표 차이

                    // 디버그 로그 추가
                    Debug.Log($"Enemy: {enemy.name}, Distance: {distance}, Direction: {directionToEnemy}, isFlipped: {isFlipped}");

                    // flipX 상태에 따라 거리 인식 조정
                    if (distance <= skill.effectRadius && yDifference <= maxYDifference) // 범위 내에 있고, Y좌표 차이가 허용 범위 내인 경우
                    {
                        // flipX가 되어있으면 왼쪽 방향만 인식
                        if (isFlipped && directionToEnemy.x < 0)
                        {
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestEnemy = enemy;
                            }
                        }
                        // flipX가 안되어있으면 오른쪽 방향만 인식
                        else if (!isFlipped && directionToEnemy.x > 0)
                        {
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestEnemy = enemy;
                            }
                        }
                        else
                        {
                            Debug.Log($"Enemy not in correct direction: {enemy.name}, isFlipped: {isFlipped}, Direction: {directionToEnemy.x}");
                        }
                    }
                }

                // 가장 가까운 적에게 데미지 적용
                if (closestEnemy != null)
                {
                    closestEnemyPosition = closestEnemy.transform.position; // 가장 가까운 적의 위치 저장
                    // 타격 이펙트 생성
                    if (hitEffectPrefab != null)
                    {
                        GameObject hitEffect = Instantiate(hitEffectPrefab, closestEnemy.transform.position, Quaternion.identity);
                        Destroy(hitEffect, 0.5f); // 0.5초 후 이펙트 제거
                    }

                    // 화면 흔들림 효과
                    //StartCoroutine(ShakeCamera(0.02f, 0.02f));

                    if (closestEnemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                    {
                        meleeEnemy.TakeDamage(skill.skillDamage);
                    }
                    else if (closestEnemy.TryGetComponent(out RangedEnemy rangedEnemy))
                    {
                        rangedEnemy.TakeDamage(skill.skillDamage);
                    }
                }
                break;

            case "Fireball":
                // Player(Clone) 오브젝트 찾기
                GameObject playerObject_fireball = GameObject.Find("Player(Clone)");
                if (playerObject_fireball == null)
                {
                    Debug.LogError("Player(Clone) object not found!");
                    return;
                }

                // 플레이어의 위치를 기준으로 Fireball 위치 설정
                fireballPosition = playerObject_fireball.transform.position;
                
                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_fireball = GameObject.FindGameObjectsWithTag("Enemy");
                enemyCount = 0;

                // 캐릭터가 바라보는 방향
                forwardDirection = characterTransform.forward;

                // Player의 SpriteRenderer 컴포넌트 가져오기
                SpriteRenderer spriteRenderer = playerObject_fireball.GetComponent<SpriteRenderer>();
                isFlipped = spriteRenderer != null && spriteRenderer.flipX;
                Debug.Log($"Player found - flipX: {isFlipped}, SpriteRenderer: {(spriteRenderer != null ? "Found" : "Not Found")}");

                // Fireball 이펙트 생성
                if (fireballEffectPrefab != null)
                {
                    // 캐릭터의 방향에 따라 회전 설정
                    float rotationZ = isFlipped ? 180f : 0f;
                    GameObject fireballEffect = Instantiate(fireballEffectPrefab, fireballPosition, Quaternion.Euler(0f, 0f, rotationZ));
                    Destroy(fireballEffect, 0.4f); // 0.4초 후 이펙트 제거
                }

                // 범위 내의 적 찾기
                foreach (GameObject enemy in enemies_fireball)
                {
                    // 캐릭터 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - playerObject_fireball.transform.position; // 플레이어 기준으로 적까지의 방향 벡터
                    float distance = directionToEnemy.magnitude; // 거리 계산

                    // Y좌표 차이 계산
                    float yDifference = Mathf.Abs(enemy.transform.position.y - playerObject_fireball.transform.position.y);
                    float maxYDifference = 4f; // 최대 허용 Y좌표 차이

                    // 디버그 로그 추가
                    Debug.Log($"Enemy: {enemy.name}, Distance: {distance}, Direction: {directionToEnemy}, isFlipped: {isFlipped}, Player Position: {playerObject_fireball.transform.position}, Enemy Position: {enemy.transform.position}");

                    // flipX 상태에 따라 거리 인식 조정
                    if (distance <= skill.effectRadius && yDifference <= maxYDifference) // 범위 내에 있고, Y좌표 차이가 허용 범위 내인 경우
                    {
                        // flipX가 되어있으면 왼쪽 방향만 인식
                        if (isFlipped && directionToEnemy.x < 0)
                        {
                            Debug.Log($"Hit left enemy: {enemy.name}, Direction: {directionToEnemy.x}");
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
                            Debug.Log($"Hit right enemy: {enemy.name}, Direction: {directionToEnemy.x}");
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
                        else
                        {
                            Debug.Log($"Enemy not in correct direction: {enemy.name}, isFlipped: {isFlipped}, Direction: {directionToEnemy.x}");
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
                
            case "Ultimo":
                // Player(Clone) 오브젝트 찾기
                GameObject playerObject_ultimo = GameObject.Find("Player(Clone)");
                if (playerObject_ultimo == null)
                {
                    Debug.LogError("Player(Clone) object not found!");
                    return;
                }

                // 플레이어의 위치를 기준으로 Ultimo 위치 설정
                ultimoPosition = playerObject_ultimo.transform.position;

                // Ultimo 이펙트 생성
                if (ultimoEffectPrefab != null)
                {
                    GameObject ultimoEffect = Instantiate(ultimoEffectPrefab, ultimoPosition, Quaternion.identity);
                    Destroy(ultimoEffect, 1.0f); // 1초 후 이펙트 제거
                }
                
                // 모든 Enemy 오브젝트 찾기
                GameObject[] enemies_ultimo = GameObject.FindGameObjectsWithTag("Enemy");
                enemyCount = 0;

                // 범위 내의 적 찾기 - 바라보는 방향 상관없이 모든 적에게 효과 적용
                foreach (GameObject enemy in enemies_ultimo)
                {
                    // 카메라 위치와 적의 거리 계산
                    Vector3 directionToEnemy = enemy.transform.position - playerObject_ultimo.transform.position;
                    float distance = directionToEnemy.magnitude;

                    // 범위 내에 있다면 방향에 상관없이 효과 적용
                    if (distance <= skill.effectRadius)
                    {
                        enemyCount++;
                        if (enemy.TryGetComponent(out MeleeEnemy meleeEnemy))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to MeleeEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"MeleeEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(meleeEnemy, skill.skillDamage, 0.5f));
                        }
                        else if (enemy.TryGetComponent(out RangedEnemy rangedEnemy))
                        {
                            // KnockBack 효과 먼저 적용
                            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRb != null)
                            {
                                Debug.Log($"Applying knockback to RangedEnemy: {enemy.name}");
                                Vector2 knockbackDirection = new Vector2(0f, 1f); // 위로만 넉백
                                enemyRb.velocity = Vector2.zero;
                                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                                Debug.Log($"Knockback applied to {enemy.name} - Direction: {knockbackDirection}, Force: {knockbackDirection * 10f}");
                                // 위치 고정 및 해제 코루틴 시작
                                StartCoroutine(FreezePosition(enemyRb, 0.3f, 0.4f));
                            }
                            else
                            {
                                Debug.LogWarning($"RangedEnemy {enemy.name} has no Rigidbody2D component!");
                            }
                            // 1초 후 데미지 적용
                            StartCoroutine(DelayedDamage(rangedEnemy, skill.skillDamage, 0.5f));
                        }
                    }
                }

                Debug.Log($"Number of enemies hit by Ultimo: {enemyCount}");
                break;

            default: // Heal 계열 스킬
                ApplyDefaultEffect(skill, characterTransform);
                break;
        }
    }

    private void ApplyDefaultEffect(CharacterSkill skill, Transform characterTransform)
    {
        // 모든 객체의 체력 값을 로그로 출력
        PlayerController playerCtrl = FindObjectOfType<PlayerController>();
        Debug.Log($"[디버깅] SkillManager - 체력 값들: PlayerController.currentHealth = {(playerCtrl != null ? playerCtrl.CurrentHealth : -1)}, GameManager.currentPlayerHealth = {GameManager.Instance.CurrentPlayerHealth}");
        
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
                // 디버깅 로그 추가
                Debug.Log($"[디버깅] Heal 스킬 처리 시작 - PlayerController 체력: {(playerCtrl != null ? playerCtrl.CurrentHealth : -1)}, GameManager 체력: {GameManager.Instance.CurrentPlayerHealth}");
                
                // 체력 회복량 계산
                int healAmount = CalculateHealAmount(skill.effectValue);
                
                // 현재 체력은 PlayerController에서 직접 가져옴
                int playerCurrentHealth = playerCtrl != null ? playerCtrl.CurrentHealth : -1;
                
                // GameManager의 체력 값도 확인
                int gameManagerCurrentHealth = GameManager.Instance.CurrentPlayerHealth;
                
                // 두 값이 다르면 로그로 출력
                if (playerCurrentHealth != gameManagerCurrentHealth && playerCurrentHealth != -1)
                {
                    Debug.LogWarning($"[디버깅] 체력 불일치 발견! PlayerController: {playerCurrentHealth}, GameManager: {gameManagerCurrentHealth}");
                }
                
                // 실제 사용할 체력 값 결정 (PlayerController 우선)
                int currentHealth = playerCurrentHealth != -1 ? playerCurrentHealth : gameManagerCurrentHealth;
                int maxHealth = GameManager.Instance.MaxHealth;
                
                // 현재 체력 상태 기록
                Debug.Log($"Heal 스킬 사용 전 - 현재 체력: {currentHealth}, 최대 체력: {maxHealth}");
                
                // 회복 후 체력 계산 (최대 체력 초과 방지)
                int newHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
                
                // 변화량 계산
                int actualHealAmount = newHealth - currentHealth;
                
                // 체력 업데이트 (실제 변화량이 있을 때만)
                if (actualHealAmount > 0)
                {
                    Debug.Log($"[디버깅] Heal 스킬 적용 전 - PlayerController 체력: {playerCurrentHealth}, GameManager 체력: {gameManagerCurrentHealth}, 적용할 체력: {newHealth}");
                    
                    // PlayerController 직접 찾아서 체력 동기화
                    if (playerCtrl != null)
                    {
                        playerCtrl.UpdateHealth(newHealth);
                        Debug.Log($"[디버깅] PlayerController.UpdateHealth({newHealth}) 호출 완료");
                        
                        // PlayerUI 찾아서 슬라이더 직접 업데이트
                        PlayerUI playerUI = FindObjectOfType<PlayerUI>();
                        if (playerUI != null)
                        {
                            float healthPercent = (float)newHealth / maxHealth;
                            playerUI.UpdateHealthSlider(healthPercent);
                            Debug.Log($"[디버깅] PlayerUI.UpdateHealthSlider({healthPercent}) 호출 완료");
                        }
                    }
                    
                    // GameManager 체력 업데이트
                    GameManager.Instance.CurrentPlayerHealth = newHealth;
                    Debug.Log($"[디버깅] GameManager.CurrentPlayerHealth = {newHealth} 설정 완료");
                    
                    Debug.Log($"체력 회복 적용 - 현재 체력: {newHealth}, 회복량: {healAmount}, 실제 회복량: {actualHealAmount}, 최대 체력: {maxHealth}");
                    
                    // 적용 후 체력 값 확인
                    Debug.Log($"[디버깅] Heal 스킬 적용 후 - PlayerController 체력: {playerCtrl.CurrentHealth}, GameManager 체력: {GameManager.Instance.CurrentPlayerHealth}");
                }
                else
                {
                    Debug.Log($"체력이 이미 최대이거나 회복량이 0입니다. 현재 체력: {currentHealth}, 최대 체력: {maxHealth}");
                }
                break;
            case CharacterSkill.EffectType.Buff:
                // 캐릭터 버프 적용 로직
                Debug.Log($"Buffing character with {skill.effectValue}.");
                break;
            default:
                break;
        }
    }

    private int CalculateHealAmount(string effectValue)
    {
        // 현재 사용 중인 캐릭터의 최대 체력 가져오기
        int characterMaxHealth = GameManager.Instance.MaxHealth;
        
        Debug.Log($"캐릭터의 최대 체력: {characterMaxHealth}, 효과 값: {effectValue}");
        
        if (effectValue.EndsWith("%"))
        {
            // 퍼센트로 해석
            if (int.TryParse(effectValue.TrimEnd('%'), out int percentage))
            {
                int healAmount = Mathf.RoundToInt(characterMaxHealth * (percentage / 100f));
                Debug.Log($"백분율 회복: {percentage}%, 회복량: {healAmount}");
                return healAmount;
            }
            else
            {
                Debug.LogError($"유효하지 않은 회복 백분율 값: {effectValue}");
                return 0;
            }
        }
        else
        {
            // 정수로 해석
            if (int.TryParse(effectValue, out int fixedAmount))
            {
                Debug.Log($"고정 회복량: {fixedAmount}");
                return fixedAmount;
            }
            else
            {
                Debug.LogError($"유효하지 않은 회복 값: {effectValue}");
                return 0;
            }
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

    private IEnumerator DelayedDamage(MeleeEnemy enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }

    private IEnumerator DelayedDamage(RangedEnemy enemy, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.TakeDamage(damage);
    }

    private IEnumerator FreezePosition(Rigidbody2D rb, float freezeDelay, float unfreezeDelay)
    {
        // freezeDelay 초 후에 위치 고정
        yield return new WaitForSeconds(freezeDelay);
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Debug.Log($"Position frozen for {rb.gameObject.name}");
        
        // unfreezeDelay 초 후에 위치 고정 해제 (회전만 고정)
        yield return new WaitForSeconds(unfreezeDelay);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전만 고정
        Debug.Log($"Position unfrozen for {rb.gameObject.name}");
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    private void OnDrawGizmos()
    {
        // Fireball 스킬의 효과 범위 표시 (파란색)
        Gizmos.color = Color.blue;
        // 기즈모로 effectRadius 범위 그리기
        Gizmos.DrawWireSphere(fireballPosition, 15f); // Fireball 위치에 기즈모 그리기
        
        // Ultimo 스킬의 효과 범위 표시 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ultimoPosition, 50f); // Ultimo 위치에 기즈모 그리기

        // BaseG 스킬의 효과 범위 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(baseAttackPosition, 10f); // BaseG 위치에 기즈모 그리기

        // BaseG 공격과 가장 가까운 적 사이의 직선 표시 (초록색)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(baseAttackPosition, closestEnemyPosition);
    }
}
