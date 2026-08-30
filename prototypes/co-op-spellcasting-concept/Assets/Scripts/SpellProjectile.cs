// PROTOTYPE - NOT FOR PRODUCTION
// Question: Do cross-player elemental synergies discovered in real-time combat feel spontaneous and fun?
// Date: 2026-08-26

using UnityEngine;

/// <summary>
/// Prototype-only spell projectile: lerps toward a target dummy and applies an
/// elemental hit on arrival. Self-destructs early if the target dies or is removed
/// before impact.
/// </summary>
public class SpellProjectile : MonoBehaviour
{
    public float travelTime = 0.35f;

    EnemyDummy _target;
    ElementalType _type;
    string _sourcePlayerLabel;
    Vector3 _startPos;
    float _elapsed;

    public void Launch(EnemyDummy target, ElementalType type, string sourcePlayerLabel)
    {
        _target = target;
        _type = type;
        _sourcePlayerLabel = sourcePlayerLabel;
        _startPos = transform.position;
        _elapsed = 0f;
    }

    void Update()
    {
        if (_target == null || _target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / travelTime);
        Vector3 destination = _target.transform.position + Vector3.up;
        transform.position = Vector3.Lerp(_startPos, destination, t);

        if (t >= 1f)
        {
            _target.ApplyHit(_type, _sourcePlayerLabel);
            Destroy(gameObject);
        }
    }
}
