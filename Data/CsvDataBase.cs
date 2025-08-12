using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CsvDatabase : MonoBehaviour
{
    // CSV 파일을 읽어서 Dictionary<ID,T> 로 변환해 두는 제너릭 로더
    public static Dictionary<string, T> LoadCsvDict<T>(
        string fileName,         // Resources/ 하위 경로, 확장자 뺀 파일명
        Func<string[], T> factory)    // 한 줄(fields[])으로 T 객체를 만드는 람다
    {
        var dict = new Dictionary<string, T>();
        
        // TextAsset 로드
        var ta = Resources.Load<TextAsset>($"CSV/{fileName}");
        if (ta == null)
        {
            Debug.LogError($"CSV 로드 실패: Resources/CSV/{fileName}.csv 를 찾을 수 없습니다.");
            return dict;
        }
        
        // 줄 단위로 분할
        var lines = ta.text.Split(new[]{ "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return dict;  // 헤더만 있거나 빈 파일

        // 헤더 스킵 후 데이터 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            // 원본 필드 분할
            var raw   = lines[i].Split(',');
            // "null" 문자열을 빈 문자열로 교체
            var fields = raw
                         .Select(f => f == "null" ? string.Empty : f)
                         .ToArray();
            // 객체 생성
            var obj  = factory(fields);
            var key  = fields[0];  // ID 가 0번 컬럼이라고 가정
            dict[key] = obj;
        }
        return dict;
    }
}