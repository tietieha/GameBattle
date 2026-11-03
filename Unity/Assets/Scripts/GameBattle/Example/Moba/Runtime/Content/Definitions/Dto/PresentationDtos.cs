using System;

namespace AbilityKit.Demo.Moba.Share.Config
{
    [Serializable]
    public sealed class VfxDTO
    {
        public int Id;
        public string Resource;
        public int DurationMs;
    }

    [Serializable]
    public sealed class PresentationTemplateDTO
    {
        public int Id;
        public string Name;

        public int Kind;
        public int AssetId;
        public int DefaultDurationMs;

        public int AttachMode;
        public string Socket;
        public bool Follow;

        public int StackPolicy;
        public int StopPolicy;

        public float Scale;
        public float ColorR;
        public float ColorG;
        public float ColorB;
        public float ColorA;
        public float Radius;
        public float OffsetX;
        public float OffsetY;
        public float OffsetZ;
    }
}
