using UnityEngine;

[System.Serializable]
public struct UpgradeOption
{
    public StatType StatType; // Hangi statı yükseltecek (MovementSpeed, BaseDamage, vb.)
    public float Value;       // Ne kadar yükseltecek (Örn: 5f, 0.1f)
    public string Description; // Butonda görünecek açıklama (Örn: "+5 Hasar Gücü")
    public Color Color;       // Butonun rengi (Opsiyonel)
}