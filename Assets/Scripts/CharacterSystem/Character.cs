using UnityEngine;

public abstract class Character : MonoBehaviour, IDamagable
{
    [Header("Temel İstatistikler")]
    [SerializeField]
    public float currentHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] protected float movementSpeed;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float attackDamage; // Ek hasar bonusu
    [SerializeField] protected float collectRange;
    [SerializeField] protected float lifeSteal;
    [SerializeField] protected int projectileCount;
    protected bool _isAlive = true; // Dahili değişkenin adını karışmaması için değiştirdik
    
    [Header("Ekonomik ve Gelişim")]
    [SerializeField] protected float currentXP = 0f;       // Mevcut Tecrübe Puanı
    [SerializeField] protected float currentGold = 0f;     // Mevcut Altın Miktarı
    
    [Header("Seviye Sistemi")]
    [SerializeField] protected int currentLevel = 1;               // Mevcut seviye
    [SerializeField] protected int xpRequiredForNextLevel = 100; // Sonraki seviye için gereken XP
    [SerializeField] protected float baseXPForLevel = 100f;        // Level 1 için temel XP ihtiyacı
    [SerializeField] protected float levelXPScale = 1.5f;          // XP artışının üstel çarpanı

    [Header("Properties")]
    public float MovementSpeed => movementSpeed;
    public float CurrentGold => currentGold;
    public float CurrentXP => currentXP;
    public int CurrentLevel => currentLevel;
    public float XPRequiredForNextLevel => xpRequiredForNextLevel;

    // --- DİĞER KRİTİK STATLAR ---

    // Can Değerleri
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    // Saldırı Değerleri
    public float BaseDamage => baseDamage;
    public float AttackDamage => attackDamage; // Geçici boost'lar için ek hasar
    public float AttackSpeed => attackSpeed;
    public float LifeSteal => lifeSteal;
    public int ProjectileCount => projectileCount; // Eğer bu statı tutuyorsanız

    // Toplama Menzili (Mıknatıs)
    public float CollectRange => collectRange;

    // Durum Kontrolü
    public bool IsAlive => _isAlive;

    // Unity Referans? (IDamagable arayüzü gerektirir)
    public GameObject gameObject => base.gameObject;
    

    // Karakterin tüm başlangıç istatistiklerini sıfırdan kurar.
    public virtual void Initialize()
    {
        // Örnek başlangıç değerleri atayabilirsiniz
        maxHealth = 100f;
        currentHealth = maxHealth; 
        movementSpeed = 5f;
        attackSpeed = 1f;
        baseDamage = 10f;
        attackDamage = 0f;
        collectRange = 2f;
        lifeSteal = 0f;
        projectileCount = 1;
        _isAlive = true;
        
        currentXP = 0f; 
        currentGold = 0f;

        Debug.Log($"{gameObject.name} başlatıldı. HP: {maxHealth}");
    }

    // Belirtilen istatistik türünün değerini günceller (Buff/Debuff veya Seviye Atlama).
    // Alt sınıfların kendi özel hesaplamalarını yapabilmesi için Abstract bırakılmıştır.
    /// <param name="statType">Güncellenecek istatistik türü.</param>
    /// <param name="value">Stat'a eklenecek/çıkarılacak değer.</param>
    public abstract void UpdateStatValue(StatType statType, float value);


    // Karakterin ana saldırısını tetikler.
    public virtual void Attack()
    {
        if (!_isAlive) return;

        // Saldırı için silaha gönderilecek nihai hasar değeri hesaplanır.
        float finalDamage = CalculateDamage();

        // Burası silahı veya hasar sistemini tetikleyecek kısımdır.
        // Örneğin: weapon.Fire(finalDamage, projectileCount);

        // Şimdilik sadece logluyoruz.
        Debug.Log($"{gameObject.name} saldırdı. Gönderilen Hasar: {finalDamage} (Proje Sayısı: {projectileCount})");
    }

    // Karakterin uygulayacağı nihai hasarı hesaplar.
    
    // <returns>BaseDamage ve AttackDamage toplamı.</returns>
    protected float CalculateDamage()
    {
        return baseDamage + attackDamage;
    }

    // Karakterin hasar almasını sağlar ve canını azaltır.
    
    // <param name="damage">Alınan hasar miktarı.</param>
    public virtual void TakeDamage(float damage) // Artık interface'den geliyor
    {
        if (!IsAlive) return; // Interface'den gelen özelliği kullanıyoruz

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log($"{gameObject.name} {damage} hasar aldı. Kalan HP: {currentHealth}");

        CheckForDeath();
    }

    // HP 0 veya altına düştüğünde çağrılır.
    
    protected virtual void CheckForDeath()
    {
        if (currentHealth <= 0 && IsAlive)
        {
            Die();
        }
    }

    // Karakter öldüğünde çalışan metot.
    protected abstract void Die();
}
