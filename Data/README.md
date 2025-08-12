# Battle/Data — 전투 데이터

전투에 사용되는 데이터 객체와 로더(캐릭터, 카드, 환경효과 등).

---

## 📂 폴더 내 스크립트
 ├── CardData.cs
 ├── CharacterData.cs
 ├── DataManager.cs
 ├── Editor/SkillDataImporter.cs
 ├── Enums/OwnerType.cs
 ├── EnvironmentEffect.cs

---

## 🔎 대표 스크립트: `DataManager.cs` 예시

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("SO 로드")]
    [SerializeField] CharacterDataSO defaultEnemy;

    [Serializable]
    public class SaveMetadata
    {
        public DateTime timestamp; // 저장용 시간
        public string chapterName; // 저장용 챕터 이름
        public string questName;   // 저장용 퀘스트 이름
    }
    public CharacterDataSO playerData { get; private set; }
    public CharacterDataSO enemyData { get; private set; }
    public CardData[] allCards { get; private set; }   // 모든 카드 SO

```

> 위 코드는 핵심 로직의 일부 예시입니다. 실제 구현은 프로젝트 원본 파일을 참고하세요.
