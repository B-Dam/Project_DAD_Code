# 🔊 Setting/Audio — 오디오

오디오 모듈 설명입니다.

---

## ✨ 설계 특징 (Highlights)
- (추가 예정)

---

## 🔁 핵심 흐름
Init → PlaySfx → SetVolume

---

## 🧩 대표 스크립트 & 핵심 코드 예시 — `AudioManager.cs`
```csharp
public void PlayBGM(string key, bool loop = true)
    {
        if (currentBGMName == key && bgmSource.isPlaying)
        {
            return;
        }

        var clip = LoadClip($"Audio/BGM/{key}");
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
            currentBGMName = key;
        }
        else
            Debug.LogWarning($"BGM 키 \"{key}\" 에 해당하는 클립을 찾을 수 없습니다.");
    }

// ...

private void Start()
    {
        // PlayerPrefs에 저장된 값을 불러오고, 불러온 값이 없다면 0.5로 설정
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume    = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume    = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        
        // 각 볼륨 세팅
        ApplyVolumes();
    }

// ...

private AudioClip LoadClip(string resourcePath)
    {
        if (clipCache.TryGetValue(resourcePath, out var cached))
            return cached;

        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
            clipCache[resourcePath] = clip;
        return clip;
    }
```
