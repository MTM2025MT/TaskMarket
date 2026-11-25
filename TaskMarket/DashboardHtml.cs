namespace TaskMarket
{
    public static class DashboardHtml
    {
        public const string Page = """
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="UTF-8"/>
      <title>Request Dashboard</title>
      <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', sans-serif; background: #0f172a; color: #e2e8f0; padding: 24px; }
        h1 { font-size: 1.5rem; margin-bottom: 16px; color: #7dd3fc; }
        .stats { display: flex; gap: 16px; margin-bottom: 24px; flex-wrap: wrap; }
        .stat { background: #1e293b; border-radius: 10px; padding: 16px 24px; flex: 1; min-width: 140px; }
        .stat-label { font-size: 0.75rem; color: #94a3b8; text-transform: uppercase; letter-spacing: 1px; }
        .stat-value { font-size: 2rem; font-weight: bold; color: #38bdf8; }
        table { width: 100%; border-collapse: collapse; background: #1e293b; border-radius: 10px; overflow: hidden; }
        th { text-align: left; padding: 12px 16px; background: #0f172a; color: #94a3b8; font-size: 0.75rem; text-transform: uppercase; }
        td { padding: 10px 16px; border-top: 1px solid #334155; font-size: 0.875rem; }
        tr:hover td { background: #263548; }
        .badge { display: inline-block; padding: 2px 8px; border-radius: 999px; font-size: 0.75rem; font-weight: bold; }
        .GET { background: #164e63; color: #7dd3fc; }
        .POST { background: #14532d; color: #86efac; }
        .PUT { background: #3b1f09; color: #fdba74; }
        .DELETE { background: #450a0a; color: #fca5a5; }
        .ok { color: #4ade80; }
        .warn { color: #facc15; }
        .err { color: #f87171; }
        .fast { color: #4ade80; }
        .mid { color: #facc15; }
        .slow { color: #f87171; }
        .refresh { float: right; font-size: 0.8rem; color: #64748b; }
      </style>
    </head>
    <body>
      <h1>⚡ Request Timing Dashboard <span class="refresh" id="countdown"></span></h1>
      <div class="stats" id="stats"></div>
      <table>
        <thead>
          <tr><th>Time</th><th>Method</th><th>Path</th><th>Status</th><th>Duration</th></tr>
        </thead>
        <tbody id="rows"></tbody>
      </table>
      <script>
        let countdown = 5;
        async function load() {
          const res = await fetch('/dashboard/data');
          const logs = await res.json();
          const total = logs.length;
          const avg = total ? Math.round(logs.reduce((a,b) => a + b.elapsedMs, 0) / total) : 0;
          const max = total ? Math.max(...logs.map(l => l.elapsedMs)) : 0;
          const errors = logs.filter(l => l.statusCode >= 400).length;
          document.getElementById('stats').innerHTML = `
            <div class="stat"><div class="stat-label">Total Requests</div><div class="stat-value">${total}</div></div>
            <div class="stat"><div class="stat-label">Avg Duration</div><div class="stat-value">${avg}ms</div></div>
            <div class="stat"><div class="stat-label">Slowest</div><div class="stat-value">${max}ms</div></div>
            <div class="stat"><div class="stat-label">Errors (4xx/5xx)</div><div class="stat-value" style="color:${errors?'#f87171':'#4ade80'}">${errors}</div></div>
          `;
          document.getElementById('rows').innerHTML = logs.map(l => {
            const t = new Date(l.timestamp).toLocaleTimeString();
            const ms = l.elapsedMs;
            const msClass = ms < 100 ? 'fast' : ms < 500 ? 'mid' : 'slow';
            const scClass = l.statusCode < 400 ? 'ok' : l.statusCode < 500 ? 'warn' : 'err';
            const m = l.method.toUpperCase();
            return `<tr>
              <td>${t}</td>
              <td><span class="badge ${m}">${m}</span></td>
              <td>${l.path}</td>
              <td class="${scClass}">${l.statusCode}</td>
              <td class="${msClass}">${ms}ms</td>
            </tr>`;
          }).join('');
        }
        load();
        setInterval(() => {
          countdown--;
          document.getElementById('countdown').textContent = `Refreshes in ${countdown}s`;
          if (countdown <= 0) { countdown = 5; load(); }
        }, 4000);
      </script>
    </body>
    </html>
    """;
    }
}
