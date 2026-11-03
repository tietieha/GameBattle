const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const cp = require('node:child_process');
const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
const root = path.resolve(__dirname, '../..');
const base = 'Unity/Assets/Scripts/GameBattle';
const evidence = 'Docs/GameBattle/BuffExtraction';
const full = f => path.resolve(root, f);
const read = f => fs.readFileSync(full(f), 'utf8');
const hash = f => crypto.createHash('sha256').update(fs.readFileSync(full(f))).digest('hex');
const write = (f, text) => {
  fs.mkdirSync(path.dirname(full(f)), { recursive: true });
  fs.writeFileSync(full(f), text);
};
const edited = [
  'Runtime/Buff/State/BuffState.cs',
  'Runtime/Buff/Policies/BuffStackingPolicyApplier.cs',
  'Example/Moba/Runtime/Buff/State/BuffComponent.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/Core/BuffRuntimeContexts.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/Core/BuffRepository.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/Lifecycle/BuffApplyFlow.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/Lifecycle/BuffEndFlow.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/MobaBuffService.cs',
  'Example/Moba/Runtime/Buff/Services/Buffs/Runtime/BuffContinuousRuntime.cs',
  'Example/Moba/Runtime/Effect/Services/Continuous/MobaContinuousTickProcessor.cs',
  'Example/Moba/Runtime/Battle/Services/Context/Runtime/MobaRuntimeContextService.cs',
].map(f => base + '/' + f);
const added = [
  'Runtime/Buff/Storage/BuffRuntimeKey.cs',
  'Runtime/Buff/Storage/BuffStorage.cs',
  'Runtime/Buff/Lifecycle/BuffApplyProcessor.cs',
  'Runtime/Buff/Lifecycle/BuffEndProcessor.cs',
  'Runtime/Buff/Lifecycle/BuffLifetimeReconciler.cs',
  'Runtime/Buff/Commands/BuffCommandQueue.cs',
  'Runtime/Buff/Timing/BuffDuration.cs',
  'Runtime/Buff/Context/BuffContextBinding.cs',
  'Runtime/Effect/Continuous/Timing/FixedIntervalTicker.cs',
].map(f => base + '/' + f);
const journalFile = evidence + '/changes.json';
const load = () => JSON.parse(read(journalFile));
function metadata(file, folder = false) {
  if (fs.existsSync(full(file + '.meta'))) return;
  write(file + '.meta', 'fileFormatVersion: 2\nguid: ' + crypto.randomBytes(16).toString('hex') +
    '\n' + (folder ? 'folderAsset: yes\n' : '') +
    'DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n');
}
function folders(file) {
  const parent = path.posix.dirname(file);
  if (parent === base || !parent.startsWith(base + '/')) return;
  folders(parent);
  fs.mkdirSync(full(parent), { recursive: true });
  metadata(parent, true);
}
const action = process.argv[2];
if (action === 'plan') {
  if (fs.existsSync(full(journalFile))) throw new Error('Buff journal already exists.');
  const files = edited.flatMap(f => [f, f + '.meta']).map(file => {
    write(evidence + '/Backups/' + file, fs.readFileSync(full(file)));
    return { from: file, to: file, sha256: hash(file) };
  });
  write(journalFile, JSON.stringify({ date: '2026-10-26', state: 'planned', routes: [], files, added, rewritten: [] }, null, 2) + '\n');
  console.log('Recorded Buff baseline: ' + files.length);
} else if (action === 'storage') {
  const source = read(evidence + '/Backups/' + base + '/Example/Moba/Runtime/Buff/Services/Buffs/Core/BuffRepository.cs');
  const index = source.indexOf('        public static int FindExistingBuffIndex');
  const methods = source.slice(index).replace(/\bBuffRuntime\b/g, 'TRuntime');
  write(base + '/Runtime/Buff/Storage/BuffStorage.cs',
    'using System.Collections.Generic;\nusing System.Runtime.CompilerServices;\n\nnamespace AbilityKit.Buffs\n{\n' +
    '    public static class BuffStorage<TRuntime> where TRuntime : BuffState\n    {\n' +
    '        private static readonly ConditionalWeakTable<List<TRuntime>, BuffRuntimeIndex> s_indices =\n' +
    '            new ConditionalWeakTable<List<TRuntime>, BuffRuntimeIndex>();\n\n' +
    '        public static void Forget(List<TRuntime> list)\n        {\n            if (list != null) s_indices.Remove(list);\n        }\n\n' +
    methods);
  console.log('Extracted generic indexed storage from original implementation.');
} else if (action === 'links') {
  for (const file of added) { folders(file); metadata(file); }
  const project = 'src/AbilityKit.Demo.Moba.Core/AbilityKit.Demo.Moba.Core.csproj';
  const document = new DOMParser().parseFromString(read(project), 'application/xml');
  let changed = false;
  for (const file of added) {
    const include = path.relative(path.dirname(full(project)), full(file)).replaceAll('\\', '/');
    if (Array.from(document.getElementsByTagName('Compile')).some(n => n.getAttribute('Include') === include)) continue;
    const group = document.createElement('ItemGroup'), node = document.createElement('Compile');
    node.setAttribute('Include', include); group.appendChild(node); document.documentElement.appendChild(group);
    changed = true;
  }
  if (changed) {
    const backup = evidence + '/Backups/' + project;
    if (!fs.existsSync(full(backup))) write(backup, read(project));
    write(project, new XMLSerializer().serializeToString(document));
  }
  console.log('Metadata and standalone source links updated.');
} else if (action === 'seal') {
  const journal = load();
  journal.rewritten = journal.files.filter(f => hash(f.to) !== f.sha256)
    .map(f => ({ file: f.to, beforeSha256: f.sha256, afterSha256: hash(f.to) }));
  journal.created = journal.added.map(file => ({ file, sha256: hash(file), metadataSha256: hash(file + '.meta') }));
  journal.state = 'implemented';
  write(journalFile, JSON.stringify(journal, null, 2) + '\n');
  console.log('Sealed Buff changes: ' + journal.rewritten.length);
} else if (action === 'verify') {
  const journal = load(), problems = [];
  for (const item of journal.files) {
    const expected = journal.rewritten.find(r => r.file === item.to)?.afterSha256 || item.sha256;
    if (hash(item.to) !== expected) problems.push('Source/metadata drift: ' + item.to);
  }
  for (const item of journal.created) {
    if (hash(item.file) !== item.sha256 || hash(item.file + '.meta') !== item.metadataSha256)
      problems.push('Created source/metadata drift: ' + item.file);
    if (/UnityEngine|UnityEditor|AbilityKit\.Demo\.|Entitas/.test(read(item.file)))
      problems.push('Runtime boundary violation: ' + item.file);
  }
  const report = { date: journal.date, changedSources: journal.rewritten.length, newRuntimeSources: journal.created.length,
    preservedOriginalMetadata: journal.files.filter(f => f.from.endsWith('.meta')).length, problems };
  write(evidence + '/verification.json', JSON.stringify(report, null, 2) + '\n');
  console.log(JSON.stringify(report, null, 2));
  if (problems.length) process.exitCode = 1;
} else if (action === 'build') {
  const names = JSON.parse(read('Docs/GameBattle/AssemblyConsolidation/builds.json')).map(r => r.name);
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
  if (results.some(r => r.status !== 0)) process.exitCode = 1;
} else throw new Error('Use plan, storage, links, seal, verify or build.');
