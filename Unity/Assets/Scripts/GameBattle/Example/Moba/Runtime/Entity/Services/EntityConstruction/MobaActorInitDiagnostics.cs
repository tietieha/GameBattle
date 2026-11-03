using System.Collections.Generic;
using AbilityKit.Core.Logging;

namespace AbilityKit.Demo.Moba.Services.EntityConstruction
{
    public sealed class MobaActorInitDiagnostics
    {
        private readonly HashSet<int> _missingCharacterIds = new HashSet<int>();
        private readonly HashSet<int> _missingAttributeTemplateIds = new HashSet<int>();
        private bool _missingConfigLogged;

        public void LogMissingConfig(string message)
        {
            if (_missingConfigLogged) return;
            _missingConfigLogged = true;
            Log.Error(message);
        }

        public void LogConfigResolveException(System.Exception ex)
        {
            if (_missingConfigLogged) return;
            _missingConfigLogged = true;
            Log.Exception(ex, "[ActorEntityInitPipeline] Failed to resolve MobaConfigDatabase");
        }

        public void LogMissingCharacter(int heroId, System.Exception ex)
        {
            if (!_missingCharacterIds.Add(heroId)) return;
            Log.Exception(ex, $"[ActorEntityInitPipeline] Character not found. heroId={heroId}");
        }

        public void LogMissingAttributeTemplate(int templateId, string message)
        {
            if (!_missingAttributeTemplateIds.Add(templateId)) return;
            Log.Error(message);
        }

        public void LogMissingAttributeTemplate(int templateId, System.Exception ex)
        {
            if (!_missingAttributeTemplateIds.Add(templateId)) return;
            Log.Exception(ex, $"[ActorEntityInitPipeline] AttributeTemplate not found. templateId={templateId}");
        }
    }
}
