using UnityEngine;
using UnityEngine.AI; 
using System.Collections;

public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Bileşenler")]
    private Animator animator;
    private NavMeshAgent navAgent;
    private Collider enemyCollider; // Ölünce kapatıp, doğunca açmak için

    // --- TEMEL İSTATİSTİKLER ---
    [Header("Temel İstatistikler")]
    [SerializeField] protected float movementSpeed = 3f;
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float baseDamage = 10f;
    
    // --- KONTROL VE ALGILAMA ---
    [Header("Kontrol")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float spawnAnimationDuration = 2f; // Doğma animasyon süresi
    [SerializeField] private float attackAnimDelay = 0.5f;      // Vuruş anı gecikmesi
    
    private float currentHealth;
    private float attackTimer = 0f; 
    private Transform targetPlayer;
    
    // Durum Kontrolleri
    private bool isSpawning = false;
    private bool isAttacking = false;

    // --- LOOT AYARLARI ---
    [Header("Loot (Düşen Eşyalar)")]
    public GameObject xpDropPrefab;
    public GameObject goldDropPrefab;
    
    [Range(0, 100)] 
    [SerializeField] private int goldDropChance = 50; 

    [Header("Özel Özellikler")]
    public GameObject[] featureDropPrefabs; 
    [Range(0, 100)] 
    [SerializeField] private int featureDropChance = 15;

    // IDamagable Uygulaması
    protected bool _isAlive = true;
    public bool IsAlive => _isAlive; 
    public GameObject gameObject => base.gameObject; 

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>();
        
        if (navAgent != null) navAgent.stoppingDistance = attackRange * 0.8f;
    }

    private void Update()
    {
        // Ölü, doğuyor veya saldırıyorsa hareket mantığını çalıştırma
        if (!IsAlive || isSpawning || isAttacking) return;

        HandleAnimations();

        if (targetPlayer == null)
        {
            FindPlayerTarget();
        }
        else
        {
            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            
            if (distance > attackRange)
            {
                ChasePlayer();
            }
            else
            {
                GiveDamage();
            }
        }
    }
    
    private void HandleAnimations()
    {
        if (animator == null || navAgent == null) return;
        bool isMoving = navAgent.velocity.magnitude > 0.1f && !navAgent.isStopped;
        animator.SetBool("IsRunning", isMoving);
    }

    // --- OBJECT POOL: RESETLEME VE BAŞLATMA ---
    /// <summary>
    /// Düşman havuzdan çekildiğinde Spawner tarafından çağrılır.
    /// </summary>
    public void Spawn()
    {
        // 1. Değerleri Sıfırla
        currentHealth = maxHealth;
        _isAlive = true;
        attackTimer = 0f;
        isSpawning = false;
        isAttacking = false;
        
        // 2. Bileşenleri Tekrar Aktif Et
        if (enemyCollider != null) enemyCollider.enabled = true;
        
        if (navAgent != null) 
        {
            navAgent.enabled = true; 
            navAgent.speed = movementSpeed;
            navAgent.isStopped = false;
        }

        // 3. Hedefi Bul
        GameObject player = GameObject.FindWithTag("Player"); 
        if (player != null) targetPlayer = player.transform;

        // 4. Animasyonu Başlat
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;
        if (navAgent != null) navAgent.isStopped = true; 

        if (animator != null) 
        {
            animator.Rebind(); 
            animator.SetTrigger("Spawn");
        }

        yield return new WaitForSeconds(spawnAnimationDuration);

        isSpawning = false;
        if (navAgent != null && _isAlive) navAgent.isStopped = false;
    }

    // --- HAREKET VE SALDIRI ---

    public void ChasePlayer()
    {
        if (navAgent == null || targetPlayer == null || !navAgent.isOnNavMesh) return;
        
        if (navAgent.isStopped) navAgent.isStopped = false;
        
        navAgent.SetDestination(targetPlayer.position);
    }
    
    public void GiveDamage()
    {
        if (targetPlayer == null) return;

        if (navAgent != null) 
        {
            navAgent.isStopped = true;
            Vector3 lookPos = targetPlayer.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }
        
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= 1f / attackSpeed)
        {
            attackTimer = 0f;
            StartCoroutine(AttackRoutine());
        }
    }
    
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        if (animator != null) animator.SetTrigger("Attack");
        
        yield return new WaitForSeconds(attackAnimDelay);

        // Hasar verme anı
        if (targetPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            if (distance <= attackRange + 1.5f) 
            {
                if (targetPlayer.TryGetComponent(out IDamagable target)) 
                {
                    target.TakeDamage(baseDamage);
                }
            }
        }
        
        yield return new WaitForSeconds(0.2f); 
        isAttacking = false;
    }

    // --- HASAR ALMA VE ÖLÜM ---

    public void TakeDamage(float val)
    {
        if (!IsAlive) return;

        // NavAgent'ın aktif olup olmadığını kontrol et (Hasar alımını etkilemez ama iyi bir kontrol)
        if (navAgent != null && navAgent.enabled == false)
        {
            // Eğer NavAgent kapalıysa, düşman havuza dönüyor olabilir veya yanlış durumda olabilir.
            Debug.LogWarning("Düşman hasar aldı ama NavAgent kapalıydı.");
        }

        currentHealth -= val;
        currentHealth = Mathf.Max(currentHealth, 0f);
    
        // !!! DEBUG: Hasar alındığını ve kalan canı onayla !!!
        Debug.Log($"DÜŞMAN HASAR ALDI! Kalan Can: {currentHealth}"); 

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (!_isAlive) return;
        _isAlive = false;
        
        // 1. Loot Düşür
        DropLoot(); 
        
        // OYUNCUYA KILL SAYISINI BİLDİR (Düzeltilmiş Tip)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.TryGetComponent(out Player player)) // Player yerine PlayerCharacter kullanıldı
        {
            player.AddKill(); 
        }

        // 2. Bileşenleri Kapat
        if (navAgent != null) navAgent.isStopped = true;
        if (enemyCollider != null) enemyCollider.enabled = false; 

        Debug.Log($"{gameObject.name} öldü ve havuza döndü.");

        // 3. Object Pool İçin Kapat
        gameObject.SetActive(false); 
    }
    
    private void DropLoot()
    {
        // A. XP (Kesin)
        if (xpDropPrefab != null) 
            Instantiate(xpDropPrefab, GetRandomDropPosition(), Quaternion.identity);
            
        // B. Altın (Şans)
        if (Random.Range(0, 101) <= goldDropChance && goldDropPrefab != null) 
            Instantiate(goldDropPrefab, GetRandomDropPosition(), Quaternion.identity);
            
        // C. Özel Eşya (Şans ve Tekil)
        if (Random.Range(0, 101) <= featureDropChance && featureDropPrefabs != null && featureDropPrefabs.Length > 0)
        {
            int randIndex = Random.Range(0, featureDropPrefabs.Length);
            if (featureDropPrefabs[randIndex] != null) 
                Instantiate(featureDropPrefabs[randIndex], GetRandomDropPosition(), Quaternion.identity);
        }
    }

    private Vector3 GetRandomDropPosition()
    {
        float spread = 1.0f;
        return transform.position + new Vector3(Random.Range(-spread, spread), 0.5f, Random.Range(-spread, spread));
    }
    
    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= detectionRange)
            targetPlayer = player.transform;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    protected virtual void OnGUI()
    {
        if (!IsAlive)
            return;
        
        Vector3 worldPos = transform.position + Vector3.up * 2f;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        if (screenPos.z > 0)
        {
            float healthPercent = currentHealth / maxHealth;
            Rect barRect = new Rect(screenPos.x - 25, Screen.height - screenPos.y - 10, 50, 5);
            GUI.color = Color.black;
            GUI.DrawTexture(barRect, Texture2D.whiteTexture);
            barRect.width *= healthPercent;
            GUI.color = Color.Lerp(Color.red, Color.green, healthPercent);
            GUI.DrawTexture(barRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

    }
}