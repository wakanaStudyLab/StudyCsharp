# Modern C# .csproj 完全理解ガイド (SDK-Style .csproj Master Guide)

.NET Core / .NET 5 以降で導入された **「SDK スタイル .csproj」** の完全解説書です。

古い .NET Framework 時代の「長大で編集困難な XML」から完全に脱却し、現代の C# におけるプロジェクト設定、NuGet パッケージ管理、マルチターゲット、Native AOT（単一実行ファイル出力）、パフォーマンス・警告の厳格化までを網羅しています。

---

## 📑 目次

1. [SDK スタイル .csproj の本質（レガシーとの決定的違い）](#1-sdk-スタイル-csproj-の本質レガシーとの決定的違い)
2. [最重要基本プロパティ完全リファレンス (`<PropertyGroup>`)](#2-最重要基本プロパティ完全リファレンス-propertygroup)
3. [依存関係の管理 (`<ItemGroup>`: NuGet & プロジェクト参照)](#3-依存関係の管理-itemgroup-nuget--プロジェクト参照)
4. [ファイル・静的アセットのコピー設定 (`CopyToOutputDirectory`)](#4-ファイル静的アセットのコピー設定-copytooutputdirectory)
5. [条件分岐 (`Condition`) による高度な環境制御](#5-条件分岐-condition-による高度な環境制御)
6. [マルチターゲット (Multi-targeting) によるライブラリ開発](#6-マルチターゲット-multi-targeting-によるライブラリ開発)
7. [単一バイナリ配布・Native AOT・トリミング設定](#7-単一バイナリ配布native-aotトリミング設定)
8. [実務でそのまま使えるテンプレート集](#8-実務でそのまま使えるテンプレート集)
9. [.NET CLI (`dotnet`) コマンド早見表](#9-net-cli-dotnet-コマンド早見表)

---

## 1. SDK スタイル .csproj の本質（レガシーとの決定的違い）

### 1-1. ファイル明示列挙からの解放（ワイルドカード自動インクルード）
- **レガシー .csproj (.NET Framework 時代)**:  
  ファイルを追加するたびに `<Compile Include="Services\UserService.cs" />` と XML に全ファイルを明記しなければならず、Git のマージコンフリクトの温床でした。
- **SDK スタイル .csproj (.NET 5 / 6 / 7 / 8 / 9+)**:  
  プロジェクトフォルダ配下のすべての `.cs` ファイルが**自動的にコンパイル対象として認識（Globbing）**されます。最小限のファイルなら**たった 5 行**で記述できます。

```xml
<!-- 最小限の SDK スタイル csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>
```

### 1-2. `<Project Sdk="...">` の種類
プロジェクトの種類に応じてルートタグの `Sdk` 属性を切り替えます。

| Sdk 名 | 用途 |
| :--- | :--- |
| **`Microsoft.NET.Sdk`** | コンソールアプリ、クラスライブラリ、CLI ツール（標準） |
| **`Microsoft.NET.Sdk.Web`** | ASP.NET Core Web API、Minimal API、Blazor、MVC |
| **`Microsoft.NET.Sdk.Worker`** | バックグラウンドワーカーサービス（デーモン / Windows サービス） |

---

## 2. 最重要基本プロパティ完全リファレンス (`<PropertyGroup>`)

実務で必ず設定する主要プロパティ一覧です。

```xml
<PropertyGroup>
  <!-- 1. 成果物の種類: Exe (実行可能バイナリ), Library (dll クラスライブラリ), WinExe -->
  <OutputType>Exe</OutputType>

  <!-- 2. ターゲットフレームワーク (TFM: Target Framework Moniker) -->
  <TargetFramework>net9.0</TargetFramework>

  <!-- 3. 言語バージョン: latest (推奨), preview, 12.0, 13.0 -->
  <LangVersion>latest</LangVersion>

  <!-- 4. Null 許容参照型 (NRT) の有効化 (C# 8+ の必須設定) -->
  <Nullable>enable</Nullable>

  <!-- 5. 一般的な名前空間 (System, System.Collections.Generic 等) の暗黙 using (C# 10+) -->
  <ImplicitUsings>enable</ImplicitUsings>

  <!-- 6. 警告をエラーとして扱う (CI/CD でのコード品質維持に推奨) -->
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

  <!-- 7. ビルド時にコードスタイル規則 (EditorConfig) を強制検査 -->
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

### 💡 `Nullable` と `ImplicitUsings` の重要性
- **`<Nullable>enable</Nullable>`**:  
  これを有効にすると、`string` は Non-nullable（null 代入不可）になり、null を許容したい場合は明示的に `string?` と書く必要があります。null 参照のバグをコンパイル時に検知できるため、**現代のすべての新規プロジェクトで必須**です。
- **`<ImplicitUsings>enable</ImplicitUsings>`**:  
  ファイルの先頭に `using System;` や `using System.Linq;` を毎回書く必要がなくなります。

---

## 3. 依存関係の管理 (`<ItemGroup>`: NuGet & プロジェクト参照)

### 3-1. NuGet パッケージ参照 (`<PackageReference>`)
NuGet から外部ライブラリを導入する場合に記述します（CLI の `dotnet add package` でも自動追記されます）。

```xml
<ItemGroup>
  <!-- 通常のパッケージ追加 -->
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />

  <!-- 開発時のみ使用するツール（成果物へのバンドル除外） -->
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### 3-2. ソリューション内プロジェクト参照 (`<ProjectReference>`)
別プロジェクト（例: `Domain` や `Infrastructure`）を参照する場合です。

```xml
<ItemGroup>
  <ProjectReference Include="..\MyProject.Core\MyProject.Core.csproj" />
</ItemGroup>
```

---

## 4. ファイル・静的アセットのコピー設定 (`CopyToOutputDirectory`)

設定ファイル（`appsettings.json`）やテスト用データファイルを、ビルド出力先フォルダ（`bin/Debug/...`）に自動コピーしたい場合の設定です。

```xml
<ItemGroup>
  <!-- appsettings.json を「新しい場合にのみコピー」 -->
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>

  <!-- または Content として指定 (Webプロジェクト等) -->
  <Content Update="config\settings.yaml">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

| 設定値 | 挙動 | 推奨用途 |
| :--- | :--- | :--- |
| **`PreserveNewest`** | 元ファイルが更新された時だけコピー | **設定ファイル、テストデータ（推奨）** |
| **`Always`** | 毎ビルド必ず上書きコピー | 頻繁に書き換わるリソース |
| **`Never`** | コピーしない | プロジェクト内でのみ参照 |

---

## 5. 条件分岐 (`Condition`) による高度な環境制御

MSBuild の強力な機能として、環境変数やビルド構成に応じた **`Condition` 属性による条件分岐** が可能です。

```xml
<!-- Release ビルド時のみ最適化と警告の厳格化を適用 -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <Optimize>true</Optimize>
</PropertyGroup>

<!-- OS 別のパッケージ参照切り替え -->
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
  <PackageReference Include="System.Diagnostics.EventLog" Version="8.0.0" />
</ItemGroup>
```

---

## 6. マルチターゲット (Multi-targeting) によるライブラリ開発

「.NET 8 と .NET 9、およびレガシーな .NET Standard 2.0 の両方で動く NuGet ライブラリを作りたい」場合、**複数ターゲットフレームワーク（`<TargetFrameworks>` 複数形）** を指定します。

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- セミコロン区切りで複数指定 -->
    <TargetFrameworks>net8.0;net9.0;netstandard2.0</TargetFrameworks>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- netstandard2.0 の時だけ下位互換パッケージを追加 -->
  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
    <PackageReference Include="IndexRange" Version="1.0.3" />
  </ItemGroup>
</Project>
```

### C# コード内でのプリプロセッサ分岐
```csharp
#if NET9_0_OR_GREATER
    // .NET 9 以降の最新 API を使用
    return System.Threading.Lock();
#else
    // 古いフレームワーク用のフォールバック
    return new object();
#endif
```

---

## 7. 単一バイナリ配布・Native AOT・トリミング設定

現代の .NET では、Go や Rust のように **「.NET ランタイム未インストールの環境でも動く単一の実行可能バイナリ」** を出力できます。

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net9.0</TargetFramework>

  <!-- 1. 単一ファイル (Single-File) としてパッケージング -->
  <PublishSingleFile>true</PublishSingleFile>

  <!-- 2. .NET ランタイムをバイナリ内に同梱 (自己完結型: Self-Contained) -->
  <SelfContained>true</SelfContained>

  <!-- 3. 未使用の IL コードを削ぎ落としてファイルサイズを削減 (Trimmed) -->
  <PublishTrimmed>true</PublishTrimmed>

  <!-- 4. Native AOT: C# を直接マシン語に事前コンパイル (起動時間ゼロ・超省メモリ) -->
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

---

## 8. 実務でそのまま使えるテンプレート集

### テンプレートA: 本番グレード CLI / コンソールアプリ
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>

    <!-- コード品質設定 -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <!-- 設定ファイルの配置 -->
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <!-- 定番ライブラリ -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
  </ItemGroup>

</Project>
```

### テンプレートB: 公開用 NuGet ライブラリ
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- NuGet パッケージメタデータ -->
    <PackageId>MyCompany.AwesomeLib</PackageId>
    <Version>1.0.0</Version>
    <Authors>Harun</Authors>
    <Description>High-performance utility library for modern .NET.</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <GenerateDocumentationFile>true</GenerateDocumentationFile> <!-- XML ドキュメント生成 -->
  </PropertyGroup>

</Project>
```

---

## 9. .NET CLI (`dotnet`) コマンド早見表

手作業で `.csproj` を編集しなくても、CLI から安全に操作できます。

| 操作 | コマンド例 |
| :--- | :--- |
| **プロジェクト作成** | `dotnet new console -o MyApp` / `dotnet new webapi -o MyApi` |
| **NuGet 追加** | `dotnet add package Newtonsoft.Json` |
| **NuGet 削除** | `dotnet remove package Newtonsoft.Json` |
| **プロジェクト参照追加** | `dotnet add reference ../Core/Core.csproj` |
| **ビルド** | `dotnet build -c Release` |
| **実行** | `dotnet run` |
| **自己完結型発行** | `dotnet publish -c Release -r win-x64 --self-contained` |
| **Native AOT 発行** | `dotnet publish -c Release -r win-x64 /p:PublishAot=true` |
