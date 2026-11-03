const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const cp = require('node:child_process');
const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
const YAML = require('yaml');
const root = path.resolve(__dirname, '../..');
const base = 'Unity/Assets/Scripts/GameBattle';
const editor = base + '/Editor';
const evidence = 'Docs/GameBattle/EditorMirroring';
const full = file => path.resolve(root, file);
const relative = file => path.relative(root, file).replaceAll('\\', '/');
const read = file => fs.readFileSync(full(file), 'utf8');
const json = file => JSON.parse(read(file).replace(/^\uFEFF/, ''));
const hash = file => crypto.createHash('sha256').update(fs.readFileSync(full(file))).digest('hex');
const ignored = new Set(['.git', 'Library', 'Temp', 'Logs', 'obj', 'bin', 'node_modules', '.idea']);
function walk(dir) {
  return fs.readdirSync(full(dir), { withFileTypes: true }).flatMap(item => {
    const file = dir + '/' + item.name;
    return item.isDirectory() ? ignored.has(item.name) ? [] : walk(file) : item.isFile() ? [file] : [];
  });
}
function directories(dir) {
  return fs.readdirSync(full(dir), { withFileTypes: true }).filter(item => item.isDirectory())
    .flatMap(item => [dir + '/' + item.name, ...directories(dir + '/' + item.name)]);
}
function write(file, text) {
  fs.mkdirSync(path.dirname(full(file)), { recursive: true });
  fs.writeFileSync(full(file), text);
}
function checked(file) {
  const target = full(file);
  if (!target.startsWith(full(editor) + path.sep) && !target.startsWith(full(evidence) + path.sep))
    throw new Error('Move outside the editor migration boundary: ' + file);
  return target;
}
function move(from, to) {
  const source = checked(from), destination = checked(to);
  if (fs.existsSync(destination)) throw new Error('Destination already exists: ' + to);
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.renameSync(source, destination);
}
function replacer(plan) {
  const variants = new Map();
  for (const route of plan.routes) {
    for (const [from, to] of [[route.from, route.to], [route.from.slice(6), route.to.slice(6)]]) {
      variants.set(from, to);
      variants.set(from.replaceAll('/', '\\'), to.replaceAll('/', '\\'));
      variants.set(from.replaceAll('/', '\\\\'), to.replaceAll('/', '\\\\'));
    }
  }
  const escape = value => value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const pattern = new RegExp([...variants.keys()].sort((a, b) => b.length - a.length).map(escape).join('|'), 'g');
  return text => text.replace(pattern, value => variants.get(value));
}
function metadata(dir) {
  if (dir === editor || !dir.startsWith(editor + '/')) return;
  metadata(relative(path.dirname(full(dir))));
  fs.mkdirSync(full(dir), { recursive: true });
  if (!fs.existsSync(full(dir + '.meta')))
    write(dir + '.meta', 'fileFormatVersion: 2\nguid: ' + crypto.randomBytes(16).toString('hex') +
      '\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n');
}
const planPath = evidence + '/plan.json';
const action = process.argv[2] || 'plan';
if (action === 'plan') {
  if (fs.existsSync(full(planPath))) throw new Error('An editor mirror baseline already exists.');
  const routes = [
    ['Authoring/Ability', 'Runtime/Ability'],
    ['Authoring/Decision/BehaviorTree', 'Runtime/Decision/BehaviorTree'],
    ['Authoring/Decision/StateMachine', 'Unity/Decision/StateMachine'],
    ['Authoring/Rules', 'Runtime/Battle/Rules'],
    ['Authoring/Tags', 'Runtime/Entity/Tags'],
    ['Debugging/AbilityExplain', 'Runtime/Battle/Diagnostics/AbilityExplain'],
    ['Debugging/Metrics', 'Runtime/Battle/Diagnostics/Metrics'],
    ['Debugging/Networking', 'Runtime/Battle/Networking/Sdk'],
    ['Debugging/Trace', 'Runtime/Battle/Diagnostics/Trace'],
    ['Dependencies/ActionEditor', 'Unity/Content/Timeline/ActionEditor'],
    ['Dependencies/DesperateDevs', 'Runtime/Foundation/Dependencies/DesperateDevs'],
    ['Dependencies/Entitas', 'Unity/Entity/Dependencies/Entitas'],
    ['Dependencies/GameFramework', 'Runtime/Foundation/Dependencies/GameFramework'],
    ['Dependencies/Svelto/PoolDebugger', 'Unity/Entity/Dependencies/Svelto/common/ObjectPool'],
    ['ImportExport/Excel/Editor', 'Runtime/Content/Serialization/Excel'],
    ['ImportExport/Protocol/ProtocolEditor', 'Runtime/Battle/Serialization/Protocol/ProtocolEditor'],
    ['Testing/BattleScenario', 'Runtime/Battle/Testing/BattleScenario'],
    ['Validation/Analysis', 'Runtime/Battle/Diagnostics/Analysis'],
    ['Workspace/ActionEditorImpl', 'Unity/Content/Timeline/Extensions/ActionEditorImpl'],
    ['Workspace/Core', 'Unity/Foundation'],
    ['Workspace/GamplayTag', 'Runtime/Entity/Tags/Ability'],
    ['Workspace/Platform', 'Unity/Foundation/Platform'],
    ['Workspace/PoolExtension', 'Runtime/Pooling/Diagnostics'],
  ].map(([from, to]) => ({ from: editor + '/' + from, to: editor + '/' + to }));
  const folders = directories(editor);
  const files = walk(editor).map(from => {
    const route = routes.find(item => from === item.from + '.meta' || from.startsWith(item.from + '/'));
    let to = route ? route.to + from.slice(route.from.length) : from;
    if (!route && from.endsWith('.meta') && folders.includes(from.slice(0, -5)))
      to = evidence + '/Archive/' + from;
    if (!route && to === from && !from.startsWith(editor + '/GameBattle.Editor.asmdef'))
      throw new Error('Unassigned editor content: ' + from);
    return { from, to, sha256: hash(from) };
  });
  if (new Set(files.map(item => item.to)).size !== files.length) throw new Error('Duplicate destination.');
  for (const route of routes)
    if (fs.existsSync(full(route.to))) throw new Error('Existing destination: ' + route.to);
  const boundaries = ['Runtime', 'Unity', 'Example'].flatMap(layer => walk(base + '/' + layer))
    .map(from => ({ from, sha256: hash(from) }));
  const plan = { date: '2026-10-26', state: 'planned', routes, folders, files, boundaries, rewritten: [] };
  write(planPath, JSON.stringify(plan, null, 2) + '\n');
  console.log({ routes: routes.length, files: files.length, sources: files.filter(f => f.from.endsWith('.cs')).length });
} else if (action === 'apply') {
  const plan = json(planPath);
  if (plan.state !== 'planned') throw new Error('Requires a planned baseline.');
  for (const item of [...plan.files, ...plan.boundaries])
    if (!fs.existsSync(full(item.from)) || hash(item.from) !== item.sha256) throw new Error('Baseline changed: ' + item.from);
  for (const route of [...plan.routes].sort((a, b) => a.to.split('/').length - b.to.split('/').length)) {
    move(route.from, route.to);
    if (fs.existsSync(full(route.from + '.meta'))) move(route.from + '.meta', route.to + '.meta');
  }
  for (const item of plan.files.filter(f => f.to.startsWith(evidence + '/Archive/')))
    move(item.from, item.to);
  for (const dir of [...plan.folders].sort((a, b) => b.length - a.length))
    if (fs.existsSync(full(dir)) && fs.readdirSync(full(dir)).length === 0) fs.rmdirSync(checked(dir));
  for (const dir of directories(editor)) metadata(dir);
  plan.state = 'moved';
  write(planPath, JSON.stringify(plan, null, 2) + '\n');
  const replace = replacer(plan);
  for (const file of walk('.').map(file => file.replace(/^\.\//, ''))) {
    if (file.startsWith('Docs/') || file.startsWith('tools/migration/') ||
      !/\.(cs|csproj|props|targets|asset|prefab|unity|json|uxml|uss|xml|ps1|py|cjs|js|sh|bat)$/.test(file)) continue;
    const text = read(file);
    if (text.includes('\0') || replace(text) === text) continue;
    let next = text;
    if (/\.(csproj|props|targets|xml|uxml)$/.test(file)) {
      const document = new DOMParser().parseFromString(text, 'application/xml');
      const visit = node => {
        for (const attribute of Array.from(node.attributes || [])) attribute.value = replace(attribute.value);
        if (node.nodeType === 3 || node.nodeType === 4) node.data = replace(node.data);
        for (const child of Array.from(node.childNodes || [])) visit(child);
      };
      visit(document);
      next = new XMLSerializer().serializeToString(document);
    } else if (/\.(asset|prefab|unity|json)$/.test(file)) {
      const documents = YAML.parseAllDocuments(text, { prettyErrors: false, logLevel: 'silent' });
      if (documents.some(doc => doc.errors.length)) throw new Error('Unparseable path-bearing asset: ' + file);
      const edits = [];
      const visit = node => {
        if (YAML.isScalar(node) && typeof node.value === 'string') {
          const value = replace(node.value);
          if (value !== node.value) {
            const output = node.type === 'QUOTE_DOUBLE' ? JSON.stringify(value) :
              node.type === 'QUOTE_SINGLE' ? "'" + value.replaceAll("'", "''") + "'" : value;
            edits.push({ start: node.range[0], end: node.range[1], output });
          }
        } else if (YAML.isMap(node)) for (const pair of node.items) visit(pair.value);
        else if (YAML.isSeq(node)) for (const child of node.items) visit(child);
      };
      for (const doc of documents) visit(doc.contents);
      for (const edit of edits.sort((a, b) => b.start - a.start))
        next = next.slice(0, edit.start) + edit.output + next.slice(edit.end);
    } else next = replace(text);
    if (next !== text) {
      write(evidence + '/Backups/' + file, text);
      plan.rewritten.push({ file, before: hash(file) });
      write(file, next);
    }
  }
  plan.state = 'referenced';
  write(planPath, JSON.stringify(plan, null, 2) + '\n');
  console.log({ state: plan.state, referencesUpdated: plan.rewritten.length });
} else if (action === 'verify') {
  const plan = json(planPath), problems = [];
  const correctionFile = 'Docs/GameBattle/EditorDiagnostics/changes.json';
  const correction = fs.existsSync(full(correctionFile)) ? json(correctionFile) : null;
  const extractionFile = 'Docs/GameBattle/ExampleExtraction/changes.json';
  const extraction = fs.existsSync(full(extractionFile)) ? json(extractionFile) : null;
  const buffFile = 'Docs/GameBattle/BuffExtraction/changes.json';
  const buffExtraction = fs.existsSync(full(buffFile)) ? json(buffFile) : null;
  const corrected = file => {
    const result = correction?.state !== 'planned' ?
      correction?.files.find(item => item.from === file)?.to || file : file;
    return extraction?.state !== 'planned' ?
      extraction?.files.find(item => item.from === result)?.to || result : result;
  };
  const allowed = new Set(plan.rewritten.map(item => item.file));
  for (const item of plan.files) {
    if (!fs.existsSync(full(item.to))) problems.push('Missing original file: ' + item.to);
    else if (!allowed.has(item.to) && hash(item.to) !== item.sha256) problems.push('Unexpected file edit: ' + item.to);
  }
  for (const item of plan.boundaries) {
    const file = corrected(item.from);
    const extractionHash = buffExtraction?.rewritten.find(r => r.file === file)?.afterSha256 ||
      extraction?.rewritten.find(r => r.file === file)?.afterSha256;
    if (!fs.existsSync(full(file)) || !allowed.has(file) && hash(file) !== (extractionHash || item.sha256))
      problems.push('Unexpected Runtime/Unity/Example edit: ' + file);
  }
  const current = walk(editor);
  const definitions = current.filter(file => file.endsWith('.asmdef'));
  if (definitions.length !== 1 || definitions[0] !== editor + '/GameBattle.Editor.asmdef')
    problems.push('Editor assembly boundary changed.');
  if (current.some(file => file.endsWith('.asmref'))) problems.push('Unexpected editor asmref.');
  const top = fs.readdirSync(full(editor), { withFileTypes: true }).filter(item => item.isDirectory()).map(item => item.name).sort();
  if (top.join(',') !== 'Runtime,Unity') problems.push('Unexpected Editor root directories: ' + top);
  const guids = new Set();
  for (const file of current.filter(file => file.endsWith('.meta'))) {
    const guid = read(file).match(/^guid:\s*(\S+)/m)?.[1];
    if (guid && guids.has(guid)) problems.push('Duplicate Editor GUID: ' + file);
    if (guid) guids.add(guid);
    if (!fs.existsSync(full(file.slice(0, -5)))) problems.push('Orphan metadata: ' + file);
  }
  for (const layer of ['Runtime', 'Unity']) {
    const modules = fs.readdirSync(full(editor + '/' + layer), { withFileTypes: true })
      .filter(item => item.isDirectory()).map(item => item.name);
    for (const module of modules)
      if (!fs.existsSync(full(base + '/' + layer + '/' + module))) problems.push('Unmirrored module: ' + layer + '/' + module);
  }
  const replace = replacer(plan);
  for (const file of walk('.').map(file => file.replace(/^\.\//, ''))) {
    if (file.startsWith('Docs/') || file.startsWith('tools/migration/') ||
      !/\.(cs|csproj|props|targets|asset|prefab|unity|json|uxml|uss|xml|ps1|py|cjs|js|sh|bat)$/.test(file)) continue;
    const text = read(file);
    if (!text.includes('\0') && replace(text) !== text) problems.push('Old editor path reference: ' + file);
  }
  const report = { date: plan.date, filesPreserved: plan.files.length,
    sources: plan.files.filter(item => item.from.endsWith('.cs')).length,
    roots: top, assembly: 'GameBattle.Editor', pathEdits: plan.rewritten.map(item => item.file), problems };
  write(evidence + '/verification.json', JSON.stringify(report, null, 2) + '\n');
  if (!problems.length) {
    plan.state = 'verified';
    write(planPath, JSON.stringify(plan, null, 2) + '\n');
  }
  console.log(JSON.stringify(report, null, 2));
  if (problems.length) process.exitCode = 1;
} else if (action === 'build') {
  const names = json('Docs/GameBattle/AssemblyConsolidation/builds.json').map(item => item.name);
  const results = names.map(name => {
    const log = evidence + '/BuildLogs/' + name + '.log';
    fs.mkdirSync(path.dirname(full(log)), { recursive: true });
    const result = cp.spawnSync('dotnet', ['build', 'Unity/' + name + '.csproj', '--no-restore',
      '-verbosity:quiet', '-consoleloggerparameters:ErrorsOnly', '-fileLogger', '-fileLoggerParameters:LogFile=' + log],
    { cwd: root, encoding: 'utf8', maxBuffer: 8 * 1024 * 1024 });
    console.log(name + ': ' + (result.status === 0 ? 'PASS' : 'FAIL'));
    if (result.status !== 0) console.log(result.stdout + result.stderr);
    return { name, status: result.status, log, output: result.stdout + result.stderr };
  });
  write(evidence + '/builds.json', JSON.stringify(results, null, 2) + '\n');
  if (results.some(item => item.status !== 0)) process.exitCode = 1;
} else throw new Error('Unknown action: ' + action);
