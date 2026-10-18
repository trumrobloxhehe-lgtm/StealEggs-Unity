#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
        EnsureFolder(Root + "/Scenes"); EnsureFolder(MaterialsPath);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Random.InitState(20261018);

        Material grass = Mat("Grass", new Color(.22f, .48f, .24f));
        Material path = Mat("ArenaPath", new Color(.50f, .49f, .47f));
        Material fence = Mat("ArenaFence", new Color(.22f, .25f, .29f));
        Material blue = Mat("PlayerBlue", new Color(.10f, .42f, .94f));
        Material orange = Mat("RivalOrange", new Color(.96f, .29f, .08f));
        Material pants = Mat("AvatarPants", new Color(.10f, .16f, .30f));
        Material skin = Mat("AvatarSkin", new Color(.92f, .68f, .45f));
        Material hair = Mat("AvatarHair", new Color(.18f, .10f, .06f));
        Material shoes = Mat("AvatarShoes", new Color(.08f, .08f, .10f));
        Material face = Mat("AvatarFace", new Color(.035f, .025f, .02f));
        Material eggCommon = Mat("EggCream", new Color(1f, .94f, .75f));
        Material eggRare = Mat("EggRare", new Color(.18f, .84f, 1f));
        Material eggGold = Mat("EggGold", new Color(1f, .70f, .06f));
        Material eggDiamond = Mat("EggDiamond", new Color(.72f, .42f, 1f));
        Material trunk = Mat("TreeTrunk", new Color(.32f, .18f, .08f));
        Material leaves = Mat("TreeLeaves", new Color(.12f, .34f, .16f));
        Material woodland = Mat("WoodlandGround", new Color(.16f, .34f, .18f));
        Material lakeside = Mat("LakesideGround", new Color(.70f, .60f, .36f));
        Material ruins = Mat("AncientGround", new Color(.36f, .39f, .43f));
        Material crystal = Mat("RegionCrystal", new Color(.22f, .76f, .95f));
        Material flower = Mat("Wildflower", new Color(1f, .43f, .72f));

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Arena Ground"; ground.transform.localScale = new Vector3(16f, 1f, 11f);
        ground.GetComponent<Renderer>().sharedMaterial = grass;
        GroundPatch("WOODLAND GROVE", new Vector3(-11.5f, .008f, 0f), new Vector3(3.3f, 1f, 10.9f), woodland);
        GroundPatch("SUNLIT LAKESIDE", new Vector3(20.5f, .009f, 0f), new Vector3(3.1f, 1f, 10.9f), lakeside);
        GroundPatch("ANCIENT RUINS", new Vector3(58f, .010f, 0f), new Vector3(4.4f, 1f, 10.9f), ruins);
        Cube("Grand Stone Road", new Vector3(0f, .045f, 0f), new Vector3(148f, .07f, 8.5f), path);
        Cube("North Perimeter Wall", new Vector3(0f, 1.1f, 54.5f), new Vector3(160f, 2.2f, .8f), fence);
        Cube("South Perimeter Wall", new Vector3(0f, 1.1f, -54.5f), new Vector3(160f, 2.2f, .8f), fence);
        Cube("West Perimeter Wall", new Vector3(-79.5f, 1.1f, 0f), new Vector3(.8f, 2.2f, 110f), fence);
        Cube("East Perimeter Wall", new Vector3(79.5f, 1.1f, 0f), new Vector3(.8f, 2.2f, 110f), fence);

        for (int x = -72; x <= 72; x += 12)
        {
            Tree(new Vector3(x, 0f, -44f), trunk, leaves, Random.Range(.8f, 1.25f));
            Tree(new Vector3(x, 0f, 44f), trunk, leaves, Random.Range(.8f, 1.25f));
        }
        for (int i = 0; i < 46; i++)
        {
            float x = Random.Range(-73f, 73f);
            float z = Random.Range(-39f, 39f);
            if (Mathf.Abs(z) < 11f || Mathf.Abs(x + 65f) < 12f || Mathf.Abs(x - 65f) < 12f ||
                Mathf.Abs(x + 28f) < 4.5f || Mathf.Abs(x - 5f) < 4.5f || Mathf.Abs(x - 36f) < 4.5f)
                continue;
            Material foliage = x < 5f ? leaves : x < 36f ? eggRare : ruins;
            Tree(new Vector3(x, 0f, z), trunk, foliage, Random.Range(.65f, 1.15f));
            if (i % 3 == 0) Rock(new Vector3(x + Random.Range(-2f, 2f), .15f, z + Random.Range(-2f, 2f)), i % 2 == 0 ? crystal : fence, Random.Range(.5f, 1.25f));
            if (i % 4 == 0) FlowerPatch(new Vector3(x + 2f, 0f, z - 1.5f), flower);
        }

        Transform blueBase = Base("BLUE HOME BASE", new Vector3(-65f, .12f, 0f), blue);
        Transform orangeBase = Base("ORANGE RIVAL BASE", new Vector3(65f, .12f, 0f), orange);
        GameObject player = Avatar("Player", new Vector3(-62f, 0f, 0f), blue, pants, skin, hair, shoes, face);
        player.AddComponent<StealEggsPlayerController>();
        TrailRenderer dashTrail = player.AddComponent<TrailRenderer>();
        dashTrail.time = .14f; dashTrail.startWidth = .35f; dashTrail.endWidth = .01f;
        Shader trailShader = Shader.Find("Sprites/Default");
        if (trailShader != null) dashTrail.material = new Material(trailShader);
        dashTrail.startColor = new Color(.2f, .8f, 1f, .8f); dashTrail.endColor = new Color(.2f, .8f, 1f, 0f);
        GameObject rival = Avatar("Rival Bot", new Vector3(62f, 0f, 0f), orange, pants, skin, hair, shoes, face);
        rival.AddComponent<StealEggsRivalAI>().rivalBase = orangeBase;

        // Train speed on a treadmill to unlock the next zones.
        Cube("PREMIUM SPEED TRAINING TREADMILL", new Vector3(-64f, .18f, -14f), new Vector3(10f, .36f, 6f), eggRare);
        for (int i = 0; i < 7; i++)
            Cube("Treadmill Belt Stripe", new Vector3(-68f + i * 1.3f, .39f, -14f), new Vector3(.12f, .04f, 5.2f), fence);
        GameObject trainingTrigger = new GameObject("Training Area Trigger");
        trainingTrigger.transform.position = new Vector3(-64f, 1.4f, -14f);
        BoxCollider trainingCollider = trainingTrigger.AddComponent<BoxCollider>();
        trainingCollider.isTrigger = true;
        trainingCollider.size = new Vector3(9.6f, 2.8f, 5.7f);
        trainingTrigger.AddComponent<StealEggsTrainingPad>();
        WorldLabel("SPEED TRAINING ZONE", new Vector3(-64f, 3f, -17.5f), 180f);

        MakeSpeedGate("WOODLAND GATE", -28f, 10f, "Woodland", fence);
        MakeSpeedGate("LAKE GATE", 5f, 35f, "Lake", fence);
        MakeSpeedGate("ANCIENT GATE", 36f, 80f, "Ancient", fence);

        StealEggsGuardianAI[] guardians = new StealEggsGuardianAI[4];
        Vector3[] guardPositions = { new Vector3(-47f,0f,16f), new Vector3(-12f,0f,-18f), new Vector3(22f,0f,16f), new Vector3(57f,0f,-18f) };
        string[] guardNames = { "Mossback Guardian", "Tideclaw Guardian", "Dunebreak Guardian", "Frostmane Guardian" };
        Material[] guardColors = { leaves, eggRare, eggGold, eggDiamond };
        for (int i = 0; i < guardians.Length; i++)
        {
            GameObject guard = Avatar(guardNames[i], guardPositions[i], guardColors[i], pants, skin, hair, shoes, face);
            guardians[i] = guard.AddComponent<StealEggsGuardianAI>();
            guardians[i].guardianName = guardNames[i];
            guardians[i].playerTarget = player.transform;
            guardians[i].nest = guard.transform;
            guardians[i].chaseSpeed = 4.8f + i * .35f;
        }

        GameObject petPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        petPlatform.name = "PET COLLECTION DISPLAY";
        petPlatform.transform.position = new Vector3(-65f, .10f, 16f);
        petPlatform.transform.localScale = new Vector3(10f, .12f, 7f);
        petPlatform.GetComponent<Renderer>().sharedMaterial = eggGold;
        GameObject petShowcase = new GameObject("Pet Showcase Point");
        petShowcase.transform.position = new Vector3(-65f, .25f, 16f);
        WorldLabel("YOUR PET COLLECTION", new Vector3(-65f, 2.4f, 20f), 180f);

        Vector3[] positions = {
            new Vector3(-75f,.48f,16f), new Vector3(-72f,.48f,-25f), new Vector3(-65f,.48f,29f),
            new Vector3(-59f,.48f,-8f), new Vector3(-51f,.48f,-29f), new Vector3(-45f,.48f,24f),
            new Vector3(-35f,.48f,1f), new Vector3(-24f,.48f,18f), new Vector3(-19f,.48f,-24f),
            new Vector3(-13f,.48f,1f), new Vector3(-7f,.48f,31f), new Vector3(-2f,.48f,-32f),
            new Vector3(7f,.48f,15f), new Vector3(12f,.48f,-18f), new Vector3(17f,.48f,29f),
            new Vector3(23f,.48f,-7f), new Vector3(29f,.48f,20f), new Vector3(33f,.48f,-31f),
            new Vector3(41f,.48f,-18f), new Vector3(45f,.48f,25f), new Vector3(51f,.48f,-30f),
            new Vector3(56f,.48f,8f), new Vector3(60f,.48f,-18f), new Vector3(64f,.48f,26f),
            new Vector3(70f,.48f,1f), new Vector3(62f,.48f,-4f), new Vector3(67f,.48f,7f),
            new Vector3(72f,.48f,-9f), new Vector3(53f,.48f,36f), new Vector3(73f,.48f,-35f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            bool onRivalSide = positions[i].x >= 58f;
            float roll = Random.value;
            StealEggRarity rarity = roll < .03f ? StealEggRarity.Diamond : roll < .13f ? StealEggRarity.Golden : roll < .35f ? StealEggRarity.Rare : StealEggRarity.Common;
            Material eggMat = rarity == StealEggRarity.Diamond ? eggDiamond : rarity == StealEggRarity.Golden ? eggGold : rarity == StealEggRarity.Rare ? eggRare : eggCommon;
            int coinValue = rarity == StealEggRarity.Diamond ? 100 : rarity == StealEggRarity.Golden ? 30 : rarity == StealEggRarity.Rare ? 12 : 5;
            Egg("Egg " + (i + 1) + " - " + rarity, positions[i], onRivalSide, eggMat, rarity, coinValue);
        }

        GameObject managerObj = new GameObject("Steal Eggs Game Manager");
        StealEggsGameManager manager = managerObj.AddComponent<StealEggsGameManager>();
        manager.player = player.transform;
        manager.playerHomeBase = blueBase; manager.rivalHomeBase = orangeBase;
        manager.petShowcase = petShowcase.transform;
        manager.guardians = guardians;
        BuildHUD(manager); BuildCamera(player.transform); BuildLight(); BuildEventSystem();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        bool saved = EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        if (saved)
        {
            EditorUtility.DisplayDialog("Steal Eggs", "Enhanced third-person demo created!\n\nW/A/S/D or arrows: move\nRight mouse + drag: rotate camera\nSPACE: jump | SHIFT: sprint | Q: dash\nE: collect an egg\nBring eggs to BLUE base to earn coins\nEggs respawn; rare eggs pay more\nUse shop buttons to upgrade your character.\nProgress is saved locally.", "OK");
            Debug.Log("Steal Eggs demo created at " + ScenePath);
        }
    }

    private static void MakeSpeedGate(string gateName, float x, float requiredSpeed, string region, Material gateMaterial)
    {
        GameObject gate = Cube(gateName + " - NEED SPEED " + requiredSpeed, new Vector3(x, 1.65f, 0f), new Vector3(.9f, 3.3f, 108f), gateMaterial);
        gate.GetComponent<Renderer>().material.color = new Color(1f, .14f, .12f);
        StealEggsSpeedGate script = gate.AddComponent<StealEggsSpeedGate>();
        script.requiredSpeed = requiredSpeed;
        script.regionName = region;
        WorldLabel("REQUIRES SPEED " + requiredSpeed, new Vector3(x, 4.1f, 0f), 90f);
    }

    private static void WorldLabel(string label, Vector3 position, float rotationY)
    {
        GameObject obj = new GameObject("Sign - " + label);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        TextMesh text = obj.AddComponent<TextMesh>();
        text.text = label; text.fontSize = 64; text.characterSize = .06f;
        text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = Color.white;
    }

    private static GameObject Avatar(string name, Vector3 position, Material shirt, Material pants, Material skin, Material hair, Material shoes, Material face)
    {
        GameObject root = new GameObject(name); root.transform.position = position;
        CharacterController cc = root.AddComponent<CharacterController>(); cc.height = 2.35f; cc.radius = .38f; cc.center = new Vector3(0f, 1.16f, 0f); cc.stepOffset = .3f;
        Transform torso = Part("Torso", root.transform, new Vector3(0f,1.39f,0f), new Vector3(.78f,.80f,.42f), shirt);
        Transform head = Part("Head", root.transform, new Vector3(0f,2.02f,0f), new Vector3(.62f,.62f,.62f), skin);
        Part("Hair", head, new Vector3(0f,.39f,-.01f), new Vector3(1.08f,.28f,1.08f), hair);
        Part("Left Eye", head, new Vector3(-.24f,.04f,.51f), new Vector3(.12f,.14f,.04f), face);
        Part("Right Eye", head, new Vector3(.24f,.04f,.51f), new Vector3(.12f,.14f,.04f), face);
        Part("Smile", head, new Vector3(0f,-.21f,.51f), new Vector3(.25f,.04f,.04f), face);
        Transform la = Pivot("Left Arm Pivot", root.transform, new Vector3(-.51f,1.68f,0f)); Part("Left Arm", la, new Vector3(0f,-.34f,0f), new Vector3(.28f,.69f,.32f), skin);
        Transform ra = Pivot("Right Arm Pivot", root.transform, new Vector3(.51f,1.68f,0f)); Part("Right Arm", ra, new Vector3(0f,-.34f,0f), new Vector3(.28f,.69f,.32f), skin);
        Transform ll = Pivot("Left Leg Pivot", root.transform, new Vector3(-.20f,.88f,0f)); Part("Left Leg", ll, new Vector3(0f,-.32f,0f), new Vector3(.32f,.62f,.35f), pants); Part("Left Shoe", ll, new Vector3(0f,-.70f,.075f), new Vector3(.36f,.18f,.46f), shoes);
        Transform rl = Pivot("Right Leg Pivot", root.transform, new Vector3(.20f,.88f,0f)); Part("Right Leg", rl, new Vector3(0f,-.32f,0f), new Vector3(.32f,.62f,.35f), pants); Part("Right Shoe", rl, new Vector3(0f,-.70f,.075f), new Vector3(.36f,.18f,.46f), shoes);
        StealEggsCharacterAnimation anim = root.AddComponent<StealEggsCharacterAnimation>();
        anim.leftArmPivot=la; anim.rightArmPivot=ra; anim.leftLegPivot=ll; anim.rightLegPivot=rl; anim.torso=torso; anim.head=head; anim.CacheRestPose();
        return root;
    }
    private static Transform Part(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(parent,false); obj.transform.localPosition=pos; obj.transform.localScale=scale; obj.GetComponent<Renderer>().sharedMaterial=mat;
        Collider c=obj.GetComponent<Collider>(); if(c!=null)Object.DestroyImmediate(c); return obj.transform;
    }
    private static Transform Pivot(string name, Transform parent, Vector3 pos)
    {
        GameObject obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=pos;return obj.transform;
    }
    private static void Egg(string name, Vector3 pos, bool rivalSide, Material mat, StealEggRarity rarity, int value)
    {
        GameObject obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);obj.name=name;obj.transform.position=pos;obj.transform.localScale=new Vector3(.65f,.82f,.65f);obj.GetComponent<Renderer>().sharedMaterial=mat;
        EggPickup egg=obj.AddComponent<EggPickup>();egg.spawnedAtRivalBase=rivalSide;egg.SetRarity(rarity,value);egg.respawnSeconds=Random.Range(8f,13f);
    }
    private static Transform Base(string name, Vector3 pos, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(10f, .18f, 10f);
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = name + " Inner Ring"; marker.transform.position = pos + new Vector3(0f, .20f, 0f);
        marker.transform.localScale = new Vector3(7.2f, .035f, 7.2f);
        marker.GetComponent<Renderer>().sharedMaterial = mat;
        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null) Object.DestroyImmediate(markerCollider);
        return obj.transform;
    }
    private static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.position=pos;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=mat;return obj;
    }
    private static void GroundPatch(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
        patch.name = name; patch.transform.position = position; patch.transform.localScale = scale;
        patch.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = patch.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static void Tree(Vector3 pos, Material trunkMat, Material leafMat, float scale = 1f)
    {
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Tree Trunk";
        trunk.transform.position = pos + new Vector3(0f, .85f * scale, 0f);
        trunk.transform.localScale = new Vector3(.45f * scale, .85f * scale, .45f * scale);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Tree Crown";
        crown.transform.position = pos + new Vector3(0f, 2.1f * scale, 0f);
        crown.transform.localScale = new Vector3(2.1f * scale, 2.2f * scale, 2.1f * scale);
        crown.GetComponent<Renderer>().sharedMaterial = leafMat;
    }

    private static void Rock(Vector3 pos, Material material, float scale)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Decorative Boulder";
        rock.transform.position = pos;
        rock.transform.localScale = new Vector3(scale * 1.5f, scale, scale * 1.2f);
        rock.transform.rotation = Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f));
        rock.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void FlowerPatch(Vector3 pos, Material material)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1.2f, 1.2f), .12f, Random.Range(-1.2f, 1.2f));
            GameObject bloom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bloom.name = "Wildflower";
            bloom.transform.position = pos + offset;
            bloom.transform.localScale = new Vector3(.16f, .16f, .16f);
            bloom.GetComponent<Renderer>().sharedMaterial = material;
            Collider col = bloom.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }
    }
    private static void BuildCamera(Transform target)
    {
        GameObject obj = new GameObject("Main Camera");
        obj.tag = "MainCamera";
        Camera cam = obj.AddComponent<Camera>();
        cam.nearClipPlane = .1f; cam.farClipPlane = 360f; cam.fieldOfView = 64f;
        cam.allowHDR = true; cam.allowMSAA = true;
        obj.AddComponent<AudioListener>();
        StealEggsThirdPersonCamera orbit = obj.AddComponent<StealEggsThirdPersonCamera>();
        orbit.target = target; orbit.distance = 9.2f; orbit.focusOffset = new Vector3(0f, 1.55f, 0f);
        obj.transform.position = target.position + new Vector3(0f, 4.5f, -8.5f);
        obj.transform.LookAt(target.position + new Vector3(0f, 1.55f, 0f));
    }
    private static void BuildLight()
    {
        // Apply the highest available built-in quality level; actual maximum still depends on PC/GPU and URP assets.
        if (QualitySettings.names.Length > 0) QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
        QualitySettings.antiAliasing = 4;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowDistance = 180f;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.lodBias = 2f;
        GameObject obj = new GameObject("Sun Light");
        Light light = obj.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.45f; light.shadows = LightShadows.Soft;
        light.shadowStrength = .82f; light.shadowResolution = LightShadowResolution.VeryHigh;
        light.bounceIntensity = 1.1f; light.color = new Color(1f, .94f, .82f);
        obj.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.54f, .69f, .86f);
        RenderSettings.ambientEquatorColor = new Color(.42f, .48f, .39f);
        RenderSettings.ambientGroundColor = new Color(.20f, .19f, .16f);
        RenderSettings.ambientIntensity = 1.15f;
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(.65f, .76f, .83f); RenderSettings.fogDensity = .0008f;
    }
    private static void BuildEventSystem()
    {
        if(Object.FindFirstObjectByType<EventSystem>()!=null)return;
        GameObject obj=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));obj.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
    private static void BuildHUD(StealEggsGameManager manager)
    {
        GameObject canvasObj=new GameObject("HUD Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));Canvas canvas=canvasObj.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=canvasObj.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280f,720f);scaler.matchWidthOrHeight=.5f;
        GameObject panel=new GameObject("HUD Panel",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvasObj.transform,false);RectTransform pr=panel.GetComponent<RectTransform>();pr.anchorMin=new Vector2(0f,1f);pr.anchorMax=new Vector2(0f,1f);pr.pivot=new Vector2(0f,1f);pr.anchoredPosition=new Vector2(18f,-18f);pr.sizeDelta=new Vector2(450f,145f);panel.GetComponent<Image>().color=new Color(.04f,.05f,.07f,.8f);
        Text hud=TextUI(panel.transform,"Score Text",new Vector2(12f,-10f),new Vector2(428f,125f),19,TextAnchor.UpperLeft);hud.color=Color.white;hud.fontStyle=FontStyle.Bold;manager.hudText=hud;
        Text message=TextUI(canvasObj.transform,"Message Text",Vector2.zero,new Vector2(950f,52f),20,TextAnchor.MiddleCenter);RectTransform mr=message.GetComponent<RectTransform>();mr.anchorMin=new Vector2(.5f,0f);mr.anchorMax=new Vector2(.5f,0f);mr.pivot=new Vector2(.5f,0f);mr.anchoredPosition=new Vector2(0f,18f);message.color=Color.white;message.fontStyle=FontStyle.Bold;manager.messageText=message;
        GameObject mp=new GameObject("Mission Panel",typeof(RectTransform),typeof(Image));mp.transform.SetParent(canvasObj.transform,false);RectTransform mpr=mp.GetComponent<RectTransform>();mpr.anchorMin=Vector2.one;mpr.anchorMax=Vector2.one;mpr.pivot=Vector2.one;mpr.anchoredPosition=new Vector2(-18f,-18f);mpr.sizeDelta=new Vector2(390f,112f);mp.GetComponent<Image>().color=new Color(.04f,.05f,.07f,.8f);
        Text missions=TextUI(mp.transform,"Mission Text",new Vector2(10f,-8f),new Vector2(370f,96f),15,TextAnchor.UpperLeft);missions.color=Color.white;manager.missionText=missions;
        manager.hatchButtonText=ShopButton(canvasObj.transform,"Hatch Egg",new Vector2(18f,242f),StealEggsShopButton.ActionType.HatchEgg);
        manager.speedButtonText=ShopButton(canvasObj.transform,"Speed Upgrade",new Vector2(18f,196f),StealEggsShopButton.ActionType.BuySpeed);
        manager.reachButtonText=ShopButton(canvasObj.transform,"Pickup Range Upgrade",new Vector2(18f,150f),StealEggsShopButton.ActionType.BuyReach);
        manager.bagButtonText=ShopButton(canvasObj.transform,"Bag Upgrade",new Vector2(18f,104f),StealEggsShopButton.ActionType.BuyBag);
        manager.dashButtonText=ShopButton(canvasObj.transform,"Dash Upgrade",new Vector2(18f,58f),StealEggsShopButton.ActionType.BuyDash);
        Text controls=TextUI(canvasObj.transform,"Controls Text",Vector2.zero,new Vector2(395f,104f),15,TextAnchor.LowerRight);RectTransform cr=controls.GetComponent<RectTransform>();cr.anchorMin=new Vector2(1f,0f);cr.anchorMax=new Vector2(1f,0f);cr.pivot=new Vector2(1f,0f);cr.anchoredPosition=new Vector2(-18f,18f);controls.text="MOVE: WASD / ARROWS\nCAMERA: RIGHT MOUSE + DRAG\nJUMP: SPACE | SPRINT: SHIFT\nDASH: Q | PICK UP: E";controls.color=Color.white;
    }
    private static Text ShopButton(Transform parent,string name,Vector2 pos,StealEggsShopButton.ActionType actionType)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));obj.transform.SetParent(parent,false);RectTransform r=obj.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.zero;r.pivot=Vector2.zero;r.anchoredPosition=pos;r.sizeDelta=new Vector2(260f,40f);
        Image image=obj.GetComponent<Image>();image.color=new Color(.08f,.20f,.34f,.96f);Button button=obj.GetComponent<Button>();ColorBlock colors=button.colors;colors.highlightedColor=new Color(.16f,.42f,.66f,1f);colors.pressedColor=new Color(.04f,.13f,.24f,1f);button.colors=colors;StealEggsShopButton runtimeAction=obj.AddComponent<StealEggsShopButton>();runtimeAction.actionType=actionType;
        Text label=TextUI(obj.transform,"Label",Vector2.zero,new Vector2(250f,36f),14,TextAnchor.MiddleCenter);RectTransform lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(5f,2f);lr.offsetMax=new Vector2(-5f,-2f);label.color=Color.white;label.fontStyle=FontStyle.Bold;return label;
    }
    private static Text TextUI(Transform parent,string name,Vector2 pos,Vector2 size,int fontSize,TextAnchor alignment)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Text));obj.transform.SetParent(parent,false);RectTransform rect=obj.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0f,1f);rect.anchorMax=new Vector2(0f,1f);rect.pivot=new Vector2(0f,1f);rect.anchoredPosition=pos;rect.sizeDelta=size;
        Text text=obj.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");if(text.font==null)text.font=Font.CreateDynamicFontFromOSFont("Arial",fontSize);text.fontSize=fontSize;text.alignment=alignment;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;text.text="";return text;
    }
    private static Material Mat(string name,Color color)
    {
        string path = MaterialsPath + "/" + name + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) { existing.color = color; TuneMaterial(existing, name); EditorUtility.SetDirty(existing); return existing; }
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material m = new Material(shader); m.name = name; m.color = color; TuneMaterial(m, name);
        AssetDatabase.CreateAsset(m, path); return m;
    }
    private static void TuneMaterial(Material material, string materialName)
    {
        bool premium = materialName.Contains("Gold") || materialName.Contains("Diamond") ||
                      materialName.Contains("Rare") || materialName.Contains("Crystal");
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", premium ? .42f : .04f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", premium ? .78f : .34f);
        if (materialName.Contains("Gold") && material.HasProperty("_Metallic")) material.SetFloat("_Metallic", .72f);
    }

    private static void EnsureFolder(string folder)
    {
        if(AssetDatabase.IsValidFolder(folder))return;string parent=Path.GetDirectoryName(folder).Replace("\\","/");string name=Path.GetFileName(folder);if(!AssetDatabase.IsValidFolder(parent))EnsureFolder(parent);AssetDatabase.CreateFolder(parent,name);
    }
}
#endif
