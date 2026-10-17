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
        EnsureFolder(Root + "/Scenes"); EnsureFolder(MaterialsPath);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material grass = MakeMaterial("Grass", new Color(0.22f, 0.48f, 0.24f));
        Material path = MakeMaterial("ArenaPath", new Color(0.50f, 0.49f, 0.47f));
        Material fence = MakeMaterial("ArenaFence", new Color(0.22f, 0.25f, 0.29f));
        Material blue = MakeMaterial("PlayerBlue", new Color(0.10f, 0.42f, 0.94f));
        Material orange = MakeMaterial("RivalOrange", new Color(0.96f, 0.29f, 0.08f));
        Material pants = MakeMaterial("AvatarPants", new Color(0.10f, 0.16f, 0.30f));
        Material skin = MakeMaterial("AvatarSkin", new Color(0.92f, 0.68f, 0.45f));
        Material hair = MakeMaterial("AvatarHair", new Color(0.18f, 0.10f, 0.06f));
        Material shoes = MakeMaterial("AvatarShoes", new Color(0.08f, 0.08f, 0.10f));
        Material face = MakeMaterial("AvatarFace", new Color(0.035f, 0.025f, 0.02f));
        Material gold = MakeMaterial("EggGold", new Color(1f, 0.76f, 0.08f));
        Material cream = MakeMaterial("EggCream", new Color(1f, 0.94f, 0.75f));

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Arena Ground"; floor.transform.localScale = new Vector3(4f, 1f, 4f);
        floor.GetComponent<Renderer>().sharedMaterial = grass;
        Cube("Center Path", new Vector3(0f, 0.015f, 0f), new Vector3(30f, 0.03f, 5f), path);
        Cube("North Fence", new Vector3(0f, 0.7f, 20f), new Vector3(40f, 1.4f, 0.5f), fence);
        Cube("South Fence", new Vector3(0f, 0.7f, -20f), new Vector3(40f, 1.4f, 0.5f), fence);
        Cube("West Fence", new Vector3(-20f, 0.7f, 0f), new Vector3(0.5f, 1.4f, 40f), fence);
        Cube("East Fence", new Vector3(20f, 0.7f, 0f), new Vector3(0.5f, 1.4f, 40f), fence);

        Transform blueBase = Base("BLUE HOME BASE", new Vector3(-15f, 0.12f, 0f), blue);
        Transform orangeBase = Base("ORANGE RIVAL BASE", new Vector3(15f, 0.12f, 0f), orange);
        GameObject player = Avatar("Player", new Vector3(-13.5f, 0f, 0f), blue, pants, skin, hair, shoes, face);
        player.AddComponent<StealEggsPlayerController>();
        GameObject rival = Avatar("Rival Bot", new Vector3(13.5f, 0f, 0f), orange, pants, skin, hair, shoes, face);
        rival.AddComponent<StealEggsRivalAI>().rivalBase = orangeBase;

        Vector3[] eggPositions = {
            new Vector3(-7f,.8f,0f), new Vector3(-5f,.8f,2f), new Vector3(-3f,.8f,-2f),
            new Vector3(-1f,.8f,1.5f), new Vector3(1f,.8f,-1.5f), new Vector3(3f,.8f,2f),
            new Vector3(5f,.8f,-2f), new Vector3(7f,.8f,0f), new Vector3(9f,.8f,2.5f),
            new Vector3(11f,.8f,-2.5f), new Vector3(14f,.8f,3f), new Vector3(16f,.8f,-3f),
            new Vector3(15f,.8f,1f), new Vector3(12.5f,.8f,1.2f)
        };
        for (int i = 0; i < eggPositions.Length; i++)
        {
            bool rivalSide = eggPositions[i].x >= 12f;
            Egg("Egg " + (i + 1), eggPositions[i], rivalSide, rivalSide ? gold : cream);
        }

        GameObject managerObject = new GameObject("Steal Eggs Game Manager");
        StealEggsGameManager manager = managerObject.AddComponent<StealEggsGameManager>();
        manager.playerHomeBase = blueBase; manager.rivalHomeBase = orangeBase;
        HUD(manager); CameraRig(player.transform); LightRig();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        bool saved = EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        if (saved)
            EditorUtility.DisplayDialog("Steal Eggs", "Third-person arena created!\n\nW/A/S/D or arrows: move\nHold RIGHT MOUSE + drag: rotate camera\nMouse wheel: zoom\nSPACE: jump | SHIFT: run\nE: pick up an egg\nReturn to BLUE base to score.", "OK");
    }

    private static GameObject Avatar(string name, Vector3 position, Material shirt, Material pants, Material skin, Material hair, Material shoes, Material face)
    {
        GameObject root = new GameObject(name); root.transform.position = position;
        CharacterController cc = root.AddComponent<CharacterController>();
        cc.height = 2.35f; cc.radius = 0.38f; cc.center = new Vector3(0f, 1.16f, 0f); cc.stepOffset = 0.3f;

        Transform torso = Part("Torso", root.transform, PrimitiveType.Cube, new Vector3(0f,1.39f,0f), new Vector3(.78f,.80f,.42f), shirt);
        Transform head = Part("Head", root.transform, PrimitiveType.Cube, new Vector3(0f,2.02f,0f), new Vector3(.62f,.62f,.62f), skin);
        Part("Hair", head, PrimitiveType.Cube, new Vector3(0f,.39f,-.01f), new Vector3(1.08f,.28f,1.08f), hair);
        Part("Left Eye", head, PrimitiveType.Cube, new Vector3(-.24f,.04f,.51f), new Vector3(.12f,.14f,.04f), face);
        Part("Right Eye", head, PrimitiveType.Cube, new Vector3(.24f,.04f,.51f), new Vector3(.12f,.14f,.04f), face);
        Part("Smile", head, PrimitiveType.Cube, new Vector3(0f,-.21f,.51f), new Vector3(.25f,.04f,.04f), face);

        Transform la = Pivot("Left Arm Pivot", root.transform, new Vector3(-.51f,1.68f,0f));
        Part("Left Arm", la, PrimitiveType.Cube, new Vector3(0f,-.34f,0f), new Vector3(.28f,.69f,.32f), skin);
        Transform ra = Pivot("Right Arm Pivot", root.transform, new Vector3(.51f,1.68f,0f));
        Part("Right Arm", ra, PrimitiveType.Cube, new Vector3(0f,-.34f,0f), new Vector3(.28f,.69f,.32f), skin);
        Transform ll = Pivot("Left Leg Pivot", root.transform, new Vector3(-.20f,.88f,0f));
        Part("Left Leg", ll, PrimitiveType.Cube, new Vector3(0f,-.32f,0f), new Vector3(.32f,.62f,.35f), pants);
        Part("Left Shoe", ll, PrimitiveType.Cube, new Vector3(0f,-.70f,.075f), new Vector3(.36f,.18f,.46f), shoes);
        Transform rl = Pivot("Right Leg Pivot", root.transform, new Vector3(.20f,.88f,0f));
        Part("Right Leg", rl, PrimitiveType.Cube, new Vector3(0f,-.32f,0f), new Vector3(.32f,.62f,.35f), pants);
        Part("Right Shoe", rl, PrimitiveType.Cube, new Vector3(0f,-.70f,.075f), new Vector3(.36f,.18f,.46f), shoes);

        StealEggsCharacterAnimation anim = root.AddComponent<StealEggsCharacterAnimation>();
        anim.leftArmPivot = la; anim.rightArmPivot = ra; anim.leftLegPivot = ll; anim.rightLegPivot = rl;
        anim.torso = torso; anim.head = head; anim.CacheRestPose();
        return root;
    }

    private static Transform Pivot(string name, Transform parent, Vector3 localPosition)
    {
        GameObject obj = new GameObject(name); obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition; obj.transform.localRotation = Quaternion.identity; return obj.transform;
    }
    private static Transform Part(string name, Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent, false);
        obj.transform.localPosition = pos; obj.transform.localRotation = Quaternion.identity; obj.transform.localScale = scale;
        Renderer r = obj.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = mat;
        Collider c = obj.GetComponent<Collider>(); if (c != null) Object.DestroyImmediate(c);
        return obj.transform;
    }
    private static void Egg(string name, Vector3 pos, bool rivalSide, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos;
        obj.transform.localScale = new Vector3(.65f,.82f,.65f); obj.GetComponent<Renderer>().sharedMaterial = mat;
        obj.AddComponent<EggPickup>().spawnedAtRivalBase = rivalSide;
    }
    private static Transform Base(string name, Vector3 pos, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name = name; obj.transform.position = pos;
        obj.transform.localScale = new Vector3(5f,.12f,5f); obj.GetComponent<Renderer>().sharedMaterial = mat; return obj.transform;
    }
    private static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = pos;
        obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = mat; return obj;
    }
    private static void CameraRig(Transform player)
    {
        GameObject obj = new GameObject("Main Camera"); obj.tag = "MainCamera";
        Camera camera = obj.AddComponent<Camera>(); camera.nearClipPlane = .1f; camera.farClipPlane = 150f; camera.fieldOfView = 60f;
        obj.AddComponent<AudioListener>();
        StealEggsThirdPersonCamera orbit = obj.AddComponent<StealEggsThirdPersonCamera>();
        orbit.target = player; orbit.distance = 7.5f; orbit.focusOffset = new Vector3(0f,1.35f,0f);
        obj.transform.position = player.position + new Vector3(0f,3.5f,-7f); obj.transform.LookAt(player.position + new Vector3(0f,1.35f,0f));
    }
    private static void LightRig()
    {
        GameObject obj = new GameObject("Sun Light"); Light l = obj.AddComponent<Light>();
        l.type = LightType.Directional; l.intensity = 1.25f; obj.transform.rotation = Quaternion.Euler(50f,-30f,0f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.55f,.62f,.72f);
        RenderSettings.ambientEquatorColor = new Color(.36f,.39f,.34f);
        RenderSettings.ambientGroundColor = new Color(.22f,.20f,.17f);
    }
    private static void HUD(StealEggsGameManager manager)
    {
        GameObject canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f,720f); scaler.matchWidthOrHeight = .5f;
        GameObject panel = new GameObject("HUD Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvasObject.transform,false);
        RectTransform pr = panel.GetComponent<RectTransform>(); pr.anchorMin=new Vector2(0f,1f); pr.anchorMax=new Vector2(0f,1f); pr.pivot=new Vector2(0f,1f); pr.anchoredPosition=new Vector2(18f,-18f); pr.sizeDelta=new Vector2(500f,145f);
        panel.GetComponent<Image>().color = new Color(.04f,.05f,.07f,.78f);
        Text hud = MakeText(panel.transform,"Score Text",new Vector2(12f,-10f),new Vector2(470f,125f),22,TextAnchor.UpperLeft);
        hud.color=Color.white; hud.fontStyle=FontStyle.Bold; manager.hudText=hud;
        Text message=MakeText(canvasObject.transform,"Message Text",Vector2.zero,new Vector2(1100f,60f),22,TextAnchor.MiddleCenter);
        RectTransform mr=message.GetComponent<RectTransform>(); mr.anchorMin=new Vector2(.5f,0f); mr.anchorMax=new Vector2(.5f,0f); mr.pivot=new Vector2(.5f,0f); mr.anchoredPosition=new Vector2(0f,34f);
        message.color=Color.white; message.fontStyle=FontStyle.Bold; manager.messageText=message;
        Text controls=MakeText(canvasObject.transform,"Controls Text",Vector2.zero,new Vector2(410f,105f),16,TextAnchor.LowerRight);
        RectTransform cr=controls.GetComponent<RectTransform>(); cr.anchorMin=new Vector2(1f,0f); cr.anchorMax=new Vector2(1f,0f); cr.pivot=new Vector2(1f,0f); cr.anchoredPosition=new Vector2(-18f,18f);
        controls.text="MOVE: WASD / ARROWS\nCAMERA: HOLD RIGHT MOUSE + DRAG\nJUMP: SPACE  |  RUN: SHIFT\nPICK UP: E"; controls.color=Color.white;
    }
    private static Text MakeText(Transform parent,string name,Vector2 pos,Vector2 size,int fontSize,TextAnchor align)
    {
        GameObject obj=new GameObject(name,typeof(RectTransform),typeof(Text)); obj.transform.SetParent(parent,false);
        RectTransform rect=obj.GetComponent<RectTransform>(); rect.anchorMin=new Vector2(0f,1f); rect.anchorMax=new Vector2(0f,1f); rect.pivot=new Vector2(0f,1f); rect.anchoredPosition=pos; rect.sizeDelta=size;
        Text t=obj.GetComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if(t.font==null)t.font=Font.CreateDynamicFontFromOSFont("Arial",fontSize);
        t.fontSize=fontSize; t.alignment=align; t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Overflow; t.text=string.Empty; return t;
    }
    private static Material MakeMaterial(string name,Color color)
    {
        string path=MaterialsPath+"/"+name+".mat"; Material existing=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(existing!=null){existing.color=color;EditorUtility.SetDirty(existing);return existing;}
        Shader shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null)shader=Shader.Find("Standard");
        Material mat=new Material(shader);mat.name=name;mat.color=color;AssetDatabase.CreateAsset(mat,path);return mat;
    }
    private static void EnsureFolder(string folder)
    {
        if(AssetDatabase.IsValidFolder(folder))return;
        string parent=Path.GetDirectoryName(folder).Replace("\\","/"); string name=Path.GetFileName(folder);
        if(!AssetDatabase.IsValidFolder(parent))EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent,name);
    }
}
#endif
