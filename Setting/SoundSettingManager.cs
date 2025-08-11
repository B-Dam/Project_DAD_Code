using UnityEngine;
using UnityEngine.UI;

public class SoundSettingManager : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        // 슬라이더 기본 값을 PlayerPrefs에서 가져옴
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        
        // 슬라이더 값이 변경될 때 마다 AudioManager로 전달
        masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }
}
