using GameEngine.Blazor.Enumerations;
using GameEngine.Blazor.Interfaces;
using GameEngine.Blazor.Shared;
using GameEngine.Library.Exceptions;

namespace GameEngine.Blazor.Helpers;

public static class UIHelpers
{
    public static async Task<(bool success, GameEngineException? gameEngineException)> TryAndAlertOnGameEngineException(
        Func<Task> action)
    {
        try
        {
            await action();
            return (true, null);
        }
        catch (GameEngineException gEx)
        {
            await BodyElement.ShowAlert(message: gEx.Message);
            return (false, gEx);
        }
    }

    public static async Task<(bool success, GameEngineException? gameEngineException)>
        TryChangeGameStateAndAlertOnGameEngineException(IGameStateMachine stateMachine, GameState newState,
            bool alertOnFailure = true)
    {
        try
        {
            await stateMachine.ChangeStateAsync(newState: newState);
            return (true, null);
        }
        catch (GameEngineException gEx)
        {
            if (alertOnFailure)
                await BodyElement.ShowAlert(message: gEx.Message);
            return (false, gEx);
        }
    }

    public static async Task<(bool success, GameEngineException? gameEngineException)>
        TryPreviousStateAndAlertOnGameEngineException(IGameStateMachine stateMachine)
    {
        return await TryChangeGameStateAndAlertOnGameEngineException(
            stateMachine: stateMachine,
            newState: stateMachine.PreviousState);
    }
}