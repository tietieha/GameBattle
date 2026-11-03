const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const root = path.resolve(__dirname, '../..');
const assets = path.join(root, 'Unity/Assets');
const target = path.join(assets, 'Scripts/GameBattle');
function ensure(dir) {
  if (dir === assets) return;
  ensure(path.dirname(dir));
  if (dir.split(path.sep).some(p => p.endsWith('~'))) return;
  if (!fs.existsSync(dir + '.meta'))
    fs.writeFileSync(dir + '.meta', `fileFormatVersion: 2\nguid: ${crypto.randomBytes(16).toString('hex')}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
}
function visit(dir) {
  if (dir.split(path.sep).some(p => p.endsWith('~'))) return;
  ensure(dir);
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const file = path.join(dir, e.name);
    if (e.isDirectory()) visit(file);
    else if (file.endsWith('.asmref') && fs.existsSync(file + '.meta')) {
      const text = fs.readFileSync(file + '.meta', 'utf8');
      fs.writeFileSync(file + '.meta', text.replace('DefaultImporter:', 'AssemblyDefinitionReferenceImporter:'));
    }
  }
}
visit(target);
const readme = path.join(target, 'README.md');
if (!fs.existsSync(readme + '.meta')) fs.writeFileSync(readme + '.meta',
  `fileFormatVersion: 2\nguid: ${crypto.randomBytes(16).toString('hex')}\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`);
