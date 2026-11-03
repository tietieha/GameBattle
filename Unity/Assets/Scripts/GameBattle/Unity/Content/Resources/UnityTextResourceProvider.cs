using System;
using System.IO;
using AbilityKit.Content.Resources;
using UnityEngine;

namespace AbilityKit.Unity.Content.Resources
{
    /// <summary>Resources-backed default. Addressables and other loaders implement ITextResourceProvider externally.</summary>
    public sealed class UnityTextResourceProvider : ITextResourceProvider
    {
        public string LoadText(string path)
        {
            if (TryLoadText(path, out var content)) return content;
            throw new FileNotFoundException("Text resource not found: " + path, path);
        }

        public bool TryLoadText(string path, out string content)
        {
            ValidatePath(path);
            var asset = UnityEngine.Resources.Load<TextAsset>(path);
            content = asset != null ? asset.text : null;
            return asset != null;
        }

        public bool Exists(string path)
        {
            ValidatePath(path);
            return UnityEngine.Resources.Load<TextAsset>(path) != null;
        }

        private static void ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Resource key is required.", nameof(path));
        }
    }
}
