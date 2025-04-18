using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float detectionRange;
    
    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    
    [Header("Edge Detection")]
    public float edgeCheckDistance = 1.0f; // 모서리 감지 거리
    public LayerMask platformLayer; // 플랫폼 레이어

    [Header("Collision")]
    public CompositeCollider2D compositeCollider;

    [Header("Damage")]
    private float knockbackForce;
    private float damageCooldown;
    private float nextDamageTime;
    public int baseDamage = 10; // 기본 공격력
    public DamageMultiplier damageMultiplier; // 공격력 비율을 위한 ScriptableObject
    public int attackDamage;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool isGrounded;
    private bool isFacingRight = true;

    [Header("Health")]
    public float baseHealth = 100f; // 기본 체력
    public HealthMultiplier healthMultiplier; // 체력 비율을 위한 ScriptableObject
    public float calculatedHealth;
    public float currentHealth; // 현재 체력

    [Header("Item Drop")]
    [SerializeField] private GameObject itemPrefab; // 아이템 프리팹
    private InventoryManager inventoryManager;

    void Start()
    {
        // GameManager에서 값 가져오기
        moveSpeed = GameManager.Instance.meleeEnemyMoveSpeed;
        detectionRange = GameManager.Instance.meleeEnemyDetectionRange;
        knockbackForce = GameManager.Instance.meleeEnemyKnockbackForce;
        damageCooldown = GameManager.Instance.meleeEnemyDamageCooldown;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 체력과 공격력 초기화
        float healthMultiplierValue = healthMultiplier.GetHealthMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter);
        calculatedHealth = baseHealth * healthMultiplierValue;
        currentHealth = calculatedHealth;
        Debug.Log($"MeleeEnemy spawned with current health: {currentHealth}");

        attackDamage = Mathf.RoundToInt(baseDamage * damageMultiplier.GetDamageMultiplier(GameManager.Instance.Stage, GameManager.Instance.Chapter));

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 2.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // 연속 충돌 감지 모드로 변경

        // 기존 Collider는 물리적 충돌용으로 사용
        GetComponent<Collider2D>().isTrigger = false;

        // 새로운 Trigger Collider 추가
        BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        // 기존 Collider와 같은 크기로 설정
        triggerCollider.size = GetComponent<BoxCollider2D>().size;
        triggerCollider.offset = GetComponent<BoxCollider2D>().offset;

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        // 씬에 있는 모든 Enemy들과의 충돌을 무시
        RangedEnemy[] rangedEnemies = FindObjectsOfType<RangedEnemy>();
        MeleeEnemy[] meleeEnemies = FindObjectsOfType<MeleeEnemy>();

        foreach (var enemy in meleeEnemies)
        {
            if (enemy != this)  // 자기 자신은 제외
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
            }
        }

        foreach (var enemy in rangedEnemies)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>(), true);
        }

        // 플랫폼 레이어 설정
        platformLayer = LayerMask.GetMask("Ground", "Half Tile");
    }

    private void Awake()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found in the scene.");
            return; // InventoryManager가 없으면 메서드 종료
        }

        inventoryManager = InventoryManager.Instance; // inventoryManager 초기화
    }

    void Update()
    {
        // 플레이어가 죽었거나 없으면 더 이상 진행하지 않음
        if (PlayerController.IsDead || playerTransform == null)
        {
            // 정지
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // 플레이어와의 거리 체크
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // 플레이어 방향으로 이동
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            
            // 이동하기 전에 모서리를 확인
            bool canMove = CheckGroundAhead(direction.x);
            
            // 모서리가 감지되지 않아 이동 가능한 경우에만 이동
            if (canMove)
            {
                // x축 방향으로만 이동
                rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
                
                // 스프라이트 방향 전환
                if (direction.x > 0 && !isFacingRight)
                {
                    Flip();
                }
                else if (direction.x < 0 && isFacingRight)
                {
                    Flip();
                }
            }
            else
            {
                // 모서리가 감지되면 정지
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        else
        {
            // 플레이어가 감지 범위를 벗어나면 정지
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        // 체력 체크
        if (calculatedHealth <= 0)
        {
            DropItem();
            Destroy(gameObject);
        }
        //테스트용
        // 디버그용: K 키를 누르면 몬스터 체력을 0으로 설정
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Debug: Monster health set to 0 manually.");
            calculatedHealth = 0;
            CheckDeath();
        }
    }
    //테스트용 
    // 체력 0이 되면 아이템 드롭 및 몬스터 파괴 처리
    private void CheckDeath()
    {
        if (calculatedHealth <= 0)
        {
            DropItem();
            Destroy(gameObject);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    void DropItem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager is not initialized.");
            return; // inventoryManager가 null이면 메서드 종료
        }

        if (itemPrefab == null)
        {
            Debug.LogError("Item prefab is not assigned.");
            return; // itemPrefab이 null이면 메서드 종료
        }

        string itemName = inventoryManager.GetItemNameById(0);
        
        // 랜덤 ID 생성 (0~4 중 하나)
        int randomId = Random.Range(0, 5);

        // 드랍 위치
        Vector3 dropPosition = transform.position;

        // 아이템 생성
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        // 아이템 초기화
        DroppedItem itemComponent = droppedItem.GetComponent<DroppedItem>();
        if (itemComponent != null)
        {
            itemName = inventoryManager.GetItemNameById(randomId); // ID에 따른 이름
            itemComponent.Initialize(randomId, itemName);
        }
        else
        {
            Debug.LogError("DroppedItem component not found on the instantiated item.");
        }

        Debug.Log($"Dropped {itemName} at {dropPosition}");
    }

    // 디버그용 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // 새로 스폰되는 Enemy들과도 충돌을 무시하기 위한 트리거 체크
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other, true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.5f;
                    
                    damageable.TakeDamage(attackDamage, knockbackDir, knockbackForce);
                    nextDamageTime = Time.time + damageCooldown;
                }
            }
        }
    }

    // 체력을 변경하는 메서드 예시
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 데미지를 받아 현재 체력 감소
        Debug.Log($"MeleeEnemy took damage: {damage}. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(); // 체력이 0 이하가 되면 사망 처리
        }
    }

    private void Die()
    {
        Debug.Log("MeleeEnemy died.");
        PortalManager.Instance.killEnemy(1);
        // 사망 처리 로직 (예: 게임 오브젝트 비활성화)
        gameObject.SetActive(false);
    }

    // 전방에 지면이 있는지 확인하는 메서드
    private bool CheckGroundAhead(float directionX)
    {
        // 캐릭터의 발 위치 계산
        Vector2 footPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);
        
        // 이동 방향으로의 레이캐스트 방향 설정
        Vector2 rayDirection = new Vector2(directionX, -0.5f).normalized;
        
        // 레이캐스트를 통해 전방의 지면 확인
        RaycastHit2D hit = Physics2D.Raycast(footPosition, rayDirection, edgeCheckDistance, platformLayer);
        
        // 디버그용 시각화
        Debug.DrawRay(footPosition, rayDirection * edgeCheckDistance, hit ? Color.green : Color.red);
        
        return hit.collider != null;
    }
}
