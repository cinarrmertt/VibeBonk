using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ için

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Mevcut Tüm Yükseltmelerin Listesi")]
    public List<UpgradeOption> availableUpgrades;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Örnek yükseltmeleri buraya ekleyin (Inspector'da da yapabilirsiniz)
        if (availableUpgrades.Count == 0)
        {
            availableUpgrades.Add(new UpgradeOption { StatType = StatType.BaseDamage, Value = 5f, Description = "Saldırı Gücü +5", Color = Color.red });
            availableUpgrades.Add(new UpgradeOption { StatType = StatType.MovementSpeed, Value = 0.2f, Description = "Hız +0.2", Color = Color.blue });
            availableUpgrades.Add(new UpgradeOption { StatType = StatType.BaseHealth, Value = 15f, Description = "Maks HP +15", Color = Color.green });
            availableUpgrades.Add(new UpgradeOption { StatType = StatType.LifeSteal, Value = 0.05f, Description = "Can Çalma +5%", Color = Color.magenta });
        }
    }

    /// <summary>
    /// Rastgele, tekrar etmeyen üç yükseltme seçer.
    /// </summary>
    public UpgradeOption[] GetRandomUpgrades(int count)
    {
        // Yeterli seçenek yoksa mevcut olanları döndür
        if (availableUpgrades.Count < count)
        {
            return availableUpgrades.ToArray();
        }
        
        // Random.Range kullanarak listeden rastgele üç öğeyi seç
        return availableUpgrades
            .OrderBy(x => Random.value) // Rastgele sırala
            .Take(count)                // İlk üçünü al
            .ToArray();
    }
}