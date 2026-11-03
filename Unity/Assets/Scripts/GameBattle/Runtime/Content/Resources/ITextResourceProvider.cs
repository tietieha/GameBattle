namespace AbilityKit.Content.Resources
{
    /// <summary>Caller-owned text loading. Paths are provider-specific logical resource keys.</summary>
    public interface ITextResourceProvider
    {
        /// <summary>Throws System.IO.FileNotFoundException when the resource does not exist.</summary>
        string LoadText(string path);

        /// <summary>Returns false with null content for a missing resource; other errors propagate.</summary>
        bool TryLoadText(string path, out string content);

        bool Exists(string path);
    }
}
