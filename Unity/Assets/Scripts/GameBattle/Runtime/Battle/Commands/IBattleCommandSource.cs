namespace AbilityKit.Battle.Commands
{
    /// <summary>External player, AI or scripted command producer. Called before draining the target frame.</summary>
    public interface IBattleCommandSource<TCommand>
    {
        void CollectCommands(int targetFrame, IBattleCommandSink<TCommand> sink);
    }

    /// <summary>Frame-addressed submission. False means the caller must handle rejection; no world mutation occurs here.</summary>
    public interface IBattleCommandSink<in TCommand>
    {
        bool TrySubmit(int targetFrame, TCommand command);
    }
}
