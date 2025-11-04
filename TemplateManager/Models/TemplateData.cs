namespace TemplateManager.Models;

public class TemplateData
{
    public List<Template> Templates { get; set; } = new();
    public int NextId { get; set; } = 1;
}
