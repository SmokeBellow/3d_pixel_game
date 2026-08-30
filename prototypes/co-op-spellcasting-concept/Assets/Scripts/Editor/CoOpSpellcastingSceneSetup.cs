// PROTOTYPE - NOT FOR PRODUCTION
// Question: Do cross-player elemental synergies discovered in real-time combat feel spontaneous and fun?
// Date: 2026-08-26

using UnityEditor;
using UnityEngine;

/// <summary>
/// Prototype-only one-click scene builder. Assembles the entire co-op spellcasting
/// test scene (environment, two local players, projectile templates, dummies,
/// split-screen chase cameras, debug HUD) so a solo tester can validate the
/// cross-player elemental synergy hypothesis without manual scene setup.
/// </summary>
public static class CoOpSpellcastingSceneSetup
{
    static GameObject _fireTemplate;
    static GameObject _waterTemplate;
    static GameObject _lightningTemplate;

    [MenuItem("Tools/Co-op Spellcasting Prototype/Build Scene")]
    public static void BuildScene()
    {
        if (GameObject.Find("Player1") != null)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Co-op Spellcasting Prototype",
                "It looks like the scene is already built (Player1 exists). Build again anyway? This may create duplicates.",
                "Build Anyway",
                "Cancel");
            if (!proceed) return;
        }

        BuildEnvironment();
        BuildProjectileTemplates();
        GameObject player1 = BuildPlayer(PlayerSpellController.PlayerId.One, "Player1", new Color(0.2f, 0.4f, 0.9f), new Vector3(-3f, 1f, -6f));
        GameObject player2 = BuildPlayer(PlayerSpellController.PlayerId.Two, "Player2", new Color(0.9f, 0.25f, 0.25f), new Vector3(3f, 1f, -6f));
        BuildDummies();
        BuildCameras(player1.transform, player2.transform);
        BuildHud(player1.GetComponent<PlayerSpellController>(), player2.GetComponent<PlayerSpellController>());

        Selection.activeGameObject = player1;

        EditorUtility.DisplayDialog(
            "Co-op Spellcasting Prototype — Scene Built",
            "Controls:\n" +
            "Player 1 (blue): WASD move, 1=Fire 2=Water 3=Lightning\n" +
            "Player 2 (red): Arrow keys move, Numpad1=Fire Numpad2=Water Numpad3=Lightning\n\n" +
            "Hint: Soak a dummy with Water, then hit it with Lightning for Chain Shock (3x damage + chains to nearby dummies). Watch the Console for [SYNERGY] logs.",
            "Got it");
    }

    static void BuildEnvironment()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(2f, 1f, 2f); // 20x20

        Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMat.color = new Color(0.3f, 0.3f, 0.3f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        BuildWall("WallNorth", new Vector3(0f, 1.5f, 10f), new Vector3(20f, 3f, 0.5f));
        BuildWall("WallSouth", new Vector3(0f, 1.5f, -10f), new Vector3(20f, 3f, 0.5f));
        BuildWall("WallEast", new Vector3(10f, 1.5f, 0f), new Vector3(0.5f, 3f, 20f));
        BuildWall("WallWest", new Vector3(-10f, 1.5f, 0f), new Vector3(0.5f, 3f, 20f));

        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void BuildWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;

        Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMat.color = new Color(0.15f, 0.15f, 0.18f);
        wall.GetComponent<Renderer>().sharedMaterial = wallMat;
    }

    static void BuildProjectileTemplates()
    {
        GameObject parent = new GameObject("ProjectileTemplates");

        _fireTemplate = BuildProjectileTemplate("FireProjectileTemplate", new Color(1f, 0.4f, 0.1f), parent.transform);
        _waterTemplate = BuildProjectileTemplate("WaterProjectileTemplate", new Color(0.2f, 0.45f, 0.85f), parent.transform);
        _lightningTemplate = BuildProjectileTemplate("LightningProjectileTemplate", new Color(0.95f, 0.9f, 0.2f), parent.transform);
    }

    static GameObject BuildProjectileTemplate(string name, Color color, Transform parent)
    {
        GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        proj.name = name;
        proj.transform.SetParent(parent);
        proj.transform.localScale = Vector3.one * 0.35f;

        Object.DestroyImmediate(proj.GetComponent<Collider>());

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        proj.GetComponent<Renderer>().sharedMaterial = mat;

        proj.AddComponent<SpellProjectile>();
        proj.SetActive(false); // template stays inactive; PlayerSpellController activates clones on cast

        return proj;
    }

    static GameObject BuildPlayer(PlayerSpellController.PlayerId id, string name, Color color, Vector3 position)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = name;
        player.transform.position = position;

        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 1f, 0f);
        controller.height = 2f;
        controller.radius = 0.4f;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        player.GetComponent<Renderer>().sharedMaterial = mat;

        GameObject castOrigin = new GameObject("CastOrigin");
        castOrigin.transform.SetParent(player.transform);
        castOrigin.transform.localPosition = new Vector3(0f, 1f, 0.6f);

        PlayerSpellController controllerScript = player.AddComponent<PlayerSpellController>();
        controllerScript.playerId = id;
        controllerScript.castOrigin = castOrigin.transform;
        controllerScript.fireProjectileTemplate = _fireTemplate;
        controllerScript.waterProjectileTemplate = _waterTemplate;
        controllerScript.lightningProjectileTemplate = _lightningTemplate;

        return player;
    }

    static void BuildDummies()
    {
        Vector3[] positions =
        {
            new Vector3(-4f, 1f, 4f),
            new Vector3(-1.5f, 1f, 6f),
            new Vector3(1.5f, 1f, 6f),
            new Vector3(4f, 1f, 4f),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = $"Dummy{i + 1}";
            dummy.transform.position = positions[i];

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.55f, 0.55f, 0.55f);
            dummy.GetComponent<Renderer>().sharedMaterial = mat;

            dummy.AddComponent<EnemyDummy>();

            GameObject hpLabelObj = new GameObject("HpLabel");
            hpLabelObj.transform.SetParent(dummy.transform);
            hpLabelObj.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            TextMesh hpLabel = hpLabelObj.AddComponent<TextMesh>();
            hpLabel.text = "100";
            hpLabel.characterSize = 0.2f;
            hpLabel.fontSize = 48;
            hpLabel.anchor = TextAnchor.MiddleCenter;
            hpLabel.color = Color.white;
        }
    }

    static void BuildCameras(Transform player1, Transform player2)
    {
        Camera existingMain = Camera.main;
        if (existingMain != null)
        {
            Object.DestroyImmediate(existingMain.gameObject);
        }

        GameObject cam1Obj = new GameObject("Player1Camera");
        Camera cam1 = cam1Obj.AddComponent<Camera>();
        cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
        ChaseCamera chase1 = cam1Obj.AddComponent<ChaseCamera>();
        chase1.target = player1;

        GameObject cam2Obj = new GameObject("Player2Camera");
        Camera cam2 = cam2Obj.AddComponent<Camera>();
        cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        ChaseCamera chase2 = cam2Obj.AddComponent<ChaseCamera>();
        chase2.target = player2;
    }

    static void BuildHud(PlayerSpellController player1, PlayerSpellController player2)
    {
        GameObject hudObj = new GameObject("HudOverlay");
        HudOverlay hud = hudObj.AddComponent<HudOverlay>();
        hud.player1 = player1;
        hud.player2 = player2;
    }
}
