using System.Collections.Generic;
using AbilityKit.Ability.Config;

namespace AbilityKit.Demo.Moba.Config.Core
{
    /// <summary>
    /// Provides config groups for DI-driven MOBA config loading.
    /// </summary>
    public interface IMobaConfigGroupProvider : IConfigGroupProvider
    {
    }

    /// <summary>
    /// Default provider that returns the groups declared by MobaConfigGroups.
    /// </summary>
    public sealed class DefaultMobaConfigGroupProvider : IMobaConfigGroupProvider
    {
        public static readonly DefaultMobaConfigGroupProvider Instance = new DefaultMobaConfigGroupProvider();

        private DefaultMobaConfigGroupProvider() { }

        public IReadOnlyList<IConfigGroup> GetGroups()
        {
            return MobaConfigGroups.All;
        }
    }
}
