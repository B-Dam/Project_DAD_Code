# 💾 Setting/SaveLoad — 저장/로드 파이프라인

세이브 슬롯 → 직렬화 래퍼 → 개별 오브젝트 상태 복원까지 **두 패스 복원 + 안정화 루프**를 포함합니다.

---

## ✨ 설계 특징 (Highlights)
- 🧮 ISaveable 수집 → SaveWrapper/SaveEntry 직렬화
- 🔁 1차 복원 후 **SettleLoop**로 참조 안정화 → `OnPostLoad`
- 🆔 UniqueID 보장(에디터 OnValidate + 런타임 보정)
- 🧯 재진입 가드/로깅

---

## 🔁 핵심 흐름
Collect → Serialize → Load(1st) → SettleLoop → PostLoad

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `SaveLoadManagerCore.cs`
```csharp
public void RegisterSaveables()
    {
        saveables.Clear();

        var all = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
        var byId = new Dictionary<string, List<ISaveable>>();

        foreach (var sv in all)
        {
            string id = sv.UniqueID;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[SaveCore] 빈 ID: {((MonoBehaviour)sv).name}");
                continue;
            }
            if (!byId.TryGetValue(id, out var list))
                byId[id] = list = new List<ISaveable>();
            list.Add(sv);
        }

        int dup = 0;
        foreach (var kv in byId)
        {
            var list = kv.Value;
            ISaveable chosen;
            if (list.Count == 1)
            {
                chosen = list[0];
            }
            else
            {
                dup++;
                // 활성 오브젝트 우선, 그 다음 이름/경로로 안정화
                chosen = list
                         .OrderByDescending(sv => ((MonoBehaviour)sv).gameObject.activeInHierarchy)
                         .ThenBy(sv => ((MonoBehaviour)sv).gameObject.scene.name)
                         .ThenBy(sv => ((MonoBehaviour)sv).name)
                         .First();

                var info = string.Join(" | ", list.Select(sv =>
                {
                    var mb = (MonoBehaviour)sv;
                    return $"{mb.name}(activeInHierarchy={mb.gameObject.activeInHierarchy})";
                }));
                Debug.LogWarning($"[SaveCore] 중복 ID 해결: {kv.Key} -> {((MonoBehaviour)chosen).name} 선택 | 후보: {info}");
            }

            saveables[kv.Key] = chosen;
        }

        Debug.Log($"[SaveCore] ISaveable 등록 완료: {saveables.Count}개 (중복 해결 {dup}개)");
    }

// ...

public void SaveGame(int slotIndex)
    {
        RegisterSaveables();

        var entries = saveables.Select(kv => new SaveEntry {
            id   = kv.Key,
            json = JsonUtility.ToJson(kv.Value.CaptureState())
        }).ToArray();

        var wrapper = new SaveWrapper { version = 1, entries = entries };
        var json = JsonUtility.ToJson(wrapper, true);

        var path = Path.Combine(Application.persistentDataPath, string.Format(FILE_PATTERN, slotIndex));
        File.WriteAllText(path, json);
        Debug.Log($"[SaveCore] 슬롯 {slotIndex} 저장 → {path}");
    }

// ...

public bool HasSaveFile(int slotIndex)
    {
        var path = System.IO.Path.Combine(
            Application.persistentDataPath,
            string.Format(FILE_PATTERN, slotIndex));
        return System.IO.File.Exists(path);
    }
```

---

## 🐛 트러블슈팅
- `pending`이 0 미도달 시 UniqueID/OnPostLoad 점검
- 위치 의존 컴포넌트는 콜라이더 비활성화→적용→재활성화 권장
