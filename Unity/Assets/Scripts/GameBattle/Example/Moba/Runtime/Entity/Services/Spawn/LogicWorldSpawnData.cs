namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// 运行时服务使用的逻辑世界生成数据。
    /// 外部宿主必须在世界启动前把自身启动数据映射到该运行时模型。
    /// </summary>
    public readonly struct LogicWorldSpawnData
    {
        public readonly int PlayerId;
        public readonly int CharacterId;
        public readonly int TeamId;
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly string Name;

        public LogicWorldSpawnData(int playerId, int characterId, int teamId, float x, float y, float z, string name = null)
        {
            PlayerId = playerId;
            CharacterId = characterId;
            TeamId = teamId;
            X = x;
            Y = y;
            Z = z;
            Name = name;
        }
    }
}
