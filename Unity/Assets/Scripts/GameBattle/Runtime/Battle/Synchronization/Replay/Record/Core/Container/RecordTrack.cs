using System;

namespace AbilityKit.Core.Recording.Core
{
    [Serializable]
    public sealed class RecordTrack
    {
        public RecordTrackId Id;

        public int Version;

        public string Schema;

        public EventTrack Events;
    }
}
