using System.Runtime.CompilerServices;

namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 04: 例外処理・リソース管理 (Exception & Resource Management)
/// ============================================================================
/// 
/// 【他言語経験者向け要点】
/// 1. using 宣言 (C# 8+):
///    - `using var res = new MyResource();` と書くだけで、現在のスコープを抜けた時に
///      自動的に `Dispose()` が呼ばれる（RustのRAII / Drop、Javaのtry-with-resourcesに相当）。
/// 
/// 2. 例外フィルター (catch ... when ...):
///    - スタックを巻き戻す前に条件を評価できるため、スタックトレースを綺麗に保ったまま
///      特定のステータスコードやプロパティの時だけ例外をキャッチできる。
/// 
/// 3. 引数検証ガード & CallerArgumentExpression (C# 10+):
///    - `ArgumentNullException.ThrowIfNull(arg)` による一行バリデーション。
///    - コンパイラが呼び出し元の引数表現式（変数名や式）を自動抽出してエラーメッセージに含める。
/// </summary>
public static class ExceptionAndResource
{
    public sealed class DatabaseSession : IDisposable, IAsyncDisposable
    {
        public string SessionId { get; }

        public DatabaseSession(string sessionId)
        {
            SessionId = sessionId;
            Console.WriteLine($"  [DB] Session opened: {SessionId}");
        }

        public void Query(string sql) => Console.WriteLine($"  [DB] Executing: {sql} ({SessionId})");

        public void Dispose()
        {
            Console.WriteLine($"  [DB] Session disposed (Cleaned up): {SessionId}");
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }

    public class BusinessRuleException(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    public static async Task RunAsync()
    {
        DemoUsingDeclaration();
        await DemoAsyncUsingDeclaration();
        DemoExceptionFilters();
        DemoArgumentValidation();
    }

    private static void DemoUsingDeclaration()
    {
        Console.WriteLine("=== 1. 'using' Declaration (Auto-Dispose / RAII) ===");

        using var session = new DatabaseSession("conn-sync-999");
        session.Query("SELECT * FROM users");
        Console.WriteLine("Exiting DemoUsingDeclaration scope...");
    }

    private static async Task DemoAsyncUsingDeclaration()
    {
        Console.WriteLine("\n=== 2. 'await using' Declaration (IAsyncDisposable) ===");

        await using var session = new DatabaseSession("conn-async-123");
        session.Query("INSERT INTO audit_logs VALUES ('action')");
        Console.WriteLine("Exiting DemoAsyncUsingDeclaration scope...");
    }

    private static void DemoExceptionFilters()
    {
        Console.WriteLine("\n=== 3. Exception Filters ('catch ... when ...') ===");

        try
        {
            throw new BusinessRuleException("ERR_INSUFFICIENT_FUNDS", "Account balance is negative");
        }
        catch (BusinessRuleException ex) when (ex.Code == "ERR_INSUFFICIENT_FUNDS")
        {
            Console.WriteLine($"  [Filtered Catch]: Handled code '{ex.Code}': {ex.Message}");
        }
        catch (BusinessRuleException ex)
        {
            Console.WriteLine($"  [General Business Error]: {ex.Message}");
        }
    }

    private static void DemoArgumentValidation()
    {
        Console.WriteLine("\n=== 4. Modern Guard Clauses & CallerArgumentExpression (C# 10+) ===");

        string? validUser = "Alice";
        EnsureNotNull(validUser);
        Console.WriteLine($"Validated argument successfully: {validUser}");

        try
        {
            string? invalidInput = null;
            // null の場合、変数名 'invalidInput' が自動的に ArgumentNullException に埋め込まれる
            ArgumentNullException.ThrowIfNull(invalidInput);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"  -> Caught expected ThrowIfNull: ParamName='{ex.ParamName}'");
        }
    }

    // CallerArgumentExpression を使ったカスタムバリデータ
    private static void EnsureNotNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? expr = null) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(expr, $"Expression '{expr}' evaluated to null.");
        }
    }
}
