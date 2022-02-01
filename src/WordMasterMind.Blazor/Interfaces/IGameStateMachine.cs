using WordMasterMind.Blazor.Enumerations;

namespace WordMasterMind.Blazor.Interfaces;

public interface IGameStateMachine
{
    public GameState State { get; set; }
}