using System;
using System.Collections.Generic;
using UnityEngine;

public class UE_HandTrackingGestureController : MonoBehaviour
{
    public enum HandRole
    {
        Left,
        Right
    }

    public readonly struct HandSnapshot
    {
        public HandSnapshot(bool isTracked, Vector2 center, Vector2 movement, bool isPinching, bool isFist, bool isPalmHeld, float confidence)
        {
            IsTracked = isTracked;
            Center = center;
            Movement = movement;
            IsPinching = isPinching;
            IsFist = isFist;
            IsPalmHeld = isPalmHeld;
            Confidence = confidence;
        }

        public bool IsTracked { get; }
        public Vector2 Center { get; }
        public Vector2 Movement { get; }
        public bool IsPinching { get; }
        public bool IsFist { get; }
        public bool IsPalmHeld { get; }
        public float Confidence { get; }
    }

    [Serializable]
    private class HandState
    {
        public bool isTracked;
        public Vector2 center = new Vector2(0.5f, 0.5f);
        public Vector2 previousCenter = new Vector2(0.5f, 0.5f);
        public readonly Vector2[] landmarks = new Vector2[LandmarkCount];
        public Vector2 movement;
        public bool wantsPinch;
        public bool wantsFist;
        public bool wantsPalm;
        public bool isPinching;
        public bool isFist;
        public bool isPalmHeld;
        public float confidence;
        public float lastSeenTime = -100f;
        public float pinchStartedAt = -1f;
        public float fistStartedAt = -1f;
        public float palmStartedAt = -1f;
    }

    [Header("References")]
    [SerializeField] private UE_HandPlayroomFlowController flowController;
    [SerializeField] private UE_HandPlayroomProgressFeedbackController progressFeedbackController;

    [Header("Tracking")]
    [SerializeField, Range(0f, 1f)] private float centerSmoothing = 0.35f;
    [SerializeField, Min(0f)] private float trackingGraceSeconds = 0.2f;
    [SerializeField, Range(0f, 1f)] private float minimumConfidence = 0.5f;

    [Header("Gesture Hold Times")]
    [SerializeField, Min(0f)] private float pinchHoldSeconds = 0.03f;
    [SerializeField, Min(0f)] private float fistHoldSeconds = 0.18f;
    [SerializeField, Min(0f)] private float palmHoldSeconds = 2f;

    [Header("Mock Defaults")]
    [SerializeField] private Vector2 mockLeftCenter = new Vector2(0.35f, 0.5f);
    [SerializeField] private Vector2 mockRightCenter = new Vector2(0.65f, 0.5f);
    [SerializeField, Range(0f, 1f)] private float mockConfidence = 1f;

    public event Action<HandRole, bool> HandTrackingChanged;
    public event Action HandsReady;
    public event Action HandsLost;

    private readonly HandState leftHand = new HandState();
    private readonly HandState rightHand = new HandState();
    private bool handsWereReady;

    public const int LandmarkCount = 21;
    public HandSnapshot LeftHand => CreateSnapshot(leftHand);
    public HandSnapshot RightHand => CreateSnapshot(rightHand);
    public bool AreBothHandsTracked => leftHand.isTracked && rightHand.isTracked;
    public Vector2 LeftMovement => leftHand.movement;
    public bool IsRightPinching => rightHand.isPinching;
    public bool IsRightFist => rightHand.isFist;
    public bool IsRightPalmHeld => rightHand.isPalmHeld;

    private void Awake()
    {
        PublishHandStatus();
    }

    private void Update()
    {
        RefreshHand(leftHand, HandRole.Left);
        RefreshHand(rightHand, HandRole.Right);
        PublishReadiness();
    }

    public void SubmitMediaPipeHandState(
        HandRole handRole,
        Vector2 normalizedCenter,
        float confidence,
        bool pinchDetected,
        bool fistDetected,
        bool palmDetected)
    {
        SubmitHandState(handRole, normalizedCenter, null, confidence, pinchDetected, fistDetected, palmDetected);
    }

    public void SubmitMediaPipeHandState(
        HandRole handRole,
        Vector2 normalizedCenter,
        IReadOnlyList<Vector2> normalizedLandmarks,
        float confidence,
        bool pinchDetected,
        bool fistDetected,
        bool palmDetected)
    {
        SubmitHandState(handRole, normalizedCenter, normalizedLandmarks, confidence, pinchDetected, fistDetected, palmDetected);
    }

    public void SubmitMockHandState(
        HandRole handRole,
        bool isTracked,
        Vector2 normalizedCenter,
        bool pinchDetected = false,
        bool fistDetected = false,
        bool palmDetected = false)
    {
        if (!isTracked)
        {
            SetHandLost(handRole);
            return;
        }

        SubmitHandState(handRole, normalizedCenter, null, mockConfidence, pinchDetected, fistDetected, palmDetected);
    }

    public void SetMockBothHandsTracked(bool tracked)
    {
        SubmitMockHandState(HandRole.Left, tracked, mockLeftCenter);
        SubmitMockHandState(HandRole.Right, tracked, mockRightCenter);
    }

    public void SetMockRightPinch(bool active)
    {
        SubmitMockHandState(HandRole.Right, true, mockRightCenter, active, rightHand.wantsFist, rightHand.wantsPalm);
    }

    public void SetMockRightFist(bool active)
    {
        SubmitMockHandState(HandRole.Right, true, mockRightCenter, rightHand.wantsPinch, active, rightHand.wantsPalm);
    }

    public void SetMockRightPalm(bool active)
    {
        SubmitMockHandState(HandRole.Right, true, mockRightCenter, rightHand.wantsPinch, rightHand.wantsFist, active);
    }

    public void SetHandLost(HandRole handRole)
    {
        HandState hand = GetHand(handRole);
        hand.lastSeenTime = Time.time - trackingGraceSeconds - 0.001f;
        hand.wantsPinch = false;
        hand.wantsFist = false;
        hand.wantsPalm = false;
        RefreshHand(hand, handRole);
        PublishReadiness();
    }

    public HandSnapshot GetSnapshot(HandRole handRole)
    {
        return CreateSnapshot(GetHand(handRole));
    }

    public bool IsTracked(HandRole handRole)
    {
        return GetHand(handRole).isTracked;
    }

    public Vector2 GetCenter(HandRole handRole)
    {
        return GetHand(handRole).center;
    }

    public bool TryCopyLandmarks(HandRole handRole, Vector2[] target)
    {
        if (target == null || target.Length < LandmarkCount)
        {
            return false;
        }

        HandState hand = GetHand(handRole);

        if (!hand.isTracked)
        {
            return false;
        }

        Array.Copy(hand.landmarks, target, LandmarkCount);
        return true;
    }

    private void SubmitHandState(
        HandRole handRole,
        Vector2 normalizedCenter,
        IReadOnlyList<Vector2> normalizedLandmarks,
        float confidence,
        bool pinchDetected,
        bool fistDetected,
        bool palmDetected)
    {
        HandState hand = GetHand(handRole);
        bool wasTracked = hand.isTracked;

        hand.previousCenter = hand.center;
        hand.center = wasTracked
            ? Vector2.Lerp(hand.center, Clamp01(normalizedCenter), centerSmoothing)
            : Clamp01(normalizedCenter);
        CopyLandmarks(hand, normalizedLandmarks);
        hand.movement = hand.center - hand.previousCenter;
        hand.confidence = Mathf.Clamp01(confidence);
        hand.lastSeenTime = Time.time;
        hand.isTracked = hand.confidence >= minimumConfidence;

        UpdateCandidate(ref hand.wantsPinch, ref hand.pinchStartedAt, pinchDetected && hand.isTracked);
        UpdateCandidate(ref hand.wantsFist, ref hand.fistStartedAt, fistDetected && hand.isTracked);
        UpdateCandidate(ref hand.wantsPalm, ref hand.palmStartedAt, palmDetected && hand.isTracked);
        ApplyGestureHolds(hand);

        if (wasTracked != hand.isTracked)
        {
            HandTrackingChanged?.Invoke(handRole, hand.isTracked);
        }

        PublishHandStatus();
    }

    private void RefreshHand(HandState hand, HandRole handRole)
    {
        bool wasTracked = hand.isTracked;

        if (hand.isTracked && Time.time - hand.lastSeenTime > trackingGraceSeconds)
        {
            hand.isTracked = false;
            hand.movement = Vector2.zero;
            ClearGestures(hand);
        }

        ApplyGestureHolds(hand);

        if (wasTracked != hand.isTracked)
        {
            HandTrackingChanged?.Invoke(handRole, hand.isTracked);
            PublishHandStatus();
        }
    }

    private void PublishReadiness()
    {
        bool handsReady = AreBothHandsTracked;

        if (handsReady == handsWereReady)
        {
            return;
        }

        handsWereReady = handsReady;

        if (handsReady)
        {
            HandsReady?.Invoke();

            if (flowController != null)
            {
                flowController.NotifyHandsReady();
            }
        }
        else
        {
            HandsLost?.Invoke();
        }
    }

    private void PublishHandStatus()
    {
        if (progressFeedbackController != null)
        {
            progressFeedbackController.SetHandStatus(leftHand.isTracked, rightHand.isTracked);
        }
    }

    private void ApplyGestureHolds(HandState hand)
    {
        hand.isPinching = IsHeld(hand.wantsPinch, hand.pinchStartedAt, pinchHoldSeconds);
        hand.isFist = IsHeld(hand.wantsFist, hand.fistStartedAt, fistHoldSeconds);
        hand.isPalmHeld = IsHeld(hand.wantsPalm, hand.palmStartedAt, palmHoldSeconds);
    }

    private static void UpdateCandidate(ref bool candidate, ref float startedAt, bool detected)
    {
        if (detected)
        {
            if (!candidate)
            {
                startedAt = Time.time;
            }

            candidate = true;
            return;
        }

        candidate = false;
        startedAt = -1f;
    }

    private static bool IsHeld(bool candidate, float startedAt, float holdSeconds)
    {
        return candidate && startedAt >= 0f && Time.time - startedAt >= holdSeconds;
    }

    private static void ClearGestures(HandState hand)
    {
        hand.wantsPinch = false;
        hand.wantsFist = false;
        hand.wantsPalm = false;
        hand.isPinching = false;
        hand.isFist = false;
        hand.isPalmHeld = false;
        hand.pinchStartedAt = -1f;
        hand.fistStartedAt = -1f;
        hand.palmStartedAt = -1f;
    }

    private static void CopyLandmarks(HandState hand, IReadOnlyList<Vector2> normalizedLandmarks)
    {
        if (normalizedLandmarks == null || normalizedLandmarks.Count < LandmarkCount)
        {
            for (int i = 0; i < LandmarkCount; i++)
            {
                hand.landmarks[i] = hand.center;
            }

            return;
        }

        for (int i = 0; i < LandmarkCount; i++)
        {
            hand.landmarks[i] = Clamp01(normalizedLandmarks[i]);
        }
    }

    private HandState GetHand(HandRole handRole)
    {
        return handRole == HandRole.Left ? leftHand : rightHand;
    }

    private static HandSnapshot CreateSnapshot(HandState hand)
    {
        return new HandSnapshot(hand.isTracked, hand.center, hand.movement, hand.isPinching, hand.isFist, hand.isPalmHeld, hand.confidence);
    }

    private static Vector2 Clamp01(Vector2 value)
    {
        return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
    }
}
