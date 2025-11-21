using System;
using System.Collections;
using UnityEngine;


public class Player : Character
{
    [Header("Silah Kontrolü")]
    [SerializeField] private Weapon currentWeapon;
    
    // Otomatik saldırı zamanlayıcısı
    private float timeSinceLastAttack = 0f;
    
    // --- TOPLAMA ÇEKİM MANTIĞI ---
    private float collectionCheckTimer = 0f;
    [Header("Toplama Ayarları")]
    [SerializeField] private float collectionCheckRate = 0.5f; // Saniyede kaç kez menzil kontrolü yapılacak

    //RageBoost
    private bool isBoosted = false; // Rage'in aktif olup olmadığını tutar (EKLENECEK ALAN)
    private float originalMovementSpeed;
    
    //MagnetBoost
    private Coroutine magnetCoroutine; // Aktif sayacı takip etmek için
    private float currentAddedRange = 0f; // Şu an fazladan ekli olan menzil miktarı

    [Header("UI Ayarları")]
    [SerializeField] private Color textColor = Color.yellow; // Metin rengi
    [SerializeField] private int fontSize = 18;              // Font boyutu

    // Stili tanımlamak için GUIStyle değişkeni
    private GUIStyle guiStyle = new GUIStyle();
    private void Awake() 
    {
        Initialize(); // Karakteri başlat
        // YENİ: Başlangıç XP ihtiyacını hesapla
        xpRequiredForNextLevel = (int)CalculateRequiredXP(currentLevel); 
        
        // GUI stil ayarlarını yap
        guiStyle.fontSize = fontSize;
        guiStyle.normal.textColor = textColor;
        guiStyle.fontStyle = FontStyle.Bold;
    }
    private void Update()
    {
        // 1. Saldırı Zamanlayıcısını Güncelle
        timeSinceLastAttack += Time.deltaTime;

        // 2. Otomatik Saldırıyı Kontrol Et
        // Saldırı zamanı (1 / attackSpeed) dolduğunda Attack() metodunu çağır.
        float attackCooldown = 1f / attackSpeed;
        
        if (timeSinceLastAttack >= attackCooldown)
        {
            // Attack() metodu hasar verme ve menzil kontrolünü Sword.cs'ye yaptıracak
            Attack(); 
            timeSinceLastAttack = 0f; // Zamanlayıcıyı sıfırla
        }
        
        // 2. TOPLAMA MANTIĞI KONTROLÜ
        HandleCollectionPull();
        
        HandleDebugInput();
    }
    private void CheckForLevelUp()
    {
        // Yeterli XP olduğu sürece seviye atlamaya devam et
        while (currentXP >= xpRequiredForNextLevel)
        {
            // 1. Kalan XP'yi Hesapla (Fazla XP'yi yeni level'a taşı)
            currentXP -= xpRequiredForNextLevel;

            // 2. Level Up
            currentLevel++;
            
            // 3. Yeni Level İçin Gereken XP'yi Hesapla
            xpRequiredForNextLevel = (int)CalculateRequiredXP(currentLevel);

            Debug.Log($"SEVİYE ATLANDI! Yeni Level: {currentLevel}. Kalan XP: {currentXP}. Sonraki XP: {xpRequiredForNextLevel}");

            // 4. Stat Artışları veya Ödüller (Örnek)
            ApplyLevelUpBonuses();
        }
    }
    
    // Verilen level için gereken toplam XP miktarını hesaplar.
    // Formül: BaseXP * (Level ^ Scale)
    private float CalculateRequiredXP(int level)
    {
        if (level <= 1)
        {
            return baseXPForLevel;
        }
        
        // 1. Level Büyüme Katsayısı Hesaplama: BaseXP'nin LevelXPScale ile çarpımı (Örn: 100 * 1.5 = 150)
        // Her seviye artışında eklenecek sabit miktar.
        float levelGrowthFactor = baseXPForLevel * (levelXPScale / 10f); // 1.5'u çarpan olarak kullanmak için bölme yapabilirsiniz,
        // ya da direkt Inspector'da 0.15 gibi düşük bir değer kullanabilirsiniz.
        
        // 2. Nihai Hesaplama: Temel XP + (Büyüme Katsayısı * (Mevcut Seviye - 1))
        // Seviye 1'de (1-1=0) bu kısım sıfırlanır, Level 2'den itibaren artar.
        return baseXPForLevel + (levelGrowthFactor * (level - 1));
    }
    
    // RageDrop tarafından çağrılır. Boost Coroutine'ini başlatır.
    /// <param name="duration">Boost'un süresi.</param>
    public void BoostTrigger(float duration)
    {
        if (isBoosted) 
        {
            // Eğer zaten aktifse, mevcut boost'u sıfırla ve yeniden başlat
            StopCoroutine("RageBoostCoroutine"); 
            isBoosted = false; // Statları geri alma EndBoost'ta gerçekleşecek.
        }

        StartCoroutine(RageBoostCoroutine(duration));
    }
    
    // Statları 2 katına çıkarır ve süreyi yönetir.
    private IEnumerator RageBoostCoroutine(float duration)
    {
        Debug.Log("Rage: Statlar 2 katına çıkarıldı!");
        
        // 1. Boost'u Uygula: Mevcut base değerler kadar ekleme yap
        // Not: Bu, Character.cs'deki AttackDamage ve AttackSpeed değişkenlerini günceller.
        
        // AttackDamage'ı baseDamage kadar artır (2 kat hasar verir)
        originalMovementSpeed = movementSpeed;
        UpdateStatValue(StatType.MovementSpeed, movementSpeed);

        isBoosted = true;
        
        // Süre bitene kadar bekle
        yield return new WaitForSeconds(duration);

        // Süre bittiğinde geri al
        EndBoost();
    }
    
    // Boost süresi dolduğunda statları orijinal haline geri döndürür.
    public void EndBoost()
    {
        if (!isBoosted) return;
        
        // Geri Al (Uygulanan değerleri negatif olarak geri gönder)
        float speedDifference = movementSpeed - originalMovementSpeed;
        UpdateStatValue(StatType.MovementSpeed, -speedDifference); 
        
        isBoosted = false;
        Debug.Log("Rage: Stat güçlendirmesi sona erdi.");
    }
    
    // PlayerCharacter.cs sınıfının içine eklenmeli:

    // --- MAGNET FONKSİYONU ---

    /// <summary>
    /// MagnetDrop tarafından çağrılır.
    /// </summary>
    public void TriggerMagnet(float duration, float amount)
    {
        // 1. Eğer hali hazırda aktif bir mıknatıs etkisi varsa, önce onu temizle (Reset)
        if (magnetCoroutine != null)
        {
            StopCoroutine(magnetCoroutine);
            collectRange -= currentAddedRange; // Eski eklenen fazlalığı çıkar (Orjinale dön)
            currentAddedRange = 0f;
        }

        // 2. Yeni etkiyi başlat
        magnetCoroutine = StartCoroutine(MagnetRoutine(duration, amount));
    }

    private IEnumerator MagnetRoutine(float duration, float amount)
    {
        // Değeri uygula
        // Güvenlik: currentAddedRange sadece amount kadar olmalı.
        currentAddedRange = amount; 
        
        // collectRange'e sadece bir kere (currentAddedRange kadar) ekleme yap
        collectRange += currentAddedRange;
        
        Debug.Log($"Mıknatıs Aktif! Menzil {amount} arttı. (Toplam: {collectRange})");

        // Süre kadar bekle
        yield return new WaitForSeconds(duration);

        // Süre bitince eklenen miktarı geri al
        collectRange -= currentAddedRange;
        
        // Değişkenleri sıfırla
        currentAddedRange = 0f;
        magnetCoroutine = null;

        Debug.Log($"Mıknatıs bitti. Menzil orjinale döndü. (Toplam: {collectRange})");
    }
    
    // Karakterin ana saldırısını (Kılıç) tetikler.
    // Bu metot artık otomatik olarak çağrılıyor.
    public override void Attack()
    {
        // Temel can ve varlık kontrolü
        if (!IsAlive) return;
        
        if (currentWeapon != null)
        {
            // Silahın Hasar Çarpanını (BaseDamage + AttackDamage) hesapla
            float damageMultiplier = baseDamage + attackDamage;
            
            // Kılıcı tetikle: Sword.cs (TriggerAttack) menzil kontrolünü yapacak ve hasar verecek.
            currentWeapon.TriggerAttack(damageMultiplier);
            
            // Debug.Log($"Oyuncu otomatik olarak saldırdı. Silah hasar çarpanı: {damageMultiplier}");
        }
        else
        {
            // Silah yoksa temel bir yumruk hasarı verilebilir.
            // Debug.LogWarning("Karakterde silah (currentWeapon) bağlı değil!");
        }
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
    
    private void ApplyLevelUpBonuses()
    {
        // stat arttırma
    }
    
    private void HandleCollectionPull()
    {
        collectionCheckTimer += Time.deltaTime;

        // Belirlenen aralıkta menzil kontrolü yap
        if (collectionCheckTimer >= collectionCheckRate)
        {
            collectionCheckTimer = 0f;

            // Physics.OverlapSphere ile collectRange menzilindeki tüm Collider'ları bul
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, collectRange);

            foreach (var hitCollider in hitColliders)
            {
                // Çarpışan objede CollectibleDrop script'i var mı kontrol et
                if (hitCollider.TryGetComponent(out CollectibleDrop collectible))
                {
                    // Eğer varsa, çekim hareketini başlat
                    collectible.StartPull(transform);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Unity Editor'de toplama menzilini göster
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, collectRange);
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

    // --- ABSTRACT METOT UYGULAMALARI (Aynı Kalır) ---

    protected override void Die()
    {
        _isAlive = false;
        Debug.Log($"Oyuncu {gameObject.name} öldü!");
    }

    public override void UpdateStatValue(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.CollectRange:
                collectRange += value;
                Debug.Log($"Toplama Menzili Arttı! Yeni Menzil: {collectRange}");
                break;
            case StatType.CurrentGold :
                currentGold += value;
                Debug.Log($"Yeni Altın Miktarı: {currentGold}");
                break;
            case StatType.CurrentXP :
                currentXP += value;
                CheckForLevelUp();
                Debug.Log($"Yeni XP Miktarı: {currentXP}");
                break;
            default:
                Debug.LogWarning($"Stat türü {statType} için güncelleme mantığı eksik!");
                break;
        }
    }
}