# Modern C# Crash Course (For Rust, Go, Python, Java Developers)

Rust, Go, Python, Java などの静的・動的型付け言語を習得済みのエンジニアが、**最短でモダン C#（C# 12 / 13 on .NET 8 / 9）をマスターするための実践リファレンス**です。

---

## 🚀 クイックスタート (実行方法)

```powershell
cd C:\Users\harun\programming\c#\sample

# ビルド & 実行 (全モジュールが一括実行されます)
dotnet run

# リリースビルド
dotnet build -c Release
```

---

## 🗺️ 言語対比マッピング早見表 (C# vs Rust vs Go vs Java vs Python)

| 概念・機能 | Modern C# (12+) | Rust | Go | Java (21+) | Python (3.10+) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **ローカル型推論** | `var x = 10;` | `let x = 10;` | `x := 10` | `var x = 10;` | `x = 10` |
| **不変データ構造** | `record User(...)` | `struct User` | `type User struct` | `record User(...)` | `@dataclass(frozen=True)` |
| **スタック値型** | `struct` / `record struct`| `struct` (値型) | `struct` (値型) | ❌ (全てヒープ参照) | ❌ (全てヒープ参照) |
| **ゼロコピー切り出し** | `Span<T>` / `ReadOnlySpan<T>` | `&[T]` / `&str` | `slice [a:b]` | ❌ (コピー発生) | `memoryview` / slice |
| **値の同値性比較** | `==` (オーバーロード可) | `==` (`PartialEq`) | `==` | `a.equals(b)` | `==` (`__eq__`) |
| **参照の一致比較** | `ReferenceEquals(a, b)` | `std::ptr::eq` | ポインタ比較 | `a == b` | `a is b` |
| **パターンマッチング** | switch 式 / プロパティ / リスト | `match` 式 | type switch | switch パターン | `match ... case` |
| **コレクション操作** | **LINQ** (`Where`, `Select`) | `.iter().filter().map()` | slices / ループ | Stream API | 内包表記 / `filter` |
| **Null 安全性** | `string?` (NRT) | `Option<T>` | `*string` / `nil` | `Optional<T>` | `str \| None` |
| **リソース自動解放** | `using var r = ...` | `Drop` トレイト | `defer r.Close()` | `try (var r = ...)` | `with r:` |
| **非同期 Promise** | `Task<T>` / `ValueTask<T>` | `Future<T>` (tokio) | goroutine + channel | `CompletableFuture` | `asyncio.Task` |
| **並行パイプライン** | `System.Threading.Channels` | `tokio::sync::mpsc` | **channel** (`chan T`) | `BlockingQueue` | `asyncio.Queue` |

---

## ⚠️ 他言語経験者が最もハマる C# の「罠」と作法

### 1. `record` のコレクションプロパティ等価性の罠
- `record` は全プロパティの `Equals()` を呼び出して値同値性を判定します。
- しかし、**`string[]` や `List<T>` などのコレクションの `Equals` は「参照比較（同一インスタンスか）」** になります。
- そのため、要素が全く同じでも `u1 == u2` が `false` になります。
- **対策**: コレクションを含む record の等価性を保ちたい場合は、カスタム `Equals` を書くか `StructuralComparisons.StructuralEqualityComparer` を使います。

### 2. `async void` は絶対に書いてはいけない（`async Task` を使う）
- `async void` はトップレベルの UI イベントハンドラ（ボタンクリック等）専用の例外的な構文です。
- 通常のメソッドで `async void` を使うと、**内部で発生した例外が `try-catch` で捕捉できず、プロセス全体が即座にクラッシュ**します。
- **原則**: 戻り値のない非同期メソッドは必ず `async Task` を返してください。

### 3. `IEnumerable<T>` の二重評価（Multiple Enumeration）
- LINQ のメソッド（`Where`, `Select` 等）は**遅延評価（Deferred Execution）**です。
- `IEnumerable<T> query = ...;` を `foreach` で2回回したり、`query.Any()` の後に `query.Count()` を呼ぶと、**背後のフィルタ処理や DB クエリが2回実行**されます。
- **対策**: 複数回参照する場合は、必ず `.ToList()` や `.ToArray()` でメモリ上に具体化（Materialize）してください。

### 4. `Span<T>` の制約（`ref struct` の生存期間ルール）
- `Span<T>` はスタック領域にのみ配置可能な `ref struct` です。
- そのため、**クラスのフィールドに持たせること、`async` メソッドをまたぐこと、ヒープに退避（ボックス化）することはコンパイルエラー**で禁止されています。
- 非同期処理をまたいでメモリを参照したい場合は、`Memory<T>` / `ReadOnlyMemory<T>` を使用します。

---

## 📁 提供サンプルコードの解説

| ファイル | テーマ | 主な学習内容 |
| :--- | :--- | :--- |
| [`01_BasicsAndTypes.cs`](./01_BasicsAndTypes.cs) | **基本型・レコード・モダン構文** | `record class`, `record struct`, プライマリコンストラクタ (C# 12), `required` / `init` (C# 11), Raw String (`"""`), Index `^` & Range `..`, リストパターン, NRT |
| [`02_CollectionsAndLinq.cs`](./02_CollectionsAndLinq.cs) | **コレクション・LINQ・Span** | LINQ パイプライン (`Where`, `Select`, `GroupBy`), C# 12 コレクション式 & スプレッド (`..`), `Span<T>` / `stackalloc` (ゼロアロケーション), .NET 8 `FrozenDictionary` |
| [`03_AsyncAndConcurrency.cs`](./03_AsyncAndConcurrency.cs) | **非同期ストリーム & 並行処理** | `async / await`, `IAsyncEnumerable<T>` + `await foreach` (非同期ストリーミング), CancellationToken, Channels, .NET 9 新型 `System.Threading.Lock` (C# 13) |
| [`04_ExceptionAndResource.cs`](./04_ExceptionAndResource.cs) | **リソース管理 & 例外ガード** | `using` 宣言 (`IDisposable`), `await using` (`IAsyncDisposable`), パターンマッチング例外フィルター (`when`), `CallerArgumentExpression` ガード節 |
| [`05_LambdasAndDelegates.cs`](./05_LambdasAndDelegates.cs) | **ラムダ式・式ツリー** | `Func` / `Action`, 静的ラムダ (`static (x) => ...`), ローカル関数, 式ツリー (`Expression<Func<T, bool>>`) の構文木解析 |
| [`06_GenericsAndInterfaces.cs`](./06_GenericsAndInterfaces.cs) | **ジェネリクス & インターフェース** | 静的抽象メンバ (C# 11) と Generic Math (`INumber<T>`), デフォルトインターフェースメソッド (DIM), 明示的インターフェース実装, 型制約 (`where T : ...`) |
| [`07_AdvancedLanguageFeatures.cs`](./07_AdvancedLanguageFeatures.cs) | **高度な言語機能 & イテレータ** | `yield return` 遅延ジェネレータ, 拡張メソッド (`this T`), 演算子オーバーロード & `implicit` 変換, C# 12 型エイリアス, `in` パラメータ |
| [`Program.cs`](./Program.cs) | **統合エントリーポイント** | 上記全モジュールを順番にバナー付きで実行するメインランナー |

> 📖 **C# ラムダ式・デリゲート・式ツリーの完全理解ガイド**:  
> デリゲートの歴史（C# 1.0〜12）、裏で生成される `<>c__DisplayClass` のヒープ割り当て、静的ラムダによる GC ゼロ最適化、Entity Framework の SQL 生成を支える式ツリーの正体まで完全網羅した解説は [**`LAMBDA.md`**](./LAMBDA.md) を参照してください。

> 🛠️ **Modern C# .csproj 完全理解ガイド**:  
> SDK スタイル .csproj の記法、`<Nullable>` / `<ImplicitUsings>`、NuGet パッケージ管理、ファイルコピー設定、マルチターゲット、Native AOT（単一バイナリ出力）まで完全網羅した解説は [**`CSPROJ_GUIDE.md`**](./CSPROJ_GUIDE.md) を参照してください。
