using UnityEngine;
using System.Collections; // Coroutine için gerekli

// Artık CollectibleDrop'tan miras almıyor, çünkü toplanmayacak.
public class Bomb : MonoBehaviour
{
    [Header("Görsel Efektler")]
    [SerializeField] private GameObject explosionVFXPrefab;
    
    [Header("Patlama Ayarları")]
    [SerializeField] private float explosionDelay = 0.5f;     // Patlamadan önceki kısa gecikme (görsel/ses için)
    [SerializeField] private float explosionRadius = 5f;      // Patlama yarıçapı
    [SerializeField] private float explosionBaseDamage = 50f; // Patlamanın temel hasarı

    private void Start()
    {
        // Oyun başladığında (düşman tarafından instantiate edildiğinde) patlama coroutine'ini başlat.
        StartCoroutine(ExplodeAfterDelay());
    }

    /// <summary>
    /// Patlama gecikmesini bekler ve hasar verme işlemini gerçekleştirir.
    /// </summary>
    private IEnumerator ExplodeAfterDelay()
    {
        // 1. Patlama gecikmesini bekle
        yield return new WaitForSeconds(explosionDelay);

        // 2. HASAR VERME İŞLEMİ
        Explode();

        // 3. Patlama efektlerinden sonra objeyi yok et (Varsayım: Patlama efekti/sesi buradan tetiklenir)
        Destroy(gameObject);
    }

    private void Explode()
    {
        Vector3 explosionPosition = transform.position;

        // --- VFX OLUŞTURMA ---
        if (explosionVFXPrefab != null && ObjectPoolManager.Instance != null)
        {
            // Efekti patlama noktasında havuzdan çekerek oluştur
            ObjectPoolManager.Instance.GetPooledObject(explosionVFXPrefab, explosionPosition, Quaternion.identity);
        }

        // --- HASAR VERME MANTIĞI (Mevcut kodun) ---
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        int targetsHit = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out IDamagable target))
            {
                MonoBehaviour targetComponent = target as MonoBehaviour;
                if (targetComponent == null || targetComponent.CompareTag("Player")) continue;

                target.TakeDamage(explosionBaseDamage);
                targetsHit++;
            }
        }
        Debug.Log($"Bomba patladı! {targetsHit} hedefe hasar verildi.");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Editor'de patlama menzilini göstermek için
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}