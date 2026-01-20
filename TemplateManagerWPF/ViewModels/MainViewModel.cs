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
    /// 検索対象
    /// </summary>
    [ObservableProperty]
    private string _searchTarget = "タイトル";

    /// <summary>
    /// 検索対象の選択肢
    /// </summary>
    public ObservableCollection<string> SearchTargets { get; } = new ObservableCollection<string>
    {
        "タイトル",
        "概要",
        "本文"
    };

    /// <summary>
    /// セクション一覧
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SectionFilterItem> _sectionFilters = new();

    /// <summary>
    /// ステータスメッセージ
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "準備完了";

    public MainViewModel()
    {
        _repository = new TemplateRepository();
        _templates = new ObservableCollection<Template>();
        _sectionFilters = new ObservableCollection<SectionFilterItem>();

        LoadData();
    }

    // フィルタモード
    [ObservableProperty]
    private bool _isAndFilter = false; // デフォルトは OR (false)

    /// <summary>
    /// モードが切り替えられたときに呼ばれる
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsAndFilterChanged(bool value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// 選択された定型文が変更されたときに呼ばれる
    /// </summary>
    partial void OnSelectedTemplateChanged(Template? value)
    {
        // 選択された定型文の本文を編集エリアにコピー
        EditableBody = value?.Body ?? string.Empty;
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
            SetSortedTemplates(allTemplates);

            // セクション一覧（List<string>）を取得
            var allSectionNames = _repository.GetSections();
            var filterItems = allSectionNames.Select(name => new SectionFilterItem
            {
                Name = name,
                IsSelected = false,
                // チェックが変わったときにフィルタリングを実行する
                OnFilterChanged = () => ApplyFilters()
            });

            SectionFilters = new ObservableCollection<SectionFilterItem>(filterItems);

            StatusMessage = $"定型文 {Templates.Count} 件を読み込みました";
        }
        finally
        {
            _isInitializing = false;
        }
    }

    /// <summary>
    /// 検索
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        ApplyFilters();
    }

    /// <summary>
    /// クリップボードにコピー
    /// </summary>
    [RelayCommand]
    private void CopyToClipboard()
    {
        if (SelectedTemplate == null || string.IsNullOrWhiteSpace(EditableBody))
        {
            StatusMessage = "コピーする内容がありません";
            return;
        }

        var success = ClipboardHelper.CopyToClipboard(EditableBody);
        if (success)
        {
            SelectedTemplate.LastUsedDate = DateTime.Now;
            var templateName = SelectedTemplate?.Title ?? "編集した内容";
            StatusMessage = $"「{templateName}」をクリップボードにコピーしました";
            _repository.Update(SelectedTemplate);
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
            // SectionFilters(SectionFilterItem型) から 名前(string) だけを抜き出してリストにする
            var existingSectionNames = SectionFilters.Select(x => x.Name).ToList();

            // 文字列のリストを渡す
            var dialog = new TemplateEditorWindow(existingSectionNames);

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
            // 現在存在するセクション名のリストを作成
            var existingSectionNames = SectionFilters.Select(x => x.Name).ToList();

            // 文字列リストとして渡す
            var dialog = new TemplateEditorWindow(SelectedTemplate, existingSectionNames);

            if (dialog.ShowDialog() == true && dialog.ResultTemplate != null)
            {
                _repository.Update(dialog.ResultTemplate);
                ApplyFilters();
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
            SelectedTemplate = null;
            ApplyFilters();
            StatusMessage = $"「{templateTitle}」を削除しました";
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
        }
        finally
        {
            _isInitializing = false;
        }

        LoadData();
    }

    /// <summary>
    /// 与えられたリストを「使用順 ＞ 更新順」でソートしてTemplatesにセットする
    /// </summary>
    private void SetSortedTemplates(IEnumerable<Template> source)
    {
        var sorted = source
            .OrderByDescending(t => t.LastUsedDate ?? DateTime.MinValue)
            .ThenByDescending(t => t.UpdatedAt)
            .ToList();

        Templates = new ObservableCollection<Template>(sorted);
    }

    private void ApplyFilters()
    {
        if (_isInitializing) return;

        try
        {
            // 1. 全件取得
            var results = _repository.GetAll().AsEnumerable();

            // 2. キーワード検索（コンボボックスの選択を反映）
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var keyword = SearchKeyword.ToLower();

                // SearchTarget (タイトル/概要/本文) に応じてフィルタ条件を切り替え
                results = results.Where(t =>
                {
                    return SearchTarget switch
                    {
                        "タイトル" => t.Title?.ToLower().Contains(keyword) == true,
                        "概要" => t.Summary?.ToLower().Contains(keyword) == true,
                        "本文" => t.Body?.ToLower().Contains(keyword) == true,
                        _ => t.Title?.ToLower().Contains(keyword) == true // デフォルト
                    };
                });
            }

            // 3. セクション絞り込み（ここは現状維持）
            var selectedSections = SectionFilters
                .Where(x => x.IsSelected)
                .Select(x => x.Name)
                .ToList();

            if (selectedSections.Any())
            {
                if (IsAndFilter)
                    results = results.Where(t => selectedSections.All(s => t.Sections != null && t.Sections.Contains(s)));
                else
                    results = results.Where(t => selectedSections.Any(s => t.Sections != null && t.Sections.Contains(s)));
            }

            // 4. 結果の反映
            var finalResults = results.ToList(); // 一度リスト化

            // 5. 検索結果が0件の場合のメッセージ処理
            if (!finalResults.Any())
            {
                Templates.Clear(); // 表示を空にする
                StatusMessage = "該当する定型文は見つかりませんでした。";
                return;
            }

            // 6. ソートして反映
            SetSortedTemplates(finalResults);

            // 7. ステータスメッセージ更新
            string targetInfo = !string.IsNullOrWhiteSpace(SearchKeyword) ? $"対象:[{SearchTarget}] " : "";
            StatusMessage = $"{targetInfo}結果: {Templates.Count} 件を表示中";
        }
        catch (Exception ex)
        {
            StatusMessage = $"フィルタエラー: {ex.Message}";
        }
    }
}
