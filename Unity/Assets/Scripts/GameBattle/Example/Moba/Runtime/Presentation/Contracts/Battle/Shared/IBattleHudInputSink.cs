namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.Shared
{
    public interface IBattleHudInputSink
    {
        void OnMoveInput(float dx, float dz);

        void OnSkillAimStart(int slot, float dx, float dz);

        void OnSkillAimUpdate(int slot, float dx, float dz);

        void OnSkillAimEnd(int slot, float dx, float dz);

        void OnSkillClick(int slot);
    }
}
