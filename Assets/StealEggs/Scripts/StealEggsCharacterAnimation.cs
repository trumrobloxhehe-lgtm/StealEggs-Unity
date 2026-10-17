using UnityEngine;

/// <summary>Procedural block-avatar walk, run, idle and jump movement animation.</summary>
public class StealEggsCharacterAnimation : MonoBehaviour
{
    public Transform leftArmPivot;
    public Transform rightArmPivot;
    public Transform leftLegPivot;
    public Transform rightLegPivot;
    public Transform torso;
    public Transform head;
    public float walkCycleSpeed = 9f;
    public float armSwingDegrees = 38f;
    public float legSwingDegrees = 34f;
    public float sprintMultiplier = 1.35f;

    private Quaternion leftArmRest, rightArmRest, leftLegRest, rightLegRest;
    private Vector3 torsoRestPosition, headRestPosition;
    private float cycle, locomotion, verticalSpeed;
    private bool grounded = true, sprinting;

    private void Awake() { CacheRestPose(); }

    public void CacheRestPose()
    {
        if (leftArmPivot != null) leftArmRest = leftArmPivot.localRotation;
        if (rightArmPivot != null) rightArmRest = rightArmPivot.localRotation;
        if (leftLegPivot != null) leftLegRest = leftLegPivot.localRotation;
        if (rightLegPivot != null) rightLegRest = rightLegPivot.localRotation;
        if (torso != null) torsoRestPosition = torso.localPosition;
        if (head != null) headRestPosition = head.localPosition;
    }

    public void SetMotion(float normalizedSpeed, bool isGrounded, float yVelocity, bool isSprinting)
    {
        locomotion = Mathf.Clamp01(normalizedSpeed);
        grounded = isGrounded;
        verticalSpeed = yVelocity;
        sprinting = isSprinting;
    }

    private void Update()
    {
        if (locomotion > 0.04f) cycle += Time.deltaTime * walkCycleSpeed * (sprinting ? sprintMultiplier : 1f);
        float swing = Mathf.Sin(cycle) * locomotion;
        float arm = swing * armSwingDegrees;
        float leg = swing * legSwingDegrees;
        float idle = Mathf.Sin(Time.time * 2.2f) * 0.018f;
        float jumpRaise = !grounded && verticalSpeed > -1f ? -38f : 0f;
        float spread = !grounded ? 12f : 0f;

        if (leftArmPivot != null) leftArmPivot.localRotation = Quaternion.Slerp(leftArmPivot.localRotation, leftArmRest * Quaternion.Euler(arm + jumpRaise, 0f, -spread), 14f * Time.deltaTime);
        if (rightArmPivot != null) rightArmPivot.localRotation = Quaternion.Slerp(rightArmPivot.localRotation, rightArmRest * Quaternion.Euler(-arm + jumpRaise, 0f, spread), 14f * Time.deltaTime);
        if (leftLegPivot != null) leftLegPivot.localRotation = Quaternion.Slerp(leftLegPivot.localRotation, leftLegRest * Quaternion.Euler(-leg, 0f, 0f), 14f * Time.deltaTime);
        if (rightLegPivot != null) rightLegPivot.localRotation = Quaternion.Slerp(rightLegPivot.localRotation, rightLegRest * Quaternion.Euler(leg, 0f, 0f), 14f * Time.deltaTime);
        if (torso != null) torso.localPosition = Vector3.Lerp(torso.localPosition, torsoRestPosition + Vector3.up * (idle + Mathf.Abs(Mathf.Sin(cycle)) * 0.035f * locomotion), 10f * Time.deltaTime);
        if (head != null) head.localPosition = Vector3.Lerp(head.localPosition, headRestPosition + Vector3.up * (idle * 0.65f), 8f * Time.deltaTime);
    }
}
