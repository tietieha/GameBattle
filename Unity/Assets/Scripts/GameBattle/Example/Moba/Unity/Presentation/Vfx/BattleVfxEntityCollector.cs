using System.Collections.Generic;
using AbilityKit.Game.Battle.Component;
using EC = AbilityKit.World.ECS;

namespace AbilityKit.Game.Battle.Vfx
{
    internal sealed class BattleVfxEntityCollector
    {
        public void Collect(in EC.IEntity root, List<EC.IEntityId> results)
        {
            if (!root.IsValid) return;
            if (results == null) return;

            var stack = new Stack<EC.IEntity>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var entity = stack.Pop();
                if (!entity.IsValid) continue;

                if (entity.TryGetRef(out BattleVfxComponent vfx) && vfx != null)
                {
                    results.Add(entity.Id);
                }

                var childCount = entity.ChildCount;
                for (int i = 0; i < childCount; i++)
                {
                    stack.Push(entity.GetChild(i));
                }
            }
        }
    }
}
