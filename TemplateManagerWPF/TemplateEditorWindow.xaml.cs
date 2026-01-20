using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TemplateManagerWPF.Models;

namespace TemplateManagerWPF;

/// <summary>
/// 定型文の新規登録・編集を行うダイアログウィンドウ
/// </summary>
public partial class TemplateEditorWindow : Window
{
    /// <summary>
    /// 編集結果の定型文（保存ボタンが押された場合のみ値が設定される）
    /// </summary>
    public Template? ResultTemplate { get; private set; }

    private readonly bool _isEditMode;
    private readonly Template? _originalTemplate;

    // チェックボックス管理用のコレクション
    private ObservableCollection<SectionFilterItem> _sectionItems = new();

    /// <summary>
    /// コンストラクタ（新規登録用）
    /// </summary>
    public TemplateEditorWindow(IEnumerable<string> existingSections)
    {
        InitializeComponent();
        _isEditMode = false;
        Title = "新規定型文登録";

        SetupSections(existingSections, null);
    }

    /// <summary>
    /// コンストラクタ（編集用）
    /// </summary>
    public TemplateEditorWindow(Template template, IEnumerable<string> existingSections)
    {
        InitializeComponent();
        _isEditMode = true;
        _originalTemplate = template;
        Title = "定型文編集";

        // 既存の値を設定
        TitleTextBox.Text = template.Title;
        SummaryTextBox.Text = template.Summary;
        BodyTextBox.Text = template.Body;

        SetupSections(existingSections, template.Sections);
    }

    /// <summary>
    /// セクションリストの初期セットアップ
    /// </summary>
    private void SetupSections(IEnumerable<string> allSections, List<string>? selectedSections)
    {
        var items = allSections.Select(s => new SectionFilterItem
        {
            Name = s,
            IsSelected = selectedSections?.Contains(s) ?? false
        }).OrderBy(x => x.Name);

        _sectionItems = new ObservableCollection<SectionFilterItem>(items);
        ExistingSectionsListBox.ItemsSource = _sectionItems;
    }

    /// <summary>
    /// 新規セクション追加ボタン
    /// </summary>
    private void AddSection_Click(object sender, RoutedEventArgs e)
    {
        var newName = NewSectionTextBox.Text.Trim();
        if (string.IsNullOrEmpty(newName)) return;

        // 重複チェック
        if (!_sectionItems.Any(x => x.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            _sectionItems.Add(new SectionFilterItem { Name = newName, IsSelected = true });
            NewSectionTextBox.Clear();
        }
        else
        {
            // すでにある場合はチェックを入れる
            var existing = _sectionItems.FirstOrDefault(x => x.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
            if (existing != null) existing.IsSelected = true;
            NewSectionTextBox.Clear();
        }
    }

    /// <summary>
    /// 保存ボタンクリック時の処理
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // 入力チェック
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("タイトルを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // チェックが入っているセクションをリスト化
        var selectedSections = _sectionItems
            .Where(x => x.IsSelected)
            .Select(x => x.Name)
            .ToList();

        if (!selectedSections.Any())
        {
            MessageBox.Show("セクションを少なくとも1つ選択または入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(BodyTextBox.Text))
        {
            MessageBox.Show("本文を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_isEditMode && _originalTemplate != null)
        {
            ResultTemplate = new Template
            {
                Id = _originalTemplate.Id,
                Title = TitleTextBox.Text.Trim(),
                Sections = selectedSections,
                Summary = SummaryTextBox.Text.Trim(),
                Body = BodyTextBox.Text,
                CreatedAt = _originalTemplate.CreatedAt,
                UpdatedAt = DateTime.Now
            };
        }
        else
        {
            ResultTemplate = new Template
            {
                Title = TitleTextBox.Text.Trim(),
                Sections = selectedSections,
                Summary = SummaryTextBox.Text.Trim(),
                Body = BodyTextBox.Text,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        DialogResult = true;
        Close();
    }

    /// <summary>
    /// キャンセルボタンクリック時の処理
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}