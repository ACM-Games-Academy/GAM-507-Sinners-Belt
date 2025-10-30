using UnityEngine;
using System.Collections.Generic;

public abstract class State
{
    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public abstract void OnUpdate();
}

public class StateMachine
{
    private Dictionary<string, State> states;
    private State currentState;

    public StateMachine()
    {
        states = new Dictionary<string, State>();
    }

    public void AddState(string stateName, State state)
    {
        states.Add(stateName, state);
    }

    public void ChangeState(string stateName)
    {
        currentState?.OnExit();

        if (states.TryGetValue(stateName, out State state))
        {
            currentState = state;
            state.OnEnter();
        }
    }

    public void Update()
    {
        currentState?.OnUpdate();
    }
}