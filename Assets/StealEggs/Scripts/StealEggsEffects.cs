using UnityEngine;

/// <summary>Small self-contained particle bursts for pickups and deliveries.</summary>
public static class StealEggsEffects
{
    public static void SpawnBurst(Vector3 position, Color color)
    {
        GameObject obj = new GameObject("Egg Pickup Burst");
        obj.transform.position = position;
        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.duration = .35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(.25f, .6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(.06f, .16f);
        main.startColor = color;
        main.loop = false;
        main.maxParticles = 30;
        main.gravityModifier = .35f;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = .15f;
        ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader != null) renderer.material = new Material(shader);
        ps.Play();
        Object.Destroy(obj, 2f);
    }
}
