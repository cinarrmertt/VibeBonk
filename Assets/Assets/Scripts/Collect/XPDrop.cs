using UnityEngine;

// Tecrübe Puanı (XP) düşürme eşyası, CollectibleDrop temel sınıfından miras alır.
public class XPDrop : CollectibleDrop
{
    [Header("XP Ayarları")]
    [SerializeField] private float minXP = 10f; // Düşecek minimum XP
    [SerializeField] private float maxXP = 30f; // Düşecek maksimum XP

    private void Awake()
    {
        // StatToUpgrade'i sabitler: CurrentXP
        StatToUpgrade = StatType.CurrentXP;
        
        // Düşecek XP miktarını rastgele belirler
        Value = (int)Random.Range(minXP, maxXP);
    }

    // Oyuncu bu eşyaya dokunduğunda çağrılır.
    /// <param name="player">Toplayan PlayerCharacter objesi.</param>
    public override void Trigger(Player player)
    {
        // 1. Oyuncunun istatistiğini güncelle
        UpdatePlayerStat(player);
    
        // 2. XPController'a bu objenin toplandığını bildir
        if (XPController.Instance != null)
        {
            XPController.Instance.AddXP(this);
        }
    
        Debug.Log($"{player.gameObject.name}, {Value} XP topladı ve kontrolcüye bildirildi!");
    }
}