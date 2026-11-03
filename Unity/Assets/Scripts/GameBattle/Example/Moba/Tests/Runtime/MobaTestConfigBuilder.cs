using System;
using System.Collections.Generic;
using AbilityKit.Demo.Moba.Config.Core;

namespace AbilityKit.Demo.Moba.Testing
{
    /// <summary>
    /// Lightweight in-memory DTO configuration builder for MOBA test setup.
    /// It lets tests populate only the tables they care about and build a strict or relaxed config database.
    /// </summary>
    public sealed class MobaTestConfigBuilder
    {
        private readonly Dictionary<Type, Array> _dtoArraysByType = new Dictionary<Type, Array>();

        public MobaTestConfigBuilder AddDtos<TDto>(params TDto[] dtos)
            where TDto : class
        {
            return SetDtos(dtos);
        }

        public MobaTestConfigBuilder SetDtos<TDto>(IEnumerable<TDto> dtos)
            where TDto : class
        {
            if (dtos == null) throw new ArgumentNullException(nameof(dtos));

            var list = new List<TDto>();
            foreach (var dto in dtos)
            {
                if (dto != null)
                {
                    list.Add(dto);
                }
            }

            var array = Array.CreateInstance(typeof(TDto), list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                array.SetValue(list[i], i);
            }

            _dtoArraysByType[typeof(TDto)] = array;
            return this;
        }

        public MobaTestConfigBuilder Clear<TDto>()
            where TDto : class
        {
            _dtoArraysByType.Remove(typeof(TDto));
            return this;
        }

        public MobaTestConfigBuilder ClearAll()
        {
            _dtoArraysByType.Clear();
            return this;
        }

        public IMobaConfigDtoProvider BuildProvider()
        {
            return new MobaConfigDtoDictionaryProvider(new Dictionary<Type, Array>(_dtoArraysByType));
        }

        public DtoProviderMobaConfigLoadProfile CreateLoadProfile(bool strict = false)
        {
            return new DtoProviderMobaConfigLoadProfile(BuildProvider(), strict);
        }

        public MobaConfigDatabase BuildDatabase(bool strict = false)
        {
            var database = new MobaConfigDatabase();
            var result = database.ReloadFromDtoProvider(BuildProvider(), strict);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Error ?? "MOBA config load failed.");
            }

            return database;
        }
    }
}
