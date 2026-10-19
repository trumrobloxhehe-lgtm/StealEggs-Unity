#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Helper for placing the Free Low Poly Nature Forest lake asset into the currently open scene.
/// Install this file under Assets/Editor/ so it is editor-only and does not ship with the game.
/// </summary>
public class StealEggsLakeInstaller : EditorWindow
{
    private SceneAsset demoScene;
    private GameObject lakeAsset;
    private GameObject treeAsset;
    private GameObject rockAsset;
    private Vector3 lakePosition = new Vector3(20f, 0.02f, -42f);
    private Vector3 demoOffset = Vector3.zero;
    private float lakeScale = 1f;
    private int treeCount = 12;
    private int rockCount = 8;
    private bool addBorderTrees = false;
    private bool addBorderRocks = false;
    private string status = "Bấm 'Tự tìm asset' để Unity tìm hồ, cây và cảnh demo trong Assets.";

    [MenuItem("Tools/Steal Eggs/Lake Asset Installer")]
    public static void OpenWindow()
    {
        var window = GetWindow<StealEggsLakeInstaller>("Lake Installer");
        window.minSize = new Vector2(440, 560);
        window.TryAutoFind(false);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Steal Eggs — Free Nature Forest Lake", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Công cụ này chỉ chạy trong Unity Editor. Hãy lưu hoặc sao lưu scene trước khi thêm môi trường. " +
            "Nó không tự sửa gameplay, player, network hay hệ thống map hiện có.", MessageType.Info);

        if (GUILayout.Button("Tự tìm asset hồ, cây, đá và scene demo", GUILayout.Height(30)))
            TryAutoFind(true);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Asset đã tìm thấy (có thể kéo asset đúng từ Project vào ô bên dưới)", EditorStyles.boldLabel);
        demoScene = (SceneAsset)EditorGUILayout.ObjectField("Cảnh demo", demoScene, typeof(SceneAsset), false);
        lakeAsset = (GameObject)EditorGUILayout.ObjectField("Mô hình/Prefab hồ", lakeAsset, typeof(GameObject), false);
        treeAsset = (GameObject)EditorGUILayout.ObjectField("Prefab cây", treeAsset, typeof(GameObject), false);
        rockAsset = (GameObject)EditorGUILayout.ObjectField("Prefab đá", rockAsset, typeof(GameObject), false);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Cách A — Thay mặt nước cũ bằng hồ mới trong scene hiện tại", EditorStyles.boldLabel);
        lakePosition = EditorGUILayout.Vector3Field("Vị trí hồ (X, Y, Z)", lakePosition);
        lakeScale = EditorGUILayout.Slider("Tỉ lệ hồ", lakeScale, 0.1f, 10f);
        addBorderTrees = EditorGUILayout.Toggle("Thêm cây quanh mép", addBorderTrees);
        using (new EditorGUI.DisabledScope(!addBorderTrees))
            treeCount = EditorGUILayout.IntSlider("Số cây", treeCount, 0, 32);
        addBorderRocks = EditorGUILayout.Toggle("Thêm đá quanh mép", addBorderRocks);
        using (new EditorGUI.DisabledScope(!addBorderRocks))
            rockCount = EditorGUILayout.IntSlider("Số đá", rockCount, 0, 32);

        using (new EditorGUI.DisabledScope(lakeAsset == null))
        {
            if (GUILayout.Button("THAY HỒ CŨ BẰNG ASSET HỒ MỚI", GUILayout.Height(38)))
                BuildLakeArea();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Cách B — Chép môi trường từ cảnh demo", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Cách này lấy các đối tượng môi trường từ scene demo được chọn và chép vào scene đang mở. " +
            "Camera, đèn, UI và đối tượng có tên Player/GameManager sẽ bị bỏ qua. Để không đè lên map hiện tại, " +
            "hãy đặt Offset X khác 0 nếu muốn đặt demo cạnh map cũ.", MessageType.Warning);
        demoOffset = EditorGUILayout.Vector3Field("Offset cho toàn bộ demo", demoOffset);
        using (new EditorGUI.DisabledScope(demoScene == null))
        {
            if (GUILayout.Button("CHÉP MÔI TRƯỜNG DEMO vào scene hiện tại", GUILayout.Height(38)))
                MergeDemoScene();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(status, MessageType.None);
    }

    private void TryAutoFind(bool updateStatus)
    {
        var objectAssets = FindGameObjectAssets();
        if (lakeAsset == null)
            lakeAsset = PickBest(objectAssets, ScoreLake);
        if (treeAsset == null)
            treeAsset = PickBest(objectAssets, ScoreTree);
        if (rockAsset == null)
            rockAsset = PickBest(objectAssets, ScoreRock);
        if (demoScene == null)
            demoScene = FindDemoScene();

        if (updateStatus)
        {
            status = "Đã quét Assets. " +
                     (lakeAsset ? "Có asset hồ: " + AssetDatabase.GetAssetPath(lakeAsset) + ". " : "Chưa tự nhận diện được asset hồ; hãy kéo prefab/model hồ từ cửa sổ Project vào ô 'Mô hình/Prefab hồ'. ") +
                     (treeAsset ? "Đã tìm thấy cây. " : "Chưa tìm thấy cây phù hợp. ") +
                     (rockAsset ? "Đã tìm thấy đá. " : "Chưa tìm thấy đá phù hợp. ") +
                     (demoScene ? "Có cảnh demo: " + AssetDatabase.GetAssetPath(demoScene) : "Chưa tìm thấy scene demo.");
        }
        Repaint();
    }

    private static List<GameObject> FindGameObjectAssets()
    {
        var paths = new HashSet<string>();
        foreach (string filter in new[] { "t:Prefab", "t:Model", "t:GameObject" })
        {
            foreach (string guid in AssetDatabase.FindAssets(filter, new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
            }
        }

        var result = new List<GameObject>();
        foreach (string path in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            if (go.GetComponentInChildren<Renderer>(true) == null &&
                go.GetComponentInChildren<MeshFilter>(true) == null) continue;
            result.Add(go);
        }
        return result;
    }

    private static GameObject PickBest(List<GameObject> assets, Func<string, int> scorer)
    {
        GameObject best = null;
        int bestScore = 0;
        foreach (var asset in assets)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            int score = scorer((asset.name + " " + path).ToLowerInvariant());
            if (score > bestScore)
            {
                bestScore = score;
                best = asset;
            }
        }
        return best;
    }

    private static int ScoreLake(string text)
    {
        if (text.Contains("water shader") || text.Contains("material") || text.Contains("texture") || text.Contains("icon")) return 0;
        int s = 0;
        if (text.Contains("lake")) s += 100;
        if (text.Contains("terrain") || text.Contains("tile")) s += 25;
        if (text.Contains("free low poly nature forest")) s += 10;
        if (text.Contains("demo") || text.Contains("list scene")) s -= 30;
        return s;
    }

    private static int ScoreTree(string text)
    {
        if (text.Contains("leaf texture") || text.Contains("tree material") || text.Contains("icon")) return 0;
        int s = 0;
        if (text.Contains("tree")) s += 100;
        if (text.Contains("birch") || text.Contains("fir")) s += 25;
        if (text.Contains("prefab")) s += 5;
        if (text.Contains("demo") || text.Contains("list scene")) s -= 30;
        return s;
    }

    private static int ScoreRock(string text)
    {
        if (text.Contains("texture") || text.Contains("material") || text.Contains("icon")) return 0;
        int s = 0;
        if (text.Contains("rock") || text.Contains("stone")) s += 100;
        if (text.Contains("prefab")) s += 5;
        if (text.Contains("demo") || text.Contains("list scene")) s -= 30;
        return s;
    }

    private static SceneAsset FindDemoScene()
    {
        string bestPath = null;
        int bestScore = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            string lowerPath = path.ToLowerInvariant();
            if (!file.Contains("demo")) continue;
            int score = 20;
            if (lowerPath.Contains("free low poly nature forest")) score += 50;
            if (file.Contains("01")) score += 5;
            if (lowerPath.Contains("sample")) score += 5;
            if (score > bestScore)
            {
                bestScore = score;
                bestPath = path;
            }
        }
        return string.IsNullOrEmpty(bestPath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(bestPath);
    }

    private void BuildLakeArea()
    {
        if (lakeAsset == null)
        {
            status = "Chưa có asset hồ. Bấm 'Tự tìm asset' hoặc kéo đúng tile Lake từ thư mục Pure Poly vào ô Mô hình/Prefab hồ.";
            return;
        }

        Scene targetScene = SceneManager.GetActiveScene();
        if (!targetScene.IsValid() || string.IsNullOrEmpty(targetScene.path))
        {
            status = "Hãy mở và lưu scene game trước, rồi chạy lại công cụ.";
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Replace Old Steal Eggs Lake");

        // Remove only objects created as the old water surface / a previous installer run.
        // Leave the rest of the scene, gameplay objects, lake shore ground, and egg placement untouched.
        GameObject[] existingObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject existing in existingObjects)
        {
            if (existing == null || existing.scene != targetScene)
                continue;

            string n = existing.name.ToLowerInvariant();
            bool oldWater = n == "sunlit lakeside - shallow water" ||
                            n == "sunlit lakeside - static water fallback (lake asset not found)" ||
                            n == "sunlit lakeside - static water fallback" ||
                            n == "lake_water_surface" ||
                            (n.Contains("sunlit lakeside") && (n.Contains("water") || n.Contains("wave")));
            if (oldWater || n == "generated_lake_area")
                Undo.DestroyObjectImmediate(existing);
        }

        var group = new GameObject("Generated_Lake_Area");
        Undo.RegisterCreatedObjectUndo(group, "Create Lake Area Parent");
        group.transform.position = Vector3.zero;

        GameObject lake = InstantiateAsset(lakeAsset, targetScene);
        if (lake == null)
        {
            Undo.DestroyObjectImmediate(group);
            status = "Unity không thể tạo instance của asset hồ được chọn.";
            return;
        }
        Undo.RegisterCreatedObjectUndo(lake, "Place Lake Asset");
        lake.name = "SUNLIT LAKESIDE - Free Low Poly Nature Lake";
        lake.transform.SetParent(group.transform, false);
        lake.transform.position = lakePosition;

        Bounds beforeScale = GetBounds(lake);
        if (beforeScale.size.x > 0.01f && beforeScale.size.z > 0.01f)
        {
            float fitScale = Mathf.Min(25f / beforeScale.size.x, 20f / beforeScale.size.z);
            fitScale = Mathf.Clamp(fitScale, 0.02f, 100f);
            lake.transform.localScale *= fitScale * lakeScale;
        }

        Bounds b = GetBounds(lake);
        Vector3 correction = new Vector3(
            lakePosition.x - b.center.x,
            lakePosition.y - b.min.y,
            lakePosition.z - b.center.z);
        lake.transform.position += correction;
        b = GetBounds(lake);

        if (addBorderTrees && treeAsset != null && treeCount > 0)
            PlaceRing(treeAsset, treeCount, b, group.transform, targetScene, "Tree");
        if (addBorderRocks && rockAsset != null && rockCount > 0)
            PlaceRing(rockAsset, rockCount, b, group.transform, targetScene, "Rock");

        Selection.activeGameObject = group;
        EditorSceneManager.MarkSceneDirty(targetScene);
        Undo.CollapseUndoOperations(undoGroup);
        status = "Đã thay mặt nước cũ bằng asset hồ mới trong scene đang mở. Kiểm tra vị trí/kích thước rồi Ctrl+S. " +
                 "Nếu hình sai, nhấn Ctrl+Z; kiểm tra ô Mô hình/Prefab hồ có trỏ đúng tile Lake của Pure Poly.";
    }

    private static GameObject InstantiateAsset(GameObject asset, Scene scene)
    {
        GameObject instance = null;
        try { instance = PrefabUtility.InstantiatePrefab(asset, scene) as GameObject; }
        catch { }
        if (instance == null)
        {
            instance = UnityEngine.Object.Instantiate(asset);
            SceneManager.MoveGameObjectToScene(instance, scene);
        }
        return instance;
    }

    private static Bounds GetBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
        return new Bounds(root.transform.position, new Vector3(20f, 1f, 20f));
    }

    private void PlaceRing(GameObject asset, int count, Bounds bounds, Transform parent, Scene scene, string label)
    {
        float rx = Mathf.Max(bounds.extents.x * 0.82f, 4f);
        float rz = Mathf.Max(bounds.extents.z * 0.82f, 4f);
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i / count) + UnityEngine.Random.Range(-0.12f, 0.12f);
            float spread = UnityEngine.Random.Range(0.88f, 1.08f);
            Vector3 pos = new Vector3(
                bounds.center.x + Mathf.Cos(angle) * rx * spread,
                bounds.min.y,
                bounds.center.z + Mathf.Sin(angle) * rz * spread);
            GameObject item = InstantiateAsset(asset, scene);
            if (item == null) continue;
            Undo.RegisterCreatedObjectUndo(item, "Place Lake Border " + label);
            item.name = "Lake_Border_" + label + "_" + (i + 1).ToString("00");
            item.transform.SetParent(parent, true);
            item.transform.position = pos;
            item.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
            float scale = label == "Tree" ? UnityEngine.Random.Range(0.85f, 1.25f) : UnityEngine.Random.Range(0.75f, 1.15f);
            item.transform.localScale = item.transform.localScale * scale;
        }
    }

    private void MergeDemoScene()
    {
        if (demoScene == null)
        {
            status = "Chưa tìm thấy cảnh demo. Hãy chọn một SceneAsset demo trong ô Cảnh demo.";
            return;
        }

        Scene target = SceneManager.GetActiveScene();
        if (!target.IsValid() || string.IsNullOrEmpty(target.path))
        {
            status = "Hãy mở và lưu scene game trước khi chép môi trường.";
            return;
        }
        string sourcePath = AssetDatabase.GetAssetPath(demoScene);
        if (string.IsNullOrEmpty(sourcePath) || sourcePath == target.path)
        {
            status = "Cảnh demo không hợp lệ hoặc đang là scene hiện tại.";
            return;
        }
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
        {
            status = "Đã hủy thao tác vì bạn chưa xác nhận lưu scene đang sửa.";
            return;
        }

        Scene source = EditorSceneManager.OpenScene(sourcePath, OpenSceneMode.Additive);
        if (!source.IsValid())
        {
            status = "Không mở được scene demo: " + sourcePath;
            return;
        }

        int moved = 0;
        foreach (GameObject root in source.GetRootGameObjects())
        {
            if (root == null || ShouldSkipDemoRoot(root)) continue;
            root.transform.position += demoOffset;
            SceneManager.MoveGameObjectToScene(root, target);
            moved++;
        }
        EditorSceneManager.CloseScene(source, true);
        SceneManager.SetActiveScene(target);
        EditorSceneManager.MarkSceneDirty(target);
        status = "Đã chép " + moved + " đối tượng gốc từ cảnh demo vào scene game. Kiểm tra Scene view rồi Ctrl+S. " +
                 "Nếu môi trường đè lên map cũ, dùng Ctrl+Z ngay hoặc Undo.";
    }

    private static bool ShouldSkipDemoRoot(GameObject root)
    {
        string n = root.name.ToLowerInvariant();
        if (n.Contains("main camera") || n.Contains("camera") || n.Contains("eventsystem") ||
            n.Contains("canvas") || n.Contains("player") || n.Contains("gamemanager") ||
            n.Contains("game manager") || n.Contains("audio listener") || n.Contains("ui"))
            return true;
        if (root.GetComponentInChildren<Camera>(true) != null) return true;
        if (root.GetComponentInChildren<AudioListener>(true) != null) return true;
        if (root.GetComponentInChildren<Light>(true) != null) return true;
        if (root.GetComponentInChildren<Canvas>(true) != null) return true;
        return false;
    }
}
#endif