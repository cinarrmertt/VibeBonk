using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Referanslar")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private Player player;

    [Header("Ses Ayarları")]
    [Range(0f, 1f)]
    [SerializeField] private float startVolume = 0.2f; // Oyun başındaki ses (Kısık)
    [SerializeField] private float volumeIncreasePerLevel = 0.05f; // Her levelda artış miktarı
    [SerializeField] private float maxVolume = 1.0f; // Ulaşılabilecek maksimum ses

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateVolume(); // Başlangıçta sesi ayarla
    }

    // Bu metot her level atlandığında Player scriptinden çağrılacak
    public void UpdateVolume()
    {
        if (backgroundMusic == null || player == null) return;

        // Hesaplama: Başlangıç Sesi + ((Mevcut Level - 1) * Artış Miktarı)
        // Matematiksel Formül: $volume = \text{startVolume} + ((\text{currentLevel} - 1) \times \text{volumeIncreasePerLevel})$
        float newVolume = startVolume + ((player.CurrentLevel - 1) * volumeIncreasePerLevel);

        // Sesin maksimum sınırı aşmamasını sağla
        backgroundMusic.volume = Mathf.Min(newVolume, maxVolume);
        
        Debug.Log($"Ses Güncellendi: Level {player.CurrentLevel}, Yeni Volume: {backgroundMusic.volume}");
    }
}