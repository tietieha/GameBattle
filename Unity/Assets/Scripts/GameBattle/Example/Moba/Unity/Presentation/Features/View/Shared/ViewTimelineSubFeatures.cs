using AbilityKit.Game.Flow.Battle.View;
using AbilityKit.Game.Flow.Modules;

namespace AbilityKit.Game.Flow
{
    internal sealed class ViewTimelineSubFeature<TFeature> : IViewSubFeature<TFeature>
        where TFeature : class, IViewFeatureRuntime
    {
        public void OnAttach(in FeatureModuleContext<TFeature> ctx)
        {
            var runtime = ctx.Feature;
            if (runtime == null) return;

            runtime.Timeline?.Clear();
            runtime.Timeline = new ViewTimeline();
        }

        public void OnDetach(in FeatureModuleContext<TFeature> ctx)
        {
            var runtime = ctx.Feature;
            if (runtime == null) return;

            runtime.Timeline?.Clear();
            runtime.Timeline = null;
        }

        public void Tick(in FeatureModuleContext<TFeature> ctx, float deltaTime) { }

        public void RebindAll(in FeatureModuleContext<TFeature> ctx) { }
    }
}
