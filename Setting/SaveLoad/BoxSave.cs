using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct BoxData
{
    public int        ver;             // ★ 세이브 버전 (v2부터 tag/layer/collider 저장)
    public bool       isActive;
    public Vector3    position;
    public Quaternion rotation;

    // v2~
    public int        layer;
    public string     tag;
    public bool       colliderEnabled;
}

[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(Rigidbody2D))]
public class BoxSave : MonoBehaviour, ISaveable, IPostLoad
{
    private UniqueID    idComp;
    private Rigidbody2D rb;
    private Collider2D  col;

    private BoxData _pending;
    private bool    _hasPending;

    public string UniqueID
    {
        get
        {
            if (!idComp)
            {
                idComp = GetComponent<UniqueID>();
                if (!idComp) Debug.LogError($"[Save] UniqueID 누락: {name}", this);
            }
            return idComp.ID;
        }
    }

    void Awake()
    {
        idComp = GetComponent<UniqueID>();
        rb     = GetComponent<Rigidbody2D>();
        col    = GetComponent<Collider2D>();
    }

    public object CaptureState()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();

        return new BoxData
        {
            ver            = 2,                        // ★ 현재 버전
            isActive       = gameObject.activeSelf,
            position       = transform.position,
            rotation       = transform.rotation,
            layer          = gameObject.layer,
            tag            = gameObject.tag,
            colliderEnabled= col ? col.enabled : true
        };
    }

    public void RestoreState(object state)
    {
        var json = state as string;
        if (string.IsNullOrEmpty(json)) return;

        _pending    = JsonUtility.FromJson<BoxData>(json);
        _hasPending = true;
        
        // 🔧 추가: 런타임 이동/코루틴 정지 (같은 GO 한정)
        StopAllCoroutines();
        foreach (var mb in GetComponents<MonoBehaviour>()) mb.StopAllCoroutines();

        var push = GetComponent<BoxPush>();
        if (push) push.ForceStop();

        // 즉시 1회 적용
        ApplyNow(_pending);
        // 다음 프레임 재확정
        StartCoroutine(ApplyNextFrame());
        // 상호작용 보정
        StartCoroutine(PostRestoreSanity());
    }

    public void OnPostLoad()
    {
        if (_hasPending) ApplyNow(_pending);
    }

    private IEnumerator ApplyNextFrame()
    {
        yield return null;
        if (_hasPending) ApplyNow(_pending);
    }

    private void ApplyNow(BoxData data)
    {
        bool v2 = data.ver >= 2;

        // v2부터만 태그/레이어/콜라이더 복원 (구버전 저장본 보호)
        // 🔧 추가: 위치 적용 전에 한번 더 이동 강제 정지
        var push = GetComponent<BoxPush>();
        if (push) push.ForceStop();
        
        if (v2)
        {
            if (!string.IsNullOrEmpty(data.tag))
            {
                try { gameObject.tag = data.tag; } catch { /* 미등록 태그면 무시 */ }
            }
            if (data.layer >= 0 && data.layer <= 31)
                gameObject.layer = data.layer;

            if (!col) col = GetComponent<Collider2D>();
            if (col) col.enabled = data.colliderEnabled;
        }

        // 물리 리셋 + 진짜 순간이동
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            var prev = rb.interpolation;
            rb.interpolation   = RigidbodyInterpolation2D.None;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity  = Vector2.zero;
#else
            rb.velocity        = Vector2.zero;
#endif
            rb.angularVelocity = 0f;

            rb.position = (Vector2)data.position;

            bool hasRot = !(data.rotation.x == 0f && data.rotation.y == 0f &&
                            data.rotation.z == 0f && data.rotation.w == 0f);
            rb.rotation = hasRot ? data.rotation.eulerAngles.z
                                 : transform.rotation.eulerAngles.z;

            Physics2D.SyncTransforms();
            rb.interpolation = prev;
            rb.WakeUp();
        }
        else
        {
            transform.SetPositionAndRotation(
                data.position,
                (data.rotation.x == 0f && data.rotation.y == 0f &&
                 data.rotation.z == 0f && data.rotation.w == 0f)
                    ? transform.rotation : data.rotation
            );
        }

        // 활성/비활성은 맨 마지막
        if (gameObject.activeSelf != data.isActive)
            gameObject.SetActive(data.isActive);
    }
    
    private IEnumerator PostRestoreSanity()
    {
        yield return null; // 모든 Load/OnPostLoad 이후

        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
        rb.velocity = Vector2.zero;
#endif
            rb.angularVelocity = 0f;
            rb.simulated = true;
            rb.WakeUp();
        }

        // ✅ 콜라이더는 반드시 켜기
        foreach (var c in GetComponentsInChildren<Collider2D>(true))
            c.enabled = true;

        Physics2D.SyncTransforms();
    }
}
