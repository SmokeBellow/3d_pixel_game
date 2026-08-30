// PROTOTYPE - NOT FOR PRODUCTION
// Question: Do cross-player elemental synergies discovered in real-time combat feel spontaneous and fun?
// Date: 2026-08-26

using UnityEngine;

/// <summary>
/// Prototype-only player controller: moves via legacy Input Manager (WASD or arrow keys),
/// auto-targets the nearest living dummy within range, and casts one of three elemental
/// spells on number-key press. FPS mouse-aim is deliberately skipped for this prototype —
/// auto-targeting isolates the "does the synergy feel fun" question from aiming precision.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerSpellController : MonoBehaviour
{
    public enum PlayerId { One, Two }

    [Header("Identity")]
    public PlayerId playerId = PlayerId.One;
    public string PlayerLabel => playerId == PlayerId.One ? "Player 1" : "Player 2";

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;

    [Header("Casting")]
    public float castRange = 12f;
    public Transform castOrigin;
    public GameObject fireProjectileTemplate;
    public GameObject waterProjectileTemplate;
    public GameObject lightningProjectileTemplate;

    [Header("Cooldowns (seconds)")]
    public float fireCooldown = 1.0f;
    public float waterCooldown = 1.2f;
    public float lightningCooldown = 1.5f;

    float _fireTimer;
    float _waterTimer;
    float _lightningTimer;
    CharacterController _controller;

    public float FireCooldownRemaining => Mathf.Max(0f, _fireTimer);
    public float WaterCooldownRemaining => Mathf.Max(0f, _waterTimer);
    public float LightningCooldownRemaining => Mathf.Max(0f, _lightningTimer);

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        TickCooldowns();
        HandleMovement();
        HandleCastInput();
    }

    void TickCooldowns()
    {
        if (_fireTimer > 0f) _fireTimer -= Time.deltaTime;
        if (_waterTimer > 0f) _waterTimer -= Time.deltaTime;
        if (_lightningTimer > 0f) _lightningTimer -= Time.deltaTime;
    }

    void HandleMovement()
    {
        float h = 0f;
        float v = 0f;

        if (playerId == PlayerId.One)
        {
            h = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
            v = Input.GetKey(KeyCode.S) ? -1f : Input.GetKey(KeyCode.W) ? 1f : 0f;
        }
        else
        {
            h = Input.GetKey(KeyCode.LeftArrow) ? -1f : Input.GetKey(KeyCode.RightArrow) ? 1f : 0f;
            v = Input.GetKey(KeyCode.DownArrow) ? -1f : Input.GetKey(KeyCode.UpArrow) ? 1f : 0f;
        }

        Vector3 move = new Vector3(h, 0f, v);
        if (move.sqrMagnitude > 0.001f)
        {
            move.Normalize();
            _controller.SimpleMove(move * moveSpeed);

            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
        else
        {
            _controller.SimpleMove(Vector3.zero);
        }
    }

    void HandleCastInput()
    {
        bool castFire, castWater, castLightning;

        if (playerId == PlayerId.One)
        {
            castFire = Input.GetKeyDown(KeyCode.Alpha1);
            castWater = Input.GetKeyDown(KeyCode.Alpha2);
            castLightning = Input.GetKeyDown(KeyCode.Alpha3);
        }
        else
        {
            castFire = Input.GetKeyDown(KeyCode.Keypad1);
            castWater = Input.GetKeyDown(KeyCode.Keypad2);
            castLightning = Input.GetKeyDown(KeyCode.Keypad3);
        }

        if (castFire) TryCast(ElementalType.Fire);
        else if (castWater) TryCast(ElementalType.Water);
        else if (castLightning) TryCast(ElementalType.Lightning);
    }

    void TryCast(ElementalType type)
    {
        switch (type)
        {
            case ElementalType.Fire:
                if (_fireTimer > 0f) return;
                break;
            case ElementalType.Water:
                if (_waterTimer > 0f) return;
                break;
            case ElementalType.Lightning:
                if (_lightningTimer > 0f) return;
                break;
        }

        EnemyDummy target = FindNearestEnemy();
        if (target == null) return;

        // Debug aid: auto-targeting picks "nearest to caster" independently per cast,
        // so it can pick a different dummy than a previous cast if the player moved.
        // This log lets us verify whether Water and Lightning actually landed on the same target.
        Debug.Log($"[TARGET] {PlayerLabel} casts {type} at {target.name} (Wet={target.IsWet})");

        GameObject template = type switch
        {
            ElementalType.Fire => fireProjectileTemplate,
            ElementalType.Water => waterProjectileTemplate,
            ElementalType.Lightning => lightningProjectileTemplate,
            _ => null
        };
        if (template == null) return;

        Vector3 spawnPos = castOrigin != null ? castOrigin.position : transform.position + Vector3.up;
        GameObject proj = Instantiate(template, spawnPos, Quaternion.identity);
        proj.SetActive(true); // template is stored inactive in the scene — must activate the clone
        proj.GetComponent<SpellProjectile>().Launch(target, type, PlayerLabel);

        switch (type)
        {
            case ElementalType.Fire: _fireTimer = fireCooldown; break;
            case ElementalType.Water: _waterTimer = waterCooldown; break;
            case ElementalType.Lightning: _lightningTimer = lightningCooldown; break;
        }
    }

    EnemyDummy FindNearestEnemy()
    {
        EnemyDummy[] all = Object.FindObjectsByType<EnemyDummy>(FindObjectsSortMode.None);
        EnemyDummy best = null;
        float bestDist = castRange;

        foreach (var dummy in all)
        {
            if (dummy.IsDead) continue;
            float d = Vector3.Distance(transform.position, dummy.transform.position);
            if (d <= bestDist)
            {
                bestDist = d;
                best = dummy;
            }
        }

        return best;
    }
}
