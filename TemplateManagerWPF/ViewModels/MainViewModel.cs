using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TemplateManagerWPF.Models;
using TemplateManagerWPF.Repositories;
using TemplateManagerWPF.Helpers;

namespace TemplateManagerWPF.ViewModels;

/// <summary>
/// メイン画面のViewModel
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly TemplateRepository _repository;

    /// <summary>
    /// 定型文リスト（表示用）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Template> _templates;

    /// <summary>
    /// 選択中の定型文
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditableBody))]
    private Template? _selectedTemplate;

    /// <summary>
    /// 編集可能な本文（一時的な編集用）
    /// </summary>
    [ObservableProperty]
    private string _editableBody = string.Empty;

    /// <summary>
    /// 検索キーワード
    /// </summary>
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    /// <summary>
    /// セクション一覧
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _sections;

    /// <summary>
    /// 選択中のセクション
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FilterBySectionCommand))]
    private string? _selectedSection;

    /// <summary>
    /// ステータスメッセージ
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "準備完了";

    public MainViewModel()
    {
        _repository = new TemplateRepository();
        _templates = new ObservableCollection<Template>();
        _sections = new ObservableCollection<string>();

        LoadData();
    }

    /// <summary>
    /// 選択された定型文が変更されたときに呼ばれる
    /// </summary>
    partial void OnSelectedTemplateChanged(Template? value)
    {
        // 選択された定型文の本文を編集エリアにコピー
        EditableBody = value?.Body ?? string.Empty;
    }

    /// <summary>
    /// 選択されたセクションが変更されたときに呼ばれる
    /// </summary>
    partial void OnSelectedSectionChanged(string? value)
    {
        // 初期化中やnullへの変更は無視（無限ループ防止）
        if (_isInitializing)
        {
            return;
        }

        FilterBySection();
    }

    private bool _isInitializing = false;

    /// <summary>
    /// データを読み込む
    /// </summary>
    private void LoadData()
    {
        _isInitializing = true;
        try
        {
            // 全定型文を取得
            var allTemplates = _repository.GetAll();
            Templates = new ObservableCollection<Template>(allTemplates);

            // セクション一覧を取得
            var allSections = _repository.GetSections();
            Sections = new ObservableCollection<string>(allSections);

            StatusMessage = $"定型文 {Templates.Count} 件を読み込みました";
        }
        finally
        {
            _isInitializing = false;
        }
    }

    /// <summary>
    /// 検索コマンド
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        try
        {
            var results = _repository.Search(SearchKeyword);
            Templates = new ObservableCollection<Template>(results);
            StatusMessage = $"検索結果: {Templates.Count} 件";
        }
        catch (Exception ex)
        {
            StatusMessage = $"検索エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// セクションでフィルタ
    /// </summary>
    [RelayCommand]
    private void FilterBySection()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedSection))
            {
                LoadData();
                return;
            }

            var results = _repository.GetBySection(SelectedSection);
            Templates = new ObservableCollection<Template>(results);
            StatusMessage = $"セクション「{SelectedSection}」: {Templates.Count} 件";
        }
        catch (Exception ex)
        {
            StatusMessage = $"フィルタエラー: {ex.Message}";
        }
    }

    /// <summary>
    /// クリップボードにコピー
    /// </summary>
    [RelayCommand]
    private void CopyToClipboard()
    {
        if (string.IsNullOrWhiteSpace(EditableBody))
        {
            StatusMessage = "コピーする内容がありません";
            return;
        }

        var success = ClipboardHelper.CopyToClipboard(EditableBody);
        if (success)
        {
            var templateName = SelectedTemplate?.Title ?? "編集した内容";
            StatusMessage = $"「{templateName}」をクリップボードにコピーしました";
        }
        else
        {
            StatusMessage = "クリップボードへのコピーに失敗しました";
        }
    }

    /// <summary>
    /// 新規登録
    /// </summary>
    [RelayCommand]
    private void AddTemplate()
    {
        try
        {
            var dialog = new TemplateEditorWindow(Sections);
            if (dialog.ShowDialog() == true && dialog.ResultTemplate != null)
            {
                _repository.Add(dialog.ResultTemplate);
                LoadData();
                StatusMessage = $"「{dialog.ResultTemplate.Title}」を登録しました";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"登録エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// 編集
    /// </summary>
    [RelayCommand]
    private void EditTemplate()
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "定型文を選択してください";
            return;
        }

        try
        {
            var dialog = new TemplateEditorWindow(SelectedTemplate, Sections);
            if (dialog.ShowDialog() == true && dialog.ResultTemplate != null)
            {
                _repository.Update(dialog.ResultTemplate);
                LoadData();
                StatusMessage = $"「{dialog.ResultTemplate.Title}」を更新しました";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"更新エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// 削除
    /// </summary>
    [RelayCommand]
    private void DeleteTemplate()
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "定型文を選択してください";
            return;
        }

        // 削除確認ダイアログ
        var result = System.Windows.MessageBox.Show(
            $"「{SelectedTemplate.Title}」を削除してもよろしいですか？",
            "削除の確認",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
        {
            StatusMessage = "削除をキャンセルしました";
            return;
        }

        try
        {
            var templateTitle = SelectedTemplate.Title;
            _repository.Delete(SelectedTemplate.Id);
            LoadData();
            StatusMessage = $"「{templateTitle}」を削除しました";
            SelectedTemplate = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"削除エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// 全件表示
    /// </summary>
    [RelayCommand]
    private void ShowAll()
    {
        _isInitializing = true;
        try
        {
            SearchKeyword = string.Empty;
            SelectedSection = null;
        }
        finally
        {
            _isInitializing = false;
        }

        LoadData();
    }
}