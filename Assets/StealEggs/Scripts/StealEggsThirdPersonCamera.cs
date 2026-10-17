using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Third-person orbit camera with Roblox-like right-mouse drag controls.</summary>
public class StealEggsThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 focusOffset = new Vector3(0f, 1.35f, 0f);
    public float distance = 7.5f;
    public float minDistance = 4f;
    public float maxDistance = 12f;
    public float mouseSensitivity = 0.16f;
    public float minPitch = -10f;
    public float maxPitch = 65f;
    public float followSmooth = 16f;
    private float yaw;
    private float pitch = 14f;

    private void Start()
    {
        if (target != null) yaw = target.eulerAngles.y;
        UpdateCameraPose(true);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, minPitch, maxPitch);
            }
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
                distance = Mathf.Clamp(distance - scroll * 0.006f, minDistance, maxDistance);
        }
        UpdateCameraPose(false);
    }

    private void UpdateCameraPose(bool instant)
    {
        if (target == null) return;
        Vector3 focus = target.position + focusOffset;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 ray = orbit * Vector3.back * distance;
        Vector3 desiredPosition = focus + ray;
        float length = ray.magnitude;
        if (length > 0.01f && Physics.SphereCast(focus, 0.22f, ray.normalized, out RaycastHit hit, length, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != target && !hit.transform.IsChildOf(target))
                desiredPosition = focus + ray.normalized * Mathf.Max(0.6f, hit.distance - 0.12f);
        }
        if (instant) transform.position = desiredPosition;
        else transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
    }
}
