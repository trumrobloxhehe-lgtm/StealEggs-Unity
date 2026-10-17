using UnityEngine;

/// <summary>Original guardian AI: stays at its nest until an egg is taken, then chases briefly.</summary>
[RequireComponent(typeof(CharacterController))]
public class StealEggsGuardianAI : MonoBehaviour
{
    public string guardianName = "Nest Guardian";
    public Transform playerTarget;
    public Transform nest;
    public float chaseSpeed = 5.1f;
    public float chaseSeconds = 9f;
    public float catchDistance = 1.45f;
    public float stunSeconds = 1.3f;
    private CharacterController controller;
    private float chaseRemaining;
    private float verticalVelocity;
    private float nextCatchTime;
    private StealEggsCharacterAnimation animationRig;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animationRig = GetComponent<StealEggsCharacterAnimation>();
    }

    public void Alert()
    {
        chaseRemaining = Mathf.Max(chaseRemaining, chaseSeconds);
        if (StealEggsGameManager.Instance != null)
            StealEggsGameManager.Instance.ShowMessage(guardianName + " is chasing you! Escape with the egg!");
    }

    private void Update()
    {
        if (chaseRemaining <= 0f || playerTarget == null)
        {
            chaseRemaining = Mathf.Max(0f, chaseRemaining - Time.deltaTime);
            if (animationRig != null) animationRig.SetMotion(0f, controller.isGrounded, verticalVelocity, false);
            return;
        }
        chaseRemaining -= Time.deltaTime;
        Vector3 delta = playerTarget.position - transform.position; delta.y = 0f;
        float distance = delta.magnitude;
        if (distance <= catchDistance && Time.time >= nextCatchTime)
        {
            nextCatchTime = Time.time + stunSeconds + 1.5f;
            StealEggsGameManager.Instance?.GuardianCaughtPlayer(guardianName, transform.position);
            chaseRemaining = 0f;
            return;
        }
        if (distance > .1f)
        {
            Vector3 direction = delta.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 1f - Mathf.Exp(-9f * Time.deltaTime));
            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            else verticalVelocity -= 24f * Time.deltaTime;
            Vector3 motion = direction * chaseSpeed; motion.y = verticalVelocity;
            controller.Move(motion * Time.deltaTime);
            if (animationRig != null) animationRig.SetMotion(1f, controller.isGrounded, verticalVelocity, false);
        }
    }
}
