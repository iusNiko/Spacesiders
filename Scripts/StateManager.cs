using Godot;
using System;

public partial class StateManager
{
    public State CurrentState;
    public State PreviousState;
    public Unit Unit;
    public StateManager(Unit unit)
    {
        Unit = unit;
    }
    public void ChangeState(State newState)
    {
        if(CurrentState != null) {
            CurrentState.Exit();
        }
        PreviousState = CurrentState;
        CurrentState = newState;
        if(CurrentState != null) {
            CurrentState.StateManager = this;
            CurrentState.Enter();
        }
        
    }
}
