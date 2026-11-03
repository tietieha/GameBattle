const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { DOMParser, XMLSerializer } = require('@xmldom/xmldom');
const root = path.resolve(__dirname, '../..');
const base = 'Unity/Assets/Scripts/GameBattle/Example/Moba';
const evidence = 'Docs/GameBattle/EditorDiagnostics';
const from = base + '/Editor/CodeGen/AbilityKit.Demo.Moba.CodeGen.dll';
const to = base + '/Runtime/CodeGen/AbilityKit.Demo.Moba.CodeGen.dll';
const full = file => path.resolve(root, file);
const hash = file => crypto.createHash('sha256').update(fs.readFileSync(full(file))).digest('hex');
const read = file => fs.readFileSync(full(file), 'utf8');
const write = (file, text) => {
  fs.mkdirSync(path.dirname(full(file)), { recursive: true });
  fs.writeFileSync(full(file), text);
};
if (fs.existsSync(full(evidence + '/changes.json'))) throw new Error('Codegen correction journal already exists.');
if (!read(from + '.meta').includes('RoslynAnalyzer')) throw new Error('Expected compiler-only analyzer.');
const journal = { date: '2026-10-26', state: 'planned', routes: [{ from, to }],
  files: [from, from + '.meta'].map(file => ({ from: file, to: file.replace(from, to), sha256: hash(file) })), rewritten: [] };
write(evidence + '/changes.json', JSON.stringify(journal, null, 2) + '\n');
const destination = full(base + '/Runtime/CodeGen');
if (!destination.startsWith(full(base + '/Runtime') + path.sep)) throw new Error('Invalid destination boundary.');
fs.mkdirSync(destination, { recursive: true });
for (const file of journal.files) {
  if (fs.existsSync(full(file.to))) throw new Error('Existing destination: ' + file.to);
  fs.renameSync(full(file.from), full(file.to));
  if (hash(file.to) !== file.sha256) throw new Error('Moved file hash changed: ' + file.to);
}
if (!fs.existsSync(destination + '.meta'))
  write(base + '/Runtime/CodeGen.meta', 'fileFormatVersion: 2\nguid: ' + crypto.randomBytes(16).toString('hex') +
    '\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n');
const variants = [
  [from, to], [from.slice(6), to.slice(6)],
  [from.replaceAll('/', '\\'), to.replaceAll('/', '\\')],
  [from.slice(6).replaceAll('/', '\\'), to.slice(6).replaceAll('/', '\\')],
];
const replace = text => variants.reduce((current, [old, next]) => current.replaceAll(old, next), text);
const ignored = new Set(['.git', 'Library', 'Temp', 'Logs', 'Docs', 'obj', 'bin', 'node_modules']);
function walk(dir) {
  return fs.readdirSync(full(dir), { withFileTypes: true }).flatMap(item => item.isDirectory() ?
    ignored.has(item.name) ? [] : walk(dir + '/' + item.name) : item.name.endsWith('.csproj') ? [dir + '/' + item.name] : []);
}
for (const file of walk('.')) {
  const text = read(file);
  if (replace(text) === text) continue;
  const document = new DOMParser().parseFromString(text, 'application/xml');
  const visit = node => {
    for (const attribute of Array.from(node.attributes || [])) attribute.value = replace(attribute.value);
    if (node.nodeType === 3) node.data = replace(node.data);
    for (const child of Array.from(node.childNodes || [])) visit(child);
  };
  visit(document);
  write(evidence + '/Backups/' + file.replace(/^\.\//, ''), text);
  write(file, new XMLSerializer().serializeToString(document));
  journal.rewritten.push(file.replace(/^\.\//, ''));
}
journal.state = 'applied';
write(evidence + '/changes.json', JSON.stringify(journal, null, 2) + '\n');
console.log(JSON.stringify(journal, null, 2));
