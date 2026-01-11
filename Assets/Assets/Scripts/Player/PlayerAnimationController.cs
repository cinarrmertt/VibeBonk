using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Bileşenler")]
    private Animator animator;
    private PlayerMovement playerMovement; 

    // Animator Parametre İsimleri
    private readonly int AnimID_IsRunning = Animator.StringToHash("IsRunning");
    private readonly int AnimID_JumpTrigger = Animator.StringToHash("Jump"); // Trigger için yeni isim
    private readonly int AnimID_Die = Animator.StringToHash("Die");
    
    // PlayerMovement script'inden alınacak durumlar
    private bool isGrounded;
    private bool isMoving;
    
    // Durum takibi için eski zemin durumunu tutan değişken
    private bool wasGrounded; 

    private void Awake()
    {
        // Gerekli bileşenleri al
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (animator == null)
        {
            Debug.LogError("Animator bileşeni bulunamadı!");
        }
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script'i bulunamadı!");
        }
    }

    private void Update()
    {
        // PlayerMovement script'inden gerekli durumları çek
        isGrounded = playerMovement.IsGrounded;
        isMoving = playerMovement.IsMoving; 

        // 1. Koşma/Idle Animasyonunu Yönet
        HandleMovementAnimation();

        // 2. Zıplama Animasyonunu Yönet
        HandleJumpTrigger();
        
        
        // Mevcut durumu bir sonraki karede kullanmak üzere kaydet
        wasGrounded = isGrounded;
    }

    private void HandleMovementAnimation()
    {
        // Eğer yerdeyse ve hareket ediyorsa (input.magnitude >= 0.1f)
        bool shouldRun = isGrounded && isMoving;
        
        // Animator'daki IsRunning boolean parametresini ayarla
        animator.SetBool(AnimID_IsRunning, shouldRun);
    }

    private void HandleJumpTrigger()
    {
        // Yere basılı durumdan (wasGrounded = true) havaya kalkışa (isGrounded = false) geçildiğinde
        // Zıplama eyleminin gerçekleştiği anı yakalar.
        if (wasGrounded && !isGrounded)
        {
            // Jump Trigger'ını tetikle
            animator.SetTrigger(AnimID_JumpTrigger);
        }
        
        // Not: Zıplamadan sonraki düşme (Falling) ve yere iniş (Landing) animasyonları
        // genellikle ayrı bir durum makinesinde veya Bool parametrelerle yönetilir.
        // Bu kod, sadece zıplama kalkışını (take-off) tetikler.
    }
    
    public void PlayDeathAnimation()
    {
        // Ölme animasyonunu tetikle
        animator.SetTrigger(AnimID_Die);
    }
}