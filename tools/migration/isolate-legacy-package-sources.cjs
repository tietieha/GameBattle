const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const report = path.join(root, 'Docs/GameBattle/Migration');
const plan = JSON.parse(fs.readFileSync(path.join(report, 'plan.json'), 'utf8'));
const pairs = [
  ['Unity/Assets/Scripts/GameBattle/Examples/Modules/triggering', 'Unity/Assets/Scripts/GameBattle/Examples/Optional~/Legacy/triggering'],
  ['Unity/Assets/Scripts/GameBattle/Examples/Modules/pipeline', 'Unity/Assets/Scripts/GameBattle/Examples/Optional~/Legacy/pipeline'],
];
const moves = [];
for (const [old, next] of pairs) {
  const source = path.resolve(root, old), target = path.resolve(root, next);
  const allowed = path.resolve(root, 'Unity/Assets/Scripts/GameBattle');
  if (!source.startsWith(allowed + path.sep) || !target.startsWith(allowed + path.sep)) throw new Error('Invalid move boundary.');
  if (!fs.existsSync(source)) continue;
  if (fs.existsSync(target)) throw new Error('Target exists.');
  fs.mkdirSync(path.dirname(target), { recursive: true });
  fs.renameSync(source, target);
  if (fs.existsSync(source + '.meta')) fs.renameSync(source + '.meta', target + '.meta');
  for (const file of plan.files) if (file.to === old + '.meta' || file.to.startsWith(old + '/')) file.to = next + file.to.slice(old.length);
  for (const file of plan.sourceOwnership) if (file.to.startsWith(old + '/')) file.to = next + file.to.slice(old.length);
  moves.push({ from: old, to: next });
}
const oldTests = 'Unity/Assets/Scripts/GameBattle/Tests/Unity/triggering';
const newTests = oldTests + '/Legacy~';
for (const file of plan.files.filter(f =>
  f.to.startsWith(oldTests + '/') && !f.to.slice(oldTests.length + 1).includes('/') &&
  /^Trigger(Config|Instance|Registry)Tests\.cs(\.meta)?$/.test(path.basename(f.to)))) {
  const target = newTests + '/' + path.basename(file.to);
  fs.mkdirSync(path.dirname(path.join(root, target)), { recursive: true });
  fs.renameSync(path.join(root, file.to), path.join(root, target));
  for (const source of plan.sourceOwnership) if (source.to === file.to) source.to = target;
  moves.push({ from: file.to, to: target });
  file.to = target;
}
fs.writeFileSync(path.join(report, 'plan.json'), JSON.stringify(plan, null, 2));
function projectFiles(dir) {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap(e =>
    e.isDirectory() ? ['.git', 'Library', 'Temp', 'obj', 'bin', 'node_modules'].includes(e.name)
      ? [] : projectFiles(path.join(dir, e.name)) : e.name.endsWith('.csproj') ? [path.join(dir, e.name)] : []);
}
for (const file of projectFiles(root)) {
  const text = fs.readFileSync(file, 'utf8');
  let output = text;
  for (const [from, to] of pairs) {
    for (const separator of ['/', '\\'])
      output = output.replaceAll(from.replace(/^Unity\//, '').replaceAll('/', separator),
        to.replace(/^Unity\//, '').replaceAll('/', separator));
  }
  if (output !== text) fs.writeFileSync(file, output);
}
fs.writeFileSync(path.join(report, 'legacy-source-isolation.json'), JSON.stringify({
  reason: 'Package loose legacy tests/samples were not in the previous active Unity assemblies; Assets would compile them into Assembly-CSharp.',
  moves,
}, null, 2));
console.log(JSON.stringify({ moved: moves.length }, null, 2));
