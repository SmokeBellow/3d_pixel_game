// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10

using System.Collections;
using UnityEngine;

/// <summary>
/// Stationary hit-reaction target. On hit: flashes color, gets knocked back via Rigidbody,
/// and slowly resets position so it can be hit again. No health/death — this prototype only
/// tests whether a single hit FEELS good, not damage systems.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TargetDummy : MonoBehaviour
{
    public Color flashColor = Color.white;
    public float flashDuration = 0.08f;
    public float resetDelay = 0.6f;
    public float resetSpeed = 4f;

    Rigidbody _rb;
    Renderer _renderer;
    Color _originalColor;
    Vector3 _homePosition;
    Coroutine _flashRoutine;
    Coroutine _resetRoutine;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
        _homePosition = transform.position;
    }

    public void OnHit(Vector3 direction, float knockbackForce, int comboStep)
    {
        // Knockback scales with combo step so the finisher reads as heavier than the opener.
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(direction * knockbackForce + Vector3.up * (knockbackForce * 0.25f), ForceMode.Impulse);

        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());

        if (_resetRoutine != null) StopCoroutine(_resetRoutine);
        _resetRoutine = StartCoroutine(ResetRoutine());
    }

    IEnumerator FlashRoutine()
    {
        _renderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        _renderer.material.color = _originalColor;
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(resetDelay);
        while (Vector3.Distance(transform.position, _homePosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _homePosition, resetSpeed * Time.deltaTime);
            _rb.linearVelocity = Vector3.zero;
            yield return null;
        }
        transform.position = _homePosition;
    }
}
