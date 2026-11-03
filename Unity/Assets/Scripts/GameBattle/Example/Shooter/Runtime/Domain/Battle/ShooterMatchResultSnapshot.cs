namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterMatchResultSnapshot
    {
        public ShooterMatchResultSnapshot(
            ShooterBattleMatchState matchState,
            int completedFrame,
            bool isFinal,
            bool isVictory,
            int defeatedEnemies,
            int victoryTargetDefeats,
            int timeLimitFrames,
            int remainingTimeFrames)
        {
            MatchState = matchState;
            CompletedFrame = completedFrame;
            IsFinal = isFinal;
            IsVictory = isVictory;
            DefeatedEnemies = defeatedEnemies;
            VictoryTargetDefeats = victoryTargetDefeats;
            TimeLimitFrames = timeLimitFrames < 0 ? 0 : timeLimitFrames;
            RemainingTimeFrames = remainingTimeFrames < 0 ? 0 : remainingTimeFrames;
        }

        public ShooterBattleMatchState MatchState { get; }

        public int CompletedFrame { get; }

        public bool IsFinal { get; }

        public bool IsVictory { get; }

        public int DefeatedEnemies { get; }

        public int VictoryTargetDefeats { get; }

        public int TimeLimitFrames { get; }

        public int RemainingTimeFrames { get; }

        public bool IsTimeLimited => TimeLimitFrames > 0;

        public bool IsTimeExpired => IsTimeLimited && RemainingTimeFrames == 0;

        public static ShooterMatchResultSnapshot NotCompleted(int currentFrame, int defeatedEnemies, int victoryTargetDefeats)
        {
            return NotCompleted(currentFrame, defeatedEnemies, victoryTargetDefeats, timeLimitFrames: 0, remainingTimeFrames: 0);
        }

        public static ShooterMatchResultSnapshot NotCompleted(
            int currentFrame,
            int defeatedEnemies,
            int victoryTargetDefeats,
            int timeLimitFrames,
            int remainingTimeFrames)
        {
            return new ShooterMatchResultSnapshot(
                ShooterBattleMatchState.Running,
                currentFrame,
                isFinal: false,
                isVictory: false,
                defeatedEnemies,
                victoryTargetDefeats,
                timeLimitFrames,
                remainingTimeFrames);
        }
    }
}
