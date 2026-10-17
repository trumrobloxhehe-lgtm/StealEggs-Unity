using UnityEngine;

/// <summary>An egg that can be collected by the player or the rival bot.</summary>
public class EggPickup : MonoBehaviour
{
    [Tooltip("True for eggs initially placed beside the orange rival base.")]
    public bool spawnedAtRivalBase;

    private bool collected;
    private Vector3 initialPosition;
    private float spinSpeed = 45f;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (collected) return;
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        Vector3 position = initialPosition;
        position.y += Mathf.Sin(Time.time * 2.5f + initialPosition.x) * 0.12f;
        transform.position = position;
    }

    public void PickUpByPlayer()
    {
        if (collected) return;
        collected = true;
        if (StealEggsGameManager.Instance != null)
            StealEggsGameManager.Instance.PlayerCollectedEgg(spawnedAtRivalBase);
        Destroy(gameObject);
    }

    public void PickUpByRival()
    {
        if (collected) return;
        collected = true;
        if (StealEggsGameManager.Instance != null)
            StealEggsGameManager.Instance.RivalCollectedEgg(spawnedAtRivalBase);
        Destroy(gameObject);
    }
}
