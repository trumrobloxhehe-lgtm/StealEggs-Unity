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
    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = .18f;
    [Header("Egg interaction")]
    public float eggReach = 2.5f;
    public float baseDeliveryDistance = 3.1f;

    private CharacterController controller;
    private StealEggsGameManager gameManager;
    private StealEggsCharacterAnimation characterAnimation;
    private float verticalVelocity;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private Vector3 lastMoveDirection = Vector3.forward;

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
        if (dashCooldownRemaining > 0f) dashCooldownRemaining -= Time.deltaTime;
        if (keyboard.qKey.wasPressedThisFrame && dashCooldownRemaining <= 0f)
        {
            dashTimeRemaining = dashDuration;
            dashCooldownRemaining = gameManager != null ? gameManager.DashCooldown : 4f;
            StealEggsEffects.SpawnBurst(transform.position + Vector3.up * .8f, new Color(.2f, .8f, 1f));
        }

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
        if (direction.sqrMagnitude > .01f) lastMoveDirection = direction;

        bool sprinting = (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed) && input.sqrMagnitude > .01f;
        float multiplier = gameManager != null ? gameManager.SpeedMultiplier : 1f;
        float currentSpeed = (sprinting ? sprintSpeed : moveSpeed) * multiplier;
        if (direction.sqrMagnitude > .001f)
        {
            Quaternion desired = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }

        bool grounded = controller.isGrounded;
        if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (grounded && keyboard.spaceKey.wasPressedThisFrame) verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        else verticalVelocity -= gravity * Time.deltaTime;

        Vector3 velocity = direction * currentSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
        if (dashTimeRemaining > 0f)
        {
            controller.Move(lastMoveDirection * dashSpeed * multiplier * Time.deltaTime);
            dashTimeRemaining -= Time.deltaTime;
        }

        if (characterAnimation != null)
            characterAnimation.SetMotion(direction.sqrMagnitude > .001f ? currentSpeed / (sprintSpeed * Mathf.Max(1f, multiplier)) : 0f,
                controller.isGrounded, verticalVelocity, sprinting || dashTimeRemaining > 0f);

        if (keyboard.eKey.wasPressedThisFrame) TryCollectNearestEgg();
        TryDeliverAtHomeBase();
    }

    private void TryCollectNearestEgg()
    {
        if (gameManager != null && !gameManager.CanCollectMoreEggs()) return;
        EggPickup[] eggs = Object.FindObjectsByType<EggPickup>(FindObjectsSortMode.None);
        EggPickup nearest = null;
        float range = eggReach + (gameManager != null ? gameManager.PickupRangeBonus : 0f);
        float nearestDistance = range;
        foreach (EggPickup egg in eggs)
        {
            if (egg == null || !egg.IsAvailable) continue;
            Vector3 a = transform.position, b = egg.transform.position;
            a.y = 0f; b.y = 0f;
            float distance = Vector3.Distance(a, b);
            if (distance < nearestDistance) { nearestDistance = distance; nearest = egg; }
        }
        if (nearest != null) nearest.PickUpByPlayer();
        else if (gameManager != null) gameManager.ShowMessage("No available egg close enough. Move nearer or upgrade pickup range.");
    }

    private void TryDeliverAtHomeBase()
    {
        if (gameManager == null || gameManager.playerHomeBase == null || gameManager.PlayerCarriedEggs <= 0) return;
        Vector3 a = transform.position, b = gameManager.playerHomeBase.position;
        a.y = 0f; b.y = 0f;
        if (Vector3.Distance(a, b) <= baseDeliveryDistance) gameManager.DepositPlayerEggs();
    }
}
