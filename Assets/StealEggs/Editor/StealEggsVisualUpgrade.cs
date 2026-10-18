#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds a lightweight visual polish pass for the current Steal Eggs scene.
/// Uses Unity's built-in renderer so it does not require the URP package.
/// </summary>
public static class StealEggsVisualUpgrade
{
    private const string RootName = "__StealEggs_VisualUpgrade";
    private const string VisualFolder = "Assets/StealEggs/Visuals";
    private const string MaterialFolder = VisualFolder + "/Materials";

    [MenuItem("Tools/Steal Eggs/Apply Visual Upgrade")]
    public static void ApplyUpgrade()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Steal Eggs", "Stop Play Mode before applying the visual upgrade.", "OK");
            return;
        }

        if (!EditorSceneManager.GetActiveScene().IsValid())
        {
            EditorUtility.DisplayDialog("Steal Eggs", "Open the Steal Eggs demo scene first.", "OK");
            return;
        }

        EnsureFolders();

        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot != null)
            Undo.DestroyObjectImmediate(oldRoot);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Apply Steal Eggs Visual Upgrade");

        ApplyEnvironment();
        ApplyCameraSettings();
        PolishEggMaterials();
        CreateBaseLights(root.transform);
        CreateRouteMarkers(root.transform);
        CreateCrystalLandmarks(root.transform);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Visual Upgrade Applied",
            "Lighting, egg materials, base accents, and route details have been added. Check the scene, then press Ctrl+S to save. This upgrade uses built-in Unity rendering and does not add URP.",
            "OK");
    }

    private static void ApplyEnvironment()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.34f, 0.48f, 0.68f);
        RenderSettings.ambientEquatorColor = new Color(0.34f, 0.38f, 0.32f);
        RenderSettings.ambientGroundColor = new Color(0.16f, 0.18f, 0.20f);
        RenderSettings.ambientIntensity = 1.12f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.62f, 0.73f, 0.83f);
        RenderSettings.fogDensity = 0.00065f;
        RenderSettings.reflectionIntensity = 0.8f;

        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader != null)
        {
            string skyPath = VisualFolder + "/StealEggsSky.mat";
            Material sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if (sky == null)
            {
                sky = new Material(skyShader);
                AssetDatabase.CreateAsset(sky, skyPath);
            }
            sky.shader = skyShader;
            if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", 0.035f);
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 0.85f);
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.15f);
            RenderSettings.skybox = sky;
        }

        Light sun = null;
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light candidate in lights)
        {
            if (candidate.name.Contains("Sun") || candidate.type == LightType.Directional)
            {
                sun = candidate;
                break;
            }
        }

        if (sun == null)
        {
            GameObject lightObject = new GameObject("Sun Light - Visual Upgrade");
            Undo.RegisterCreatedObjectUndo(lightObject, "Create Sun Light");
            sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        Undo.RecordObject(sun, "Polish Sun Light");
        sun.name = "Sun Light";
        sun.type = LightType.Directional;
        sun.intensity = 1.25f;
        sun.color = new Color(1f, 0.90f, 0.76f);
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.78f;
        sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

        QualitySettings.antiAliasing = 4;
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowDistance = 150f;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.lodBias = 1.8f;

        DynamicGI.UpdateEnvironment();
    }

    private static void ApplyCameraSettings()
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera == null) continue;
            Undo.RecordObject(camera, "Polish Camera");
            camera.allowHDR = true;
            camera.allowMSAA = true;
        }
    }

    private static void PolishEggMaterials()
    {
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer target in renderers)
        {
            if (target == null || !target.gameObject.name.ToLowerInvariant().Contains("egg"))
                continue;

            string label = target.gameObject.name.ToLowerInvariant();
            string materialName;
            Color color;
            Color emission;
            float metallic;
            float smoothness = 0.82f;

            if (label.Contains("diamond"))
            {
                materialName = "Egg_Diamond_Polished";
                color = new Color(0.72f, 0.50f, 1f);
                emission = new Color(0.20f, 0.08f, 0.48f);
                metallic = 0.26f;
            }
            else if (label.Contains("gold") || label.Contains("golden"))
            {
                materialName = "Egg_Gold_Polished";
                color = new Color(1f, 0.69f, 0.12f);
                emission = new Color(0.35f, 0.16f, 0.01f);
                metallic = 0.58f;
            }
            else if (label.Contains("rare"))
            {
                materialName = "Egg_Rare_Polished";
                color = new Color(0.18f, 0.82f, 1f);
                emission = new Color(0.02f, 0.20f, 0.32f);
                metallic = 0.32f;
            }
            else
            {
                materialName = "Egg_Common_Polished";
                color = new Color(1f, 0.93f, 0.72f);
                emission = new Color(0.12f, 0.075f, 0.025f);
                metallic = 0.12f;
            }

            Material material = CreateMaterial(materialName, color, metallic, smoothness, emission);
            target.sharedMaterial = material;
        }
    }

    private static void CreateBaseLights(Transform parent)
    {
        Material blue = CreateMaterial("Accent_Blue", new Color(0.08f, 0.45f, 1f), 0.18f, 0.72f, new Color(0.03f, 0.24f, 1f));
        Material orange = CreateMaterial("Accent_Orange", new Color(1f, 0.28f, 0.06f), 0.16f, 0.72f, new Color(1f, 0.10f, 0.015f));
        CreateBaseRing(parent, "BLUE", new Vector3(-65f, 0f, 0f), blue);
        CreateBaseRing(parent, "ORANGE", new Vector3(65f, 0f, 0f), orange);
    }

    private static void CreateBaseRing(Transform parent, string label, Vector3 center, Material material)
    {
        const int count = 18;
        const float radius = 5.65f;
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i) / count;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, 0.39f, Mathf.Sin(angle) * radius);
            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = label + " Base - Emissive Marker";
            bulb.transform.SetParent(parent);
            bulb.transform.position = position;
            bulb.transform.localScale = new Vector3(0.30f, 0.12f, 0.30f);
            bulb.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = bulb.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }
    }

    private static void CreateRouteMarkers(Transform parent)
    {
        Color[] palette =
        {
            new Color(0.10f, 0.46f, 1f),
            new Color(0.08f, 0.78f, 0.90f),
            new Color(0.26f, 0.88f, 0.76f),
            new Color(1f, 0.68f, 0.16f)
        };

        float[] positions = { -48f, -31f, -14f, 14f, 31f, 48f };
        for (int i = 0; i < positions.Length; i++)
        {
            Material material = CreateMaterial("RouteGlow_" + i, palette[Mathf.Min(i / 2, palette.Length - 1)], 0.08f, 0.65f, palette[Mathf.Min(i / 2, palette.Length - 1)] * 0.35f);
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "Route Accent";
                marker.transform.SetParent(parent);
                marker.transform.position = new Vector3(positions[i], 0.045f, side * 8.5f);
                marker.transform.localScale = new Vector3(0.34f, 0.025f, 0.34f);
                marker.GetComponent<Renderer>().sharedMaterial = material;
                Collider collider = marker.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }
        }
    }

    private static void CreateCrystalLandmarks(Transform parent)
    {
        Material cyan = CreateMaterial("Crystal_Cyan", new Color(0.18f, 0.82f, 1f), 0.24f, 0.88f, new Color(0.04f, 0.40f, 0.72f));
        Material purple = CreateMaterial("Crystal_Violet", new Color(0.58f, 0.31f, 0.95f), 0.28f, 0.88f, new Color(0.24f, 0.04f, 0.48f));

        Vector3[] positions =
        {
            new Vector3(-23f, 0f, 16f), new Vector3(-19f, 0f, 18f),
            new Vector3(23f, 0f, -16f), new Vector3(27f, 0f, -18f),
            new Vector3(49f, 0f, 20f), new Vector3(53f, 0f, 17f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            crystal.name = "Decorative Crystal Landmark";
            crystal.transform.SetParent(parent);
            crystal.transform.position = positions[i] + new Vector3(0f, 1.15f, 0f);
            crystal.transform.localScale = new Vector3(0.45f, 1.45f, 0.45f);
            crystal.transform.rotation = Quaternion.Euler(0f, i * 37f, (i % 2 == 0) ? 10f : -10f);
            crystal.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? cyan : purple;
            Collider collider = crystal.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }
    }

    private static Material CreateMaterial(string label, Color color, float metallic, float smoothness, Color emission)
    {
        string path = MaterialFolder + "/" + label + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            material.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(VisualFolder))
            AssetDatabase.CreateFolder("Assets/StealEggs", "Visuals");
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder(VisualFolder, "Materials");
    }
}
#endif
