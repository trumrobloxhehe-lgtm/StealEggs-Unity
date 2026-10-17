using UnityEngine;

/// <summary>Stand on this treadmill to train permanent speed and unlock new areas.</summary>
[RequireComponent(typeof(Collider))]
public class StealEggsTrainingPad : MonoBehaviour
{
    public float trainingPerSecond = 3f;
    private float messageTimer;

    private void Reset() { GetComponent<Collider>().isTrigger = true; }

    private void OnTriggerStay(Collider other)
    {
        StealEggsPlayerController player = other.GetComponent<StealEggsPlayerController>();
        if (player == null) player = other.GetComponentInParent<StealEggsPlayerController>();
        if (player == null || StealEggsGameManager.Instance == null) return;
        StealEggsGameManager.Instance.TrainSpeed(trainingPerSecond * Time.deltaTime);
        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f)
        {
            StealEggsGameManager.Instance.ShowMessage("TRAINING! Speed " +
                StealEggsGameManager.Instance.TrainingSpeed.ToString("F0") + " — stronger speed unlocks more zones.");
            messageTimer = 1.4f;
        }
    }
}
