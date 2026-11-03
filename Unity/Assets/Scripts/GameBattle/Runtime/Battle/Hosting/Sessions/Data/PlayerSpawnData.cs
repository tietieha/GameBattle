namespace AbilityKit.Coordinator
{
    /// <summary>
    /// 玩家出生数据。
    /// </summary>
    public struct PlayerSpawnData
    {
        /// <summary>
        /// 玩家标识。
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// 角色标识。
        /// </summary>
        public int CharacterId;

        /// <summary>
        /// 队伍标识。
        /// </summary>
        public int TeamId;

        /// <summary>
        /// 出生位置 X。
        /// </summary>
        public float X;

        /// <summary>
        /// 出生位置 Y。
        /// </summary>
        public float Y;

        /// <summary>
        /// 出生位置 Z。
        /// </summary>
        public float Z;

        /// <summary>
        /// 玩家名称（可选）。
        /// </summary>
        public string Name;

        public PlayerSpawnData(int playerId, int characterId, int teamId, float x, float y, float z, string name = null)
        {
            PlayerId = playerId;
            CharacterId = characterId;
            TeamId = teamId;
            X = x;
            Y = y;
            Z = z;
            Name = name ?? $"Player_{playerId}";
        }

        public static PlayerSpawnData CreateLocalPlayer(int playerId, int characterId, float x, float z)
        {
            return new PlayerSpawnData(playerId, characterId, 1, x, 0, z, "LocalPlayer");
        }
    }
}
