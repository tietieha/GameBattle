using System;
using Svelto.ECS;
using Svelto.ECS.Schedulers;

namespace AbilityKit.World.Svelto
{
    public sealed class SveltoWorldContext : ISveltoWorldContext
    {
        private bool _disposed;

        public SveltoWorldContext()
            : this(new EntitiesSubmissionScheduler())
        {
        }

        public SveltoWorldContext(EntitiesSubmissionScheduler scheduler)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            EnginesRoot = new EnginesRoot(Scheduler);
            EntityFactory = EnginesRoot.GenerateEntityFactory();
            EntityFunctions = EnginesRoot.GenerateEntityFunctions();
            EntitiesDB = ((IUnitTestingInterface)EnginesRoot).entitiesForTesting;
        }

        public EnginesRoot EnginesRoot { get; }

        public EntitiesSubmissionScheduler Scheduler { get; }

        public EntitiesDB EntitiesDB { get; }

        public IEntityFactory EntityFactory { get; }

        public IEntityFunctions EntityFunctions { get; }

        public void SubmitEntities()
        {
            ThrowIfDisposed();
            Scheduler.SubmitEntities();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            EnginesRoot.Dispose();
            Scheduler.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SveltoWorldContext));
        }
    }
}
