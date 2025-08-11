// Assets/01.Scripts/Setting/SaveLoad/CameraSave.cs
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
public class CameraSave : MonoBehaviour, ISaveable
{
    [Serializable]
    private struct Data
    {
        public string mapId;           // 저장 당시 맵ID (컨파이너 이름: "Confine_{mapId}")
        public float  orthoSize;       // 메인 카메라 직교 사이즈(옵션)
        public Vector3 camPosition;    // 메인 카메라 위치(옵션)
    }

    private UniqueID _uid;
    public string UniqueID => (_uid ??= GetComponent<UniqueID>()).ID;

    [Header("옵션")]
    [SerializeField] bool snapFollowInsideOnLoad = true;  // 로드 직후 Follow가 바깥이면 안으로 한 번 스냅
    [SerializeField] float snapLerp = 0.02f;              // 바깥→안쪽으로 들어갈 보정 비율(0.0~0.1 권장)
    [SerializeField] string confinePrefix = "Confine_";   // 컨파이너 오브젝트 접두사

    // ===== 저장 =====
    public object CaptureState()
    {
        var data = new Data
        {
            mapId       = MapManager.Instance ? MapManager.Instance.currentMapID : null,
            orthoSize   = Camera.main ? Camera.main.orthographicSize : 0f,
            camPosition = Camera.main ? Camera.main.transform.position : Vector3.zero
        };

        Debug.Log($"[CameraSave/Capture] id={UniqueID} mapId={data.mapId} size={data.orthoSize}");
        return data; // JsonUtility 직렬화용 POCO
    }

    // ===== 불러오기 =====
    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<Data>(json);

        // 1) 카메라 기초 복원(원치 않으면 주석 처리)
        if (Camera.main)
        {
            if (data.orthoSize > 0f) Camera.main.orthographicSize = data.orthoSize;
            if (data.camPosition != Vector3.zero) Camera.main.transform.position = data.camPosition;
        }

        // 2) 두 프레임 대기 후(씬 콜라이더/맵 준비 보장) 컨파이너/VCam 재바인딩
        StartCoroutine(RebindRoutine(data.mapId));
    }

    IEnumerator RebindRoutine(string savedMapId)
    {
        yield return null;
        yield return null; // 콜라이더/시네머신/맵 오브젝트 준비 보장

        // MapManager의 값을 우선 진실로 사용
        string mapId = (MapManager.Instance && !string.IsNullOrEmpty(MapManager.Instance.currentMapID))
            ? MapManager.Instance.currentMapID
            : savedMapId;

        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogWarning("[CameraSave] mapId가 비어 있어 컨파이너 바인딩을 건너뜁니다.");
            yield break;
        }

        var poly = FindConfineCollider(mapId);
        if (!poly)
        {
            Debug.LogWarning($"[CameraSave] {confinePrefix}{mapId} Collider2D를 찾지 못했습니다.");
            yield break;
        }

        var vcam = GetActiveVirtualCamera();
        if (!vcam)
        {
            Debug.LogWarning("[CameraSave] 활성 CinemachineVirtualCamera를 찾지 못했습니다.");
            yield break;
        }

        // 활성 VCam의 Confiner에만 바인딩(2D/구버전 대응)
        if (!BindConfinerToVcam(vcam, poly))
        {
            Debug.LogWarning($"[CameraSave] {vcam.name}에 Confiner(2D/구버전)가 없습니다.");
            yield break;
        }

        // Follow가 바깥이면 안으로 한 번 스냅(옵션)
        if (snapFollowInsideOnLoad) SnapFollowInsideIfNeeded(vcam, poly);

        // 다음 LateUpdate에서 강제 재계산
        InvalidateVcamState(vcam);

        Debug.Log($"[CameraSave] 컨파이너 바운딩 갱신 완료 (map={mapId}, vcam={vcam.name})");
    }

    // ───────────────────── Helper: Confiner/VCam 찾기/바인딩 ─────────────────────

    Collider2D FindConfineCollider(string mapId)
    {
        var go = GameObject.Find($"Cameras/MapCollider/{mapId}_Collider");
        if (go) return go.GetComponent<Collider2D>();

        return null;
    }

    Component GetActiveVirtualCamera()
    {
        // CinemachineCore.Instance.GetActiveBrain(0) → brain.ActiveVirtualCamera.VirtualCameraGameObject
        var cmAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name.StartsWith("Cinemachine", StringComparison.OrdinalIgnoreCase));
        if (cmAsm == null) return null;

        var tCore = cmAsm.GetType("Cinemachine.CinemachineCore");
        var tVcam = cmAsm.GetType("Cinemachine.CinemachineVirtualCamera");
        if (tCore == null || tVcam == null) return null;

        var inst = tCore.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        if (inst == null) return null;

        var brainCount = (int)(tCore.GetProperty("BrainCount")?.GetValue(inst) ?? 0);
        if (brainCount <= 0) return null;

        var brain = tCore.GetMethod("GetActiveBrain", BindingFlags.Public | BindingFlags.Instance)?.Invoke(inst, new object[] { 0 });
        if (brain == null) return null;

        var icam = brain.GetType().GetProperty("ActiveVirtualCamera", BindingFlags.Public | BindingFlags.Instance)?.GetValue(brain);
        if (icam == null) return null;

        var vcamGO = icam.GetType().GetProperty("VirtualCameraGameObject", BindingFlags.Public | BindingFlags.Instance)?.GetValue(icam) as GameObject;
        if (!vcamGO) return null;

        var vcam = vcamGO.GetComponent(tVcam) as Component;
        return vcam;
    }

    bool BindConfinerToVcam(Component vcam, Collider2D poly)
    {
        var tConf2D = vcam.GetType().Assembly.GetType("Cinemachine.CinemachineConfiner2D");
        var tConf   = vcam.GetType().Assembly.GetType("Cinemachine.CinemachineConfiner");

        Component conf = null;
        if (tConf2D != null) conf = vcam.GetComponent(tConf2D) as Component;
        if (conf == null && tConf != null) conf = vcam.GetComponent(tConf) as Component;
        if (conf == null) return false;

        var fld = conf.GetType().GetField("m_BoundingShape2D",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        fld?.SetValue(conf, poly);

        var inv = conf.GetType().GetMethod("InvalidateCache",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? conf.GetType().GetMethod("InvalidatePathCache",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        inv?.Invoke(conf, null);

        return true;
    }

    void InvalidateVcamState(Component vcam)
    {
        var prop = vcam.GetType().GetProperty("PreviousStateIsValid",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null && prop.CanWrite) prop.SetValue(vcam, false);
    }

    void SnapFollowInsideIfNeeded(Component vcam, Collider2D poly)
    {
        // vcam.Follow (Transform)
        var pFollow = vcam.GetType().GetProperty("Follow", BindingFlags.Instance | BindingFlags.Public);
        var follow = pFollow?.GetValue(vcam) as Transform;
        if (!follow || !poly) return;

        var p = (Vector2)follow.position;
        if (poly.OverlapPoint(p)) return;

        var edge   = poly.ClosestPoint(p);
        var center = (Vector2)poly.bounds.center;
        var inside = Vector2.Lerp(edge, center, Mathf.Clamp01(snapLerp));
        follow.position = new Vector3(inside.x, inside.y, follow.position.z);

        Debug.Log("[CameraSave] Follow를 컨파이너 내부로 스냅");
    }
}