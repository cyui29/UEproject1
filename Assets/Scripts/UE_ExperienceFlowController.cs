using System;
using UnityEngine;

public class UE_ExperienceFlowController : MonoBehaviour
{
    public enum ExperienceState
    {
        Title,
        MusicSelection,
        WaitingForHands,
        Playing,
        Ended
    }

    [SerializeField] private ExperienceState initialState = ExperienceState.Title;
    [SerializeField] private bool logStateChanges = true;

    private ExperienceState currentState;
    private int selectedMusicIndex = -1;
    private bool sessionActive;
    private bool restartRequested;

    public event Action<ExperienceState> StateChanged;

    public ExperienceState CurrentState => currentState;
    public int SelectedMusicIndex => selectedMusicIndex;
    public bool HasSelectedMusic => selectedMusicIndex >= 0;
    public bool IsSessionActive => sessionActive;
    public bool RestartRequested => restartRequested;

    private void Awake()
    {
        ResetSessionData();
        TransitionTo(initialState);
    }

    public void StartExperience()
    {
        ResetSessionData();
        TransitionTo(ExperienceState.Title);
    }

    public void EnterMusicSelection()
    {
        restartRequested = false;
        sessionActive = false;
        TransitionTo(ExperienceState.MusicSelection);
    }

    public void SelectMusic(int musicIndex)
    {
        if (musicIndex < 0)
        {
            Debug.LogWarning("Music index must be zero or greater.", this);
            return;
        }

        selectedMusicIndex = musicIndex;
    }

    public void EnterWaitingForHands()
    {
        if (!HasSelectedMusic)
        {
            Debug.LogWarning("Cannot enter hand waiting state before selecting music.", this);
            return;
        }

        sessionActive = false;
        TransitionTo(ExperienceState.WaitingForHands);
    }

    public void StartPlay()
    {
        if (!HasSelectedMusic)
        {
            Debug.LogWarning("Cannot start play before selecting music.", this);
            return;
        }

        restartRequested = false;
        sessionActive = true;
        TransitionTo(ExperienceState.Playing);
    }

    public void EndExperience()
    {
        ResetSessionData();
        TransitionTo(ExperienceState.Ended);
    }

    public void RequestRestart()
    {
        ResetSessionData();
        restartRequested = true;
        TransitionTo(ExperienceState.Title);
    }

    public void ResetSessionData()
    {
        selectedMusicIndex = -1;
        sessionActive = false;
        restartRequested = false;
    }

    private void TransitionTo(ExperienceState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;

        if (logStateChanges)
        {
            Debug.Log($"Experience state changed: {currentState}", this);
        }

        StateChanged?.Invoke(currentState);
    }
}
