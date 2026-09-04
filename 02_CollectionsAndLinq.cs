using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 02: コレクション・LINQ・Span (Collections & LINQ)
/// ============================================================================
/// 
/// 【他言語経験者向け要点】
/// 1. LINQ (Language Integrated Query):
///    - C# の最大の強み。RustのイテレータやJavaのStream、Goのslices操作に相当するパイプライン。
///    - 遅延評価 (Deferred Execution) されるため、`.ToList()` などを呼ぶまで実行されない。
///    - .NET 6+ の `Chunk` や `Zip` などの便利メソッドも豊富。
/// 
/// 2. C# 12 コレクション式 & スプレッド演算子 `..`:
///    - `[1, 2, 3]` や `[.. array1, 99, .. array2]` で直感的な配列・リスト初期化が可能。
/// 
/// 3. Span<T> & stackalloc (ゼロアロケーション):
///    - 配列や文字列の部分領域（スライス）をヒープ割り当てゼロで高速に参照。
///    - `stackalloc` と組み合わせることで GC を完全にバイパスした高速メモリ処理を実現。
/// </summary>
public static class CollectionsAndLinq
{
    public record Product(string Id, string Name, string Category, decimal Price, bool InStock);

    public static void Run()
    {
        DemoLinqPipeline();
        DemoCollectionExpressionsAndSpread();
        DemoSpanZeroAllocation();
        DemoStackallocMemory();
        DemoFrozenCollections();
    }

    private static void DemoLinqPipeline()
    {
        Console.WriteLine("=== 1. LINQ (Query Pipeline / Deferred Execution) ===");

        // C# 12 コレクション式 [ ... ]
        List<Product> products =
        [
            new("p1", "MacBook Pro", "Electronics", 250000m, true),
            new("p2", "Mechanical Keyboard", "Electronics", 18000m, true),
            new("p3", "Noise Cancelling Headphones", "Electronics", 35000m, false),
            new("p4", "Rust in Action", "Books", 4200m, true),
            new("p5", "Clean Code", "Books", 3800m, true),
            new("p6", "Coffee Mug", "Misc", 1500m, true)
        ];

        // LINQ クエリ: 在庫ありかつ1万円以上の商品を価格降順で抽出
        var premiumInStock = products
            .Where(p => p.InStock && p.Price >= 10000m)
            .OrderByDescending(p => p.Price)
            .Select(p => new { p.Name, p.Price })
            .ToList();

        Console.WriteLine("Premium In-Stock Products:");
        foreach (var item in premiumInStock)
        {
            Console.WriteLine($"  * {item.Name} ({item.Price:C})");
        }

        // GroupBy によるカテゴリ別集計
        var categoryStats = products
            .GroupBy(p => p.Category)
            .Select(g => new
            {
                Category = g.Key,
                Count = g.Count(),
                AvgPrice = g.Average(p => p.Price),
                TotalValue = g.Where(p => p.InStock).Sum(p => p.Price)
            });

        Console.WriteLine("\nCategory Aggregation:");
        foreach (var stat in categoryStats)
        {
            Console.WriteLine($"  [{stat.Category}] Count: {stat.Count}, AvgPrice: {stat.AvgPrice:C}, TotalInStock: {stat.TotalValue:C}");
        }

        // Chunk (バッチ分割) & Zip (2つのシーケンス結合)
        int[] ids = [1, 2, 3, 4, 5];
        var batches = ids.Chunk(2);
        Console.WriteLine($"Chunked batches count: {batches.Count()} (e.g. first batch size: {batches.First().Length})");
    }

    private static void DemoCollectionExpressionsAndSpread()
    {
        Console.WriteLine("\n=== 2. Collection Expressions & Spread (..) (C# 12) ===");

        int[] front = [1, 2];
        int[] back = [4, 5];

        // スプレッド演算子 '..' による合成
        int[] combined = [.. front, 3, .. back];
        Console.WriteLine($"Combined via spread: [{string.Join(", ", combined)}]");

        // 任意のコレクション型（List, HashSet, ImmutableArray）に同一構文で代入可能
        ImmutableArray<string> immutableNames = ["Alpha", "Beta", "Gamma"];
        Console.WriteLine($"ImmutableArray count: {immutableNames.Length}");
    }

    private static void DemoSpanZeroAllocation()
    {
        Console.WriteLine("\n=== 3. Span<T> & ReadOnlySpan<T> (Zero-Allocation) ===");

        string logLine = "2026-09-03 22:30:00 [ERROR] Connection timed out: 192.168.1.50";

        // ReadOnlySpan<char> を使って文字列を一切ヒープに複製（Substring）せずに解析
        ReadOnlySpan<char> span = logLine.AsSpan();

        // 日時部分のスライシング
        ReadOnlySpan<char> dateSpan = span[..10];
        // ログレベル部分の抽出
        int openBracket = span.IndexOf('[');
        int closeBracket = span.IndexOf(']');
        ReadOnlySpan<char> levelSpan = span.Slice(openBracket + 1, closeBracket - openBracket - 1);
        // メッセージ部分の抽出
        ReadOnlySpan<char> messageSpan = span[(closeBracket + 2)..];

        Console.WriteLine($"Extracted Date:    {dateSpan.ToString()}");
        Console.WriteLine($"Extracted Level:   {levelSpan.ToString()}");
        Console.WriteLine($"Extracted Message: {messageSpan.ToString()}");
        Console.WriteLine("  -> No new string allocations occurred during slicing!");
    }

    private static void DemoStackallocMemory()
    {
        Console.WriteLine("\n=== 4. stackalloc with Span<byte> (Bypassing Heap/GC) ===");

        // ヒープではなくスタックに 128 バイトを直接確保 (unsafe 不要で安全)
        Span<byte> buffer = stackalloc byte[128];
        buffer[0] = 0xAA;
        buffer[1] = 0xBB;

        Console.WriteLine($"Allocated stack buffer length: {buffer.Length} bytes, First bytes: 0x{buffer[0]:X2} 0x{buffer[1]:X2}");
    }

    private static void DemoFrozenCollections()
    {
        Console.WriteLine("\n=== 5. .NET 8 Frozen Collections (Optimized Read-Only) ===");

        var pairs = new Dictionary<string, int>
        {
            ["USD"] = 150,
            ["EUR"] = 165,
            ["GBP"] = 195
        };

        // 読み取り専用に凍結し、検索処理を最適化
        FrozenDictionary<string, int> rates = pairs.ToFrozenDictionary();

        if (rates.TryGetValue("USD", out int rate))
        {
            Console.WriteLine($"Frozen rate for USD: JPY {rate}");
        }
    }
}
