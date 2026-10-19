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
using System.Collections.Generic;

public static class StealEggsDemoSetup
{
    private const string Root = "Assets/StealEggs";
    private const string ScenePath = Root + "/Scenes/StealEggsDemo.unity";
    private const string MaterialsPath = Root + "/Materials";
    private const string TexturesPath = Root + "/Textures";
    private static readonly Dictionary<string, List<GameObject>> NaturePrefabCache =
        new Dictionary<string, List<GameObject>>();

    [MenuItem("Steal Eggs/Build Demo Arena")]
    public static void BuildDemoArena()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        NaturePrefabCache.Clear();
        EnsureFolder(Root + "/Scenes"); EnsureFolder(MaterialsPath); EnsureFolder(TexturesPath);
        EnsureURPActive();
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

        // Procedural surface maps add natural color breakup without external packages.
        ApplyNaturalTexture(grass, "GrassField", new Color(.055f, .15f, .065f), new Color(.22f, .34f, .12f), new Vector2(32f, 24f), false);
        ApplyNaturalTexture(path, "WornStonePath", new Color(.22f, .22f, .20f), new Color(.48f, .45f, .39f), new Vector2(34f, 3f), false);
        ApplyNaturalTexture(trunk, "BarkGrooves", new Color(.09f, .035f, .015f), new Color(.34f, .17f, .065f), new Vector2(2f, 1f), true);
        ApplyNaturalTexture(woodland, "ForestSoil", new Color(.045f, .13f, .045f), new Color(.19f, .30f, .095f), new Vector2(12f, 9f), false);
        ApplyNaturalTexture(lakeside, "DryLakesideSand", new Color(.34f, .25f, .12f), new Color(.76f, .65f, .39f), new Vector2(14f, 8f), false);
        ApplyNaturalTexture(ruins, "AncientWeatheredStone", new Color(.16f, .17f, .17f), new Color(.48f, .47f, .43f), new Vector2(16f, 8f), false);

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

        // The dense outer forest is placed after eggs spawn, so tree trunks avoid egg pickup points.
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

        if (!CreateNaturalGroundCover()) CreateGrassDetails();

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

        CreateDenseForest(trunk, leaves);

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
        GameObject root = new GameObject(name);
        root.transform.position = position;

        CharacterController cc = root.AddComponent<CharacterController>();
        cc.height = 2.30f;
        cc.radius = .31f;
        cc.center = new Vector3(0f, 1.15f, 0f);
        cc.stepOffset = .28f;

        Material trim = Mat("ClothingTrim_" + name.Replace(" ", "_"), Color.Lerp(shirt.color, new Color(.025f, .035f, .045f), .40f));
        Material highlight = Mat("ClothingHighlight_" + name.Replace(" ", "_"), Color.Lerp(shirt.color, Color.white, .24f));
        Material eyeWhite = Mat("EyeWhite", new Color(.92f, .91f, .83f));
        Material sole = Mat("ShoeSole", new Color(.035f, .04f, .045f));
        Material buckle = Mat("BeltBuckle_" + name.Replace(" ", "_"), new Color(.74f, .56f, .22f));

        // Rounded torso, neck and shaped head replace the old cuboid mannequin.
        Transform torso = ShapePart("Torso", root.transform, new Vector3(0f, 1.42f, 0f), new Vector3(.55f, .43f, .38f), shirt, PrimitiveType.Capsule);
        ShapePart("Neck", root.transform, new Vector3(0f, 1.87f, .005f), new Vector3(.105f, .13f, .105f), skin, PrimitiveType.Cylinder);
        Transform head = ShapePart("Head", root.transform, new Vector3(0f, 2.16f, .015f), new Vector3(.51f, .63f, .45f), skin, PrimitiveType.Sphere);

        // Layered hair clumps give characters different silhouettes.
        int hairstyle = Mathf.Abs(name.GetHashCode()) % 3;
        ShapePart("Hair Cap", root.transform, new Vector3(0f, 2.405f, -.005f), new Vector3(.54f, .24f, .48f), hair, PrimitiveType.Sphere);
        ShapePart("Hair Fringe L", root.transform, new Vector3(-.115f, 2.355f, .177f), new Vector3(.105f, .10f, .09f), hair, PrimitiveType.Sphere);
        ShapePart("Hair Fringe R", root.transform, new Vector3(.06f, 2.365f, .19f), new Vector3(.12f, .085f, .075f), hair, PrimitiveType.Sphere);
        ShapePart("Side Hair L", root.transform, new Vector3(-.238f, 2.19f, -.005f), new Vector3(.065f, .18f, .12f), hair, PrimitiveType.Sphere);
        ShapePart("Side Hair R", root.transform, new Vector3(.238f, 2.19f, -.005f), new Vector3(.065f, .18f, .12f), hair, PrimitiveType.Sphere);
        if (hairstyle == 1)
        {
            ShapePart("Back Hair", root.transform, new Vector3(0f, 2.17f, -.19f), new Vector3(.46f, .34f, .24f), hair, PrimitiveType.Sphere);
            ShapePart("Long Fringe", root.transform, new Vector3(-.04f, 2.29f, .20f), new Vector3(.13f, .16f, .07f), hair, PrimitiveType.Sphere);
        }
        else if (hairstyle == 2)
        {
            ShapePart("Ponytail", root.transform, new Vector3(.12f, 2.20f, -.25f), new Vector3(.10f, .22f, .10f), hair, PrimitiveType.Sphere);
            ShapePart("Top Curl", root.transform, new Vector3(.045f, 2.48f, -.045f), new Vector3(.12f, .09f, .12f), hair, PrimitiveType.Sphere);
        }

        // Facial features are positioned on the front side (+Z) of the head.
        ShapePart("Left Ear", root.transform, new Vector3(-.254f, 2.15f, 0f), new Vector3(.065f, .085f, .06f), skin, PrimitiveType.Sphere);
        ShapePart("Right Ear", root.transform, new Vector3(.254f, 2.15f, 0f), new Vector3(.065f, .085f, .06f), skin, PrimitiveType.Sphere);
        ShapePart("Left Eye White", root.transform, new Vector3(-.092f, 2.175f, .218f), new Vector3(.050f, .064f, .025f), eyeWhite, PrimitiveType.Sphere);
        ShapePart("Right Eye White", root.transform, new Vector3(.092f, 2.175f, .218f), new Vector3(.050f, .064f, .025f), eyeWhite, PrimitiveType.Sphere);
        ShapePart("Left Pupil", root.transform, new Vector3(-.086f, 2.174f, .241f), new Vector3(.022f, .036f, .014f), face, PrimitiveType.Sphere);
        ShapePart("Right Pupil", root.transform, new Vector3(.098f, 2.174f, .241f), new Vector3(.022f, .036f, .014f), face, PrimitiveType.Sphere);
        ShapePart("Left Brow", root.transform, new Vector3(-.092f, 2.245f, .214f), new Vector3(.060f, .018f, .025f), hair, PrimitiveType.Cube);
        ShapePart("Right Brow", root.transform, new Vector3(.092f, 2.245f, .214f), new Vector3(.060f, .018f, .025f), hair, PrimitiveType.Cube);
        ShapePart("Nose", root.transform, new Vector3(0f, 2.115f, .246f), new Vector3(.038f, .050f, .035f), skin, PrimitiveType.Sphere);
        ShapePart("Mouth", root.transform, new Vector3(0f, 2.065f, .226f), new Vector3(.068f, .012f, .012f), face, PrimitiveType.Cube);

        // Clothing accents, belt, cuffs and pack make the body read as layered clothing.
        ShapePart("Shirt Collar", root.transform, new Vector3(0f, 1.825f, .13f), new Vector3(.12f, .055f, .045f), highlight, PrimitiveType.Sphere);
        ShapePart("Belt", root.transform, new Vector3(0f, 1.08f, 0f), new Vector3(.47f, .065f, .30f), trim, PrimitiveType.Cube);
        ShapePart("Belt Buckle", root.transform, new Vector3(0f, 1.08f, .154f), new Vector3(.075f, .055f, .018f), buckle, PrimitiveType.Cube);
        ShapePart("Backpack", root.transform, new Vector3(0f, 1.39f, -.235f), new Vector3(.25f, .30f, .13f), trim, PrimitiveType.Capsule);

        Transform la = Pivot("Left Arm Pivot", root.transform, new Vector3(-.365f, 1.68f, 0f));
        Transform ra = Pivot("Right Arm Pivot", root.transform, new Vector3(.365f, 1.68f, 0f));
        ShapePart("Left Shoulder", la, Vector3.zero, new Vector3(.17f, .17f, .17f), shirt, PrimitiveType.Sphere);
        ShapePart("Right Shoulder", ra, Vector3.zero, new Vector3(.17f, .17f, .17f), shirt, PrimitiveType.Sphere);
        ShapePart("Left Upper Arm", la, new Vector3(0f, -.19f, 0f), new Vector3(.125f, .225f, .125f), shirt, PrimitiveType.Capsule);
        ShapePart("Right Upper Arm", ra, new Vector3(0f, -.19f, 0f), new Vector3(.125f, .225f, .125f), shirt, PrimitiveType.Capsule);
        ShapePart("Left Forearm", la, new Vector3(0f, -.445f, .005f), new Vector3(.105f, .20f, .105f), skin, PrimitiveType.Capsule);
        ShapePart("Right Forearm", ra, new Vector3(0f, -.445f, .005f), new Vector3(.105f, .20f, .105f), skin, PrimitiveType.Capsule);
        ShapePart("Left Cuff", la, new Vector3(0f, -.34f, .005f), new Vector3(.11f, .055f, .11f), highlight, PrimitiveType.Cylinder);
        ShapePart("Right Cuff", ra, new Vector3(0f, -.34f, .005f), new Vector3(.11f, .055f, .11f), highlight, PrimitiveType.Cylinder);
        ShapePart("Left Hand", la, new Vector3(0f, -.625f, .025f), new Vector3(.105f, .12f, .09f), skin, PrimitiveType.Sphere);
        ShapePart("Right Hand", ra, new Vector3(0f, -.625f, .025f), new Vector3(.105f, .12f, .09f), skin, PrimitiveType.Sphere);

        Transform ll = Pivot("Left Leg Pivot", root.transform, new Vector3(-.17f, .98f, 0f));
        Transform rl = Pivot("Right Leg Pivot", root.transform, new Vector3(.17f, .98f, 0f));
        ShapePart("Left Thigh", ll, new Vector3(0f, -.19f, 0f), new Vector3(.15f, .225f, .15f), pants, PrimitiveType.Capsule);
        ShapePart("Right Thigh", rl, new Vector3(0f, -.19f, 0f), new Vector3(.15f, .225f, .15f), pants, PrimitiveType.Capsule);
        ShapePart("Left Shin", ll, new Vector3(0f, -.55f, .005f), new Vector3(.12f, .30f, .125f), pants, PrimitiveType.Capsule);
        ShapePart("Right Shin", rl, new Vector3(0f, -.55f, .005f), new Vector3(.12f, .30f, .125f), pants, PrimitiveType.Capsule);
        ShapePart("Left Shoe Sole", ll, new Vector3(0f, -.92f, .055f), new Vector3(.18f, .06f, .27f), sole, PrimitiveType.Sphere);
        ShapePart("Right Shoe Sole", rl, new Vector3(0f, -.92f, .055f), new Vector3(.18f, .06f, .27f), sole, PrimitiveType.Sphere);
        ShapePart("Left Shoe", ll, new Vector3(0f, -.88f, .075f), new Vector3(.17f, .105f, .245f), shoes, PrimitiveType.Sphere);
        ShapePart("Right Shoe", rl, new Vector3(0f, -.88f, .075f), new Vector3(.17f, .105f, .245f), shoes, PrimitiveType.Sphere);

        StealEggsCharacterAnimation anim = root.AddComponent<StealEggsCharacterAnimation>();
        anim.leftArmPivot = la; anim.rightArmPivot = ra;
        anim.leftLegPivot = ll; anim.rightLegPivot = rl;
        anim.torso = torso; anim.head = head; anim.CacheRestPose();
        return root;
    }

    private static Transform ShapePart(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, PrimitiveType type)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        return obj.transform;
    }

    private static Transform Part(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        return ShapePart(name, parent, pos, scale, mat,
            name == "Head" || name.Contains("Eye") || name.Contains("Smile") ? PrimitiveType.Sphere : PrimitiveType.Capsule);
    }

    private static Transform Pivot(string name, Transform parent, Vector3 pos)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = pos;
        return obj.transform;
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
        List<GameObject> treePrefabs = FindNaturePrefabs(
            "tree", "oak", "pine", "fir", "birch", "maple", "palm", "spruce", "willow", "broadleaf", "redwood", "cedar", "cypress", "eurya", "laurel", "bamboo", "cherryblossom", "planetree", "ginkgo", "japanesecedar", "japanesecypress", "sawtoothoak", "dawnredwood");
        if (treePrefabs.Count > 0 && TryPlaceNaturePrefab(treePrefabs, pos, Random.Range(10.5f, 14.5f) * scale))
            return;

        GameObject treeRoot = new GameObject("Tree");
        treeRoot.transform.position = pos;
        treeRoot.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        StealEggsFoliage.Build(treeRoot.transform, pos, trunkMat, leafMat, scale);
    }

    private static void CreateBranch(Transform parent, Vector3 start, Vector3 end, float thickness, Material material, string objectName)
    {
        Vector3 delta = end - start;
        GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        branch.name = objectName;
        branch.transform.SetParent(parent, false);
        branch.transform.localPosition = (start + end) * .5f;
        branch.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        branch.transform.localScale = new Vector3(thickness, delta.magnitude * .5f, thickness);
        branch.GetComponent<Renderer>().sharedMaterial = material;
        RemoveCollider(branch);
    }

    private static void RemoveCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
    }

    private static void Rock(Vector3 pos, Material material, float scale)
    {
        List<GameObject> rockPrefabs = FindNaturePrefabs("rock", "boulder", "stone", "pebble");
        if (rockPrefabs.Count > 0 && TryPlaceNaturePrefab(rockPrefabs, pos, Mathf.Max(.4f, scale * 1.1f)))
            return;

        GameObject root = new GameObject("Weathered Boulder Cluster");
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(Random.Range(-18f, 18f), Random.Range(0f, 360f), Random.Range(-16f, 16f));
        Material stoneLight = Mat("Stone_" + material.name + "_Light", Color.Lerp(material.color, new Color(.63f, .61f, .55f), .22f));
        Material stoneDark = Mat("Stone_" + material.name + "_Shadow", Color.Lerp(material.color, Color.black, .22f));
        int count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Irregular Rock";
            rock.transform.SetParent(root.transform, false);
            rock.transform.localPosition = new Vector3(Random.Range(-.28f, .28f), Random.Range(-.03f, .12f), Random.Range(-.22f, .22f)) * scale;
            rock.transform.localScale = new Vector3(scale * Random.Range(.85f, 1.45f), scale * Random.Range(.55f, .94f), scale * Random.Range(.75f, 1.3f));
            rock.transform.localRotation = Quaternion.Euler(Random.Range(-25f, 25f), Random.Range(0f, 360f), Random.Range(-22f, 22f));
            rock.GetComponent<Renderer>().sharedMaterial = i == 0 ? material : (i % 2 == 0 ? stoneLight : stoneDark);
            RemoveCollider(rock);
        }
    }

    private static void FlowerPatch(Vector3 pos, Material material)
    {
        Material stemMaterial = Mat("WildflowerStem", new Color(.08f, .22f, .055f));
        Material flowerCenter = Mat("WildflowerCenter", new Color(.92f, .68f, .13f));
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-1.2f, 1.2f), 0f, Random.Range(-1.2f, 1.2f));
            float height = Random.Range(.20f, .38f);
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Wildflower Stem";
            stem.transform.position = pos + offset + Vector3.up * (height * .48f);
            stem.transform.localScale = new Vector3(.018f, height * .5f, .018f);
            stem.GetComponent<Renderer>().sharedMaterial = stemMaterial;
            RemoveCollider(stem);

            Vector3 bloomCenter = pos + offset + Vector3.up * height;
            float size = Random.Range(.07f, .11f);
            for (int petal = 0; petal < 5; petal++)
            {
                float angle = petal * 72f;
                Vector3 petalOffset = Quaternion.Euler(0f, angle, 0f) * new Vector3(size * .68f, 0f, 0f);
                GameObject bloom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bloom.name = "Wildflower Petal";
                bloom.transform.position = bloomCenter + petalOffset;
                bloom.transform.localScale = new Vector3(size, size * .55f, size * .78f);
                bloom.GetComponent<Renderer>().sharedMaterial = material;
                RemoveCollider(bloom);
            }

            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.name = "Wildflower Center";
            center.transform.position = bloomCenter;
            center.transform.localScale = Vector3.one * .055f;
            center.GetComponent<Renderer>().sharedMaterial = flowerCenter;
            RemoveCollider(center);
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
        if (QualitySettings.names.Length > 0)
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);

        QualitySettings.antiAliasing = 4;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowDistance = 150f;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.lodBias = 1.8f;

        GameObject obj = new GameObject("Late Afternoon Sun");
        Light light = obj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.12f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = .72f;
        light.shadowResolution = LightShadowResolution.VeryHigh;
        light.bounceIntensity = 1.0f;
        light.color = new Color(1f, .88f, .73f);
        obj.transform.rotation = Quaternion.Euler(34f, -28f, 0f);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.43f, .59f, .76f);
        RenderSettings.ambientEquatorColor = new Color(.42f, .46f, .37f);
        RenderSettings.ambientGroundColor = new Color(.20f, .19f, .15f);
        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(.66f, .74f, .79f);
        RenderSettings.fogDensity = .00055f;

        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader != null)
        {
            string skyPath = MaterialsPath + "/NaturalOutdoorSky.mat";
            Material sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if (sky == null)
            {
                sky = new Material(skyShader);
                sky.name = "NaturalOutdoorSky";
                AssetDatabase.CreateAsset(sky, skyPath);
            }
            if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", .035f);
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", .9f);
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.15f);
            RenderSettings.skybox = sky;
        }

        DynamicGI.UpdateEnvironment();
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
        GameObject panel=new GameObject("HUD Panel",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvasObj.transform,false);RectTransform pr=panel.GetComponent<RectTransform>();pr.anchorMin=new Vector2(0f,1f);pr.anchorMax=new Vector2(0f,1f);pr.pivot=new Vector2(0f,1f);pr.anchoredPosition=new Vector2(18f,-18f);pr.sizeDelta=new Vector2(495f,164f);panel.GetComponent<Image>().color=new Color(.025f,.045f,.085f,.92f);
        Text hud=TextUI(panel.transform,"Score Text",new Vector2(14f,-12f),new Vector2(465f,135f),20,TextAnchor.UpperLeft);hud.color=Color.white;hud.fontStyle=FontStyle.Bold;manager.hudText=hud;
        Text message=TextUI(canvasObj.transform,"Message Text",Vector2.zero,new Vector2(950f,52f),20,TextAnchor.MiddleCenter);RectTransform mr=message.GetComponent<RectTransform>();mr.anchorMin=new Vector2(.5f,0f);mr.anchorMax=new Vector2(.5f,0f);mr.pivot=new Vector2(.5f,0f);mr.anchoredPosition=new Vector2(0f,18f);message.color=Color.white;message.fontStyle=FontStyle.Bold;manager.messageText=message;
        GameObject mp=new GameObject("Mission Panel",typeof(RectTransform),typeof(Image));mp.transform.SetParent(canvasObj.transform,false);RectTransform mpr=mp.GetComponent<RectTransform>();mpr.anchorMin=Vector2.one;mpr.anchorMax=Vector2.one;mpr.pivot=Vector2.one;mpr.anchoredPosition=new Vector2(-18f,-18f);mpr.sizeDelta=new Vector2(420f,126f);mp.GetComponent<Image>().color=new Color(.025f,.045f,.085f,.92f);
        Text missions=TextUI(mp.transform,"Mission Text",new Vector2(12f,-10f),new Vector2(396f,106f),16,TextAnchor.UpperLeft);missions.color=Color.white;missions.fontStyle=FontStyle.Bold;manager.missionText=missions;
        manager.hatchButtonText=ShopButton(canvasObj.transform,"Hatch Egg",new Vector2(18f,258f),StealEggsShopButton.ActionType.HatchEgg);
        manager.speedButtonText=ShopButton(canvasObj.transform,"Speed Upgrade",new Vector2(18f,208f),StealEggsShopButton.ActionType.BuySpeed);
        manager.reachButtonText=ShopButton(canvasObj.transform,"Pickup Range Upgrade",new Vector2(18f,158f),StealEggsShopButton.ActionType.BuyReach);
        manager.bagButtonText=ShopButton(canvasObj.transform,"Bag Upgrade",new Vector2(18f,108f),StealEggsShopButton.ActionType.BuyBag);
        manager.dashButtonText=ShopButton(canvasObj.transform,"Dash Upgrade",new Vector2(18f,58f),StealEggsShopButton.ActionType.BuyDash);
        Text controls=TextUI(canvasObj.transform,"Controls Text",Vector2.zero,new Vector2(395f,104f),15,TextAnchor.LowerRight);RectTransform cr=controls.GetComponent<RectTransform>();cr.anchorMin=new Vector2(1f,0f);cr.anchorMax=new Vector2(1f,0f);cr.pivot=new Vector2(1f,0f);cr.anchoredPosition=new Vector2(-18f,18f);controls.text="MOVE: WASD / ARROWS\nCAMERA: RIGHT MOUSE + DRAG\nJUMP: SPACE | SPRINT: SHIFT\nDASH: Q | PICK UP: E";controls.color=Color.white;
    }
    private static Text ShopButton(Transform parent,string name,Vector2 pos,StealEggsShopButton.ActionType actionType)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));obj.transform.SetParent(parent,false);RectTransform r=obj.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.zero;r.pivot=Vector2.zero;r.anchoredPosition=pos;r.sizeDelta=new Vector2(292f,44f);
        Image image=obj.GetComponent<Image>();
        switch (actionType)
        {
            case StealEggsShopButton.ActionType.HatchEgg: image.color = new Color(.48f,.30f,.05f,.98f); break;
            case StealEggsShopButton.ActionType.BuySpeed: image.color = new Color(.05f,.28f,.52f,.98f); break;
            case StealEggsShopButton.ActionType.BuyReach: image.color = new Color(.06f,.39f,.30f,.98f); break;
            case StealEggsShopButton.ActionType.BuyBag: image.color = new Color(.31f,.20f,.51f,.98f); break;
            default: image.color = new Color(.52f,.17f,.12f,.98f); break;
        }
        Button button=obj.GetComponent<Button>();ColorBlock colors=button.colors;colors.highlightedColor=new Color(.32f,.62f,.90f,1f);colors.pressedColor=new Color(.04f,.13f,.24f,1f);button.colors=colors;StealEggsShopButton runtimeAction=obj.AddComponent<StealEggsShopButton>();runtimeAction.actionType=actionType;
        Text label=TextUI(obj.transform,"Label",Vector2.zero,new Vector2(280f,40f),15,TextAnchor.MiddleCenter);RectTransform lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(5f,2f);lr.offsetMax=new Vector2(-5f,-2f);label.color=Color.white;label.fontStyle=FontStyle.Bold;return label;
    }
    private static Text TextUI(Transform parent,string name,Vector2 pos,Vector2 size,int fontSize,TextAnchor alignment)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Text));obj.transform.SetParent(parent,false);RectTransform rect=obj.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0f,1f);rect.anchorMax=new Vector2(0f,1f);rect.pivot=new Vector2(0f,1f);rect.anchoredPosition=pos;rect.sizeDelta=size;
        Text text=obj.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");if(text.font==null)text.font=Font.CreateDynamicFontFromOSFont("Arial",fontSize);text.fontSize=fontSize;text.alignment=alignment;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;text.text="";return text;
    }
    private static void EnsureURPActive()
    {
        // Activate the URP asset the user created, if one is present in the project.
        string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[] { "Assets" });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            RenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            if (pipeline == null) continue;

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
            Debug.Log("Steal Eggs: enabled URP pipeline from " + path);
            return;
        }

        Debug.LogWarning("Steal Eggs: no Universal Render Pipeline Asset found. Create one via Assets > Create > Rendering, then run Build Demo Arena again.");
    }

    private static List<GameObject> FindNaturePrefabs(params string[] keywords)
    {
        string cacheKey = string.Join("|", keywords);
        List<GameObject> cached;
        if (NaturePrefabCache.TryGetValue(cacheKey, out cached))
            return cached;

        List<GameObject> preferred = new List<GameObject>();
        List<GameObject> fallback = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string lowerPath = path.ToLowerInvariant();
            string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();

            if (lowerPath.Contains("/samples/") || lowerPath.Contains("/demo/") ||
                lowerPath.Contains("preview") || lowerPath.Contains("thumbnail") ||
                lowerPath.Contains("icon"))
                continue;

            bool matches = false;
            for (int k = 0; k < keywords.Length; k++)
            {
                if (fileName.Contains(keywords[k].ToLowerInvariant()))
                {
                    matches = true;
                    break;
                }
            }
            if (!matches) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool fromNaturePack =
                lowerPath.Contains("essential") || lowerPath.Contains("polyeler") ||
                lowerPath.Contains("nature") || lowerPath.Contains("environment");
            if (fromNaturePack) preferred.Add(prefab);
            else fallback.Add(prefab);
        }

        if (preferred.Count > 0) cached = preferred;
        else cached = fallback;
        NaturePrefabCache[cacheKey] = cached;
        return cached;
    }

    private static bool TryPlaceNaturePrefab(List<GameObject> candidates, Vector3 position, float desiredHeight)
    {
        if (candidates == null || candidates.Count == 0) return false;

        GameObject source = candidates[Random.Range(0, candidates.Count)];
        GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (instance == null) instance = Object.Instantiate(source);
        if (instance == null) return false;

        instance.name = source.name;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        Bounds bounds;
        if (!TryGetRendererBounds(instance, out bounds) || bounds.size.y < .001f)
        {
            Object.DestroyImmediate(instance);
            return false;
        }

        float factor = Mathf.Clamp(desiredHeight / bounds.size.y, .001f, 100f);
        instance.transform.localScale *= factor;

        if (TryGetRendererBounds(instance, out bounds))
            instance.transform.position += Vector3.up * (position.y - bounds.min.y);

        return true;
    }

    private static bool TryGetRendererBounds(GameObject obj, out Bounds bounds)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            bounds = new Bounds(obj.transform.position, Vector3.zero);
            return false;
        }

        bool initialized = false;
        bounds = new Bounds(obj.transform.position, Vector3.zero);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled) continue;
            if (!initialized)
            {
                bounds = renderers[i].bounds;
                initialized = true;
            }
            else bounds.Encapsulate(renderers[i].bounds);
        }
        return initialized;
    }

    private static void CreateDenseForest(Material trunkMat, Material leafMat)
    {
        // Six staggered rows create a dense, tall woodland backdrop while leaving the egg lanes playable.
        float[] rows = { -50.5f, -45.5f, -40.5f, 40.5f, 45.5f, 50.5f };
        EggPickup[] eggs = Object.FindObjectsByType<EggPickup>(FindObjectsSortMode.None);
        int placed = 0;
        const float spacing = 5.0f;

        for (int row = 0; row < rows.Length; row++)
        {
            float stagger = row % 2 == 0 ? 0f : spacing * 0.5f;
            for (float baseX = -76f + stagger; baseX <= 76f; baseX += spacing)
            {
                float x = Mathf.Clamp(baseX + Random.Range(-0.55f, 0.55f), -77f, 77f);
                float z = rows[row] + Random.Range(-0.55f, 0.55f);

                bool tooCloseToEgg = false;
                for (int i = 0; i < eggs.Length; i++)
                {
                    if (eggs[i] == null) continue;
                    Vector2 treePoint = new Vector2(x, z);
                    Vector2 eggPoint = new Vector2(eggs[i].transform.position.x, eggs[i].transform.position.z);
                    if (Vector2.Distance(treePoint, eggPoint) < 4.7f)
                    {
                        tooCloseToEgg = true;
                        break;
                    }
                }
                if (tooCloseToEgg) continue;

                // Vary height and rotation so the forest does not look like evenly repeated props.
                Tree(new Vector3(x, 0f, z), trunkMat, leafMat, Random.Range(1.0f, 1.48f));
                placed++;
            }
        }

        Debug.Log("Steal Eggs: placed " + placed + " tall trees for the dense woodland border.");
    }

    private static bool CreateNaturalGroundCover()
    {
        List<GameObject> plants = FindNaturePrefabs("weed", "grass", "fern", "bush", "shrub", "clover", "plant", "mugwort", "viola", "eurya", "laurel", "bamboo", "fallenleaves");
        if (plants.Count == 0) return false;

        int placed = 0;
        const int attempts = 260;
        for (int i = 0; i < attempts && placed < 125; i++)
        {
            float x = Random.Range(-76f, 76f);
            float z = Random.Range(-50f, 50f);
            if (Mathf.Abs(z) < 5.2f ||
                Vector2.Distance(new Vector2(x, z), new Vector2(-65f, 0f)) < 10f ||
                Vector2.Distance(new Vector2(x, z), new Vector2(65f, 0f)) < 10f ||
                (Mathf.Abs(x + 64f) < 6f && Mathf.Abs(z + 14f) < 4f) ||
                (Mathf.Abs(x + 28f) < 2.5f && Mathf.Abs(z) < 3.5f) ||
                (Mathf.Abs(x - 5f) < 2.5f && Mathf.Abs(z) < 3.5f) ||
                (Mathf.Abs(x - 36f) < 2.5f && Mathf.Abs(z) < 3.5f))
                continue;

            if (TryPlaceNaturePrefab(plants, new Vector3(x, .02f, z), Random.Range(.35f, 1.05f)))
                placed++;
        }

        Debug.Log("Steal Eggs: placed " + placed + " real ground-cover prefabs from the Nature Pack.");
        return placed > 0;
    }

    private static void ApplyNaturalTexture(Material material, string textureName, Color darkColor, Color lightColor, Vector2 tiling, bool verticalGrain)
    {
        Texture2D texture = CreateNaturalTexture(textureName, darkColor, lightColor, verticalGrain);
        material.mainTexture = texture;
        material.mainTextureScale = tiling;
        material.color = Color.white;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .22f);
        Texture2D normalMap = CreateNaturalNormalTexture(textureName, verticalGrain);
        if (material.HasProperty("_BumpMap") && normalMap != null)
        {
            material.SetTexture("_BumpMap", normalMap);
            material.EnableKeyword("_NORMALMAP");
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", verticalGrain ? .34f : .18f);
        }
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
    }

    private static Texture2D CreateNaturalTexture(string textureName, Color darkColor, Color lightColor, bool verticalGrain)
    {
        string assetPath = TexturesPath + "/" + textureName + ".asset";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (existing != null) return existing;

        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false, false);
        texture.name = textureName;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 8;

        float seed = textureName.Length * .173f;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n1 = verticalGrain
                    ? Mathf.PerlinNoise(x * .035f + seed, y * .008f + seed)
                    : Mathf.PerlinNoise(x * .025f + seed, y * .025f + seed * .7f);
                float n2 = verticalGrain
                    ? Mathf.PerlinNoise(x * .12f + seed * 2f, y * .045f + seed)
                    : Mathf.PerlinNoise(x * .09f + seed * 1.7f, y * .09f + seed * .3f);
                float noise = Mathf.Clamp01(n1 * .72f + n2 * .28f);
                float fleck = Mathf.Sin(x * 12.9898f + y * 78.233f + seed * 31.7f) * .035f;
                noise = Mathf.Clamp01(noise + fleck);
                pixels[y * size + x] = Color.Lerp(darkColor, lightColor, noise);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        AssetDatabase.CreateAsset(texture, assetPath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
    }

    private static Texture2D CreateNaturalNormalTexture(string textureName, bool verticalGrain)
    {
        string assetPath = TexturesPath + "/" + textureName + "_Normal.asset";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (existing != null) return existing;

        const int size = 256;
        float seed = textureName.Length * .173f;
        float[] heightMap = new float[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n1 = verticalGrain
                    ? Mathf.PerlinNoise(x * .035f + seed, y * .008f + seed)
                    : Mathf.PerlinNoise(x * .025f + seed, y * .025f + seed * .7f);
                float n2 = verticalGrain
                    ? Mathf.PerlinNoise(x * .12f + seed * 2f, y * .045f + seed)
                    : Mathf.PerlinNoise(x * .09f + seed * 1.7f, y * .09f + seed * .3f);
                heightMap[y * size + x] = n1 * .72f + n2 * .28f;
            }
        }

        Texture2D normalMap = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        normalMap.name = textureName + "_Normal";
        normalMap.wrapMode = TextureWrapMode.Repeat;
        normalMap.filterMode = FilterMode.Bilinear;
        normalMap.anisoLevel = 8;
        Color[] pixels = new Color[size * size];
        float strength = verticalGrain ? 5.0f : 2.1f;

        for (int y = 0; y < size; y++)
        {
            int up = (y + 1) % size;
            int down = (y - 1 + size) % size;
            for (int x = 0; x < size; x++)
            {
                int left = (x - 1 + size) % size;
                int right = (x + 1) % size;
                float dx = (heightMap[y * size + left] - heightMap[y * size + right]) * strength;
                float dy = (heightMap[down * size + x] - heightMap[up * size + x]) * strength;
                Vector3 normal = new Vector3(dx, dy, 1f).normalized;
                pixels[y * size + x] = new Color(normal.x * .5f + .5f, normal.y * .5f + .5f, normal.z * .5f + .5f, 1f);
            }
        }

        normalMap.SetPixels(pixels);
        normalMap.Apply(false, false);
        AssetDatabase.CreateAsset(normalMap, assetPath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
    }

    private static void CreateGrassDetails()
    {
        const int tuftCount = 720;
        List<Vector3> vertices = new List<Vector3>(tuftCount * 12);
        List<int>[] triangles = { new List<int>(), new List<int>(), new List<int>() };
        Color[] grassColors =
        {
            new Color(.16f, .34f, .09f),
            new Color(.23f, .40f, .12f),
            new Color(.31f, .43f, .15f)
        };

        for (int i = 0; i < tuftCount; i++)
        {
            float x = Random.Range(-77f, 77f);
            float z = Random.Range(-51f, 51f);
            if (Mathf.Abs(z) < 4.8f ||
                Vector2.Distance(new Vector2(x, z), new Vector2(-65f, 0f)) < 10f ||
                Vector2.Distance(new Vector2(x, z), new Vector2(65f, 0f)) < 10f ||
                (Mathf.Abs(x + 64f) < 6f && Mathf.Abs(z + 14f) < 4f) ||
                (Mathf.Abs(x + 28f) < 2f && Mathf.Abs(z) < 3f) ||
                (Mathf.Abs(x - 5f) < 2f && Mathf.Abs(z) < 3f) ||
                (Mathf.Abs(x - 36f) < 2f && Mathf.Abs(z) < 3f))
                continue;

            int variant = Random.Range(0, grassColors.Length);
            float height = Random.Range(.18f, .42f);
            float width = Random.Range(.045f, .09f);
            Vector3 basePoint = new Vector3(x, .025f, z);

            for (int blade = 0; blade < 3; blade++)
            {
                float angle = blade * 60f + Random.Range(-16f, 16f);
                Vector3 right = Quaternion.Euler(0f, angle, 0f) * Vector3.right * width;
                Vector3 lean = Quaternion.Euler(0f, angle + 90f, 0f) * Vector3.forward * Random.Range(.04f, .15f);
                int start = vertices.Count;
                vertices.Add(basePoint - right);
                vertices.Add(basePoint + right);
                vertices.Add(basePoint + lean + Vector3.up * height + right * .12f);
                vertices.Add(basePoint + lean + Vector3.up * height - right * .12f);
                List<int> t = triangles[variant];
                // Double sided triangles keep blades visible from the third-person camera.
                t.Add(start); t.Add(start + 2); t.Add(start + 1);
                t.Add(start); t.Add(start + 3); t.Add(start + 2);
                t.Add(start + 1); t.Add(start + 2); t.Add(start);
                t.Add(start + 2); t.Add(start + 3); t.Add(start);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Combined Natural Grass Tufts";
        mesh.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.subMeshCount = triangles.Length;
        for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        string meshPath = Root + "/NaturalGrassDetails.asset";
        Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (existingMesh == null)
            AssetDatabase.CreateAsset(mesh, meshPath);
        else
        {
            existingMesh.Clear();
            existingMesh.indexFormat = mesh.indexFormat;
            existingMesh.SetVertices(vertices);
            existingMesh.subMeshCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++) existingMesh.SetTriangles(triangles[i], i);
            existingMesh.RecalculateNormals();
            existingMesh.RecalculateBounds();
            Object.DestroyImmediate(mesh);
            mesh = existingMesh;
        }

        GameObject grassObject = new GameObject("Combined Meadow Grass Details");
        MeshFilter filter = grassObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = grassObject.AddComponent<MeshRenderer>();
        filter.sharedMesh = mesh;
        renderer.sharedMaterials = new[]
        {
            Mat("GrassBladeDeep", grassColors[0]),
            Mat("GrassBladeMid", grassColors[1]),
            Mat("GrassBladeSunlit", grassColors[2])
        };
        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            Material mat = renderer.sharedMaterials[i];
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", .18f);
            mat.enableInstancing = true;
        }
    }

    private static Material Mat(string name,Color color)
    {
        string path = MaterialsPath + "/" + name + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) { UpgradeMaterialToURP(existing); existing.color = color; TuneMaterial(existing, name); EditorUtility.SetDirty(existing); return existing; }
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material m = new Material(shader); m.name = name; m.color = color; UpgradeMaterialToURP(m); TuneMaterial(m, name);
        AssetDatabase.CreateAsset(m, path); return m;
    }
    private static void UpgradeMaterialToURP(Material material)
    {
        if (material == null) return;
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null || material.shader == urpShader) return;

        string oldShaderName = material.shader != null ? material.shader.name : string.Empty;
        if (oldShaderName != "Standard" && !oldShaderName.Contains("Legacy Shaders"))
            return;

        Color oldColor = material.color;
        Texture oldMainTexture = material.mainTexture;
        float oldMetallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
        float oldSmoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : .25f;
        material.shader = urpShader;

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", oldColor);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", oldMainTexture);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", oldMetallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", oldSmoothness);
        if (material.HasProperty("_GlossMapScale")) material.SetFloat("_GlossMapScale", oldSmoothness);
        EditorUtility.SetDirty(material);
    }

    private static void TuneMaterial(Material material, string materialName)
    {
        bool premium = materialName.Contains("Gold") || materialName.Contains("Diamond") ||
                      materialName.Contains("Rare") || materialName.Contains("Crystal");
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", premium ? .34f : .015f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", premium ? .72f : .24f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", premium ? .72f : .24f);
        if (materialName.Contains("Gold") && material.HasProperty("_Metallic")) material.SetFloat("_Metallic", .72f);
        material.enableInstancing = true;
    }

    private static void EnsureFolder(string folder)
    {
        if(AssetDatabase.IsValidFolder(folder))return;string parent=Path.GetDirectoryName(folder).Replace("\\","/");string name=Path.GetFileName(folder);if(!AssetDatabase.IsValidFolder(parent))EnsureFolder(parent);AssetDatabase.CreateFolder(parent,name);
    }
}
#endif
