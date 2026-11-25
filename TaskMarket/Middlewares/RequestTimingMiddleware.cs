  using System.Collections.Concurrent;
  using System.Diagnostics;
  namespace TaskMarket.Middlewares
    {

    public record RequestLog(string Method, string Path, int StatusCode, long ElapsedMs, DateTime Timestamp);

    public static class RequestStore
    {
        public static readonly ConcurrentQueue<RequestLog> Logs = new();
        public const int MaxLogs = 200;
    }

    //public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    //{
    //    public async Task InvokeAsync(HttpContext context)
    //    {
    //        var sw = Stopwatch.StartNew();

    //        await next(context);

    //        sw.Stop();

    //        var log = new RequestLog(
    //            context.Request.Method,
    //            context.Request.Path,
    //            context.Response.StatusCode,
    //            sw.ElapsedMilliseconds,
    //            DateTime.UtcNow
    //        );

    //        // Console log
    //        logger.LogInformation("[{Time}] {Method} {Path} → {Status} in {Ms}ms",
    //            log.Timestamp.ToString("HH:mm:ss"),
    //            log.Method, log.Path, log.StatusCode, log.ElapsedMs);

    //        // Store for dashboard
    //        RequestStore.Logs.Enqueue(log);
    //        while (RequestStore.Logs.Count > RequestStore.MaxLogs)
    //            RequestStore.Logs.TryDequeue(out _);
    //    }
    //}
    public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        private static readonly string LogFilePath = Path.Combine(AppContext.BaseDirectory, "request-logs.txt");
        private static readonly SemaphoreSlim FileLock = new(1, 1);

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            await next(context);

            sw.Stop();

            // The dashboard polls its own data endpoint; logging that would drown out real traffic
            if (context.Request.Path.StartsWithSegments("/dashboard"))
            {
                return;
            }

            var line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} → {context.Response.StatusCode} in {sw.ElapsedMilliseconds}ms";

            // Console log (as before)
            logger.LogInformation(line);

            // Write to text file
            await WriteToFileAsync(line);
        }

        private static async Task WriteToFileAsync(string line)
        {
            await FileLock.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(LogFilePath, line + Environment.NewLine);
            }
            finally
            {
                FileLock.Release();
            }
        }
    }
}
