using System;
using UnityEngine;
using UnityEngine.Events;

public class UE_HandPlayroomFlowController : MonoBehaviour
{
    public enum ExperienceState
    {
        Boot,
        WaitingForHands,
        Playing,
        Cleared,
        Resetting
    }

    [SerializeField] private ExperienceState initialState = ExperienceState.WaitingForHands;
    [SerializeField] private ExperienceState currentState = ExperienceState.Boot;
    [SerializeField] private bool logStateChanges = true;

    public event Action<ExperienceState, ExperienceState> StateChanged;
    public event Action ResetRequested;

    public UnityEvent OnWaitingForHandsEntered;
    public UnityEvent OnPlayStarted;
    public UnityEvent OnExperienceCleared;
    public UnityEvent OnResetRequested;

    public ExperienceState CurrentState => currentState;

    private void Awake()
    {
        ChangeState(initialState);
    }

    public void EnterWaitingForHands()
    {
        ChangeState(ExperienceState.WaitingForHands);
        OnWaitingForHandsEntered?.Invoke();
    }

    public void StartPlay()
    {
        ChangeState(ExperienceState.Playing);
        OnPlayStarted?.Invoke();
    }

    public void NotifyHandsReady()
    {
        StartPlay();
    }

    public void CompleteExperience()
    {
        ChangeState(ExperienceState.Cleared);
        OnExperienceCleared?.Invoke();
    }

    public void RequestReset()
    {
        ChangeState(ExperienceState.Resetting);
        ResetRequested?.Invoke();
        OnResetRequested?.Invoke();
        EnterWaitingForHands();
    }

    private void ChangeState(ExperienceState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        ExperienceState previousState = currentState;
        currentState = nextState;

        if (logStateChanges)
        {
            Debug.Log($"{nameof(UE_HandPlayroomFlowController)} state: {previousState} -> {currentState}", this);
        }

        StateChanged?.Invoke(previousState, currentState);
    }
}
