// PROTOTYPE - NOT FOR PRODUCTION
// Question: Do cross-player elemental synergies discovered in real-time combat feel spontaneous and fun?
// Date: 2026-08-26

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prototype-only enemy dummy: takes elemental hits, tracks the "Wet" status flag,
/// applies bonus damage + chain lightning when Lightning hits a Wet target, and
/// respawns after death so a solo tester can keep testing without resetting the scene.
/// </summary>
public class EnemyDummy : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Elemental Tuning")]
    public float fireDamage = 20f;
    public float waterDamage = 5f;
    public float lightningDamage = 15f;
    public float lightningWetMultiplier = 3f;
    public float wetDuration = 4f;
    public float chainRadius = 4f;
    public int chainMaxTargets = 2;
    public float chainDamage = 15f;

    [Header("Respawn")]
    public float respawnDelay = 2f;

    [Header("Visuals")]
    public Color baseColor = new Color(0.55f, 0.55f, 0.55f);
    public Color wetColor = new Color(0.2f, 0.45f, 0.85f);
    public Color hitFlashColor = Color.white;

    float _health;
    bool _isWet;
    float _wetTimer;
    bool _isDead;
    Renderer _renderer;
    TextMesh _hpLabel;
    Vector3 _homePosition;
    Coroutine _flashRoutine;

    public bool IsDead => _isDead;
    public bool IsWet => _isWet;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _hpLabel = GetComponentInChildren<TextMesh>();
        _homePosition = transform.position;
        _health = maxHealth;
        UpdateVisual();
        UpdateHpLabel();
    }

    void Update()
    {
        if (_isWet)
        {
            _wetTimer -= Time.deltaTime;
            if (_wetTimer <= 0f)
            {
                _isWet = false;
                UpdateVisual();
            }
        }
    }

    public void ApplyHit(ElementalType type, string sourcePlayerLabel)
    {
        if (_isDead) return;

        switch (type)
        {
            case ElementalType.Fire:
                TakeDamage(fireDamage);
                break;

            case ElementalType.Water:
                TakeDamage(waterDamage);
                _isWet = true;
                _wetTimer = wetDuration;
                UpdateVisual();
                break;

            case ElementalType.Lightning:
                bool wasWet = _isWet;
                float dmg = wasWet ? lightningDamage * lightningWetMultiplier : lightningDamage;
                TakeDamage(dmg);
                if (wasWet)
                {
                    Debug.Log($"[SYNERGY] {sourcePlayerLabel} triggered Chain Shock on {name}! ({dmg} dmg)");
                    ChainToNearby();
                }
                break;
        }
    }

    void ChainToNearby()
    {
        EnemyDummy[] all = Object.FindObjectsByType<EnemyDummy>(FindObjectsSortMode.None);
        var candidates = new List<(EnemyDummy dummy, float dist)>();
        foreach (var dummy in all)
        {
            if (dummy == this || dummy.IsDead) continue;
            float d = Vector3.Distance(transform.position, dummy.transform.position);
            if (d <= chainRadius) candidates.Add((dummy, d));
        }
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        int chained = 0;
        foreach (var (dummy, _) in candidates)
        {
            if (chained >= chainMaxTargets) break;
            dummy.TakeDamage(chainDamage);
            dummy.FlashHit();
            chained++;
        }
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        _health -= amount;
        FlashHit();
        UpdateHpLabel();

        if (_health <= 0f)
        {
            Die();
        }
    }

    public void FlashHit()
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        if (_renderer != null) _renderer.material.color = hitFlashColor;
        yield return new WaitForSeconds(0.08f);
        UpdateVisual();
    }

    void Die()
    {
        _isDead = true;
        SetAlive(false);
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        transform.position = _homePosition;
        _health = maxHealth;
        _isWet = false;
        _isDead = false;
        SetAlive(true);
        UpdateVisual();
        UpdateHpLabel();
    }

    // Deactivating the whole GameObject would also suspend this MonoBehaviour's
    // coroutines (including RespawnRoutine itself), so "death" is faked by hiding
    // the renderer/collider/label instead of SetActive(false) on the root object.
    void SetAlive(bool alive)
    {
        if (_renderer != null) _renderer.enabled = alive;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = alive;
        if (_hpLabel != null) _hpLabel.gameObject.SetActive(alive);
    }

    void UpdateVisual()
    {
        if (_renderer == null) return;
        _renderer.material.color = _isWet ? wetColor : baseColor;
    }

    void UpdateHpLabel()
    {
        if (_hpLabel != null) _hpLabel.text = Mathf.CeilToInt(_health).ToString();
    }
}
