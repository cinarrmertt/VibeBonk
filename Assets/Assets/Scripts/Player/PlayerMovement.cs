using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Bileşenler")]
    private CharacterController controller;
    private PlayerActions playerActions; // Yeni Input System referansı
    private Transform mainCameraTransform;
    private Player player;
    
    [Header("Hız Ayarları")]
    [SerializeField] private float debugMoveSpeed = 5.0f;
    // ... (Diğer hız ayarları: gravity, jumpHeight, fallMultiplier, rotationSpeed)
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float rotationSpeed = 10f; 
    
    [Header("Bunny Hop Ayarları")]
    [SerializeField] private float jumpMomentumMultiplier = 1.1f; // Her zıplamada hızı artırma çarpanı
    [SerializeField] private float maxMomentumSpeed = 15f;         // Ulaşılabilir maksimum hız

    
    [Header("Yer Kontrolü")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    // Bunny Hop için input durumunu tutan değişken
    private bool jumpInputPressed = false; 
    // Bunny Hop sırasında momentumu tutan değişken
    private Vector3 currentMomentum = Vector3.zero; 
    
    // Hareket ve Yerçekimi Durum Değişkenleri
    private Vector3 velocity;
    private bool isGrounded;
    
    // Input Değişkenleri
    private Vector2 movementInput; // Yeni Input System'den gelen Vector2 input

    // Public Property'ler (Animasyon Controller'ı için gerekli)
    public bool IsGrounded => isGrounded;
    public bool IsMoving { get; private set; } 
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("PlayerMovement, aynı objede PlayerCharacter bileşenini bulamadı! Hız referansı alınamıyor.");
        }
        
        // Kamera referansını al
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        
        // Input Actions'ı başlat
        playerActions = new PlayerActions();
        
        // --- Input Action'ları Bağla (Subscription) ---
        
        // Movement Input'u bağlama
        // Not: Input Action asset'inizde bu eylemin adı "Movement" olmalıdır.
        playerActions.PlayerMap.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerActions.PlayerMap.Movement.canceled += ctx => movementInput = Vector2.zero;
        
        // Jump Input'u bağlama
        // Not: Input Action asset'inizde bu eylemin adı "Jump" olmalıdır.
        // Zıplama input'unu artık sadece tuşa basılı tutma durumunu tutmak için kullanıyoruz
        playerActions.PlayerMap.Jump.started += ctx => jumpInputPressed = true; // <-- YENİ
        playerActions.PlayerMap.Jump.canceled += ctx => jumpInputPressed = false; // <-- YENİ
    }

    private void OnEnable()
    {
        playerActions.PlayerMap.Enable();
    }

    private void OnDisable()
    {
        playerActions.PlayerMap.Disable();
    }

    private void Update()
    {
        // Eğer karakter ölü ise hareket hesaplamalarını hiç yapma
        if (player != null && !player.IsAlive) return;
        
        CheckGround();
        ApplyGravity();
        HandleBunnyHop();
        HandleMovement();
    }
    
    // ... (CheckGround ve ApplyGravity metotları aynı kalır)

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Yere indiğimiz an momentum sıfırlanabilir veya korunabilir.
        // Momentumun korunmasını (Bunny Hop) sağlamak için burada sıfırlamıyoruz.
        
        // NOT: Eğer karakter duruyorsa, momentumu sıfırlamalıyız.
        if (isGrounded && movementInput.magnitude < 0.1f)
        {
            currentMomentum = Vector3.zero;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;
            }
            velocity.y += gravity * Time.deltaTime;
        }
    }
    
    private void HandleBunnyHop()
    {
        // Eğer yerdeysek ve zıplama tuşuna basılıyorsa, tekrar zıpla
        if (isGrounded && jumpInputPressed)
        {
            Jump(); 
        }
    }
    
    private void HandleMovement()
    {
        // 1. Ham Hareket Yönünü Hesapla (XZ Düzlemi)
        // Yeni input'un x'i ve y'si kullanılır
        Vector3 rawInputDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        Vector3 horizontalVelocity;
        IsMoving = rawInputDirection.magnitude >= 0.1f;

        // --- Rotasyon ve Yatay Hız Hesaplaması ---

        if (IsMoving)
        {
            // Kamera Yönüne Çevirme mantığı (Önceki cevaptan alınan)
            if (mainCameraTransform == null) return;
            
            // Kamera İleri ve Sağ vektörlerini al (Y eğimini sıfırla)
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            
            // --- GÜNCEL HIZ REFERANSINI AL ---
            // PlayerCharacter'dan (Character.cs'den miras alan) güncel hızı okur.
            float currentMoveSpeed = player != null ? player.MovementSpeed : debugMoveSpeed;
            
            // Dünya Uzayında Hareket Yönünü Hesapla (Kamera Yönünde)
            // movementInput.y = ileri/geri; movementInput.x = sağ/sol
            Vector3 moveDirection = camForward * movementInput.y + camRight * movementInput.x;
            moveDirection.Normalize(); 
            
            // Karakteri hareket yönüne yumuşakça döndür
            if (moveDirection.magnitude >= 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            
            // Yatay Hızı Uygula
            horizontalVelocity = moveDirection * currentMoveSpeed; // <-- REFERANS KULLANILIYOR
        }
        else
        {
            horizontalVelocity = Vector3.zero;
        }
    
        // --- Hızları Birleştir ve Uygula ---
        // ... (Hızları Birleştirme ve Controller.Move kısmı)
        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = velocity.y; 
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // --- HIZLANMA MANTIĞI (MOMENTUM) ---
            
            // 1. Mevcut Hızı Al (Stat'tan gelen temel hız veya mevcut momentum)
            float baseSpeed = player != null ? player.MovementSpeed : debugMoveSpeed;

            // Karakter hareket ediyorsa momentumu artır
            if (movementInput.magnitude >= 0.1f)
            {
                // 2. Momentum Hızını Hesapla
                float momentumSpeed = currentMomentum.magnitude > baseSpeed 
                    ? currentMomentum.magnitude // Zaten momentum varsa onu kullan
                    : baseSpeed; // Yoksa temel hızı kullan

                // 3. Çarpanı Uygula
                momentumSpeed *= jumpMomentumMultiplier;
                
                // 4. Maksimum Hızı Kısıtla
                momentumSpeed = Mathf.Min(momentumSpeed, maxMomentumSpeed);
                
                // 5. Momentum Vektörünü Güncelle
                // Momentum yönü, karakterin mevcut ileri yönü (transform.forward) olmalıdır.
                currentMomentum = transform.forward * momentumSpeed;
            }
            // --- HIZLANMA MANTIĞI SONU ---
        }
    }
}