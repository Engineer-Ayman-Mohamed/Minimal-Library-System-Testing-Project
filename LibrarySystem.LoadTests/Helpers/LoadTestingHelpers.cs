using System.Diagnostics;

namespace LibrarySystem.LoadTests.Helpers;


public record RequestMetric(
    int StatusCode,
    long ResponseTimeMs,
    bool IsSuccess,
    DateTime Timestamp = default);

public class MetricsCollector
{
    private readonly List<RequestMetric> _metrics = new(1000);
    private readonly Stopwatch _totalStopwatch = new();
    private readonly object _lock = new();

    public void Start() => _totalStopwatch.Start();
    public void Stop() => _totalStopwatch.Stop();

    public void Record(int statusCode, long responseTimeMs)
    {
        lock (_lock)
        {
            _metrics.Add(new RequestMetric(
                statusCode,
                responseTimeMs,
                statusCode is >= 200 and < 300,
                DateTime.UtcNow));
        }
    }

    public int Total
    {
        get { lock (_lock) return _metrics.Count; }
    }

    public double ErrorRate
    {
        get
        {
            lock (_lock)
            {
                return _metrics.Count is 0 ? 0 : (double)_metrics.Count(m => !m.IsSuccess) / _metrics.Count;
            }
        }
    }

    public double AverageMs
    {
        get { lock (_lock) return _metrics.Count is 0 ? 0 : _metrics.Average(m => m.ResponseTimeMs); }
    }

    public long Percentile(int p)
    {
        lock (_lock)
        {
            if (_metrics.Count is 0) return 0;
            var sorted = _metrics.Select(m => m.ResponseTimeMs).OrderBy(t => t).ToList();
            var index = (int)Math.Ceiling(p / 100.0 * sorted.Count) - 1;
            return sorted[Math.Max(0, index)];
        }
    }

    public long MaxMs
    {
        get { lock (_lock) return _metrics.Count is 0 ? 0 : _metrics.Max(m => m.ResponseTimeMs); }
    }

    public double Throughput
    {
        get
        {
            var elapsed = _totalStopwatch.Elapsed.TotalSeconds;
            return elapsed is 0 ? 0 : Total / elapsed;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _metrics.Clear();
            _totalStopwatch.Restart();
        }
    }

    public string Summary()
    {
        lock (_lock)
        {
            var p95 = Percentile(95);
            var p99 = Percentile(99);
            var avg = AverageMs;
            var rate = ErrorRate;

            return $"""
            {"Metric",-25} {"Value",-12}
            {"─" + new string('─', 24),-25} {"─" + new string('─', 11),-12}
            {"Total Requests",-25} {Total,-12:N0}
            {"Successful",-25} {Total - (int)(rate * Total),-12:N0} ({(1 - rate) * 100,-7:F2}%)
            {"Failed",-25} {(int)(rate * Total),-12:N0} ({rate * 100,-7:F2}%)
            {"Average (ms)",-25} {avg,-12:F0}
            {"P50 (ms)",-25} {Percentile(50),-12}
            {"P95 (ms)",-25} {p95,-12}
            {"P99 (ms)",-25} {p99,-12}
            {"Max (ms)",-25} {MaxMs,-12}
            {"Throughput (req/s)",-25} {Throughput,-12:F1}
            {"Duration (s)",-25} {_totalStopwatch.Elapsed.TotalSeconds,-12:F1}
            """;
        }
    }
}