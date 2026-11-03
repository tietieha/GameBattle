const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const root = path.resolve(__dirname, '../..');
const evidence = path.join(root, 'Docs/GameBattle/Migration');
const plan = JSON.parse(fs.readFileSync(path.join(evidence, 'plan.json'), 'utf8'));
const prefix = 'Unity/Assets/Scripts/GameBattle/';
const pairs = [
  ['Gameplay/Moba/Runtime/Application/Systems/Bootstrap', 'Battle/Core/Moba/Systems/Bootstrap'],
  ['Gameplay/Moba/Runtime/Application/Systems/Eventing', 'Battle/Core/Moba/Systems/Eventing'],
  ['Gameplay/Moba/Runtime/Application/Systems/Collision', 'Battle/Spatial/Moba/Systems/Collision'],
  ['Gameplay/Moba/Runtime/Application/Systems/Effects', 'Battle/Effect/Moba/Systems/Effects'],
].map(([a, b]) => [prefix + a, prefix + b]);
const owner = plan.assemblyIdentities.find(a => a.name === 'AbilityKit.Demo.Moba.Runtime');
const created = [];
for (const [old, target] of pairs) {
  const source = path.resolve(root, old), destination = path.resolve(root, target);
  const boundary = path.resolve(root, prefix);
  if (!source.startsWith(boundary + path.sep) || !destination.startsWith(boundary + path.sep))
    throw new Error('Move outside GameBattle.');
  if (!fs.existsSync(source)) continue;
  if (fs.existsSync(destination)) throw new Error('Existing target: ' + destination);
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.renameSync(source, destination);
  if (fs.existsSync(source + '.meta')) fs.renameSync(source + '.meta', destination + '.meta');
  const reference = path.join(destination, 'AbilityKit.Demo.Moba.Runtime.asmref');
  fs.writeFileSync(reference, JSON.stringify({ reference: 'GUID:' + owner.guid }, null, 2) + '\n');
  fs.writeFileSync(reference + '.meta', `fileFormatVersion: 2\nguid: ${crypto.randomBytes(16).toString('hex')}\nAssemblyDefinitionReferenceImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
  created.push(reference);
  for (const file of plan.files)
    if (file.to === old + '.meta' || file.to.startsWith(old + '/')) file.to = target + file.to.slice(old.length);
  for (const file of plan.sourceOwnership)
    if (file.to.startsWith(old + '/')) file.to = target + file.to.slice(old.length);
}
function walk(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap(e =>
    e.isDirectory() ? ['.git', 'Library', 'Temp', 'obj', 'bin', 'node_modules'].includes(e.name)
      ? [] : walk(path.join(dir, e.name)) : [path.join(dir, e.name)]);
}
for (const file of walk(root).filter(f => /\.(csproj|cs|json|props|targets|ps1)$/.test(f) &&
  !f.startsWith(evidence + path.sep) && !f.startsWith(__dirname + path.sep))) {
  const bytes = fs.readFileSync(file);
  if (bytes.includes(0)) continue;
  const text = bytes.toString('utf8');
  let output = text;
  for (const [a, b] of pairs) {
    for (const separator of ['/', '\\']) {
      const old = a.replace(/^Unity\//, '').replaceAll('/', separator);
      const target = b.replace(/^Unity\//, '').replaceAll('/', separator);
      output = output.replaceAll(old, target);
    }
  }
  if (output !== text) fs.writeFileSync(file, output);
}
fs.writeFileSync(path.join(evidence, 'plan.json'), JSON.stringify(plan, null, 2));
const resultPath = path.join(evidence, 'result.json');
const result = JSON.parse(fs.readFileSync(resultPath, 'utf8'));
result.assemblyReferencesAdded += created.length;
for (const [a, b] of pairs) result.scriptsWithPathChanges = result.scriptsWithPathChanges.map(p => p.startsWith(a + '/') ? b + p.slice(a.length) : p);
fs.writeFileSync(resultPath, JSON.stringify(result, null, 2));
console.log({ systemsMoved: created.length });
