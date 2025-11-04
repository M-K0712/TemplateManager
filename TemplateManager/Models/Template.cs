namespace TemplateManager.Models;
public class Template
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string Section { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}