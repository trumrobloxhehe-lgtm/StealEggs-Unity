#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.IO;

public static class StealEggsDemoSetup
{
    private const string Root = "Assets/StealEggs";
    private const string ScenePath = Root + "/Scenes/StealEggsDemo.unity";
    private const string MaterialsPath = Root + "/Materials";

    [MenuItem("Steal Eggs/Build Demo Arena")]
    public static void BuildDemoArena()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder(Root + "/Scenes");
        EnsureFolder(MaterialsPath);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material grass = MakeMaterial("Grass", new Color(0.25f, 0.48f, 0.25f));
        Material pathMat = MakeMaterial("ArenaPath", new Color(0.48f, 0.48f, 0.46f));
        Material wallMat = MakeMaterial("ArenaFence", new Color(0.23f, 0.25f, 0.28f));
        Material blue = MakeMaterial("PlayerBlue", new Color(0.12f, 0.55f, 1f));
        Material orange = MakeMaterial("RivalOrange", new Color(1f, 0.38f, 0.08f));
        Material gold = MakeMaterial("EggGold", new Color(1f, 0.82f, 0.12f));
        Material white = MakeMaterial("EggCream", new Color(1f, 0.94f, 0.72f));

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Arena Ground";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(4f, 1f, 4f);
        floor.GetComponent<Renderer>().sharedMaterial = grass;

        CreateCube("Center Path", new Vector3(0f, 0.015f, 0f), new Vector3(30f, 0.03f, 5f), pathMat);
        CreateCube("North Fence", new Vector3(0f, 0.7f, 20f), new Vector3(40f, 1.4f, 0.5f), wallMat);
        CreateCube("South Fence", new Vector3(0f, 0.7f, -20f), new Vector3(40f, 1.4f, 0.5f), wallMat);
        CreateCube("West Fence", new Vector3(-20f, 0.7f, 0f), new Vector3(0.5f, 1.4f, 40f), wallMat);
        CreateCube("East Fence", new Vector3(20f, 0.7f, 0f), new Vector3(0.5f, 1.4f, 40f), wallMat);

        Transform playerBase = CreateBase("BLUE HOME BASE", new Vector3(-15f, 0.12f, 0f), blue);
        Transform rivalBase = CreateBase("ORANGE RIVAL BASE", new Vector3(15f, 0.12f, 0f), orange);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(-13.5f, 1f, 0f);
        player.GetComponent<Renderer>().sharedMaterial = blue;
        Collider playerCollider = player.GetComponent<Collider>();
        Object.DestroyImmediate(playerCollider);
        CharacterController playerCC = player.AddComponent<CharacterController>();
        playerCC.height = 2f;
        playerCC.radius = 0.42f;
        playerCC.center = Vector3.zero;
        player.AddComponent<StealEggsPlayerController>();

        GameObject rival = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rival.name = "Rival Bot";
        rival.transform.position = new Vector3(13.5f, 1f, 0f);
        rival.GetComponent<Renderer>().sharedMaterial = orange;
        Collider rivalCollider = rival.GetComponent<Collider>();
        Object.DestroyImmediate(rivalCollider);
        CharacterController rivalCC = rival.AddComponent<CharacterController>();
        rivalCC.height = 2f;
        rivalCC.radius = 0.42f;
        rivalCC.center = Vector3.zero;
        StealEggsRivalAI rivalAI = rival.AddComponent<StealEggsRivalAI>();
        rivalAI.rivalBase = rivalBase;

        Vector3[] positions = new Vector3[]
        {
            new Vector3(-7f, 0.8f, 0f), new Vector3(-5f, 0.8f, 2f), new Vector3(-3f, 0.8f, -2f),
            new Vector3(-1f, 0.8f, 1.5f), new Vector3(1f, 0.8f, -1.5f), new Vector3(3f, 0.8f, 2f),
            new Vector3(5f, 0.8f, -2f), new Vector3(7f, 0.8f, 0f), new Vector3(9f, 0.8f, 2.5f),
            new Vector3(11f, 0.8f, -2.5f), new Vector3(14f, 0.8f, 3f), new Vector3(16f, 0.8f, -3f),
            new Vector3(15f, 0.8f, 1f), new Vector3(12.5f, 0.8f, 1.2f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            bool rivalSide = positions[i].x >= 12f;
            CreateEgg("Egg " + (i + 1), positions[i], rivalSide, rivalSide ? gold : white);
        }

        GameObject managerObject = new GameObject("Steal Eggs Game Manager");
        StealEggsGameManager manager = managerObject.AddComponent<StealEggsGameManager>();
        manager.playerHomeBase = playerBase;
        manager.rivalHomeBase = rivalBase;

        CreateHUD(manager);
        CreateCamera(player.transform);
        CreateLight();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        bool saved = EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (saved)
        {
            EditorUtility.DisplayDialog("Steal Eggs", "Demo arena created! Press Play to test.\n\nW/A/S/D or arrow keys = move\nE = collect a nearby egg\nReturn to the BLUE base to score.", "OK");
            Debug.Log("Steal Eggs demo scene created at " + ScenePath);
        }
    }

    private static void CreateEgg(string objectName, Vector3 position, bool fromRivalBase, Material material)
    {
        GameObject egg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        egg.name = objectName;
        egg.transform.position = position;
        egg.transform.localScale = new Vector3(0.65f, 0.82f, 0.65f);
        egg.GetComponent<Renderer>().sharedMaterial = material;
        EggPickup pickup = egg.AddComponent<EggPickup>();
        pickup.spawnedAtRivalBase = fromRivalBase;
    }

    private static Transform CreateBase(string objectName, Vector3 position, Material material)
    {
        GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObject.name = objectName;
        baseObject.transform.position = position;
        baseObject.transform.localScale = new Vector3(5f, 0.12f, 5f);
        baseObject.GetComponent<Renderer>().sharedMaterial = material;
        return baseObject.transform;
    }

    private static GameObject CreateCube(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = objectName;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private static void CreateCamera(Transform player)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 150f;
        camera.fieldOfView = 55f;
        cameraObject.AddComponent<AudioListener>();
        StealEggsCameraFollow follow = cameraObject.AddComponent<StealEggsCameraFollow>();
        follow.target = player;
        follow.offset = new Vector3(0f, 18f, -16f);
        cameraObject.transform.position = player.position + follow.offset;
        cameraObject.transform.LookAt(player.position + Vector3.up * 0.5f);
        camera.clearFlags = CameraClearFlags.Skybox;
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Sun Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.72f);
        RenderSettings.ambientEquatorColor = new Color(0.36f, 0.39f, 0.34f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.2f, 0.17f);
    }

    private static void CreateHUD(StealEggsGameManager manager)
    {
        GameObject canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("HUD Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(18f, -18f);
        panelRect.sizeDelta = new Vector2(500f, 145f);
        panelObject.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 0.78f);

        Text hud = CreateText(panelObject.transform, "Score Text", new Vector2(12f, -10f), new Vector2(470f, 125f), 23, TextAnchor.UpperLeft);
        hud.color = Color.white;
        hud.fontStyle = FontStyle.Bold;
        manager.hudText = hud;

        Text message = CreateText(canvasObject.transform, "Message Text", new Vector2(0f, 30f), new Vector2(1100f, 54f), 24, TextAnchor.MiddleCenter);
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0f);
        messageRect.anchorMax = new Vector2(0.5f, 0f);
        messageRect.pivot = new Vector2(0.5f, 0f);
        message.color = Color.white;
        message.fontStyle = FontStyle.Bold;
        manager.messageText = message;

        Text controls = CreateText(canvasObject.transform, "Controls Text", new Vector2(-18f, 18f), new Vector2(350f, 60f), 18, TextAnchor.LowerRight);
        RectTransform controlsRect = controls.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(1f, 0f);
        controlsRect.anchorMax = new Vector2(1f, 0f);
        controlsRect.pivot = new Vector2(1f, 0f);
        controls.text = "MOVE: WASD / ARROWS\nPICK UP: E";
        controls.color = Color.white;
    }

    private static Text CreateText(Transform parent, string objectName, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = string.Empty;
        return text;
    }

    private static Material MakeMaterial(string materialName, Color color)
    {
        string path = MaterialsPath + "/" + materialName + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string assetFolder)
    {
        if (AssetDatabase.IsValidFolder(assetFolder)) return;
        string parent = Path.GetDirectoryName(assetFolder).Replace("\\", "/");
        string folderName = Path.GetFileName(assetFolder);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
