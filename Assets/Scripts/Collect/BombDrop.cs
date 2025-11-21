using UnityEngine;
using System.Collections; // Coroutine için gerekli

// Artık CollectibleDrop'tan miras almıyor, çünkü toplanmayacak.
public class Bomb : MonoBehaviour
{
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
        // Patlama pozisyonu (Bombanın kendi pozisyonu)
        Vector3 explosionPosition = transform.position;

        // Physics.OverlapSphere ile yarıçaptaki hedefleri bul
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        int targetsHit = 0;

        foreach (var hitCollider in hitColliders)
        {
            // IDamagable interface'i olan her şeye hasar ver
            if (hitCollider.TryGetComponent(out IDamagable target))
            {
                // Hasar vermeden önce canlının oyuncu olup olmadığını kontrol et
                // Düşmanın attığı bomba oyuncuya da zarar verebilir, ancak bu kontrolü PlayerCharacter'ı korumak için ekleyebiliriz.
                
                // Güvenli dönüşüm (Önceki hatayı önler)
                MonoBehaviour targetComponent = target as MonoBehaviour;
                if (targetComponent == null) continue;
                
                // Eğer hedef Player tag'ına sahipse hasar verme (Oyuncuyu korumak için)
                if (targetComponent.CompareTag("Player")) continue; 

                // Hedefe hasar ver
                target.TakeDamage(explosionBaseDamage);
                targetsHit++;
            }
        }

        Debug.Log($"Bomba patladı! {explosionRadius}m yarıçapta, {targetsHit} hedefe hasar verildi.");
        
        // Buraya Particle Effect (patlama görseli) ve Sound Effect (patlama sesi) eklenebilir.
    }
    
    private void OnDrawGizmosSelected()
    {
        // Editor'de patlama menzilini göstermek için
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}