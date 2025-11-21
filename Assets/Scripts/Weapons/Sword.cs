using UnityEngine;

public class Sword : Weapon
{
    [Header("Kılıç Özellikleri")]
    [SerializeField] private float swingAngle = 90f; // Kılıcın vuruş açısı
    
    // Kılıcın birincil saldırı aksiyonunu tetikler ve özelleştirir.
    /// <param name="damageMultiplier">Karakterden gelen hasar çarpanı.</param>
    public override void TriggerAttack(float damageMultiplier)
    {
        float finalDamage = itemDamage + damageMultiplier;
        
        // --- 1. Kılıcı Tutan Karakteri Belirle ---
        // Silah (Sword) script'inin ebeveyni, hasar verme eylemini başlatan karakterdir.
        GameObject ownerObject = transform.root.gameObject; // Kök objeyi (PlayerCharacter) al

        // Eğer CharacterController'lı ana objeyi değil de, sadece immediate parent'ı alıyorsak:
        // GameObject ownerObject = transform.parent != null ? transform.parent.gameObject : gameObject;
        // Ancak genellikle 'transform.root.gameObject' en güvenli yoldur.

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        int hits = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out IDamagable target))
            {
                // Hedefin null olup olmadığını kontrol et
                if (target == null) continue;

                // --- 2. HATA GİDERME KISMI: Kendini Vurma Engeli ---
                
                // IDamagable'ı bir MonoBehaviour'a çevirerek gameObject'ine eriş
                MonoBehaviour targetMono = target as MonoBehaviour; 
                
                // Eğer hedef bir MonoBehaviour değilse veya null ise atla.
                if (targetMono == null) continue;

                // HEDEF VURUCUNUN KENDİSİ Mİ KONTROLÜ
                if (targetMono.gameObject == ownerObject)
                {
                    // Kendisi, yani saldırıyı yapan kişi, hasar almasın.
                    continue; 
                }
                
                // Hedef yaşıyorsa hasar ver
                if (target.IsAlive)
                {
                    target.TakeDamage(finalDamage);
                    hits++;
                }
            }
        }
    
        Debug.Log($"Kılıç sallandı ve menzildeki {hits} hedefe toplam {finalDamage} hasar verildi.");
    }

    // Kılıç özelleştirmesi için ek metodlar buraya eklenebilir.
}