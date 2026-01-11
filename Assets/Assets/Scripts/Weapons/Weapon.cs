using UnityEngine;

// Weapon sınıfı, tüm silahların temel özelliklerini ve davranışlarını tanımlar.
public class Weapon : MonoBehaviour
{
    [Header("Silah İstatistikleri")]
    [SerializeField] protected float itemDamage = 10f; // Silahın temel hasar değeri
    [SerializeField] protected float attackRange = 5f;  // Silahın saldırı menzili (metre)
    
    public float ItemDamage => itemDamage;
    public float AttackRange => attackRange;

// Silahın birincil saldırı aksiyonunu tetikler.
    // Bu metot public ve virtual'dır, böylece alt sınıflar (Kılıç, Tüfek vb.) özelleştirebilir.
    /// <param name="damageMultiplier">Karakterden gelen BaseDamage + AttackDamage değeri.</param>
    public virtual void TriggerAttack(float damageMultiplier)
    {
        float finalDamage = itemDamage + damageMultiplier;
        
        // Bu kod, kılıcın bulunduğu yerden (genellikle karakterin eli) bir menzil kontrolü yapar.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        
        // ... (Hasar verme mantığı buraya gelir)
        Debug.Log($"{gameObject.name} saldırdı. Toplam Hasar: {finalDamage}");
    }

    private void Update()
    {
        HandleDebugInput();
    }
    
    private void HandleDebugInput()
    {
        const float step = 1f; // Her tuşa basıldığında değişecek miktar

        // 1. ItemDamage Kontrolü (Örn: I ve O Tuşları)
        if (Input.GetKeyDown(KeyCode.I))
        {
            // I tuşu: Hasarı artır
            UpdateItemDamage(itemDamage + step);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            // O tuşu: Hasarı azalt (minimum 0'a kadar)
            UpdateItemDamage(Mathf.Max(0f, itemDamage - step));
        }
        
        // 2. AttackRange Kontrolü (Örn: K ve L Tuşları)
        if (Input.GetKeyDown(KeyCode.K))
        {
            // K tuşu: Menzili artır
            UpdateAttackRange(attackRange + step);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            // L tuşu: Menzili azalt (minimum 0.5f'e kadar)
            UpdateAttackRange(Mathf.Max(0.5f, attackRange - step));
        }
    }
    
    // Silahın temel hasar değerini günceller.
    /// <param name="newValue">Yeni temel hasar değeri.</param>
    public void UpdateItemDamage(float newValue)
    {
        itemDamage = newValue;
        Debug.Log($"{gameObject.name} Item Damage güncellendi: {itemDamage}");
    }
    
    // Silahın saldırı menzilini günceller.
    /// <param name="newValue">Yeni saldırı menzili değeri.</param>
    public void UpdateAttackRange(float newValue)
    {
        attackRange = newValue;
        Debug.Log($"{gameObject.name} Attack Range güncellendi: {attackRange}");
    }

    // Geliştirme kolaylığı için menzili gösteren görsel bir debug çizimi.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}