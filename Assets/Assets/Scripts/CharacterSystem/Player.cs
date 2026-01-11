using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : Character
{
    // ====================================================================
    // 1. STATİK ALANLAR VE DEĞİŞKENLER
    // ====================================================================

    [Header("Silah Kontrolü")]
    [SerializeField] public Weapon currentWeapon; // private yerine public yaptık
    
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
        HandleAttackCooldown();
        HandleCollectionPull();
        HandleDebugInput();

        // --- TAB TUŞU KONTROLÜ (Geliştirilmiş) ---
        if (Keyboard.current != null) // Klavye bağlı mı kontrolü
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                Debug.Log("Tab tuşuna basıldı!"); // Console'da bunu görüyor musun?
                if (uiManager != null) uiManager.ToggleStatsPanel(true);
            }
        
            if (Keyboard.current.tabKey.wasReleasedThisFrame)
            {
                Debug.Log("Tab tuşu bırakıldı!");
                if (uiManager != null) uiManager.ToggleStatsPanel(false);
            }
        }
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
    
        // SADECE SİLAH VARSA SALDIRI YAP
        if (currentWeapon != null)
        {
            float damageMultiplier = baseDamage + attackDamage;
            currentWeapon.TriggerAttack(damageMultiplier);
        }
        else
        {
            // Yumruk mekaniği tamamen kaldırıldı. 
            // Silah yoksa hiçbir şey yapmayacak.
            Debug.LogWarning("Karakterin elinde silah yok, saldırı yapılamaz!");
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

            // --- SESİ GÜNCELLE ---
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.UpdateVolume();
            }
        
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
            maxHealth += value;
            // Eğer seviye atlayıp can kapasiten artınca mevcut canın ARTMASIN istiyorsan
            // aşağıdaki satırı yorum satırı yap veya sil:
            // currentHealth += value; 
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
    
        // BURADAKİ CAN YENİLEME SATIRI SİLİNDİ:
        // currentHealth = maxHealth; 
    
        Debug.Log("Oyun devam ediyor, can yenilenmedi.");
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
    
    /// <summary>
    /// Verilen hasar üzerinden can çalma hesaplar ve oyuncuyu iyileştirir.
    /// </summary>
    /// <param name="damageDealt">Düşmana verilen toplam hasar</param>
    public void ApplyLifeSteal(float damageDealt)
    {
        if (!_isAlive || lifeSteal <= 0) return;

        // Örn: lifeSteal 0.1 ise hasarın %10'unu can olarak geri verir.
        float healAmount = damageDealt * lifeSteal;
    
        currentHealth += healAmount;

        // Canın maxHealth'i geçmemesini sağla
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (healAmount > 0.1f) // Çok küçük değerleri loglamamak için
        {
            Debug.Log($"Can Çalma: {healAmount:F1} HP kazanıldı. Mevcut Can: {currentHealth:F1}");
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
       
        if (!_isAlive) return; // Zaten ölüyse tekrar çalışma
    
        _isAlive = false; // Hayatta olma durumunu kapat
    
        Debug.Log($"Oyuncu {gameObject.name} öldü!");

        // 1. Animasyonu Oynat
        PlayerAnimationController animControl = GetComponent<PlayerAnimationController>();
        if (animControl != null)
        {
            animControl.PlayDeathAnimation();
        }

        // 2. Kontrolleri Kapat (Ölüyken hareket etmesin)
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false; // Fizik etkileşimini kes
        }

        // 3. UI'ı Bildir (Örn: Game Over ekranı açılabilir)
        // uiManager.ShowGameOver(); 
    }
    
}