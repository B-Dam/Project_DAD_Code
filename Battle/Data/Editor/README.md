# Battle/Data/Editor — 데이터 임포터

에디터 툴: 스킬/데이터 임포트 유틸리티.

---

## 📂 폴더 내 스크립트
 ├── SkillDataImporter.cs

---

## 🔎 대표 스크립트: `SkillDataImporter.cs` 예시

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SkillDataImporter : MonoBehaviour
{
    // CSV 경로
    private const string cardCsvPath = "Assets/Resources/CSV/skillcardDB.csv";

    // SO를 저장할 폴더
    private const string cardSoFolder = "Assets/Resources/ScriptableObjects/Cards";

    [MenuItem("Battle/Import Cards From CSV")]
    public static void ImportCards()
    {
        // 대상 폴더가 없으면 생성
        if (!Directory.Exists(cardSoFolder))
            Directory.CreateDirectory(cardSoFolder);

        // CSV 읽기
        var lines = File.ReadAllLines(cardCsvPath);
        
        static int ParseIntOrDefault(string raw, int defaultValue = 0) {
```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
