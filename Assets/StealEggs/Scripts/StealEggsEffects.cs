using UnityEngine;

/// <summary>
/// Lightweight pickup burst made from tiny temporary meshes.
/// This deliberately avoids Unity's optional Particle System module.
/// </summary>
public class StealEggsEffects : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 initialScale;
    private float age;
    private float lifetime;
    private bool isBurstPiece;
    private Material ownedMaterial;

    public static void SpawnBurst(Vector3 position, Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");

        const int pieceCount = 14;
        for (int i = 0; i < pieceCount; i++)
        {
            PrimitiveType type = (i % 4 == 0) ? PrimitiveType.Cube : PrimitiveType.Sphere;
            GameObject piece = GameObject.CreatePrimitive(type);
            piece.name = "Egg Pickup Spark";
            piece.transform.position = position;

            float size = Random.Range(0.07f, 0.15f);
            piece.transform.localScale = new Vector3(size, size, size);

            Collider pieceCollider = piece.GetComponent<Collider>();
            if (pieceCollider != null)
            {
                pieceCollider.enabled = false;
                Object.Destroy(pieceCollider);
            }

            Renderer pieceRenderer = piece.GetComponent<Renderer>();
            Material material = null;
            if (shader != null)
            {
                material = new Material(shader);
                material.color = color;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", color * 0.35f);
                    material.EnableKeyword("_EMISSION");
                }
                pieceRenderer.sharedMaterial = material;
            }

            StealEggsEffects animation = piece.AddComponent<StealEggsEffects>();
            Vector3 direction = Random.insideUnitSphere + Vector3.up * 0.7f;
            if (direction.sqrMagnitude < 0.001f) direction = Vector3.up;
            animation.InitializePiece(direction.normalized * Random.Range(1.0f, 3.1f),
                Random.Range(0.28f, 0.58f), material);
        }
    }

    private void InitializePiece(Vector3 initialVelocity, float life, Material material)
    {
        velocity = initialVelocity;
        lifetime = Mathf.Max(0.1f, life);
        ownedMaterial = material;
        initialScale = transform.localScale;
        isBurstPiece = true;
    }

    private void Update()
    {
        if (!isBurstPiece) return;

        float delta = Time.deltaTime;
        age += delta;
        transform.position += velocity * delta;
        velocity += Vector3.down * (3.2f * delta);
        transform.Rotate(180f * delta, 240f * delta, 120f * delta, Space.Self);

        float progress = Mathf.Clamp01(age / lifetime);
        transform.localScale = initialScale * (1f - progress) * (1f - progress);

        if (age >= lifetime)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ownedMaterial != null)
            Destroy(ownedMaterial);
    }
}
