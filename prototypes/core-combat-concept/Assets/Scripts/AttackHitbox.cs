// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger volume on the weapon/hand. Armed for a short "active window" per combo step
/// by PlayerCombatController. Hits each target once per active window (no multi-hit spam).
/// </summary>
public class AttackHitbox : MonoBehaviour
{
    public Collider hitboxCollider; // trigger collider on this or a child object
    int _comboStep;
    float _knockback;
    readonly HashSet<Collider> _alreadyHitThisWindow = new HashSet<Collider>();

    void Awake()
    {
        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    public void ArmForStep(int comboStep, float knockback)
    {
        _comboStep = comboStep;
        _knockback = knockback;
        _alreadyHitThisWindow.Clear();
    }

    public void SetActive(bool active)
    {
        if (hitboxCollider.enabled == active) return;
        hitboxCollider.enabled = active;
        if (!active)
            _alreadyHitThisWindow.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_alreadyHitThisWindow.Contains(other)) return;
        var dummy = other.GetComponent<TargetDummy>();
        if (dummy == null) return;

        _alreadyHitThisWindow.Add(other);
        Vector3 hitDirection = (other.transform.position - transform.root.position);
        hitDirection.y = 0f;
        hitDirection.Normalize();

        dummy.OnHit(hitDirection, _knockback, _comboStep);
        HitFeedback.Instance?.PlayHit(_comboStep);
    }
}
