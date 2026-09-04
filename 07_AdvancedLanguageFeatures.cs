// C# 12 任意の型のエイリアス (using alias for any type)
using Point2D = (int X, int Y);

namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 07: 高度な言語機能 & イテレータ (Advanced Features & Iterators)
/// ============================================================================
/// 
/// 【他言語経験者向け要点】
/// 1. yield return による遅延ジェネレータ:
///    - Pythonの yield と同等。状態機械 (State Machine) が自動生成され、
///      要素が要求された時だけオンデマンドで生成される。
/// 
/// 2. 拡張メソッド (Extension Methods):
///    - 既存の型（string や IEnumerable など）に新しいメソッドを生やす Rustのtrait impl相当の構文。
/// 
/// 3. 演算子オーバーロード & ユーザー定義型変換:
///    - `operator +` や `implicit` / `explicit` 演算子によるドメインプリミティブの設計。
/// 
/// 4. in / ref readonly パラメータ:
///    - 巨大な struct をコピーなし（参照渡し）かつ不変（読み取り専用）で渡す C++ の const T& 相当の機能。
/// </summary>
public static class AdvancedLanguageFeatures
{
    // ドメイン値オブジェクト (通貨)
    public readonly record struct Money(decimal Amount, string Currency)
    {
        // 演算子オーバーロード +
        public static Money operator +(Money left, Money right)
        {
            if (left.Currency != right.Currency)
            {
                throw new InvalidOperationException($"Cannot add {left.Currency} and {right.Currency}");
            }
            return new Money(left.Amount + right.Amount, left.Currency);
        }

        // 暗黙の型変換 (decimal -> Money)
        public static implicit operator Money(decimal amount) => new(amount, "JPY");
    }

    public static void Run()
    {
        DemoYieldIterators();
        DemoExtensionMethods();
        DemoOperatorOverloadingAndConversions();
        DemoTypeAliasAndTuples();
        DemoInAndRefReadonly();
    }

    private static void DemoYieldIterators()
    {
        Console.WriteLine("=== 1. 'yield return' Lazy Generator (Iterators) ===");

        // フィボナッチ数列を遅延評価で生成
        var fibonacci = GenerateFibonacci().Take(7);
        Console.WriteLine($"First 7 Fibonacci numbers: [{string.Join(", ", fibonacci)}]");
    }

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

    private static void DemoExtensionMethods()
    {
        Console.WriteLine("\n=== 2. Extension Methods (Adding methods to existing types) ===");

        string rawText = "hello modern csharp";
        // 既存の string 型に対して拡張メソッド ToTitleCase を呼び出し
        string titleCased = rawText.ToTitleCase();
        Console.WriteLine($"Original: '{rawText}' -> TitleCased: '{titleCased}'");

        int val = 42;
        Console.WriteLine($"Is {val} between 10 and 50? {val.IsBetween(10, 50)}");
    }

    private static void DemoOperatorOverloadingAndConversions()
    {
        Console.WriteLine("\n=== 3. Operator Overloading & Implicit Conversion ===");

        // 暗黙の型変換 (decimal -> Money)
        Money price1 = 1500m;
        Money price2 = 2500m;

        // + 演算子オーバーロード
        Money total = price1 + price2;
        Console.WriteLine($"Money addition: {price1.Amount} + {price2.Amount} = {total.Amount} {total.Currency}");
    }

    private static void DemoTypeAliasAndTuples()
    {
        Console.WriteLine("\n=== 4. C# 12 Type Aliases (using Point2D = (int X, int Y)) ===");

        Point2D origin = (0, 0);
        Point2D target = (10, 20);

        Console.WriteLine($"Point Origin: ({origin.X}, {origin.Y}), Target: ({target.X}, {target.Y})");
    }

    private static void DemoInAndRefReadonly()
    {
        Console.WriteLine("\n=== 5. 'in' Parameter (Pass-by-reference without Copying) ===");

        var hugeStruct = new Money(999999m, "JPY");
        // in 修飾子により、コピーを発生させずに参照渡し（読み取り専用）
        PrintMoney(in hugeStruct);
    }

    private static void PrintMoney(in Money m)
    {
        // m.Amount = 0; // ❌ in 引数は読み取り専用のため変更不可 (コンパイルエラー)
        Console.WriteLine($"  Money details (passed by reference): {m.Amount:N0} {m.Currency}");
    }
}

// 拡張メソッドの定義 (static class 内に this 引数を定義)
public static class CustomExtensions
{
    public static string ToTitleCase(this string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var words = s.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
            }
        }
        return string.Join(' ', words);
    }

    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }
}
