using UnityEngine;

public class UE_HandPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform forwardMarker;
    [SerializeField] private Transform cameraRig;
    [SerializeField] private UE_HandTrackingGestureController handTrackingController;
    [SerializeField] private UE_HandPlayroomFlowController flowController;
    [SerializeField] private UE_HandPlayroomProgressFeedbackController progressFeedbackController;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 1.1f;
    [SerializeField, Min(0f)] private float rotationSpeed = 35f;
    [SerializeField, Min(0f)] private float lookPitchSensitivity = 18f;
    [SerializeField, Min(0f)] private float inputDeadZone = 0.18f;
    [SerializeField] private Vector2 neutralHandCenter = new Vector2(0.25f, 0.5f);
    [SerializeField, Min(0.01f)] private float normalizedInputRange = 0.42f;
    [SerializeField, Range(0f, 1f)] private float maximumLeftHandX = 0.5f;
    [SerializeField] private bool rotateWithLeftHandX = true;
    [SerializeField] private bool requirePlayingState;

    [Header("Fist Look")]
    [SerializeField, Min(0f)] private float fistLookYawSensitivity = 45f;
    [SerializeField, Min(0f)] private float fistMovementSuppressionTime = 0.25f;

    [Header("Bounds")]
    [SerializeField] private Vector3 movementAreaCenter = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 movementAreaSize = new Vector3(12f, 0f, 8f);

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float cameraPitch;
    private float lastLeftFistTime = float.NegativeInfinity;
    private bool warnedMissingReferences;

    private Transform EffectivePlayerTransform => playerTransform != null ? playerTransform : transform;

    private void Awake()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        startPosition = EffectivePlayerTransform.position;
        startRotation = EffectivePlayerTransform.rotation;
    }

    private void Update()
    {
        if (!CanMove())
        {
            return;
        }

        UE_HandTrackingGestureController.HandSnapshot leftHand = handTrackingController.LeftHand;

        if (!leftHand.IsTracked || !IsInLeftMovementArea(leftHand))
        {
            return;
        }

        Vector2 input = ResolveMovementInput(leftHand);

        if (leftHand.IsFist)
        {
            lastLeftFistTime = Time.time;
            ApplyFistLook(input);
            return;
        }

        if (Time.time - lastLeftFistTime <= fistMovementSuppressionTime)
        {
            return;
        }

        if (input == Vector2.zero)
        {
            return;
        }

        ApplyMovement(new Vector2(0f, input.y));
    }

    public void ResetToStart()
    {
        Transform target = EffectivePlayerTransform;
        target.SetPositionAndRotation(startPosition, startRotation);
        cameraPitch = 0f;

        if (cameraRig != null)
        {
            cameraRig.localRotation = Quaternion.identity;
        }
    }

    public void RecordCurrentPositionAsStart()
    {
        startPosition = EffectivePlayerTransform.position;
        startRotation = EffectivePlayerTransform.rotation;
        cameraPitch = 0f;
    }

    private bool CanMove()
    {
        if (handTrackingController == null)
        {
            WarnMissingReferences();
            return false;
        }

        if (requirePlayingState
            && flowController != null
            && flowController.CurrentState != UE_HandPlayroomFlowController.ExperienceState.Playing)
        {
            return false;
        }

        return true;
    }

    private void ApplyRotation(float horizontalInput)
    {
        if (!rotateWithLeftHandX || Mathf.Approximately(horizontalInput, 0f))
        {
            return;
        }

        Transform target = EffectivePlayerTransform;
        float yaw = horizontalInput * rotationSpeed * Time.deltaTime;
        target.Rotate(Vector3.up, yaw, Space.World);
    }

    private void ApplyMovement(Vector2 input)
    {
        Transform target = EffectivePlayerTransform;
        Vector3 forward = ResolveForward();
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        if (rotateWithLeftHandX)
        {
            right = Vector3.zero;
        }

        Vector3 delta = ((forward * input.y) + (right * input.x)) * moveSpeed * Time.deltaTime;
        target.position = ClampToMovementArea(target.position + delta);
    }

    private void ApplyLookPitch(float verticalInput)
    {
        if (cameraRig == null || Mathf.Approximately(verticalInput, 0f))
        {
            return;
        }

        cameraPitch = Mathf.Clamp(cameraPitch - verticalInput * lookPitchSensitivity * Time.deltaTime, -45f, 45f);
        cameraRig.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void ApplyFistLook(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            return;
        }

        if (!Mathf.Approximately(input.x, 0f))
        {
            Transform target = EffectivePlayerTransform;
            target.Rotate(Vector3.up, input.x * fistLookYawSensitivity * Time.deltaTime, Space.World);
        }

        ApplyLookPitch(input.y);
    }

    private Vector3 ResolveForward()
    {
        if (forwardMarker != null)
        {
            Vector3 markerForward = Vector3.ProjectOnPlane(forwardMarker.forward, Vector3.up);

            if (markerForward.sqrMagnitude > 0.0001f)
            {
                return markerForward.normalized;
            }
        }

        Vector3 playerForward = Vector3.ProjectOnPlane(EffectivePlayerTransform.forward, Vector3.up);
        return playerForward.sqrMagnitude > 0.0001f ? playerForward.normalized : Vector3.forward;
    }

    private Vector3 ClampToMovementArea(Vector3 position)
    {
        Vector3 halfSize = movementAreaSize * 0.5f;

        if (movementAreaSize.x > 0f)
        {
            position.x = Mathf.Clamp(position.x, movementAreaCenter.x - halfSize.x, movementAreaCenter.x + halfSize.x);
        }

        if (movementAreaSize.z > 0f)
        {
            position.z = Mathf.Clamp(position.z, movementAreaCenter.z - halfSize.z, movementAreaCenter.z + halfSize.z);
        }

        return position;
    }

    private Vector2 ApplyDeadZone(Vector2 value)
    {
        return value.sqrMagnitude >= inputDeadZone * inputDeadZone ? value : Vector2.zero;
    }

    private Vector2 ResolveMovementInput(UE_HandTrackingGestureController.HandSnapshot leftHand)
    {
        Vector2 input = new Vector2(
            leftHand.Center.x - neutralHandCenter.x,
            neutralHandCenter.y - leftHand.Center.y);

        input /= normalizedInputRange;
        input = Vector2.ClampMagnitude(input, 1f);

        return ApplyDeadZone(input);
    }

    private bool IsInLeftMovementArea(UE_HandTrackingGestureController.HandSnapshot leftHand)
    {
        return leftHand.Center.x <= maximumLeftHandX;
    }

    private void WarnMissingReferences()
    {
        if (warnedMissingReferences)
        {
            return;
        }

        warnedMissingReferences = true;
        Debug.LogWarning($"{nameof(UE_HandPlayerController)} is missing required references and will stay idle.", this);
    }
}
