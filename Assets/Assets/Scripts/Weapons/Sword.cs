using UnityEngine;

public class Sword : Weapon
{
    [Header("Kılıç Özellikleri")]
    [SerializeField] private float swingAngle = 90f; // Kılıcın vuruş açısı
    
    // Kılıcın birincil saldırı aksiyonunu tetikler ve özelleştirir.
    /// <param name="damageMultiplier">Karakterden gelen hasar çarpanı.</param>
    // Sword.cs içindeki TriggerAttack metodunun güncel hali:

    public override void TriggerAttack(float damageMultiplier)
    {
        float finalDamage = itemDamage + damageMultiplier;
        GameObject ownerObject = transform.root.gameObject;
    
        // Can çalmayı tetiklemek için Player referansını alalım
        Player player = ownerObject.GetComponent<Player>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        int hits = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out IDamagable target))
            {
                MonoBehaviour targetMono = target as MonoBehaviour; 
                if (targetMono == null || targetMono.gameObject == ownerObject) continue;

                if (target.IsAlive)
                {
                    target.TakeDamage(finalDamage);
                    hits++;

                    // --- CAN ÇALMA BURADA DEVREYE GİRİYOR ---
                    if (player != null)
                    {
                        player.ApplyLifeSteal(finalDamage);
                    }
                }
            }
        }
        Debug.Log($"Kılıç sallandı. {hits} vuruş yapıldı.");
    }
}