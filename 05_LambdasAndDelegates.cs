using System.Linq.Expressions;

namespace Sample;

/// <summary>
/// ============================================================================
/// モジュール 05: ラムダ式・デリゲート・式ツリー (Lambdas & Delegates)
/// ============================================================================
/// 
/// 【他言語経験者向け要点】
/// 1. デリゲートの三兄弟 (Func, Action, Predicate):
///    - `Func<T, R>`: 戻り値がある関数 (Java の Function)
///    - `Action<T>`: 戻り値が void の関数 (Java の Consumer)
///    - `Predicate<T>`: bool を返す関数 (Java の Predicate)
/// 
/// 2. 静的ラムダ (static lambda: C# 9+):
///    - `static (x, y) => x + y` のように `static` を付与することで、
///      誤って外側の変数をキャプチャするのをコンパイル時に防ぎ、アロケーションをゼロにする。
/// 
/// 3. 式ツリー (Expression Trees: Expression<Func<...>>):
///    - ラムダ式を「実行コード」ではなく「AST（データ構造）」として受け取る。
///    - Entity Framework Core が C# のラムダ式を SQL クエリに変換できる秘密がこれ。
/// </summary>
public static class LambdasAndDelegates
{
    public static void Run()
    {
        DemoBasicLambdasAndDelegates();
        DemoVariableCaptureAndStaticLambda();
        DemoLocalFunctionsVsLambdas();
        DemoExpressionTreesAst();
    }

    private static void DemoBasicLambdasAndDelegates()
    {
        Console.WriteLine("=== 1. Built-in Delegates (Func, Action) & Natural Types ===");

        // 1. Func<int, int, int>: 2つの int を受け取り int を返す
        Func<int, int, int> add = (a, b) => a + b;
        Console.WriteLine($"Func add(10, 20): {add(10, 20)}");

        // 2. Action<string>: string を受け取り void を返す
        Action<string> logger = msg => Console.WriteLine($"  [LOG]: {msg}");
        logger("Standard Action executed");

        // 3. C# 10 自然な型推論 (var でラムダを変数に代入可能)
        var multiply = (int x, int y) => x * y;
        Console.WriteLine($"Natural type lambda multiply(4, 5): {multiply(4, 5)}");
    }

    private static void DemoVariableCaptureAndStaticLambda()
    {
        Console.WriteLine("\n=== 2. Closures & C# 9 Static Lambdas ===");

        int threshold = 50;

        // クロージャ: threshold をキャプチャ (裏でコンパイラがクロージャクラスをヒープ生成)
        Func<int, bool> isAbove = x => x > threshold;
        Console.WriteLine($"Is 70 above {threshold}? {isAbove(70)}");

        // 静的ラムダ (static lambda):
        // 外部変数をキャプチャしないことをコンパイラに保証。
        // もし外部変数を参照しようとするとコンパイルエラーになる。
        // -> GC アロケーションが完全にゼロになる！
        Func<int, int, int> maxFunc = static (a, b) => Math.Max(a, b);
        Console.WriteLine($"Static lambda max(15, 25): {maxFunc(15, 25)}");
    }

    private static void DemoLocalFunctionsVsLambdas()
    {
        Console.WriteLine("\n=== 3. Local Functions vs Lambdas ===");

        // ローカル関数:
        // メソッド内部に直接定義する関数。
        // デリゲートオブジェクトの生成オーバーヘッドがなく、インライン化されやすい。
        static int Factorial(int n)
        {
            return n <= 1 ? 1 : n * Factorial(n - 1);
        }

        Console.WriteLine($"Local function Factorial(5): {Factorial(5)}");
    }

    private static void DemoExpressionTreesAst()
    {
        Console.WriteLine("\n=== 4. Expression Trees (AST / LINQ to SQL Mechanism) ===");

        // ラムダ式を Expression<Func<int, bool>> として受け取る
        // これにより、コンパイル結果はマシン語ではなく「式ツリー（AST）」になる！
        Expression<Func<int, bool>> isAdultExpr = age => age >= 18;

        Console.WriteLine($"Expression Body:       {isAdultExpr.Body}");
        Console.WriteLine($"Expression NodeType:   {isAdultExpr.Body.NodeType}"); // GreaterThanOrEqual
        Console.WriteLine($"Expression Parameter:  {isAdultExpr.Parameters[0].Name}");

        if (isAdultExpr.Body is BinaryExpression binary)
        {
            Console.WriteLine($"  Left operand:  {binary.Left}");  // age
            Console.WriteLine($"  Right operand: {binary.Right}"); // 18
        }

        // 式ツリーを実行可能コードにコンパイルして動かすことも可能
        Func<int, bool> compiled = isAdultExpr.Compile();
        Console.WriteLine($"Compiled AST result for age 20: {compiled(20)}");
    }
}
