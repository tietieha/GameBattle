# Triggering 兼容说明

这里仅保留正式边界的机器可读索引。根目录 `.cs` 兼容占位入口已清理完成，当前不再新增回流点。

- [`RootRuntimeCompatibilityCatalog.cs`](RootRuntimeCompatibilityCatalog.cs)：当前为空的根目录兼容清单，用于阻止新占位入口回流。
- [`RuntimeCompatibilityCatalog`](../Validation/RuntimeCompatibilityCatalog.cs)：验证层门面，与正式清单保持一致。

## 规则

1. 新功能不得在 Runtime 根目录 `.cs` 文件首发。
2. 不再新增根目录 `.cs` 兼容占位入口。
3. 未登记的旧入口视为回流风险，应直接迁移到正式目录或删除。
