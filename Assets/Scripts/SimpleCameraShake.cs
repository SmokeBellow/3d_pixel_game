// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10

using System.Collections;
using UnityEngine;

/// <summary>
/// Procedural camera shake using unscaled time so it still plays during hit-stop freezes.
/// Attach to the camera rig (the object OrbitCamera moves), not the Camera itself, so the
/// shake offset composes cleanly with the orbit follow logic.
/// </summary>
public class SimpleCameraShake : MonoBehaviour
{
    Vector3 _shakeOffset;
    Coroutine _routine;

    public Vector3 CurrentOffset => _shakeOffset;

    public void Shake(float intensity, float duration)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            _shakeOffset = Random.insideUnitSphere * intensity * damper;
            yield return null;
        }
        _shakeOffset = Vector3.zero;
    }
}
