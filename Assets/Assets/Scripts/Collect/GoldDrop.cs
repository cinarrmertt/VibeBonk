using UnityEngine;

public class GoldDrop : CollectibleDrop
{
    [Header("Altın Ayarları")]
    [SerializeField] private float minGold = 5f;
    [SerializeField] private float maxGold = 15f;

    private void Awake()
    {
        // Altın düşürme eşyası olduğu için StatToUpgrade'i sabitleriz.
        StatToUpgrade = StatType.CurrentGold;
        
        // Düşecek altın miktarını rastgele belirleriz.
        Value = (int)Random.Range(minGold, maxGold);
    }

    public override void Trigger(Player player)
    {
        // 1. Oyuncunun istatistiğini güncelle (UpdatePlayerStat, base sınıfta tanımlı)
        UpdatePlayerStat(player);
        
        // 2. Özel efektleri burada yap (ses, animasyon, vb.)
        Debug.Log($"{player.gameObject.name}, {Value} Altın topladı!");
    }
}