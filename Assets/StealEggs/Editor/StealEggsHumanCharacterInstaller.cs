#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces the generated procedural bodies with a locally imported, rigged human model.
/// The selected Store/Mixamo model stays in the user's project and is not copied into this repo.
/// </summary>
public sealed class StealEggsHumanCharacterInstaller : EditorWindow
{
    private GameObject mainCharacterPrefab;
    private GameObject rivalCharacterPrefab;
    private GameObject guardianCharacterPrefab;
    private AnimatorController existingController;
    private AnimationClip idleClip;
    private AnimationClip walkClip;
    private AnimationClip runClip;
    private AnimationClip jumpClip;

    private bool replacePlayer = true;
    private bool replaceRival = true;
    private bool replaceGuardians = false;
    private float targetHeight = 1.82f;
    private float modelYawCorrection = 0f;
    private string status = "Import model FBX/prefab vào Assets trước, rồi kéo nó vào ô Model nhân vật.";

    [MenuItem("Steal Eggs/Install Realistic Human Character")]
    public static void OpenWindow()
    {
        var window = GetWindow<StealEggsHumanCharacterInstaller>("Human Character Installer");
        window.minSize = new Vector2(455f, 650f);
        window.TryFindCharacterModel(false);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Steal Eggs — Realistic Human Character", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Công cụ này thay phần hình người được tạo bằng hình khối, nhưng giữ nguyên Player, CharacterController, " +
            "nhặt trứng, AI và camera. Bạn phải import một model người 3D riêng; công cụ không tự tải model từ Internet.",
            MessageType.Info);

        if (GUILayout.Button("Tìm model Humanoid đã import trong Assets", GUILayout.Height(28f)))
            TryFindCharacterModel(true);

        EditorGUILayout.Space(5f);
        mainCharacterPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Model người chính (FBX/Prefab)", mainCharacterPrefab, typeof(GameObject), false);
        rivalCharacterPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Model riêng cho Rival (không bắt buộc)", rivalCharacterPrefab, typeof(GameObject), false);
        guardianCharacterPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Model riêng cho Guardian (không bắt buộc)", guardianCharacterPrefab, typeof(GameObject), false);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        existingController = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller có sẵn", existingController, typeof(AnimatorController), false);
        EditorGUILayout.LabelField("Hoặc kéo các AnimationClip vào đây để tạo controller đơn giản:");
        idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle clip", idleClip, typeof(AnimationClip), false);
        walkClip = (AnimationClip)EditorGUILayout.ObjectField("Walk clip", walkClip, typeof(AnimationClip), false);
        runClip = (AnimationClip)EditorGUILayout.ObjectField("Run clip (không bắt buộc)", runClip, typeof(AnimationClip), false);
        jumpClip = (AnimationClip)EditorGUILayout.ObjectField("Jump clip (không bắt buộc)", jumpClip, typeof(AnimationClip), false);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Đối tượng cần thay", EditorStyles.boldLabel);
        replacePlayer = EditorGUILayout.ToggleLeft("Player — người chơi chính", replacePlayer);
        replaceRival = EditorGUILayout.ToggleLeft("Rival Bot — đối thủ", replaceRival);
        replaceGuardians = EditorGUILayout.ToggleLeft("Guardians — các NPC bảo vệ", replaceGuardians);

        EditorGUILayout.Space(8f);
        targetHeight = EditorGUILayout.Slider("Chiều cao model (m)", targetHeight, 1.35f, 2.15f);
        modelYawCorrection = EditorGUILayout.Slider("Xoay model quanh trục Y", modelYawCorrection, -180f, 180f);

        EditorGUILayout.Space(10f);
        using (new EditorGUI.DisabledScope(mainCharacterPrefab == null && rivalCharacterPrefab == null && guardianCharacterPrefab == null))
        {
            if (GUILayout.Button("THAY NHÂN VẬT TRONG SCENE HIỆN TẠI", GUILayout.Height(42f)))
                InstallCharacters();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(status, MessageType.None);
    }

    private void TryFindCharacterModel(bool updateStatus)
    {
        string bestPath = null;
        int bestScore = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            string lower = path.ToLowerInvariant();
            int score = 0;

            if (importer != null && importer.animationType == ModelImporterAnimationType.Human)
                score += 100;
            if (lower.Contains("mixamo")) score += 30;
            if (lower.Contains("human") || lower.Contains("character") || lower.Contains("avatar")) score += 15;
            if (lower.Contains("animation") || lower.Contains("idle") || lower.Contains("walk") || lower.Contains("run")) score -= 40;
            if (score > bestScore)
            {
                bestScore = score;
                bestPath = path;
            }
        }

        if (mainCharacterPrefab == null && !string.IsNullOrEmpty(bestPath))
            mainCharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);

        if (updateStatus)
        {
            status = mainCharacterPrefab != null
                ? "Đã chọn: " + AssetDatabase.GetAssetPath(mainCharacterPrefab) + ". Kiểm tra Rig = Humanoid rồi cài vào scene."
                : "Chưa tìm thấy model người. Hãy import FBX/Prefab, rồi kéo từ Project vào ô Model người chính.";
        }
        Repaint();
    }

    private void InstallCharacters()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            status = "Không tìm thấy scene hiện tại.";
            return;
        }

        if (EditorUtility.DisplayDialog(
                "Thay nhân vật 3D",
                "Hãy lưu hoặc sao lưu scene trước. Công cụ sẽ ẩn phần hình khối cũ rồi thêm model người đã chọn. " +
                "Các script gameplay trên root được giữ lại. Tiếp tục?",
                "Thay nhân vật", "Hủy") == false)
            return;

        AnimatorController controllerToUse = existingController;
        if (idleClip != null || walkClip != null || runClip != null || jumpClip != null)
        {
            if (idleClip == null || walkClip == null)
            {
                status = "Để tự tạo animation controller, cần ít nhất Idle và Walk clip. Hoặc chọn Animator Controller có sẵn.";
                return;
            }
            controllerToUse = CreateSimpleController();
            if (controllerToUse == null)
            {
                status = "Không tạo được Animator Controller. Kiểm tra Console để biết thêm.";
                return;
            }
        }

        GameObject[] objects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int changed = 0;
        int missingAnimator = 0;
        int missingTarget = 0;

        foreach (GameObject obj in objects)
        {
            if (obj == null || obj.scene != scene || obj.transform.parent != null)
                continue;

            string name = obj.name;
            GameObject prefabForTarget = null;
            bool matches = false;

            if (replacePlayer && string.Equals(name, "Player", StringComparison.OrdinalIgnoreCase))
            {
                prefabForTarget = mainCharacterPrefab;
                matches = true;
            }
            else if (replaceRival && string.Equals(name, "Rival Bot", StringComparison.OrdinalIgnoreCase))
            {
                prefabForTarget = rivalCharacterPrefab != null ? rivalCharacterPrefab : mainCharacterPrefab;
                matches = true;
            }
            else if (replaceGuardians && name.IndexOf("Guardian", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                prefabForTarget = guardianCharacterPrefab != null ? guardianCharacterPrefab : mainCharacterPrefab;
                matches = true;
            }

            if (!matches) continue;
            if (prefabForTarget == null)
            {
                missingTarget++;
                continue;
            }

            if (InstallOnRoot(obj, prefabForTarget, controllerToUse))
                changed++;

            Animator installedAnimator = obj.GetComponentInChildren<Animator>(true);
            if (installedAnimator == null || installedAnimator.avatar == null || !installedAnimator.avatar.isValid)
                missingAnimator++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();

        if (changed == 0)
        {
            status = "Không tìm thấy nhân vật mục tiêu trong scene đang mở, hoặc chưa chọn được model. Hãy mở scene game, chọn model và thử lại.";
        }
        else
        {
            status = "Đã thay hình người ở " + changed + " đối tượng. " +
                     (missingAnimator > 0
                         ? missingAnimator + " model chưa có Avatar Animator hợp lệ; chúng có thể hiện hình nhưng chưa chạy animation. Kiểm tra Model Import Settings > Rig > Humanoid. "
                         : "Đã tìm thấy Animator/Avatar hợp lệ. ") +
                     (missingTarget > 0 ? missingTarget + " đối tượng chưa có model được gán. " : "") +
                     "Kiểm tra Scene/Game view và Ctrl+S.";
        }
    }

    private bool InstallOnRoot(GameObject characterRoot, GameObject selectedModel, AnimatorController controllerToUse)
    {
        Transform previousVisual = characterRoot.transform.Find("StealEggsRealisticVisual");
        if (previousVisual != null)
            Undo.DestroyObjectImmediate(previousVisual.gameObject);

        // Disable only renderers under the existing procedural body; leave root TrailRenderer and gameplay intact.
        Renderer[] oldRenderers = characterRoot.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in oldRenderers)
        {
            if (renderer == null || renderer.transform == characterRoot.transform)
                continue;
            Undo.RecordObject(renderer, "Hide procedural character renderer");
            renderer.enabled = false;
        }

        StealEggsCharacterAnimation oldAnimation = characterRoot.GetComponent<StealEggsCharacterAnimation>();
        if (oldAnimation != null)
        {
            Undo.RecordObject(oldAnimation, "Disable procedural limb animation");
            oldAnimation.enabled = false;
        }

        GameObject visual;
        try
        {
            visual = PrefabUtility.InstantiatePrefab(selectedModel, characterRoot.scene) as GameObject;
        }
        catch
        {
            visual = null;
        }
        if (visual == null)
        {
            visual = UnityEngine.Object.Instantiate(selectedModel);
            SceneManager.MoveGameObjectToScene(visual, characterRoot.scene);
        }
        if (visual == null)
            return false;

        Undo.RegisterCreatedObjectUndo(visual, "Install human character model");
        visual.name = "StealEggsRealisticVisual";
        visual.transform.SetParent(characterRoot.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(0f, modelYawCorrection, 0f);
        visual.transform.localScale = Vector3.one;

        // Prevent imported model colliders/rigidbodies from fighting the existing CharacterController.
        Collider[] modelColliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in modelColliders)
        {
            Undo.RecordObject(col, "Disable imported model collider");
            col.enabled = false;
        }
        Rigidbody[] modelBodies = visual.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody body in modelBodies)
        {
            Undo.RecordObject(body, "Make imported model rigidbody kinematic");
            body.isKinematic = true;
            body.useGravity = false;
        }

        Bounds bounds;
        if (TryGetRenderBounds(visual, out bounds) && bounds.size.y > 0.01f)
        {
            float scale = targetHeight / bounds.size.y;
            visual.transform.localScale *= scale;

            TryGetRenderBounds(visual, out bounds);
            Vector3 alignment = new Vector3(
                characterRoot.transform.position.x - bounds.center.x,
                characterRoot.transform.position.y - bounds.min.y,
                characterRoot.transform.position.z - bounds.center.z);
            visual.transform.position += alignment;
        }

        Animator animator = visual.GetComponentInChildren<Animator>(true);
        if (animator != null)
        {
            if (controllerToUse != null)
                animator.runtimeAnimatorController = controllerToUse;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        StealEggsHumanoidAnimatorDriver driver = characterRoot.GetComponent<StealEggsHumanoidAnimatorDriver>();
        if (driver == null)
            driver = Undo.AddComponent<StealEggsHumanoidAnimatorDriver>(characterRoot);
        Undo.RecordObject(driver, "Configure humanoid animation driver");
        EditorUtility.SetDirty(driver);
        EditorUtility.SetDirty(characterRoot);
        return true;
    }

    private AnimatorController CreateSimpleController()
    {
        const string folder = "Assets/StealEggs/Generated";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/StealEggs"))
                AssetDatabase.CreateFolder("Assets", "StealEggs");
            AssetDatabase.CreateFolder("Assets/StealEggs", "Generated");
        }

        string path = folder + "/StealEggsHumanoid.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
        {
            if (!AssetDatabase.DeleteAsset(path))
            {
                Debug.LogError("Could not replace generated Animator Controller at " + path);
                return null;
            }
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        if (controller == null) return null;
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = idleClip != null ? machine.AddState("Idle") : null;
        AnimatorState walk = walkClip != null ? machine.AddState("Walk") : null;
        AnimatorState run = runClip != null ? machine.AddState("Run") : null;
        AnimatorState jump = jumpClip != null ? machine.AddState("Jump") : null;

        if (idle != null) idle.motion = idleClip;
        if (walk != null) walk.motion = walkClip;
        if (run != null) run.motion = runClip;
        if (jump != null) jump.motion = jumpClip;

        machine.defaultState = idle != null ? idle : (walk != null ? walk : (run != null ? run : jump));
        if (idle != null && walk != null)
            AddTransition(idle, walk, AnimatorConditionMode.Greater, 0.12f, "Speed");
        if (walk != null && idle != null)
            AddTransition(walk, idle, AnimatorConditionMode.Less, 0.12f, "Speed");
        if (walk != null && run != null)
            AddTransition(walk, run, AnimatorConditionMode.Greater, 0.72f, "Speed");
        if (run != null && walk != null)
            AddTransition(run, walk, AnimatorConditionMode.Less, 0.68f, "Speed");
        if (walk != null && idle != null && run == null)
        {
            // The default walk/idle pair already covers movement; no additional state is needed.
        }

        if (jump != null)
        {
            AnimatorStateTransition toJump = machine.AddAnyStateTransition(jump);
            toJump.hasExitTime = false;
            toJump.duration = 0.08f;
            toJump.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");

            AnimatorState returnState = idle != null ? idle : (walk != null ? walk : run);
            if (returnState != null)
                AddTransition(jump, returnState, AnimatorConditionMode.IfNot, 0f, "IsJumping");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return controller;
    }

    private static void AddTransition(
        AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.12f;
        transition.AddCondition(mode, threshold, parameter);
    }

    private static bool TryGetRenderBounds(GameObject root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        bounds = new Bounds(root.transform.position, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled)
                continue;
            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else bounds.Encapsulate(renderer.bounds);
        }
        return found;
    }
}
#endif
