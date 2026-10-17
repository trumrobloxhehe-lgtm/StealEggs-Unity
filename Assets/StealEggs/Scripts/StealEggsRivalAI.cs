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
    private float verticalVelocity;
    private float nextTargetRefresh;
    private EggPickup targetEgg;

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
        if (gameManager == null || rivalBase == null) return;

        if (gameManager.RivalCarriedEggs > 0 && FlatDistance(transform.position, rivalBase.position) <= deliveryDistance)
        {
            gameManager.DepositRivalEggs();
            targetEgg = null;
            return;
        }

        if (gameManager.RivalCarriedEggs > 0 && FlatDistance(transform.position, rivalBase.position) > deliveryDistance)
        {
            MoveToward(rivalBase.position);
            return;
        }

        if (targetEgg == null || Time.time >= nextTargetRefresh)
        {
            targetEgg = FindNearestEgg();
            nextTargetRefresh = Time.time + 0.5f;
        }

        if (targetEgg == null) return;

        if (FlatDistance(transform.position, targetEgg.transform.position) <= pickupDistance)
        {
            targetEgg.PickUpByRival();
            targetEgg = null;
            return;
        }

        MoveToward(targetEgg.transform.position);
    }

    private EggPickup FindNearestEgg()
    {
        EggPickup[] eggs = Object.FindObjectsByType<EggPickup>(FindObjectsSortMode.None);
        EggPickup closest = null;
        float bestDistance = float.MaxValue;
        foreach (EggPickup egg in eggs)
        {
            if (egg == null) continue;
            float distance = FlatDistance(transform.position, egg.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = egg;
            }
        }
        return closest;
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.05f)
        {
            direction.Normalize();
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 8f * Time.deltaTime);
        }

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        else verticalVelocity -= 25f * Time.deltaTime;

        Vector3 motion = direction * moveSpeed;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
