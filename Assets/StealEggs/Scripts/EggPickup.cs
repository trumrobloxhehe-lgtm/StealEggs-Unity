using UnityEngine;

public enum StealEggRarity { Common, Rare, Golden, Diamond }

/// <summary>Collectible egg. It disappears briefly when picked up, then respawns at the same nest.</summary>
public class EggPickup : MonoBehaviour
{
    [Tooltip("True for eggs initially placed beside the orange rival base.")]
    public bool spawnedAtRivalBase;
    public StealEggRarity rarity = StealEggRarity.Common;
    public int coinValue = 5;
    public float respawnSeconds = 10f;

    public bool IsAvailable => !collected;
    public int CoinValue => Mathf.Max(1, coinValue);
    public string RarityLabel => rarity.ToString();

    private bool collected;
    private Vector3 initialPosition;
    private float phase;
    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;

    private void Awake()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
        phase = Random.Range(0f, 10f);
    }

    private void Start() { initialPosition = transform.position; }

    private void Update()
    {
        if (collected) return;
        transform.Rotate(0f, 55f * Time.deltaTime, 0f, Space.World);
        Vector3 p = initialPosition;
        p.y += Mathf.Sin(Time.time * 2.5f + initialPosition.x + phase) * 0.12f;
        transform.position = p;
    }

    public void SetRarity(StealEggRarity newRarity, int value)
    {
        rarity = newRarity;
        coinValue = Mathf.Max(1, value);
    }

    public void PickUpByPlayer()
    {
        if (collected) return;
        StealEggsGameManager manager = StealEggsGameManager.Instance;
        if (manager == null || !manager.CanCollectMoreEggs()) return;
        collected = true;
        manager.PlayerCollectedEgg(this);
        HideAndRespawn();
    }

    public void PickUpByRival()
    {
        if (collected) return;
        collected = true;
        if (StealEggsGameManager.Instance != null)
            StealEggsGameManager.Instance.RivalCollectedEgg(this);
        HideAndRespawn();
    }

    private void HideAndRespawn()
    {
        foreach (Renderer r in cachedRenderers) if (r != null) r.enabled = false;
        foreach (Collider c in cachedColliders) if (c != null) c.enabled = false;
        StealEggsEffects.SpawnBurst(transform.position, GetRarityColor());
        Invoke(nameof(RespawnEgg), Mathf.Max(4f, respawnSeconds));
    }

    private void RespawnEgg()
    {
        transform.position = initialPosition;
        foreach (Renderer r in cachedRenderers) if (r != null) r.enabled = true;
        foreach (Collider c in cachedColliders) if (c != null) c.enabled = true;
        collected = false;
    }

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case StealEggRarity.Rare: return new Color(.2f, .85f, 1f);
            case StealEggRarity.Golden: return new Color(1f, .68f, .08f);
            case StealEggRarity.Diamond: return new Color(.75f, .45f, 1f);
            default: return new Color(1f, .93f, .74f);
        }
    }
}
