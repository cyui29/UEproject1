using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10003)]
public class UE_RightHandInteractionGuideOverlay : MaskableGraphic
{
    private const string MainCanvasName = "UE_MainCanvas";
    private const string OverlayName = "UE_RightHandInteractionGuideOverlay";
    private const string LabelName = "UE_RightHandInteractionGuideLabel";
    private const string StatusLabelName = "UE_RightHandGestureStatusLabel";

    [SerializeField] private UE_HandTrackingGestureController handTrackingController;
    [SerializeField] private Text gestureStatusText;
    [SerializeField] private bool fitToParent;
    [SerializeField] private bool useLocalGuideCoordinates = true;
    [SerializeField, Range(0f, 1f)] private float rightAreaMinX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float lookNeutralY = 0.5f;
    [SerializeField, Min(0.01f)] private float lookInputRange = 0.32f;
    [SerializeField] private Color areaColor = new Color(0.08f, 0.13f, 0.08f, 0.23f);
    [SerializeField] private Color boundaryColor = new Color(0.65f, 1f, 0.25f, 0.7f);
    [SerializeField] private Color guideColor = new Color(0.82f, 1f, 0.65f, 0.88f);
    [SerializeField] private Color handColor = new Color(0.95f, 1f, 0.28f, 0.95f);
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

        UE_RightHandInteractionGuideOverlay graphic = overlay.AddComponent<UE_RightHandInteractionGuideOverlay>();
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

        if (gestureStatusText == null)
        {
            Transform statusTransform = transform.Find(StatusLabelName);
            gestureStatusText = statusTransform != null ? statusTransform.GetComponent<Text>() : null;
        }
    }

    private void LateUpdate()
    {
        RectTransform currentRectTransform = transform as RectTransform;

        if (fitToParent && currentRectTransform != null)
        {
            StretchToParent(currentRectTransform);
        }

        SetVerticesDirty();
        UpdateGestureStatusText();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;
        float boundaryX = useLocalGuideCoordinates ? rect.xMin : Mathf.Lerp(rect.xMin, rect.xMax, rightAreaMinX);
        Vector2 neutral = ToLocalPoint(rect, new Vector2(0.5f, lookNeutralY));
        float rangePixels = lookInputRange * (useLocalGuideCoordinates ? Mathf.Min(rect.width, rect.height) : rect.width);

        AddRect(vertexHelper, new Rect(useLocalGuideCoordinates ? rect.xMin : boundaryX, rect.yMin, useLocalGuideCoordinates ? rect.width : rect.width * (1f - rightAreaMinX), rect.height), areaColor);
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

        UE_HandTrackingGestureController.HandSnapshot rightHand = handTrackingController.RightHand;

        if (rightHand.IsTracked && rightHand.Center.x >= rightAreaMinX)
        {
            AddCircle(vertexHelper, ToLocalPoint(rect, ResolveGuidePoint(rightHand.Center)), handRadius, 24, handColor);
        }
    }

    private void UpdateGestureStatusText()
    {
        if (gestureStatusText == null)
        {
            return;
        }

        if (handTrackingController == null)
        {
            gestureStatusText.text = "GESTURE: WAITING";
            return;
        }

        UE_HandTrackingGestureController.HandSnapshot rightHand = handTrackingController.RightHand;

        if (!rightHand.IsTracked || rightHand.Center.x < rightAreaMinX)
        {
            gestureStatusText.text = "GESTURE: WAITING";
            return;
        }

        if (rightHand.IsPinching)
        {
            gestureStatusText.text = "GESTURE: PINCH";
            return;
        }

        if (rightHand.IsFist)
        {
            gestureStatusText.text = "GESTURE: FIST";
            return;
        }

        if (rightHand.IsPalmHeld)
        {
            gestureStatusText.text = "GESTURE: PALM";
            return;
        }

        gestureStatusText.text = "GESTURE: READY";
    }

    private static void CreateLabel(Transform parent)
    {
        GameObject labelObject = new GameObject(LabelName, typeof(RectTransform));
        labelObject.layer = LayerMask.NameToLayer("UI");
        labelObject.transform.SetParent(parent, false);

        RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
        labelRectTransform.anchorMin = new Vector2(0f, 0.62f);
        labelRectTransform.anchorMax = new Vector2(1f, 1f);
        labelRectTransform.pivot = new Vector2(0.5f, 0.5f);
        labelRectTransform.anchoredPosition = Vector2.zero;
        labelRectTransform.sizeDelta = new Vector2(-32f, -24f);

        Text label = labelObject.AddComponent<Text>();
        label.raycastTarget = false;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.alignment = TextAnchor.UpperLeft;
        label.color = new Color(0.9f, 1f, 0.78f, 0.95f);
        label.text =
            "RIGHT HAND\n" +
            "Pinch: grab / release\n" +
            "While holding: move object\n" +
            "Fist: press / break\n" +
            "Open palm: authenticate";

        GameObject statusObject = new GameObject(StatusLabelName, typeof(RectTransform));
        statusObject.layer = LayerMask.NameToLayer("UI");
        statusObject.transform.SetParent(parent, false);

        RectTransform statusRectTransform = statusObject.GetComponent<RectTransform>();
        statusRectTransform.anchorMin = new Vector2(0f, 0f);
        statusRectTransform.anchorMax = new Vector2(1f, 0.22f);
        statusRectTransform.pivot = new Vector2(0.5f, 0.5f);
        statusRectTransform.anchoredPosition = Vector2.zero;
        statusRectTransform.sizeDelta = new Vector2(-32f, -16f);

        Text statusLabel = statusObject.AddComponent<Text>();
        statusLabel.raycastTarget = false;
        statusLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusLabel.fontSize = 30;
        statusLabel.fontStyle = FontStyle.Bold;
        statusLabel.alignment = TextAnchor.MiddleCenter;
        statusLabel.color = new Color(1f, 0.95f, 0.25f, 1f);
        statusLabel.text = "GESTURE: WAITING";
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

        return new Vector2(Mathf.InverseLerp(rightAreaMinX, 1f, handCenter.x), handCenter.y);
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

    private static void StretchToParent(RectTransform targetRectTransform)
    {
        targetRectTransform.anchorMin = Vector2.zero;
        targetRectTransform.anchorMax = Vector2.one;
        targetRectTransform.pivot = new Vector2(0.5f, 0.5f);
        targetRectTransform.anchoredPosition = Vector2.zero;
        targetRectTransform.sizeDelta = Vector2.zero;
        targetRectTransform.localRotation = Quaternion.identity;
        targetRectTransform.localScale = Vector3.one;
    }

    private void WarnMissingController()
    {
        if (hasWarnedMissingController)
        {
            return;
        }

        hasWarnedMissingController = true;
        Debug.LogWarning($"{nameof(UE_RightHandInteractionGuideOverlay)} could not find {nameof(UE_HandTrackingGestureController)}.", this);
    }
}
