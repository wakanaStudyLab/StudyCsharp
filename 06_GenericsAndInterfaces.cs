using System.Numerics;

namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 06: ジェネリクス・インターフェース・モダン型システム (Generics & Interfaces)
/// ============================================================================
/// 
/// 【他言語経験者向け要点】
/// 1. 静的抽象メンバ (Static Abstract Members: C# 11) & Generic Math:
///    - インターフェース内に `static abstract` メソッドや演算子を定義可能。
///    - `INumber<T>` や `IAdditionOperators<T, T, T>` により、int や double など
///      異なる数値型に対して同一のジェネリック計算関数を記述可能（Rustのトレイト制約に匹敵）。
/// 
/// 2. デフォルトインターフェースメソッド (DIM: C# 8):
///    - Java の default メソッドと同様に、既存インターフェースを壊さずにメソッド追加が可能。
/// 
/// 3. ジェネリック型制約 (where T : ...):
///    - `where T : class`, `where T : struct`, `where T : notnull`, `where T : new()`
/// 
/// 4. 明示的インターフェース実装 (Explicit Implementation):
///    - 同名のメソッドを持つ複数のインターフェースを実装する際の衝突回避。
/// </summary>
public static class GenericsAndInterfaces
{
    // 1. デフォルトインターフェースメソッド (DIM)
    public interface ILogger
    {
        void Log(string message);

        // デフォルト実装: 実装クラス側でオーバーライドしなくても利用可能
        void LogError(string error) => Log($"[ERROR] {error}");
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine($"ConsoleLogger: {message}");
    }

    // 2. 静的抽象メンバを持つカスタムインターフェース
    public interface IFactory<TSelf> where TSelf : IFactory<TSelf>
    {
        // 静的抽象ファクトリメソッド (実装クラス側で static メソッドとして実装を強制)
        static abstract TSelf CreateDefault();
    }

    public record AppConfig(string AppName, int TimeoutSeconds) : IFactory<AppConfig>
    {
        public static AppConfig CreateDefault() => new("DefaultApplication", 30);
    }

    // 3. 明示的インターフェース実装
    public interface IOrderReader
    {
        void Read();
    }

    public interface IInvoiceReader
    {
        void Read();
    }

    public class DocumentProcessor : IOrderReader, IInvoiceReader
    {
        // IOrderReader 経由でのみ呼び出せる
        void IOrderReader.Read() => Console.WriteLine("  Reading Order document...");

        // IInvoiceReader 経由でのみ呼び出せる
        void IInvoiceReader.Read() => Console.WriteLine("  Reading Invoice document...");
    }

    public static void Run()
    {
        DemoGenericMath();
        DemoDefaultInterfaceMethods();
        DemoStaticAbstractFactory();
        DemoExplicitInterfaceImplementation();
        DemoGenericConstraints();
    }

    // C# 11 汎用数値演算 (Generic Math: INumber<T>)
    private static void DemoGenericMath()
    {
        Console.WriteLine("=== 1. Generic Math via Static Abstract Members (C# 11) ===");

        // int でも double でも同じ sum 関数が動作する！
        int sumInt = SumAll([10, 20, 30, 40]);
        double sumDouble = SumAll([1.5, 2.5, 3.5]);

        Console.WriteLine($"Generic Math Sum (int):    {sumInt}");
        Console.WriteLine($"Generic Math Sum (double): {sumDouble}");
    }

    // INumber<T> を制約に持つ汎用集計メソッド
    private static T SumAll<T>(ReadOnlySpan<T> numbers) where T : INumber<T>
    {
        T total = T.Zero; // TSelf の静的プロパティ Zero を呼び出し
        foreach (T n in numbers)
        {
            total += n;   // TSelf の静的 + 演算子を呼び出し
        }
        return total;
    }

    private static void DemoDefaultInterfaceMethods()
    {
        Console.WriteLine("\n=== 2. Default Interface Methods (DIM - C# 8) ===");

        ILogger logger = new ConsoleLogger();
        logger.Log("Normal notification");
        // デフォルト実装の LogError を呼び出す
        logger.LogError("Database connection failed!");
    }

    private static void DemoStaticAbstractFactory()
    {
        Console.WriteLine("\n=== 3. Static Abstract Members & Factory Pattern ===");

        // 型パラメータ T の静的ファクトリを直接呼び出す汎用ヘルパー
        AppConfig config = InstantiateDefault<AppConfig>();
        Console.WriteLine($"Created default config: {config.AppName} (Timeout: {config.TimeoutSeconds}s)");
    }

    private static T InstantiateDefault<T>() where T : IFactory<T>
    {
        return T.CreateDefault();
    }

    private static void DemoExplicitInterfaceImplementation()
    {
        Console.WriteLine("\n=== 4. Explicit Interface Implementation ===");

        var processor = new DocumentProcessor();

        // 直接 processor.Read() とは呼べず、インターフェース型にキャストして呼び分ける
        IOrderReader orderReader = processor;
        orderReader.Read();

        IInvoiceReader invoiceReader = processor;
        invoiceReader.Read();
    }

    private static void DemoGenericConstraints()
    {
        Console.WriteLine("\n=== 5. Generic Constraints (where T : class, new()) ===");

        var holder = new EntityContainer<DatabaseSetting>();
        Console.WriteLine($"Container created item with default host: {holder.Item.Host}");
    }

    public class DatabaseSetting
    {
        public string Host { get; set; } = "localhost";
    }

    public class EntityContainer<T> where T : class, new()
    {
        public T Item { get; } = new();
    }
}
