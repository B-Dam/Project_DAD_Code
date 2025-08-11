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
            raw = raw?.Trim().ToLower();
            if (string.IsNullOrEmpty(raw) || raw == "null" || !int.TryParse(raw, out var v))
                return defaultValue;
            return v;
        }

        /*static string ParseStringOrDefault(string raw, string defaultValue = "") {
            raw = raw?.Trim();
            if (string.IsNullOrEmpty(raw) || raw.ToLower() == "null")
                return defaultValue;
            return raw;
        }*/
        
        // 첫 줄은 헤더이므로 i=1부터
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 13)
            {
                Debug.LogWarning($"[ImportCards] {i+1}행 컬럼 부족({cols.Length}/13), 스킵");
                continue;
            }
            
            string id = cols[0].Trim();
            string ownerRaw  = cols[2].Trim();
            if (string.IsNullOrEmpty(ownerRaw) || ownerRaw.ToLower() == "")
                continue;
            var ownerTokens  = ownerRaw.Split('|');
            foreach (var tok in ownerTokens)
            {
                int ownerID = ParseIntOrDefault(tok);

                // SO 인스턴스 생성
                string assetPath = $"{cardSoFolder}/{id}_{ownerID}.asset";
                var so = AssetDatabase.LoadAssetAtPath<CardData>(assetPath)
                         ?? ScriptableObject.CreateInstance<CardData>();
                so.ownerID = ownerID;

                // 필드에 CSV 값 할당
                so.cardId = int.Parse(cols[0].Trim());
                so.displayName = cols[1].Trim();
                so.typePrimary = (CardTypePrimary)System.Enum.Parse(typeof(CardTypePrimary), cols[3].Trim(), true);

                // 카드 분류
                // 보조 분류 빈 문자열은 None 처리
                string sec = cols[4].Trim();
                if (string.IsNullOrEmpty(sec))
                {
                    so.typeSecondary = CardTypeSecondary.None;
                }
                else
                {
                    so.typeSecondary = (CardTypeSecondary)System.Enum.Parse(
                        typeof(CardTypeSecondary), sec, true);
                }

                // 효과 수치
                so.effectAttackValue         =  ParseIntOrDefault(cols[5]);
                so.effectShieldValue         =  ParseIntOrDefault(cols[6]);
                so.effectAttackIncreaseValue =  ParseIntOrDefault(cols[7]);
                so.effectAttackDebuffValue   =  ParseIntOrDefault(cols[8]);
                so.effectTurnValue           =  ParseIntOrDefault(cols[9]);

                // 효과 텍스트
                so.effectText = cols[10];

                // 등급
                if (so.ownerID == /*플레이어ID*/1000)
                {
                    so.rank = ParseIntOrDefault(cols[11], /*기본 1성*/1);
                }
                else 
                {
                    so.rank = 1;
                }
                
                // icon 과 costAP 는 CSV에 없으니 에디터에서 수동 할당

                // SO 에셋 생성 또는 갱신
                if (!File.Exists(assetPath)) 
                    AssetDatabase.CreateAsset(so, assetPath);
                else 
                    EditorUtility.SetDirty(so);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardData SO 생성 완료!");
    }
}
#endif