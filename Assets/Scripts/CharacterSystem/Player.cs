using System;
using System.Collections;
using UnityEngine;


public class Player : Character
{
    // ====================================================================
    // 1. STATİK ALANLAR VE DEĞİŞKENLER
    // ====================================================================

    [Header("Silah Kontrolü")]
    [SerializeField] private Weapon currentWeapon=null;
    
    [Header("Yumruk Ayarları")] 
    [SerializeField] private float punchRange = 2.0f;
    
    [Header("Sistem Sayaçları")]
    [SerializeField] protected int totalKills = 0; 
    public int TotalKills => totalKills; // Dışarıdan okuma için
    
    // --- TOPLAMA VE SALDIRI ZAMANI ---
    private float timeSinceLastAttack = 0f;
    private float collectionCheckTimer = 0f;
    [Header("Toplama Ayarları")]
    [SerializeField] private float collectionCheckRate = 0.5f; 

    // --- BOOST DURUMLARI ---
    // RageBoost
    private bool isBoosted = false; 
    private float originalMovementSpeed;
    
    // MagnetBoost
    private Coroutine magnetCoroutine;
    private float currentAddedRange = 0f;

    [Header("UI Ayarları")]
    private UIManager uiManager;

    // ====================================================================
    // 2. UNITY YAŞAM DÖNGÜSÜ METOTLARI
    // ====================================================================

    private void Awake() 
    {
        Initialize(); // Karakteri başlat
        
        uiManager = FindObjectOfType<UIManager>();
        
        // Başlangıç XP ihtiyacını hesapla
        xpRequiredForNextLevel = (int)CalculateRequiredXP(currentLevel); 
    }
    
    private void Update()
    {
        // 1. Saldırı Zamanlayıcısını Güncelle ve Kontrol Et
        HandleAttackCooldown();

        // 2. Toplama Çekim Mantığını Yönet
        HandleCollectionPull();
        
        HandleDebugInput();
    }
    
    // ====================================================================
    // 3. HAREKET & SALDIRI MANTIKLARI
    // ====================================================================
    
    private void HandleAttackCooldown()
    {
        timeSinceLastAttack += Time.deltaTime;
        float attackCooldown = 1f / attackSpeed;
        
        if (timeSinceLastAttack >= attackCooldown)
        {
            Attack(); 
            timeSinceLastAttack = 0f;
        }
    }

    public override void Attack()
    {
        if (!IsAlive) return;
        
        // Cooldown kontrolü (Update'te yapıldığı için bu satır gereksiz ama kodu değiştirmedik)
        // if (timeSinceLastAttack < 1f / attackSpeed) return;

        if (currentWeapon != null)
        {
            // SİLAH VARSA
            float damageMultiplier = baseDamage + attackDamage;
            currentWeapon.TriggerAttack(damageMultiplier);
        }
        else
        {
            // SİLAH YOKSA (Yumruk/Silahsız saldırı)
            float finalDamage = baseDamage; 
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, punchRange);
            int hits = 0;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out IDamagable target))
                {
                    MonoBehaviour targetComponent = target as MonoBehaviour; 
                    if (targetComponent == null || targetComponent.gameObject == gameObject) continue;

                    if (target.IsAlive)
                    {
                        target.TakeDamage(finalDamage);
                        hits++;
                    }
                }
            }
            Debug.Log($"Silahsız saldırı yapıldı. {hits} hedefe {finalDamage} hasar verildi.");
        }
    }
    
    // ====================================================================
    // 4. SEVİYE VE STAT YÖNETİMİ
    // ====================================================================
    
    private void CheckForLevelUp()
    {
        while (currentXP >= xpRequiredForNextLevel)
        {
            currentXP -= xpRequiredForNextLevel;
            currentLevel++;
            xpRequiredForNextLevel = (int)CalculateRequiredXP(currentLevel);

            Debug.Log($"SEVİYE ATLANDI! Yeni Level: {currentLevel}. Kalan XP: {currentXP}. Sonraki XP: {xpRequiredForNextLevel}");

            ApplyLevelUpBonuses();
            
            uiManager.OpenUpgradePanel(this);
        }
    }
    
    private float CalculateRequiredXP(int level)
    {
        if (level <= 1)
        {
            return baseXPForLevel;
        }
        
        float levelGrowthFactor = baseXPForLevel * (levelXPScale / 10f);
        return baseXPForLevel + (levelGrowthFactor * (level - 1));
    }
    
    public override void UpdateStatValue(StatType statType, float value)
    {
        switch (statType)
        {
           // --- HAREKET VE TEMEL STATLAR ---
        case StatType.MovementSpeed:
            movementSpeed += value;
            Debug.Log($"Hareket Hızı Arttı! Yeni Hız: {movementSpeed}");
            break;
        case StatType.BaseHealth:
            // Maksimum canı artır
            maxHealth += value;
            // Mevcut canı da artır (Level Up gibi kalıcı artışlar için)
            currentHealth += value; 
            break;
        case StatType.BaseDamage:
            baseDamage += value;
            break;
        case StatType.LifeSteal:
            lifeSteal += value;
            break;
            
        // --- GEÇİCİ/BOOST STATLARI ---
        case StatType.AttackDamage:
            // Rage boost'tan gelen geçici hasarı artır
            attackDamage += value;
            break;
        case StatType.AttackSpeed:
            // Rage boost'tan gelen geçici saldırı hızını artır
            attackSpeed += value;
            break;
            
        // --- LOOT VE TOPLAMA ---
        case StatType.CollectRange:
            // Mıknatıs (Magnet) veya yükseltme ile menzili artır
            collectRange += value;
            Debug.Log($"Toplama Menzili Arttı! Yeni Menzil: {collectRange}");
            break;
            
        // --- EKONOMİ VE GELİŞİM ---
        case StatType.CurrentGold:
            // Altın ekle/çıkar (Refresh butonu için negatif değerler gelebilir)
            currentGold += value;
            // Altının 0'ın altına düşmesini engelle
            currentGold = Mathf.Max(0, currentGold); 
            break;
        case StatType.CurrentXP :
            // XP ekle ve seviye atlama kontrolünü başlat
            currentXP += value;
            CheckForLevelUp();
            Debug.Log($"Yeni XP Miktarı: {currentXP}");
            break;
            
        // --- PROJEKTİL SAYISI (Eğer kullanılıyorsa) ---
        case StatType.ProjectileCount:
            // Not: Bu int bir değerdir, float ile toplandığı için dikkatli olunmalıdır.
            projectileCount += (int)value;
            break;

        default:
            Debug.LogWarning($"Stat türü {statType} için güncelleme mantığı eksik!");
            break;
        }
    }
    
    // ====================================================================
    // 5. BOOST VE GEÇİCİ ETKİLER
    // ====================================================================

    // --- RAGE BOOST ---
    public void BoostTrigger(float duration)
    {
        if (isBoosted) 
        {
            StopCoroutine("RageBoostCoroutine"); 
            isBoosted = false;
        }

        originalMovementSpeed = movementSpeed;
        
        StartCoroutine(RageBoostCoroutine(duration));
    }
    
    private IEnumerator RageBoostCoroutine(float duration)
    {
        UpdateStatValue(StatType.MovementSpeed, movementSpeed);
        isBoosted = true;
        
        yield return new WaitForSeconds(duration);

        EndBoost();
    }
    
    public void EndBoost()
    {
        if (!isBoosted) return;
        
        float speedDifference = movementSpeed - originalMovementSpeed;
        UpdateStatValue(StatType.MovementSpeed, -speedDifference); 
        
        isBoosted = false;
        Debug.Log("Rage: Stat güçlendirmesi sona erdi.");
    }
    
    // --- MAGNET BOOST ---
    public void TriggerMagnet(float duration, float amount)
    {
        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
            collectRange -= currentAddedRange; 
            currentAddedRange = 0f;
        }
        magnetCoroutine = StartCoroutine(MagnetRoutine(duration, amount));
    }

    private IEnumerator MagnetRoutine(float duration, float amount)
    {
        currentAddedRange = amount; 
        collectRange += currentAddedRange;
        yield return new WaitForSeconds(duration);
        collectRange -= currentAddedRange;
        currentAddedRange = 0f;
        magnetCoroutine = null;
    }
    
    // ====================================================================
    // 6. ÖLÜM, ENTEGRASYON VE DEBUG
    // ====================================================================
    
    public void ResumeGameAfterUpgrade() // UIManager tarafından çağrılır
    {
        Time.timeScale = 1f;
        currentHealth = maxHealth; 
    }
    
    public void AddKill()
    {
        totalKills++;
    }
    
    private void HandleCollectionPull()
    {
        collectionCheckTimer += Time.deltaTime;
        if (collectionCheckTimer >= collectionCheckRate)
        {
            collectionCheckTimer = 0f;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, collectRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out CollectibleDrop collectible))
                {
                    collectible.StartPull(transform);
                }
            }
        }
    }
    
    private void ApplyLevelUpBonuses()
    {
        // stat arttırma
    }

    private void HandleDebugInput()
    {
        // Klavye 1 tuşuna basıldığında hasar al
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            DebugTakeDamage();
        }
        
        // Klavye 2 tuşuna basıldığında stat güncelle
        if (Input.GetKeyDown(KeyCode.Alpha2)) 
        {
            DebugUpdateStats();
        }
    }

    private void DebugTakeDamage()
    {
        TakeDamage(25f); 
        
        float healAmount = 5f * lifeSteal;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); 
        Debug.Log($"Can Çalma Testi: {healAmount} iyileştirildi.");
    }

    private void DebugUpdateStats()
    {
        UpdateStatValue(StatType.MovementSpeed, 1.0f); 
        UpdateStatValue(StatType.BaseHealth, 10f);
        
        currentHealth = maxHealth;
        Debug.Log("İstatistikler güncellendi ve can yenilendi (Full HP).");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
    
    protected override void Die()
    {
        _isAlive = false;
        Debug.Log($"Oyuncu {gameObject.name} öldü!");
    }
    
}