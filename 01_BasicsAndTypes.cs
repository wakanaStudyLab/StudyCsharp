namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 01: 基本型・レコード・モダン構文 (Basics, Records & Modern Syntax)
/// ============================================================================
/// 
/// 【他言語経験者（Rust, Go, Java, Python）向け要点】
/// 1. record class vs record struct:
///    - C# 9+ の record は「値ベースの等価性 (Value Equality)」を持つ不変参照型。
///    - C# 10+ の record struct はスタック上に確保される値型のレコード。
///    - `with` 式による非破壊的コピー (Non-destructive mutation) が強力。
/// 
/// 2. プライマリコンストラクタ (C# 12):
///    - クラスや構造体の宣言部に直接引数を定義し、ボイラープレートなフィールド初期化を排除。
/// 
/// 3. required プロパティ & init 専用セッター (C# 11):
///    - オブジェクト初期化子で必須のプロパティをコンパイル時に強制。
/// 
/// 4. Raw String Literals (C# 11):
///    - `"""` によるエスケープ不要な複数行生文字列（JSONやSQLに最適）。
/// 
/// 5. インデックス `^` と 範囲 `..` (C# 8+):
///    - Python の負数インデックスやスライスに相当する安全な記法。
/// </summary>
public static class BasicsAndTypes
{
    // 不変参照型 (レコードクラス: プリミティブ・イミュータブルプロパティ)
    public record Person(string Id, string Name, int Age);

    // コレクションを含むレコードクラス (配列やリストは参照比較されるという C# の罠！)
    public record UserWithRoles(string Id, string Name, string[] Roles);

    // 不変値型 (レコード構造体: ヒープ確保なし)
    public readonly record struct GeoPoint(double Latitude, double Longitude);

    // C# 12 プライマリコンストラクタを持つ通常のクラス
    public class ServiceEndpoint(string host, int port)
    {
        public string Url => $"https://{host}:{port}";
    }

    // C# 11 required プロパティと init セッター
    public class OrderRequest
    {
        public required string OrderId { get; init; }
        public required decimal TotalAmount { get; init; }
        public string Note { get; init; } = string.Empty;
    }

    // パターンマッチング用のドメインモデル
    public abstract record PaymentMethod;
    public record CreditCard(string CardNumber, string HolderName, decimal Balance) : PaymentMethod;
    public record CryptoTransfer(string WalletAddress, string Network) : PaymentMethod;
    public record BankAccount(string Iban, string BankName) : PaymentMethod;

    public static void Run()
    {
        DemoRecordsAndValueEquality();
        DemoPrimaryConstructorsAndRequiredProperties();
        DemoPatternMatching();
        DemoListPatternsAndRanges();
        DemoRawStringLiterals();
        DemoNullableReferenceTypes();
    }

    private static void DemoRecordsAndValueEquality()
    {
        Console.WriteLine("=== 1. Records & Value-based Equality ===");

        var p1 = new Person("p101", "Alice", 28);
        var p2 = new Person("p101", "Alice", 28);

        // 1. 純粋な record の値ベース比較 (True)
        Console.WriteLine($"p1 == p2 (Record Value Equality): {p1 == p2}"); // True
        Console.WriteLine($"ReferenceEquals(p1, p2):          {ReferenceEquals(p1, p2)}"); // False

        // 'with' 式による非破壊的更新 (一部のプロパティだけ変えて新しいオブジェクトを生成)
        var p3 = p1 with { Age = 29 };
        Console.WriteLine($"Original p1 Age: {p1.Age}, Cloned p3 Age: {p3.Age}");

        // 2. 【C#の罠】コレクションを含む record の等価性
        var u1 = new UserWithRoles("u101", "Alice", ["admin", "dev"]);
        var u2 = new UserWithRoles("u101", "Alice", ["admin", "dev"]);
        // 配列の Equals は参照比較になるため、False になる！
        Console.WriteLine($"u1 == u2 (Collection property trap): {u1 == u2} (Array uses ReferenceEquals!)");

        // レコード構造体 (スタック確保)
        var point = new GeoPoint(35.6812, 139.7671);
        Console.WriteLine($"GeoPoint (Record Struct): {point.Latitude}, {point.Longitude}");
    }

    private static void DemoPrimaryConstructorsAndRequiredProperties()
    {
        Console.WriteLine("\n=== 2. Primary Constructors (C# 12) & Required Properties (C# 11) ===");

        // プライマリコンストラクタによるクラス初期化
        var endpoint = new ServiceEndpoint("api.example.com", 443);
        Console.WriteLine($"Endpoint URL: {endpoint.Url}");

        // required プロパティの初期化 (OrderId と TotalAmount の指定がコンパイル時に必須)
        var order = new OrderRequest
        {
            OrderId = "ORD-2026-999",
            TotalAmount = 4500m
        };
        Console.WriteLine($"Order: {order.OrderId}, Amount: {order.TotalAmount:C}");
    }

    private static void DemoPatternMatching()
    {
        Console.WriteLine("\n=== 3. Modern Pattern Matching (Switch Expressions) ===");

        PaymentMethod payment = new CreditCard("1234-5678-9012-3456", "Alice", 15000m);

        // switch 式とプロパティパターン
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
        Console.WriteLine($"Payment Status: {result}");

        // 関係パターン (Relational Patterns)
        int temperature = 25;
        string weather = temperature switch
        {
            < 0 => "Freezing",
            >= 0 and < 15 => "Cold",
            >= 15 and < 28 => "Pleasant",
            _ => "Hot"
        };
        Console.WriteLine($"Temperature {temperature}C is: {weather}");
    }

    private static void DemoListPatternsAndRanges()
    {
        Console.WriteLine("\n=== 4. Index (^), Range (..), and List Patterns ===");

        string[] words = ["zero", "one", "two", "three", "four", "five"];

        // インデックス ^ (末尾からの指定: ^1 は最後の要素)
        Console.WriteLine($"First: {words[0]}, Last (^1): {words[^1]}, Second to last (^2): {words[^2]}");

        // 範囲演算子 .. (スライシング)
        string[] slice = words[1..^1]; // "one" から "four" まで
        Console.WriteLine($"Slice [1..^1]: [{string.Join(", ", slice)}]");

        // C# 11 リストパターンによる分解
        int[] numbers = [10, 20, 30, 40, 50];
        string analysis = numbers switch
        {
            [var first, .., var last] => $"Starts with {first} and ends with {last}",
            [var single] => $"Single element: {single}",
            [] => "Empty list",
            _ => "Other pattern"
        };
        Console.WriteLine($"List pattern analysis: {analysis}");
    }

    private static void DemoRawStringLiterals()
    {
        Console.WriteLine("\n=== 5. Raw String Literals (C# 11: \"\"\" ... \"\"\") ===");

        string author = "Harun";
        int year = 2026;

        // エスケープ不要な生文字列リテラル ($$""" により {author} 等の補間と JSON の中括弧 {} を分離)
        string json = $$"""
        {
            "title": "Modern C# Guide",
            "author": "{{author}}",
            "year": {{year}},
            "features": ["Records", "Pattern Matching", "Raw Strings"]
        }
        """;
        Console.WriteLine(json);
    }

    private static void DemoNullableReferenceTypes()
    {
        Console.WriteLine("\n=== 6. Nullable Reference Types (NRT) ===");

        string? nullableName = null;
        // null 合体代入演算子 ??=
        nullableName ??= "Default Guest";
        Console.WriteLine($"Resolved Name: {nullableName}");

        // null 条件演算子 ?. と null 合体演算子 ??
        string? email = null;
        string displayEmail = email?.ToUpperInvariant() ?? "no-email@domain.com";
        Console.WriteLine($"Display Email: {displayEmail}");
    }
}
