# Modern C# 実践完全解説ガイド (Crash Course All-in-One Guide)

本ドキュメントは、`C:\Users\harun\programming\c#\sample` に含まれるすべてのコード（モジュール 01〜07、`Program.cs`、`sample.csproj`）の内容、実行結果、動作原理、およびモダン C#（C# 12 / 13 on .NET 9）の言語仕様を**これ 1 つ読めば網羅的かつ完全に理解できるようにまとめた総合解説書**です。

Rust, Go, Python, Java, C++ などの経験を持つエンジニアが、最短でモダン C# のコア技術とイディオムを把握できるように設計されています。

---

## 📑 目次

1. [言語対比マッピング早見表 (C# vs 他言語)](#1-言語対比マッピング早見表-c-vs-他言語)
2. [他言語経験者がハマる C# 5大落とし穴とベストプラクティス](#2-他言語経験者がハマる-c-5大落とし穴とベストプラクティス)
3. [統合エントリーポイント: `Program.cs`](#3-統合エントリーポイント-programcs)
4. [モジュール 01: 基本型・レコード・モダン構文 (`01_BasicsAndTypes.cs`)](#4-モジュール-01-基本型レコードモダン構文-01_basicsandtypescs)
5. [モジュール 02: コレクション・LINQ・Span (`02_CollectionsAndLinq.cs`)](#5-モジュール-02-コレクションlinqspan-02_collectionsandlinqcs)
6. [モジュール 03: 非同期プログラミング & 並行処理 (`03_AsyncAndConcurrency.cs`)](#6-モジュール-03-非同期プログラミング--並行処理-03_asyncandconcurrencycs)
7. [モジュール 04: 例外処理・リソース管理 (`04_ExceptionAndResource.cs`)](#7-モジュール-04-例外処理リソース管理-04_exceptionandresourcecs)
8. [モジュール 05: ラムダ式・デリゲート・式ツリー (`05_LambdasAndDelegates.cs`)](#8-モジュール-05-ラムダ式デリゲート式ツリー-05_lambdasanddelegatescs)
9. [モジュール 06: ジェネリクス・インターフェース・モダン型システム (`06_GenericsAndInterfaces.cs`)](#9-モジュール-06-ジェネリクスインターフェースモダン型システム-06_genericsandinterfacescs)
10. [モジュール 07: 高度な言語機能・イテレータ・拡張メソッド (`07_AdvancedLanguageFeatures.cs`)](#10-モジュール-07-高度な言語機能イテレータ拡張メソッド-07_advancedlanguagefeaturescs)
11. [プロジェクト設定 (`sample.csproj`) と関連ガイド](#11-プロジェクト設定-samplecsproj-と関連ガイド)

---

## 1. 言語対比マッピング早見表 (C# vs 他言語)

C# の各機能が、他の主要プログラミング言語（Rust, Go, Java, Python）のどの概念に相当するかを整理した対応表です。

| 概念・機能 | Modern C# (12 / 13) | Rust | Go | Java (21+) | Python (3.10+) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **ローカル型推論** | `var x = 10;` | `let x = 10;` | `x := 10` | `var x = 10;` | `x = 10` |
| **不変データ構造** | `record User(...)` | `struct User` | `type User struct` | `record User(...)` | `@dataclass(frozen=True)` |
| **スタック値型** | `struct` / `record struct` | `struct` (値型) | `struct` (値型) | ❌ (ヒープ参照のみ) | ❌ (ヒープ参照のみ) |
| **ゼロコピー切り出し** | `Span<T>` / `ReadOnlySpan<T>` | `&[T]` / `&str` | `slice [a:b]` | ❌ (配列コピー発生) | `memoryview` / slice |
| **値の同値性比較** | `==` (オーバーロード可) | `==` (`PartialEq`) | `==` | `a.equals(b)` | `==` (`__eq__`) |
| **参照の一致比較** | `ReferenceEquals(a, b)` | `std::ptr::eq` | ポインタ比較 | `a == b` | `a is b` |
| **パターンマッチング** | switch 式 / プロパティ / リスト | `match` 式 | type switch | switch パターン | `match ... case` |
| **コレクション操作** | **LINQ** (`Where`, `Select`) | `.iter().filter().map()` | slices / ループ | Stream API | 内包表記 / `filter` |
| **Null 安全性** | `string?` (NRT) | `Option<T>` | `*string` / `nil` | `Optional<T>` | `str \| None` |
| **リソース自動解放** | `using var r = ...` | `Drop` トレイト | `defer r.Close()` | `try (var r = ...)` | `with r:` |
| **非同期 Promise** | `Task<T>` / `ValueTask<T>` | `Future<T>` (tokio) | goroutine + channel | `CompletableFuture` | `asyncio.Task` |
| **並行パイプライン** | `System.Threading.Channels` | `tokio::sync::mpsc` | **channel** (`chan T`) | `BlockingQueue` | `asyncio.Queue` |

---

## 2. 他言語経験者がハマる C# 5大落とし穴とベストプラクティス

### ① `record` のコレクションプロパティ等価性の罠
- **現象**: `record` は自動生成される `Equals` で全フィールドを値比較しますが、**`string[]` や `List<T>` などのコレクション型プロパティは「参照比較（同一インスタンスか）」** になります。要素が完全に同一でも `u1 == u2` は `false` になります。
- **対策**: 配列やリストを含む record の等価性を担保したい場合は、カスタム `Equals` を実装するか `IEqualityComparer`（`StructuralComparisons.StructuralEqualityComparer`）を使用します。

### ② `async void` は絶対に書いてはいけない
- **現象**: `async void` は GUI のボタンクリックなどのイベントハンドラ用に作られた例外的な構文です。通常のコードで使うと、**内部の例外を `try-catch` で捕捉できず、プロセス全体が即座に強制終了**します。
- **対策**: 戻り値のない非同期メソッドは必ず `async Task` と宣言してください。

### ③ `IEnumerable<T>` の二重評価 (Multiple Enumeration)
- **現象**: LINQ のメソッド（`Where`, `Select` など）は**遅延評価（Deferred Execution）**です。クエリ結果の変数を複数回 `foreach` で回したり、`.Any()` の直後に `.Count()` を呼ぶと、フィルタ計算や背後の DB クエリが複数回実行されます。
- **対策**: 結果を再利用する場合は、必ず末尾で `.ToList()` や `.ToArray()` を呼び出してメモリ上に実体化（Materialize）してください。

### ④ `Span<T>` の制約 (`ref struct` の生存期間ルール)
- **現象**: `Span<T>` はスタック上にしか存在できない `ref struct` です。ヒープ領域への脱出が禁じられているため、**通常のクラスのフィールドに保持すること、`async` メソッドの `await` をまたぐこと、object にボックス化することはコンパイルエラー**になります。
- **対策**: 非同期処理やフィールド保持が必要な場合は、ヒープ対応の `Memory<T>` / `ReadOnlyMemory<T>` を使用します。

### ⑤ 構造体（`struct`）の暗黙コピーと防御的コピー
- **現象**: C# の `struct` は代入や引数渡しのたびに値の全バイトがメモリコピーされます。また、`readonly` ではない struct に対して読み取り専用メソッドを呼ぶと、コンパイラが安全のために内部で「防御的コピー」を発生させ、意図しない性能劣化を招きます。
- **対策**: 変更不要な構造体は必ず `readonly struct` または `readonly record struct` として宣言し、引数には `in` 修飾子を使用します。

---

## 3. 統合エントリーポイント: `Program.cs`

### 概要
C# 9+ の**トップレベルステートメント**を採用しており、従来の `class Program { static async Task Main(string[] args) { ... } }` という冗長なボイラープレートなしで直接コードを実行できます。

### 実行フロー
1. 実行環境の .NET バージョン（例: `.NET 9.0.19`）をバナー表示
2. モジュール 01 から 07 を順番に呼び出し（非同期メソッドは `await` 実行）
3. 完了バナーを出力して正常終了 (Exit Code: 0)

```csharp
using Sample;

PrintBanner("MODERN C# CRASH COURSE (Running on .NET " + Environment.Version + ")");

// 順次各モジュールを実行
BasicsAndTypes.Run();
CollectionsAndLinq.Run();
await AsyncAndConcurrency.RunAsync();
await ExceptionAndResource.RunAsync();
LambdasAndDelegates.Run();
GenericsAndInterfaces.Run();
AdvancedLanguageFeatures.Run();

PrintBanner("ALL C# TUTORIAL MODULES COMPLETED SUCCESSFULLY!");
```

---

## 4. モジュール 01: 基本型・レコード・モダン構文 (`01_BasicsAndTypes.cs`)

### 4-1. `record class` と `record struct`
C# 9 で追加された `record` は、イミュータブルなデータ転送オブジェクト（DTO）やドメインモデルに最適です。

- **値ベースの等価性 (Value Equality)**: `==` 演算子が全プロパティの同値性を比較（クラスの標準である参照比較とは異なる）。
- **非破壊的更新 (`with` 式)**: 元のインスタンスを変更せず、指定プロパティのみを差し替えた新しいインスタンスを生成。
- **`record struct`**: スタック上に確保され、ヒープ割り当て（GC）が発生しない軽量レコード。

```csharp
public record Person(string Id, string Name, int Age);
public readonly record struct GeoPoint(double Latitude, double Longitude);

var p1 = new Person("p101", "Alice", 28);
var p2 = new Person("p101", "Alice", 28);

// 値ベース比較なので True になる (参照は異なるため ReferenceEquals は False)
bool isEqual = (p1 == p2); // True

// with 式によるイミュータブルな複製更新
var p3 = p1 with { Age = 29 };
```

### 4-2. プライマリコンストラクタ (C# 12) & required プロパティ (C# 11)
- **プライマリコンストラクタ**: クラス宣言の引数としてコンストラクタを定義。プライベートフィールドの手動宣言と代入を省略できます。
- **`required` プロパティ**: オブジェクト初期化子（`new OrderRequest { ... }`）で必須のプロパティをコンパイル時に強制します。
- **`init` 専用セッター**: 初期化時のみ書き込み可能で、以降は変更不可（不変性を保証）。

```csharp
// プライマリコンストラクタ (C# 12)
public class ServiceEndpoint(string host, int port)
{
    public string Url => $"https://{host}:{port}";
}

// required + init (C# 11)
public class OrderRequest
{
    public required string OrderId { get; init; }
    public required decimal TotalAmount { get; init; }
    public string Note { get; init; } = string.Empty;
}
```

### 4-3. パターンマッチング (Switch 式・プロパティ・関係パターン)
C# のパターンマッチングは非常に強力で、型の判定、内部プロパティの条件、数値の範囲判定を 1 つの式で表現できます。

```csharp
string result = payment switch
{
    CreditCard { Balance: >= 10000m } c => $"CreditCard Approved (High Balance: {c.Balance:C})",
    CreditCard c => $"CreditCard Approved (Normal: {c.Balance:C})",
    CryptoTransfer { Network: "Ethereum" } eth => $"Crypto ETH to {eth.WalletAddress}",
    CryptoTransfer crypto => $"Other Crypto ({crypto.Network}) to {crypto.WalletAddress}",
    BankAccount bank => $"Bank transfer via {bank.BankName}",
    null => "Payment method is null",
    _ => "Unknown payment method"
};

// 関係パターン (and / or / not / 比較演算子)
string weather = temperature switch
{
    < 0 => "Freezing",
    >= 0 and < 15 => "Cold",
    >= 15 and < 28 => "Pleasant",
    _ => "Hot"
};
```

### 4-4. インデックス (`^`)・範囲 (`..`)・リストパターン
Python のスライスや Rust のレンジに近い構文を型安全に提供します。
- `^1`: 末尾から 1 番目の要素（`length - 1`）
- `1..^1`: 1 番目から末尾の 1 つ手前までのスライス
- **リストパターン (C# 11)**: コレクションの要素構成をパターンマッチ（`[var first, .., var last]`）

```csharp
string[] words = ["zero", "one", "two", "three", "four", "five"];
string lastWord = words[^1];         // "five"
string[] slice = words[1..^1];       // ["one", "two", "three", "four"]

// リストパターン
int[] numbers = [10, 20, 30, 40, 50];
string analysis = numbers switch
{
    [var first, .., var last] => $"Starts with {first} and ends with {last}",
    [var single] => $"Single element: {single}",
    [] => "Empty list",
    _ => "Other pattern"
};
```

### 4-5. 生文字列リテラル (Raw String Literals: C# 11)
`"""` で囲むことで、バックスラッシュ（`\`）や改行、ダブルクォーテーションのエスケープが不要になります。
`$$"""` のように `$` を複数個前置すると、その個数の波括弧（`{{...}}`）だけを変数展開として扱い、単一の波括弧 `{}` はそのまま JSON 構文として扱えます。

```csharp
string author = "Harun";
int year = 2026;

string json = $$"""
{
    "title": "Modern C# Guide",
    "author": "{{author}}",
    "year": {{year}}
}
""";
```

### 4-6. Null 許容参照型 (Nullable Reference Types: NRT)
C# 8 以降、プロジェクト設定 `<Nullable>enable</Nullable>` により、すべての参照型はデフォルトで non-null になります。null を許容する場合は `string?` と明示します。
- `??=`: null の場合のみ代入（null 合体代入）
- `?.`: null 条件演算子（null なら後続を評価せず null を返す）
- `??`: null 合体演算子（null 時のフォールバック値を指定）

### 実行結果 (モジュール 01)
```text
=== 1. Records & Value-based Equality ===
p1 == p2 (Record Value Equality): True
ReferenceEquals(p1, p2):          False
Original p1 Age: 28, Cloned p3 Age: 29
u1 == u2 (Collection property trap): False (Array uses ReferenceEquals!)
GeoPoint (Record Struct): 35.6812, 139.7671

=== 2. Primary Constructors (C# 12) & Required Properties (C# 11) ===
Endpoint URL: https://api.example.com:443
Order: ORD-2026-999, Amount: ¥4,500

=== 3. Modern Pattern Matching (Switch Expressions) ===
Payment Status: CreditCard Approved (High Balance: ¥15,000)
Temperature 25C is: Pleasant

=== 4. Index (^), Range (..), and List Patterns ===
First: zero, Last (^1): five, Second to last (^2): four
Slice [1..^1]: [one, two, three, four]
List pattern analysis: Starts with 10 and ends with 50

=== 5. Raw String Literals (C# 11: """ ... """) ===
{
    "title": "Modern C# Guide",
    "author": "Harun",
    "year": 2026,
    "features": ["Records", "Pattern Matching", "Raw Strings"]
}

=== 6. Nullable Reference Types (NRT) ===
Resolved Name: Default Guest
Display Email: no-email@domain.com
```

---

## 5. モジュール 02: コレクション・LINQ・Span (`02_CollectionsAndLinq.cs`)

### 5-1. LINQ (Language Integrated Query) パイプライン
LINQ は C# の最も洗練された機能の 1 つです。関数型プログラミングスタイルの高階関数（`Where`, `Select`, `OrderBy`, `GroupBy`）を連結してデータパイプラインを構築します。

- **遅延評価**: `Where` や `Select` を呼んだ時点ではイテレーションは始まらず、`foreach` で走査されたり `.ToList()` が呼ばれた瞬間にオンデマンドで評価されます。
- **.NET 6+ の新機能**: `.Chunk(size)` によるバッチ分割、`.Zip()` による複数シーケンスの同時走査などが追加されています。

```csharp
var premiumInStock = products
    .Where(p => p.InStock && p.Price >= 10000m)
    .OrderByDescending(p => p.Price)
    .Select(p => new { p.Name, p.Price })
    .ToList(); // ここで実体化

// GroupBy による集計
var categoryStats = products
    .GroupBy(p => p.Category)
    .Select(g => new
    {
        Category = g.Key,
        Count = g.Count(),
        AvgPrice = g.Average(p => p.Price),
        TotalValue = g.Where(p => p.InStock).Sum(p => p.Price)
    });
```

### 5-2. コレクション式とスプレッド演算子 `..` (C# 12)
C# 12 から導入されたコレクション式 `[...]` により、配列、`List<T>`、`HashSet<T>`、`ImmutableArray<T>`、`Span<T>` をすべて同一の統一構文で宣言・初期化できます。スプレッド演算子 `..` を使うと、既存のコレクションを展開して結合できます。

```csharp
int[] front = [1, 2];
int[] back = [4, 5];

// スプレッド合成
int[] combined = [.. front, 3, .. back]; // [1, 2, 3, 4, 5]

// 任意のコレクション型にそのまま代入可能
ImmutableArray<string> immutableNames = ["Alpha", "Beta", "Gamma"];
```

### 5-3. `Span<T>` & `ReadOnlySpan<T>` (ゼロアロケーション)
従来の `string.Substring()` は切り出すたびに新しい文字列インスタンスをヒープに確保（GC 対象）していました。`ReadOnlySpan<char>` を使用すると、**元のメモリバッファの特定範囲をポインタと長さだけで参照**するため、ヒープ確保が一切発生しません。

```csharp
string logLine = "2026-09-03 22:30:00 [ERROR] Connection timed out: 192.168.1.50";
ReadOnlySpan<char> span = logLine.AsSpan();

// ヒープメモリ割り当てゼロで各要素を切り出し
ReadOnlySpan<char> dateSpan = span[..10];
int openBracket = span.IndexOf('[');
int closeBracket = span.IndexOf(']');
ReadOnlySpan<char> levelSpan = span.Slice(openBracket + 1, closeBracket - openBracket - 1);
ReadOnlySpan<char> messageSpan = span[(closeBracket + 2)..];
```

### 5-4. `stackalloc` によるスタックメモリ直接確保
`Span<T>` と `stackalloc` を組み合わせることで、C言語のようにスタック領域に直接バッファを確保できます。ヒープアロケーションが不要なため、超高速かつ GC の負荷が完全にゼロになります（`unsafe` キーワード不要でメモリ安全）。

```csharp
// スタックに直接 128 バイトを確保
Span<byte> buffer = stackalloc byte[128];
buffer[0] = 0xAA;
buffer[1] = 0xBB;
```

### 5-5. .NET 8 `FrozenDictionary` / `FrozenSet`
起動時や初期化時に作成され、以降は読み取り専用となるマスタデータやキャッシュに最適化されたコレクションです。構築時にハッシュ計算アルゴリズムを特殊最適化し、通常の `Dictionary` よりも高速なキー検索を実現します。

```csharp
FrozenDictionary<string, int> rates = pairs.ToFrozenDictionary();
if (rates.TryGetValue("USD", out int rate)) { ... }
```

### 実行結果 (モジュール 02)
```text
=== 1. LINQ (Query Pipeline / Deferred Execution) ===
Premium In-Stock Products:
  * MacBook Pro (¥250,000)
  * Mechanical Keyboard (¥18,000)

Category Aggregation:
  [Electronics] Count: 3, AvgPrice: ¥101,000, TotalInStock: ¥268,000
  [Books] Count: 2, AvgPrice: ¥4,000, TotalInStock: ¥8,000
  [Misc] Count: 1, AvgPrice: ¥1,500, TotalInStock: ¥1,500
Chunked batches count: 3 (e.g. first batch size: 2)

=== 2. Collection Expressions & Spread (..) (C# 12) ===
Combined via spread: [1, 2, 3, 4, 5]
ImmutableArray count: 3

=== 3. Span<T> & ReadOnlySpan<T> (Zero-Allocation) ===
Extracted Date:    2026-09-03
Extracted Level:   ERROR
Extracted Message: Connection timed out: 192.168.1.50
  -> No new string allocations occurred during slicing!

=== 4. stackalloc with Span<byte> (Bypassing Heap/GC) ===
Allocated stack buffer length: 128 bytes, First bytes: 0xAA 0xBB

=== 5. .NET 8 Frozen Collections (Optimized Read-Only) ===
Frozen rate for USD: JPY 150
```

---

## 6. モジュール 03: 非同期プログラミング & 並行処理 (`03_AsyncAndConcurrency.cs`)

### 6-1. `async / await` と `CancellationToken`
C# の非同期処理は `Task`（戻り値ありなら `Task<T>`）を基盤とします。タイムアウトやユーザーによる中断処理には `CancellationToken` を渡すのが標準作法です。

- `CancellationTokenSource(TimeSpan)` により、指定時間経過で自動キャンセルを発火。
- キャンセルされると `OperationCanceledException` がスローされ、リソースを速やかに解放可能。

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

try
{
    // 500ms かかるタスクに 200ms トークンを渡す
    string res = await FetchDataAsync("Service-B", 500, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("タイムアウトにより正常にキャンセルされました");
}
```

### 6-2. `IAsyncEnumerable<T>` と `await foreach` (非同期ストリーミング)
C# 8 で導入された非同期ストリームは、**「各要素が非同期に到着するシーケンス」** を表します。LLM のトークンストリーミング、gRPC サーバーからのプッシュ通知、大容量データのチャンク処理に必須の機能です。

- 供給側: `async IAsyncEnumerable<T>` と `yield return`
- 消費側: `await foreach (var item in stream)`

```csharp
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

// 消費側
await foreach (int number in GenerateNumbersAsync(3, 20))
{
    Console.WriteLine($"Received: {number}");
}
```

### 6-3. `Parallel.ForEachAsync` (.NET 6+)
大量の非同期タスクを実行する際、`Task.WhenAll` で一斉に起動すると接続数上限やスレッド枯渇を起こす危険があります。`Parallel.ForEachAsync` を使うと、`MaxDegreeOfParallelism` で同時実行並列数を厳密に制御しながら非同期バッチ処理を行えます。

```csharp
var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };

await Parallel.ForEachAsync(items, options, async (item, ct) =>
{
    await ProcessItemAsync(item, ct);
});
```

### 6-4. `System.Threading.Channels` (Go 言語相当の Channel)
Go の `chan T` と同等の機能を提供する、ハイパフォーマンスでスレッドセーフな非同期プロデューサー・コンシューマーキューです。ロックフリーアルゴリズムで実装されており、スループットが極めて高いのが特徴です。

- `Channel.CreateBounded<T>(capacity)`: バッファサイズ制限付きチャンネル
- `channel.Writer.WriteAsync(item)`: プロデューサーの書き込み
- `channel.Writer.Complete()`: 終了通知
- `channel.Reader.ReadAllAsync()`: コンシューマーの順次読み出し

### 6-5. 新型 `System.Threading.Lock` (.NET 9 / C# 13)
従来の C# では `lock (new object())` というように任意のヒープオブジェクトをロック対象にしていましたが、.NET 9 / C# 13 で専用の `System.Threading.Lock` クラスが導入されました。内部実装が軽量化され、型安全かつ高速に排他制御が行えます。

```csharp
private static readonly Lock SyncLock = new();

lock (SyncLock)
{
    SharedCounter++;
}
```

### 実行結果 (モジュール 03)
```text
=== 1. async/await & CancellationToken ===
Result 1: Service-A Data (waited 50ms)
Starting slow task (will be cancelled)...
  -> Successfully caught OperationCanceledException (Timeout)!

=== 2. IAsyncEnumerable<T> & await foreach (Streaming) ===
  [Stream Item Received]: 10
  [Stream Item Received]: 20
  [Stream Item Received]: 30

=== 3. Parallel.ForEachAsync (Controlled Concurrency) ===
Processed 6 items in parallel (76ms)

=== 4. System.Threading.Channels (Go-like Channel) ===
  [Consumer received]: Message #1
  [Consumer received]: Message #2
  [Consumer received]: Message #3

=== 5. System.Threading.Lock (.NET 9 / C# 13) ===
Thread-safe counter value under Lock: 1
```

---

## 7. モジュール 04: 例外処理・リソース管理 (`04_ExceptionAndResource.cs`)

### 7-1. `using` 宣言による RAII 自動リソース解放 (`IDisposable`)
C# 8 から、中括弧 `{}` でブロックを囲む従来の `using (...) { }` に加え、スコープの末尾で自動破棄される `using var` 宣言が使えます（Rust の `Drop` や C++ の RAII に相当）。

```csharp
// メソッド終了時に自動的に session.Dispose() が呼び出される
using var session = new DatabaseSession("conn-sync-999");
session.Query("SELECT * FROM users");
```

### 7-2. `await using` 宣言 (`IAsyncDisposable`)
非同期でネットワークコネクションを切断したり、バッファをリモートストレージにフラッシュして閉じるリソースのための構文です。`DisposeAsync()` が `await` されて安全に解放されます。

```csharp
await using var session = new DatabaseSession("conn-async-123");
session.Query("INSERT INTO audit_logs VALUES ('action')");
```

### 7-3. 例外フィルター (`catch ... when ...`)
`catch (Exception ex) when (条件式)` の構文を使うと、**「例外スタックを巻き戻す前」に条件を評価**できます。条件に合致しない例外は catch 節に入らずそのまま上位へ伝播するため、スタックトレースが破壊（変形）されずにデバッグ情報が完全に保持されます。

```csharp
try
{
    throw new BusinessRuleException("ERR_INSUFFICIENT_FUNDS", "残高不足");
}
catch (BusinessRuleException ex) when (ex.Code == "ERR_INSUFFICIENT_FUNDS")
{
    // 特定のエラーコードの場合だけ捕捉
    Console.WriteLine($"Filtered Catch: {ex.Message}");
}
```

### 7-4. ガード節と `[CallerArgumentExpression]` (C# 10)
引数の null チェックには、一行で書ける `ArgumentNullException.ThrowIfNull(arg)` が標準提供されています。
また、`[CallerArgumentExpression]` 属性を付与した引数を定義すると、**呼び出し元で渡された変数名や式そのものをコンパイラが自動的に文字列として注入**してくれます。

```csharp
// null の場合、ParamName に自動的に "invalidInput" が入る
string? invalidInput = null;
ArgumentNullException.ThrowIfNull(invalidInput);

// 自作バリデータでの応用
private static void EnsureNotNull<T>(
    T? value,
    [CallerArgumentExpression(nameof(value))] string? expr = null) where T : class
{
    if (value is null)
    {
        throw new ArgumentNullException(expr, $"式 '{expr}' は null です。");
    }
}
```

### 実行結果 (モジュール 04)
```text
=== 1. 'using' Declaration (Auto-Dispose / RAII) ===
  [DB] Session opened: conn-sync-999
  [DB] Executing: SELECT * FROM users (conn-sync-999)
Exiting DemoUsingDeclaration scope...
  [DB] Session disposed (Cleaned up): conn-sync-999

=== 2. 'await using' Declaration (IAsyncDisposable) ===
  [DB] Session opened: conn-async-123
  [DB] Executing: INSERT INTO audit_logs VALUES ('action') (conn-async-123)
Exiting DemoAsyncUsingDeclaration scope...
  [DB] Session disposed (Cleaned up): conn-async-123

=== 3. Exception Filters ('catch ... when ...') ===
  [Filtered Catch]: Handled code 'ERR_INSUFFICIENT_FUNDS': Account balance is negative

=== 4. Modern Guard Clauses & CallerArgumentExpression (C# 10+) ===
Validated argument successfully: Alice
  -> Caught expected ThrowIfNull: ParamName='invalidInput'
```

---

## 8. モジュール 05: ラムダ式・デリゲート・式ツリー (`05_LambdasAndDelegates.cs`)

> 📖 **より深い内部構造（コンパイラが裏で生成するクラスや IL コードなど）は [`LAMBDA.md`](./LAMBDA.md) に完全解説されています。**

### 8-1. 標準デリゲート (`Func`, `Action`) と自然な型推論
- `Func<T1, T2, TResult>`: 戻り値がある関数
- `Action<T1, T2>`: 戻り値がない（void）関数
- **自然な型推論 (C# 10)**: `var multiply = (int x, int y) => x * y;` のように、左辺に型を明記せず `var` でラムダ式を宣言可能。

### 8-2. クロージャと静的ラムダ (`static lambda`: C# 9)
- **クロージャのオーバーヘッド**: ラムダ式が外側のローカル変数を参照（キャプチャ）すると、コンパイラは裏で隠しクラス（`<>c__DisplayClass`）を生成し、ヒープにインスタンスを割り当てます。
- **静的ラムダ (`static (...) => ...`)**: 外部変数のキャプチャを禁止する修飾子。もし外部変数を使おうとするとコンパイルエラーになります。これにより、**ヒープアロケーション（GC 負荷）が完全にゼロ**であることが保証されます。

```csharp
// 静的ラムダ: 外部変数キャプチャを許さないため GC アロケーションがゼロ
Func<int, int, int> maxFunc = static (a, b) => Math.Max(a, b);
```

### 8-3. ローカル関数 vs ラムダ式
メソッド内に記述する `static int Factorial(int n)` などのローカル関数は、デリゲートオブジェクトを作成しないため、ラムダ式よりも呼び出しオーバーヘッドが少なく、インライン化されやすいという性能上の利点があります。

### 8-4. 式ツリー (Expression Trees: AST)
ラムダ式を `Func<...>` ではなく `Expression<Func<...>>` 型として受け取ると、コンパイラは実行コードではなく**抽象構文木（AST: Abstract Syntax Tree）のオブジェクト**を生成します。

- これが、**Entity Framework Core が C# のラムダ式を解析して SQL の `WHERE` 句に自動変換できるコア技術**です。
- `.Compile()` を呼ぶことで、実行時に動的コンパイルして実行することも可能です。

```csharp
Expression<Func<int, bool>> isAdultExpr = age => age >= 18;

Console.WriteLine(isAdultExpr.Body);           // (age >= 18)
Console.WriteLine(isAdultExpr.Body.NodeType);   // GreaterThanOrEqual

if (isAdultExpr.Body is BinaryExpression binary)
{
    Console.WriteLine(binary.Left);  // age (パラメータ)
    Console.WriteLine(binary.Right); // 18 (定数)
}
```

### 実行結果 (モジュール 05)
```text
=== 1. Built-in Delegates (Func, Action) & Natural Types ===
Func add(10, 20): 30
  [LOG]: Standard Action executed
Natural type lambda multiply(4, 5): 20

=== 2. Closures & C# 9 Static Lambdas ===
Is 70 above 50? True
Static lambda max(15, 25): 25

=== 3. Local Functions vs Lambdas ===
Local function Factorial(5): 120

=== 4. Expression Trees (AST / LINQ to SQL Mechanism) ===
Expression Body:       (age >= 18)
Expression NodeType:   GreaterThanOrEqual
Expression Parameter:  age
  Left operand:  age
  Right operand: 18
Compiled AST result for age 20: True
```

---

## 9. モジュール 06: ジェネリクス・インターフェース・モダン型システム (`06_GenericsAndInterfaces.cs`)

### 9-1. 静的抽象メンバ (Static Abstract Members: C# 11) & Generic Math
C# 11 最大の目玉機能です。インターフェースの中に `static abstract` なメソッドや演算子（`+`, `-` など）を宣言できるようになりました。

.NET 7 で導入された `INumber<T>` はこれを利用しており、`int`, `double`, `decimal`, `float` などのあらゆる数値型に対して、**同一のロジックで動作する汎用計算関数**を完全に型安全かつオーバーヘッドなしで記述できます（Rust のトレイト境界に匹敵）。

```csharp
// int でも double でも同一の関数が動作する
private static T SumAll<T>(ReadOnlySpan<T> numbers) where T : INumber<T>
{
    T total = T.Zero; // TSelf の静的プロパティ Zero を呼び出し
    foreach (T n in numbers)
    {
        total += n;   // TSelf の静的 + 演算子を呼び出し
    }
    return total;
}
```

### 9-2. デフォルトインターフェースメソッド (DIM: C# 8)
Java の default メソッドと同様に、インターフェース内にメソッドの標準実装を持たせることができます。これにより、ライブラリ公開後にインターフェースへ新メソッドを追加しても、既存の実装クラスがコンパイルエラー（破壊的変更）になるのを防げます。

```csharp
public interface ILogger
{
    void Log(string message);
    // デフォルト実装
    void LogError(string error) => Log($"[ERROR] {error}");
}
```

### 9-3. 静的抽象ファクトリパターン (`IFactory<TSelf>`)
`static abstract` を利用すると、「インスタンスを作ることなく、型パラメータそのものからデフォルト値を生成する」ファクトリメソッドを型安全に強制できます。

```csharp
public interface IFactory<TSelf> where TSelf : IFactory<TSelf>
{
    static abstract TSelf CreateDefault();
}

public record AppConfig(...) : IFactory<AppConfig>
{
    public static AppConfig CreateDefault() => new("DefaultApplication", 30);
}

// 呼び出し側ヘルパー
T instance = T.CreateDefault();
```

### 9-4. 明示的インターフェース実装 (Explicit Implementation)
2 つのインターフェースが偶然同じシグネチャのメソッド（例: `void Read()`）を持っていた場合、`void IOrderReader.Read()` と宣言することで、呼び出し元のインターフェース型に応じて挙動を完全に分離できます。

### 9-5. ジェネリック型制約
- `where T : class` (参照型に限定)
- `where T : struct` (値型に限定)
- `where T : notnull` (null 非許容に限定)
- `where T : new()` (引数なしコンストラクタを持つ型に限定)

### 実行結果 (モジュール 06)
```text
=== 1. Generic Math via Static Abstract Members (C# 11) ===
Generic Math Sum (int):    100
Generic Math Sum (double): 7.5

=== 2. Default Interface Methods (DIM - C# 8) ===
ConsoleLogger: Normal notification
ConsoleLogger: [ERROR] Database connection failed!

=== 3. Static Abstract Members & Factory Pattern ===
Created default config: DefaultApplication (Timeout: 30s)

=== 4. Explicit Interface Implementation ===
  Reading Order document...
  Reading Invoice document...

=== 5. Generic Constraints (where T : class, new()) ===
Container created item with default host: localhost
```

---

## 10. モジュール 07: 高度な言語機能・イテレータ・拡張メソッド (`07_AdvancedLanguageFeatures.cs`)

### 10-1. `yield return` による遅延イテレータ
メソッド内で `yield return` を使用すると、コンパイラが自動的にステートマシンクラスを生成します。シーケンス全体をメモリに保持せず、要求された分だけオンデマンドで計算するため、**無限数列（フィボナッチ数列など）でもメモリを圧迫せずに表現**できます。

```csharp
private static IEnumerable<int> GenerateFibonacci()
{
    int a = 0, b = 1;
    while (true)
    {
        yield return a;
        int next = a + b;
        a = b;
        b = next;
    }
}

// 最初の 7 個だけをオンデマンドで取得
var fibonacci = GenerateFibonacci().Take(7);
```

### 10-2. 拡張メソッド (Extension Methods)
静的クラス内で第 1 引数に `this` を付与することで、既存の型（自作クラスだけでなく、`string` や `IEnumerable<T>` など）にインスタンスメソッドが生えているかのように新しいメソッドを追加できます。

```csharp
public static class CustomExtensions
{
    public static string ToTitleCase(this string s) { ... }
    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T> { ... }
}

// 呼び出し側
string title = "hello modern csharp".ToTitleCase();
bool ok = 42.IsBetween(10, 50);
```

### 10-3. 演算子オーバーロード & 暗黙の型変換 (`implicit operator`)
独自のドメイン値オブジェクト（DDD の Value Object）を作成する際、`operator +` や `implicit operator` を定義することで、自然な構文で型安全な演算が可能になります。

```csharp
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency) throw new InvalidOperationException();
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    // decimal から Money (JPY) への暗黙変換
    public static implicit operator Money(decimal amount) => new(amount, "JPY");
}

// 暗黙変換と演算子オーバーロードの動作
Money price1 = 1500m; // 暗黙変換
Money price2 = 2500m;
Money total = price1 + price2; // 4000 JPY
```

### 10-4. C# 12 任意の型のエイリアス
ファイルの先頭で `using エイリアス名 = 型;` と書くことで、タプルや配列などにプロジェクト固有の別名を付与できます。

```csharp
using Point2D = (int X, int Y);

Point2D origin = (0, 0);
Point2D target = (10, 20);
```

### 10-5. `in` / `ref readonly` 引数によるゼロコピー参照渡し
構造体（`struct`）は通常、引数に渡すと全データがメモリコピーされます。引数に `in` 修飾子を付与すると、**「コピーを発生させず参照渡し（ポインタ渡し）しつつ、メソッド内での書き換えをコンパイル時に禁止」** できます（C++ の `const T&` に相当）。

```csharp
private static void PrintMoney(in Money m)
{
    // m.Amount = 0; // ❌ in 引数のため変更不可 (コンパイルエラー)
    Console.WriteLine($"Money: {m.Amount:N0} {m.Currency}");
}
```

### 実行結果 (モジュール 07)
```text
=== 1. 'yield return' Lazy Generator (Iterators) ===
First 7 Fibonacci numbers: [0, 1, 1, 2, 3, 5, 8]

=== 2. Extension Methods (Adding methods to existing types) ===
Original: 'hello modern csharp' -> TitleCased: 'Hello Modern Csharp'
Is 42 between 10 and 50? True

=== 3. Operator Overloading & Implicit Conversion ===
Money addition: 1500 + 2500 = 4000 JPY

=== 4. C# 12 Type Aliases (using Point2D = (int X, int Y)) ===
Point Origin: (0, 0), Target: (10, 20)

=== 5. 'in' Parameter (Pass-by-reference without Copying) ===
  Money details (passed by reference): 999,999 JPY
```

---

## 11. プロジェクト設定 (`sample.csproj`) と関連ガイド

### 11-1. `sample.csproj` の解説

本プロジェクトのプロジェクト設定ファイルです。SDK スタイルの最小限かつ最新の構成を採用しています。

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- 実行可能バイナリを出力 -->
    <OutputType>Exe</OutputType>
    <!-- .NET 9.0 をターゲット -->
    <TargetFramework>net9.0</TargetFramework>
    <!-- System や System.Linq などの頻出名前空間を自動 using -->
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Null 許容参照型 (NRT) による静的 null 安全性の強制 -->
    <Nullable>enable</Nullable>
    <!-- 最新の言語バージョン (C# 12 / 13) を適用 -->
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

</Project>
```

### 11-2. さらなる深掘り用ガイド
本リポジトリには、特定の重要トピックについてさらに深く掘り下げた 2 つの専門ドキュメントが付属しています。

1. [**`LAMBDA.md` (ラムダ式・デリゲート・式ツリー完全理解ガイド)**](./LAMBDA.md)
   - C# 1.0 の名前付きデリゲートから C# 12 までの進化の歴史
   - クロージャが生成する裏クラス（`<>c__DisplayClass`）とヒープ確保のメモリプロファイリング
   - 静的ラムダによるアロケーション完全ゼロ化
   - 式ツリー（AST）の構造と Entity Framework Core が SQL に変換する仕組みの全貌
2. [**`CSPROJ_GUIDE.md` (.csproj 完全理解ガイド)**](./CSPROJ_GUIDE.md)
   - レガシー .NET Framework と現代の SDK スタイル .csproj の本質的な違い
   - 主要プロパティ完全リファレンス
   - NuGet 依存関係・マルチターゲット設定・Native AOT（単一バイナリ出力）の設定手法
