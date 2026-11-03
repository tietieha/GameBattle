using System;
using System.Collections.Generic;
using AbilityKit.Ability.Config;

namespace AbilityKit.Demo.Moba.Config.Core
{
    public interface IMobaConfigDtoProvider
    {
        bool TryGetDtos(Type dtoType, out Array dtos);
    }

    public sealed class EmptyMobaConfigDtoProvider : IMobaConfigDtoProvider
    {
        public static readonly EmptyMobaConfigDtoProvider Instance = new EmptyMobaConfigDtoProvider();

        private EmptyMobaConfigDtoProvider()
        {
        }

        public bool TryGetDtos(Type dtoType, out Array dtos)
        {
            dtos = null;
            return false;
        }
    }

    public sealed class MobaConfigDtoDictionaryProvider : IMobaConfigDtoProvider
    {
        private readonly IReadOnlyDictionary<Type, Array> _dtoArraysByType;

        public MobaConfigDtoDictionaryProvider(IReadOnlyDictionary<Type, Array> dtoArraysByType)
        {
            _dtoArraysByType = dtoArraysByType ?? throw new ArgumentNullException(nameof(dtoArraysByType));
        }

        public bool TryGetDtos(Type dtoType, out Array dtos)
        {
            if (dtoType == null) throw new ArgumentNullException(nameof(dtoType));
            return _dtoArraysByType.TryGetValue(dtoType, out dtos) && dtos != null;
        }
    }
}
