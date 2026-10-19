#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class StealEggsFoliage
{
    private const string MaterialsPath = "Assets/StealEggs/Materials";
    private const string TexturesPath = "Assets/StealEggs/Textures";

    public static void Build(Transform root, Vector3 worldPos, Material trunkMat, Material leafMat, float scale)
    {
        EnsureFolder(MaterialsPath);
        EnsureFolder(TexturesPath);

        int kind = 0;
        if (worldPos.x > 8f && worldPos.x < 42f && Mathf.Abs(worldPos.z) > 16f && Random.value < .62f) kind = 2;
        else if (worldPos.x < 10f ? Random.value < .72f : Random.value < .38f) kind = 1;

        Color leaf = leafMat != null ? leafMat.color : new Color(.16f, .42f, .18f);
        if (kind == 1) leaf = Color.Lerp(new Color(.07f, .28f, .12f), leaf, .35f);
        if (kind == 2) leaf = Color.Lerp(new Color(.10f, .46f, .16f), leaf, .25f);

        Material bark = trunkMat != null ? trunkMat : Mat("TreeTrunk", new Color(.32f, .18f, .08f));
        Material barkLight = Mat("BarkHighlight", new Color(.46f, .28f, .14f));
        Material canopy = CardMat(kind == 1 ? "PineNeedles" : kind == 2 ? "PalmFronds" : "BroadleafCanopy", leaf, kind);
        Material canopyLight = CardMat(kind == 1 ? "PineNeedlesSun" : kind == 2 ? "PalmFrondsSun" : "BroadleafCanopySun", Color.Lerp(leaf, new Color(.78f, .86f, .42f), .38f), kind);
        Material canopyDark = CardMat(kind == 1 ? "PineNeedlesShade" : kind == 2 ? "PalmFrondsShade" : "BroadleafCanopyShade", Color.Lerp(leaf, new Color(.03f, .08f, .03f), .45f), kind);

        root.name = kind == 1 ? "Pine Tree" : kind == 2 ? "Palm Tree" : "Broadleaf Tree";
        root.localScale = Vector3.one * scale;
        float lean = Random.Range(-3.5f, 3.5f);

        if (kind == 1) Pine(root, bark, barkLight, canopy, canopyLight, canopyDark, lean);
        else if (kind == 2) Palm(root, bark, barkLight, canopy, canopyLight, lean);
        else Broadleaf(root, bark, barkLight, canopy, canopyLight, canopyDark, lean);
    }

    private static void Pine(Transform root, Material bark, Material barkLight, Material canopy, Material canopyLight, Material canopyDark, float lean)
    {
        float height = Random.Range(6.4f, 8.6f);
        Trunk(root, height, .34f, .11f, bark, barkLight, lean, 4);
        int layers = Random.Range(7, 10);
        for (int layer = 0; layer < layers; layer++)
        {
            float t = layer / (float)(layers - 1);
            float y = height * Mathf.Lerp(.22f, .96f, t);
            float radius = Mathf.Lerp(1.85f, .22f, Mathf.Pow(t, .85f)) * Random.Range(.92f, 1.08f);
            float cardHeight = Mathf.Lerp(1.55f, .62f, t);
            Material mat = t < .25f ? canopyDark : (t > .72f ? canopyLight : canopy);
            float yaw = layer * 18f + Random.Range(0f, 20f);
            for (int fan = 0; fan < 3; fan++)
                LeafCard(root, new Vector3(0f, y, 0f), new Vector3(radius * 2.05f, cardHeight, 1f), Quaternion.Euler(Random.Range(-8f, 6f), yaw + fan * 60f, 0f), mat, "Pine Fan");
        }
    }

    private static void Broadleaf(Transform root, Material bark, Material barkLight, Material canopy, Material canopyLight, Material canopyDark, float lean)
    {
        float height = Random.Range(4.6f, 6.4f);
        Trunk(root, height * .62f, .42f, .16f, bark, barkLight, lean, 3);
        int limbs = Random.Range(4, 6);
        for (int i = 0; i < limbs; i++)
        {
            float yaw = i * (360f / limbs) + Random.Range(-18f, 18f);
            Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            float limbH = height * Random.Range(.48f, .7f);
            Vector3 start = new Vector3(0f, limbH, 0f);
            Vector3 end = start + dir * Random.Range(1.05f, 1.7f) + Vector3.up * Random.Range(.35f, .9f);
            Branch(root, start, end, Random.Range(.06f, .09f), barkLight, "Limb");
            Puff(root, end, Random.Range(.95f, 1.45f), canopy, canopyLight, canopyDark, 4);
        }
        Puff(root, new Vector3(0f, height * .78f, 0f), Random.Range(1.35f, 1.85f), canopy, canopyLight, canopyDark, 6);
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = Quaternion.Euler(0f, i * 120f + Random.Range(-20f, 20f), 0f) * new Vector3(Random.Range(.55f, .9f), 0f, 0f);
            Puff(root, new Vector3(offset.x, Random.Range(.28f, .48f), offset.z), Random.Range(.35f, .55f), canopyDark, canopy, canopyDark, 2);
        }
    }

    private static void Palm(Transform root, Material bark, Material barkLight, Material canopy, Material canopyLight, float lean)
    {
        int segments = 6;
        float height = Random.Range(5.2f, 6.8f);
        Vector3 prev = Vector3.zero;
        float curve = Random.Range(-.55f, .55f);
        for (int i = 0; i < segments; i++)
        {
            float t0 = i / (float)segments;
            float t1 = (i + 1) / (float)segments;
            Vector3 a = new Vector3(curve * t0 * t0, height * t0, lean * .01f * t0);
            Vector3 b = new Vector3(curve * t1 * t1, height * t1, lean * .01f * t1);
            Branch(root, a, b, Mathf.Lerp(.18f, .08f, t0), i % 2 == 0 ? bark : barkLight, "Palm Trunk");
            prev = b;
        }
        int fronds = Random.Range(8, 11);
        for (int i = 0; i < fronds; i++)
        {
            float yaw = i * (360f / fronds) + Random.Range(-8f, 8f);
            float droop = Random.Range(18f, 48f);
            float length = Random.Range(2.1f, 3.1f);
            Vector3 crown = prev + Vector3.up * .15f;
            LeafCard(root, crown + Quaternion.Euler(droop, yaw, 0f) * (Vector3.up * length * .42f), new Vector3(Random.Range(.55f, .78f), length, 1f), Quaternion.Euler(droop, yaw, 0f), i % 2 == 0 ? canopyLight : canopy, "Palm Frond");
        }
    }

    private static void Puff(Transform root, Vector3 center, float radius, Material canopy, Material light, Material dark, int cards)
    {
        for (int i = 0; i < cards; i++)
        {
            Vector3 offset = Random.insideUnitSphere * radius * .55f;
            offset.y = Mathf.Abs(offset.y) * .65f;
            float size = radius * Random.Range(.85f, 1.25f);
            Material mat = i == 0 ? light : (i % 3 == 0 ? dark : canopy);
            LeafCard(root, center + offset, new Vector3(size * Random.Range(1.1f, 1.45f), size * Random.Range(.72f, 1.05f), 1f), Quaternion.Euler(Random.Range(-18f, 18f), Random.Range(0f, 180f), Random.Range(-12f, 12f)), mat, "Leaf Card");
        }
    }

    private static void Trunk(Transform root, float height, float baseRadius, float topRadius, Material bark, Material barkLight, float lean, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float t0 = i / (float)segments;
            float t1 = (i + 1) / (float)segments;
            float y0 = height * t0;
            float y1 = height * t1;
            float radius = Mathf.Lerp(baseRadius, topRadius, (t0 + t1) * .5f);
            Vector3 a = new Vector3(Mathf.Sin(lean * .04f) * y0 * .08f, y0, 0f);
            Vector3 b = new Vector3(Mathf.Sin(lean * .04f) * y1 * .08f, y1, 0f);
            Branch(root, a, b, radius, i % 2 == 0 ? bark : barkLight, "Trunk");
        }
        for (int i = 0; i < 4; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, i * 90f + Random.Range(-16f, 16f), 0f) * Vector3.forward;
            Branch(root, new Vector3(0f, .18f, 0f), dir * Random.Range(.45f, .8f) + Vector3.up * .05f, baseRadius * .42f, bark, "Root Flare");
        }
    }

    private static void Branch(Transform parent, Vector3 start, Vector3 end, float thickness, Material material, string objectName)
    {
        Vector3 delta = end - start;
        if (delta.sqrMagnitude < .0001f) return;
        GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        branch.name = objectName;
        branch.transform.SetParent(parent, false);
        branch.transform.localPosition = (start + end) * .5f;
        branch.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        branch.transform.localScale = new Vector3(thickness, delta.magnitude * .5f, thickness);
        branch.GetComponent<Renderer>().sharedMaterial = material;
        Strip(branch);
    }

    private static void LeafCard(Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material, string objectName)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Quad);
            card.name = objectName;
            card.transform.SetParent(parent, false);
            card.transform.localPosition = localPosition;
            card.transform.localRotation = localRotation * Quaternion.Euler(0f, i * 90f, 0f);
            card.transform.localScale = localScale;
            Renderer renderer = card.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
            Strip(card);
        }
    }

    private static void Strip(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static Material CardMat(string name, Color tint, int kind)
    {
        Material material = Mat(name, tint);
        Texture2D texture = FoliageTexture(kind == 1 ? "pine" : kind == 2 ? "palm" : "broad");
        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
        material.color = tint;
        if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 1f);
        material.EnableKeyword("_ALPHATEST_ON");
        if (material.HasProperty("_Cutoff")) material.SetFloat("_Cutoff", .38f);
        if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .06f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        material.SetOverrideTag("RenderType", "TransparentCutout");
        material.renderQueue = (int)RenderQueue.AlphaTest;
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture2D FoliageTexture(string kind)
    {
        string assetPath = TexturesPath + "/FoliageCard_" + kind + ".asset";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (existing != null) return existing;
        const int width = 128;
        const int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, true, false);
        texture.name = "FoliageCard_" + kind;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[width * height];
        float seed = kind.Length * 1.7f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (x + .5f) / width;
                float v = (y + .5f) / height;
                float dx = u - .5f;
                float alpha = 0f;
                float shade = .75f + .25f * Mathf.PerlinNoise(u * 6f + seed, v * 6f);
                if (kind == "pine")
                {
                    float ang = Mathf.Atan2(dx, v + .02f);
                    float dist = Mathf.Sqrt(dx * dx * 3.2f + (1f - v) * (1f - v));
                    float spokes = Mathf.Pow(Mathf.Abs(Mathf.Sin(ang * 17f + seed)), .35f);
                    float fan = Mathf.Exp(-spokes * 5.5f) * Mathf.Clamp01(1.15f - dist * 1.05f);
                    alpha = Mathf.Clamp01(fan * .95f + Mathf.Exp(-Mathf.Abs(dx) * 18f) * Mathf.Clamp01(v * 1.4f) * .35f);
                    shade *= Mathf.Lerp(.72f, 1f, v);
                }
                else if (kind == "palm")
                {
                    float rib = Mathf.Exp(-Mathf.Abs(dx) * 26f);
                    float leaflet = Mathf.Abs(Mathf.Sin((v * 42f) + dx * 8f));
                    float side = Mathf.Exp(-Mathf.Pow(Mathf.Abs(dx) * 3.1f, 1.4f)) * Mathf.Clamp01(1.05f - Mathf.Abs(v - .5f) * 1.7f);
                    alpha = Mathf.Clamp01(rib * .9f + side * (1f - leaflet) * .85f);
                }
                else
                {
                    for (int leaf = 0; leaf < 7; leaf++)
                    {
                        float lx = .5f + Mathf.Sin(seed + leaf * 2.1f) * .28f;
                        float ly = .18f + (leaf * .11f) % .72f;
                        float nx = (u - lx) / (.10f + (leaf % 3) * .015f);
                        float ny = (v - ly) / (.07f + (leaf % 2) * .02f);
                        float blob = 1f - (nx * nx + ny * ny);
                        if (blob > 0f) alpha = Mathf.Max(alpha, Mathf.Clamp01(blob * 1.35f));
                    }
                    alpha = Mathf.Clamp01(Mathf.Max(alpha, Mathf.Exp(-Mathf.Abs(dx) * 22f) * Mathf.Clamp01(v) * .55f));
                }
                alpha *= Mathf.Clamp01(Mathf.Min(u, 1f - u, v, 1f - v) * 18f);
                Color rgb = Color.Lerp(new Color(.62f, .78f, .48f), new Color(.93f, 1f, .72f), shade);
                pixels[y * width + x] = new Color(rgb.r, rgb.g, rgb.b, alpha > .08f ? alpha : 0f);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply(true, false);
        AssetDatabase.CreateAsset(texture, assetPath);
        return texture;
    }

    private static Material Mat(string name, Color color)
    {
        string path = MaterialsPath + "/" + name + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (existing != null)
        {
            if (shader != null && existing.shader != shader && (existing.shader == null || existing.shader.name == "Standard"))
                existing.shader = shader;
            existing.color = color;
            if (existing.HasProperty("_BaseColor")) existing.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        Material created = new Material(shader);
        created.name = name;
        created.color = color;
        if (created.HasProperty("_BaseColor")) created.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(created, path);
        return created;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(folder);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
