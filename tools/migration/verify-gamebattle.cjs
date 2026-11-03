const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const cp = require('node:child_process');
const root = path.resolve(__dirname, '../..');
const reportRoot = path.join(root, 'Docs/GameBattle/Migration');
const plan = JSON.parse(fs.readFileSync(path.join(reportRoot, 'plan.json'), 'utf8'));
const result = JSON.parse(fs.readFileSync(path.join(reportRoot, 'result.json'), 'utf8'));
const read = p => fs.readFileSync(p, 'utf8').replace(/^\uFEFF/, '');
const hash = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const slash = p => p.replaceAll('\\', '/');
const full = p => path.join(root, p);
const all = dir => fs.readdirSync(dir, { withFileTypes: true }).flatMap(e =>
  e.isDirectory() ? ['obj', 'bin', '.git', 'node_modules'].includes(e.name) ? [] : all(path.join(dir, e.name)) : [path.join(dir, e.name)]);
const missing = plan.files.filter(f => !fs.existsSync(full(f.to))).map(f => f.to);
const changed = plan.files.filter(f => fs.existsSync(full(f.to)) && hash(full(f.to)) !== f.sha256);
const changedMetadata = changed.filter(f => f.from.endsWith('.meta'));
const changedAssemblies = plan.assemblyIdentities.filter(a => {
  const data = JSON.parse(read(full(a.to)));
  const meta = full(a.to) + '.meta';
  const guid = fs.existsSync(meta) ? read(meta).match(/^guid:\s*(\S+)/m)?.[1] : null;
  return data.name !== a.name || guid !== a.guid;
});
const gamebattle = full(plan.root);
const boundaries = all(gamebattle).filter(f => /\.(asmdef|asmref)$/.test(f) && !slash(f).split('/').some(x => x.endsWith('~'))).map(file => {
  const data = JSON.parse(read(file));
  const identity = plan.assemblyIdentities.find(a => a.guid && data.reference === 'GUID:' + a.guid || a.name === data.reference);
  return { dir: path.dirname(file), name: data.name || identity?.name, file };
}).sort((a, b) => b.dir.length - a.dir.length);
const wrongOwners = plan.sourceOwnership.filter(s => {
  const file = full(s.to);
  if (slash(file).split('/').some(x => x.endsWith('~'))) return false;
  const owner = boundaries.find(b => file.startsWith(b.dir + path.sep))?.name || null;
  return owner !== s.assembly;
});
const projectChecks = [];
for (const name of ['AbilityKit.Core', 'AbilityKit.Ability', 'AbilityKit.Demo.Moba.Core', 'AbilityKit.Demo.Moba.Presentation.Core', 'AbilityKit.Demo.Shooter.Core']) {
  const file = path.join(root, 'src', name, name + '.csproj');
  if (!fs.existsSync(file)) continue;
  try {
    const output = cp.execFileSync('dotnet', ['msbuild', file, '-getItem:Compile', '-verbosity:quiet'], {
      cwd: root, maxBuffer: 30 * 1024 * 1024, encoding: 'utf8',
    });
    const items = JSON.parse(output).Items.Compile;
    projectChecks.push({ name, count: items.length, missing: items.filter(i => !fs.existsSync(i.FullPath)).map(i => i.FullPath) });
  } catch (e) { projectChecks.push({ name, error: e.message.slice(0, 2000) }); }
}
const verification = {
  files: plan.files.length, missing, changedFiles: changed.length,
  changedMetadata: changedMetadata.map(f => f.to), changedAssemblies,
  scripts: plan.sourceOwnership.length, unchangedScripts: plan.sourceOwnership.length - result.scriptsWithPathChanges.length,
  scriptsWithPathChanges: result.scriptsWithPathChanges.length, wrongOwners,
  projectChecks,
};
fs.writeFileSync(path.join(reportRoot, 'verification.json'), JSON.stringify(verification, null, 2));
console.log(JSON.stringify(verification, null, 2));
if (missing.length || changedMetadata.length || changedAssemblies.length || wrongOwners.length ||
    projectChecks.some(p => p.error || p.missing?.length)) process.exitCode = 1;
