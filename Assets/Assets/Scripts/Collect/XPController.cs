using System.Collections.Generic;
using UnityEngine;

public class XPController : MonoBehaviour
{
    // Toplanan XP objelerinin listesi (referansları).
    // Bu listeye XPDrop script'leri eklenecektir.
    private List<XPDrop> collectedXPList = new List<XPDrop>();

    // Singleton deseni (opsiyonel ancak kullanımı kolaylaştırır)
    public static XPController Instance { get; private set; }

    private void Awake()
    {
        // Singleton kurulumu
        if (Instance == null)
        {
            Instance = this;
            // Sahne değiştirdiğinde yok olmasını istemiyorsanız: DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Toplanan bir XPDrop objesini listeye ekler.
    // Bu metot, XPDrop.cs'deki Trigger() metodundan çağrılmalıdır.
    /// <param name="xpDrop">Toplanan XPDrop objesinin script'i.</param>
    public void AddXP(XPDrop xpDrop)
    {
        if (xpDrop != null)
        {
            collectedXPList.Add(xpDrop);
            Debug.Log($"XP toplandı ve listeye eklendi. Toplam XP objesi: {collectedXPList.Count}");

            // NOT: Burada sadece objeyi listeye ekliyoruz.
            // Karakterin CurrentXP'si PlayerCharacter.cs'de güncellenmelidir.
        }
    }
    // Toplanan tüm XP objelerinin toplam değerini hesaplar.
    public float GetTotalCollectedXPValue()
    {
        float totalXP = 0f;
        foreach (var xp in collectedXPList)
        {
            // XPDrop sınıfının Value alanına erişim
            // NOT: XPDrop.Value alanı protected olduğu için,
            // XPDrop sınıfına public bir "Value" property'si eklemeniz gerekir.
            // Şimdilik, XPDrop içindeki Value'yu doğrudan kullanacağız.
            
            // Eğer XPDrop.Value protected ise bu satır hata verebilir! 
            // Geçici çözüm: XPDrop.cs'de Value alanını public yapın veya bir Property ekleyin.
            // Varsayım: XPDrop.Value alanına erişim public/protected olarak ayarlanmıştır.
            // totalXP += xp.Value; // Bu satır, XPDrop kodunuza bağlıdır.
        }
        return totalXP;
    }
    
    // Listeyi temizler (Örneğin seviye atlandıktan sonra).
    public void ClearCollectedXP()
    {
        collectedXPList.Clear();
        Debug.Log("Toplanan XP listesi temizlendi.");
    }
}