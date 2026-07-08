using UnityEngine;

[DefaultExecutionOrder(10000)]
public class UE_AnnotationLayerGameViewFitter : MonoBehaviour
{
    private const string MainCanvasName = "UE_MainCanvas";
    private const string HandCanvasName = "Hand Canvas";
    private const string AnnotationLayerName = "Annotation Layer";

    private RectTransform mainCanvasRect;
    private RectTransform handCanvasRect;
    private Transform annotationLayer;
    private RectTransform annotationLayerRect;
    private int uiLayer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        GameObject mainCanvas = GameObject.Find(MainCanvasName);

        if (mainCanvas == null || mainCanvas.GetComponent<UE_AnnotationLayerGameViewFitter>() != null)
        {
            return;
        }

        mainCanvas.AddComponent<UE_AnnotationLayerGameViewFitter>();
    }

    private void Awake()
    {
        uiLayer = LayerMask.NameToLayer("UI");
        mainCanvasRect = GetComponent<RectTransform>();
        CacheReferences();
        FitNow();
    }

    private void LateUpdate()
    {
        if (handCanvasRect == null || annotationLayer == null)
        {
            CacheReferences();
        }

        FitNow();
    }

    private void CacheReferences()
    {
        Transform root = transform.root;
        Transform handCanvas = FindChildByName(root, HandCanvasName);
        handCanvasRect = handCanvas != null ? handCanvas.GetComponent<RectTransform>() : null;
        annotationLayer = FindChildByName(root, AnnotationLayerName);
        annotationLayerRect = annotationLayer != null ? annotationLayer.GetComponent<RectTransform>() : null;
    }

    private void FitNow()
    {
        if (mainCanvasRect != null)
        {
            StretchToParent(mainCanvasRect);
        }

        if (handCanvasRect != null)
        {
            StretchToParent(handCanvasRect);
        }

        if (annotationLayer == null)
        {
            return;
        }

        annotationLayer.gameObject.SetActive(true);
        annotationLayer.SetAsLastSibling();

        if (uiLayer >= 0)
        {
            SetLayerRecursively(annotationLayer, uiLayer);
        }

        if (annotationLayerRect != null)
        {
            StretchToParent(annotationLayerRect);
            return;
        }

        annotationLayer.localPosition = Vector3.zero;
        annotationLayer.localRotation = Quaternion.identity;
        annotationLayer.localScale = Vector3.one;
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

    private static Transform FindChildByName(Transform root, string targetName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildByName(root.GetChild(i), targetName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }
}
