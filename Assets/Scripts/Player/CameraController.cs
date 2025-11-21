using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // Takip edilecek karakter
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f); // Karakterin hangi noktas?n? takip edece?iz

    [Header("Camera Distance")]
    [SerializeField] private float distance = 5f; // Kameradan karaktere uzakl?k
    [SerializeField] private float minDistance = 2f; // Minimum zoom uzakl???
    [SerializeField] private float maxDistance = 10f; // Maximum zoom uzakl???
    [SerializeField] private float zoomSpeed = 2f; // Zoom h?z? (mouse scroll)

    [Header("Camera Rotation")]
    [SerializeField] private float mouseSensitivity = 2f; // Fare hassasiyeti
    [SerializeField] private float minVerticalAngle = -30f; // A?a?? bakma limiti
    [SerializeField] private float maxVerticalAngle = 70f; // Yukar? bakma limiti

    [Header("Camera Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.1f; // Rotasyon yumu?akl???
    [SerializeField] private float positionSmoothTime = 0.1f; // Pozisyon yumu?akl???

    [Header("Collision Detection")]
    [SerializeField] private bool checkCollision = true; // Duvardan ge�me kontrol�
    [SerializeField] private LayerMask collisionLayers; // Hangi layerlarla �arp??ma kontrol� yap?lacak
    [SerializeField] private float collisionOffset = 0.3f; // Duvar yak?nl??? offset

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerActions;

    private InputAction lookAction;
    private InputAction zoomAction;

    private float currentYaw = 0f; // Yatay a�?
    private float currentPitch = 20f; // Dikey a�?

    private float targetYaw = 0f;
    private float targetPitch = 20f;

    private float currentDistance;
    private Vector3 currentVelocity = Vector3.zero;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentDistance = distance;

        // Input Actions'? ayarla
        if (playerActions != null)
        {
            var actionMap = playerActions.FindActionMap("PlayerMap");
            if (actionMap != null)
            {
                lookAction = actionMap.FindAction("Look");
                zoomAction = actionMap.FindAction("Zoom");
            }
        }

        // Ba?lang?� rotasyonunu ayarla
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            currentYaw = angles.y;
            currentPitch = angles.x;
            targetYaw = currentYaw;
            targetPitch = currentPitch;
        }

        // Cursor'u kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (lookAction != null)
        {
            lookAction.Enable();
        }

        if (zoomAction != null)
        {
            zoomAction.Enable();
            zoomAction.performed += OnZoom;
        }
    }

    private void OnDisable()
    {
        if (lookAction != null)
        {
            lookAction.Disable();
        }

        if (zoomAction != null)
        {
            zoomAction.performed -= OnZoom;
            zoomAction.Disable();
        }
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<float>();
        distance = Mathf.Clamp(distance - scrollValue * zoomSpeed * 0.1f, minDistance, maxDistance);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Fare inputunu al
        Vector2 lookInput = Vector2.zero;
        if (lookAction != null)
        {
            lookInput = lookAction.ReadValue<Vector2>();
        }

        // Kamera a�?lar?n? g�ncelle
        targetYaw += lookInput.x * mouseSensitivity;
        targetPitch -= lookInput.y * mouseSensitivity;
        targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);

        // Smooth rotasyon
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationSmoothTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

        // Kamera pozisyonunu hesapla
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 targetPosition = target.position + offset;

        // Kameran?n hedef pozisyonu (karakterin arkas?nda)
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // �arp??ma kontrol�
        float finalDistance = distance;
        if (checkCollision)
        {
            Vector3 direction = desiredPosition - targetPosition;
            if (Physics.Raycast(targetPosition, direction.normalized, out RaycastHit hit, distance, collisionLayers))
            {
                finalDistance = Mathf.Clamp(hit.distance - collisionOffset, minDistance, distance);
            }
        }

        // Final pozisyon
        Vector3 finalPosition = targetPosition - (rotation * Vector3.forward * finalDistance);

        // Smooth pozisyon ge�i?i
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, positionSmoothTime);

        // Kameray? hedefe bakt?r
        transform.LookAt(targetPosition);
    }

    // ESC tu?uyla cursor'u g�ster/gizle (debug i�in)
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Gizmos ile debug g�rselle?tirmesi
    private void OnDrawGizmosSelected()
    {
        if (target == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position + offset, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(target.position + offset, transform.position);
    }
}