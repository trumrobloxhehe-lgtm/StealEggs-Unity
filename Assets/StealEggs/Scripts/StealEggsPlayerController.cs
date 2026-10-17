using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class StealEggsPlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float turnSpeed = 12f;
    public float eggReach = 2.2f;
    public float baseDeliveryDistance = 2.8f;

    private CharacterController controller;
    private float verticalVelocity;
    private StealEggsGameManager gameManager;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        gameManager = StealEggsGameManager.Instance;
    }

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

        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity -= 25f * Time.deltaTime;

        Vector3 motion = direction * moveSpeed;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        if (keyboard.eKey.wasPressedThisFrame)
            TryCollectNearestEgg();

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
            float distance = Vector3.Distance(transform.position, egg.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = egg;
            }
        }

        if (nearest != null) nearest.PickUpByPlayer();
    }

    private void TryDeliverAtHomeBase()
    {
        if (gameManager == null || gameManager.playerHomeBase == null || gameManager.PlayerCarriedEggs <= 0) return;
        Vector3 playerPosition = transform.position;
        Vector3 basePosition = gameManager.playerHomeBase.position;
        playerPosition.y = 0f;
        basePosition.y = 0f;

        if (Vector3.Distance(playerPosition, basePosition) <= baseDeliveryDistance)
            gameManager.DepositPlayerEggs();
    }
}
