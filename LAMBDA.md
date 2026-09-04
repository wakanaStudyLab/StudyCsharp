# C# ラムダ式・デリゲート・式ツリー 完全理解ガイド (C# Lambdas, Delegates & Expression Trees Deep Dive)

C# 3.0 で導入され、LINQ の基盤となり、現代の C#（C# 10 / 12 / 13）に至るまで進化を続けてきた **C# の最重要機能「ラムダ式（Lambda Expressions）」** の完全解説書です。

Java や Python、Rust などのエンジニアが気になる「デリゲートとの違い」「裏で生成されるクロージャクラス（`<>c__DisplayClass`）と GC プレッシャー」「C# 9 静的ラムダによるゼロアロケーション」「**Entity Framework が C# のラムダ式から SQL を自動生成できる秘密（式ツリー）**」まで徹底解説します。

---

## 📑 目次

1. [ラムダ式の誕生背景とデリゲートの歴史 (C# 1.0 〜 C# 12)](#1-ラムダ式の誕生背景とデリゲートの歴史-c-10--c-12)
2. [ラムダ式の基本構文と省略ルール](#2-ラムダ式の基本構文と省略ルール)
3. [組み込み汎用デリゲート完全攻略 (`Func`, `Action`, `Predicate`)](#3-組み込み汎用デリゲート完全攻略-func-action-predicate)
4. [クロージャの内部実装：`<>c__DisplayClass` と GC プレッシャー ★最重要★](#4-クロージャの内部実装c__displayclass-と-gc-プレッシャー-最重要)
5. [ゼロアロケーションの極意：静的ラムダ (`static`) & ローカル関数](#5-ゼロアロケーションの極意静的ラムダ-static--ローカル関数)
6. [C# の独自最強機能：式ツリー (Expression Trees: `Expression<T>`)](#6-c-の独自最強機能式ツリー-expression-trees-expressiont)
7. [C# 10 / 11 / 12 の最新ラムダ進化](#7-c-10--11--12-の最新ラムダ進化)
8. [他言語エンジニア向け比較表 (C# vs Java vs Modern C++ vs Rust vs Go vs Python)](#8-他言語エンジニア向け比較表-c-vs-java-vs-modern-c-vs-rust-vs-go-vs-python)
9. [理解度チェッククイズ & よくある落とし穴](#9-理解度チェッククイズ--よくある落とし穴)

---

## 1. ラムダ式の誕生背景とデリゲートの歴史 (C# 1.0 〜 C# 12)

C# のラムダ式は、**「型安全な関数ポインタであるデリゲート（Delegate）」の進化の歴史**そのものです。

### 歴史年表
1. **C# 1.0 (2002)**: **名前付きデリゲート**  
   わざわざ `delegate void MyHandler(int x);` を宣言し、`new MyHandler(NamedMethod)` とメソッド名を渡す必要があった。
2. **C# 2.0 (2005)**: **匿名メソッド (Anonymous Methods)**  
   インラインで関数を書けるようになったが、`delegate(int x) { return x * 2; }` と記述が長かった。
3. **C# 3.0 (2007)**: **ラムダ式 (`=>`) & LINQ**  
   矢印記号 `=>`（Goes to 演算子）により、型推論を備えた簡潔な `x => x * 2` が登場。
4. **C# 9.0 (2020)**: **静的ラムダ (`static (x) => ...`)**  
   意図せぬ外部変数のキャプチャ（メモリ割り当て）をコンパイル時に防止。
5. **C# 10.0 (2021)**: **自然な型推論 (Natural Types)**  
   `var f = (int x) => x * 2;` のように `var` でラムダを受け取れるようになった。

```csharp
// C# 1.0: メソッドを別途宣言
public int DoubleMethod(int x) => x * 2;
Func<int, int> d1 = DoubleMethod;

// C# 2.0: 匿名メソッド
Func<int, int> d2 = delegate(int x) { return x * 2; };

// C# 3.0: ラムダ式 (現代の標準)
Func<int, int> d3 = x => x * 2;
```

---

## 2. ラムダ式の基本構文と省略ルール

ラムダ式には **「式形式 (Expression Lambda)」** と **「ステートメント形式 (Statement Lambda)」** の2種類があります。

### 2-1. 式形式 vs ステートメント形式
```csharp
// 式形式 (Expression Lambda): => の右側が単一の式 (return不要)
Func<int, int, int> add = (a, b) => a + b;

// ステートメント形式 (Statement Lambda): => の右側に {} ブロックを持つ (return必須)
Func<int, int, int> addBlock = (a, b) =>
{
    Console.WriteLine($"Adding {a} and {b}");
    return a + b;
};
```

### 2-2. 省略ルール早見表
| パターン | 完全な記法 | 省略記法 | 省略可能な条件 |
| :--- | :--- | :--- | :--- |
| **単一引数** | `(x) => x * 2` | `x => x * 2` | **引数が1つ**のときのみカッコ `()` を省略可能 |
| **型指定** | `(int x, int y) => x + y` | `(x, y) => x + y` | コンパイラが型推論できる場合 |
| **引数なし** | `() => 42` | `() => 42` | **引数がない場合は `()` の省略不可** |
| **破棄引数 (Discard)** | `(_, _) => "OK"` | `(_, _) => "OK"` | 使わない引数はアンダースコア `_` で破棄可能 (C# 9+) |

---

## 3. 組み込み汎用デリゲート完全攻略 (`Func`, `Action`, `Predicate`)

独自に `delegate` を宣言しなくても、.NET 標準のジェネリックデリゲートで99%カバーできます。

```
          引数あり                     引数なし
       ┌──────────────┐             ┌──────────────┐
戻り値 │ Func<T, R>   │             │ Func<R>      │
あり   │ (T -> R 変換) │             │ (() -> R)    │
       └──────────────┘             └──────────────┘
       ┌──────────────┐             ┌──────────────┐
戻り値 │ Action<T>    │             │ Action       │
なし   │ (T -> void)  │             │ (() -> void) │
       └──────────────┘             └──────────────┘
       ┌──────────────┐
真偽値 │ Predicate<T> │ (Func<T, bool> と実質同等)
       │(T -> bool)   │
       └──────────────┘
```

```csharp
// 1. Func<T1, T2, ..., TResult>: 最後の型パラメータが戻り値
Func<string, int> getLength = s => s.Length;

// 2. Action<T1, T2, ...>: 戻り値 void (副作用)
Action<string> printLog = msg => Console.WriteLine($"[LOG] {msg}");

// 3. Predicate<T>: bool を返す (List<T>.FindAll などで使用)
Predicate<int> isEven = n => n % 2 == 0;
```

---

## 4. クロージャの内部実装：`<>c__DisplayClass` と GC プレッシャー ★最重要★

ラムダ式が外部のローカル変数を参照（キャプチャ）したとき、コンパイラは何をしているのでしょうか？

### 4-1. コンパイラが裏で生成する隠しクラス
```csharp
// 私たちが書いたコード:
public void Process()
{
    int multiplier = 5;
    Func<int, int> scale = x => x * multiplier;
    Console.WriteLine(scale(10));
}
```

コンパイル時、Roslyn コンパイラは裏で**以下のような隠しクラス（DisplayClass）を自動生成**しています：

```csharp
// コンパイラが裏で生成する C# IL 等価コード:
[CompilerGenerated]
private sealed class <>c__DisplayClass0_0
{
    public int multiplier; // キャプチャされた変数がヒープ上のフィールドになる！

    public int <Process>b__0(int x)
    {
        return x * this.multiplier;
    }
}

public void Process()
{
    // ヒープ上に新しいオブジェクトが new される！ (アロケーション発生)
    var display = new <>c__DisplayClass0_0();
    display.multiplier = 5;
    
    Func<int, int> scale = new Func<int, int>(display.<Process>b__0);
    Console.WriteLine(scale(10));
}
```

### 4-2. キャプチャによる代償（GC プレッシャー）
- 変数をキャプチャすると、**呼び出しのたびにヒープ上にクラスインスタンスが `new`** されます。
- 高頻度に呼ばれるループや Web API のホットパスで不用意にキャプチャを行うと、**ガベージコレクション（GC Gen 0）が頻発してスループットが低下**します。

### 4-3. ループ変数キャプチャの罠（C# の歴史）
- **`foreach` ループ**: C# 5.0 で仕様変更され、イテレーションごとに新しい変数がキャプチャされるよう安全になりました。
- **古典的 `for (int i = 0; ...)` ループ**: **今でも古いキャプチャ仕様のまま**です！

```csharp
var actions = new List<Action>();

// ❌ 古典的 for ループでは全ラムダが同じ変数 i の最終値 (3) を参照してしまう！
for (int i = 0; i < 3; i++)
{
    actions.Add(() => Console.WriteLine(i));
}
actions.ForEach(a => a()); // 出力: 3, 3, 3

// ⭕ 回避策: ループ内でローカル変数にコピーする
for (int i = 0; i < 3; i++)
{
    int copy = i;
    actions.Add(() => Console.WriteLine(copy));
}
```

---

## 5. ゼロアロケーションの極意：静的ラムダ (`static`) & ローカル関数

### 5-1. C# 9 静的ラムダ (`static (args) => ...`)
ラムダ式の前に `static` を付けると、**外部変数のキャプチャを明示的に禁止**できます。

```csharp
int threshold = 50;

// ❌ コンパイルエラー: 静的ラムダ内からローカル変数 threshold は参照できない
Func<int, bool> check = static x => x > threshold;

// ⭕ キャプチャなしであることが保証され、不要なヒープ割り当てがゼロになる
Func<int, int, int> maxFunc = static (a, b) => Math.Max(a, b);
```

### 5-2. ローカル関数 (`Local Functions`) vs ラムダ式
C# 7 で導入されたローカル関数は、メソッドの内部に直接関数を定義する機能です。

```csharp
public void DoWork()
{
    // ラムダ式 (デリゲートインスタンスのヒープ確保が発生)
    Func<int, int> lambdaSquare = x => x * x;

    // ローカル関数 (コンパイラが通常の静的メソッドに変換し、デリゲート確保ゼロ！)
    static int LocalSquare(int x) => x * x;
}
```

> **💡 ベストプラクティス**:  
> LINQ 引数など他のメソッドに渡す場合はラムダ式を使い、**そのメソッド内だけで再帰呼び出しやヘルパーとして使う場合は「ローカル関数」**を使うのが最速です。

---

## 6. C# の独自最強機能：式ツリー (Expression Trees: `Expression<T>`)

C# のラムダ式が他のすべての言語（Java, Rust, Python, Go）と決定的に異なる最強の武器が **「式ツリー (Expression Trees)」** です。

### 6-1. 「コード」が「データ（構文木）」になる
同じラムダ式 `x => x >= 18` でも、代入先の型によって**生成されるものが全く異なります**。

```csharp
// 1. Func<int, bool> に代入した場合:
// -> 実行可能なマシン語 (IL バイトコード) が生成される
Func<int, bool> func = x => x >= 18;
bool isAdult = func(20); // 実行

// 2. Expression<Func<int, bool>> に代入した場合:
// -> コードではなく「構文木（AST: Abstract Syntax Tree）」オブジェクトが生成される！
Expression<Func<int, bool>> expr = x => x >= 18;
```

### 6-2. なぜ Entity Framework Core は C# から SQL を生成できるのか？
```csharp
// 私たちが書いた C# コード:
var adults = dbContext.Users
    .Where(u => u.Age >= 18) // この引数は Expression<Func<User, bool>>
    .ToList();
```

1. `Where` メソッドはラムダ式をコンパイルされたコードではなく、**`Expression` オブジェクト（式ツリー）** として受け取ります。
2. EF Core のクエリプロバイダが式ツリーを走査し、
   - 左辺: `u.Age` → 列名 `Age`
   - 演算子: `>=` → SQL の `>=`
   - 右辺: `18` → パラメータ `@p0 = 18`
3. これらを組み合わせて、**`SELECT * FROM Users WHERE Age >= 18` という SQL 文字列を自動生成**してデータベースに送信します。

> **🚀 結論**: 式ツリーがあるからこそ、C# は完全な型安全性を保ったままデータベースクエリを美しく記述できます。

---

## 7. C# 10 / 11 / 12 の最新ラムダ進化

### 7-1. 自然な型推論 (Natural Types: C# 10)
C# 10 から、型を `Func<...>` と明記しなくても `var` でラムダを変数に代入できるようになりました。

```csharp
// C# 9 以前: 型の明記が必須
Func<string, int> parse = (string s) => int.Parse(s);

// C# 10 以降: var で代入可能
var parse = (string s) => int.Parse(s);
```

### 7-2. 明示的な戻り値の指定 (C# 10)
コンパイラが推論できない場合や、`object` や基底型として返したい場合に先頭に戻り値型を指定できます。

```csharp
// 戻り値型を string? と明示
var choose = string? (bool flag) => flag ? "Hello" : null;
```

---

## 8. 他言語エンジニア向け比較表 (C# vs Java vs Modern C++ vs Rust vs Go vs Python)

| 項目 | C# (12+) | Java (21+) | Modern C++ (20+) | Rust | Go | Python |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **記法** | `(x) => x * 2` | `(x) -> x * 2` | `[](auto x){ return x*2; }` | `\|x\| x * 2` | `func(x int) int` | `lambda x: x*2` |
| **型モデル** | デリゲート / `Expression` | SAM インターフェース | 無名ファンクタ構造体 | `Fn` トレイト | 関数型 | `function` |
| **式ツリー (AST)** | ⭕ **標準完全サポート** | ❌ なし | ❌ なし | ❌ なし | ❌ なし | ❌ なし |
| **外部変数の書き換え**| ⭕ 可能 (`DisplayClass`) | ❌ 不可 (final限定) | ⭕ `[&]` や `mutable` | ⭕ `mut` 借用 | ⭕ 可能 (参照) | ⚠️ `nonlocal` |
| **静的ラムダ** | ⭕ `static (x) =>` | ❌ なし | ⭕ `[]` (キャプチャなし) | ⭕ キャプチャなし | ❌ なし | ❌ なし |
| **複数行・文** | ⭕ サポート | ⭕ サポート | ⭕ サポート | ⭕ サポート | ⭕ サポート | ❌ **式のみ** |

---

## 9. 理解度チェッククイズ & よくある落とし穴

### Q1. 次のコードのうち、ヒープアロケーション（GC 負荷）が最も少ないのはどれですか？
- A. `int factor = 2; Func<int, int> f = x => x * factor;`
- B. `Func<int, int> f = static x => x * 2;`
- C. `static int Square(int x) => x * x;` (ローカル関数)
- D. `Expression<Func<int, int>> expr = x => x * 2;`

<details>
<summary>▶ 解答と解説</summary>

**正解: C (および B)**
- C のローカル関数はデリゲートオブジェクトすら生成せず、通常のスタック呼び出しとしてインライン展開されるためアロケーションが完全にゼロです。
- B の静的ラムダもキャプチャクラスは生成されませんが、初回にデリゲートインスタンスがキャッシュされます。
- A は DisplayClass が `new` され、D は式ツリーの AST ノードがヒープ上に大量生成されます。
</details>

### Q2. `Expression<Func<int, bool>>` と `Func<int, bool>` の決定的な違いは何ですか？
<details>
<summary>▶ 解答と解説</summary>

**解答**: `Func<int, bool>` は CPU が直接実行できるコンパイル済み IL バイトコード（実行コード）です。一方、`Expression<Func<int, bool>>` はコードの構文構造（パラメータ名、演算子、リテラル値）を表現した「データ構造（抽象構文木: AST）」であり、プログラム実行時にクエリ文字列（SQL等）への変換や動的解析が可能です。
</details>

---

## まとめ

1. **「ラムダの正体はデリゲートと DisplayClass」**: キャプチャするとヒープ割り当てが発生することを意識する。
2. **「アロケーションを嫌うなら `static` ラムダとローカル関数」**: C# 9 の静的ラムダを活用してクリーンな設計を保つ。
3. **「C# の真骨頂は式ツリー (Expression Trees)」**: LINQ や ORM を支えるメタプログラミングの美しさを理解する。
