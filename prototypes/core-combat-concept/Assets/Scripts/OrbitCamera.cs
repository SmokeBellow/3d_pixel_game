// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10

using UnityEngine;

/// <summary>
/// Minimal third-person orbit/follow camera. Right mouse button held + mouse move orbits;
/// otherwise the camera just follows behind the player. Applies SimpleCameraShake's offset
/// on top so hit feedback reads even though the camera is otherwise locked to the player.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 2.5f;
    public float rotationSpeed = 180f;
    public float followSharpness = 10f;
    public SimpleCameraShake shake;

    float _yaw;

    void Start()
    {
        if (target != null)
            _yaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
            _yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.unscaledDeltaTime;

        Quaternion rot = Quaternion.Euler(0f, _yaw, 0f);
        Vector3 desiredPos = target.position + Vector3.up * height - rot * Vector3.forward * distance;

        Vector3 shakeOffset = shake != null ? shake.CurrentOffset : Vector3.zero;
        transform.position = Vector3.Lerp(transform.position, desiredPos + shakeOffset, followSharpness * Time.unscaledDeltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}
