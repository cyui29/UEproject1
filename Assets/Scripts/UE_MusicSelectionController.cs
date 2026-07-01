using UnityEngine;

public class UE_MusicSelectionController : MonoBehaviour
{
    [System.Serializable]
    private class MusicCardData
    {
        [SerializeField] private string title;
        [SerializeField] private AudioClip clip;
        [SerializeField] private bool loop = true;
        [SerializeField] private GameObject selectedIndicator;

        public string Title => title;
        public AudioClip Clip => clip;
        public bool Loop => loop;

        public void SetSelected(bool selected)
        {
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(selected);
            }
        }
    }

    [SerializeField] private UE_ExperienceFlowController flowController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fallbackClip;
    [SerializeField] private MusicCardData[] musicCards = new MusicCardData[3];
    [SerializeField] private bool enterWaitingStateOnSelection = true;
    [SerializeField] private bool logSelectionChanges = true;

    private int selectedIndex = -1;

    public int SelectedIndex => selectedIndex;
    public bool HasSelection => GetSelectedCard() != null;
    public string SelectedTitle => HasSelection ? GetSelectedCard().Title : string.Empty;
    public AudioClip SelectedClip => ResolveSelectedClip();

    private void Awake()
    {
        UpdateSelectionVisuals();
    }

    public void SelectMusic(int index)
    {
        if (!IsValidIndex(index))
        {
            Debug.LogWarning($"Music index {index} is out of range.", this);
            return;
        }

        selectedIndex = index;
        UpdateSelectionVisuals();

        if (flowController != null)
        {
            flowController.SelectMusic(selectedIndex);

            if (enterWaitingStateOnSelection)
            {
                flowController.EnterWaitingForHands();
            }
        }

        if (logSelectionChanges)
        {
            Debug.Log($"Selected music: {SelectedTitle}", this);
        }
    }

    public void SelectFirstMusic()
    {
        SelectMusic(0);
    }

    public void SelectSecondMusic()
    {
        SelectMusic(1);
    }

    public void SelectThirdMusic()
    {
        SelectMusic(2);
    }

    public void SelectNext()
    {
        if (musicCards == null || musicCards.Length == 0)
        {
            return;
        }

        int nextIndex = HasSelection ? (selectedIndex + 1) % musicCards.Length : 0;
        SelectMusic(nextIndex);
    }

    public void SelectPrevious()
    {
        if (musicCards == null || musicCards.Length == 0)
        {
            return;
        }

        int previousIndex = HasSelection ? selectedIndex - 1 : musicCards.Length - 1;

        if (previousIndex < 0)
        {
            previousIndex = musicCards.Length - 1;
        }

        SelectMusic(previousIndex);
    }

    public bool PlaySelectedTrack()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Cannot play selected track because AudioSource is missing.", this);
            return false;
        }

        AudioClip clipToPlay = ResolveSelectedClip();

        if (clipToPlay == null)
        {
            Debug.LogWarning("Cannot play selected track because no AudioClip or fallback clip is assigned.", this);
            return false;
        }

        audioSource.clip = clipToPlay;
        MusicCardData selectedCard = GetSelectedCard();
        audioSource.loop = selectedCard != null && selectedCard.Loop;
        audioSource.Play();
        return true;
    }

    public void StopPlayback()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void ResetSelection()
    {
        selectedIndex = -1;
        StopPlayback();
        UpdateSelectionVisuals();
    }

    private AudioClip ResolveSelectedClip()
    {
        MusicCardData selectedCard = GetSelectedCard();

        if (selectedCard != null && selectedCard.Clip != null)
        {
            return selectedCard.Clip;
        }

        return fallbackClip;
    }

    private MusicCardData GetSelectedCard()
    {
        if (!IsValidIndex(selectedIndex))
        {
            return null;
        }

        return musicCards[selectedIndex];
    }

    private bool IsValidIndex(int index)
    {
        return musicCards != null && index >= 0 && index < musicCards.Length;
    }

    private void UpdateSelectionVisuals()
    {
        if (musicCards == null)
        {
            return;
        }

        for (int i = 0; i < musicCards.Length; i++)
        {
            if (musicCards[i] != null)
            {
                musicCards[i].SetSelected(i == selectedIndex);
            }
        }
    }

    private void OnValidate()
    {
        if (musicCards == null)
        {
            return;
        }

        if (musicCards.Length < 2 || musicCards.Length > 3)
        {
            Debug.LogWarning("Prototype music selection should use 2 or 3 music cards.", this);
        }
    }
}
