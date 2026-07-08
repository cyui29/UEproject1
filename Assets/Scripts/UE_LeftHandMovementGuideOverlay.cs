using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10002)]
public class UE_LeftHandMovementGuideOverlay : MaskableGraphic
{
    private const string MainCanvasName = "UE_MainCanvas";
    private const string OverlayName = "UE_LeftHandMovementGuideOverlay";
    private const string LabelName = "UE_LeftHandMovementGuideLabel";

    [SerializeField] private UE_HandTrackingGestureController handTrackingController;
    [SerializeField] private bool fitToParent;
    [SerializeField] private bool useLocalGuideCoordinates = true;
    [SerializeField] private Vector2 neutralPoint = new Vector2(0.5f, 0.5f);
    [SerializeField, Range(0f, 1f)] private float leftAreaMaxX = 0.5f;
    [SerializeField, Min(0.01f)] private float inputRange = 0.42f;
    [SerializeField] private Color areaColor = new Color(0.05f, 0.12f, 0.2f, 0.22f);
    [SerializeField] private Color boundaryColor = new Color(0.45f, 0.75f, 1f, 0.65f);
    [SerializeField] private Color guideColor = new Color(0.8f, 0.95f, 1f, 0.85f);
    [SerializeField] private Color handColor = new Color(0.25f, 0.7f, 1f, 0.95f);
    [SerializeField, Min(1f)] private float lineThickness = 3f;
    [SerializeField, Min(1f)] private float centerRadius = 11f;
    [SerializeField, Min(1f)] private float handRadius = 13f;

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

        UE_LeftHandMovementGuideOverlay graphic = overlay.AddComponent<UE_LeftHandMovementGuideOverlay>();
        graphic.fitToParent = true;
        graphic.raycastTarget = false;

        CreateLabel(overlay.transform);
        rectTransform.SetAsLastSibling();
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

        if (fitToParent && rectTransform != null)
        {
            StretchToParent(rectTransform);
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;
        float boundaryX = useLocalGuideCoordinates ? rect.xMax : Mathf.Lerp(rect.xMin, rect.xMax, leftAreaMaxX);
        Vector2 neutral = ToLocalPoint(rect, neutralPoint);
        float rangePixels = inputRange * (useLocalGuideCoordinates ? Mathf.Min(rect.width, rect.height) : rect.width);

        AddRect(vertexHelper, new Rect(rect.xMin, rect.yMin, useLocalGuideCoordinates ? rect.width : rect.width * leftAreaMaxX, rect.height), areaColor);
        AddLine(vertexHelper, new Vector2(boundaryX, rect.yMin), new Vector2(boundaryX, rect.yMax), lineThickness, boundaryColor);

        AddLine(vertexHelper, neutral + Vector2.down * rangePixels, neutral + Vector2.up * rangePixels, lineThickness, guideColor);
        AddLine(vertexHelper, neutral + Vector2.left * rangePixels, neutral + Vector2.right * rangePixels, lineThickness, guideColor);
        AddCircle(vertexHelper, neutral, centerRadius, 20, guideColor);

        DrawArrowHead(vertexHelper, neutral + Vector2.up * rangePixels, Vector2.up, guideColor);
        DrawArrowHead(vertexHelper, neutral + Vector2.down * rangePixels, Vector2.down, guideColor);
        DrawArrowHead(vertexHelper, neutral + Vector2.left * rangePixels, Vector2.left, guideColor);
        DrawArrowHead(vertexHelper, neutral + Vector2.right * rangePixels, Vector2.right, guideColor);

        if (handTrackingController == null)
        {
            WarnMissingController();
            return;
        }

        UE_HandTrackingGestureController.HandSnapshot leftHand = handTrackingController.LeftHand;

        if (leftHand.IsTracked && leftHand.Center.x <= leftAreaMaxX)
        {
            AddCircle(vertexHelper, ToLocalPoint(rect, ResolveGuidePoint(leftHand.Center)), handRadius, 24, handColor);
        }
    }

    private static void CreateLabel(Transform parent)
    {
        GameObject labelObject = new GameObject(LabelName, typeof(RectTransform));
        labelObject.layer = LayerMask.NameToLayer("UI");
        labelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0.72f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(28f, -28f);
        rectTransform.sizeDelta = new Vector2(-56f, -56f);

        Text label = labelObject.AddComponent<Text>();
        label.raycastTarget = false;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.alignment = TextAnchor.UpperLeft;
        label.color = new Color(0.9f, 0.97f, 1f, 0.95f);
        label.text =
            "LEFT HAND\n" +
            "Open + up: forward\n" +
            "Open + down: backward\n" +
            "Fist + move: look around\n" +
            "Center: stop";
    }

    private static Vector2 ToLocalPoint(Rect rect, Vector2 normalizedPoint)
    {
        return new Vector2(
            rect.xMin + normalizedPoint.x * rect.width,
            rect.yMin + (1f - normalizedPoint.y) * rect.height);
    }

    private Vector2 ResolveGuidePoint(Vector2 handCenter)
    {
        if (!useLocalGuideCoordinates)
        {
            return handCenter;
        }

        return new Vector2(Mathf.InverseLerp(0f, leftAreaMaxX, handCenter.x), handCenter.y);
    }

    private static void AddRect(VertexHelper vertexHelper, Rect rect, Color color)
    {
        int index = vertexHelper.currentVertCount;

        vertexHelper.AddVert(new Vector2(rect.xMin, rect.yMin), color, Vector2.zero);
        vertexHelper.AddVert(new Vector2(rect.xMin, rect.yMax), color, Vector2.zero);
        vertexHelper.AddVert(new Vector2(rect.xMax, rect.yMax), color, Vector2.zero);
        vertexHelper.AddVert(new Vector2(rect.xMax, rect.yMin), color, Vector2.zero);
        vertexHelper.AddTriangle(index, index + 1, index + 2);
        vertexHelper.AddTriangle(index, index + 2, index + 3);
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

    private static void AddCircle(VertexHelper vertexHelper, Vector2 center, float radius, int segmentCount, Color color)
    {
        int centerIndex = vertexHelper.currentVertCount;
        vertexHelper.AddVert(center, color, Vector2.zero);

        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = Mathf.PI * 2f * i / segmentCount;
            Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            vertexHelper.AddVert(point, color, Vector2.zero);
        }

        for (int i = 1; i <= segmentCount; i++)
        {
            vertexHelper.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
        }
    }

    private static void DrawArrowHead(VertexHelper vertexHelper, Vector2 tip, Vector2 direction, Color color)
    {
        Vector2 side = new Vector2(-direction.y, direction.x);
        Vector2 baseCenter = tip - direction * 18f;
        int index = vertexHelper.currentVertCount;

        vertexHelper.AddVert(tip, color, Vector2.zero);
        vertexHelper.AddVert(baseCenter + side * 9f, color, Vector2.zero);
        vertexHelper.AddVert(baseCenter - side * 9f, color, Vector2.zero);
        vertexHelper.AddTriangle(index, index + 1, index + 2);
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
        Debug.LogWarning($"{nameof(UE_LeftHandMovementGuideOverlay)} could not find {nameof(UE_HandTrackingGestureController)}.", this);
    }
}
