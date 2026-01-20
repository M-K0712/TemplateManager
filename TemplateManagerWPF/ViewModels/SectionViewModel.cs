using CommunityToolkit.Mvvm.ComponentModel;

public partial class SectionFilterItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isSelected;

    // チェック状態が変わったときにフィルタを走らせるためのイベント
    public Action? OnFilterChanged { get; set; }
    partial void OnIsSelectedChanged(bool value) => OnFilterChanged?.Invoke();
}