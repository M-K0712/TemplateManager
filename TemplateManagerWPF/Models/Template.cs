using System.Text.Json.Serialization; // 必要に応じて

public class Template
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<string> Sections { get; set; } = new List<string>();
    // 複数セクション対応時の互換性維持用プロパティ
    [JsonPropertyName("Section")]
    public string? OldSection { get; set; }
    public string Body { get; set; } = "";
    public string Summary { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string SectionsText => Sections != null ? string.Join(", ", Sections) : string.Empty;
    public DateTime? LastUsedDate { get; set; } 
}