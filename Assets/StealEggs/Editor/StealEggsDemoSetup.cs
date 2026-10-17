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

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Arena Ground"; ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = grass;
        Cube("Center Path", new Vector3(0f, .015f, 0f), new Vector3(30f, .03f, 5f), path);
        Cube("North Fence", new Vector3(0f, .7f, 20f), new Vector3(40f, 1.4f, .5f), fence);
        Cube("South Fence", new Vector3(0f, .7f, -20f), new Vector3(40f, 1.4f, .5f), fence);
        Cube("West Fence", new Vector3(-20f, .7f, 0f), new Vector3(.5f, 1.4f, 40f), fence);
        Cube("East Fence", new Vector3(20f, .7f, 0f), new Vector3(.5f, 1.4f, 40f), fence);
        foreach (int z in new int[] { -14, -8, 8, 14 })
        {
            Tree(new Vector3(-17f, 0f, z), trunk, leaves);
            Tree(new Vector3(17f, 0f, z), trunk, leaves);
        }

        Transform blueBase = Base("BLUE HOME BASE", new Vector3(-15f, .12f, 0f), blue);
        Transform orangeBase = Base("ORANGE RIVAL BASE", new Vector3(15f, .12f, 0f), orange);
        GameObject player = Avatar("Player", new Vector3(-13.5f, 0f, 0f), blue, pants, skin, hair, shoes, face);
        player.AddComponent<StealEggsPlayerController>();
        TrailRenderer dashTrail = player.AddComponent<TrailRenderer>();
        dashTrail.time = .14f; dashTrail.startWidth = .35f; dashTrail.endWidth = .01f;
        Shader trailShader = Shader.Find("Sprites/Default");
        if (trailShader != null) dashTrail.material = new Material(trailShader);
        dashTrail.startColor = new Color(.2f, .8f, 1f, .8f); dashTrail.endColor = new Color(.2f, .8f, 1f, 0f);
        GameObject rival = Avatar("Rival Bot", new Vector3(13.5f, 0f, 0f), orange, pants, skin, hair, shoes, face);
        rival.AddComponent<StealEggsRivalAI>().rivalBase = orangeBase;

        Vector3[] positions = {
            new Vector3(-7f,.8f,0f), new Vector3(-5f,.8f,2f), new Vector3(-3f,.8f,-2f),
            new Vector3(-1f,.8f,1.5f), new Vector3(1f,.8f,-1.5f), new Vector3(3f,.8f,2f),
            new Vector3(5f,.8f,-2f), new Vector3(7f,.8f,0f), new Vector3(9f,.8f,2.5f),
            new Vector3(11f,.8f,-2.5f), new Vector3(14f,.8f,3f), new Vector3(16f,.8f,-3f),
            new Vector3(15f,.8f,1f), new Vector3(12.5f,.8f,1.2f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            bool onRivalSide = positions[i].x >= 12f;
            float roll = Random.value;
            StealEggRarity rarity = roll < .03f ? StealEggRarity.Diamond : roll < .13f ? StealEggRarity.Golden : roll < .35f ? StealEggRarity.Rare : StealEggRarity.Common;
            Material eggMat = rarity == StealEggRarity.Diamond ? eggDiamond : rarity == StealEggRarity.Golden ? eggGold : rarity == StealEggRarity.Rare ? eggRare : eggCommon;
            int coinValue = rarity == StealEggRarity.Diamond ? 100 : rarity == StealEggRarity.Golden ? 30 : rarity == StealEggRarity.Rare ? 12 : 5;
            Egg("Egg " + (i + 1) + " - " + rarity, positions[i], onRivalSide, eggMat, rarity, coinValue);
        }

        GameObject managerObj = new GameObject("Steal Eggs Game Manager");
        StealEggsGameManager manager = managerObj.AddComponent<StealEggsGameManager>();
        manager.playerHomeBase = blueBase; manager.rivalHomeBase = orangeBase;
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
        GameObject obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder);obj.name=name;obj.transform.position=pos;obj.transform.localScale=new Vector3(5f,.12f,5f);obj.GetComponent<Renderer>().sharedMaterial=mat;return obj.transform;
    }
    private static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.position=pos;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=mat;return obj;
    }
    private static void Tree(Vector3 pos, Material trunkMat, Material leafMat)
    {
        GameObject trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder);trunk.name="Tree Trunk";trunk.transform.position=pos+new Vector3(0f,.85f,0f);trunk.transform.localScale=new Vector3(.45f,.85f,.45f);trunk.GetComponent<Renderer>().sharedMaterial=trunkMat;
        GameObject crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name="Tree Crown";crown.transform.position=pos+new Vector3(0f,2.1f,0f);crown.transform.localScale=new Vector3(2.1f,2.2f,2.1f);crown.GetComponent<Renderer>().sharedMaterial=leafMat;
    }
    private static void BuildCamera(Transform target)
    {
        GameObject obj=new GameObject("Main Camera");obj.tag="MainCamera";Camera cam=obj.AddComponent<Camera>();cam.nearClipPlane=.1f;cam.farClipPlane=150f;cam.fieldOfView=60f;obj.AddComponent<AudioListener>();
        StealEggsThirdPersonCamera orbit=obj.AddComponent<StealEggsThirdPersonCamera>();orbit.target=target;orbit.distance=7.5f;orbit.focusOffset=new Vector3(0f,1.35f,0f);
        obj.transform.position=target.position+new Vector3(0f,3.5f,-7f);obj.transform.LookAt(target.position+new Vector3(0f,1.35f,0f));
    }
    private static void BuildLight()
    {
        GameObject obj=new GameObject("Sun Light");Light light=obj.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;obj.transform.rotation=Quaternion.Euler(50f,-30f,0f);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.55f,.62f,.72f);RenderSettings.ambientEquatorColor=new Color(.36f,.39f,.34f);RenderSettings.ambientGroundColor=new Color(.22f,.20f,.17f);
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
        manager.speedButtonText=ShopButton(canvasObj.transform,"Speed Upgrade",new Vector2(18f,196f),manager.BuySpeedUpgrade);
        manager.reachButtonText=ShopButton(canvasObj.transform,"Pickup Range Upgrade",new Vector2(18f,150f),manager.BuyReachUpgrade);
        manager.bagButtonText=ShopButton(canvasObj.transform,"Bag Upgrade",new Vector2(18f,104f),manager.BuyBagUpgrade);
        manager.dashButtonText=ShopButton(canvasObj.transform,"Dash Upgrade",new Vector2(18f,58f),manager.BuyDashUpgrade);
        Text controls=TextUI(canvasObj.transform,"Controls Text",Vector2.zero,new Vector2(395f,104f),15,TextAnchor.LowerRight);RectTransform cr=controls.GetComponent<RectTransform>();cr.anchorMin=new Vector2(1f,0f);cr.anchorMax=new Vector2(1f,0f);cr.pivot=new Vector2(1f,0f);cr.anchoredPosition=new Vector2(-18f,18f);controls.text="MOVE: WASD / ARROWS\nCAMERA: RIGHT MOUSE + DRAG\nJUMP: SPACE | SPRINT: SHIFT\nDASH: Q | PICK UP: E";controls.color=Color.white;
    }
    private static Text ShopButton(Transform parent,string name,Vector2 pos,System.Action action)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));obj.transform.SetParent(parent,false);RectTransform r=obj.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.zero;r.pivot=Vector2.zero;r.anchoredPosition=pos;r.sizeDelta=new Vector2(260f,40f);
        Image image=obj.GetComponent<Image>();image.color=new Color(.08f,.20f,.34f,.96f);Button button=obj.GetComponent<Button>();ColorBlock colors=button.colors;colors.highlightedColor=new Color(.16f,.42f,.66f,1f);colors.pressedColor=new Color(.04f,.13f,.24f,1f);button.colors=colors;button.onClick.AddListener(()=>action());
        Text label=TextUI(obj.transform,"Label",Vector2.zero,new Vector2(250f,36f),14,TextAnchor.MiddleCenter);RectTransform lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(5f,2f);lr.offsetMax=new Vector2(-5f,-2f);label.color=Color.white;label.fontStyle=FontStyle.Bold;return label;
    }
    private static Text TextUI(Transform parent,string name,Vector2 pos,Vector2 size,int fontSize,TextAnchor alignment)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Text));obj.transform.SetParent(parent,false);RectTransform rect=obj.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0f,1f);rect.anchorMax=new Vector2(0f,1f);rect.pivot=new Vector2(0f,1f);rect.anchoredPosition=pos;rect.sizeDelta=size;
        Text text=obj.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");if(text.font==null)text.font=Font.CreateDynamicFontFromOSFont("Arial",fontSize);text.fontSize=fontSize;text.alignment=alignment;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;text.text="";return text;
    }
    private static Material Mat(string name,Color color)
    {
        string path=MaterialsPath+"/"+name+".mat";Material existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing!=null){existing.color=color;EditorUtility.SetDirty(existing);return existing;}
        Shader shader=Shader.Find("Universal Render Pipeline/Lit");if(shader==null)shader=Shader.Find("Standard");Material m=new Material(shader);m.name=name;m.color=color;AssetDatabase.CreateAsset(m,path);return m;
    }
    private static void EnsureFolder(string folder)
    {
        if(AssetDatabase.IsValidFolder(folder))return;string parent=Path.GetDirectoryName(folder).Replace("\\","/");string name=Path.GetFileName(folder);if(!AssetDatabase.IsValidFolder(parent))EnsureFolder(parent);AssetDatabase.CreateFolder(parent,name);
    }
}
#endif
