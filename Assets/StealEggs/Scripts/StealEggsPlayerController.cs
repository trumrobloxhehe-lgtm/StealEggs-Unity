using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class StealEggsPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6.5f;
    public float sprintSpeed = 9f;
    public float turnSpeed = 14f;
    public float jumpHeight = 1.6f;
    public float gravity = 24f;
    [Header("Egg interaction")]
    public float eggReach = 2.5f;
    public float baseDeliveryDistance = 3.1f;

    private CharacterController controller;
    private StealEggsGameManager gameManager;
    private StealEggsCharacterAnimation characterAnimation;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        characterAnimation = GetComponent<StealEggsCharacterAnimation>();
    }
    private void Start() { gameManager = StealEggsGameManager.Instance; }

    private void Update()
    {
        if (gameManager == null) gameManager = StealEggsGameManager.Instance;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
        input = Vector2.ClampMagnitude(input, 1f);

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        forward.y = 0f; right.y = 0f; forward.Normalize(); right.Normalize();
        Vector3 direction = (forward * input.y + right * input.x).normalized;

        bool sprinting = (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed) && input.sqrMagnitude > 0.01f;
        float speed = sprinting ? sprintSpeed : moveSpeed;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }

        bool groundedBeforeMove = controller.isGrounded;
        if (groundedBeforeMove && verticalVelocity < 0f) verticalVelocity = -2f;
        if (groundedBeforeMove && keyboard.spaceKey.wasPressedThisFrame)
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        else
            verticalVelocity -= gravity * Time.deltaTime;

        Vector3 velocity = direction * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
        if (characterAnimation != null)
            characterAnimation.SetMotion(direction.sqrMagnitude > 0.001f ? speed / sprintSpeed : 0f, controller.isGrounded, verticalVelocity, sprinting);
        if (keyboard.eKey.wasPressedThisFrame) TryCollectNearestEgg();
        TryDeliverAtHomeBase();
    }

    private void TryCollectNearestEgg()
    {
        EggPickup[] eggs = Object.FindObjectsByType<EggPickup>(FindObjectsSortMode.None);
        EggPickup nearest = null;
        float nearestDistance = eggReach;
        foreach (EggPickup egg in eggs)
        {
            if (egg == null) continue;
            Vector3 a = transform.position, b = egg.transform.position;
            a.y = 0f; b.y = 0f;
            float distance = Vector3.Distance(a, b);
            if (distance < nearestDistance) { nearestDistance = distance; nearest = egg; }
        }
        if (nearest != null) nearest.PickUpByPlayer();
    }

    private void TryDeliverAtHomeBase()
    {
        if (gameManager == null || gameManager.playerHomeBase == null || gameManager.PlayerCarriedEggs <= 0) return;
        Vector3 p = transform.position, b = gameManager.playerHomeBase.position;
        p.y = 0f; b.y = 0f;
        if (Vector3.Distance(p, b) <= baseDeliveryDistance) gameManager.DepositPlayerEggs();
    }
}
