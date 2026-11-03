const fs = require('node:fs');
const path = require('node:path');
const endpoint = 'http://127.0.0.1:8080/mcp';
let session;
let nextId = 1;
async function request(method, params, notification = false) {
  const response = await fetch(endpoint, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json, text/event-stream',
      ...(session ? { 'Mcp-Session-Id': session } : {}) },
    body: JSON.stringify({ jsonrpc: '2.0', ...(notification ? {} : { id: nextId++ }), method, params }),
    signal: AbortSignal.timeout(120000),
  });
  session = response.headers.get('mcp-session-id') || session;
  const text = await response.text();
  if (!text) return null;
  if (text.startsWith('{')) return JSON.parse(text);
  return text.split(/\r?\n/).filter(l => l.startsWith('data: ')).map(l => JSON.parse(l.slice(6))).at(-1);
}
async function main() {
  await request('initialize', { protocolVersion: '2024-11-05', capabilities: {}, clientInfo: { name: 'gamebattle-diagnostics', version: '1.0' } });
  await request('notifications/initialized', {}, true);
  const method = process.argv[2] || 'tools/list';
  const params = process.argv[3] ? JSON.parse(process.argv[3]) : {};
  if (method === 'tools/call' && params.name !== 'set_active_instance') {
    const instances = await request('resources/read', { uri: 'mcpforunity://instances' });
    const payload = JSON.parse(instances.result?.contents?.[0]?.text || '{}');
    const matching = (payload.instances || []).filter(i =>
      JSON.stringify(i).replaceAll('\\\\', '/').replaceAll('\\', '/').includes('AbilityKit'));
    const all = payload.instances || [];
    const active = matching.length === 1 ? matching[0] : all.length === 1 ? all[0] : null;
    if (!active) throw new Error('No unique AbilityKit Unity instance; refusing an ambiguous tool call.');
    if (active) {
      await request('tools/call', { name: 'set_active_instance',
        arguments: { instance: active.id || active.hash || active.instance_id || active.name } });
    }
  }
  const output = await request(method, params);
  const outputFile = path.resolve(__dirname, '../..', process.argv[4] || 'Docs/GameBattle/Migration/mcp-last-response.json');
  fs.mkdirSync(path.dirname(outputFile), { recursive: true });
  fs.writeFileSync(outputFile, JSON.stringify(output, null, 2));
  console.log(JSON.stringify(output, null, 2));
}
main().catch(e => { console.error(e); process.exitCode = 1; });
