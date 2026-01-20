using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TemplateManagerWPF.Models;

namespace TemplateManagerWPF.Repositories;

/// <summary>
/// 定型文データの永続化とCRUD操作を管理するリポジトリクラス
/// </summary>
public class TemplateRepository
{
    private readonly string _filePath;
    private TemplateData _data;

    /// <summary>
    /// コンストラクタ - デフォルトのファイルパスを使用
    /// </summary>
    public TemplateRepository()
    {
        // %USERPROFILE%\Documents\TemplateManager\templates.json
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var appFolder = Path.Combine(documentsPath, "TemplateManager");

        // フォルダが存在しない場合は作成
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _filePath = Path.Combine(appFolder, "templates.json");
        _data = new TemplateData();

        LoadFromJson();
    }

    /// <summary>
    /// JSONファイルからデータを読み込む
    /// </summary>
    private void LoadFromJson()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<TemplateData>(json) ?? new TemplateData();

                if (_data.Templates != null)
                {
                    foreach (var t in _data.Templates)
                    {
                        if ((t.Sections == null || t.Sections.Count == 0) && !string.IsNullOrEmpty(t.OldSection))
                        {
                            t.Sections = new List<string> { t.OldSection };
                        }

                        // nullガード（後続の処理でエラーにならないように）
                        t.Sections ??= new List<string>();
                    }
                }
            }
            else
            {
                _data = new TemplateData();
                SaveToJson();
            }
        }
        catch (Exception ex)
        {
            _data = new TemplateData();
            throw new Exception($"JSONファイルの読み込みに失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// JSONファイルにデータを保存
    /// </summary>
    private void SaveToJson()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // 読みやすいようにインデント
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 日本語をエスケープしない
            };

            var json = JsonSerializer.Serialize(_data, options);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"JSONファイルの保存に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 全定型文を取得
    /// </summary>
    public List<Template> GetAll()
    {
        return _data.Templates;
    }

    /// <summary>
    /// ID指定で定型文を取得
    /// </summary>
    public Template? GetById(int id)
    {
        return _data.Templates.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>
    /// タイトルで検索（部分一致、大文字小文字を区別しない）
    /// </summary>
    public List<Template> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return _data.Templates;
        }

        return _data.Templates
            .Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// セクション別に定型文を取得
    /// </summary>
    public List<Template> GetBySections(IEnumerable<string> sectionNames)
    {
        // 選択されたセクションのいずれか1つでも持っているテンプレートを返す
        return _data.Templates
            .Where(t => t.Sections.Any(s => sectionNames.Contains(s)))
            .ToList();
    }

    /// <summary>
    /// セクション一覧を取得（重複なし）
    /// </summary>
    public List<string> GetSections()
    {
        var allTemplates = GetAll();

        return allTemplates
            .SelectMany(t => t.Sections)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct() 
            .OrderBy(s => s)
            .ToList();
    }

    /// <summary>
    /// 定型文を追加
    /// </summary>
    public void Add(Template template)
    {
        // IDを自動採番
        template.Id = _data.NextId;
        template.CreatedAt = DateTime.Now;
        template.UpdatedAt = DateTime.Now;

        _data.Templates.Add(template);
        _data.NextId++;

        SaveToJson();
    }

    /// <summary>
    /// 定型文を更新
    /// </summary>
    public void Update(Template template)
    {
        var existing = GetById(template.Id);
        if (existing == null)
        {
            throw new Exception($"ID {template.Id} の定型文が見つかりません。");
        }

        // 既存のデータを更新
        existing.Title = template.Title;
        existing.Body = template.Body;
        existing.Sections = template.Sections;
        existing.Summary = template.Summary;
        existing.UpdatedAt = DateTime.Now;
        existing.LastUsedDate = DateTime.Now;

        SaveToJson();
    }

    /// <summary>
    /// 定型文を削除
    /// </summary>
    public void Delete(int id)
    {
        var template = GetById(id);
        if (template == null)
        {
            throw new Exception($"ID {id} の定型文が見つかりません。");
        }

        _data.Templates.Remove(template);
        SaveToJson();
    }
}
