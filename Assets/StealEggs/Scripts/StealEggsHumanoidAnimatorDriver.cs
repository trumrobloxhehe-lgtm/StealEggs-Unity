using UnityEngine;

/// <summary>
/// Feeds simple movement values into common Humanoid Animator parameter names.
/// The CharacterController/root remains responsible for movement; animation root motion stays disabled.
/// </summary>
[DisallowMultipleComponent]
public sealed class StealEggsHumanoidAnimatorDriver : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private CharacterController characterController;
    [SerializeField, Min(0.5f)] private float runSpeedForFullBlend = 9f;

    private Vector3 previousPosition;

    private void Awake()
    {
        if (targetAnimator == null)
            targetAnimator = GetComponentInChildren<Animator>(true);
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        previousPosition = transform.position;

        if (targetAnimator != null)
            targetAnimator.applyRootMotion = false;
    }

    private void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 displacement = transform.position - previousPosition;
        Vector3 velocity = displacement / dt;
        previousPosition = transform.position;

        bool grounded = characterController == null || characterController.isGrounded;
        bool jumping = !grounded && Mathf.Abs(velocity.y) > 0.08f;
        velocity.y = 0f;

        float normalizedSpeed = Mathf.Clamp01(velocity.magnitude / Mathf.Max(0.5f, runSpeedForFullBlend));

        // Update only parameters that exist on the selected character's controller.
        SetFloatIfExists("Speed", normalizedSpeed);
        SetFloatIfExists("speed", normalizedSpeed);
        SetFloatIfExists("MotionSpeed", normalizedSpeed);
        SetFloatIfExists("MovementSpeed", normalizedSpeed);
        SetFloatIfExists("VelocityZ", normalizedSpeed);

        SetBoolIfExists("IsGrounded", grounded);
        SetBoolIfExists("Grounded", grounded);
        SetBoolIfExists("isGrounded", grounded);
        SetBoolIfExists("IsJumping", jumping);
        SetBoolIfExists("Jumping", jumping);
        SetBoolIfExists("isJumping", jumping);
    }

    private void SetFloatIfExists(string parameterName, float value)
    {
        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
            return;

        foreach (AnimatorControllerParameter parameter in targetAnimator.parameters)
        {
            if (parameter.nameHash == Animator.StringToHash(parameterName) &&
                parameter.type == AnimatorControllerParameterType.Float)
            {
                targetAnimator.SetFloat(parameter.nameHash, value, 0.08f, Time.deltaTime);
                return;
            }
        }
    }

    private void SetBoolIfExists(string parameterName, bool value)
    {
        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
            return;

        foreach (AnimatorControllerParameter parameter in targetAnimator.parameters)
        {
            if (parameter.nameHash == Animator.StringToHash(parameterName) &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                targetAnimator.SetBool(parameter.nameHash, value);
                return;
            }
        }
    }
}
