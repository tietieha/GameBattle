using System;
using Svelto.ECS;
using Svelto.ECS.Schedulers;

namespace AbilityKit.World.Svelto
{
    public interface ISveltoWorldContext : IDisposable
    {
        EnginesRoot EnginesRoot { get; }

        EntitiesSubmissionScheduler Scheduler { get; }

        EntitiesDB EntitiesDB { get; }

        IEntityFactory EntityFactory { get; }

        IEntityFunctions EntityFunctions { get; }

        void SubmitEntities();
    }
}
