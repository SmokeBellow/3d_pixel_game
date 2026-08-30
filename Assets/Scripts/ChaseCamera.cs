// PROTOTYPE - NOT FOR PRODUCTION
// Question: Do cross-player elemental synergies discovered in real-time combat feel spontaneous and fun?
// Date: 2026-08-26

using UnityEngine;

/// <summary>
/// Prototype-only chase camera: lerp-follows a target player from a fixed offset.
/// FPS-style aiming is deliberately out of scope for this prototype (auto-targeting
/// is used instead), so a simple third-person chase cam is enough to see the action.
/// </summary>
public class ChaseCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -6f);
    public float followSpeed = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up);
    }
}
