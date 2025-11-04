using System.Collections.Generic;
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

    /// <summary>
    /// 編集モードかどうか（true: 編集、false: 新規登録）
    /// </summary>
    private readonly bool _isEditMode;

    /// <summary>
    /// 編集対象の定型文（編集モードの場合のみ）
    /// </summary>
    private readonly Template? _originalTemplate;

    /// <summary>
    /// コンストラクタ（新規登録用）
    /// </summary>
    /// <param name="existingSections">既存のセクション一覧</param>
    public TemplateEditorWindow(IEnumerable<string> existingSections)
    {
        InitializeComponent();
        _isEditMode = false;
        Title = "新規定型文登録";

        // セクションのドロップダウンに既存セクションを設定
        SectionComboBox.ItemsSource = existingSections.ToList();
    }

    /// <summary>
    /// コンストラクタ（編集用）
    /// </summary>
    /// <param name="template">編集対象の定型文</param>
    /// <param name="existingSections">既存のセクション一覧</param>
    public TemplateEditorWindow(Template template, IEnumerable<string> existingSections)
    {
        InitializeComponent();
        _isEditMode = true;
        _originalTemplate = template;
        Title = "定型文編集";

        // セクションのドロップダウンに既存セクションを設定
        SectionComboBox.ItemsSource = existingSections.ToList();

        // 既存の値を設定
        TitleTextBox.Text = template.Title;
        SectionComboBox.Text = template.Section;
        BodyTextBox.Text = template.Body;
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

        if (string.IsNullOrWhiteSpace(SectionComboBox.Text))
        {
            MessageBox.Show("セクションを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(BodyTextBox.Text))
        {
            MessageBox.Show("本文を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 結果の定型文を作成
        if (_isEditMode && _originalTemplate != null)
        {
            // 編集モード: 既存のテンプレートを更新
            ResultTemplate = new Template
            {
                Id = _originalTemplate.Id,
                Title = TitleTextBox.Text.Trim(),
                Section = SectionComboBox.Text.Trim(),
                Body = BodyTextBox.Text,
                CreatedAt = _originalTemplate.CreatedAt,
                UpdatedAt = _originalTemplate.UpdatedAt // Repositoryで更新される
            };
        }
        else
        {
            // 新規登録モード: 新しいテンプレートを作成
            ResultTemplate = new Template
            {
                Title = TitleTextBox.Text.Trim(),
                Section = SectionComboBox.Text.Trim(),
                Body = BodyTextBox.Text
                // Id, CreatedAt, UpdatedAtはRepositoryで設定される
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
