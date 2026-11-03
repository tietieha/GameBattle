// This migration changes source ownership and paths, not C# behavior or assembly identities.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
const root = path.resolve(__dirname, '../..');
const unity = path.join(root, 'Unity');
const sourceRoot = path.join(unity, 'Packages');
const destinationRoot = path.join(unity, 'Assets/Scripts/GameBattle');
const evidenceRoot = path.join(root, 'Docs/GameBattle/Migration');
const slash = s => s.replaceAll('\\', '/');
const relative = p => slash(path.relative(root, p));
const read = p => fs.readFileSync(p, 'utf8');
const hash = buffer => crypto.createHash('sha256').update(buffer).digest('hex');
const skip = new Set(['.git', 'Library', 'Temp', 'obj', 'bin', 'node_modules', '.idea']);
function walk(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).sort((a, b) => a.name.localeCompare(b.name)).flatMap(e => {
    const file = path.join(dir, e.name);
    return e.isDirectory() ? skip.has(e.name) ? [] : walk(file) : e.isFile() ? [file] : [];
  });
}
const roots = {
  core: 'Foundation/Core', deterministic: 'Foundation/Math', timer: 'Foundation/Time', flow: 'Foundation/Flow',
  context: 'Battle/Core/ExecutionContext', trace: 'Battle/Core/Diagnostics/Trace', diagnostics: 'Battle/Core/Diagnostics/Metrics',
  'world.di': 'Battle/Core/World', 'world.ecs': 'Battle/Core/EntityStorage',
  'combat.entitymanager': 'Battle/Core/EntityIndex', 'game.battle.runtime': 'Battle/Core/Requests',
  attributes: 'Battle/Actor/Attributes', modifiers: 'Battle/Actor/Modifiers', gameplaytags: 'Battle/Actor/Tags',
  behavior: 'Battle/Actor/Decision/Behavior', behaviortree: 'Battle/Actor/Decision/BehaviorTree', hfsm: 'Battle/Actor/Decision/StateMachine',
  ability: 'Battle/Ability/Common', pipeline: 'Battle/Ability/Pipeline', 'combat.skilllibrary': 'Content/Catalog/SkillLibrary',
  triggering: 'Battle/Core/Rules', continuous: 'Battle/Effect/Continuous', dataflow: 'Battle/Effect/Calculation',
  'combat.damage': 'Battle/Effect/Damage', 'combat.targeting': 'Battle/Spatial/Targeting',
  'combat.motion': 'Battle/Spatial/Motion', 'combat.collision.abstractions': 'Battle/Spatial/Collision',
  'combat.navigation': 'Battle/Spatial/Navigation', 'combat.projectile': 'Battle/Spatial/Projectile',
  'base.editor': 'Editor/Workspace', 'actioneditor.impl': 'Editor/Authoring/Timeline/Extensions',
  actionschema: 'Content/Timeline/Exchange', 'ability.explain': 'Editor/Debugging/AbilityExplain',
  'excel-sync': 'Editor/ImportExport/Excel', analyzer: 'Tooling/Analysis',
  scenario: 'Tests/Scenarios/Schema', battlescenario: 'Tests/Scenarios/Battle',
  samples: 'Examples/Foundation', 'demo.starter': 'Examples/Launcher', 'demo.common': 'Examples/Shared',
  'demo.moba.runtime': 'Gameplay/Moba/Runtime', 'demo.moba.share': 'Gameplay/Moba/Shared',
  'demo.moba.view.runtime': 'Gameplay/Moba/Client', 'demo.moba.view.abstractions': 'Gameplay/Moba/Presentation/Contracts',
  'demo.moba.host': 'Gameplay/Moba/Hosting', 'demo.moba.environment': 'Gameplay/Moba/Composition',
  'demo.moba.editor': 'Gameplay/Moba/Editor', 'demo.moba.codegen': 'Tooling/CodeGen/Moba',
  'demo.moba.trace.adapter': 'Gameplay/Moba/Diagnostics',
  'demo.shooter.runtime': 'Gameplay/Shooter/Runtime', 'demo.shooter.share': 'Gameplay/Shooter/Shared',
  'demo.shooter.host': 'Gameplay/Shooter/Hosting', 'demo.shooter.view.runtime': 'Gameplay/Shooter/Presentation',
  'demo.shooter.editor': 'Gameplay/Shooter/Editor', 'demo.shooter.jobs': 'Gameplay/Shooter/Compute',
  'demo.shooter.ai': 'Gameplay/Shooter/AI', 'demo.tiny': 'Examples/Tiny/Client',
  'demo.tiny.logic': 'Examples/Tiny/Logic', 'demo.tiny.turn': 'Examples/Tiny/Turn',
  'game.view.runtime': 'Presentation/Client',
  'ai.abstractions': 'Integration/AI/Contracts', 'ai.mlagents.bridge': 'Integration/AI/MLAgents',
  environment: 'Integration/Hosting/Configuration', host: 'Integration/Hosting/Host',
  'host.extension': 'Integration/Hosting/Extensions', 'host.network': 'Integration/Networking/HostingAdapter',
  coordinator: 'Integration/Hosting/Sessions', hotreload: 'Integration/HotReload',
  compute: 'Integration/Compute/Managed', 'compute.unityjobs': 'Integration/Compute/UnityJobs',
  'unity.pool': 'Integration/Unity/Pooling', 'world.entitas': 'Integration/Ecs/Entitas',
  'world.svelto': 'Integration/Ecs/Svelto', 'world.framesync': 'Integration/Synchronization/FrameSync',
  'world.statesync': 'Integration/Synchronization/StateSync', 'world.snapshot': 'Integration/Synchronization/Snapshot',
  'world.networkfragments': 'Integration/Synchronization/Frames', record: 'Integration/Synchronization/Replay',
  'record.memorypack': 'Integration/Serialization/MemoryPackRecord',
  'network.runtime': 'Integration/Networking/Core', 'network.sdk': 'Integration/Networking/Sdk',
  'network.host': 'Integration/Networking/Server', 'network.battle': 'Integration/Networking/Battle',
  'network.battle.config': 'Integration/Networking/Configuration', 'network.room': 'Integration/Networking/Room',
  'network.client': 'Integration/Networking/Client', 'network.transport.inmemory': 'Integration/Networking/Transport/InMemory',
  'network.transport.litenet': 'Integration/Networking/Transport/LiteNet',
  'network.transport.websocket': 'Integration/Networking/Transport/WebSocket',
  protocol: 'Integration/Serialization/Protocol', 'protocol.editor': 'Editor/ImportExport/Protocol',
  'protocol.room': 'Integration/Networking/Room/Protocol', 'protocol.moba': 'Gameplay/Moba/Protocol',
  'protocol.shooter': 'Gameplay/Shooter/Protocol', gameframework: 'Integration/GameFramework/Core',
  'gameframework.network': 'Integration/GameFramework/Networking',
  'thirdparty.actioneditor': 'ThirdParty/ActionEditor', 'thirdparty.entitas': 'ThirdParty/Entitas',
  'thirdparty.desperatedevs': 'ThirdParty/DesperateDevs', 'thirdparty.svelto': 'ThirdParty/Svelto',
  'thirdparty.luban.runtime': 'ThirdParty/Luban', 'thirdparty.rvo2': 'ThirdParty/Rvo2',
  'thirdparty.unsafe': 'ThirdParty/Unsafe',
};
const overrides = [];
const route = (pkg, old, target) => overrides.push({ pkg, old, target });
for (const [pkg, base] of Object.entries(roots)) {
  route(pkg, 'Runtime', base);
  route(pkg, 'Editor', ['Editor', 'Gameplay', 'ThirdParty'].some(x => base.startsWith(x + '/'))
    ? base + '/Editor' : 'Editor/Modules/' + pkg);
  route(pkg, 'Tests', 'Tests/Unity/' + pkg);
  route(pkg, 'Samples~', 'Examples/Optional~/' + pkg);
  route(pkg, 'Samples', 'Examples/Modules/' + pkg);
  route(pkg, 'Documentation~', 'Documentation~/Modules/' + pkg);
  route(pkg, 'Document', 'Documentation~/Modules/' + pkg + '/Document');
  route(pkg, 'Document~', 'Documentation~/Modules/' + pkg + '/Document');
}
route('core', 'Runtime/Unity', 'Integration/Unity/Core');
route('hfsm', 'Runtime/HFSM/Unity', 'Integration/Unity/StateMachine');
route('ability', 'Runtime/Ability/Config', 'Content/Config/Ability');
route('ability', 'Runtime/Ability/Effect', 'Battle/Effect/Common');
route('ability', 'Runtime/Ability/Tags', 'Battle/Actor/Tags/Ability');
route('ability', 'Runtime/Ability/World', 'Battle/Core/AbilityServices');
route('ability', 'Runtime/Ability/ECS', 'Battle/Actor/Contracts');
route('ability', 'Runtime/Ability/Impl', 'Integration/Unity/Ability');
const serviceGroups = {
  Actor: 'Actor', Unit: 'Actor', Tags: 'Actor', EntityConstruction: 'Core', EntityManager: 'Core',
  Behavior: 'Actor', StateMachine: 'Actor', ComponentTemplates: 'Actor',
  Skill: 'Ability', Passive: 'Ability',
  Buffs: 'Effect', Effect: 'Effect', Combat: 'Effect', Continuous: 'Effect', ContinuousPeriodic: 'Effect',
  Area: 'Spatial', Motion: 'Spatial', Navigation: 'Spatial', Map: 'Spatial', Projectile: 'Spatial',
  Search: 'Spatial', Spawn: 'Spatial', Summon: 'Spatial',
  Context: 'Core', Core: 'Core', Diagnostics: 'Core', Observability: 'Core', Lifecycle: 'Core',
  LogicWorld: 'Core', Input: 'Core', IO: 'Core', Triggering: 'Core', Validation: 'Core', Templates: 'Core',
};
for (const [name, group] of Object.entries(serviceGroups)) {
  route('demo.moba.runtime', 'Runtime/Application/Services/' + name, `Battle/${group}/Moba/Services/${name}`);
  route('demo.moba.runtime', 'Runtime/Application/Systems/' + name, `Battle/${group}/Moba/Systems/${name}`);
}
for (const name of ['FrameSync', 'Snapshot', 'StateImport', 'StateSync', 'Projection', 'Routing', 'Coedc'])
  route('demo.moba.runtime', 'Runtime/Application/Services/' + name, 'Integration/Synchronization/Moba/' + name);
route('demo.moba.runtime', 'Runtime/Application/Rollback', 'Integration/Synchronization/Moba/Rollback');
route('demo.moba.runtime', 'Runtime/Infrastructure/Config', 'Content/Moba/Config');
route('demo.moba.runtime', 'Runtime/Worlds', 'Gameplay/Moba/Composition/Worlds');
route('demo.moba.runtime', 'Runtime/Common/Shared/Entitas', 'Integration/Ecs/Entitas/Moba/Generated');
route('demo.moba.runtime', 'Runtime/Testing', 'Tests/Runtime/Moba');
route('demo.moba.runtime', 'Runtime/Docs', 'Documentation~/Moba/Runtime');
route('demo.moba.runtime', 'DiagnosticsCore', 'Battle/Core/Diagnostics/Moba');
for (const [name, group] of Object.entries({
  Ability: 'Ability', ActionTimeline: 'Ability', Attributes: 'Actor', Behavior: 'Actor',
  Actions: 'Effect', Input: 'Core', Events: 'Core', Predicates: 'Core', Triggering: 'Core',
})) route('demo.moba.runtime', 'Runtime/Domain/' + name, `Battle/${group}/Moba/Domain/${name}`);
route('demo.moba.runtime', 'Runtime/Domain/Components', 'Battle/Actor/Moba/Components');
route('demo.moba.share', 'Runtime/Game/Config', 'Content/Moba/Definitions');
route('demo.moba.share', 'Runtime/Game/Flow/Battle/Presentation', 'Presentation/Moba/Contracts');
route('demo.moba.view.runtime', 'Runtime/Game/Battle/Presentation', 'Presentation/Moba');
route('demo.moba.view.runtime', 'Runtime/Game/Battle/Shared', 'Presentation/Shared');
route('demo.moba.view.runtime', 'Runtime/Game/Battle/Input', 'Integration/Unity/Input/Moba');
route('demo.moba.view.runtime', 'Runtime/Game/Battle/Client', 'Integration/Hosting/Client/Moba');
route('demo.moba.view.runtime', 'Runtime/Game/Battle/Networking', 'Integration/Networking/Moba');
route('demo.moba.view.runtime', 'Runtime/Game/Battle/Testing', 'Tests/Runtime/MobaClient');
route('demo.moba.view.runtime', 'Runtime/Game/Test', 'Tests/MobaClient');
route('demo.moba.view.runtime', 'Game', 'Examples/Moba/LegacyGame');
route('demo.moba.view.runtime', 'Resources', 'Examples/Moba/Resources');
route('demo.moba.view.runtime', 'Configs', 'Examples/Moba/Configs');
route('demo.moba.view.runtime', 'Scenes', 'Examples/Moba/Scenes');
route('demo.moba.view.runtime', 'Prefabs', 'Examples/Moba/Prefabs');
route('demo.moba.view.runtime', 'Models', 'Examples/Moba/Models');
route('demo.moba.editor', 'Editor', 'Editor/Gameplay/Moba');
route('battlescenario', 'Editor', 'Editor/BattleTesting');
for (const pkg of Object.keys(roots)) {
  if (pkg.startsWith('demo.shooter.') || ['demo.tiny', 'demo.tiny.logic', 'demo.tiny.turn'].includes(pkg)) {
    for (const dir of ['Resources', 'Scenes', 'Configs', 'Prefabs'])
      route(pkg, dir, 'Examples/' + (pkg.startsWith('demo.shooter.') ? 'Shooter' : 'Tiny') + '/' + pkg + '/' + dir);
  }
}
overrides.sort((a, b) => b.old.length - a.old.length);
function mapLocal(pkg, local) {
  if (['package.json', 'package.json.meta'].includes(local))
    return 'Documentation~/PackageManifests/' + pkg + '/' + local;
  if (local.endsWith('.meta') && !fs.existsSync(path.join(sourceRoot, 'com.abilitykit.' + pkg, local.slice(0, -5))))
    return 'Documentation~/OrphanMetadata/' + pkg + '/' + local;
  if (!local.includes('/') && !/\.(cs|dll|asmdef|asmref)(\.meta)?$/.test(local) && !local.endsWith('.meta'))
    return 'Documentation~/PackageRoots/' + pkg + '/' + local;
  if (/^(README|LICENSE|CHANGELOG|NOTICE)/i.test(local) && !local.includes('/'))
    return 'Documentation~/PackageRoots/' + pkg + '/' + local;
  // A split parent keeps its old metadata at its default ownership location.
  const plain = local.endsWith('.meta') ? local.slice(0, -5) : local;
  const rule = overrides.find(r => r.pkg === pkg && (plain === r.old || plain.startsWith(r.old + '/')));
  if (rule) return rule.target + local.slice(rule.old.length);
  return roots[pkg] + '/' + local;
}
function write(file, text) { fs.mkdirSync(path.dirname(file), { recursive: true }); fs.writeFileSync(file, text); }
function meta(file, folder = false) {
  if (fs.existsSync(file + '.meta')) return;
  const guid = crypto.randomBytes(16).toString('hex');
  write(file + '.meta', `fileFormatVersion: 2\nguid: ${guid}\n${folder ? 'folderAsset: yes\n' : ''}DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
}
function ensureFolders(dir) {
  if (dir === path.join(unity, 'Assets') || !dir.startsWith(path.join(unity, 'Assets') + path.sep)) return;
  ensureFolders(path.dirname(dir));
  fs.mkdirSync(dir, { recursive: true });
  if (!slash(dir).split('/').some(x => x.endsWith('~'))) meta(dir, true);
}
const execute = process.argv.includes('--apply');
if (fs.existsSync(destinationRoot)) throw new Error('GameBattle destination already exists; migration is deliberately not overwriteable.');
const packageDirs = fs.readdirSync(sourceRoot, { withFileTypes: true })
  .filter(e => e.isDirectory() && e.name.startsWith('com.abilitykit.')).map(e => e.name).sort();
const unknown = packageDirs.filter(n => !roots[n.slice(15)]);
if (unknown.length) throw new Error('Unmapped packages: ' + unknown);
const moves = [], assemblies = [];
for (const name of packageDirs) {
  const pkg = name.slice(15);
  for (const source of walk(path.join(sourceRoot, name))) {
    const local = slash(path.relative(path.join(sourceRoot, name), source));
    const destination = path.join(destinationRoot, mapLocal(pkg, local));
    moves.push({ source, destination, pkg, local, hash: hash(fs.readFileSync(source)) });
    if (source.endsWith('.asmdef')) {
      const data = JSON.parse(read(source).replace(/^\uFEFF/, ''));
      const guid = fs.existsSync(source + '.meta') ? read(source + '.meta').match(/^guid:\s*(\S+)/m)?.[1] : null;
      assemblies.push({ source, destination, name: data.name, guid });
    }
  }
}
const destinations = new Set();
for (const move of moves) {
  const key = move.destination.toLowerCase();
  if (destinations.has(key)) throw new Error('Destination collision: ' + move.destination + '\n' + moves.filter(x => x.destination.toLowerCase() === key).map(x => x.source).join('\n'));
  destinations.add(key);
  if (!path.resolve(move.source).startsWith(path.resolve(sourceRoot) + path.sep) ||
      !path.resolve(move.destination).startsWith(path.resolve(destinationRoot) + path.sep))
    throw new Error('Migration path escaped its verified root.');
}
const moveBySource = new Map(moves.map(m => [slash(m.source).toLowerCase(), m]));
const assembliesByDepth = [...assemblies].sort((a, b) => b.source.length - a.source.length);
function originalOwner(source) {
  return assembliesByDepth.find(a => source.startsWith(path.dirname(a.source) + path.sep));
}
// Build original ownership requirements before Unity can observe any intermediate move.
const ownership = moves.filter(m => m.source.endsWith('.cs')).map(m => ({ ...m, owner: originalOwner(m.source) }));
const plannedOwners = new Map();
for (const m of ownership) {
  const dir = path.dirname(m.destination);
  const owner = m.owner?.name || null;
  if (plannedOwners.has(dir) && plannedOwners.get(dir) !== owner)
    throw new Error('Mixed source assembly ownership: ' + dir);
  plannedOwners.set(dir, owner);
}
const pathPairs = moves.filter(m => !m.source.endsWith('.meta')).map(m => [
  slash(path.relative(unity, m.source)), slash(path.relative(unity, m.destination)),
]).sort((a, b) => b[0].length - a[0].length);
const prefixPairs = packageDirs.map(n => ['Packages/' + n, 'Assets/Scripts/GameBattle/' + roots[n.slice(15)]]);
for (const r of overrides) prefixPairs.push(['Packages/com.abilitykit.' + r.pkg + '/' + r.old, 'Assets/Scripts/GameBattle/' + r.target]);
prefixPairs.sort((a, b) => b[0].length - a[0].length);
const replacements = new Map([...prefixPairs, ...pathPairs]);
const oldPaths = [...replacements.keys()].sort((a, b) => b.length - a.length);
const escape = x => x.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
const pattern = new RegExp(oldPaths.map(escape).join('|'), 'g');
const replacePaths = text => {
  text = text.replace(pattern, old => replacements.get(old));
  const backslashPattern = new RegExp(oldPaths.map(x => escape(x.replaceAll('/', '\\'))).join('|'), 'g');
  text = text.replace(backslashPattern, old => replacements.get(old.replaceAll('\\', '/')).replaceAll('/', '\\'));
  // C# escaped Windows literals have two backslash characters per separator.
  const escapedPattern = new RegExp(oldPaths.map(x => escape(x.replaceAll('/', '\\\\'))).join('|'), 'g');
  return text.replace(escapedPattern, old => replacements.get(old.replaceAll('\\\\', '/')).replaceAll('/', '\\\\'));
};
// Expand only Compile items whose package paths are being moved. This preserves source sets
// even when one original wildcard now spans several capability directories.
const projects = walk(root).filter(f => f.endsWith('.csproj') && !f.startsWith(unity + path.sep) ||
  f.endsWith('.csproj') && f.startsWith(sourceRoot + path.sep));
const projectEdits = [];
for (const file of projects) {
  const text = read(file);
  if (!text.includes('com.abilitykit.')) continue;
  const doc = new DOMParser().parseFromString(text, 'application/xml');
  let count = 0;
  for (const item of Array.from(doc.getElementsByTagName('Compile'))) {
    for (const attribute of ['Include', 'Remove']) {
      const value = item.getAttribute(attribute);
      if (!value || !value.includes('com.abilitykit.')) continue;
      if (value.includes('$(')) {
        if (value.replaceAll('$(RepoRoot)', '').includes('$('))
          throw new Error('Unknown MSBuild variable in moved item: ' + file + ' ' + value);
      }
      const patterns = value.split(';').map(x => slash(x.replaceAll('$(RepoRoot)', root)));
      const excludes = (item.getAttribute('Exclude') || '').split(';').filter(Boolean).map(slash);
      const matches = [...fs.globSync(patterns, { cwd: path.dirname(file), exclude: excludes })]
        .map(f => path.resolve(path.dirname(file), f)).filter(f => fs.statSync(f).isFile());
      const mapped = matches.map(f => moveBySource.get(slash(path.resolve(f)).toLowerCase())?.destination || path.resolve(f));
      if (matches.length) {
        item.setAttribute(attribute, mapped.map(f => slash(path.relative(path.dirname(file), f))).join(';'));
        if (attribute === 'Include') item.removeAttribute('Exclude');
      } else {
        item.setAttribute(attribute, replacePaths(value));
      }
      count += matches.length;
    }
  }
  let output = new XMLSerializer().serializeToString(doc);
  output = replacePaths(output);
  // A moved project computes its relative references from its new physical location.
  const projectMove = moveBySource.get(slash(file).toLowerCase());
  if (projectMove) {
    const edited = new DOMParser().parseFromString(output, 'application/xml');
    for (const tag of ['ProjectReference', 'Import']) {
      for (const node of Array.from(edited.getElementsByTagName(tag))) {
        const key = tag === 'Import' ? 'Project' : 'Include';
        const originalValue = Array.from(doc.getElementsByTagName(tag))[Array.from(edited.getElementsByTagName(tag)).indexOf(node)]?.getAttribute(key);
        if (!originalValue || originalValue.includes('$(')) continue;
        const resolved = path.resolve(path.dirname(file), originalValue.replaceAll('\\', '/'));
        const target = moveBySource.get(slash(resolved).toLowerCase())?.destination || resolved;
        node.setAttribute(key, slash(path.relative(path.dirname(projectMove.destination), target)));
      }
    }
    output = new XMLSerializer().serializeToString(edited);
  }
  if (output !== text) projectEdits.push({ file, output, count });
}
const baseline = {
  date: '2026-10-23', root: relative(destinationRoot), packages: packageDirs.length,
  files: moves.map(m => ({ from: relative(m.source), to: relative(m.destination), sha256: m.hash })),
  assemblyIdentities: assemblies.map(a => ({ name: a.name, guid: a.guid, from: relative(a.source), to: relative(a.destination) })),
  sourceOwnership: ownership.map(m => ({ to: relative(m.destination), assembly: m.owner?.name || null })),
};
write(path.join(evidenceRoot, 'plan.json'), JSON.stringify(baseline, null, 2));
console.log(JSON.stringify({ mode: execute ? 'apply' : 'dry-run', packages: packageDirs.length,
  files: moves.length, scripts: ownership.length, assemblies: assemblies.length, projects: projectEdits.length }, null, 2));
if (!execute) process.exit(0);
// Move leaf files, including original .meta files, before generating metadata for new parents.
for (const m of moves) {
  fs.mkdirSync(path.dirname(m.destination), { recursive: true });
  fs.renameSync(m.source, m.destination);
}
const movedAssemblyDirs = assemblies.map(a => ({ dir: path.dirname(a.destination), ...a })).sort((a, b) => b.dir.length - a.dir.length);
const asmrefs = [];
const ownerByDirectory = new Map();
for (const m of ownership) {
  const dir = path.dirname(m.destination);
  const key = m.owner?.name || null;
  if (ownerByDirectory.has(dir) && ownerByDirectory.get(dir) !== key)
    throw new Error('Mixed assembly ownership in directory: ' + dir);
  ownerByDirectory.set(dir, key);
}
for (const [dir, name] of [...ownerByDirectory].sort((a, b) => a[0].length - b[0].length)) {
  if (!name) continue;
  const inherited = [
    ...movedAssemblyDirs.filter(a => dir === a.dir || dir.startsWith(a.dir + path.sep)).map(a => ({ dir: a.dir, name: a.name })),
    ...asmrefs.filter(a => dir === a.dir || dir.startsWith(a.dir + path.sep)),
  ].sort((a, b) => b.dir.length - a.dir.length)[0];
  if (inherited?.name === name) continue;
  const owner = assemblies.find(a => a.name === name);
  const file = path.join(dir, name + '.asmref');
  write(file, JSON.stringify({ reference: owner.guid ? 'GUID:' + owner.guid : owner.name }, null, 2) + '\n');
  meta(file);
  asmrefs.push({ dir, name, file });
}
for (const edit of projectEdits) {
  const target = moveBySource.get(slash(edit.file).toLowerCase())?.destination || edit.file;
  write(target, edit.output);
}
const textExtensions = new Set(['.cs', '.json', '.asset', '.unity', '.prefab', '.uxml', '.uss', '.shader',
  '.compute', '.sh', '.ps1', '.cmd', '.bat', '.py', '.js', '.cjs', '.props', '.targets', '.yaml', '.yml', '.xml']);
const textFiles = [
  ...walk(destinationRoot),
  ...walk(path.join(root, 'src')), ...walk(path.join(root, 'Server')), ...walk(path.join(root, 'tools')),
  ...walk(path.join(unity, 'ProjectSettings')),
  ...walk(path.join(unity, 'Assets')).filter(f => !f.startsWith(destinationRoot + path.sep)),
].filter(f => textExtensions.has(path.extname(f)) && f !== __filename);
const rewritten = [];
for (const file of textFiles) {
  const bytes = fs.readFileSync(file);
  if (bytes.includes(0)) continue;
  const text = bytes.toString('utf8');
  const output = replacePaths(text);
  if (output !== text) { write(file, output); rewritten.push(relative(file)); }
}
// Embedded packages are no longer installed. Keep all external dependencies explicitly.
const manifestFile = path.join(sourceRoot, 'manifest.json');
const manifest = JSON.parse(read(manifestFile));
for (const name of Object.keys(manifest.dependencies)) if (name.startsWith('com.abilitykit.')) delete manifest.dependencies[name];
for (const pkg of packageDirs) {
  const archived = path.join(destinationRoot, 'Documentation~/PackageManifests', pkg.slice(15), 'package.json');
  for (const [name, version] of Object.entries(JSON.parse(read(archived)).dependencies || {})) {
    if (!name.startsWith('com.abilitykit.') && !manifest.dependencies[name]) manifest.dependencies[name] = version;
  }
}
write(manifestFile, JSON.stringify(manifest, null, 2) + '\n');
const lockFile = path.join(sourceRoot, 'packages-lock.json');
if (fs.existsSync(lockFile)) {
  const lock = JSON.parse(read(lockFile));
  for (const name of Object.keys(lock.dependencies || {})) if (name.startsWith('com.abilitykit.')) delete lock.dependencies[name];
  for (const dep of Object.values(lock.dependencies || {}))
    for (const name of Object.keys(dep.dependencies || {})) if (name.startsWith('com.abilitykit.')) delete dep.dependencies[name];
  write(lockFile, JSON.stringify(lock, null, 2) + '\n');
}
// Remove only verified empty source directories. Generated/cache directories are retained.
function removeEmpty(dir) {
  if (!path.resolve(dir).startsWith(path.resolve(sourceRoot) + path.sep)) throw new Error('Empty cleanup outside Packages.');
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    if (e.isDirectory() && !skip.has(e.name)) removeEmpty(path.join(dir, e.name));
  }
  if (!fs.readdirSync(dir).length) fs.rmdirSync(dir);
}
for (const pkg of packageDirs) {
  const dir = path.join(sourceRoot, pkg);
  if (fs.existsSync(dir)) removeEmpty(dir);
}
for (const m of moves) ensureFolders(path.dirname(m.destination));
const checks = moves.map(m => ({ ...m, currentHash: hash(fs.readFileSync(m.destination)) }));
const changedScripts = checks.filter(m => m.source.endsWith('.cs') && m.currentHash !== m.hash);
for (const m of changedScripts) {
  // Only path literal/comment substitutions are allowed in the initial migration.
  const before = require('node:child_process').execFileSync('git', ['show', 'HEAD:' + relative(m.source)], { cwd: root, maxBuffer: 10 * 1024 * 1024 }).toString('utf8');
  if (replacePaths(before) !== read(m.destination))
    console.warn('Source includes pre-existing edits or requires review: ' + relative(m.destination));
}
write(path.join(evidenceRoot, 'result.json'), JSON.stringify({
  date: '2026-10-23', files: moves.length, scripts: ownership.length, assemblies: assemblies.length,
  assemblyReferencesAdded: asmrefs.length, projectsUpdated: projectEdits.length,
  pathRewrites: rewritten, scriptsWithPathChanges: changedScripts.map(m => relative(m.destination)),
  missingDestinations: moves.filter(m => !fs.existsSync(m.destination)).map(m => relative(m.destination)),
}, null, 2));
console.log(JSON.stringify({ moved: moves.length, assemblyReferencesAdded: asmrefs.length,
  rewrittenFiles: rewritten.length, scriptsWithPathChanges: changedScripts.length }, null, 2));
