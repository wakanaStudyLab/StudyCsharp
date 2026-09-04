using Sample;

// ============================================================================
// Modern C# (C# 12 / 13 on .NET 9) Crash Course - Main Runner
// For Rust / Go / Java / Python / C++ Developers
// ============================================================================

PrintBanner("MODERN C# CRASH COURSE (Running on .NET " + Environment.Version + ")");

// モジュール 01: 基本型・レコード・パターンマッチング
PrintSection("01: Primitive Types, Records, Pattern Matching, and NRT");
BasicsAndTypes.Run();

// モジュール 02: コレクション・LINQ・Span (ゼロアロケーション)
PrintSection("02: Collections, LINQ Deferred Execution, and Span<T>");
CollectionsAndLinq.Run();

// モジュール 03: 非同期プログラミング・並行処理・Channels
PrintSection("03: Async/Await, CancellationToken, and Channels");
await AsyncAndConcurrency.RunAsync();

// モジュール 04: 例外処理・リソース管理 (using / IDisposable)
PrintSection("04: Exception Handling, using Declaration, and Exception Filters");
await ExceptionAndResource.RunAsync();

// モジュール 05: ラムダ式・デリゲート・式ツリー (Expression Trees)
PrintSection("05: Lambdas, Static Lambdas, Local Functions, and Expression Trees");
LambdasAndDelegates.Run();

// モジュール 06: ジェネリクス・インターフェース・モダン型システム
PrintSection("06: Generics, Static Abstract Members (Generic Math), and DIM");
GenericsAndInterfaces.Run();

// モジュール 07: 高度な言語機能・イテレータ・拡張メソッド
PrintSection("07: Yield Iterators, Extension Methods, Operator Overloading, and Type Aliases");
AdvancedLanguageFeatures.Run();

PrintBanner("ALL C# TUTORIAL MODULES COMPLETED SUCCESSFULLY!");

static void PrintBanner(string title)
{
    Console.WriteLine("\n" + new string('=', 64));
    Console.WriteLine($"  {title}");
    Console.WriteLine(new string('=', 64) + "\n");
}

static void PrintSection(string title)
{
    Console.WriteLine("\n" + new string('#', 64));
    Console.WriteLine($"# {title}");
    Console.WriteLine(new string('#', 64) + "\n");
}
