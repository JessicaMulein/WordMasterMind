using GameEngine.Blazor.Enumerations;

// ReSharper disable MemberCanBePrivate.Global

namespace GameEngine.Blazor.Models;

public class StateChangedEventArgs : EventArgs
{
    public readonly GameState NewState;
    public readonly GameState PreviousState;

    public StateChangedEventArgs(GameState previousState, GameState newState)
    {
        this.PreviousState = previousState;
        this.NewState = newState;
    }
}