using System;
using TextCopy;

namespace TemplateManagerWPF.Helpers;

/// <summary>
/// クリップボード操作を行うヘルパークラス
/// </summary>
public static class ClipboardHelper
{
    /// <summary>
    /// 指定したテキストをクリップボードにコピーする
    /// </summary>
    /// <param name="text">コピーするテキスト</param>
    /// <returns>成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool CopyToClipboard(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            ClipboardService.SetText(text);
            return true;
        }
        catch (Exception)
        {
            // クリップボードアクセスに失敗した場合
            return false;
        }
    }
}