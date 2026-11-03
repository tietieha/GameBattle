namespace AbilityKit.Game.View.Modules
{
    public readonly struct FeatureModuleContext<TPhaseContext, TFeature>
    {
        public readonly TPhaseContext Phase;
        public readonly TFeature Feature;

        public FeatureModuleContext(in TPhaseContext phase, TFeature feature)
        {
            Phase = phase;
            Feature = feature;
        }
    }
}
