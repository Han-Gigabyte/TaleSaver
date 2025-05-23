using System.Collections;
using UnityEngine;


public class RangedEnemy : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float attackRange;
    private float detectionRange;

    [Header("Edge Detection")]
    public float edgeCheckDistance = 1.0f; // 모서리 감지 거리
    public LayerMask platformLayer; // 플랫폼 레이어

    [Header("Damage")]

    public int baseDamage = 10; // 기본 공격력
    public DamageMultiplier damageMultiplier; // 공격력 비율을 위한 ScriptableObject
    public int attackDamage; // 공격력
    private float projectileSpeed; // 발사체 속도
    private float attackCooldown;
    protected string projectileKey = "EnemyProjectile";
    protected Transform firePoint;

    [Header("Health")]
    public float baseHealth = 80f; // 기본 체력
    public HealthMultiplier healthMultiplier; // 체력 비율을 위한 ScriptableObject
    public float calculatedHealth; // 계산된 체력
    public float currentHealth; // 현재 체력
    public bool isDead;

    private float nextAttackTime;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform playerTransform;
    private bool isPlayerInRange = false;

    [Header("Item Drop")]
    [SerializeField] private GameObject itemPrefab; // 아이템 프리팹

    void Start()
    {
        // GameManager에서 값 가져오기
        moveSpeed = GameManager.Instance.rangedEnemyMoveSpeed;
        attackRange = GameManager.Instance.rangedEnemyAttackRange;
        detectionRange = GameManager.Instance.rangedEnemyDetectionRange;
        attackCooldown = GameManager.Instance.rangedEnemyAttackCooldown;
        projectileSpeed = GameManager.Instance.rangedEnemyProjectileSpeed;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 체력과 공격력 초기화
        float healthMultiplierValue = healthMultiplier.GetHealthMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter);
        calculatedHealth = baseHealth * healthMultiplierValue;
        currentHealth = calculatedHealth;
        isDead = false;

        attackDamage = Mathf.RoundToInt(baseDamage * damageMultiplier.GetDamageMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter));

        // Rigidbody2D 설정
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 2.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 플랫폼 레이어 설정
        platformLayer = LayerMask.GetMask("Ground", "Half Tile");

        // 기존 Collider는 물리적 충돌용으로 사용
        BoxCollider2D existingCollider = GetComponent<BoxCollider2D>();
        if (existingCollider != null)
        {
            existingCollider.isTrigger = false;

            // 새로운 Trigger Collider 추가
            BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = existingCollider.size;
            triggerCollider.offset = existingCollider.offset;
        }

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        // 씬에 있는 모든 Enemy들과의 충돌을 무시
        RangedEnemy[] rangedEnemies = FindObjectsOfType<RangedEnemy>();
        MeleeEnemy[] meleeEnemies = FindObjectsOfType<MeleeEnemy>();

        foreach (var enemy in rangedEnemies)
        {
            if (enemy != this)  // 자기 자신은 제외
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
            }
        }

        foreach (var enemy in meleeEnemies)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
        }

        // firePoint 자동 생성
        GameObject firePointObj = new GameObject("FirePoint");
        firePoint = firePointObj.transform;
        firePoint.SetParent(transform);
        firePoint.localPosition = new Vector3(0f, 0f, 0f); // 발사 위치 고정

        Debug.Log($"RangedEnemy spawned with current health: {currentHealth}");
    }

    // 새로 스폰되는 Enemy들과도 충돌을 무시하기 위한 트리거 체크
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other, true);
        }
    }

    void Update()
    {
        // 플레이어가 죽었거나 없으면 더 이상 진행하지 않음
        if (PlayerController.IsDead || playerTransform == null)
        {
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                // 이동하기 전에 모서리 확인
                bool canMove = CheckGroundAhead((playerTransform.position.x > transform.position.x) ? 1f : -1f);

                if (canMove)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    StopMoving();
                }
                isPlayerInRange = false;
            }
            else
            {
                StopMoving();
                isPlayerInRange = true;

                // 공격 범위 안에 있고 쿨다운이 끝났으면 발사
                if (Time.time >= nextAttackTime)
                {
                    ShootProjectile();
                    nextAttackTime = Time.time + attackCooldown;
                    animator.SetTrigger("Attack");
                }
            }

            UpdateFacingDirection();
        }
        else
        {
            StopMoving();
            isPlayerInRange = false;
        }
    }

    // 전방에 지면이 있는지 확인하는 메서드
    private bool CheckGroundAhead(float directionX)
    {
        // 캐릭터의 발 위치 계산 (캐릭터의 바닥부분)
        Vector2 footPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);

        // 이동 방향으로의 레이캐스트 방향 설정
        Vector2 rayDirection = new Vector2(directionX, -0.5f).normalized;

        // 레이캐스트를 통해 전방의 지면 확인
        RaycastHit2D hit = Physics2D.Raycast(footPosition, rayDirection, edgeCheckDistance, platformLayer);

        // 디버그용 시각화
        Debug.DrawRay(footPosition, rayDirection * edgeCheckDistance, hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    public void ApplyMonsterData(MonsterData data)
    {
        // 몬스터 데이터 적용
        baseHealth = data.health;
        baseDamage = data.damage;
        moveSpeed = data.moveSpeed;
        

        // 스프라이트 및 애니메이션 적용
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        RefreshPolygonCollider();

        if (spriteRenderer != null)
            spriteRenderer.sprite = data.GetMonsterSprite();

        if (animator != null)
            animator.runtimeAnimatorController = data.GetAnimatorController(); ;
    }

    void RefreshPolygonCollider()
    {
        PolygonCollider2D old = GetComponent<PolygonCollider2D>();
        if (old != null) Destroy(old);

        gameObject.AddComponent<PolygonCollider2D>();
    }

    // virtual로 변경하여 오버라이드 가능하게 함
    protected void ShootProjectile()
    {
        if (PlayerController.IsDead) return;

        EnemySkillInfo skillInfo = GetComponent<EnemySkillInfo>();
        if (skillInfo == null || skillInfo.projectilePrefab == null)
        {
            Debug.LogWarning("EnemySkillInfo 누락 or projectilePrefab 없음");
            return;
        }

        // 발사 이펙트
        if (skillInfo.skillEffectPrefab != null)
        {
            Instantiate(skillInfo.skillEffectPrefab, firePoint.position, Quaternion.identity);
        }

        GameObject projectile = Instantiate(skillInfo.projectilePrefab, firePoint.position, Quaternion.identity);

        EnemyProjectile projectileComp = projectile.GetComponent<EnemyProjectile>();
        if (projectileComp != null)
        {
            Vector3 adjustedTarget = playerTransform.position + new Vector3(0f, 0.4f, 0f);
            Vector2 direction = (adjustedTarget - firePoint.position).normalized;

            projectileComp.Initialize(direction, skillInfo.projectileSpeed, attackDamage);

            // 이펙트 프리팹을 Projectile에게 전달
            projectileComp.SetHitEffect(skillInfo.hitEffectPrefab);
        }
    }


    void MoveTowardsPlayer()
    {
        // x축 방향으로만 이동하도록 수정
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y); // Y속도 유지
        animator.SetTrigger("Walk");
    }

    void StopMoving()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetTrigger("Idle");
    }

    void UpdateFacingDirection()
    {
        // 플레이어가 왼쪽에 있으면 true, 오른쪽에 있으면 false
        spriteRenderer.flipX = playerTransform.position.x > transform.position.x;
    }

    // 디버그용 시각화
    void OnDrawGizmosSelected()
    {
        // 공격 범위 표시 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 감지 범위 표시 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // 현재 플레이어가 공격 범위 안에 있는지 확인하는 프로퍼티
    public bool IsPlayerInRange => isPlayerInRange;

    // 체력을 변경하는 메서드 예시
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 데미지를 받아 현재 체력 감소
        Debug.Log($"RangedEnemy took damage: {damage}. Current health: {currentHealth}");
        /*
        // 데미지 인디케이터 표시
        if (DamageIndicatorManager.Instance != null)
        {
            DamageIndicatorManager.Instance.ShowDamageIndicator(transform.position, Mathf.RoundToInt(damage), false);
        }
        */
        int i = Random.Range(0, 2);
        if (i == 0)
        {
            BGMManager.instance.PlaySE(BGMManager.instance.demagedSE, 0.5f);
        }
        else
        {
            BGMManager.instance.PlaySE(BGMManager.instance.demagedSE2, 0.5f);
        }

        if (currentHealth <= 0 && !isDead)
        {
            Die(); // 체력이 0 이하가 되면 사망 처리
        }
    }

    private void Die()
    {
        PortalManager.Instance.killEnemy(1);
        StopMoving();
        animator.SetTrigger("Dead");
        isDead = true;
        Debug.Log("RangedEnemy died.");

        // Rigidbody 비활성화
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.simulated = false;

        // Collider 비활성화 (공격/피격 충돌 차단)
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // 스크립트에서 Update 등 동작 정지 (필요 시)
        this.enabled = false; // 스크립트 자체를 비활성화


        // 게임 오브젝트 제거
        StartCoroutine(DestroyAfterDelay(1f));
    }

    // 일정 시간 후 몬스터 제거하는 코루틴
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // 완전히 삭제

        // 아이템 드롭
        if (itemPrefab != null)
        {
            DroppedItem droppedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<DroppedItem>();
            droppedItem.DropItem();
        }

    }


}