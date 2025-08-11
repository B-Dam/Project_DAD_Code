using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    
    // Resource 폴더에서 로드한 클립을 캐싱할 딕셔너리
    private readonly System.Collections.Generic.Dictionary<string, AudioClip> clipCache
        = new System.Collections.Generic.Dictionary<string, AudioClip>();
    
    private float masterVolume = 1f;
    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.5f;

    public string currentBGMName { get; private set; }

    /*외부 호출시 다음 메서드 사용
    AudioManager.Instance.PlaySFX("효과음 이름");
    AudioManager.Instance.PlayBGM("BGM 이름");*/

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // PlayerPrefs에 저장된 값을 불러오고, 불러온 값이 없다면 0.5로 설정
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume    = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume    = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        
        // 각 볼륨 세팅
        ApplyVolumes();
    }
    
    // 마스터, BGM, SFX 각각 호출 시 재적용
    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        ApplyVolumes();
    }
    
    // BGM 볼륨 설정
    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        ApplyVolumes();
    }

    // SFX 볼륨 설정
    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        ApplyVolumes();
    }
    
    // 실제 AudioSource에 적용
    private void ApplyVolumes()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * masterVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }
    
    /// <summary>
    /// Resources/Audio/SFX/{key} 경로에서 AudioClip을 로드 후 PlayOneShot
    /// </summary>
    public void PlaySFX(string key, float volumeScale = 1f)
    {
        var clip = LoadClip($"Audio/SFX/{key}");
        if (clip != null)
            sfxSource.PlayOneShot(clip, volumeScale);
        else
            Debug.LogWarning($"SFX 키 \"{key}\" 에 해당하는 클립을 찾을 수 없습니다.");
    }
    
    /// <summary>
    /// Resources/Audio/BGM/{key} 경로에서 AudioClip을 로드 후 루프 재생
    /// </summary>
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

    /// <summary>
    /// 캐시에 없으면 Resources.Load, 있으면 캐시에서 바로 반환
    /// </summary>
    private AudioClip LoadClip(string resourcePath)
    {
        if (clipCache.TryGetValue(resourcePath, out var cached))
            return cached;

        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
            clipCache[resourcePath] = clip;
        return clip;
    }
}