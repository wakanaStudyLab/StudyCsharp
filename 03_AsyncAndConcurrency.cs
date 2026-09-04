using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 03: 非同期プログラミング & 並行処理 (Async, Streams & Concurrency)
/// ============================================================================
/// 
/// 【他言語経験者向け要点】
/// 1. async / await の基本ルール:
///    - 戻り値は必ず `Task` または `Task<T>` を使う。
///    - `async void` はイベントハンドラ専用。通常コードで書くと未補足例外でプロセスが死ぬ。
///    - 同期的に完了する可能性が高いホットパスには `ValueTask<T>` を使う（ヒープアロケーション削減）。
/// 
/// 2. IAsyncEnumerable<T> & await foreach (C# 8+):
///    - 非同期に生成されるデータストリームを順次消費（gRPC や LLM のストリーミングに必須）。
/// 
/// 3. C# 13 / .NET 9 新型 System.Threading.Lock:
///    - 従来の `object` に対する Monitor ロックに比べ、高速かつ型安全な専用ロックオブジェクト。
/// 
/// 4. Channels (Go の channel 相当):
///    - 高性能でスレッドセーフな非同期プロデューサー・コンシューマーキュー。
/// </summary>
public static class AsyncAndConcurrency
{
    // C# 13 / .NET 9 新型ロックオブジェクト
    private static readonly Lock SyncLock = new();
    private static int SharedCounter = 0;

    public static async Task RunAsync()
    {
        await DemoAsyncAwaitAndCancellation();
        await DemoAsyncEnumerableStreaming();
        await DemoParallelForEachAsync();
        await DemoChannelsProducerConsumer();
        DemoModernLock();
    }

    private static async Task DemoAsyncAwaitAndCancellation()
    {
        Console.WriteLine("=== 1. async/await & CancellationToken ===");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        try
        {
            // 50ms の高速タスク (正常完了)
            string res1 = await FetchDataAsync("Service-A", 50, cts.Token);
            Console.WriteLine($"Result 1: {res1}");

            // 500ms のタスク (200ms でタイムアウトキャンセルされる)
            Console.WriteLine("Starting slow task (will be cancelled)...");
            string res2 = await FetchDataAsync("Service-B", 500, cts.Token);
            Console.WriteLine($"Result 2: {res2}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  -> Successfully caught OperationCanceledException (Timeout)!");
        }
    }

    private static async Task<string> FetchDataAsync(string name, int delayMs, CancellationToken ct)
    {
        await Task.Delay(delayMs, ct);
        return $"{name} Data (waited {delayMs}ms)";
    }

    private static async Task DemoAsyncEnumerableStreaming()
    {
        Console.WriteLine("\n=== 2. IAsyncEnumerable<T> & await foreach (Streaming) ===");

        // 非同期ストリームからデータを1件ずつ非同期で消費
        await foreach (int number in GenerateNumbersAsync(3, 20))
        {
            Console.WriteLine($"  [Stream Item Received]: {number}");
        }
    }

    // 非同期ジェネレータ (yield return と await の併用)
    private static async IAsyncEnumerable<int> GenerateNumbersAsync(
        int count,
        int delayMs,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 1; i <= count; i++)
        {
            await Task.Delay(delayMs, ct);
            yield return i * 10;
        }
    }

    private static async Task DemoParallelForEachAsync()
    {
        Console.WriteLine("\n=== 3. Parallel.ForEachAsync (Controlled Concurrency) ===");

        int[] items = [1, 2, 3, 4, 5, 6];

        // 最大並列度 3 で非同期バッチ処理
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 3
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Parallel.ForEachAsync(items, options, async (item, ct) =>
        {
            await Task.Delay(30, ct); // I/O 待機シミュレーション
        });
        sw.Stop();

        Console.WriteLine($"Processed {items.Length} items in parallel ({sw.ElapsedMilliseconds}ms)");
    }

    private static async Task DemoChannelsProducerConsumer()
    {
        Console.WriteLine("\n=== 4. System.Threading.Channels (Go-like Channel) ===");

        var channel = Channel.CreateBounded<string>(5);

        var producer = Task.Run(async () =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await channel.Writer.WriteAsync($"Message #{i}");
            }
            channel.Writer.Complete();
        });

        var consumer = Task.Run(async () =>
        {
            await foreach (string message in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"  [Consumer received]: {message}");
            }
        });

        await Task.WhenAll(producer, consumer);
    }

    private static void DemoModernLock()
    {
        Console.WriteLine("\n=== 5. System.Threading.Lock (.NET 9 / C# 13) ===");

        // 新型 Lock クラスに対して lock 文がそのまま使える
        lock (SyncLock)
        {
            SharedCounter++;
            Console.WriteLine($"Thread-safe counter value under Lock: {SharedCounter}");
        }
    }
}
