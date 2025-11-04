# 定型文管理アプリ (TemplateManagerWPF)

定型文の登録・検索・管理を行うWindows GUIアプリケーション（WPF）

## 概要

よく使う定型文を管理し、検索・編集・クリップボードへのコピーを簡単に行えるアプリケーションです。

## 主な機能

- ✅ 定型文の一覧表示
- ✅ キーワード検索（部分一致）
- ✅ セクション別フィルタリング
- ✅ 一時編集機能（元の定型文は変更せずに編集可能）
- ✅ クリップボードへのコピー
- ✅ 新規登録
- ✅ 定型文の編集・保存
- ✅ 削除（確認ダイアログ付き）

## 技術スタック

- **フレームワーク**: .NET 8.0
- **UIフレームワーク**: WPF
- **アーキテクチャ**: MVVM (Model-View-ViewModel)
- **ライブラリ**:
  - CommunityToolkit.Mvvm 8.4.0 (MVVM実装支援)
  - TextCopy 6.2.1 (クリップボード操作)

## セットアップ

### 必要要件

- Windows 10/11
- .NET 8.0 SDK

### ビルド方法

```bash
# プロジェクトをクローン
git clone <repository-url>
cd TemplateManager

# ビルド
dotnet build

# 実行
dotnet run --project TemplateManagerWPF
```

または Visual Studio で `TemplateManagerWPF.sln` を開いて実行

## データ保存場所

```
%USERPROFILE%\Documents\TemplateManager\templates.json
```

例: `C:\Users\YourName\Documents\TemplateManager\templates.json`

## プロジェクト構造

```
TemplateManagerWPF/
├── Models/                    # データモデル
│   ├── Template.cs
│   └── TemplateData.cs
├── Repositories/              # データアクセス層
│   └── TemplateRepository.cs
├── Helpers/                   # ユーティリティ
│   └── ClipboardHelper.cs
├── ViewModels/                # ビューモデル
│   └── MainViewModel.cs
├── MainWindow.xaml           # メインウィンドウUI
├── TemplateEditorWindow.xaml # 編集ダイアログUI
└── App.xaml                  # アプリケーション設定
```

## 使い方

1. アプリを起動
2. 左側のリストから定型文を選択
3. 右側の編集エリアで自由に編集
4. 「クリップボードにコピー」ボタンでコピー
5. 新規登録や編集は下部のボタンから

## 開発者向け情報

### アーキテクチャ

MVVMパターンを採用し、UI（View）とビジネスロジック（ViewModel）を分離しています。

- **Model**: データ構造の定義
- **View**: XAML による UI 定義
- **ViewModel**: UIロジックとデータバインディング
- **Repository**: データの永続化

### データバインディング

WPFのデータバインディング機能により、ViewとViewModelが自動的に同期されます。

```xml
<TextBox Text="{Binding SearchKeyword, UpdateSourceTrigger=PropertyChanged}"/>
```

### CommunityToolkit.Mvvm の活用

Source Generatorにより、ボイラープレートコードを自動生成：

```csharp
[ObservableProperty]
private string _searchKeyword = string.Empty;

[RelayCommand]
private void Search() { ... }
```

## ライセンス

MIT License

## 作者

Kento
