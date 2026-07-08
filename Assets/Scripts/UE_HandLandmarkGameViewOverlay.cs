using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10001)]
public class UE_HandLandmarkGameViewOverlay : MaskableGraphic
{
    private const string MainCanvasName = "UE_MainCanvas";
    private const string OverlayName = "UE_HandLandmarkGameViewOverlay";

    private static readonly int[] Connections =
    {
        0, 1, 1, 2, 2, 3, 3, 4,
        0, 5, 5, 6, 6, 7, 7, 8,
        0, 9, 9, 10, 10, 11, 11, 12,
        0, 13, 13, 14, 14, 15, 15, 16,
        0, 17, 17, 18, 18, 19, 19, 20
    };

    [SerializeField] private UE_HandTrackingGestureController handTrackingController;
    [SerializeField, Min(1f)] private float pointRadius = 7f;
    [SerializeField, Min(1f)] private float lineThickness = 3f;
    [SerializeField] private Color leftHandColor = new Color(0.2f, 0.65f, 1f, 0.95f);
    [SerializeField] private Color rightHandColor = new Color(1f, 0.35f, 0.45f, 0.95f);
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.7f);

    private readonly Vector2[] leftLandmarks = new Vector2[UE_HandTrackingGestureController.LandmarkCount];
    private readonly Vector2[] rightLandmarks = new Vector2[UE_HandTrackingGestureController.LandmarkCount];
    private bool hasWarnedMissingController;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        GameObject mainCanvas = GameObject.Find(MainCanvasName);

        if (mainCanvas == null || mainCanvas.transform.Find(OverlayName) != null)
        {
            return;
        }

        GameObject overlay = new GameObject(OverlayName, typeof(RectTransform));
        overlay.layer = LayerMask.NameToLayer("UI");
        overlay.transform.SetParent(mainCanvas.transform, false);

        RectTransform rectTransform = overlay.GetComponent<RectTransform>();
        StretchToParent(rectTransform);
        rectTransform.SetAsLastSibling();

        UE_HandLandmarkGameViewOverlay graphic = overlay.AddComponent<UE_HandLandmarkGameViewOverlay>();
        graphic.raycastTarget = false;
    }

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;

        if (handTrackingController == null)
        {
            handTrackingController = FindAnyObjectByType<UE_HandTrackingGestureController>();
        }
    }

    private void LateUpdate()
    {
        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform != null)
        {
            StretchToParent(rectTransform);
            rectTransform.SetAsLastSibling();
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        if (handTrackingController == null)
        {
            WarnMissingController();
            return;
        }

        Rect rect = rectTransform.rect;

        if (handTrackingController.TryCopyLandmarks(UE_HandTrackingGestureController.HandRole.Left, leftLandmarks))
        {
            DrawHand(vertexHelper, rect, leftLandmarks, leftHandColor);
        }

        if (handTrackingController.TryCopyLandmarks(UE_HandTrackingGestureController.HandRole.Right, rightLandmarks))
        {
            DrawHand(vertexHelper, rect, rightLandmarks, rightHandColor);
        }
    }

    private void DrawHand(VertexHelper vertexHelper, Rect rect, Vector2[] landmarks, Color pointColor)
    {
        for (int i = 0; i < Connections.Length; i += 2)
        {
            AddLine(
                vertexHelper,
                ToLocalPoint(rect, landmarks[Connections[i]]),
                ToLocalPoint(rect, landmarks[Connections[i + 1]]),
                lineThickness,
                lineColor);
        }

        for (int i = 0; i < landmarks.Length; i++)
        {
            AddQuad(vertexHelper, ToLocalPoint(rect, landmarks[i]), pointRadius * 2f, pointRadius * 2f, pointColor);
        }
    }

    private static Vector2 ToLocalPoint(Rect rect, Vector2 normalizedPoint)
    {
        return new Vector2(
            rect.xMin + normalizedPoint.x * rect.width,
            rect.yMin + (1f - normalizedPoint.y) * rect.height);
    }

    private static void AddLine(VertexHelper vertexHelper, Vector2 start, Vector2 end, float thickness, Color color)
    {
        Vector2 delta = end - start;

        if (delta.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector2 normal = new Vector2(-delta.y, delta.x).normalized * (thickness * 0.5f);
        int index = vertexHelper.currentVertCount;

        vertexHelper.AddVert(start - normal, color, Vector2.zero);
        vertexHelper.AddVert(start + normal, color, Vector2.zero);
        vertexHelper.AddVert(end + normal, color, Vector2.zero);
        vertexHelper.AddVert(end - normal, color, Vector2.zero);
        vertexHelper.AddTriangle(index, index + 1, index + 2);
        vertexHelper.AddTriangle(index, index + 2, index + 3);
    }

    private static void AddQuad(VertexHelper vertexHelper, Vector2 center, float width, float height, Color color)
    {
        Vector2 halfSize = new Vector2(width * 0.5f, height * 0.5f);
        int index = vertexHelper.currentVertCount;

        vertexHelper.AddVert(center + new Vector2(-halfSize.x, -halfSize.y), color, Vector2.zero);
        vertexHelper.AddVert(center + new Vector2(-halfSize.x, halfSize.y), color, Vector2.zero);
        vertexHelper.AddVert(center + new Vector2(halfSize.x, halfSize.y), color, Vector2.zero);
        vertexHelper.AddVert(center + new Vector2(halfSize.x, -halfSize.y), color, Vector2.zero);
        vertexHelper.AddTriangle(index, index + 1, index + 2);
        vertexHelper.AddTriangle(index, index + 2, index + 3);
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }

    private void WarnMissingController()
    {
        if (hasWarnedMissingController)
        {
            return;
        }

        hasWarnedMissingController = true;
        Debug.LogWarning($"{nameof(UE_HandLandmarkGameViewOverlay)} could not find {nameof(UE_HandTrackingGestureController)}.", this);
    }
}
