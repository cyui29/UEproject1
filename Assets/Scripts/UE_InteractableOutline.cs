using UnityEngine;

[ExecuteAlways]
public class UE_InteractableOutline : MonoBehaviour
{
    private static readonly int[] EdgeIndices =
    {
        0, 1, 1, 3, 3, 2, 2, 0,
        4, 5, 5, 7, 7, 6, 6, 4,
        0, 4, 1, 5, 2, 6, 3, 7
    };

    [SerializeField] private Collider targetCollider;
    [SerializeField] private Color outlineColor = new Color(0.65f, 1f, 0.25f, 0.95f);
    [SerializeField, Min(0.001f)] private float lineWidth = 0.025f;
    [SerializeField, Min(0f)] private float boundsPadding = 0.03f;

    private readonly LineRenderer[] edgeLines = new LineRenderer[12];
    private Material lineMaterial;

    public void Initialize(Collider collider, Color color, float width)
    {
        targetCollider = collider;
        outlineColor = color;
        lineWidth = width;
        EnsureLines();
        ApplyLineSettings();
        UpdateOutline();
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        ApplyLineSettings();
    }

    private void OnEnable()
    {
        EnsureLines();
        ApplyLineSettings();
        UpdateOutline();
    }

    private void LateUpdate()
    {
        UpdateOutline();
    }

    private void EnsureLines()
    {
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            lineMaterial = shader != null ? new Material(shader) : null;
        }

        for (int i = 0; i < edgeLines.Length; i++)
        {
            if (edgeLines[i] != null)
            {
                continue;
            }

            Transform existing = transform.Find($"OutlineEdge_{i:00}");
            GameObject edgeObject = existing != null
                ? existing.gameObject
                : new GameObject($"OutlineEdge_{i:00}");

            edgeObject.hideFlags = HideFlags.DontSaveInBuild;
            edgeObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer = edgeObject.GetComponent<LineRenderer>();

            if (lineRenderer == null)
            {
                lineRenderer = edgeObject.AddComponent<LineRenderer>();
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.numCornerVertices = 2;
            lineRenderer.numCapVertices = 2;
            edgeLines[i] = lineRenderer;
        }
    }

    private void ApplyLineSettings()
    {
        foreach (LineRenderer edgeLine in edgeLines)
        {
            if (edgeLine == null)
            {
                continue;
            }

            edgeLine.enabled = targetCollider != null;
            edgeLine.startColor = outlineColor;
            edgeLine.endColor = outlineColor;
            edgeLine.startWidth = lineWidth;
            edgeLine.endWidth = lineWidth;
            edgeLine.material = lineMaterial;
        }
    }

    private void UpdateOutline()
    {
        if (targetCollider == null)
        {
            ApplyLineSettings();
            return;
        }

        Bounds bounds = targetCollider.bounds;
        bounds.Expand(boundsPadding * 2f);

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z)
        };

        for (int i = 0; i < edgeLines.Length; i++)
        {
            LineRenderer edgeLine = edgeLines[i];

            if (edgeLine == null)
            {
                continue;
            }

            edgeLine.enabled = true;
            edgeLine.SetPosition(0, corners[EdgeIndices[i * 2]]);
            edgeLine.SetPosition(1, corners[EdgeIndices[i * 2 + 1]]);
        }
    }
}
