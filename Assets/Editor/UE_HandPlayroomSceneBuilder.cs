using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UE_HandPlayroomSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/HandPlayroom.unity";
    private const string MaterialPath = "Assets/Materials/HandPlayroom";
    private const string RunMarkerPath = "Assets/Editor/UE_HandPlayroomSceneBuilder.run";
    private const string ProcedureBaselineRunMarkerPath = "Assets/Editor/UE_HandPlayroomProcedureBaseline.run";
    private const string Step2RunMarkerPath = "Assets/Editor/UE_HandPlayroomStep2Blockout.run";
    private const string Step3RunMarkerPath = "Assets/Editor/UE_HandPlayroomStep3ProgressFeedback.run";
    private const string Step45RunMarkerPath = "Assets/Editor/UE_HandPlayroomStep45MediaPipeHands.run";
    private const string FloorplanPath = "Assets/Concept/cute_hand_room_2D평면도.png";
    private const string RoomMoodboardPath = "Assets/Concept/cute_hand_room_moodboard.png";
    private const string UiMoodboardPath = "Assets/Concept/cute_hand_interaction_ui_moodboard.png";
    private const string ObjectSheetPath = "Assets/Concept/cute_object_interaction_sheet.png";

    [MenuItem("UE/Build Hand Playroom Scene")]
    public static void BuildScene()
    {
        if (!CanEditScenes("Build Hand Playroom Scene"))
        {
            return;
        }

        EnsureFolder("Assets/Materials");
        EnsureFolder(MaterialPath);
        EnsureFolder("Assets/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "HandPlayroom";

        Material cream = CreateMaterial("HP_CreamWall", new Color(1f, 0.957f, 0.871f));
        Material mint = CreateMaterial("HP_MintFloor", new Color(0.75f, 0.922f, 0.847f));
        Material coral = CreateMaterial("HP_Coral", new Color(1f, 0.541f, 0.478f));
        Material sky = CreateMaterial("HP_SkyBlue", new Color(0.549f, 0.78f, 0.969f));
        Material yellow = CreateMaterial("HP_WarmYellow", new Color(1f, 0.82f, 0.4f));
        Material wood = CreateMaterial("HP_LightWood", new Color(0.82f, 0.58f, 0.36f));
        Material darkWood = CreateMaterial("HP_DarkWood", new Color(0.45f, 0.25f, 0.13f));
        Material cyan = CreateMaterial("HP_InteractionCyan", new Color(0.125f, 0.851f, 0.824f), true);
        Material white = CreateMaterial("HP_SoftWhite", new Color(1f, 0.985f, 0.94f));
        Material green = CreateMaterial("HP_PlantGreen", new Color(0.285f, 0.62f, 0.255f));
        Material shadow = CreateMaterial("HP_SoftShadow", new Color(0.72f, 0.62f, 0.47f));

        GameObject root = new GameObject("UE_HandPlayroom_Blockout");
        GameObject room = CreateGroup("Room", root.transform);
        GameObject props = CreateGroup("Interactable Props", root.transform);
        GameObject guides = CreateGroup("Guide Markers", root.transform);
        GameObject labels = CreateGroup("Scene Labels", root.transform);
        GameObject references = CreateGroup("Concept References", root.transform);
        GameObject lights = CreateGroup("Lights", root.transform);

        CreateRoom(room.transform, cream, mint, wood, darkWood);
        CreateStartZone(guides.transform, labels.transform, cyan, white);
        CreateMovePath(guides.transform, cyan, white);
        CreateCentralTable(props.transform, guides.transform, labels.transform, wood, coral, sky, yellow, cyan, green);
        CreatePuzzleBox(props.transform, guides.transform, labels.transform, wood, coral, sky, yellow, cyan);
        CreateToyShelf(props.transform, labels.transform, wood, darkWood, coral, sky, yellow, green, cyan);
        CreateSofaArea(props.transform, labels.transform, coral, yellow, green, wood);
        CreateExitDoor(props.transform, labels.transform, cream, coral, yellow, cyan);
        CreatePlants(props.transform, green, wood);
        CreateConceptBoards(references.transform, wood, white);
        CreateLights(lights.transform, yellow);

        CreateLabel("MOVE ZONE", new Vector3(0f, 0.08f, -2.95f), 0.45f, labels.transform, cyan, true);
        CreateLabel("RIGHT-HAND INTERACTION REACH", new Vector3(0f, 0.08f, 0.95f), 0.26f, labels.transform, cyan, true);
        CreateLabel("Hand Play Room", new Vector3(0f, 3.45f, 3.92f), 0.36f, labels.transform, yellow, false);

        SetupCameraAndLighting();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.92f, 0.89f, 0.82f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("UE/Apply Hand Playroom Procedure Baseline")]
    public static void ApplyProcedureBaseline()
    {
        if (!CanEditScenes("Apply Hand Playroom Procedure Baseline"))
        {
            return;
        }

        EnsureFolder("Assets/Scenes");

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject root = FindOrCreateRoot("UE_HandPlayroomRoot");
        GameObject systems = FindOrCreateChild(root.transform, "Systems");
        GameObject player = FindOrCreateChild(root.transform, "Player");
        GameObject rooms = FindOrCreateChild(root.transform, "Rooms");
        GameObject interactables = FindOrCreateChild(root.transform, "Interactables");
        GameObject ui = FindOrCreateChild(root.transform, "UI");

        EnsureCameraParent(player.transform);
        EnsureCanvas(ui.transform);
        EnsureEventSystem();
        EnsureFlowController(systems.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("UE/Apply Hand Playroom Step 2 Blockout")]
    public static void ApplyStep2Blockout()
    {
        if (!CanEditScenes("Apply Hand Playroom Step 2 Blockout"))
        {
            return;
        }

        EnsureFolder("Assets/Scenes");

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject root = FindOrCreateRoot("UE_HandPlayroomRoot");
        GameObject player = FindOrCreateOrAdoptRootChild(root.transform, "Player");
        GameObject rooms = FindOrCreateChild(root.transform, "Rooms");
        GameObject interactables = FindOrCreateChild(root.transform, "Interactables");

        EnsurePlayerBlockout(player.transform);
        EnsureRoomStep2Blockout(rooms.transform);
        EnsureInteractableStep2Blockout(interactables.transform);
        EnsureStep2Lighting(root.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("UE/Apply Hand Playroom Step 3 Progress Feedback")]
    public static void ApplyStep3ProgressFeedback()
    {
        if (!CanEditScenes("Apply Hand Playroom Step 3 Progress Feedback"))
        {
            return;
        }

        EnsureFolder("Assets/Scenes");

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject root = FindOrCreateRoot("UE_HandPlayroomRoot");
        GameObject systems = FindOrCreateChild(root.transform, "Systems");
        GameObject ui = FindOrCreateChild(root.transform, "UI");

        EnsureCanvas(ui.transform);
        EnsureProgressFeedbackController(systems.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("UE/Apply Hand Playroom Step 4.5 MediaPipe Hands")]
    public static void ApplyStep45MediaPipeHands()
    {
        if (!CanEditScenes("Apply Hand Playroom Step 4.5 MediaPipe Hands"))
        {
            return;
        }

        EnsureFolder("Assets/Scenes");

        Scene scene = File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject root = FindOrCreateRoot("UE_HandPlayroomRoot");
        GameObject systems = FindOrCreateChild(root.transform, "Systems");

        EnsureMediaPipeHandTrackingAdapter(systems.transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [InitializeOnLoadMethod]
    private static void BuildSceneWhenRequested()
    {
        if (!HasSceneMarker())
        {
            return;
        }

        EditorApplication.delayCall += RunSceneMarkersWhenReady;
    }

    private static bool HasSceneMarker()
    {
        return File.Exists(RunMarkerPath)
            || File.Exists(ProcedureBaselineRunMarkerPath)
            || File.Exists(Step2RunMarkerPath)
            || File.Exists(Step3RunMarkerPath)
            || File.Exists(Step45RunMarkerPath);
    }

    private static void RunSceneMarkersWhenReady()
    {
        if (!HasSceneMarker())
        {
            return;
        }

        if (!CanEditScenes("Run Hand Playroom scene marker"))
        {
            EditorApplication.playModeStateChanged -= RunSceneMarkersAfterPlayMode;
            EditorApplication.playModeStateChanged += RunSceneMarkersAfterPlayMode;
            return;
        }

        if (File.Exists(ProcedureBaselineRunMarkerPath))
        {
            ApplyProcedureBaseline();
            File.Delete(ProcedureBaselineRunMarkerPath);
        }

        if (File.Exists(Step2RunMarkerPath))
        {
            ApplyStep2Blockout();
            File.Delete(Step2RunMarkerPath);
        }

        if (File.Exists(Step3RunMarkerPath))
        {
            ApplyStep3ProgressFeedback();
            File.Delete(Step3RunMarkerPath);
        }

        if (File.Exists(Step45RunMarkerPath))
        {
            ApplyStep45MediaPipeHands();
            File.Delete(Step45RunMarkerPath);
        }

        if (File.Exists(RunMarkerPath))
        {
            BuildScene();
            File.Delete(RunMarkerPath);
        }

        AssetDatabase.Refresh();
    }

    private static void RunSceneMarkersAfterPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }

        EditorApplication.playModeStateChanged -= RunSceneMarkersAfterPlayMode;
        EditorApplication.delayCall += RunSceneMarkersWhenReady;
    }

    private static bool CanEditScenes(string actionName)
    {
        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return true;
        }

        Debug.LogWarning($"{actionName} skipped because Unity is in Play Mode. Stop Play Mode and run the menu item again.");
        return false;
    }

    private static void CreateRoom(Transform parent, Material cream, Material mint, Material wood, Material darkWood)
    {
        CreateCube("Mint Floor", new Vector3(0f, -0.05f, 0f), new Vector3(14f, 0.1f, 8f), mint, parent);
        CreateCube("Back Cream Wall", new Vector3(0f, 1.2f, 4.05f), new Vector3(14.4f, 2.4f, 0.25f), cream, parent);
        CreateCube("Front Low Wall", new Vector3(0f, 0.55f, -4.05f), new Vector3(14.4f, 1.1f, 0.25f), cream, parent);
        CreateCube("Left Cream Wall", new Vector3(-7.05f, 1.2f, 0f), new Vector3(0.25f, 2.4f, 8.2f), cream, parent);
        CreateCube("Right Cream Wall", new Vector3(7.05f, 1.2f, 0f), new Vector3(0.25f, 2.4f, 8.2f), cream, parent);
        CreateCube("Back Wood Trim", new Vector3(0f, 0.18f, 3.86f), new Vector3(14f, 0.18f, 0.16f), darkWood, parent);
        CreateCube("Left Window", new Vector3(-6.93f, 1.35f, -1.5f), new Vector3(0.08f, 1.8f, 1.45f), wood, parent);
        CreateCube("Window Warm Glass", new Vector3(-6.88f, 1.35f, -1.5f), new Vector3(0.04f, 1.45f, 1.1f), CreateMaterial("HP_WindowGlow", new Color(1f, 0.83f, 0.45f), true), parent);

        Vector3[] corners =
        {
            new Vector3(-6.95f, 0.55f, -3.95f),
            new Vector3(6.95f, 0.55f, -3.95f),
            new Vector3(-6.95f, 1.2f, 3.95f),
            new Vector3(6.95f, 1.2f, 3.95f)
        };

        foreach (Vector3 corner in corners)
        {
            CreateCylinder("Rounded Room Corner", corner, new Vector3(0.42f, 0.55f, 0.42f), cream, parent);
        }
    }

    private static void CreateStartZone(Transform guides, Transform labels, Material cyan, Material white)
    {
        CreateCylinder("START Footprint Pad", new Vector3(-5.55f, 0.02f, -2.85f), new Vector3(1.2f, 0.04f, 1.2f), cyan, guides);
        CreateCylinder("Start Inner Pad", new Vector3(-5.55f, 0.055f, -2.85f), new Vector3(0.92f, 0.03f, 0.92f), white, guides);
        CreateSphere("Left Foot Icon", new Vector3(-5.85f, 0.13f, -2.8f), new Vector3(0.18f, 0.04f, 0.28f), cyan, guides);
        CreateSphere("Right Foot Icon", new Vector3(-5.25f, 0.13f, -2.8f), new Vector3(0.18f, 0.04f, 0.28f), cyan, guides);
        CreateLabel("START", new Vector3(-5.55f, 0.1f, -3.55f), 0.36f, labels, cyan, true);
    }

    private static void CreateMovePath(Transform parent, Material cyan, Material white)
    {
        CreateDashedLine("Move Path Bottom", new Vector3(-4.4f, 0.06f, -2.75f), new Vector3(3.2f, 0.06f, -2.75f), 18, white, parent);
        CreateDashedLine("Move Path Top", new Vector3(-4.6f, 0.06f, 1.55f), new Vector3(4.9f, 0.06f, 2.35f), 20, white, parent);
        CreateDashedLine("Move Path Right", new Vector3(3.4f, 0.06f, -2.65f), new Vector3(5.65f, 0.06f, 2.25f), 13, white, parent);
        CreateCylinder("Central Interaction Reach", new Vector3(0f, 0.045f, 0f), new Vector3(3.9f, 0.025f, 2.25f), cyan, parent);
        CreateCylinder("Central Reach Fill", new Vector3(0f, 0.055f, 0f), new Vector3(3.65f, 0.02f, 2f), CreateMaterial("HP_ReachFill", new Color(0.75f, 0.98f, 0.95f, 0.42f), true), parent);
    }

    private static void CreateCentralTable(Transform props, Transform guides, Transform labels, Material wood, Material coral, Material sky, Material yellow, Material cyan, Material green)
    {
        CreateCube("Central Rounded Table Top", new Vector3(0f, 0.78f, 0f), new Vector3(4.6f, 0.24f, 1.8f), wood, props);
        CreateCube("Central Table Coral Inset", new Vector3(0f, 0.92f, 0f), new Vector3(4.25f, 0.05f, 1.55f), coral, props);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                CreateCylinder("Table Leg", new Vector3(x * 1.95f, 0.35f, z * 0.72f), new Vector3(0.18f, 0.7f, 0.18f), wood, props);
            }
        }

        CreateCylinder("Interact Ring Toy Train", new Vector3(-1.55f, 1.04f, 0f), new Vector3(0.72f, 0.03f, 0.72f), cyan, guides);
        CreateCylinder("Interact Ring Flower", new Vector3(0f, 1.04f, 0f), new Vector3(0.72f, 0.03f, 0.72f), cyan, guides);
        CreateCylinder("Interact Ring Stack Toy", new Vector3(1.55f, 1.04f, 0f), new Vector3(0.72f, 0.03f, 0.72f), cyan, guides);

        CreateCube("Grab Cube Blue", new Vector3(-1.7f, 1.15f, -0.05f), new Vector3(0.45f, 0.45f, 0.45f), sky, props);
        CreateCube("Grab Cube Yellow", new Vector3(-1.25f, 1.12f, -0.08f), new Vector3(0.35f, 0.35f, 0.35f), yellow, props);
        CreateCylinder("Flower Pot", new Vector3(0f, 1.14f, -0.02f), new Vector3(0.32f, 0.32f, 0.32f), wood, props);
        CreateSphere("Flower Leaves", new Vector3(0f, 1.45f, -0.02f), new Vector3(0.42f, 0.18f, 0.42f), green, props);
        CreateSphere("Flower Head", new Vector3(0f, 1.66f, -0.02f), new Vector3(0.26f, 0.08f, 0.26f), yellow, props);

        for (int i = 0; i < 4; i++)
        {
            CreateCylinder($"Stack Toy Ring {i + 1}", new Vector3(1.55f, 1.12f + i * 0.17f, 0f), new Vector3(0.55f - i * 0.09f, 0.12f, 0.55f - i * 0.09f), i % 2 == 0 ? sky : yellow, props);
        }

        CreateLabel("PINCH / PLACE", new Vector3(0f, 1.45f, -1.35f), 0.22f, labels, cyan, false);
    }

    private static void CreatePuzzleBox(Transform props, Transform guides, Transform labels, Material wood, Material coral, Material sky, Material yellow, Material cyan)
    {
        CreateCube("Puzzle Side Table", new Vector3(-5.45f, 0.45f, 1.35f), new Vector3(1.45f, 0.24f, 1.25f), wood, props);
        CreateCube("Puzzle Box Base", new Vector3(-5.45f, 0.92f, 1.35f), new Vector3(1.15f, 0.72f, 0.95f), sky, props);
        CreateCube("Puzzle Box Coral Lid", new Vector3(-5.45f, 1.3f, 1.35f), new Vector3(1.23f, 0.18f, 1.03f), coral, props);
        CreateSphere("Puzzle Box Star", new Vector3(-5.45f, 1.47f, 1.35f), new Vector3(0.32f, 0.08f, 0.32f), yellow, props);
        CreateCylinder("Puzzle Box Interaction Glow", new Vector3(-5.45f, 1.56f, 1.35f), new Vector3(0.9f, 0.025f, 0.9f), cyan, guides);
        CreateLabel("PUZZLE BOX", new Vector3(-5.45f, 1.75f, 0.45f), 0.26f, labels, cyan, false);
    }

    private static void CreateToyShelf(Transform props, Transform labels, Material wood, Material darkWood, Material coral, Material sky, Material yellow, Material green, Material cyan)
    {
        CreateCube("Toy Shelf Back", new Vector3(0f, 1.8f, 3.55f), new Vector3(6.5f, 2.0f, 0.22f), wood, props);
        CreateCube("Toy Shelf Top", new Vector3(0f, 2.75f, 3.25f), new Vector3(6.7f, 0.16f, 0.75f), darkWood, props);
        CreateCube("Toy Shelf Bottom", new Vector3(0f, 0.85f, 3.25f), new Vector3(6.7f, 0.16f, 0.75f), darkWood, props);

        for (int i = -2; i <= 2; i++)
        {
            CreateCube("Toy Shelf Divider", new Vector3(i * 1.25f, 1.8f, 3.25f), new Vector3(0.12f, 1.85f, 0.75f), darkWood, props);
        }

        for (int i = 0; i < 12; i++)
        {
            float x = -2.9f + i * 0.52f;
            CreateSphere($"Star String Light {i + 1}", new Vector3(x, 2.92f, 3.05f), new Vector3(0.16f, 0.16f, 0.16f), yellow, props);
        }

        CreateSphere("Shelf Bear Head", new Vector3(-2.85f, 2.28f, 3.02f), new Vector3(0.28f, 0.28f, 0.18f), coral, props);
        CreateCube("Shelf Books", new Vector3(-1.45f, 2.1f, 3.02f), new Vector3(0.8f, 0.45f, 0.2f), sky, props);
        CreateSphere("Shelf Plant", new Vector3(2.85f, 2.08f, 3.02f), new Vector3(0.36f, 0.36f, 0.2f), green, props);
        CreateCube("Shelf Toy Houses", new Vector3(1.05f, 1.45f, 3.02f), new Vector3(0.75f, 0.5f, 0.24f), yellow, props);

        CreateLabel("TOY SHELF", new Vector3(0f, 2.95f, 2.65f), 0.3f, labels, cyan, false);
    }

    private static void CreateSofaArea(Transform props, Transform labels, Material coral, Material yellow, Material green, Material wood)
    {
        CreateCylinder("Sofa Rug", new Vector3(4.75f, 0.02f, -1.9f), new Vector3(1.65f, 0.035f, 1.25f), CreateMaterial("HP_PeachRug", new Color(1f, 0.73f, 0.65f)), props);
        CreateCube("Sofa Seat", new Vector3(5.35f, 0.55f, -1.55f), new Vector3(1.85f, 0.45f, 0.82f), coral, props);
        CreateCube("Sofa Back", new Vector3(5.35f, 1.02f, -1.2f), new Vector3(1.95f, 0.9f, 0.25f), coral, props);
        CreateCube("Sofa Left Arm", new Vector3(4.35f, 0.8f, -1.55f), new Vector3(0.28f, 0.85f, 0.95f), coral, props);
        CreateCube("Sofa Right Arm", new Vector3(6.35f, 0.8f, -1.55f), new Vector3(0.28f, 0.85f, 0.95f), coral, props);
        CreateSphere("Star Pillow", new Vector3(5.05f, 1.1f, -1.85f), new Vector3(0.36f, 0.08f, 0.36f), yellow, props);
        CreateSphere("Heart Pillow", new Vector3(5.7f, 1.1f, -1.85f), new Vector3(0.34f, 0.08f, 0.34f), green, props);
        CreateCylinder("Side Table", new Vector3(4.1f, 0.45f, -2.55f), new Vector3(0.45f, 0.45f, 0.45f), wood, props);
        CreateSphere("Lamp Glow", new Vector3(4.1f, 1.15f, -2.55f), new Vector3(0.28f, 0.32f, 0.28f), yellow, props);
        CreateLabel("COZY REST CORNER", new Vector3(5.25f, 1.8f, -2.65f), 0.22f, labels, yellow, false);
    }

    private static void CreateExitDoor(Transform props, Transform labels, Material cream, Material coral, Material yellow, Material cyan)
    {
        CreateCube("Exit Door Arch Back", new Vector3(5.55f, 1.35f, 3.86f), new Vector3(1.65f, 2.25f, 0.25f), cream, props);
        CreateCube("Exit Door Coral Panel", new Vector3(5.55f, 1.2f, 3.68f), new Vector3(1.25f, 1.9f, 0.18f), coral, props);
        CreateSphere("Exit Door Heart Window", new Vector3(5.55f, 1.8f, 3.56f), new Vector3(0.34f, 0.18f, 0.08f), cyan, props);
        CreateSphere("Exit Door Knob", new Vector3(6.08f, 1.15f, 3.5f), new Vector3(0.12f, 0.12f, 0.12f), yellow, props);
        CreateCube("Exit Rug", new Vector3(5.55f, 0.04f, 2.86f), new Vector3(1.45f, 0.08f, 0.9f), coral, props);
        CreateLabel("EXIT", new Vector3(5.55f, 1.95f, 2.7f), 0.32f, labels, cyan, false);
    }

    private static void CreatePlants(Transform props, Material green, Material wood)
    {
        Vector3[] positions =
        {
            new Vector3(-6.2f, 0.25f, 2.75f),
            new Vector3(-6.2f, 0.25f, -2.9f),
            new Vector3(6.25f, 0.25f, 2.6f)
        };

        foreach (Vector3 position in positions)
        {
            CreateCylinder("Plant Pot", position, new Vector3(0.28f, 0.5f, 0.28f), wood, props);
            CreateSphere("Plant Leaves", position + new Vector3(0f, 0.55f, 0f), new Vector3(0.58f, 0.35f, 0.58f), green, props);
        }
    }

    private static void CreateConceptBoards(Transform parent, Material wood, Material white)
    {
        CreateTextureBoard("Floorplan Reference Board", FloorplanPath, new Vector3(-3.7f, 1.85f, -3.86f), new Vector3(2.4f, 1.35f, 0.08f), parent, wood, white);
        CreateTextureBoard("Room Moodboard Board", RoomMoodboardPath, new Vector3(0f, 1.85f, -3.86f), new Vector3(2.4f, 1.35f, 0.08f), parent, wood, white);
        CreateTextureBoard("UI Moodboard Board", UiMoodboardPath, new Vector3(3.7f, 1.85f, -3.86f), new Vector3(2.4f, 1.35f, 0.08f), parent, wood, white);
        CreateTextureBoard("Object Sheet Small Board", ObjectSheetPath, new Vector3(6.86f, 1.65f, -0.2f), new Vector3(0.08f, 1.3f, 1.9f), parent, wood, white);
    }

    private static void CreateLights(Transform parent, Material yellow)
    {
        Light sun = CreateLight("Warm Directional Light", LightType.Directional, new Vector3(0f, 5f, 0f), parent);
        SetRotation(sun.transform, Quaternion.Euler(52f, -35f, 0f));
        sun.color = new Color(1f, 0.93f, 0.78f);
        sun.intensity = 1.3f;

        Light lamp = CreateLight("Star Lamp Completion Light", LightType.Point, new Vector3(1.55f, 2.2f, 0f), parent);
        lamp.color = new Color(1f, 0.82f, 0.35f);
        lamp.range = 4.5f;
        lamp.intensity = 2.4f;

        Light window = CreateLight("Window Fill Light", LightType.Point, new Vector3(-6.1f, 2f, -1.5f), parent);
        window.color = new Color(1f, 0.82f, 0.48f);
        window.range = 5f;
        window.intensity = 1.7f;

        CreateSphere("Visible Star Lamp Glow", new Vector3(1.55f, 1.9f, 0f), new Vector3(0.28f, 0.28f, 0.28f), yellow, parent);
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        return root != null ? root : new GameObject(name);
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);

        if (child != null)
        {
            return child.gameObject;
        }

        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static GameObject FindOrCreateOrAdoptRootChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);

        if (child != null)
        {
            return child.gameObject;
        }

        GameObject existing = GameObject.Find(name);

        if (existing != null)
        {
            existing.transform.SetParent(parent, true);
            return existing;
        }

        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static GameObject FindExistingOrCreatePrimitive(string preferredName, string fallbackName, PrimitiveType primitiveType, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject gameObject = GameObject.Find(preferredName);

        if (gameObject == null && !string.IsNullOrEmpty(fallbackName))
        {
            gameObject = GameObject.Find(fallbackName);
        }

        if (gameObject == null)
        {
            gameObject = GameObject.CreatePrimitive(primitiveType);
        }

        gameObject.name = preferredName;
        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        return gameObject;
    }

    private static void EnsurePlayerBlockout(Transform player)
    {
        FindExistingOrCreatePrimitive("Player_Body", string.Empty, PrimitiveType.Capsule, player, new Vector3(0f, 0.9f, -3f), new Vector3(0.55f, 0.9f, 0.55f));
        FindExistingOrCreatePrimitive("Player_Forward_Marker", string.Empty, PrimitiveType.Cube, player, new Vector3(0f, 0.05f, -1.8f), new Vector3(0.28f, 0.08f, 0.9f));

        GameObject mainCamera = Camera.main != null ? Camera.main.gameObject : GameObject.Find("Main Camera");

        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(player, true);
        }
    }

    private static void EnsureRoomStep2Blockout(Transform rooms)
    {
        Transform existingBlockout = GameObject.Find("UE_HandPlayroom_Blockout")?.transform;

        if (existingBlockout != null)
        {
            existingBlockout.SetParent(rooms, true);
        }

        Transform tutorialLobby = FindOrCreateChild(rooms, "TutorialLobby").transform;
        Transform calibrationHall = FindOrCreateChild(rooms, "CalibrationHall").transform;
        Transform archiveChamber = FindOrCreateChild(rooms, "ArchiveChamber").transform;
        Transform coreBreakRoom = FindOrCreateChild(rooms, "CoreBreakRoom").transform;

        EnsureRoomShell(tutorialLobby, new Vector3(-4.5f, 0f, -2.2f), new Vector3(3.6f, 0.08f, 2.5f), "Tutorial");
        EnsureRoomShell(calibrationHall, new Vector3(0f, 0f, 0f), new Vector3(4.2f, 0.08f, 3f), "Calibration");
        EnsureRoomShell(archiveChamber, new Vector3(4.6f, 0f, 0.4f), new Vector3(3.5f, 0.08f, 2.8f), "Archive");
        EnsureRoomShell(coreBreakRoom, new Vector3(0f, 0f, 3.2f), new Vector3(4.4f, 0.08f, 2.3f), "Core");
    }

    private static void EnsureRoomShell(Transform parent, Vector3 floorPosition, Vector3 floorScale, string prefix)
    {
        FindExistingOrCreatePrimitive($"{prefix}_Floor", string.Empty, PrimitiveType.Cube, parent, floorPosition, floorScale);
        FindExistingOrCreatePrimitive($"{prefix}_BackWall", string.Empty, PrimitiveType.Cube, parent, floorPosition + new Vector3(0f, 0.8f, floorScale.z * 0.5f), new Vector3(floorScale.x, 1.6f, 0.12f));
        FindExistingOrCreatePrimitive($"{prefix}_LeftWall", string.Empty, PrimitiveType.Cube, parent, floorPosition + new Vector3(-floorScale.x * 0.5f, 0.8f, 0f), new Vector3(0.12f, 1.6f, floorScale.z));
        FindExistingOrCreatePrimitive($"{prefix}_RightWall", string.Empty, PrimitiveType.Cube, parent, floorPosition + new Vector3(floorScale.x * 0.5f, 0.8f, 0f), new Vector3(0.12f, 1.6f, floorScale.z));
    }

    private static void EnsureInteractableStep2Blockout(Transform interactables)
    {
        FindExistingOrCreatePrimitive("EnergyCube_A", "Grab Cube Blue", PrimitiveType.Cube, interactables, new Vector3(-1.2f, 1.05f, -0.2f), new Vector3(0.45f, 0.45f, 0.45f));
        FindExistingOrCreatePrimitive("EnergyCube_B", "Grab Cube Yellow", PrimitiveType.Cube, interactables, new Vector3(-0.55f, 1.05f, -0.2f), new Vector3(0.42f, 0.42f, 0.42f));
        FindExistingOrCreatePrimitive("Socket_A", "Interact Ring Toy Train", PrimitiveType.Cylinder, interactables, new Vector3(1.1f, 0.08f, -0.45f), new Vector3(0.7f, 0.06f, 0.7f));
        FindExistingOrCreatePrimitive("Socket_B", "Interact Ring Stack Toy", PrimitiveType.Cylinder, interactables, new Vector3(1.9f, 0.08f, 0.35f), new Vector3(0.7f, 0.06f, 0.7f));
        FindExistingOrCreatePrimitive("ArchiveDial", "Puzzle Box Star", PrimitiveType.Cylinder, interactables, new Vector3(4.6f, 1.2f, 0.3f), new Vector3(0.55f, 0.12f, 0.55f));
        FindExistingOrCreatePrimitive("CoreCrack_A", string.Empty, PrimitiveType.Cube, interactables, new Vector3(-0.75f, 1.15f, 3.25f), new Vector3(0.18f, 0.85f, 0.1f));
        FindExistingOrCreatePrimitive("CoreCrack_B", string.Empty, PrimitiveType.Cube, interactables, new Vector3(0f, 1.15f, 3.25f), new Vector3(0.18f, 0.85f, 0.1f));
        FindExistingOrCreatePrimitive("CoreCrack_C", string.Empty, PrimitiveType.Cube, interactables, new Vector3(0.75f, 1.15f, 3.25f), new Vector3(0.18f, 0.85f, 0.1f));
        FindExistingOrCreatePrimitive("FinalPalmPanel", "Exit Door Coral Panel", PrimitiveType.Cube, interactables, new Vector3(5.55f, 1.15f, 3.45f), new Vector3(1.1f, 1.5f, 0.12f));
    }

    private static void EnsureStep2Lighting(Transform root)
    {
        if (Object.FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(root, true);
            SetRotation(lightObject.transform, Quaternion.Euler(50f, -30f, 0f));
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
        }

        EnsurePointLight(root, "CalibrationHall_PointLight", new Vector3(0f, 2.6f, 0f));
        EnsurePointLight(root, "ArchiveChamber_PointLight", new Vector3(4.5f, 2.4f, 0.3f));
        EnsurePointLight(root, "CoreBreakRoom_PointLight", new Vector3(0f, 2.5f, 3.2f));
    }

    private static void EnsurePointLight(Transform parent, string name, Vector3 position)
    {
        GameObject lightObject = GameObject.Find(name);

        if (lightObject == null)
        {
            lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, true);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 5f;
            light.intensity = 1.3f;
        }

        lightObject.transform.position = position;
    }

    private static void EnsureCameraParent(Transform player)
    {
        GameObject mainCamera = Camera.main != null ? Camera.main.gameObject : GameObject.Find("Main Camera");

        if (mainCamera == null)
        {
            mainCamera = new GameObject("Main Camera");
            mainCamera.AddComponent<Camera>();
            mainCamera.AddComponent<AudioListener>();
            mainCamera.tag = "MainCamera";
        }

        mainCamera.transform.SetParent(player, false);
        mainCamera.transform.localPosition = new Vector3(0f, 1.6f, -4.5f);
        SetLocalRotation(mainCamera.transform, Quaternion.Euler(18f, 0f, 0f));
    }

    private static void EnsureCanvas(Transform uiRoot)
    {
        GameObject canvasObject = FindOrCreateUiChild(uiRoot, "UE_MainCanvas");
        Canvas canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (canvasObject.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        EnsurePanel(canvasObject.transform, "HandStatusPanel", new Vector2(260f, -70f), new Vector2(440f, 110f), "LEFT HAND: WAITING\nRIGHT HAND: WAITING");
        EnsurePanel(canvasObject.transform, "ObjectivePanel", new Vector2(0f, -70f), new Vector2(620f, 90f), "SHOW BOTH HANDS TO START");
        EnsurePanel(canvasObject.transform, "ProgressPanel", new Vector2(-300f, -70f), new Vector2(420f, 90f), "PROGRESS 0 / 3");
        EnsureReticle(canvasObject.transform);
        EnsurePanel(canvasObject.transform, "PromptPanel", new Vector2(0f, 90f), new Vector2(760f, 90f), "Point your right hand at an object.");
        GameObject debugPanel = EnsurePanel(canvasObject.transform, "DebugPanel", new Vector2(-320f, 70f), new Vector2(500f, 150f), "DEBUG\nHands: none\nGesture: none");
        debugPanel.SetActive(false);
    }

    private static GameObject EnsurePanel(Transform canvas, string name, Vector2 anchoredPosition, Vector2 size, string text)
    {
        GameObject panel = FindOrCreateUiChild(canvas, name);
        RectTransform rectTransform = EnsureRectTransform(panel);
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = panel.GetComponent<Image>();

        if (image == null)
        {
            image = panel.AddComponent<Image>();
        }

        image.color = new Color(0.05f, 0.08f, 0.1f, 0.68f);

        GameObject textObject = FindOrCreateUiChild(panel.transform, "Text");
        RectTransform textRect = EnsureRectTransform(textObject);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 12f);
        textRect.offsetMax = new Vector2(-18f, -12f);

        Text label = textObject.GetComponent<Text>();

        if (label == null)
        {
            label = textObject.AddComponent<Text>();
        }

        label.text = text;
        label.font = GetBuiltInFont();
        label.fontSize = 26;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;

        return panel;
    }

    private static void EnsureReticle(Transform canvas)
    {
        GameObject reticle = FindOrCreateUiChild(canvas, "CenterReticle");
        RectTransform rectTransform = EnsureRectTransform(reticle);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(32f, 32f);

        Image image = reticle.GetComponent<Image>();

        if (image == null)
        {
            image = reticle.AddComponent<Image>();
        }

        image.color = new Color(0.1f, 0.85f, 0.77f, 0.85f);
    }

    private static RectTransform EnsureRectTransform(GameObject gameObject)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            return rectTransform;
        }

        return gameObject.AddComponent<RectTransform>();
    }

    private static GameObject FindOrCreateUiChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);

        if (child != null)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                return child.gameObject;
            }

            Object.DestroyImmediate(child.gameObject);
        }

        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static void EnsureFlowController(Transform systems)
    {
        GameObject gameObject = FindOrCreateChild(systems, "UE_HandPlayroomFlowController");
        UE_HandPlayroomFlowController controller = gameObject.GetComponent<UE_HandPlayroomFlowController>();

        if (controller == null)
        {
            controller = gameObject.AddComponent<UE_HandPlayroomFlowController>();
        }

        SerializedObject serializedObject = new SerializedObject(controller);
        serializedObject.FindProperty("initialState").enumValueIndex = (int)UE_HandPlayroomFlowController.ExperienceState.WaitingForHands;
        serializedObject.FindProperty("logStateChanges").boolValue = true;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureProgressFeedbackController(Transform systems)
    {
        GameObject gameObject = FindOrCreateChild(systems, "UE_HandPlayroomProgressFeedbackController");
        UE_HandPlayroomProgressFeedbackController controller = gameObject.GetComponent<UE_HandPlayroomProgressFeedbackController>();

        if (controller == null)
        {
            controller = gameObject.AddComponent<UE_HandPlayroomProgressFeedbackController>();
        }

        SerializedObject serializedObject = new SerializedObject(controller);
        serializedObject.FindProperty("flowController").objectReferenceValue = Object.FindAnyObjectByType<UE_HandPlayroomFlowController>();
        serializedObject.FindProperty("starLampRequiredCompletions").intValue = 3;
        serializedObject.FindProperty("feedbackDisplaySeconds").floatValue = 2f;
        serializedObject.FindProperty("handStatusText").objectReferenceValue = FindPanelText("HandStatusPanel");
        serializedObject.FindProperty("objectiveText").objectReferenceValue = FindPanelText("ObjectivePanel");
        serializedObject.FindProperty("progressText").objectReferenceValue = FindPanelText("ProgressPanel");
        serializedObject.FindProperty("promptText").objectReferenceValue = FindPanelText("PromptPanel");
        serializedObject.FindProperty("debugText").objectReferenceValue = FindPanelText("DebugPanel");
        serializedObject.FindProperty("centerReticleImage").objectReferenceValue = GameObject.Find("CenterReticle")?.GetComponent<Image>();
        serializedObject.FindProperty("movementCircleImage").objectReferenceValue = GameObject.Find("MovementCircle")?.GetComponent<Image>();
        serializedObject.FindProperty("progressFillImage").objectReferenceValue = GameObject.Find("ProgressFill")?.GetComponent<Image>();
        serializedObject.FindProperty("starLampObject").objectReferenceValue = GameObject.Find("Visible Star Lamp Glow");
        FillDefaultObjectPrompts(serializedObject.FindProperty("objectPrompts"));
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureMediaPipeHandTrackingAdapter(Transform systems)
    {
        GameObject trackingGameObject = FindOrCreateChild(systems, "UE_HandTrackingGestureController");
        UE_HandTrackingGestureController trackingController = trackingGameObject.GetComponent<UE_HandTrackingGestureController>();

        if (trackingController == null)
        {
            trackingController = trackingGameObject.AddComponent<UE_HandTrackingGestureController>();
        }

        GameObject adapterGameObject = FindOrCreateChild(systems, "UE_MediaPipeHandTrackingAdapter");
        UE_MediaPipeHandTrackingAdapter adapter = adapterGameObject.GetComponent<UE_MediaPipeHandTrackingAdapter>();

        if (adapter == null)
        {
            adapter = adapterGameObject.AddComponent<UE_MediaPipeHandTrackingAdapter>();
        }

        SerializedObject adapterObject = new SerializedObject(adapter);
        adapterObject.FindProperty("handTrackingController").objectReferenceValue = trackingController;
        adapterObject.FindProperty("swapHands").boolValue = false;
        adapterObject.FindProperty("pinchDistance").floatValue = 0.06f;
        adapterObject.FindProperty("curledFingerRatio").floatValue = 1.15f;
        adapterObject.FindProperty("fallbackConfidence").floatValue = 0.75f;
        adapterObject.ApplyModifiedPropertiesWithoutUndo();

        Mediapipe.Unity.Sample.HandLandmarkDetection.HandLandmarkerRunner runner =
            Object.FindAnyObjectByType<Mediapipe.Unity.Sample.HandLandmarkDetection.HandLandmarkerRunner>();

        if (runner == null)
        {
            return;
        }

        SerializedObject runnerObject = new SerializedObject(runner);
        runnerObject.FindProperty("handTrackingAdapter").objectReferenceValue = adapter;
        runnerObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Text FindPanelText(string panelName)
    {
        GameObject panel = GameObject.Find(panelName);

        if (panel == null)
        {
            return null;
        }

        Transform textTransform = panel.transform.Find("Text");
        return textTransform != null ? textTransform.GetComponent<Text>() : panel.GetComponentInChildren<Text>(true);
    }

    private static Font GetBuiltInFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font == null)
        {
            Debug.LogWarning("Built-in LegacyRuntime.ttf font was not found. UI Text will use Unity's default font fallback.");
        }

        return font;
    }

    private static void FillDefaultObjectPrompts(SerializedProperty prompts)
    {
        string[,] defaults =
        {
            { "EnergyCube_A", "Pinch to pick up the blue energy cube." },
            { "EnergyCube_B", "Pinch to pick up the yellow energy cube." },
            { "Socket_A", "Release the cube on the matching socket." },
            { "Socket_B", "Release the cube on the matching socket." },
            { "ArchiveDial", "Rotate your right hand to align the archive dial." },
            { "CoreCrack_A", "Make a fist to break the cracked core." },
            { "CoreCrack_B", "Make a fist to break the cracked core." },
            { "CoreCrack_C", "Make a fist to break the cracked core." },
            { "FinalPalmPanel", "Hold your right palm on the final panel." }
        };

        prompts.arraySize = defaults.GetLength(0);

        for (int i = 0; i < defaults.GetLength(0); i++)
        {
            SerializedProperty element = prompts.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("objectId").stringValue = defaults[i, 0];
            element.FindPropertyRelative("prompt").stringValue = defaults[i, 1];
        }
    }

    private static void SetupCameraAndLighting()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 7.3f, -7.2f);
        SetRotation(cameraObject.transform, Quaternion.Euler(58f, 0f, 0f));
        camera.fieldOfView = 48f;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.backgroundColor = new Color(0.75f, 0.88f, 0.95f);
    }

    private static GameObject CreateGroup(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group;
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        AssignMaterial(gameObject, material);
        return gameObject;
    }

    private static GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        AssignMaterial(gameObject, material);
        return gameObject;
    }

    private static GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gameObject.name = name;
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        AssignMaterial(gameObject, material);
        return gameObject;
    }

    private static void CreateDashedLine(string name, Vector3 start, Vector3 end, int count, Material material, Transform parent)
    {
        GameObject group = CreateGroup(name, parent);
        Vector3 delta = end - start;
        Quaternion rotation = NormalizeRotation(Quaternion.LookRotation(delta.normalized, Vector3.up));

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0f : i / (float)(count - 1);
            GameObject dash = CreateCube("Dash", Vector3.Lerp(start, end, t), new Vector3(0.32f, 0.035f, 0.08f), material, group.transform);
            SetRotation(dash.transform, rotation);
        }
    }

    private static void CreateLabel(string text, Vector3 position, float size, Transform parent, Material material, bool floorLabel)
    {
        GameObject label = new GameObject($"{text} Label");
        label.transform.SetParent(parent);
        label.transform.position = position;
        SetRotation(label.transform, floorLabel ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.Euler(60f, 0f, 0f));

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = size;
        textMesh.fontSize = 72;
        textMesh.color = Color.white;

        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
    }

    private static void CreateTextureBoard(string name, string texturePath, Vector3 position, Vector3 scale, Transform parent, Material frameMaterial, Material fallbackMaterial)
    {
        CreateCube($"{name} Frame", position, scale + new Vector3(0.12f, 0.12f, 0.04f), frameMaterial, parent);

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        Material material = fallbackMaterial;

        if (texture != null)
        {
            material = CreateTextureMaterial(name.Replace(" ", string.Empty), texture);
        }

        GameObject face = CreateCube($"{name} Image", position + new Vector3(0f, 0f, -0.055f), scale, material, parent);

        if (scale.x < scale.z)
        {
            SetRotation(face.transform, Quaternion.Euler(0f, 90f, 0f));
        }
    }

    private static Light CreateLight(string name, LightType type, Vector3 position, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.position = position;
        Light light = gameObject.AddComponent<Light>();
        light.type = type;
        light.shadows = LightShadows.Soft;
        return light;
    }

    private static void SetRotation(Transform transform, Quaternion rotation)
    {
        transform.rotation = NormalizeRotation(rotation);
    }

    private static void SetLocalRotation(Transform transform, Quaternion rotation)
    {
        transform.localRotation = NormalizeRotation(rotation);
    }

    private static Quaternion NormalizeRotation(Quaternion rotation)
    {
        float lengthSquared = rotation.x * rotation.x
            + rotation.y * rotation.y
            + rotation.z * rotation.z
            + rotation.w * rotation.w;

        if (lengthSquared <= Mathf.Epsilon)
        {
            return Quaternion.identity;
        }

        float inverseLength = 1f / Mathf.Sqrt(lengthSquared);
        return new Quaternion(
            rotation.x * inverseLength,
            rotation.y * inverseLength,
            rotation.z * inverseLength,
            rotation.w * inverseLength);
    }

    private static Material CreateMaterial(string name, Color color, bool emissive = false)
    {
        string path = $"{MaterialPath}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            shader = shader != null ? shader : Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.SetColor("_BaseColor", color);

        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.4f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateTextureMaterial(string name, Texture2D texture)
    {
        string path = $"{MaterialPath}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            shader = shader != null ? shader : Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.mainTexture = texture;
        material.SetTexture("_BaseMap", texture);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void AssignMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folder = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent ?? "Assets", folder);
    }
}
