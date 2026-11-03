namespace AbilityKit.Protocol.Room
{
    public static class RoomGatewayIds
    {
        public static ulong CreateNumericRoomId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            var hash = offset;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }

            return hash == 0 ? 1 : hash;
        }
    }
}
