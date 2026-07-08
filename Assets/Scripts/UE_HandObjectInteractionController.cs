using System;
using System.Collections.Generic;
using UnityEngine;

public class UE_HandObjectInteractionController : MonoBehaviour
{
    public enum InteractionKind
    {
        Grabbable,
        Socket,
        Breakable,
        Button,
        PalmPanel
    }

    [Serializable]
    private class InteractableEntry
    {
        public string objectId = string.Empty;
        public Transform target = null;
        public Collider collider = null;
        public InteractionKind kind = InteractionKind.Grabbable;
        public UE_HandPlayroomProgressFeedbackController.CompletionItem completionItem = UE_HandPlayroomProgressFeedbackController.CompletionItem.CubeGrabbed;
        public Transform matchingSocket = null;
        [Min(0.05f)] public float placementRadius = 0.75f;
        public Vector3 placedOffset = new Vector3(0f, 0.25f, 0f);
    }

    private readonly struct ObjectHome
    {
        public ObjectHome(Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            Parent = parent;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = localScale;
        }

        public Transform Parent { get; }
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }
        public Vector3 LocalScale { get; }
    }

    [Header("References")]
    [SerializeField] private UE_HandTrackingGestureController handTrackingController;
    [SerializeField] private UE_HandPlayerController playerController;
    [SerializeField] private UE_HandPlayroomProgressFeedbackController progressFeedbackController;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Transform holdPreviewPoint;

    [Header("Aim")]
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField, Min(0.1f)] private float raycastDistance = 4f;
    [SerializeField, Min(0.1f)] private float holdDistance = 1f;
    [SerializeField, Range(0f, 1f)] private float minimumRightHandX = 0.5f;

    [Header("Held Object Control")]
    [SerializeField] private Vector2 heldObjectNeutralCenter = new Vector2(0.75f, 0.5f);
    [SerializeField, Min(0.01f)] private float heldObjectInputRange = 0.32f;
    [SerializeField, Min(0f)] private float heldObjectDeadZone = 0.12f;
    [SerializeField, Min(0f)] private float heldObjectMoveSpeed = 1.6f;
    [SerializeField, Min(0f)] private float heldObjectGrabInputDelay = 0.2f;
    [SerializeField, Min(0f)] private float heldObjectHorizontalLimit = 0.55f;
    [SerializeField, Min(0f)] private float heldObjectVerticalLimit = 0.35f;
    [SerializeField, Min(0.1f)] private float minimumHeldObjectDistance = 0.9f;

    [Header("Safety")]
    [SerializeField] private Vector3 roomBoundsCenter = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 roomBoundsSize = new Vector3(12f, 5f, 9f);
    [SerializeField] private float dropFloorY = -0.75f;

    [Header("Physics")]
    [SerializeField] private bool enableGrabbablePhysics = true;
    [SerializeField, Min(0.01f)] private float grabbableMass = 1f;
    [SerializeField] private bool createRuntimeFloorCollider = true;
    [SerializeField] private float runtimeFloorY = 0f;
    [SerializeField, Min(0.01f)] private float runtimeFloorThickness = 0.2f;

    [Header("Visual Guide")]
    [SerializeField] private bool showInteractableOutlines = true;
    [SerializeField] private Color interactableOutlineColor = new Color(0.65f, 1f, 0.25f, 0.95f);
    [SerializeField] private Color pinchReadyOutlineColor = new Color(1f, 0.18f, 0.12f, 0.95f);
    [SerializeField, Min(0.001f)] private float interactableOutlineWidth = 0.025f;

    [Header("Interactables")]
    [SerializeField] private InteractableEntry[] interactables = Array.Empty<InteractableEntry>();

    public event Action<string> InteractionCompleted;

    private readonly Dictionary<Transform, ObjectHome> homes = new Dictionary<Transform, ObjectHome>();
    private readonly Dictionary<Transform, Rigidbody> rigidbodies = new Dictionary<Transform, Rigidbody>();
    private readonly HashSet<Transform> placedObjects = new HashSet<Transform>();
    private readonly HashSet<string> completedObjectIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private InteractableEntry aimedEntry;
    private InteractableEntry heldEntry;
    private InteractableEntry pinchReadyEntry;
    private Vector3 heldLocalOffset;
    private float heldGrabTime;
    private bool wasPinching;
    private bool wasFist;

    public Transform CurrentAimedObject { get; private set; }
    public Transform HeldObject => heldEntry != null ? heldEntry.target : null;
    public string CurrentAimedObjectId => aimedEntry != null ? aimedEntry.objectId : string.Empty;

    private void Awake()
    {
        CacheObjectHomes();
        EnsureGrabbablePhysics();
        EnsureRuntimeFloorCollider();
        EnsureInteractableOutlines();
    }

    private void Update()
    {
        if (handTrackingController == null)
        {
            return;
        }

        UE_HandTrackingGestureController.HandSnapshot rightHand = handTrackingController.RightHand;

        if (!rightHand.IsTracked || !IsInRightInteractionArea(rightHand))
        {
            ClearAim();
            SetPinchReadyEntry(null);
            ReleaseHeldObject(false);
            wasPinching = false;
            wasFist = false;
            return;
        }

        UpdateAimedObject();
        SetPinchReadyEntry(CanShowReadyAction(aimedEntry) ? aimedEntry : null);
        UpdateActionPrompt();
        UpdateHeldObjectPreview(rightHand);
        HandlePinch(rightHand.IsPinching);
        HandleFist(rightHand.IsFist);
        HandlePalm(rightHand.IsPalmHeld);
        ResetUnsafeObjects();
    }

    private bool IsInRightInteractionArea(UE_HandTrackingGestureController.HandSnapshot rightHand)
    {
        return rightHand.Center.x >= minimumRightHandX;
    }

    public void ResetObject(string objectId)
    {
        InteractableEntry entry = FindById(objectId);

        if (entry != null)
        {
            ResetEntry(entry);
        }
    }

    public void ResetAllObjects()
    {
        foreach (InteractableEntry entry in interactables)
        {
            ResetEntry(entry);
        }

        heldEntry = null;
        placedObjects.Clear();
    }

    private void CacheObjectHomes()
    {
        homes.Clear();

        foreach (InteractableEntry entry in interactables)
        {
            if (entry?.target == null || homes.ContainsKey(entry.target))
            {
                continue;
            }

            homes.Add(entry.target, new ObjectHome(
                entry.target.parent,
                entry.target.localPosition,
                entry.target.localRotation,
                entry.target.localScale));
        }
    }

    private void EnsureInteractableOutlines()
    {
        if (!showInteractableOutlines)
        {
            return;
        }

        foreach (InteractableEntry entry in interactables)
        {
            if (entry?.target == null || entry.collider == null)
            {
                continue;
            }

            UE_InteractableOutline outline = entry.target.GetComponent<UE_InteractableOutline>();

            if (outline == null)
            {
                outline = entry.target.gameObject.AddComponent<UE_InteractableOutline>();
            }

            outline.Initialize(entry.collider, interactableOutlineColor, interactableOutlineWidth);
        }
    }

    private void EnsureGrabbablePhysics()
    {
        rigidbodies.Clear();

        if (!enableGrabbablePhysics)
        {
            return;
        }

        foreach (InteractableEntry entry in interactables)
        {
            if (entry?.target == null || entry.kind != InteractionKind.Grabbable)
            {
                continue;
            }

            Rigidbody rigidbody = entry.target.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                rigidbody = entry.target.gameObject.AddComponent<Rigidbody>();
            }

            rigidbody.mass = grabbableMass;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbodies[entry.target] = rigidbody;
        }
    }

    private void EnsureRuntimeFloorCollider()
    {
        if (!createRuntimeFloorCollider)
        {
            return;
        }

        GameObject floorObject = new GameObject("UE_RuntimeRoomFloorCollider");
        floorObject.transform.SetParent(transform, false);
        floorObject.transform.position = new Vector3(roomBoundsCenter.x, runtimeFloorY - runtimeFloorThickness * 0.5f, roomBoundsCenter.z);

        BoxCollider floorCollider = floorObject.AddComponent<BoxCollider>();
        floorCollider.size = new Vector3(Mathf.Max(0.1f, roomBoundsSize.x), runtimeFloorThickness, Mathf.Max(0.1f, roomBoundsSize.z));
        floorCollider.isTrigger = false;
    }

    private void UpdateAimedObject()
    {
        Transform origin = GetRaycastOrigin();
        Ray ray = new Ray(origin.position, origin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayers, QueryTriggerInteraction.Collide))
        {
            SetAim(FindByCollider(hit.collider));
            return;
        }

        ClearAim();
    }

    private Transform GetRaycastOrigin()
    {
        if (raycastOrigin != null)
        {
            return raycastOrigin;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return transform;
    }

    private void SetAim(InteractableEntry entry)
    {
        if (aimedEntry == entry)
        {
            return;
        }

        aimedEntry = entry;
        CurrentAimedObject = entry != null ? entry.target : null;

        UpdateActionPrompt();
    }

    private void ClearAim()
    {
        SetAim(null);
    }

    private void HandlePinch(bool isPinching)
    {
        if (isPinching && heldEntry == null)
        {
            TryGrabAimedObject();
        }
        else if (!isPinching && wasPinching && heldEntry != null)
        {
            TryPlaceHeldObject();
        }

        wasPinching = isPinching;
    }

    private void HandleFist(bool isFist)
    {
        if (isFist && !wasFist)
        {
            TryFistAction();
        }

        wasFist = isFist;
    }

    private void HandlePalm(bool isPalmHeld)
    {
        if (!isPalmHeld || aimedEntry == null || aimedEntry.kind != InteractionKind.PalmPanel)
        {
            return;
        }

        CompleteInteraction(aimedEntry);
        progressFeedbackController?.ShowSuccess("Palm authentication accepted.");
    }

    private void TryGrabAimedObject()
    {
        if (!CanPinchGrabAimedObject())
        {
            return;
        }

        heldEntry = aimedEntry;
        placedObjects.Remove(heldEntry.target);
        SetGrabbablePhysicsHeld(heldEntry);
        heldEntry.target.position = ResolveDefaultHeldPosition();
        heldLocalOffset = ClampHeldLocalOffset(GetRaycastOrigin().InverseTransformPoint(heldEntry.target.position));
        heldGrabTime = Time.time;
        CompleteInteraction(heldEntry);
        progressFeedbackController?.ShowSuccess($"Grabbed {heldEntry.objectId}.");
    }

    private bool CanPinchGrabAimedObject()
    {
        return heldEntry == null && aimedEntry != null && aimedEntry.kind == InteractionKind.Grabbable;
    }

    private bool CanShowReadyAction(InteractableEntry entry)
    {
        return entry != null && (entry.kind == InteractionKind.Grabbable || entry.kind == InteractionKind.Breakable);
    }

    private void UpdateActionPrompt()
    {
        if (progressFeedbackController == null)
        {
            return;
        }

        progressFeedbackController.ShowInteractionPrompt(ResolveActionLabel(aimedEntry), aimedEntry?.objectId ?? string.Empty);
    }

    private static string ResolveActionLabel(InteractableEntry entry)
    {
        if (entry == null)
        {
            return string.Empty;
        }

        switch (entry.kind)
        {
            case InteractionKind.Grabbable:
                return "GRAB";
            case InteractionKind.Breakable:
                return "BREAK";
            default:
                return string.Empty;
        }
    }

    private void SetPinchReadyEntry(InteractableEntry entry)
    {
        if (pinchReadyEntry == entry)
        {
            return;
        }

        SetOutlineColor(pinchReadyEntry, interactableOutlineColor);
        pinchReadyEntry = entry;
        SetOutlineColor(pinchReadyEntry, pinchReadyOutlineColor);
    }

    private static void SetOutlineColor(InteractableEntry entry, Color color)
    {
        if (entry?.target == null)
        {
            return;
        }

        UE_InteractableOutline outline = entry.target.GetComponent<UE_InteractableOutline>();

        if (outline != null)
        {
            outline.SetOutlineColor(color);
        }
    }

    private void UpdateHeldObjectPreview(UE_HandTrackingGestureController.HandSnapshot rightHand)
    {
        if (heldEntry?.target == null)
        {
            return;
        }

        Transform origin = GetRaycastOrigin();
        Vector3 localOffset = heldLocalOffset;

        if (!heldEntry.target.gameObject.activeInHierarchy)
        {
            localOffset = origin.InverseTransformPoint(ResolveDefaultHeldPosition());
        }

        Vector2 input = ResolveHeldObjectInput(rightHand);

        if (Time.time - heldGrabTime >= heldObjectGrabInputDelay && input != Vector2.zero)
        {
            localOffset += new Vector3(input.x, input.y, 0f) * heldObjectMoveSpeed * Time.deltaTime;
        }

        heldLocalOffset = ClampHeldLocalOffset(localOffset);
        heldEntry.target.position = origin.TransformPoint(heldLocalOffset);
    }

    private Vector3 ResolveDefaultHeldPosition()
    {
        Transform origin = GetRaycastOrigin();

        return holdPreviewPoint != null
            ? holdPreviewPoint.position
            : origin.position + origin.forward * holdDistance;
    }

    private Vector2 ResolveHeldObjectInput(UE_HandTrackingGestureController.HandSnapshot rightHand)
    {
        Vector2 input = new Vector2(
            rightHand.Center.x - heldObjectNeutralCenter.x,
            heldObjectNeutralCenter.y - rightHand.Center.y);

        input /= heldObjectInputRange;
        input = Vector2.ClampMagnitude(input, 1f);

        return input.sqrMagnitude >= heldObjectDeadZone * heldObjectDeadZone ? input : Vector2.zero;
    }

    private Vector3 ClampHeldLocalOffset(Vector3 localOffset)
    {
        localOffset.x = Mathf.Clamp(localOffset.x, -heldObjectHorizontalLimit, heldObjectHorizontalLimit);
        localOffset.y = Mathf.Clamp(localOffset.y, -heldObjectVerticalLimit, heldObjectVerticalLimit);
        localOffset.z = Mathf.Max(localOffset.z, minimumHeldObjectDistance);
        return localOffset;
    }

    private void TryPlaceHeldObject()
    {
        InteractableEntry socket = FindPlacementSocket(heldEntry);

        if (socket == null)
        {
            progressFeedbackController?.ShowSuccess($"Released {heldEntry.objectId}.");
            SetGrabbablePhysicsReleased(heldEntry);
            heldEntry = null;
            heldLocalOffset = Vector3.zero;
            heldGrabTime = 0f;
            return;
        }

        Transform heldTarget = heldEntry.target;
        heldTarget.position = socket.target.position + heldEntry.placedOffset;
        heldTarget.rotation = socket.target.rotation;
        SetGrabbablePhysicsPlaced(heldEntry);
        placedObjects.Add(heldTarget);
        CompleteInteraction(socket);
        CompleteInteraction(heldEntry, UE_HandPlayroomProgressFeedbackController.CompletionItem.CubePlaced);
        progressFeedbackController?.ShowSuccess($"Placed {heldEntry.objectId}.");
        heldEntry = null;
        heldLocalOffset = Vector3.zero;
        heldGrabTime = 0f;
    }

    private InteractableEntry FindPlacementSocket(InteractableEntry grabbed)
    {
        if (grabbed == null)
        {
            return null;
        }

        if (IsMatchingSocket(grabbed, aimedEntry))
        {
            return aimedEntry;
        }

        foreach (InteractableEntry entry in interactables)
        {
            if (IsMatchingSocket(grabbed, entry) && Vector3.Distance(grabbed.target.position, entry.target.position) <= grabbed.placementRadius)
            {
                return entry;
            }
        }

        return null;
    }

    private static bool IsMatchingSocket(InteractableEntry grabbed, InteractableEntry socket)
    {
        if (grabbed == null || socket == null || socket.kind != InteractionKind.Socket || socket.target == null)
        {
            return false;
        }

        return grabbed.matchingSocket == null || grabbed.matchingSocket == socket.target;
    }

    private void TryFistAction()
    {
        if (aimedEntry == null)
        {
            return;
        }

        if (aimedEntry.kind == InteractionKind.Breakable)
        {
            CompleteInteraction(aimedEntry);
            aimedEntry.target.gameObject.SetActive(false);
            progressFeedbackController?.ShowSuccess($"Broke {aimedEntry.objectId}.");
            ClearAim();
            return;
        }

        if (aimedEntry.kind == InteractionKind.Button)
        {
            CompleteInteraction(aimedEntry);
            progressFeedbackController?.ShowSuccess($"{aimedEntry.objectId} activated.");
        }
    }

    private void ReleaseHeldObject(bool snapHome)
    {
        if (heldEntry == null)
        {
            return;
        }

        if (snapHome)
        {
            ResetEntry(heldEntry);
        }
        else
        {
            SetGrabbablePhysicsReleased(heldEntry);
        }

        heldEntry = null;
        heldLocalOffset = Vector3.zero;
        heldGrabTime = 0f;
    }

    private void ResetUnsafeObjects()
    {
        Bounds roomBounds = new Bounds(roomBoundsCenter, roomBoundsSize);

        foreach (InteractableEntry entry in interactables)
        {
            if (entry?.target == null || !entry.target.gameObject.activeSelf || entry.kind != InteractionKind.Grabbable)
            {
                continue;
            }

            if (placedObjects.Contains(entry.target))
            {
                continue;
            }

            Vector3 position = entry.target.position;

            if (position.y < dropFloorY || !roomBounds.Contains(position))
            {
                ResetEntry(entry);
            }
        }
    }

    private void ResetEntry(InteractableEntry entry)
    {
        if (entry?.target == null)
        {
            return;
        }

        if (!homes.TryGetValue(entry.target, out ObjectHome home))
        {
            return;
        }

        entry.target.SetParent(home.Parent, false);
        entry.target.localPosition = home.LocalPosition;
        entry.target.localRotation = home.LocalRotation;
        entry.target.localScale = home.LocalScale;
        entry.target.gameObject.SetActive(true);
        SetGrabbablePhysicsPlaced(entry);
        placedObjects.Remove(entry.target);

        if (heldEntry == entry)
        {
            heldEntry = null;
            heldLocalOffset = Vector3.zero;
            heldGrabTime = 0f;
        }
    }

    private InteractableEntry FindByCollider(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        foreach (InteractableEntry entry in interactables)
        {
            if (entry?.target == null)
            {
                continue;
            }

            if (entry.collider == hitCollider || hitCollider.transform == entry.target || hitCollider.transform.IsChildOf(entry.target))
            {
                return entry;
            }
        }

        return null;
    }

    private InteractableEntry FindById(string objectId)
    {
        foreach (InteractableEntry entry in interactables)
        {
            if (entry != null && string.Equals(entry.objectId, objectId, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }

    private void SetGrabbablePhysicsHeld(InteractableEntry entry)
    {
        if (!TryGetGrabbableRigidbody(entry, out Rigidbody rigidbody))
        {
            return;
        }

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    private void SetGrabbablePhysicsReleased(InteractableEntry entry)
    {
        if (!TryGetGrabbableRigidbody(entry, out Rigidbody rigidbody))
        {
            return;
        }

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
    }

    private void SetGrabbablePhysicsPlaced(InteractableEntry entry)
    {
        if (!TryGetGrabbableRigidbody(entry, out Rigidbody rigidbody))
        {
            return;
        }

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    private bool TryGetGrabbableRigidbody(InteractableEntry entry, out Rigidbody rigidbody)
    {
        rigidbody = null;

        if (!enableGrabbablePhysics || entry?.target == null || entry.kind != InteractionKind.Grabbable)
        {
            return false;
        }

        if (rigidbodies.TryGetValue(entry.target, out rigidbody) && rigidbody != null)
        {
            return true;
        }

        rigidbody = entry.target.GetComponent<Rigidbody>();

        if (rigidbody == null)
        {
            return false;
        }

        rigidbodies[entry.target] = rigidbody;
        return true;
    }

    private void CompleteInteraction(InteractableEntry entry)
    {
        if (entry == null)
        {
            return;
        }

        CompleteInteraction(entry, entry.completionItem);
    }

    private void CompleteInteraction(InteractableEntry entry, UE_HandPlayroomProgressFeedbackController.CompletionItem completionItem)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.objectId))
        {
            return;
        }

        if (!completedObjectIds.Add($"{entry.objectId}:{completionItem}"))
        {
            return;
        }

        progressFeedbackController?.RecordCompletion(completionItem);
        InteractionCompleted?.Invoke(entry.objectId);
    }
}
