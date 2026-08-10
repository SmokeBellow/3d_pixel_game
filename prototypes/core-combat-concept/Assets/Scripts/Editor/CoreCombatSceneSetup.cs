// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10
//
// Editor-only automation: builds the entire test scene (player, attack hitbox, target
// dummies, camera rig, hit feedback) in one click instead of manual GameObject wiring.
// Menu: Tools > Core Combat Prototype > Build Scene

using UnityEditor;
using UnityEngine;

public static class CoreCombatSceneSetup
{
    [MenuItem("Tools/Core Combat Prototype/Build Scene")]
    public static void BuildScene()
    {
        if (GameObject.Find("Player") != null)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Core Combat Prototype",
                "A 'Player' object already exists in this scene. Running setup again may create duplicates.\n\nProceed anyway?",
                "Proceed", "Cancel");
            if (!proceed) return;
        }

        GameObject player = BuildPlayer();
        BuildDummies();
        GameObject cameraRig = BuildCameraRig(player);
        BuildHitFeedback(cameraRig);

        Selection.activeGameObject = player;
        EditorUtility.DisplayDialog(
            "Core Combat Prototype",
            "Scene built. Press Play to test.\n\nWASD move, left mouse = attack (chain near end of swing), right mouse held + move = orbit camera, space = dodge.",
            "OK");
    }

    static GameObject BuildPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = Vector3.zero;

        // CharacterController replaces the CapsuleCollider added by CreatePrimitive.
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        var controller = player.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 1f, 0f);
        controller.height = 2f;
        controller.radius = 0.5f;

        var attackPointGo = new GameObject("AttackPoint");
        attackPointGo.transform.SetParent(player.transform);
        attackPointGo.transform.localPosition = new Vector3(0f, 1f, 1f);

        var box = attackPointGo.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(1f, 1f, 1.2f);

        var hitbox = attackPointGo.AddComponent<AttackHitbox>();
        hitbox.hitboxCollider = box;

        var combat = player.AddComponent<PlayerCombatController>();
        combat.attackHitbox = hitbox;

        return player;
    }

    static void BuildDummies()
    {
        Vector3[] positions =
        {
            new Vector3(2f, 1f, 3f),
            new Vector3(-2f, 1f, 4f),
            new Vector3(0f, 1f, 6f),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = $"Dummy_{i + 1}";
            dummy.transform.position = positions[i];

            var rb = dummy.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            dummy.AddComponent<TargetDummy>();

            // Distinct color so the white hit-flash reads clearly against it.
            var renderer = dummy.GetComponent<Renderer>();
            var mat = new Material(renderer.sharedMaterial);
            mat.color = new Color(0.75f, 0.28f, 0.18f);
            renderer.material = mat;
        }
    }

    static GameObject BuildCameraRig(GameObject player)
    {
        var rig = new GameObject("CameraRig");
        rig.transform.position = Vector3.zero;

        var shake = rig.AddComponent<SimpleCameraShake>();
        var orbit = rig.AddComponent<OrbitCamera>();
        orbit.target = player.transform;
        orbit.shake = shake;

        Camera cam = Camera.main;
        GameObject camGo;
        if (cam != null)
        {
            camGo = cam.gameObject;
        }
        else
        {
            camGo = new GameObject("Main Camera");
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
        }

        camGo.transform.SetParent(rig.transform);
        camGo.transform.localPosition = Vector3.zero;
        camGo.transform.localRotation = Quaternion.identity;

        return rig;
    }

    static void BuildHitFeedback(GameObject cameraRig)
    {
        var go = new GameObject("HitFeedback");
        var audioSource = go.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        var feedback = go.AddComponent<HitFeedback>();
        feedback.audioSource = audioSource;
        feedback.cameraShake = cameraRig.GetComponent<SimpleCameraShake>();
    }
}
