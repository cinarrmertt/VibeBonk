using UnityEngine;

public class MagnetDrop : CollectibleDrop
{
    [Header("Mıknatıs Ayarları")]
    [SerializeField] private float duration = 5f; // Etki süresi

    // Not: StatToUpgrade ve Value, CollectibleDrop'tan miras alınır.
    // Value: Menzilin ne kadar artacağını belirler (Örn: 5 birim)

    public override void Trigger(Player player)
    {
        Debug.Log($"Mıknatıs toplandı! Toplama menzili {duration} saniye boyunca arttı.");
        
        // Kalıcı güncelleme (UpdatePlayerStat) yerine geçici boost metodunu çağırıyoruz.
        // Value: Artış miktarı
        player.TriggerMagnet(duration, Value);
    }
}