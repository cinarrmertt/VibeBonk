using UnityEngine;

public abstract class CollectibleDrop : MonoBehaviour
{
    [Header("Mıknatıs Ayarları")] 
    [SerializeField] private float pullSpeed = 15f; // Oyuncuya çekilme hızı
    private bool isBeingPulled = false;
    private Transform pullTarget; // Oyuncunun Transform'u
    
    [Header("Toplanabilir İstatistik")] 
    [SerializeField] protected StatType StatToUpgrade;
    [SerializeField] protected float Value;

    private void Update()
    {
        // Mıknatıs aktifse, oyuncuya doğru hareket et
        if (isBeingPulled && pullTarget != null)
        {
            // Vector3.MoveTowards ile yumuşak hareket
            transform.position =
                Vector3.MoveTowards(transform.position, pullTarget.position, pullSpeed * Time.deltaTime);
        }
    }

    // PlayerCharacter tarafından çağrılır. Eşyanın oyuncuya doğru çekilmesini başlatır.
    /// <param name="target">Oyuncunun Transform'u.</param>
    public void StartPull(Transform target)
    {
        if (isBeingPulled) return;
        isBeingPulled = true;
        pullTarget = target;
    }
    
    // Bu toplanabilir eşya, PlayerCharacter'a temas ettiğinde çağrılır.
    public abstract void Trigger(Player player);
    
    // Oyuncunun istatistiğini güncelleyen yardımcı metot.
    protected void UpdatePlayerStat(Player player)
    {
        player.UpdateStatValue(StatToUpgrade, Value);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Temas algılandığında oyuncuyu kontrol et (Çekim başarılı olduğunda toplanır)
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            Trigger(player);
            Destroy(gameObject);
        }
    }
}