using System;
using System.Globalization;
using System.Windows.Data;

namespace TemplateManagerWPF.Converters;

/// <summary>
/// 概要が空の場合に代替テキストを表示するコンバーター
/// </summary>
public class EmptySummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string summary && !string.IsNullOrWhiteSpace(summary))
        {
            return summary;
        }
        return "(概要なし)";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
