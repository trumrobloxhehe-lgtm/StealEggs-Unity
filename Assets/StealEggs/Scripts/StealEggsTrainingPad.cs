using UnityEngine;

/// <summary>Stand on this treadmill to train permanent speed and unlock new areas.</summary>
[RequireComponent(typeof(Collider))]
public class StealEggsTrainingPad : MonoBehaviour
{
    public float trainingPerSecond = 3f;
    private float messageTimer;
    private Collider zone;

    private void Awake()
    {
        zone = GetComponent<Collider>();
        if (zone != null) zone.isTrigger = true;
    }

    // Bounds overlap also works with a CharacterController when no Rigidbody is present.
    private void Update()
    {
        if (zone == null || StealEggsGameManager.Instance == null) return;
        StealEggsPlayerController[] players = Object.FindObjectsByType<StealEggsPlayerController>(FindObjectsSortMode.None);
        foreach (StealEggsPlayerController player in players)
        {
            if (player == null) continue;
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null || !zone.bounds.Intersects(controller.bounds)) continue;
            StealEggsGameManager.Instance.TrainSpeed(trainingPerSecond * Time.deltaTime);
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                StealEggsGameManager.Instance.ShowMessage("TRAINING! Speed " +
                    StealEggsGameManager.Instance.TrainingSpeed.ToString("F0") + " — train more to unlock new regions.");
                messageTimer = 1.4f;
            }
        }
    }
}
