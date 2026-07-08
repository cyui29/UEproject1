using System;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;

public class UE_MediaPipeHandTrackingAdapter : MonoBehaviour
{
    private const int Wrist = 0;
    private const int ThumbTip = 4;
    private const int IndexMcp = 5;
    private const int IndexPip = 6;
    private const int IndexTip = 8;
    private const int MiddleMcp = 9;
    private const int MiddlePip = 10;
    private const int MiddleTip = 12;
    private const int RingPip = 14;
    private const int RingTip = 16;
    private const int PinkyMcp = 17;
    private const int PinkyPip = 18;
    private const int PinkyTip = 20;
    private const int RequiredLandmarkCount = 21;

    [SerializeField] private UE_HandTrackingGestureController handTrackingController;
    [SerializeField] private bool swapHands;
    [SerializeField, Min(0.01f)] private float pinchDistance = 0.38f;
    [SerializeField, Min(0.001f)] private float minimumHandScale = 0.04f;
    [SerializeField, Min(1f)] private float curledFingerRatio = 1.15f;
    [SerializeField, Range(0f, 1f)] private float fallbackConfidence = 0.75f;

    private readonly object pendingLock = new object();
    private PendingHandState pendingLeft;
    private PendingHandState pendingRight;
    private bool hasPendingResult;

    private struct PendingHandState
    {
        public bool seen;
        public Vector2 center;
        public Vector2[] landmarks;
        public float confidence;
        public bool pinchDetected;
        public bool fistDetected;
        public bool palmDetected;
    }

    private void Update()
    {
        if (handTrackingController == null)
        {
            return;
        }

        PendingHandState left;
        PendingHandState right;
        bool shouldApply;

        lock (pendingLock)
        {
            shouldApply = hasPendingResult;
            left = pendingLeft;
            right = pendingRight;
            hasPendingResult = false;
        }

        if (!shouldApply)
        {
            return;
        }

        ApplyPendingHand(UE_HandTrackingGestureController.HandRole.Left, left);
        ApplyPendingHand(UE_HandTrackingGestureController.HandRole.Right, right);
    }

    public void Apply(HandLandmarkerResult result)
    {
        PendingHandState left = default;
        PendingHandState right = default;

        if (result.handLandmarks != null)
        {
            int count = result.handLandmarks.Count;

            for (int i = 0; i < count; i++)
            {
                IReadOnlyList<NormalizedLandmark> landmarks = result.handLandmarks[i].landmarks;

                if (landmarks == null || landmarks.Count < RequiredLandmarkCount)
                {
                    continue;
                }

                UE_HandTrackingGestureController.HandRole handRole = ResolveHandRole(result, i);
                Vector2 center = CalculateCenter(landmarks);
                float confidence = ResolveConfidence(result, i);
                bool pinchDetected = IsPinching(landmarks);
                bool fistDetected = IsFist(landmarks);
                bool palmDetected = IsPalmOpen(landmarks);

                PendingHandState pendingHandState = new PendingHandState
                {
                    seen = true,
                    center = center,
                    landmarks = CopyLandmarks(landmarks),
                    confidence = confidence,
                    pinchDetected = pinchDetected,
                    fistDetected = fistDetected,
                    palmDetected = palmDetected
                };

                if (handRole == UE_HandTrackingGestureController.HandRole.Left)
                {
                    left = pendingHandState;
                }
                else
                {
                    right = pendingHandState;
                }
            }
        }

        lock (pendingLock)
        {
            pendingLeft = left;
            pendingRight = right;
            hasPendingResult = true;
        }
    }

    public void SetSwapHands(bool enabled)
    {
        swapHands = enabled;
    }

    private void ApplyPendingHand(UE_HandTrackingGestureController.HandRole handRole, PendingHandState handState)
    {
        if (!handState.seen)
        {
            handTrackingController.SetHandLost(handRole);
            return;
        }

        handTrackingController.SubmitMediaPipeHandState(
            handRole,
            handState.center,
            handState.landmarks,
            handState.confidence,
            handState.pinchDetected,
            handState.fistDetected,
            handState.palmDetected);
    }

    private UE_HandTrackingGestureController.HandRole ResolveHandRole(HandLandmarkerResult result, int index)
    {
        string handedness = ResolveHandednessLabel(result, index);
        bool isLeft = string.Equals(handedness, "Left", StringComparison.OrdinalIgnoreCase);
        bool isRight = string.Equals(handedness, "Right", StringComparison.OrdinalIgnoreCase);

        UE_HandTrackingGestureController.HandRole handRole = isLeft && !isRight
            ? UE_HandTrackingGestureController.HandRole.Left
            : UE_HandTrackingGestureController.HandRole.Right;

        if (swapHands)
        {
            handRole = handRole == UE_HandTrackingGestureController.HandRole.Left
                ? UE_HandTrackingGestureController.HandRole.Right
                : UE_HandTrackingGestureController.HandRole.Left;
        }

        return handRole;
    }

    private static string ResolveHandednessLabel(HandLandmarkerResult result, int index)
    {
        if (result.handedness == null || index >= result.handedness.Count)
        {
            return string.Empty;
        }

        List<Category> categories = result.handedness[index].categories;

        if (categories == null || categories.Count == 0)
        {
            return string.Empty;
        }

        return categories[0].categoryName;
    }

    private float ResolveConfidence(HandLandmarkerResult result, int index)
    {
        if (result.handedness == null || index >= result.handedness.Count)
        {
            return fallbackConfidence;
        }

        List<Category> categories = result.handedness[index].categories;

        if (categories == null || categories.Count == 0)
        {
            return fallbackConfidence;
        }

        return Clamp01(categories[0].score);
    }

    private static Vector2 CalculateCenter(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        Vector2 total = Vector2.zero;

        for (int i = 0; i < RequiredLandmarkCount; i++)
        {
            total += ToVector2(landmarks[i]);
        }

        return total / RequiredLandmarkCount;
    }

    private static Vector2[] CopyLandmarks(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        Vector2[] copiedLandmarks = new Vector2[RequiredLandmarkCount];

        for (int i = 0; i < RequiredLandmarkCount; i++)
        {
            copiedLandmarks[i] = ToVector2(landmarks[i]);
        }

        return copiedLandmarks;
    }

    private bool IsPinching(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        float handScale = ResolveHandScale(landmarks);
        float pinchRatio = Vector2.Distance(ToVector2(landmarks[ThumbTip]), ToVector2(landmarks[IndexTip])) / handScale;

        return pinchRatio <= pinchDistance;
    }

    private float ResolveHandScale(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        float palmWidth = Vector2.Distance(ToVector2(landmarks[IndexMcp]), ToVector2(landmarks[PinkyMcp]));
        float palmLength = Vector2.Distance(ToVector2(landmarks[Wrist]), ToVector2(landmarks[MiddleMcp]));

        return Mathf.Max(minimumHandScale, palmWidth, palmLength);
    }

    private bool IsFist(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        int curledFingers = CountCurledFingers(landmarks);
        return curledFingers >= 3;
    }

    private bool IsPalmOpen(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        int curledFingers = CountCurledFingers(landmarks);
        return curledFingers <= 1 && !IsPinching(landmarks);
    }

    private int CountCurledFingers(IReadOnlyList<NormalizedLandmark> landmarks)
    {
        int curledFingers = 0;

        if (IsFingerCurled(landmarks, IndexPip, IndexTip))
        {
            curledFingers++;
        }

        if (IsFingerCurled(landmarks, MiddlePip, MiddleTip))
        {
            curledFingers++;
        }

        if (IsFingerCurled(landmarks, RingPip, RingTip))
        {
            curledFingers++;
        }

        if (IsFingerCurled(landmarks, PinkyPip, PinkyTip))
        {
            curledFingers++;
        }

        return curledFingers;
    }

    private bool IsFingerCurled(IReadOnlyList<NormalizedLandmark> landmarks, int pipIndex, int tipIndex)
    {
        Vector2 wrist = ToVector2(landmarks[Wrist]);
        float pipDistance = Vector2.Distance(wrist, ToVector2(landmarks[pipIndex]));
        float tipDistance = Vector2.Distance(wrist, ToVector2(landmarks[tipIndex]));

        return tipDistance <= pipDistance * curledFingerRatio;
    }

    private static Vector2 ToVector2(NormalizedLandmark landmark)
    {
        return new Vector2(landmark.x, landmark.y);
    }

    private static float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        if (value > 1f)
        {
            return 1f;
        }

        return value;
    }
}
