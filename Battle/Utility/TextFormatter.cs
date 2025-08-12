using System.Collections.Generic;

public static class TextFormatter
{
    /// <summary>
    /// template 문자열 안에 있는 "{key}" 플레이스홀더를 values[key] 문자열로 바꿔 줍니다.
    /// </summary>
    public static string Format(string template, Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template) || values == null)
            return template;

        foreach (var pair in values)
        {
            // "{damage}" → "10" 같이 치환
            template = template.Replace("{" + pair.Key + "}", pair.Value);
        }
        return template;
    }
}