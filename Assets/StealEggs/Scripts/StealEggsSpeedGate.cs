using UnityEngine;

/// <summary>Blocks the next region until the player has trained enough speed.</summary>
[RequireComponent(typeof(Collider))]
public class StealEggsSpeedGate : MonoBehaviour
{
    public float requiredSpeed = 10f;
    public string regionName = "Next region";
    private Collider gateCollider;
    private Renderer gateRenderer;
    private bool unlocked;

    private void Awake()
    {
        gateCollider = GetComponent<Collider>();
        gateRenderer = GetComponent<Renderer>();
        if (gateCollider != null) gateCollider.isTrigger = false;
    }

    private void Update()
    {
        float speed = StealEggsGameManager.Instance != null ? StealEggsGameManager.Instance.TrainingSpeed : 0f;
        bool nowUnlocked = speed >= requiredSpeed;
        if (nowUnlocked == unlocked) return;
        unlocked = nowUnlocked;
        if (gateCollider != null) gateCollider.enabled = !unlocked;
        if (gateRenderer != null) gateRenderer.material.color = unlocked
            ? new Color(.18f, .8f, .35f, .65f)
            : new Color(1f, .12f, .12f, 1f);
        if (unlocked && StealEggsGameManager.Instance != null)
            StealEggsGameManager.Instance.ShowMessage(regionName + " unlocked!");
    }
}
