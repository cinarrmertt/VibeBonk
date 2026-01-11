using UnityEngine;
using System.Collections;

// Öfke, toplandığında karaktere geçici stat güçlendirmesi verir.
public class RageDrop : CollectibleDrop
{
    [Header("Öfke Ayarları")]
    [SerializeField] private float duration = 5f; // Güçlendirmenin süresi (saniye)
    
    // Rage bir stat güncellemek yerine doğrudan aksiyon tetikler.

    public override void Trigger(Player player)
    {
        Debug.Log($"Öfke toplandı! Saldırı gücü ve hızı {duration} saniye boyunca 2 katına çıktı.");
        
        // PlayerCharacter'da tanımlı olan güçlendirme metodunu çağır.
        player.BoostTrigger(duration);
        UpdatePlayerStat(player);
        
        // Not: Coroutine başlatıldığı için, bu script hemen Destroy olabilir.
        // Güçlendirme mantığı tamamen PlayerCharacter içinde yönetilmelidir.
    }
    
    
}