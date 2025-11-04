namespace TemplateManagerWPF.Models;
public class Template
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty; // 概要（旧データとの互換性のためデフォルト値を設定）
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}