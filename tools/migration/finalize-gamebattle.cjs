const fs = require('node:fs');
const path = require('node:path');
const cp = require('node:child_process');
const root = path.resolve(__dirname, '../..');
const unity = path.join(root, 'Unity');
const destination = path.join(unity, 'Assets/Scripts/GameBattle');
const reportRoot = path.join(root, 'Docs/GameBattle/Migration');
const plan = JSON.parse(fs.readFileSync(path.join(reportRoot, 'plan.json'), 'utf8'));
const slash = p => p.replaceAll('\\', '/');
const from = 'Unity/Assets/Scripts/GameBattle/Gameplay/Moba/Client/ThirdParty/DesperateDevs/Compile.cs';
const to = 'Unity/Assets/Scripts/GameBattle/ThirdParty/DesperateDevs/EditorStubs/Compile.cs';
if (fs.existsSync(path.join(root, from))) {
  fs.mkdirSync(path.dirname(path.join(root, to)), { recursive: true });
  fs.renameSync(path.join(root, from), path.join(root, to));
  if (fs.existsSync(path.join(root, from + '.meta'))) fs.renameSync(path.join(root, from + '.meta'), path.join(root, to + '.meta'));
  for (const file of plan.files) if (file.to === from || file.to === from + '.meta') file.to = to + file.to.slice(from.length);
  for (const file of plan.sourceOwnership) if (file.to === from) file.to = to;
  fs.writeFileSync(path.join(reportRoot, 'plan.json'), JSON.stringify(plan, null, 2));
}
// Patch generated IDE projects as a temporary bridge; Unity will regenerate them on refresh.
const moved = plan.files.filter(f => !f.from.endsWith('.meta')).map(f => [
  f.from.replace(/^Unity\//, ''), f.to.replace(/^Unity\//, ''),
]).sort((a, b) => b[0].length - a[0].length);
const escaped = s => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
const byOld = new Map(moved);
const pattern = new RegExp(moved.map(([old]) => escaped(old.replaceAll('/', '\\'))).join('|'), 'g');
const forward = new RegExp(moved.map(([old]) => escaped(old)).join('|'), 'g');
let ideProjects = 0;
for (const name of fs.readdirSync(unity).filter(n => n.endsWith('.csproj'))) {
  const file = path.join(unity, name);
  const text = fs.readFileSync(file, 'utf8');
  const output = text.replace(pattern, old => byOld.get(old.replaceAll('\\', '/')).replaceAll('/', '\\'))
    .replace(forward, old => byOld.get(old));
  if (output !== text) { fs.writeFileSync(file, output); ideProjects++; }
}
// Keep pre-existing generated caches, but put them in ignored tool folders rather than Packages.
const ignored = ['obj', 'bin', '.idea'];
const packagesRoot = path.join(unity, 'Packages');
const cacheMoves = [];
function relocateCaches(dir, packageName) {
  const resolved = path.resolve(dir);
  if (!resolved.startsWith(path.resolve(packagesRoot) + path.sep)) throw new Error('Unverified cleanup path.');
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (!entry.isDirectory()) continue;
    const file = path.join(dir, entry.name);
    if (ignored.includes(entry.name)) {
      const target = path.join(root, 'tools/migration/bin/LegacyPackageCaches', packageName,
        path.relative(path.join(packagesRoot, packageName), file));
      if (fs.existsSync(target)) throw new Error('Cache destination exists: ' + target);
      fs.mkdirSync(path.dirname(target), { recursive: true });
      fs.renameSync(file, target);
      cacheMoves.push({ from: slash(path.relative(root, file)), to: slash(path.relative(root, target)) });
    } else relocateCaches(file, packageName);
  }
  if (!fs.readdirSync(dir).length) fs.rmdirSync(dir);
}
for (const entry of fs.readdirSync(packagesRoot, { withFileTypes: true }).filter(e => e.isDirectory() && e.name.startsWith('com.abilitykit.')))
  relocateCaches(path.join(packagesRoot, entry.name), entry.name);
const report = [
  '# GameBattle 目录迁移',
  '',
  '日期：2026-10-23。当前阶段仅整理物理目录与路径，不重写运行实现。',
  '',
  '## 目录边界',
  '',
  '```text',
  'Assets/Scripts/GameBattle/',
  '  Foundation/       通用数学、集合、时间、流程等基础',
  '  Content/          配置、定义、索引与时间轴交换数据',
  '  Battle/',
  '    Core/           世界、命令、生命周期、调度、规则与诊断',
  '    Actor/          战斗对象、属性、标签、决策与角色状态',
  '    Ability/        技能施法、阶段、执行与可选时间轴',
  '    Effect/         效果、数值结算、Buff 与持续过程',
  '    Spatial/        选目标、运动、导航、碰撞、投射物与区域',
  '  Presentation/     输出契约、投影与表现执行',
  '  Integration/      Unity、ECS、宿主、网络、同步、格式等适配',
  '  Gameplay/         保留的 MOBA/Shooter 玩法与装配',
  '  Editor/           编辑、编译、预览、测试与调试工具',
  '  Tooling/          原包内生成器、分析器等工具',
  '  Examples/         示例资源、场景与可选样例',
  '  Tests/            测试、用例与执行辅助',
  '  ThirdParty/       原始第三方实现与依赖',
  '  Documentation~/   原包说明、声明与历史资料（Unity 忽略）',
  '```',
  '',
  '## 本轮约束',
  '',
  '- 原包内容已移出 Packages；移动而非复制，不保留两份可编译源码。',
  '- 原 C# 类型、命名空间、程序集名与 asmdef GUID 不变，按旧程序集用 asmref 连接新目录。',
  '- Moba 子目录是现有实现的来源标记，不声称已经去除 MOBA 或 Entitas 耦合。',
  '- Effect 拥有 Buff 机制；当前实现仍是角色 Buff，本轮没有新增世界/队伍 Buff。',
  '- 时间轴仍可选，本轮没有改变技能执行策略。',
  '- 编辑状态、运行定义、运行实例的彻底分离留到实现梳理阶段，不因换目录宣称已完成。',
  '- 原 Samples~、DotNet~ 等隐藏目录继续保持隐藏，避免可选示例/工具被 Unity 默认编译。',
  '- 原文档与许可证全部保留；package.json 移入 Documentation~/PackageManifests，仅作为来源档案。',
  '- 联网、训练、加速等原有 defineConstraints 与程序集合同保持，不新增为框架必选行为。',
  '',
  '## 可核对记录',
  '',
  '- `Docs/GameBattle/Migration/plan.json`：每个原文件/新文件、迁移前哈希、程序集与脚本原归属。',
  '- `Docs/GameBattle/Migration/result.json`：迁移数量与只做路径替换的文件。',
  '- `Docs/GameBattle/Migration/verification.json`：完整性、元数据、程序集与代表性 MSBuild 源码求值。',
  '',
  '目录边界是本轮交付；程序集数量暂不收敛，依赖方向也未重构。后续按 Core → Actor → Ability → Effect → Spatial 的主线检查实现，逐步去耦，而不是一次替换所有接口。',
  '',
];
fs.writeFileSync(path.join(destination, 'README.md'), report.join('\n'));
fs.mkdirSync(path.join(root, 'Docs/GameBattle'), { recursive: true });
fs.writeFileSync(path.join(root, 'Docs/GameBattle/DirectoryMigration.md'), report.join('\n'));
fs.writeFileSync(path.join(reportRoot, 'finalization.json'), JSON.stringify({ generatedIdeProjectsUpdated: ideProjects, cacheMoves }, null, 2));
console.log(JSON.stringify({ generatedIdeProjectsUpdated: ideProjects, cachesRelocated: cacheMoves.length }, null, 2));
