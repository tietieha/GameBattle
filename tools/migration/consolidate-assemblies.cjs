const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const cp = require('node:child_process');
const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
const YAML = require('yaml');
const root = path.resolve(__dirname, '../..');
const base = 'Unity/Assets/Scripts/GameBattle';
const evidence = path.join(root, 'Docs/GameBattle/AssemblyConsolidation');
const planFile = path.join(evidence, 'plan.json');
const slash = s => s.replaceAll('\\', '/');
const full = s => path.resolve(root, s);
const relative = s => slash(path.relative(root, s));
const projectGuid = guid => '{' + guid.replace(/^(.{8})(.{4})(.{4})(.{4})(.{12})$/, '$1-$2-$3-$4-$5') + '}';
const read = s => fs.readFileSync(s, 'utf8');
const json = s => JSON.parse(read(s).replace(/^\uFEFF/, ''));
const mirrorFile = path.join(root, 'Docs/GameBattle/EditorMirroring/plan.json');
const mirror = fs.existsSync(mirrorFile) ? json(mirrorFile) : null;
const correctionFile = path.join(root, 'Docs/GameBattle/EditorDiagnostics/changes.json');
const correction = fs.existsSync(correctionFile) ? json(correctionFile) : null;
const extractionFile = path.join(root, 'Docs/GameBattle/ExampleExtraction/changes.json');
const extraction = fs.existsSync(extractionFile) ? json(extractionFile) : null;
const buffFile = path.join(root, 'Docs/GameBattle/BuffExtraction/changes.json');
const buffExtraction = fs.existsSync(buffFile) ? json(buffFile) : null;
function currentPath(file) {
  const mirrored = !mirror || mirror.state === 'planned' ? file :
    mirror.files.find(item => item.from === file)?.to || file;
  const corrected = correction?.state !== 'planned' ?
    correction?.files.find(item => item.from === mirrored)?.to || mirrored : mirrored;
  return extraction?.state !== 'planned' ?
    extraction?.files.find(item => item.from === corrected)?.to || corrected : corrected;
}
function currentText(text) {
  const routes = [
    ...(mirror && mirror.state !== 'planned' ? mirror.routes : []),
    ...(correction && correction.state !== 'planned' ? correction.routes : []),
    ...(extraction && extraction.state !== 'planned' ? extraction.routes : []),
  ];
  if (!routes.length) return text;
  const variants = new Map(routes.flatMap(route => [
    [route.from, route.to],
    [route.from.slice(6), route.to.slice(6)],
    [route.from.replaceAll('/', '\\'), route.to.replaceAll('/', '\\')],
    [route.from.slice(6).replaceAll('/', '\\'), route.to.slice(6).replaceAll('/', '\\')],
  ]));
  const escape = value => value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  return text.replace(new RegExp([...variants.keys()].sort((a, b) => b.length - a.length).map(escape).join('|'), 'g'),
    value => variants.get(value));
}
const hash = s => crypto.createHash('sha256').update(fs.readFileSync(s)).digest('hex');
const hidden = s => slash(s).split('/').some(part => part.endsWith('~'));
const ignored = new Set(['.git', 'Library', 'Temp', 'Logs', 'obj', 'bin', 'node_modules', '.idea']);
function walk(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap(e => e.isDirectory() ?
    ignored.has(e.name) ? [] : walk(path.join(dir, e.name)) : e.isFile() ? [path.join(dir, e.name)] : []);
}
function write(file, text) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, text);
}
function checked(file) {
  const resolved = path.resolve(file);
  if (!resolved.startsWith(root + path.sep)) throw new Error('Outside project: ' + file);
  return resolved;
}
function backup(file) {
  const target = path.join(evidence, 'Backups', relative(file));
  if (!fs.existsSync(target)) {
    fs.mkdirSync(path.dirname(target), { recursive: true });
    fs.copyFileSync(file, target);
  }
}
function metadata(file, folder = false) {
  if (fs.existsSync(file + '.meta') || hidden(file)) return;
  write(file + '.meta', `fileFormatVersion: 2\nguid: ${crypto.randomBytes(16).toString('hex')}\n${folder ? 'folderAsset: yes\n' : ''}${file.endsWith('.asmdef') ? 'AssemblyDefinitionImporter' : 'DefaultImporter'}:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
}
function folders(dir) {
  if (!dir.startsWith(full(base) + path.sep)) return;
  folders(path.dirname(dir));
  fs.mkdirSync(dir, { recursive: true });
  metadata(dir, true);
}
function current() {
  const files = walk(full(base)).filter(f => !hidden(f));
  const defs = files.filter(f => f.endsWith('.asmdef')).map(file => ({
    file, dir: path.dirname(file), ...json(file), guid: read(file + '.meta').match(/^guid:\s*(\S+)/m)?.[1],
  }));
  const by = new Map(defs.flatMap(a => [[a.name, a], ['GUID:' + a.guid, a]]));
  const refs = files.filter(f => f.endsWith('.asmref')).map(file => ({
    file, dir: path.dirname(file), name: by.get(json(file).reference)?.name,
  }));
  if (refs.some(a => !a.name)) throw new Error('Unresolved asmref.');
  const boundaries = [...defs, ...refs].sort((a, b) => b.dir.length - a.dir.length);
  const owner = file => boundaries.find(a => file.startsWith(a.dir + path.sep))?.name || null;
  return { files, defs, refs, by, owner };
}
const layerOf = file => slash(path.relative(full(base), file)).split('/')[0];
const layers = ['Runtime', 'Unity', 'Editor'];
const isTest = a => /Tests?$/.test(a.name) || a.optionalUnityReferences?.includes('TestAssemblies') ||
  a.defineConstraints?.includes('UNITY_INCLUDE_TESTS');
function mapPath(plan, source) {
  const route = plan.routes.find(r => source === r.from || source === r.from + '.meta' || source.startsWith(r.from + '/'));
  return route ? route.to + source.slice(route.from.length) : source;
}
function mapping(plan) {
  const by = new Map();
  for (const a of plan.merged) {
    const target = plan.targets.find(t => t.layer === a.layer);
    by.set(a.name, target.name);
    by.set('GUID:' + a.guid, 'GUID:' + target.guid);
  }
  return by;
}
function convertReference(plan, value) {
  return mapping(plan).get(value) || value;
}
function compatibilityText(source, text) {
  if (source.layer === 'Runtime' && source.from.endsWith('/IsExternalInitPolyfill.cs') &&
    !source.from.includes('/Hosting/Configuration/')) {
    return '#if !UNITY_5_3_OR_NEWER\n' + text + '\n#endif\n';
  }
  if (source.from.endsWith('/FixedStepTimeDriver.cs') || source.from.endsWith('/WaitSecondsEventNode.cs'))
    return text.replace(/\bTimer\b/g, 'System.Threading.Timer');
  return text;
}
function generateProjects(plan, preview) {
  const replacements = mapping(plan);
  const oldNames = new Set(plan.merged.map(a => a.name));
  const oldFiles = new Set(plan.merged.map(a => a.name + '.csproj'));
  for (const target of plan.targets) {
    const templatePath = full('Unity/' + target.template + '.csproj');
    const archived = path.join(evidence, 'Backups', relative(templatePath));
    const document = new DOMParser().parseFromString(read(fs.existsSync(archived) ? archived : templatePath), 'application/xml');
    const project = document.documentElement;
    const ns = project.namespaceURI;
    const create = name => document.createElementNS(ns, name);
    for (const name of ['Compile', 'ProjectReference', 'Reference', 'Analyzer', 'None']) {
      for (const node of Array.from(document.getElementsByTagName(name))) node.parentNode.removeChild(node);
    }
    for (const node of Array.from(document.getElementsByTagName('AssemblyName'))) node.textContent = target.name;
    for (const node of Array.from(document.getElementsByTagName('ProjectGuid'))) node.textContent = projectGuid(target.guid);
    for (const node of Array.from(document.getElementsByTagName('OutputPath')))
      node.textContent = 'Temp\\Bin\\Debug\\' + target.name + '\\';
    for (const node of Array.from(document.getElementsByTagName('AllowUnsafeBlocks'))) node.textContent = target.data.allowUnsafeCode ? 'True' : 'False';
    const compileGroup = create('ItemGroup');
    const extraFiles = !preview
      ? [extraction, buffExtraction].filter(stage => stage && stage.state !== 'planned')
        .flatMap(stage => [...stage.routes.map(r => r.to), ...stage.added])
        .filter(file => file.startsWith(base + '/' + target.layer + '/') && file.endsWith('.cs'))
        .map(file => ({ from: file, to: file, layer: target.layer })) : [];
    const sourceItems = [...plan.sources.filter(s => s.layer === target.layer), ...extraFiles];
    for (const source of sourceItems) {
      let file = full(preview ? source.from : currentPath(source.to));
      const originalText = read(file);
      const compatibleText = preview ? compatibilityText(source, originalText) : originalText;
      if (preview && (source.define || compatibleText !== originalText)) {
        const wrapped = path.join(evidence, 'PreviewSources', source.from);
        write(wrapped, source.define ? '#if ' + source.define + '\n' + compatibleText + '\n#endif\n' : compatibleText);
        file = wrapped;
      }
      const element = create('Compile');
      element.setAttribute('Include', slash(path.relative(full('Unity'), file)).replaceAll('/', '\\'));
      compileGroup.appendChild(element);
    }
    project.appendChild(compileGroup);
    const references = new Map(), analyzers = new Map(), projectRefs = new Set();
    const versionSymbols = new Set((target.data.versionDefines || []).map(v => v.define));
    const enabledVersionSymbols = new Set();
    for (const old of plan.merged.filter(a => a.layer === target.layer)) {
      const source = full('Unity/' + old.name + '.csproj');
      const saved = path.join(evidence, 'Backups', relative(source));
      if (!fs.existsSync(saved) && !fs.existsSync(source)) continue;
      const doc = new DOMParser().parseFromString(currentText(read(fs.existsSync(saved) ? saved : source)), 'application/xml');
      for (const node of Array.from(doc.getElementsByTagName('DefineConstants')))
        for (const symbol of node.textContent.split(';'))
          if (versionSymbols.has(symbol)) enabledVersionSymbols.add(symbol);
      for (const item of Array.from(doc.getElementsByTagName('Reference'))) {
        const include = item.getAttribute('Include').split(',')[0];
        const hint = Array.from(item.getElementsByTagName('HintPath'))[0]?.textContent || '';
        if (oldNames.has(include) || oldNames.has(path.basename(hint, '.dll'))) continue;
        if (target.layer === 'Runtime' && /^(UnityEngine|UnityEditor|Unity\.)(\.|$)?/.test(include)) continue;
        if (/TestRunner|nunit\.framework/.test(include)) continue;
        references.set(include, item);
      }
      for (const item of Array.from(doc.getElementsByTagName('Analyzer'))) analyzers.set(item.getAttribute('Include'), item);
      for (const item of Array.from(doc.getElementsByTagName('ProjectReference'))) {
        const include = item.getAttribute('Include');
        if (oldFiles.has(path.basename(include))) continue;
        if (/TestRunner/.test(include)) continue;
        projectRefs.add(include);
      }
    }
    const refsGroup = create('ItemGroup');
    for (const item of references.values()) refsGroup.appendChild(document.importNode(item, true));
    for (const item of analyzers.values()) refsGroup.appendChild(document.importNode(item, true));
    for (const dependency of target.data.references) {
      if (dependency.startsWith('GameBattle.')) projectRefs.add(dependency + '.csproj');
    }
    for (const include of projectRefs) {
      const node = create('ProjectReference'); node.setAttribute('Include', include); refsGroup.appendChild(node);
    }
    project.appendChild(refsGroup);
    for (const node of Array.from(document.getElementsByTagName('DefineConstants')))
      node.textContent = [...new Set([...node.textContent.split(';'), ...enabledVersionSymbols])].join(';');
    write(full('Unity/' + target.name + '.csproj'), new XMLSerializer().serializeToString(document));
  }
}
const action = process.argv[2] || 'plan';
if (action === 'plan') {
  if (fs.existsSync(planFile)) throw new Error('Existing consolidation plan; do not overwrite the baseline.');
  const state = current();
  const framework = state.defs.filter(a => layers.includes(layerOf(a.file)));
  const tests = framework.filter(isTest);
  const routes = tests.map(a => ({
    from: relative(a.dir), to: base + '/Example/FrameworkTests/' + slash(path.relative(full(base), a.dir)),
    reason: 'Retain test assembly and platform semantics outside the three framework assemblies.',
  }));
  for (const a of framework.filter(a => !isTest(a) && layerOf(a.file) === 'Unity' && a.includePlatforms?.includes('Editor')))
    routes.push({ from: relative(a.dir), to: base + '/Editor/Dependencies/Svelto/PoolDebugger', reason: 'Editor-only inspector.' });
  routes.sort((a, b) => b.from.length - a.from.length);
  const merged = framework.filter(a => !isTest(a)).map(a => ({
    name: a.name, guid: a.guid, from: relative(a.file),
    layer: a.includePlatforms?.includes('Editor') ? 'Editor' : layerOf(a.file), originalLayer: layerOf(a.file),
    defines: a.defineConstraints || [], data: json(a.file),
  }));
  const plan = { date: '2026-10-26', state: 'planned', routes, merged, tests: tests.map(a => ({ name: a.name, from: relative(a.file) })),
    sources: [], targets: [], archived: [], rewritten: [], files: [] };
  for (const layer of layers) {
    const template = { Runtime: 'AbilityKit.Core', Unity: 'AbilityKit.Core.Unity', Editor: 'AbilityKit.Base.Editor' }[layer];
    const selected = state.defs.find(a => a.name === template);
    const references = new Set(merged.filter(a => a.layer === layer).flatMap(a => a.data.references || []));
    const data = {
      name: 'GameBattle.' + layer, rootNamespace: 'GameBattle',
      references: [], includePlatforms: layer === 'Editor' ? ['Editor'] : [], excludePlatforms: [],
      allowUnsafeCode: merged.some(a => a.layer === layer && a.data.allowUnsafeCode),
      overrideReferences: false, precompiledReferences: [], autoReferenced: true,
      defineConstraints: [], versionDefines: [], noEngineReferences: layer === 'Runtime',
    };
    plan.targets.push({ layer, name: data.name, guid: selected.guid, template, metaFrom: relative(selected.file) + '.meta',
      file: base + '/' + layer + '/GameBattle.' + layer + '.asmdef', data, external: [...references] });
  }
  const map = mapping(plan);
  for (const target of plan.targets) {
    target.data.references = [...new Set(target.external.map(r => {
      if (map.has(r)) {
        const converted = map.get(r);
        return plan.targets.find(t => converted === t.name || converted === 'GUID:' + t.guid)?.name || converted;
      }
      return r;
    }).filter(r => r !== target.name && r !== 'GUID:' + target.guid &&
      !/TestRunner/.test(r) && !(target.layer === 'Unity' && (r === 'GameBattle.Editor' ||
        r === 'GUID:' + plan.targets.find(t => t.layer === 'Editor').guid))))];
    if (target.layer === 'Unity' && !target.data.references.includes('GameBattle.Runtime')) target.data.references.push('GameBattle.Runtime');
    if (target.layer === 'Editor') {
      for (const r of ['GameBattle.Runtime', 'GameBattle.Unity']) if (!target.data.references.includes(r)) target.data.references.push(r);
    }
  }
  for (const file of state.files) {
    const from = relative(file), oldOwner = state.owner(file);
    const old = merged.find(a => a.name === oldOwner);
    const physical = layerOf(file);
    let to = mapPath(plan, from);
    if (file.endsWith('.asmdef') && merged.some(a => a.from === from) ||
      file.endsWith('.asmdef.meta') && merged.some(a => a.from + '.meta' === from) ||
      file.endsWith('.asmref') && layers.includes(physical) && to === from ||
      file.endsWith('.asmref.meta') && layers.includes(physical) && to === from) {
      to = 'Docs/GameBattle/AssemblyConsolidation/Archive/' + from;
      plan.archived.push({ from, to });
    }
    plan.files.push({ from, to, sha256: hash(file), assembly: file.endsWith('.cs') ? oldOwner : undefined });
    if (file.endsWith('.cs') && old) plan.sources.push({ from, to: mapPath(plan, from), layer: old.layer,
      define: old.defines.length ? old.defines.map(d => d.startsWith('!') ? d : d).join(' && ') : null });
    // Previously loose scripts become members of their root framework assembly.
    if (file.endsWith('.cs') && !oldOwner && layers.includes(physical))
      plan.sources.push({ from, to: mapPath(plan, from), layer: physical, define: null });
  }
  for (const file of walk(full('Unity')).filter(f => f.endsWith('.csproj') && path.dirname(f) === full('Unity'))) backup(file);
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  generateProjects(plan, true);
  console.log({ state: plan.state, merged: merged.length, testsRetained: tests.length,
    sources: plan.sources.length, targets: plan.targets.map(t => ({ name: t.name, references: t.data.references })) });
} else if (action === 'preview') {
  generateProjects(json(planFile), true);
} else if (action === 'apply') {
  const plan = json(planFile);
  if (plan.state !== 'planned') throw new Error('Requires an unchanged planned baseline.');
  for (const item of plan.files) if (!fs.existsSync(full(item.from)) || hash(full(item.from)) !== item.sha256)
    throw new Error('Baseline changed: ' + item.from);
  for (const route of plan.routes) {
    const source = full(route.from), destination = full(route.to);
    if (fs.existsSync(destination)) throw new Error('Existing route destination: ' + destination);
    fs.mkdirSync(path.dirname(destination), { recursive: true });
    fs.renameSync(checked(source), checked(destination));
    if (fs.existsSync(source + '.meta')) fs.renameSync(checked(source + '.meta'), checked(destination + '.meta'));
  }
  for (const item of plan.archived) {
    const source = full(mapPath(plan, item.from)), destination = full(item.to);
    if (!fs.existsSync(source)) continue;
    fs.mkdirSync(path.dirname(destination), { recursive: true });
    fs.renameSync(checked(source), checked(destination));
  }
  for (const target of plan.targets) {
    write(full(target.file), JSON.stringify(target.data, null, 2) + '\n');
    const source = full(plan.archived.find(a => a.from === target.metaFrom).to);
    fs.renameSync(checked(source), checked(full(target.file) + '.meta'));
    plan.files.find(a => a.from === target.metaFrom).to = target.file + '.meta';
  }
  for (const source of plan.sources) {
    const file = full(source.to);
    const text = read(file);
    const compatible = compatibilityText(source, text);
    const next = source.define ? '#if ' + source.define + '\n' + compatible + '\n#endif\n' : compatible;
    if (next === text) continue;
    backup(file);
    write(file, next);
    plan.rewritten.push(source.to);
  }
  const test = plan.tests.find(a => a.name === 'AbilityKit.Compute.UnityJobs.Tests');
  if (test) {
    const file = full(mapPath(plan, test.from)), data = json(file);
    if (!data.defineConstraints.includes('ABILITYKIT_COMPUTE_UNITY_JOBS')) data.defineConstraints.push('ABILITYKIT_COMPUTE_UNITY_JOBS');
    write(file, JSON.stringify(data, null, 2) + '\n');
  }
  plan.state = 'moved';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log({ state: plan.state, archived: plan.archived.length });
} else if (action === 'references') {
  const plan = json(planFile), map = mapping(plan);
  if (plan.state !== 'moved') throw new Error('Requires completed assembly moves.');
  const sourcePaths = plan.routes.flatMap(r => [
    [r.from, r.to], [r.from.slice(6), r.to.slice(6)],
    [r.from.replaceAll('/', '\\'), r.to.replaceAll('/', '\\')], [r.from.slice(6).replaceAll('/', '\\'), r.to.slice(6).replaceAll('/', '\\')],
  ]).sort((a, b) => b[0].length - a[0].length);
  const assemblyNames = new Map(plan.merged.map(a => [a.name, plan.targets.find(t => t.layer === a.layer).name]));
  const escape = s => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const assemblySuffix = new RegExp('(,\\s*)(' + [...assemblyNames.keys()].sort((a, b) => b.length - a.length).map(escape).join('|') + ')(?=[,\\s\\\"]|$)', 'g');
  function mappedString(text, key) {
    let next = text.replace(assemblySuffix, (_, prefix, name) => prefix + assemblyNames.get(name));
    if (['asm', 'assemblyName', 'AssemblyName', 'assembly', 'managedReferenceFullTypename'].includes(key)) {
      if (assemblyNames.has(next)) next = assemblyNames.get(next);
      else for (const [old, current] of assemblyNames) if (next.startsWith(old + ' ')) next = current + next.slice(old.length);
    }
    if (key === 'm_EditorClassIdentifier') for (const [old, current] of assemblyNames)
      if (next.startsWith(old + '::')) next = current + next.slice(old.length);
    for (const [old, current] of sourcePaths) next = next.replaceAll(old, current);
    return next;
  }
  for (const file of walk(root)) {
    if (file.startsWith(full('Docs') + path.sep) || file.startsWith(__dirname + path.sep) || hidden(file)) continue;
    if (!/\.(asmdef|asmref|cs|csproj|props|targets|json|asset|prefab|unity|ps1|py|xml|uxml)$/.test(file)) continue;
    const text = read(file);
    if (text.includes('\0')) continue;
    let next = text;
    if (file.endsWith('.asmdef')) {
      const data = json(file);
      const references = [...new Set((data.references || []).map(r => map.get(r) || r))];
      if (JSON.stringify(references) !== JSON.stringify(data.references || [])) {
        data.references = references;
        next = JSON.stringify(data, null, 2) + '\n';
      }
    } else if (file.endsWith('.asmref')) {
      const data = json(file), reference = map.get(data.reference) || data.reference;
      if (reference !== data.reference) {
        data.reference = reference;
        next = JSON.stringify(data, null, 2) + '\n';
      }
    } else if (/\.(asset|prefab|unity|json)$/.test(file)) {
      if (![...assemblyNames.keys(), ...sourcePaths.map(p => p[0])].some(value => text.includes(value))) continue;
      const documents = YAML.parseAllDocuments(text, { prettyErrors: false, logLevel: 'silent' });
      if (documents.some(d => d.errors.length)) throw new Error('Cannot safely parse serialized asset: ' + relative(file));
      const edits = [];
      function visit(node, key = '') {
        if (YAML.isScalar(node) && typeof node.value === 'string') {
          const value = mappedString(node.value, key);
          if (value !== node.value) {
            const output = node.type === 'QUOTE_DOUBLE' ? JSON.stringify(value) : node.type === 'QUOTE_SINGLE' ?
              "'" + value.replaceAll("'", "''") + "'" : value;
            edits.push({ start: node.range[0], end: node.range[1], output });
          }
        } else if (YAML.isMap(node)) for (const pair of node.items) visit(pair.value, String(pair.key?.value || ''));
        else if (YAML.isSeq(node)) for (const child of node.items) visit(child);
      }
      for (const doc of documents) visit(doc.contents);
      for (const edit of edits.sort((a, b) => b.start - a.start)) next = next.slice(0, edit.start) + edit.output + next.slice(edit.end);
    } else {
      for (const [old, current] of sourcePaths) next = next.replaceAll(old, current);
      if (file.endsWith('.cs') && file.startsWith(full(base) + path.sep) &&
        !file.endsWith('WorldServiceContainerFactory.cs'))
        next = next.replace(/"(?:\\.|[^"\\\r\n])*"/g, literal => literal.replace(assemblySuffix, (_, prefix, name) => prefix + assemblyNames.get(name)));
      if (file.endsWith('.csproj') && path.dirname(file) === full('Unity')) {
        const document = new DOMParser().parseFromString(next, 'application/xml');
        const ns = document.documentElement.namespaceURI;
        const seen = new Set();
        for (const item of Array.from(document.getElementsByTagName('ProjectReference'))) {
          const old = path.basename(item.getAttribute('Include'), '.csproj'), name = assemblyNames.get(old) || old;
          const include = name + '.csproj';
          if (name === path.basename(file, '.csproj') || seen.has(include)) { item.parentNode.removeChild(item); continue; }
          seen.add(include); item.setAttribute('Include', include);
          for (const child of Array.from(item.getElementsByTagName('Name'))) child.textContent = name;
          for (const child of Array.from(item.getElementsByTagName('Project'))) {
            const target = plan.targets.find(t => t.name === name);
            if (target) child.textContent = projectGuid(target.guid);
          }
        }
        next = new XMLSerializer().serializeToString(document);
      }
    }
    if (next !== text) {
      backup(file); write(file, next);
      if (!plan.rewritten.includes(relative(file))) plan.rewritten.push(relative(file));
    }
  }
  // Old generated IDE projects are archived; the standalone src projects are not merged.
  for (const old of plan.merged) {
    const source = full('Unity/' + old.name + '.csproj');
    if (!fs.existsSync(source)) continue;
    const destination = path.join(evidence, 'Archive/GeneratedProjects', old.name + '.csproj');
    fs.mkdirSync(path.dirname(destination), { recursive: true });
    fs.renameSync(checked(source), checked(destination));
  }
  for (const route of plan.routes) {
    const parentMeta = full(route.from + '.meta');
    if (fs.existsSync(parentMeta) && !fs.existsSync(full(route.from))) throw new Error('Old route metadata remains.');
  }
  for (const file of walk(full(base))) {
    if (hidden(file)) continue;
    folders(path.dirname(file));
    if (!file.endsWith('.meta')) metadata(file);
  }
  generateProjects(plan, false);
  plan.state = 'referenced';
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  console.log({ state: plan.state, rewritten: plan.rewritten.length });
} else if (action === 'projects') {
  const plan = json(planFile);
  for (const target of plan.targets) {
    target.data.references = target.data.references.filter(r => r !== 'Unity.Entities');
    target.data.versionDefines = [...new Map(plan.merged.filter(a => a.layer === target.layer)
      .flatMap(a => a.data.versionDefines || []).map(v => [JSON.stringify(v), v])).values()];
    write(full(target.file), JSON.stringify(target.data, null, 2) + '\n');
  }
  const computeTest = plan.tests.find(a => a.name === 'AbilityKit.Compute.UnityJobs.Tests');
  if (computeTest) {
    const file = full(mapPath(plan, computeTest.from)), data = json(file);
    data.versionDefines = plan.targets.find(t => t.layer === 'Unity').data.versionDefines
      .filter(v => v.define === 'ABILITYKIT_COMPUTE_UNITY_JOBS');
    write(file, JSON.stringify(data, null, 2) + '\n');
  }
  write(planFile, JSON.stringify(plan, null, 2) + '\n');
  generateProjects(plan, false);
  for (const file of fs.readdirSync(full('Unity')).filter(f => f.endsWith('.csproj'))) {
    const location = full('Unity/' + file), text = read(location);
    const document = new DOMParser().parseFromString(text, 'application/xml');
    let changed = false;
    for (const reference of Array.from(document.getElementsByTagName('ProjectReference'))) {
      const target = plan.targets.find(t => path.basename(reference.getAttribute('Include')) === t.name + '.csproj');
      if (!target) continue;
      for (const child of Array.from(reference.getElementsByTagName('Project'))) {
        if (child.textContent !== projectGuid(target.guid)) {
          child.textContent = projectGuid(target.guid);
          changed = true;
        }
      }
    }
    if (file.startsWith('AbilityKit.Demo.Moba.')) {
      const generator = full(currentPath(base + '/Example/Moba/Editor/CodeGen/AbilityKit.Demo.Moba.CodeGen.dll'));
      if (!Array.from(document.getElementsByTagName('Analyzer')).some(a => a.getAttribute('Include') === generator)) {
        const group = document.createElementNS(document.documentElement.namespaceURI, 'ItemGroup');
        const analyzer = document.createElementNS(document.documentElement.namespaceURI, 'Analyzer');
        analyzer.setAttribute('Include', generator);
        group.appendChild(analyzer);
        document.documentElement.appendChild(group);
        changed = true;
      }
    }
    if (changed) { backup(location); write(location, new XMLSerializer().serializeToString(document)); }
  }
  const solution = full('Unity/Unity.sln');
  if (fs.existsSync(solution)) {
    backup(solution);
    cp.execFileSync('dotnet', ['sln', solution, 'remove', ...plan.merged.map(a => full('Unity/' + a.name + '.csproj'))],
      { cwd: root, encoding: 'utf8', maxBuffer: 4 * 1024 * 1024 });
    cp.execFileSync('dotnet', ['sln', solution, 'add', ...plan.targets.map(a => full('Unity/' + a.name + '.csproj'))],
      { cwd: root, encoding: 'utf8' });
  }
} else if (action === 'verify') {
  const plan = json(planFile), state = current(), problems = [];
  const installedDefinitions = [
    ...walk(full('Unity/Assets')), ...walk(full('Unity/Packages')),
    ...walk(full('Unity/Library/PackageCache')),
  ].filter(f => f.endsWith('.asmdef') && !hidden(f)).map(file => ({
    file, ...json(file),
    guid: fs.existsSync(file + '.meta') ? read(file + '.meta').match(/^guid:\s*(\S+)/m)?.[1] : null,
  }));
  const installedNames = new Set(installedDefinitions.flatMap(a => [a.name, 'GUID:' + a.guid]));
  for (const definition of state.defs)
    for (const reference of definition.references || [])
      if (!installedNames.has(reference)) problems.push('Unresolved assembly reference: ' + definition.name + ' -> ' + reference);
  for (const layer of layers) {
    const defs = state.defs.filter(a => layerOf(a.file) === layer);
    if (defs.length !== 1 || defs[0].name !== 'GameBattle.' + layer) problems.push('Wrong assembly count in ' + layer);
    const foreign = state.files.filter(f => f.endsWith('.cs') && layerOf(f) === layer && state.owner(f) !== 'GameBattle.' + layer);
    if (foreign.length) problems.push('Wrong ownership: ' + foreign);
    if (state.refs.some(a => layerOf(a.file) === layer)) problems.push('Nested framework asmrefs remain: ' + layer);
  }
  for (const item of plan.files) {
    const file = currentPath(item.to);
    if (!fs.existsSync(full(file))) problems.push('Missing original: ' + file);
    else if (item.from.endsWith('.meta') && hash(full(file)) !== item.sha256) problems.push('Original metadata changed: ' + file);
  }
  const byGuid = new Map();
  for (const file of state.files.filter(f => f.endsWith('.meta'))) {
    const guid = read(file).match(/^guid:\s*(\S+)/m)?.[1];
    if (guid && byGuid.has(guid)) problems.push('Duplicate GUID: ' + file);
    if (guid) byGuid.set(guid, file);
    if (!fs.existsSync(file.slice(0, -5))) problems.push('Orphan metadata: ' + file);
  }
  for (const target of plan.targets) {
    const current = state.defs.find(a => a.name === target.name);
    if (!current || current.guid !== target.guid) problems.push('Lost root identity: ' + target.name);
  }
  const originalExamples = new Map(plan.files.filter(f => f.from.endsWith('.asmdef') &&
    f.from.startsWith(base + '/Example/')).map(f => [f.from, f]));
  for (const [file, original] of originalExamples) {
    const currentDefinition = json(full(file));
    const saved = path.join(evidence, 'Backups', file);
    if (fs.existsSync(saved)) {
      if (currentDefinition.name !== json(saved).name) problems.push('Example assembly identity changed: ' + file);
    } else if (hash(full(file)) !== original.sha256 &&
      !extraction?.rewritten.some(item => item.file === file && item.afterSha256 === hash(full(file))))
      problems.push('Untracked Example assembly edit: ' + file);
  }
  const runtime = state.defs.find(a => a.name === 'GameBattle.Runtime');
  if (!runtime?.noEngineReferences || runtime.references.some(r => r.startsWith('GameBattle.Unity') || r.startsWith('GameBattle.Editor')))
    problems.push('Runtime boundary violation.');
  const map = mapping(plan);
  for (const a of state.defs) for (const r of a.references || [])
    if (map.has(r) && !plan.targets.some(t => r === 'GUID:' + t.guid)) problems.push('Old assembly reference: ' + a.name + ' -> ' + r);
  const changedSources = plan.files.filter(f => f.from.endsWith('.cs') && fs.existsSync(full(currentPath(f.to))) &&
    hash(full(currentPath(f.to))) !== f.sha256).map(f => currentPath(f.to));
  for (const source of changedSources)
    if (!plan.rewritten.some(file => currentPath(file) === source) &&
      !mirror?.rewritten.some(item => item.file === source) && !source.endsWith('/WorldServiceContainerFactory.cs') &&
      !extraction?.rewritten.some(item => item.file === source && item.afterSha256 === hash(full(source))) &&
      !buffExtraction?.rewritten.some(item => item.file === source && item.afterSha256 === hash(full(source))) &&
      source !== base + '/Example/FrameworkTests/Unity/Decision/StateMachine/Tests/DiagnosticsTests.cs')
      problems.push('Unexpected source edit: ' + source);
  const projectInputFailures = [], unrelatedMissingInputs = [];
  let projects = 0, checkedInputs = 0;
  for (const file of walk(root).filter(f => f.endsWith('.csproj') &&
    !f.startsWith(full('Docs') + path.sep))) {
    projects++;
    const document = new DOMParser().parseFromString(read(file), 'application/xml');
    for (const kind of ['Compile', 'ProjectReference']) {
      for (const node of Array.from(document.getElementsByTagName(kind))) {
        for (const include of (node.getAttribute('Include') || '').split(';')) {
          if (!include || /[$*?]/.test(include)) continue;
          checkedInputs++;
          const input = path.resolve(path.dirname(file), include);
          if (fs.existsSync(input)) continue;
          const failure = { project: relative(file), kind, input: relative(input) };
          if (file.startsWith(full('Unity') + path.sep) ||
            input.startsWith(full(base) + path.sep)) projectInputFailures.push(failure);
          else unrelatedMissingInputs.push(failure);
        }
      }
    }
  }
  for (const failure of projectInputFailures) problems.push('Missing project input: ' + JSON.stringify(failure));
  const report = { date: plan.date, originalFrameworkAssemblies: plan.merged.length + plan.tests.length,
    mergedAssemblies: plan.merged.length, retainedTestAssemblies: plan.tests.length,
    framework: plan.targets.map(t => t.name), totalAssemblies: state.defs.length,
    filesPreserved: plan.files.length, changedSources,
    projectInputs: { projects, checkedInputs, projectInputFailures, unrelatedMissingInputs }, problems };
  write(path.join(evidence, 'verification.json'), JSON.stringify(report, null, 2) + '\n');
  if (!problems.length && fs.existsSync(path.join(evidence, 'builds.json')) &&
    json(path.join(evidence, 'builds.json')).every(result => result.status === 0)) {
    plan.state = 'verified';
    write(planFile, JSON.stringify(plan, null, 2) + '\n');
  }
  console.log(JSON.stringify(report, null, 2));
  if (problems.length) process.exitCode = 1;
} else if (action === 'build') {
  const plan = json(planFile);
  const names = [...plan.targets.map(t => t.name), ...plan.tests.map(t => t.name),
    'AbilityKit.Demo.Moba.View.Runtime', 'AbilityKit.Demo.Moba.Editor',
    'AbilityKit.Demo.Shooter.View.Runtime', 'AbilityKit.Demo.Shooter.Jobs.Tests',
    'AbilityKit.Demo.Tiny.View', 'AbilityKit.Demo.Tiny.Turn.View',
    'AbilityKit.Game.UnitTests', 'Assembly-CSharp-Editor'];
  const results = [];
  for (const name of names) {
    const log = 'Docs/GameBattle/AssemblyConsolidation/BuildLogs/' + name + '.log';
    fs.mkdirSync(path.dirname(full(log)), { recursive: true });
    const result = cp.spawnSync('dotnet', ['build', 'Unity/' + name + '.csproj', '--no-restore',
      '-verbosity:quiet', '-consoleloggerparameters:ErrorsOnly', '-fileLogger', '-fileLoggerParameters:LogFile=' + log],
    { cwd: root, encoding: 'utf8', maxBuffer: 8 * 1024 * 1024 });
    results.push({ name, status: result.status, log, output: result.stdout + result.stderr });
    console.log(name + ': ' + (result.status === 0 ? 'PASS' : 'FAIL'));
    if (result.status !== 0) console.log(result.stdout + result.stderr);
  }
  write(path.join(evidence, 'builds.json'), JSON.stringify(results, null, 2) + '\n');
  if (results.some(r => r.status !== 0)) process.exitCode = 1;
} else throw new Error('Unknown action: ' + action);
