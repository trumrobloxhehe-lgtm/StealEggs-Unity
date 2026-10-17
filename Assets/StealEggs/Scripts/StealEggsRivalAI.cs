using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class StealEggsRivalAI : MonoBehaviour
{
    public float moveSpeed = 4.8f;
    public float pickupDistance = 1.6f;
    public float deliveryDistance = 2.8f;
    public Transform rivalBase;
    private CharacterController controller;
    private StealEggsGameManager gameManager;
    private StealEggsCharacterAnimation characterAnimation;
    private float verticalVelocity;
    private float nextTargetRefresh;
    private EggPickup targetEgg;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        characterAnimation = GetComponent<StealEggsCharacterAnimation>();
    }
    private void Start() { gameManager = StealEggsGameManager.Instance; }

    private void Update()
    {
        if (gameManager == null) gameManager = StealEggsGameManager.Instance;
        if (gameManager == null || rivalBase == null) return;
        if (gameManager.RivalCarriedEggs > 0 && FlatDistance(transform.position, rivalBase.position) <= deliveryDistance)
        {
            gameManager.DepositRivalEggs(); targetEgg = null; SetIdle(); return;
        }
        if (gameManager.RivalCarriedEggs > 0 && FlatDistance(transform.position, rivalBase.position) > deliveryDistance)
        {
            MoveToward(rivalBase.position); return;
        }
        if (targetEgg == null || !targetEgg.IsAvailable || Time.time >= nextTargetRefresh)
        {
            targetEgg = FindNearestEgg();
            nextTargetRefresh = Time.time + .5f;
        }
        if (targetEgg == null) { SetIdle(); return; }
        if (FlatDistance(transform.position, targetEgg.transform.position) <= pickupDistance)
        {
            targetEgg.PickUpByRival(); targetEgg = null; SetIdle(); return;
        }
        MoveToward(targetEgg.transform.position);
    }

    private EggPickup FindNearestEgg()
    {
        EggPickup[] eggs = Object.FindObjectsByType<EggPickup>(FindObjectsSortMode.None);
        EggPickup closest = null; float best = float.MaxValue;
        foreach (EggPickup egg in eggs)
        {
            if (egg == null || !egg.IsAvailable) continue;
            float d = FlatDistance(transform.position, egg.transform.position);
            if (d < best) { best = d; closest = egg; }
        }
        return closest;
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 direction = destination - transform.position; direction.y = 0f;
        if (direction.sqrMagnitude > .05f)
        {
            direction.Normalize();
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 1f - Mathf.Exp(-8f * Time.deltaTime));
        }
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity -= 24f * Time.deltaTime;
        Vector3 motion = direction * moveSpeed; motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
        if (characterAnimation != null) characterAnimation.SetMotion(moveSpeed / 9f, controller.isGrounded, verticalVelocity, false);
    }

    private void SetIdle()
    {
        if (characterAnimation != null) characterAnimation.SetMotion(0f, controller.isGrounded, verticalVelocity, false);
    }
    private static float FlatDistance(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }
}
