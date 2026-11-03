const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const cp = require('node:child_process');
const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
const root = path.resolve(__dirname, '../..');
const base = 'Unity/Assets/Scripts/GameBattle';
const evidence = 'Docs/GameBattle/ExampleExtraction';
const full = file => path.resolve(root, file);
const read = file => fs.readFileSync(full(file), 'utf8');
const hash = file => crypto.createHash('sha256').update(fs.readFileSync(full(file))).digest('hex');
const write = (file, text) => {
  fs.mkdirSync(path.dirname(full(file)), { recursive: true });
  fs.writeFileSync(full(file), text);
};
const routes = [
  ['Example/Moba/Runtime/Effect/Services/Combat/CombatNumberValue.cs', 'Runtime/Effect/Calculation/CombatNumberValue.cs'],
  ['Example/Moba/Unity/Presentation/Core/PresentationCueReconciliationController.cs', 'Runtime/Battle/Presentation/PresentationCueReconciliationController.cs'],
  ['Example/Moba/Unity/Presentation/Core/PresentationCueViewRegistry.cs', 'Unity/Presentation/Cues/PresentationCueViewRegistry.cs'],
  ['Example/Moba/Unity/Presentation/Core/PresentationLifetimePolicy.cs', 'Unity/Presentation/Cues/PresentationLifetimePolicy.cs'],
  ['Example/Moba/Unity/Presentation/Core/SessionTickLoopController.cs', 'Unity/Foundation/Driving/SessionTickLoopController.cs'],
  ['Example/Moba/Unity/Presentation/Core/BattleSessionTickProjection.cs', 'Unity/Foundation/Driving/BattleSessionTickProjection.cs'],
].map(([from, to]) => ({ from: base + '/' + from, to: base + '/' + to }));
const edited = [
  'Example/Moba/Runtime/Buff/State/BuffComponent.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/Core/BuffStackingPolicyApplier.cs',
  'Example/Foundation/Abstractions/IResourceProvider.cs',
  'Example/Moba/Runtime/Battle/Services/Triggering/PlanActions/Skill/AdjustDamageNumberSchema.cs',
  'Example/Moba/Runtime/Battle/Services/Triggering/PlanActions/Skill/AdjustDamageNumberArgs.cs',
  'Example/Moba/Runtime/Battle/Services/Triggering/PlanActions/Skill/AdjustDamageNumberPlanActionModule.cs',
  'Example/Moba/Runtime/Effect/Services/Combat/Damage/DamagePipelineModels.cs',
  'Example/Moba/Unity/Presentation/Core/AbilityKit.Demo.Moba.Presentation.Core.asmdef',
].map(file => base + '/' + file).concat([
  'src/AbilityKit.Demo.Moba.Tests/CombatNumberValueTests.cs',
  'src/AbilityKit.Demo.Moba.Tests/Buff/BuffStackingPolicyApplierTests.cs',
]);
const added = [
  'Runtime/Buff/State/BuffState.cs',
  'Runtime/Buff/Policies/BuffStackingSettings.cs',
  'Runtime/Buff/Policies/BuffStackingPolicyApplier.cs',
  'Runtime/Battle/Commands/IBattleCommandSource.cs',
  'Runtime/Content/Resources/ITextResourceProvider.cs',
  'Unity/Content/Resources/UnityTextResourceProvider.cs',
].map(file => base + '/' + file);
const journalFile = evidence + '/changes.json';
const action = process.argv[2];
const load = () => JSON.parse(read(journalFile));
function metadata(file, folder = false) {
  if (fs.existsSync(full(file + '.meta'))) return;
  write(file + '.meta', 'fileFormatVersion: 2\nguid: ' + crypto.randomBytes(16).toString('hex') +
    '\n' + (folder ? 'folderAsset: yes\n' : '') +
    'DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n');
}
function directories(file) {
  const parent = path.posix.dirname(file);
  if (parent === base || !parent.startsWith(base + '/')) return;
  directories(parent);
  fs.mkdirSync(full(parent), { recursive: true });
  metadata(parent, true);
}
if (action === 'plan') {
  if (fs.existsSync(full(journalFile))) throw new Error('Extraction journal already exists.');
  const originals = [...routes.flatMap(r => [r.from, r.from + '.meta']), ...edited];
  const files = originals.map(from => {
    const route = routes.find(r => from === r.from || from === r.from + '.meta');
    const to = route ? route.to + (from.endsWith('.meta') ? '.meta' : '') : from;
    const sha256 = hash(from);
    write(evidence + '/Backups/' + from, fs.readFileSync(full(from)));
    return { from, to, sha256 };
  });
  write(journalFile, JSON.stringify({ date: '2026-10-26', state: 'planned', routes, files, added, rewritten: [] }, null, 2) + '\n');
  console.log('Extraction baseline recorded: ' + files.length + ' files.');
} else if (action === 'move') {
  const journal = load();
  if (journal.state !== 'planned') throw new Error('Move requires planned journal.');
  for (const item of journal.files.filter(item => item.from !== item.to)) {
    if (hash(item.from) !== item.sha256 || fs.existsSync(full(item.to))) throw new Error('Move conflict: ' + item.from);
    if (!full(item.from).startsWith(full(base + '/Example') + path.sep) ||
        !full(item.to).startsWith(full(base) + path.sep)) throw new Error('Outside extraction boundary.');
    directories(item.to);
    fs.renameSync(full(item.from), full(item.to));
  }
  journal.state = 'moved';
  write(journalFile, JSON.stringify(journal, null, 2) + '\n');
  console.log('Moved original sources and GUIDs: ' + routes.length);
} else if (action === 'links') {
  const journal = load();
  const projects = [
    ...fs.readdirSync(full('Unity')).filter(f => f.endsWith('.csproj')).map(f => 'Unity/' + f),
    ...fs.readdirSync(full('src'), { withFileTypes: true }).filter(f => f.isDirectory())
      .flatMap(f => fs.readdirSync(full('src/' + f.name)).filter(p => p.endsWith('.csproj')).map(p => 'src/' + f.name + '/' + p)),
  ];
  for (const project of projects) {
    const document = new DOMParser().parseFromString(read(project), 'application/xml');
    let changed = false;
    for (const node of Array.from(document.getElementsByTagName('Compile'))) {
      const parts = (node.getAttribute('Include') || '').split(';').map(include => {
        if (/[$*?]/.test(include)) return include;
        const absolute = path.resolve(path.dirname(full(project)), include);
        const route = routes.find(r => full(r.from).toLowerCase() === absolute.toLowerCase() ||
          project.startsWith('Unity/') && !project.startsWith('Unity/GameBattle.') &&
          full(r.to).toLowerCase() === absolute.toLowerCase());
        if (!route) return include;
        changed = true;
        return project.startsWith('Unity/') ? '' :
          path.relative(path.dirname(full(project)), full(route.to)).replaceAll('\\', '/');
      });
      const include = parts.filter(Boolean).join(';');
      if (include) node.setAttribute('Include', include);
      else node.parentNode.removeChild(node);
    }
    const newSources = project === 'src/AbilityKit.Demo.Moba.Core/AbilityKit.Demo.Moba.Core.csproj'
      ? added.filter(f => f.includes('/Runtime/Buff/'))
      : project === 'src/AbilityKit.Samples.Abstractions/AbilityKit.Samples.Abstractions.csproj'
        ? added.filter(f => f.endsWith('/ITextResourceProvider.cs'))
        : project === 'src/AbilityKit.Host.Extension/AbilityKit.Host.Extension.csproj'
          ? added.filter(f => f.endsWith('/IBattleCommandSource.cs')) : [];
    for (const source of newSources) {
      const include = path.relative(path.dirname(full(project)), full(source)).replaceAll('\\', '/');
      if (Array.from(document.getElementsByTagName('Compile')).some(n => n.getAttribute('Include') === include)) continue;
      const group = document.createElement('ItemGroup');
      const node = document.createElement('Compile');
      node.setAttribute('Include', include);
      group.appendChild(node);
      document.documentElement.appendChild(group);
      changed = true;
    }
    const dependencies = project === 'Unity/AbilityKit.Demo.Moba.Presentation.Core.csproj'
      ? ['GameBattle.Runtime', 'GameBattle.Unity']
      : ['Unity/AbilityKit.Demo.Moba.Presentation.Adapter.csproj', 'Unity/AbilityKit.Samples.Editor.csproj'].includes(project)
        ? ['GameBattle.Runtime'] : [];
    if (dependencies.length) {
      for (const name of dependencies) {
        const include = name + '.csproj';
        if (Array.from(document.getElementsByTagName('ProjectReference')).some(n => n.getAttribute('Include') === include)) continue;
        const group = document.createElement('ItemGroup'), node = document.createElement('ProjectReference');
        node.setAttribute('Include', include);
        group.appendChild(node);
        document.documentElement.appendChild(group);
        changed = true;
      }
    }
    if (changed) {
      const backup = evidence + '/Backups/' + project;
      if (!fs.existsSync(full(backup))) write(backup, read(project));
      write(project, new XMLSerializer().serializeToString(document));
    }
  }
  for (const file of journal.added) { directories(file); metadata(file); }
  console.log('Compile links and new source metadata updated.');
} else if (action === 'extend') {
  const journal = load();
  for (const suffix of [
    '/Runtime/Battle/Hosting/Extensions/Server/BattleHost/BattleInputBuffer.cs',
    '/Runtime/Movement/Navigation/Navigation/NavigationWorldFactory.cs',
    '/Example/Moba/Unity/Presentation/Adapter/AbilityKit.Demo.Moba.Presentation.Adapter.asmdef',
    '/Example/Foundation/Editor/AbilityKit.Samples.Editor.asmdef',
  ]) {
    const file = base + suffix;
    if (!journal.files.some(item => item.from === file)) {
      write(evidence + '/Backups/' + file, fs.readFileSync(full(file)));
      journal.files.push({ from: file, to: file, sha256: hash(file) });
    }
  }
  journal.added = added;
  write(journalFile, JSON.stringify(journal, null, 2) + '\n');
  console.log('Recorded command boundary baseline.');
} else if (action === 'seal') {
  const journal = load();
  journal.rewritten = journal.files.filter(item => hash(item.to) !== item.sha256)
    .map(item => ({ file: item.to, beforeSha256: item.sha256, afterSha256: hash(item.to) }));
  journal.created = journal.added.map(file => ({ file, sha256: hash(file), metadataSha256: hash(file + '.meta') }));
  journal.state = 'implemented';
  write(journalFile, JSON.stringify(journal, null, 2) + '\n');
  console.log('Sealed explicit extraction changes: ' + journal.rewritten.length);
} else if (action === 'build') {
  const names = JSON.parse(read('Docs/GameBattle/AssemblyConsolidation/builds.json')).map(item => item.name);
  const results = names.map(name => {
    const result = cp.spawnSync('dotnet', ['build', full('Unity/' + name + '.csproj'), '--no-restore', '--verbosity', 'quiet'],
      { cwd: root, encoding: 'utf8', maxBuffer: 32 * 1024 * 1024 });
    const log = evidence + '/BuildLogs/' + name + '.log';
    write(log, (result.stdout || '') + (result.stderr || ''));
    console.log(name + ': ' + (result.status === 0 ? 'PASS' : 'FAIL'));
    if (result.status !== 0) console.log((result.stdout || '').split('\n').filter(line => /error /i.test(line)).join('\n'));
    return { name, status: result.status, log };
  });
  write(evidence + '/builds.json', JSON.stringify(results, null, 2) + '\n');
  if (results.some(item => item.status !== 0)) process.exitCode = 1;
} else if (action === 'verify') {
  const journal = load(), problems = [];
  const buffFile = 'Docs/GameBattle/BuffExtraction/changes.json';
  const buff = fs.existsSync(full(buffFile)) ? JSON.parse(read(buffFile)) : null;
  const latest = file => buff?.state !== 'planned' ? buff?.rewritten.find(r => r.file === file)?.afterSha256 : null;
  for (const item of journal.files) {
    const rewrite = journal.rewritten.find(r => r.file === item.to);
    if (!fs.existsSync(full(item.to)) || hash(item.to) !== (latest(item.to) || rewrite?.afterSha256 || item.sha256))
      problems.push('Source/metadata drift: ' + item.to);
    if (item.from !== item.to && fs.existsSync(full(item.from))) problems.push('Original duplicate: ' + item.from);
  }
  for (const item of journal.created)
    if (hash(item.file) !== (latest(item.file) || item.sha256) || hash(item.file + '.meta') !== item.metadataSha256)
      problems.push('Created source/metadata drift: ' + item.file);
  const runtimeFiles = [...journal.files.filter(f => f.to.includes('/Runtime/') && !f.to.includes('/Example/') && f.to.endsWith('.cs')).map(f => f.to),
    ...journal.added.filter(f => f.includes('/Runtime/'))];
  for (const file of runtimeFiles)
    if (/UnityEngine|UnityEditor|AbilityKit\.Demo\.|AbilityKit\.Samples\./.test(read(file)))
      problems.push('Runtime dependency leaked: ' + file);
  const report = { date: journal.date, movedModules: journal.routes.length, newSources: journal.created.length,
    preservedOriginalGuids: journal.files.filter(f => f.from.endsWith('.meta')).length, problems };
  write(evidence + '/verification.json', JSON.stringify(report, null, 2) + '\n');
  console.log(JSON.stringify(report, null, 2));
  if (problems.length) process.exitCode = 1;
} else throw new Error('Use plan, move, extend, links, seal, build or verify.');
