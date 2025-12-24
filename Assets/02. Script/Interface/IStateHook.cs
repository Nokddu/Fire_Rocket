public interface IStateHook
{
    void OnEnter(GameStateId from, GameStateId to);
    void OnExit(GameStateId from, GameStateId to);
}
