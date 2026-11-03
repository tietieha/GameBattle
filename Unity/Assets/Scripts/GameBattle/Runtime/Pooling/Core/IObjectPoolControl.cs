namespace AbilityKit.Core.Pooling
{
    /// <summary>
    /// 对象池运行时控制接口，用于在管理器中以非泛型方式执行裁剪和清理，避免批量操作依赖反射。
    /// </summary>
    internal interface IObjectPoolControl
    {
        int Trim(PoolTrimPolicy policy);

        int ForceTrim(PoolTrimPolicy policy);

        void Clear(bool destroy = false);
    }
}
