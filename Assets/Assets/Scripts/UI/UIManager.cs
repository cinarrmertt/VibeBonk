using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    
    // Can Yönetimi
    [SerializeField] private Image healthBarImage; 
    [SerializeField] private TextMeshProUGUI healthText; 

    // XP Yönetimi
    [SerializeField] private Image xpBarImage;
    
    // Sayaclar
    [SerializeField] private TextMeshProUGUI coinText; 
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    // Level Metni
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Referanslar")]
    private Player playerCharacter;
    private float startTime;
    private bool isReadyToRead = false; 

    [Header("Yükseltme UI")] 
    [SerializeField] private GameObject upgradePanel; 
    [SerializeField] private Button[] upgradeButtons; // 3 adet yükseltme butonu
    [SerializeField] private TextMeshProUGUI[] optionTexts; // 3 adet butonun üzerindeki Text
    [SerializeField] private Button refreshButton; // Refresh butonu
    [SerializeField] private TextMeshProUGUI refreshCostText; // Refresh maliyeti
    
    [Header("Stat Ekranı")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI statsText;
    
    private const int REFRESH_COST = 50; // Yenileme maliyeti
    private UpgradeOption[] currentUpgradeOptions; // Şu anki 3 seçenek
    private Player currentPlayer; // Yükseltme yapacak oyuncu referansı
    private void Start()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCharacter = player.GetComponent<Player>();
        }
        
        startTime = Time.time;
        
        StartCoroutine(InitializeUIReadout());
    }
    
    private IEnumerator InitializeUIReadout()
    {
        while (playerCharacter == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerCharacter = player.GetComponent<Player>();
            }
            yield return null;
        }

        while (playerCharacter.MaxHealth <= 0 || playerCharacter.CurrentHealth <= 0)
        {
            yield return null; 
        }
        
        isReadyToRead = true;
        Debug.Log("UIManager OK: Can değerleri okumaya hazır.");
    }

    private void Update()
    {
        if (isReadyToRead && playerCharacter != null)
        {
            // --- GÜNCELLEMELER ---
            UpdateHealthUI(playerCharacter.CurrentHealth, playerCharacter.MaxHealth);
            UpdateXPUI(playerCharacter.CurrentXP, playerCharacter.XPRequiredForNextLevel);
            UpdateCounters(playerCharacter.CurrentGold, playerCharacter.TotalKills, playerCharacter.CurrentLevel);
            UpdateTimeUI();
        }
    }

    public void UpdateHealthUI(float currentHP, float maxHP)
    {
        if (maxHP <= 0) maxHP = 100;
        float healthRatio = currentHP / maxHP;

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = healthRatio;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
        }
    }
    
    public void UpdateXPUI(float currentXP, float requiredXP)
    {
        if (xpBarImage == null) return;
        if (requiredXP <= 0) requiredXP = 1; 
        
        float xpRatio = Mathf.Min(currentXP / requiredXP, 1f); 
        
        xpBarImage.fillAmount = xpRatio;
    }

    /// <summary>
    /// Altın, Kill ve Level sayaçlarını günceller. SADECE SAYISAL DEĞERLERİ ATAR.
    /// </summary>
    public void UpdateCounters(float goldAmount, int killCount, int currentLevel)
    {
        // Altın Miktarını göster
        if (coinText != null)
        {
            coinText.text = $"Coin: {Mathf.FloorToInt(goldAmount)}";
        }
        
        // Kill Sayısını göster
        if (killText != null)
        {
            killText.text = $"Kill: {killCount}";
        }
        
        // LEVEL METNİ
        if (levelText != null)
        {
            levelText.text = $"LVL {currentLevel}";
        }
    }

    /// <summary>
    /// Oyun süresini günceller (Saniye cinsinden).
    /// </summary>
    public void UpdateTimeUI()
    {
        if (timeText == null) return;
        
        float timeElapsed = Time.time - startTime;
        string minutes = Mathf.Floor(timeElapsed / 60).ToString("00");
        string seconds = Mathf.Floor(timeElapsed % 60).ToString("00");
        
        // İstenen format: 00:01
        timeText.text = $"Time: {minutes}:{seconds}";
    }
    
    public void OpenUpgradePanel(Player player)
    {
        Cursor.lockState=CursorLockMode.None;
        Cursor.visible=true;
        
        if (upgradePanel != null)
        {
            currentPlayer = player;
            
            // Oyunu durdur
            Time.timeScale = 0f;
            
            // Butonların eventlerini bağla (Sadece bir kere yapılması en iyisidir, burada kontrol ediyoruz)
            SetupButtonListeners();
            
            // Seçenekleri hazırla ve paneli aktif et
            PopulateUpgradeOptions();
            
            upgradePanel.SetActive(true);
        }
    }
      private void SetupButtonListeners()
    {
        // Butonlara tıklandığında SelectUpgrade çağrısını bağlarız
    
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i; // Lambda için index'i yakala
        
            // --- KRİTİK: ÖNCE TEMİZLE ---
            upgradeButtons[i].onClick.RemoveAllListeners(); 
            // -----------------------------
        
            upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(index));
        }

        // Refresh butonu için de aynı şeyi yap
        refreshButton.onClick.RemoveAllListeners();
        refreshButton.onClick.AddListener(RefreshOptions);
    
        // NOT: Artık ilk kontrol (GetPersistentEventCount() == 0) gerekli değildir.
    }

    private void PopulateUpgradeOptions()
    {
        // 1. Yeni rastgele 3 seçenek al
        if (UpgradeManager.Instance == null) return;
        
        currentUpgradeOptions = UpgradeManager.Instance.GetRandomUpgrades(3);

        // 2. Butonları ayarla
        for (int i = 0; i < 3; i++)
        {
            if (i < currentUpgradeOptions.Length && optionTexts[i] != null)
            {
                UpgradeOption option = currentUpgradeOptions[i];
                float currentValue = GetCurrentStatValue(option.StatType);
                float newValue = currentValue + option.Value;
                
                // Metin formatını oluştur: Açıklama (Mevcut Değer -> Yeni Değer)
                string displayValue = FormatStatValue(currentValue);
                string newDisplayValue = FormatStatValue(newValue);
                
                optionTexts[i].text = 
                    $"{option.Description}\n" + 
                    $"({displayValue} \u2192 {newDisplayValue})"; // \u2192: Unicode sağ ok işareti
                
                upgradeButtons[i].interactable = true;
            }
            else
            {
                upgradeButtons[i].interactable = false;
                optionTexts[i].text = "Seçenek Yok";
            }
        }
        
        // Refresh butonu maliyetini göster
        refreshCostText.text = $"{REFRESH_COST} ALTIN";
        refreshButton.interactable = (currentPlayer != null && currentPlayer.CurrentGold >= REFRESH_COST);
    }
    
    /// <summary>
    /// Stat tipine göre oyuncunun mevcut değerini döndürür.
    /// </summary>
    private float GetCurrentStatValue(StatType statType)
    {
        if (currentPlayer == null) return 0f;
        
        // Player/Character sınıfınızdaki public property'leri kullanarak mevcut statı okuyun.
        switch (statType)
        {
            case StatType.BaseDamage: return currentPlayer.BaseDamage;
            case StatType.MovementSpeed: return currentPlayer.MovementSpeed;
            case StatType.BaseHealth: return currentPlayer.MaxHealth; // MaxHealth'i göster
            case StatType.AttackSpeed: return currentPlayer.AttackSpeed;
            case StatType.LifeSteal: return currentPlayer.LifeSteal * 100f; // Yüzde göstermek için
            case StatType.CollectRange: return currentPlayer.CollectRange;
            // Diğer statları buraya ekleyin
            default: return 0f;
        }
    }
    
    /// <summary>
    /// Stat değerini UI'da düzgün göstermek için formatlar.
    /// </summary>
    private string FormatStatValue(float value)
    {
        // LifeSteal gibi yüzde olan statlar için format
        if (value < 1.0f && value > 0f)
        {
            return $"{(value * 100f):F0}%"; // Yüzde ve ondalıksız
        }
        // Diğer float ve int statlar için format
        return $"{value:F1}"; // Bir ondalık basamak göster
    }
    
    /// <summary>
    /// Bir yükseltme seçildiğinde çağrılır.
    /// </summary>
    public void SelectUpgrade(int optionIndex)
    {
        if (currentPlayer == null || optionIndex >= currentUpgradeOptions.Length) return;
        
        UpgradeOption selectedOption = currentUpgradeOptions[optionIndex];

        // 1. Statı uygula
        currentPlayer.UpdateStatValue(selectedOption.StatType, selectedOption.Value);
        
        // 2. Paneli kapat ve oyunu devam ettir
        CloseUpgradePanel();
    }
    
    /// <summary>
    /// Yenileme butonuna basıldığında çağrılır.
    /// </summary>
    public void RefreshOptions()
    {
        if (currentPlayer == null) return;

        if (currentPlayer.CurrentGold >= REFRESH_COST)
        {
            // 1. Altını düşür
            currentPlayer.UpdateStatValue(StatType.CurrentGold, -REFRESH_COST);
            
            // 2. Yeni seçenekleri yükle
            PopulateUpgradeOptions();
        }
        else
        {
            Debug.Log("Yenilemek için yeterli altın yok!");
        }
    }

    /// <summary>
    /// Atlama (Skip) butonuna basıldığında çağrılır.
    /// </summary>
    public void SkipUpgrade()
    {
        CloseUpgradePanel(); // Sadece paneli kapatır.
    }

    private void CloseUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        // Oyuncuyu devam ettir
        if (currentPlayer != null)
        {
            currentPlayer.ResumeGameAfterUpgrade();
        }
    }
    
    public void ToggleStatsPanel(bool isActive)
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(isActive);
            if (isActive) UpdateStatsDisplay(); // Açıldığında değerleri güncelle
        }
    }
    private void UpdateStatsDisplay()
    {
        if (playerCharacter == null || statsText == null) return;

        // String interpolation ($"") kullanarak değerleri alt alta yazıyoruz
        statsText.text = "<b>KARAKTER STATLARI</b>\n\n" +
                         $"Saldırı Gücü: {playerCharacter.BaseDamage + playerCharacter.AttackDamage}\n" +
                         $"Saldırı Hızı: {playerCharacter.AttackSpeed:F1}\n" +
                         $"Hareket Hızı: {playerCharacter.MovementSpeed:F1}\n" +
                         $"Can Çalma: %{(playerCharacter.LifeSteal * 100):F0}\n" +
                         $"Toplama Menzili: {playerCharacter.CollectRange:F1}\n" +
                         $"Maksimum Can: {playerCharacter.MaxHealth}\n" +
                         $"Seviye: {playerCharacter.CurrentLevel}";
    }
}