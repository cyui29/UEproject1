using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UE_HandPlayroomProgressFeedbackController : MonoBehaviour
{
    public enum CompletionItem
    {
        CubeGrabbed,
        CubePlaced,
        CookieBlockBroken,
        ButtonSmashed,
        StarLampAuthenticated
    }

    [Serializable]
    private struct ObjectPrompt
    {
        public string objectId;
        [TextArea] public string prompt;

        public ObjectPrompt(string objectId, string prompt)
        {
            this.objectId = objectId;
            this.prompt = prompt;
        }
    }

    [SerializeField] private UE_HandPlayroomFlowController flowController;
    [SerializeField] private int starLampRequiredCompletions = 3;
    [SerializeField] private float feedbackDisplaySeconds = 2f;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private Text handStatusText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text actionPromptText;
    [SerializeField] private Text debugText;
    [SerializeField] private Image centerReticleImage;
    [SerializeField] private Image movementCircleImage;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private GameObject starLampObject;
    [SerializeField] private ObjectPrompt[] objectPrompts = Array.Empty<ObjectPrompt>();

    public UnityEvent OnStarLampActivated;
    public UnityEvent OnStarLampAuthenticated;

    private readonly HashSet<CompletionItem> completedItems = new HashSet<CompletionItem>();
    private float feedbackHideTime;
    private bool starLampActivated;

    public int CompletedCount => completedItems.Count;
    public bool IsStarLampActivated => starLampActivated;
    public float Progress01 => Mathf.Clamp01(completedItems.Count / (float)Mathf.Max(1, starLampRequiredCompletions));

    private void Awake()
    {
        if (objectivePanel == null && objectiveText != null)
        {
            objectivePanel = objectiveText.transform.parent != null
                ? objectiveText.transform.parent.gameObject
                : objectiveText.gameObject;
        }

        ApplyInitialUiState();
    }

    private void Update()
    {
        if (feedbackHideTime > 0f && Time.time >= feedbackHideTime)
        {
            feedbackHideTime = 0f;
            SetPromptText(string.Empty);
        }
    }

    public void RecordCompletion(CompletionItem item)
    {
        if (!completedItems.Add(item))
        {
            return;
        }

        UpdateProgressUi();

        if (item == CompletionItem.StarLampAuthenticated)
        {
            AuthenticateStarLamp();
            return;
        }

        if (!starLampActivated && completedItems.Count >= starLampRequiredCompletions)
        {
            ActivateStarLamp();
        }
    }

    public void RecordCompletionByName(string itemName)
    {
        if (Enum.TryParse(itemName, true, out CompletionItem item))
        {
            RecordCompletion(item);
            return;
        }

        Debug.LogWarning($"{nameof(UE_HandPlayroomProgressFeedbackController)} unknown completion item: {itemName}", this);
    }

    public void SetHandStatus(bool leftHandTracked, bool rightHandTracked)
    {
        SetText(handStatusText, $"LEFT HAND: {FormatTracked(leftHandTracked)}\nRIGHT HAND: {FormatTracked(rightHandTracked)}");
        SetObjectivePanelVisible(!leftHandTracked || !rightHandTracked);
    }

    public void SetObjective(string message)
    {
        SetText(objectiveText, message);
    }

    public void ShowObjectPrompt(string objectId)
    {
        foreach (ObjectPrompt objectPrompt in objectPrompts)
        {
            if (string.Equals(objectPrompt.objectId, objectId, StringComparison.OrdinalIgnoreCase))
            {
                SetPromptText(objectPrompt.prompt);
                return;
            }
        }

        SetPromptText(string.Empty);
    }

    public void ShowInteractionPrompt(string actionLabel, string objectId)
    {
        if (feedbackHideTime > 0f)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(actionLabel))
        {
            SetPromptText(string.Empty);
            return;
        }

        SetPromptText(string.IsNullOrWhiteSpace(objectId)
            ? actionLabel
            : $"{actionLabel}\n{objectId}");
    }

    public void ShowFeedback(string message)
    {
        SetPromptText(message);
        feedbackHideTime = Time.time + feedbackDisplaySeconds;
    }

    public void ShowSuccess(string message)
    {
        ShowFeedback(message);
    }

    public void ShowFailure(string message)
    {
        ShowFeedback(message);
    }

    public void ResetProgress()
    {
        completedItems.Clear();
        starLampActivated = false;
        feedbackHideTime = 0f;

        if (starLampObject != null)
        {
            starLampObject.SetActive(false);
        }

        ApplyInitialUiState();
    }

    private void ActivateStarLamp()
    {
        starLampActivated = true;

        if (starLampObject != null)
        {
            starLampObject.SetActive(true);
        }

        SetObjective("Hold your right palm on the final panel.");
        ShowSuccess("Star lamp is ready.");
        OnStarLampActivated?.Invoke();
    }

    private void AuthenticateStarLamp()
    {
        ActivateStarLamp();
        SetObjective("Memory Vault unlocked.");
        ShowSuccess("Escape authentication complete.");
        OnStarLampAuthenticated?.Invoke();

        if (flowController != null)
        {
            flowController.CompleteExperience();
        }
    }

    private void ApplyInitialUiState()
    {
        SetHandStatus(false, false);
        SetObjective("Show both hands to start.");
        UpdateProgressUi();
        SetPromptText("Point your right hand at an object.");
        SetText(debugText, "DEBUG\nHands: none\nGesture: none");

        if (starLampObject != null)
        {
            starLampObject.SetActive(starLampActivated);
        }

        if (centerReticleImage != null)
        {
            centerReticleImage.enabled = true;
        }

        if (movementCircleImage != null)
        {
            movementCircleImage.enabled = true;
        }
    }

    private void UpdateProgressUi()
    {
        SetText(progressText, $"PROGRESS {completedItems.Count} / {starLampRequiredCompletions}");

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = Progress01;
        }
    }

    private void SetPromptText(string message)
    {
        SetText(promptText, message);

        if (actionPromptText == null)
        {
            EnsureActionPromptText();
        }

        SetText(actionPromptText, message);

        if (actionPromptText != null)
        {
            actionPromptText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        }
    }

    private void EnsureActionPromptText()
    {
        Canvas targetCanvas = promptText != null
            ? promptText.GetComponentInParent<Canvas>()
            : GetComponentInParent<Canvas>();

        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
        }

        if (targetCanvas == null)
        {
            return;
        }

        GameObject promptObject = new GameObject("UE_ActionPromptText");
        promptObject.transform.SetParent(targetCanvas.transform, false);

        RectTransform rectTransform = promptObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -86f);
        rectTransform.sizeDelta = new Vector2(360f, 96f);

        actionPromptText = promptObject.AddComponent<Text>();
        actionPromptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        actionPromptText.fontSize = 32;
        actionPromptText.fontStyle = FontStyle.Bold;
        actionPromptText.alignment = TextAnchor.MiddleCenter;
        actionPromptText.color = new Color(1f, 0.15f, 0.1f, 0.95f);
        actionPromptText.raycastTarget = false;
        actionPromptText.text = string.Empty;
    }

    private void SetObjectivePanelVisible(bool visible)
    {
        if (objectivePanel != null && objectivePanel.activeSelf != visible)
        {
            objectivePanel.SetActive(visible);
        }
    }

    private static string FormatTracked(bool tracked)
    {
        return tracked ? "READY" : "WAITING";
    }

    private void SetText(Text text, string message)
    {
        if (text == null)
        {
            Debug.LogWarning($"{nameof(UE_HandPlayroomProgressFeedbackController)} has an empty UI text reference.", this);
            return;
        }

        text.text = message;
    }
}
