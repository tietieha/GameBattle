const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const root = path.resolve(__dirname, '../..');
const base = 'Unity/Assets/Scripts/GameBattle';
const evidence = path.join(root, 'Docs/GameBattle/Reorganization');
const slash = s => s.replaceAll('\\', '/');
const full = s => path.resolve(root, s);
const read = s => fs.readFileSync(s, 'utf8').replace(/^\uFEFF/, '');
const hash = s => crypto.createHash('sha256').update(fs.readFileSync(s)).digest('hex');
const hidden = s => slash(s).split('/').some(s => s.endsWith('~'));
const skip = new Set(['.git', 'Library', 'Temp', 'Logs', 'obj', 'bin', 'node_modules', '.idea']);
function walk(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).sort((a, b) => a.name.localeCompare(b.name))
    .flatMap(e => e.isDirectory() ? skip.has(e.name) ? [] : walk(path.join(dir, e.name))
      : e.isFile() ? [path.join(dir, e.name)] : []);
}
function write(file, text) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, text);
}
function checked(file) {
  const resolved = path.resolve(file);
  if (!resolved.startsWith(root + path.sep)) throw new Error('Path escaped project: ' + file);
  return resolved;
}
const rules = [];
const route = (from, to) => rules.push({ from, to });
route('Documentation~', '@archive/LegacyPackages');
route('README.md', 'README.md');
route('Examples', 'Example');
route('Gameplay', 'Example');
route('Battle/Ability/Moba', 'Example/Moba/Runtime/Ability');
route('Battle/Actor/Moba', 'Example/Moba/Runtime/Entity');
route('Battle/Core/Moba', 'Example/Moba/Runtime/Battle');
route('Battle/Core/Diagnostics/Moba', 'Example/Moba/Runtime/Diagnostics');
route('Battle/Effect/Moba', 'Example/Moba/Runtime/Effect');
route('Battle/Spatial/Moba', 'Example/Moba/Runtime/Spatial');
for (const [group, names] of Object.entries({
  Entity: ['Actor', 'Unit', 'EntityConstruction', 'EntityManager', 'ComponentTemplates', 'Tags', 'Spawn', 'Summon'],
  Decision: ['Behavior', 'StateMachine'],
  Ability: ['Skill', 'Passive'],
  Buff: ['Buffs'],
  Effect: ['Effect', 'Effects', 'Combat', 'Continuous', 'ContinuousPeriodic'],
  Targeting: ['Search', 'Map'],
  Movement: ['Motion', 'Navigation', 'Collision'],
  Projectile: ['Projectile', 'Area'],
})) for (const name of names) for (const old of ['Core', 'Actor', 'Ability', 'Effect', 'Spatial'])
  for (const kind of ['Services', 'Systems']) route(`Battle/${old}/Moba/${kind}/${name}`, `Example/Moba/Runtime/${group}/${kind}/${name}`);
route('Battle/Actor/Moba/Domain/Attributes', 'Example/Moba/Runtime/Attribute/Definitions');
route('Battle/Actor/Moba/Domain/Behavior', 'Example/Moba/Runtime/Decision/Definitions');
route('Battle/Actor/Moba/Components/BuffComponent.cs', 'Example/Moba/Runtime/Buff/State/BuffComponent.cs');
route('Battle/Actor/Moba/Components/AttributeSystemComponents.cs', 'Example/Moba/Runtime/Attribute/State/AttributeSystemComponents.cs');
route('Battle/Actor/Moba/Components/ShieldComponents.cs', 'Example/Moba/Runtime/Effect/State/ShieldComponents.cs');
route('Battle/Actor/Moba/Components/MotionComponent.cs', 'Example/Moba/Runtime/Movement/State/MotionComponent.cs');
for (const name of ['SkillRuntime', 'SkillLoadoutComponent', 'SkillCastInstanceComponents', 'PassiveSkillTriggerListenersComponent'])
  route(`Battle/Actor/Moba/Components/${name}.cs`, `Example/Moba/Runtime/Ability/State/${name}.cs`);
for (const name of ['ProjectileLauncherComponent', 'ProjectileEffectSnapshotComponent', 'FlyingProjectileTagComponent'])
  route(`Battle/Actor/Moba/Components/${name}.cs`, `Example/Moba/Runtime/Projectile/State/${name}.cs`);
for (const name of ['ActorBrainComponent', 'ActorStateMachineComponent', 'CharacterHfsmComponent'])
  route(`Battle/Actor/Moba/Components/${name}.cs`, `Example/Moba/Runtime/Decision/State/${name}.cs`);
route('Content/Moba', 'Example/Moba/Runtime/Content');
route('Editor/Gameplay/Moba', 'Example/Moba/Editor/Authoring');
route('Tooling/CodeGen/Moba', 'Example/Moba/Editor/CodeGen');
route('Integration/Synchronization/Moba', 'Example/Moba/Runtime/Synchronization');
route('Integration/Ecs/Entitas/Moba', 'Example/Moba/Runtime/Entity/Generated');
route('Integration/Hosting/Client/Moba', 'Example/Moba/Unity/Hosting');
route('Integration/Networking/Moba', 'Example/Moba/Unity/Networking');
route('Integration/Unity/Input/Moba', 'Example/Moba/Unity/Input');
route('Presentation/Moba', 'Example/Moba/Unity/Presentation');
route('Presentation/Shared', 'Example/Moba/Unity/Presentation/Shared');
route('Presentation/Client', 'Unity/Presentation');
route('Gameplay/Moba/Client', 'Example/Moba/Unity/Client');
route('Gameplay/Moba/Presentation', 'Example/Moba/Runtime/Presentation');
route('Gameplay/Moba/Composition', 'Example/Moba/Runtime/Composition');
route('Gameplay/Moba/Hosting', 'Example/Moba/Runtime/Hosting');
route('Gameplay/Moba/Shared', 'Example/Moba/Runtime/Shared');
route('Gameplay/Moba/Protocol', 'Example/Moba/Runtime/Protocol');
route('Gameplay/Moba/Diagnostics', 'Example/Moba/Runtime/Diagnostics/TraceAdapter');
route('Gameplay/Moba/Editor/Editor', 'Example/Moba/Editor');
route('Gameplay/Shooter/Shared', 'Example/Shooter/Runtime/Shared');
route('Gameplay/Shooter/AI', 'Example/Shooter/Runtime/Decision');
route('Gameplay/Shooter/Hosting', 'Example/Shooter/Runtime/Hosting');
route('Gameplay/Shooter/Protocol', 'Example/Shooter/Runtime/Protocol');
route('Gameplay/Shooter/Presentation', 'Example/Shooter/Unity/Presentation');
route('Gameplay/Shooter/Compute', 'Example/Shooter/Unity/Compute');
route('Gameplay/Shooter/Editor/Editor', 'Example/Shooter/Editor');
route('Examples/Tiny/Client/FrameSync', 'Example/Tiny/Runtime/FrameSync');
route('Examples/Tiny/Client/View', 'Example/Tiny/Unity/View');
route('Examples/Tiny/Logic/Logic', 'Example/Tiny/Runtime/Logic');
route('Examples/Tiny/Turn/Logic', 'Example/Tiny/Runtime/Turn');
route('Examples/Tiny/Turn/View', 'Example/Tiny/Unity/Turn');
route('Tests/Runtime/Moba', 'Example/Moba/Tests/Runtime');
route('Tests/Runtime/MobaClient', 'Example/Moba/Tests/Client');
route('Tests/MobaClient', 'Example/Moba/Tests/LegacyClient');
route('Tests/Scenarios/Schema', 'Runtime/Battle/Testing/Scenario');
route('Tests/Scenarios/Battle', 'Runtime/Battle/Testing/BattleScenario');
const testRoutes = {
  ability: 'Editor/Authoring/Ability/Tests',
  'base.editor': 'Editor/Workspace/Tests',
  behaviortree: 'Editor/Authoring/Decision/BehaviorTree/Tests',
  'combat.motion': 'Runtime/Movement/Tests',
  'compute.unityjobs': 'Unity/Compute/Tests',
  'demo.common': 'Example/Shared/Tests',
  'demo.moba.editor': 'Example/Moba/Tests/Editor',
  'demo.moba.view.runtime': 'Example/Moba/Tests/View',
  'demo.shooter.jobs': 'Example/Shooter/Tests/Compute',
  'demo.shooter.view.runtime': 'Example/Shooter/Tests/View',
  'demo.tiny': 'Example/Tiny/Tests/Client',
  'demo.tiny.logic': 'Example/Tiny/Tests/Logic',
  'demo.tiny.turn': 'Example/Tiny/Tests/Turn',
  hfsm: 'Unity/Decision/StateMachine/Tests',
  pipeline: 'Editor/Authoring/Ability/Pipeline/Tests',
  triggering: 'Editor/Authoring/Rules/Tests',
};
for (const [name, to] of Object.entries(testRoutes)) route('Tests/Unity/' + name, to);
const editorRoutes = {
  ability: 'Editor/Authoring/Ability',
  analyzer: 'Editor/Validation/Analysis',
  battlescenario: 'Editor/Testing/BattleScenario',
  behaviortree: 'Editor/Authoring/Decision/BehaviorTree',
  core: 'Editor/Workspace/Core',
  diagnostics: 'Editor/Debugging/Metrics',
  gameframework: 'Editor/Dependencies/GameFramework',
  gameplaytags: 'Editor/Authoring/Tags',
  hfsm: 'Editor/Authoring/Decision/StateMachine',
  'network.sdk': 'Editor/Debugging/Networking',
  pipeline: 'Editor/Authoring/Ability/Pipeline',
  samples: 'Example/Foundation/Editor',
  trace: 'Editor/Debugging/Trace',
  triggering: 'Editor/Authoring/Rules',
};
for (const [name, to] of Object.entries(editorRoutes)) route('Editor/Modules/' + name, to);
route('Editor/Workspace/Editor', 'Editor/Workspace');
route('Editor/ImportExport/Protocol/Editor', 'Editor/ImportExport/Protocol');
route('Editor/Authoring/Timeline/Extensions', 'Unity/Content/Timeline/Extensions');
route('Editor/Debugging/AbilityExplain/Editor', 'Editor/Debugging/AbilityExplain');
route('Editor/Debugging/AbilityExplain', 'Runtime/Battle/Diagnostics/AbilityExplain');
route('Battle/Ability/Common/Config', 'Runtime/Content/Definitions/Triggering');
route('Battle/Ability/Common/Ability/HotReload', 'Runtime/Content/HotReload');
route('Battle/Ability/Common', 'Runtime/Ability');
route('Battle/Ability/Pipeline', 'Runtime/Ability/Pipeline');
route('Battle/Actor/Attributes/Attributes', 'Runtime/Attribute');
route('Battle/Actor/Modifiers', 'Runtime/Attribute/Modifiers');
route('Battle/Actor/Contracts', 'Runtime/Entity/Contracts');
route('Battle/Actor/Tags/Ability', 'Runtime/Entity/Tags/Ability');
route('Battle/Actor/Tags', 'Runtime/Entity/Tags');
route('Battle/Actor/Decision/BehaviorTree', 'Runtime/Decision/BehaviorTree');
route('Battle/Actor/Decision/Behavior', 'Runtime/Decision/Behavior');
route('Battle/Actor/Decision/StateMachine/HFSM/Core', 'Runtime/Decision/StateMachine');
route('Battle/Core/EntityIndex', 'Runtime/Entity/Index');
route('Battle/Core/EntityStorage/AbilityKit.World.ECS', 'Runtime/Entity/Storage');
route('Battle/Core/EntityStorage', 'Runtime/Entity/Storage');
route('Battle/Core/World', 'Runtime/Battle/World');
route('Battle/Core/World/Examples', 'Example/Foundation/World');
route('Battle/Core/ExecutionContext', 'Runtime/Battle/ExecutionContext');
route('Battle/Core/Diagnostics', 'Runtime/Battle/Diagnostics');
route('Battle/Core/Requests', 'Runtime/Battle/Commands');
route('Battle/Core/Rules', 'Runtime/Battle/Rules');
route('Battle/Core/AbilityServices', 'Runtime/Ability/WorldServices');
route('Battle/Effect/Common', 'Runtime/Effect');
route('Battle/Effect/Calculation', 'Runtime/Effect/Calculation');
route('Battle/Effect/Continuous', 'Runtime/Effect/Continuous');
route('Battle/Effect/Damage', 'Runtime/Effect/Damage');
route('Battle/Spatial/Targeting', 'Runtime/Targeting');
route('Battle/Spatial/Collision', 'Runtime/Movement/Collision');
route('Battle/Spatial/Motion/MotionSystem', 'Runtime/Movement/Motion');
route('Battle/Spatial/Motion', 'Runtime/Movement/Motion');
route('Battle/Spatial/Navigation', 'Runtime/Movement/Navigation');
route('Battle/Spatial/Projectile/Projectile', 'Runtime/Projectile');
route('Battle/Spatial/Projectile', 'Runtime/Projectile');
route('Content/Catalog/SkillLibrary', 'Runtime/Content/Catalog/SkillLibrary');
route('Content/Timeline/Exchange/Ability/Share/ActionSchema', 'Runtime/Content/Definitions/Timeline');
route('Content/Timeline/Exchange', 'Runtime/Content/Definitions/Timeline');
route('Content/Config/Ability/Core', 'Runtime/Content/Contracts');
route('Content/Config/Ability/Database', 'Runtime/Content/Catalog/Database');
route('Content/Config/Ability/Deserializers', 'Runtime/Content/Serialization/Json');
route('Content/Config/Ability/Loaders', 'Runtime/Content/Sources');
route('Content/Config/Ability', 'Runtime/Content/Definitions/Ability');
route('Foundation/Core/Pooling', 'Runtime/Pooling');
route('Foundation/Core/Buffers', 'Runtime/Pooling/Buffers');
route('Foundation/Core', 'Runtime/Foundation');
route('Foundation/Math/Deterministic', 'Runtime/Foundation/Math/Deterministic');
route('Foundation/Math', 'Runtime/Foundation/Math/Deterministic');
route('Foundation/Time/Core', 'Runtime/Foundation/Time');
route('Foundation/Time/Unity', 'Unity/Time');
route('Foundation/Time', 'Runtime/Foundation/Time');
route('Foundation/Flow/Flow', 'Runtime/Foundation/Flow');
route('Foundation/Flow', 'Runtime/Foundation/Flow');
route('Integration/Unity/Ability', 'Runtime/Ability/Shared');
route('Integration/Unity/Core', 'Unity/Foundation');
route('Integration/Unity/StateMachine', 'Unity/Decision/StateMachine');
route('Integration/Unity/Pooling/Pool', 'Unity/Pooling');
route('Integration/Unity/Pooling', 'Unity/Pooling');
route('Integration/AI/Contracts', 'Runtime/Decision/Contracts');
route('Integration/AI/MLAgents', 'Unity/Decision/MLAgents');
route('Integration/Compute/Managed', 'Runtime/Foundation/Compute');
route('Integration/Compute/UnityJobs', 'Unity/Compute');
route('Integration/Ecs/Entitas', 'Runtime/Entity/Adapters/Entitas');
route('Integration/Ecs/Svelto', 'Unity/Entity/Adapters/Svelto');
route('Integration/Hosting', 'Runtime/Battle/Hosting');
route('Integration/Hosting/Extensions/Example', 'Example/Foundation/Hosting');
route('Integration/HotReload/HotReload/ConfigReloadDebugListener.cs', 'Unity/Content/HotReload/ConfigReloadDebugListener.cs');
route('Integration/HotReload', 'Runtime/Content/HotReload/Services');
route('Integration/Networking', 'Runtime/Battle/Networking');
route('Integration/Serialization', 'Runtime/Battle/Serialization');
route('Integration/Synchronization', 'Runtime/Battle/Synchronization');
route('Integration/GameFramework/Core', 'Runtime/Foundation/Dependencies/GameFramework/Core');
route('Integration/GameFramework/Networking', 'Runtime/Foundation/Dependencies/GameFramework/Networking');
route('Tooling/Analysis', 'Runtime/Battle/Diagnostics/Analysis');
route('ThirdParty/ActionEditor/ActionEditor/Editor', 'Editor/Dependencies/ActionEditor');
route('ThirdParty/ActionEditor/ActionEditor/Runtime', 'Unity/Content/Timeline/ActionEditor');
route('ThirdParty/ActionEditor', 'Unity/Content/Timeline/ActionEditor/Distribution');
route('ThirdParty/Luban', 'Runtime/Content/Serialization/Luban');
route('ThirdParty/Rvo2/RVO2', 'Runtime/Movement/Dependencies/Rvo2');
route('ThirdParty/Rvo2', 'Runtime/Movement/Dependencies/Rvo2');
route('ThirdParty/Unsafe', 'Runtime/Foundation/Dependencies/Unsafe');
route('ThirdParty/Svelto', 'Unity/Entity/Dependencies/Svelto');
route('ThirdParty/Entitas/Entitas/Editor', 'Editor/Dependencies/Entitas');
route('ThirdParty/Entitas/Entitas', 'Runtime/Entity/Dependencies/Entitas');
route('ThirdParty/DesperateDevs/DesperateDevs_Editor/Editor', 'Editor/Dependencies/DesperateDevs');
route('ThirdParty/DesperateDevs/EditorStubs', 'Editor/Dependencies/DesperateDevs/Stubs');
route('ThirdParty/DesperateDevs/DesperateDevs', 'Runtime/Foundation/Dependencies/DesperateDevs');
rules.sort((a, b) => b.from.length - a.from.length);
function mapLocal(local) {
  const plain = local.endsWith('.meta') ? local.slice(0, -5) : local;
  if (/^ThirdParty\/Entitas\/Entitas\/[^/]*Unity[^/]*\.dll$/.test(plain))
    return 'Unity/Entity/Dependencies/Entitas/' + path.posix.basename(local);
  const rule = rules.find(r => plain === r.from || plain.startsWith(r.from + '/'));
  if (rule) return rule.to + local.slice(rule.from.length);
  if (plain.startsWith('Editor/')) return local;
  if (!plain.includes('/')) {
    const targets = { Battle: 'Runtime/Battle', Content: 'Runtime/Content', Foundation: 'Runtime/Foundation',
      Gameplay: 'Example', Integration: 'Unity', Presentation: 'Unity/Presentation', Tests: '@archive/FolderMetadata/Tests',
      ThirdParty: '@archive/FolderMetadata/ThirdParty', Tooling: '@archive/FolderMetadata/Tooling' };
    if (targets[plain]) return targets[plain] + (local.endsWith('.meta') ? '.meta' : '');
  }
  // Retain metadata for removed grouping folders outside the four-directory source tree.
  if (local.endsWith('.meta') && fs.existsSync(full(base + '/' + plain)) && fs.statSync(full(base + '/' + plain)).isDirectory())
    return '@archive/FolderMetadata/' + local;
  throw new Error('Unmapped file: ' + local);
}
function mappedAbsolute(local) {
  const mapped = mapLocal(local);
  return mapped.startsWith('@archive/') ? full('Docs/GameBattle/Reorganization/Archive/' + mapped.slice(9))
    : full(base + '/' + mapped);
}
function boundaries(files) {
  const defs = files.filter(f => f.endsWith('.asmdef') && !hidden(f)).map(f => ({
    file: f, dir: path.dirname(f), ...JSON.parse(read(f)), guid: read(f + '.meta').match(/^guid:\s*(\S+)/m)?.[1],
  }));
  const by = new Map(defs.flatMap(a => [[a.name, a], ['GUID:' + a.guid, a]]));
  const refs = files.filter(f => f.endsWith('.asmref') && !hidden(f)).map(f => ({
    file: f, dir: path.dirname(f), name: by.get(JSON.parse(read(f)).reference)?.name,
  }));
  if (refs.some(r => !r.name)) throw new Error('Unresolved current asmref: ' + refs.filter(r => !r.name).map(r => r.file));
  return { defs, by, all: [...defs, ...refs].sort((a, b) => b.dir.length - a.dir.length) };
}
function owner(file, bounds) {
  return hidden(file) ? null : bounds.find(b => file.startsWith(b.dir + path.sep))?.name || null;
}
function makeMeta(file, folder = false) {
  if (fs.existsSync(file + '.meta') || hidden(file)) return;
  const importer = file.endsWith('.asmref') ? 'AssemblyDefinitionReferenceImporter' :
    file.endsWith('.asmdef') ? 'AssemblyDefinitionImporter' : file.endsWith('.cs') ? 'MonoImporter' : 'DefaultImporter';
  const scriptFields = importer === 'MonoImporter' ?
    '  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n' : '';
  write(file + '.meta', `fileFormatVersion: 2\nguid: ${crypto.randomBytes(16).toString('hex')}\n${folder ? 'folderAsset: yes\n' : ''}${importer}:\n  externalObjects: {}\n${scriptFields}  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
}
function folders(dir) {
  if (dir === full(base)) return;
  if (!dir.startsWith(full(base) + path.sep)) return;
  folders(path.dirname(dir));
  fs.mkdirSync(dir, { recursive: true });
  makeMeta(dir, true);
}
const planFile = path.join(evidence, 'plan.json');
const action = process.argv[2] || 'plan';
if (action === 'plan' || action === 'replan') {
  if (fs.existsSync(planFile)) {
    if (action !== 'replan') throw new Error('Existing plan; inspect it rather than overwrite the baseline.');
    const previous = JSON.parse(read(planFile));
    if (previous.state !== 'planned' || previous.files.some(m => !fs.existsSync(full(m.from)) || hash(full(m.from)) !== m.sha256))
      throw new Error('Cannot replan after source changes or relocation.');
  }
  const files = walk(full(base));
  const b = boundaries(files);
  const moves = files.map(from => ({
    from: slash(path.relative(root, from)), to: slash(path.relative(root, mappedAbsolute(slash(path.relative(full(base), from))))),
    sha256: hash(from), assembly: from.endsWith('.cs') ? owner(from, b.all) : undefined,
  }));
  const seen = new Map();
  for (const m of moves) {
    const key = m.to.toLowerCase();
    if (seen.has(key)) {
      if (!m.from.endsWith('.meta') || !fs.statSync(full(m.from.slice(0, -5))).isDirectory())
        throw new Error('Destination collision: ' + m.from + ' / ' + seen.get(key).from + ' => ' + m.to);
      m.to = 'Docs/GameBattle/Reorganization/Archive/FolderMetadata/' + m.from.slice(base.length + 1);
    }
    seen.set(m.to.toLowerCase(), m);
    checked(full(m.from)); checked(full(m.to));
    if (m.from !== m.to && fs.existsSync(full(m.to)) && !files.includes(full(m.to)))
      throw new Error('Destination already exists: ' + m.to);
  }
  const plan = {
    date: '2026-10-26', root: base, state: 'planned',
    stages: { inventory: 'complete', relocation: 'pending', references: 'pending', boundaries: 'pending', verification: 'pending' },
    rules, files: moves,
    assemblies: b.defs.map(a => ({ name: a.name, guid: a.guid, from: slash(path.relative(root, a.file)), to: moves.find(m => full(m.from) === a.file).to })),
    added: [], rewritten: [], ownershipAdjustments: [],
  };
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log(JSON.stringify({ stage: 'planned', files: moves.length, scripts: moves.filter(m => m.from.endsWith('.cs')).length,
    assemblies: plan.assemblies.length, destinations: [...new Set(moves.map(m => m.to.split('/').slice(0, 7).join('/')))].slice(0, 25) }, null, 2));
} else if (action === 'apply') {
  const plan = JSON.parse(read(planFile));
  if (!['planned', 'moving'].includes(plan.state)) throw new Error('Already moved; use finalize or verify.');
  const movingSources = new Set(plan.files.filter(m => m.from !== m.to).map(m => m.from.toLowerCase()));
  for (const m of plan.files) {
    if (m.from === m.to) continue;
    const staged = full('Docs/GameBattle/Reorganization/Staging/' + m.from);
    if (fs.existsSync(staged)) {
      if (hash(staged) !== m.sha256) throw new Error('Staged file changed: ' + m.from);
    } else if (fs.existsSync(full(m.to)) && !fs.existsSync(full(m.from))) {
      if (hash(full(m.to)) !== m.sha256) throw new Error('Resumed destination changed: ' + m.to);
    } else if (!fs.existsSync(full(m.from)) || hash(full(m.from)) !== m.sha256) {
      throw new Error('Source changed after inventory: ' + m.from);
    } else if (fs.existsSync(full(m.to)) && !movingSources.has(m.to.toLowerCase())) {
      throw new Error('Refusing to overwrite: ' + m.to);
    }
  }
  plan.state = 'moving';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  for (const m of plan.files) {
    if (m.from === m.to || !fs.existsSync(full(m.from))) continue;
    const staged = full('Docs/GameBattle/Reorganization/Staging/' + m.from);
    if (fs.existsSync(staged)) continue;
    fs.mkdirSync(path.dirname(staged), { recursive: true });
    fs.renameSync(checked(full(m.from)), checked(staged));
  }
  for (const m of plan.files) {
    if (m.from === m.to) continue;
    const staged = full('Docs/GameBattle/Reorganization/Staging/' + m.from);
    if (!fs.existsSync(staged)) continue;
    if (fs.existsSync(full(m.to))) throw new Error('Destination remains occupied: ' + m.to);
    fs.mkdirSync(path.dirname(full(m.to)), { recursive: true });
    fs.renameSync(checked(staged), checked(full(m.to)));
  }
  plan.state = 'moved'; plan.stages.relocation = 'complete';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log({ stage: 'moved', files: plan.files.length });
} else if (action === 'finalize') {
  const plan = JSON.parse(read(planFile));
  if (!['moved', 'finalizing'].includes(plan.state)) throw new Error('Requires completed relocation.');
  plan.state = 'finalizing';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  const pairs = new Map();
  for (const m of plan.files.filter(m => m.from !== m.to && !m.from.endsWith('.meta'))) {
    pairs.set(m.from, m.to);
    if (m.to.startsWith('Unity/')) pairs.set(m.from.slice(6), m.to.slice(6));
    if (m.to.startsWith('Unity/')) {
      pairs.set(full(m.from).replaceAll('\\', '/'), full(m.to).replaceAll('\\', '/'));
    }
  }
  for (const r of rules) {
    if (r.to.startsWith('@')) continue;
    pairs.set(base + '/' + r.from, base + '/' + r.to);
    pairs.set(base.slice(6) + '/' + r.from, base.slice(6) + '/' + r.to);
  }
  const variants = new Map([...pairs].flatMap(([a, b]) => [[a, b], [a.replaceAll('/', '\\'), b.replaceAll('/', '\\')]]));
  const escape = s => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const pattern = new RegExp([...variants.keys()].sort((a, b) => b.length - a.length).map(escape).join('|'), 'g');
  const textExtensions = /\.(cs|csproj|props|targets|json|ps1|cjs|js|py|sh|bat|cmd|xml|uxml|uss|yaml|yml|asset|prefab|unity|txt)$/;
  for (const file of walk(root)) {
    if (!textExtensions.test(file) || file.startsWith(full('Docs') + path.sep) ||
      file.startsWith(__dirname + path.sep) || hidden(file)) continue;
    const bytes = fs.readFileSync(file);
    if (bytes.includes(0)) continue;
    const text = bytes.toString('utf8');
    const next = text.replace(pattern, old => variants.get(old));
    if (next !== text) {
      const rel = slash(path.relative(root, file));
      const backup = path.join(evidence, 'ReferenceBackups', rel);
      if (!fs.existsSync(backup)) write(backup, bytes);
      write(file, next);
      if (!plan.rewritten.includes(rel)) plan.rewritten.push(rel);
    }
  }
  plan.stages.references = 'complete';
  // Empty old parents are removed only after every file is journaled at its new location.
  function prune(dir) {
    checked(dir);
    for (const e of fs.readdirSync(dir, { withFileTypes: true })) if (e.isDirectory() && !skip.has(e.name)) prune(path.join(dir, e.name));
    if (dir !== full(base) && !fs.readdirSync(dir).length) fs.rmdirSync(dir);
  }
  prune(full(base));
  for (const name of ['Battle', 'Entity', 'Attribute', 'Ability', 'Effect', 'Buff', 'Targeting', 'Movement', 'Projectile',
    'Decision', 'Content', 'Foundation', 'Pooling']) folders(full(base + '/Runtime/' + name));
  for (const name of ['Editor', 'Unity', 'Example']) folders(full(base + '/' + name));
  const files = walk(full(base));
  const b = boundaries(files);
  const defs = new Map(b.defs.map(a => [a.name, a]));
  const expected = plan.files.filter(m => m.from.endsWith('.cs') && m.assembly && !hidden(m.to));
  for (const m of expected) {
    if (owner(full(m.to), b.all) === m.assembly) continue;
    const dir = path.dirname(full(m.to));
    const ref = path.join(dir, m.assembly + '.asmref');
    if (fs.existsSync(ref)) throw new Error('Cannot repair assembly at ' + dir);
    const other = b.all.find(a => a.dir === dir);
    if (other) throw new Error('Mixed assembly ownership: ' + m.to + ' / ' + other.name);
    write(ref, JSON.stringify({ reference: 'GUID:' + defs.get(m.assembly).guid }, null, 2) + '\n');
    makeMeta(ref);
    b.all.push({ dir, name: m.assembly, file: ref });
    b.all.sort((a, b) => b.dir.length - a.dir.length);
    plan.added.push(slash(path.relative(root, ref)));
  }
  for (const file of walk(full(base))) {
    if (hidden(file)) continue;
    folders(path.dirname(file));
    if (!file.endsWith('.meta')) makeMeta(file);
  }
  plan.stages.boundaries = 'complete'; plan.state = 'finalized';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log({ stage: 'finalized', rewritten: plan.rewritten.length, asmrefsAdded: plan.added.length });
} else if (action === 'boundaries') {
  const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
  const plan = JSON.parse(read(planFile));
  if (plan.state !== 'finalized') throw new Error('Requires finalized paths.');
  for (const m of plan.files.filter(m => m.to.startsWith(base + '/') && m.to.endsWith('.meta') && !fs.existsSync(full(m.to)))) {
    const archived = 'Docs/GameBattle/Reorganization/Archive/EmptyFolderMetadata/' + m.to.slice(base.length + 1);
    if (fs.existsSync(full(archived)) && hash(full(archived)) === m.sha256) m.to = archived;
  }
  const editorMeta = plan.files.find(m => m.from === base + '/Editor.meta');
  if (editorMeta && editorMeta.to !== editorMeta.from && fs.existsSync(full(editorMeta.to))) {
    const created = full(editorMeta.from);
    if (fs.existsSync(created)) {
      const archive = full('Docs/GameBattle/Reorganization/Archive/GeneratedMetadata/Editor.meta');
      fs.mkdirSync(path.dirname(archive), { recursive: true });
      fs.renameSync(checked(created), checked(archive));
    }
    fs.renameSync(checked(full(editorMeta.to)), checked(created));
    editorMeta.to = editorMeta.from;
  }
  const b = boundaries(walk(full(base)));
  const runtimeOwners = new Set(walk(full(base + '/Runtime')).filter(f => f.endsWith('.cs') &&
    !hidden(f) && !/[\\/]Tests[\\/]/.test(f)).map(f => owner(f, b.all)));
  for (const name of runtimeOwners) {
    const a = b.defs.find(d => d.name === name);
    if (!a) throw new Error('Runtime source outside named assembly: ' + name);
    const data = JSON.parse(read(a.file));
    data.noEngineReferences = true;
    data.references = (data.references || []).filter(r => !/^(Unity|UnityEngine|UnityEditor)(\.|$)/.test(r));
    if (['AbilityKit.Analyzer', 'AbilityKit.Record.MemoryPack'].includes(name) &&
      !data.references.includes('AbilityKit.Core')) data.references.push('AbilityKit.Core');
    write(a.file, JSON.stringify(data, null, 2) + '\n');
  }
  const unityCore = b.defs.find(a => a.name === 'AbilityKit.Core.Unity');
  const unityData = JSON.parse(read(unityCore.file));
  for (const name of ['AbilityKit.Pipeline', 'AbilityKit.Diagnostics', 'AbilityKit.Ability'])
    if (!unityData.references.includes(name)) unityData.references.push(name);
  write(unityCore.file, JSON.stringify(unityData, null, 2) + '\n');
  const debug = base + '/Unity/Content/HotReload/ConfigReloadDebugListener.cs';
  const debugDir = path.dirname(full(debug));
  for (const file of fs.readdirSync(debugDir).filter(f => f.endsWith('.asmref'))) {
    const target = path.join(debugDir, file);
    write(target, JSON.stringify({ reference: 'GUID:' + unityCore.guid }, null, 2) + '\n');
  }
  if (!plan.ownershipAdjustments.some(a => a.file === debug))
    plan.ownershipAdjustments.push({ file: debug, assembly: unityCore.name, reason: 'Editor-only Unity startup hook.' });
  plan.cacheMoves ||= [];
  function cacheWalk(dir) {
    if (!fs.existsSync(dir)) return;
    for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
      if (!e.isDirectory()) continue;
      const source = path.join(dir, e.name);
      if (skip.has(e.name)) {
        const rel = slash(path.relative(full(base), source));
        const target = full('Docs/GameBattle/Reorganization/Archive/BuildCaches/' + rel);
        if (fs.existsSync(target)) throw new Error('Cache archive already exists: ' + target);
        fs.mkdirSync(path.dirname(target), { recursive: true });
        fs.renameSync(checked(source), checked(target));
        plan.cacheMoves.push({ from: slash(path.relative(root, source)), to: slash(path.relative(root, target)) });
      } else cacheWalk(source);
    }
  }
  for (const old of ['Integration', 'ThirdParty']) cacheWalk(full(base + '/' + old));
  function prune(dir) {
    if (!fs.existsSync(dir)) return;
    checked(dir);
    for (const e of fs.readdirSync(dir, { withFileTypes: true })) if (e.isDirectory()) prune(path.join(dir, e.name));
    if (dir !== full(base) && !fs.readdirSync(dir).length) fs.rmdirSync(dir);
  }
  for (const old of ['Integration', 'ThirdParty']) prune(full(base + '/' + old));
  for (const file of walk(full(base)).filter(f => f.endsWith('.meta') && !hidden(f) && !fs.existsSync(f.slice(0, -5)))) {
    const rel = slash(path.relative(full(base), file));
    const target = full('Docs/GameBattle/Reorganization/Archive/EmptyFolderMetadata/' + rel);
    if (fs.existsSync(target)) throw new Error('Metadata archive already exists: ' + target);
    fs.mkdirSync(path.dirname(target), { recursive: true });
    fs.renameSync(checked(file), checked(target));
    const item = plan.files.find(m => full(m.to) === file);
    if (item) item.to = slash(path.relative(root, target));
  }
  for (const name of ['Battle', 'Entity', 'Attribute', 'Ability', 'Effect', 'Buff', 'Targeting', 'Movement', 'Projectile',
    'Decision', 'Content', 'Foundation', 'Pooling']) folders(full(base + '/Runtime/' + name));
  for (const file of walk(full(base))) if (!hidden(file)) {
    folders(path.dirname(file));
    if (!file.endsWith('.meta')) makeMeta(file);
  }
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  const newSources = {
    'AbilityKit.Core': [base + '/Runtime/Foundation/RuntimeEnvironment.cs'],
    'AbilityKit.Core.Unity': [base + '/Unity/Foundation/RuntimeEnvironmentBootstrap.cs',
      base + '/Unity/Foundation/UnityRuntimeDiagnostics.cs', debug],
  };
  function patchProject(file, assembly, generated) {
    if (!fs.existsSync(file)) return;
    const text = read(file);
    const document = new DOMParser().parseFromString(text, 'application/xml');
    const project = document.documentElement;
    const ns = project.namespaceURI;
    const create = name => document.createElementNS(ns, name);
    let changed = false;
    const compile = Array.from(document.getElementsByTagName('Compile'));
    const has = new Set(compile.flatMap(e => e.getAttribute('Include').split(';')).map(s => slash(s)));
    const group = create('ItemGroup');
    for (const source of newSources[assembly] || []) {
      const relative = slash(path.relative(path.dirname(file), full(source)));
      if (has.has(relative)) continue;
      const element = create('Compile');
      element.setAttribute('Include', generated ? relative.replaceAll('/', '\\') : relative);
      group.appendChild(element); changed = true;
    }
    if (group.childNodes.length) project.appendChild(group);
    if (generated && assembly === unityCore.name) {
      for (const dependency of unityData.references) {
        const target = full('Unity/' + dependency + '.csproj');
        if (!fs.existsSync(target)) continue;
        const rel = dependency + '.csproj';
        if (Array.from(document.getElementsByTagName('ProjectReference')).some(e => e.getAttribute('Include') === rel)) continue;
        const g = create('ItemGroup'), e = create('ProjectReference');
        e.setAttribute('Include', rel);
        const name = create('Name'); name.appendChild(document.createTextNode(dependency));
        e.appendChild(name); g.appendChild(e); project.appendChild(g); changed = true;
      }
    }
    if (generated && ['AbilityKit.Analyzer', 'AbilityKit.Record.MemoryPack'].includes(assembly)) {
      const g = create('ItemGroup'), e = create('ProjectReference');
      if (!Array.from(document.getElementsByTagName('ProjectReference')).some(e => e.getAttribute('Include') === 'AbilityKit.Core.csproj')) {
        e.setAttribute('Include', 'AbilityKit.Core.csproj'); g.appendChild(e); project.appendChild(g); changed = true;
      }
    }
    if (assembly === 'AbilityKit.HotReload') for (const e of compile) {
      const include = e.getAttribute('Include');
      const next = include.split(';').filter(s => !slash(s).endsWith('/Unity/Content/HotReload/ConfigReloadDebugListener.cs'));
      if (next.length !== include.split(';').length) {
        if (next.length) e.setAttribute('Include', next.join(';')); else e.parentNode.removeChild(e);
        changed = true;
      }
    }
    if (generated && runtimeOwners.has(assembly)) {
      for (const e of Array.from(document.getElementsByTagName('Reference'))) {
        if (/^(UnityEngine|UnityEditor)(\.|$)/.test(e.getAttribute('Include'))) {
          e.parentNode.removeChild(e); changed = true;
        }
      }
    }
    if (changed) write(file, new XMLSerializer().serializeToString(document));
  }
  for (const name of fs.readdirSync(full('Unity')).filter(n => n.endsWith('.csproj')))
    patchProject(full('Unity/' + name), name.slice(0, -7), true);
  for (const name of ['AbilityKit.Core', 'AbilityKit.HotReload'])
    patchProject(full('src/' + name + '/' + name + '.csproj'), name, false);
  plan.stages.boundaries = 'complete';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log({ stage: 'boundaries', runtimeAssemblies: runtimeOwners.size, cachesArchived: plan.cacheMoves.length });
} else if (action === 'verify') {
  const plan = JSON.parse(read(planFile));
  const files = walk(full(base));
  const b = boundaries(files);
  const problems = [];
  const changed = [];
  const adapterChanges = new Set([
    'Runtime/Ability/Pipeline/Pipeline.cs',
    'Runtime/Entity/Storage/Impl/EntityWorld.cs',
    'Runtime/Battle/Diagnostics/Analysis/AbilityKit.Analyzer/Configuration/ConstraintLoader.cs',
    'Runtime/Battle/Diagnostics/Analysis/DotNet~/AbilityKit.Analyzer/Config/ConstraintLoader.cs',
    'Runtime/Battle/Diagnostics/Metrics/Core/ProfilerHub.cs',
    'Runtime/Battle/Rules/Plans/Serialization/Json/Loading/TriggerPlanDirectoryLoader.cs',
    'Runtime/Battle/Serialization/MemoryPackRecord/FrameRecord/FrameRecordMemoryPackCodec.cs',
    'Runtime/Battle/Synchronization/Replay/Record/Core/Session/RecordFileStore.cs',
    ...['FrameRecordBinaryReader', 'FrameRecordBinaryWriter', 'FrameRecordJsonReader', 'FrameRecordJsonWriter',
      'FrameRecordOptimizedBinaryDataCodec'].map(name => 'Runtime/Battle/Synchronization/Replay/Record/FrameRecord/' + name + '.cs'),
  ].map(local => base + '/' + local));
  const currentDestinations = new Set(plan.files.map(m => m.to.toLowerCase()));
  for (const m of plan.files) {
    if (!fs.existsSync(full(m.to))) problems.push('Missing: ' + m.to);
    else if (hash(full(m.to)) !== m.sha256) {
      changed.push(m.to);
      if (m.from.endsWith('.meta')) problems.push('Metadata changed: ' + m.to);
      if (m.from.endsWith('.cs') && !plan.rewritten.includes(m.to) && !adapterChanges.has(m.to))
        problems.push('Unaccounted source modification: ' + m.to);
    }
    if (m.from !== m.to && fs.existsSync(full(m.from)) && !currentDestinations.has(m.from.toLowerCase()))
      problems.push('Old file remains: ' + m.from);
    if (m.from.endsWith('.cs') && !hidden(m.to)) {
      const actual = owner(full(m.to), b.all);
      const approved = plan.ownershipAdjustments.find(a => a.file === m.to)?.assembly || m.assembly;
      if (actual !== approved) problems.push('Assembly changed: ' + m.to + ': ' + m.assembly + ' -> ' + actual);
    }
  }
  const top = fs.readdirSync(full(base), { withFileTypes: true }).filter(e => e.isDirectory()).map(e => e.name).sort();
  if (top.join(',') !== 'Editor,Example,Runtime,Unity') problems.push('Unexpected root directories: ' + top);
  const guids = new Map();
  for (const file of files.filter(f => f.endsWith('.meta') && !hidden(f))) {
    const guid = read(file).match(/^guid:\s*(\S+)/m)?.[1];
    if (guid && guids.has(guid)) problems.push('Duplicate GUID: ' + file + ' / ' + guids.get(guid));
    if (guid) guids.set(guid, file);
    if (!fs.existsSync(file.slice(0, -5))) problems.push('Orphan metadata: ' + file);
  }
  for (const a of plan.assemblies) {
    const current = b.defs.find(d => d.name === a.name);
    if (!current || current.guid !== a.guid) problems.push('Assembly identity changed: ' + a.name);
  }
  const runtimeFiles = files.filter(f => f.startsWith(full(base + '/Runtime') + path.sep) && f.endsWith('.cs') && !hidden(f));
  const runtimeOwners = new Set(runtimeFiles.filter(f => !/[\\/]Tests[\\/]/.test(f)).map(f => owner(f, b.all)));
  for (const name of runtimeOwners) {
    const a = b.defs.find(a => a.name === name);
    if (!a || !a.noEngineReferences) problems.push('Runtime assembly permits engine references: ' + name);
    if (a) for (const ref of a.references || []) {
      const target = b.by.get(ref);
      if (/^(Unity|UnityEngine|UnityEditor)(\.|$)/.test(ref)) problems.push('Engine assembly reference: ' + name + ' -> ' + ref);
      if (target && (target.file.startsWith(full(base + '/Example') + path.sep) ||
        target.file.startsWith(full(base + '/Editor') + path.sep) ||
        target.file.startsWith(full(base + '/Unity') + path.sep))) problems.push('Runtime outward dependency: ' + name + ' -> ' + target.name);
    }
  }
  // Ignore comments and strings when checking actual engine API use.
  for (const file of runtimeFiles) {
    const text = read(file).replace(/\/\*[\s\S]*?\*\/|\/\/[^\r\n]*|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"/g, '');
    if (/\b(UnityEngine|UnityEditor|Unity\.(Mathematics|Collections|Jobs|Burst))\b/.test(text))
      problems.push('Engine API in Runtime: ' + slash(path.relative(root, file)));
  }
  const report = { date: '2026-10-26', files: plan.files.length, scripts: plan.files.filter(m => m.from.endsWith('.cs')).length,
    top, runtimeScripts: runtimeFiles.length, runtimeAssemblies: runtimeOwners.size, changed, problems };
  write(path.join(evidence, 'verification.json'), JSON.stringify(report, null, 2) + '\n');
  plan.stages.verification = problems.length ? 'failed' : 'complete';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log(JSON.stringify({ files: report.files, scripts: report.scripts, top: report.top,
    runtimeScripts: report.runtimeScripts, runtimeAssemblies: report.runtimeAssemblies,
    accountedChanges: report.changed.length, problems: report.problems }, null, 2));
  if (problems.length) process.exitCode = 1;
} else throw new Error('Unknown action: ' + action);
