using UnityEngine;
using UnityEngine.UI;

public class BGMAudioControl : MonoBehaviour
{
    public Slider bgmSlider;
    public AudioSource bgmAudioSource;

    void Start()
    {
        if (bgmSlider != null && bgmAudioSource != null)
        {
            // 初始化 Slider 显示为当前音量
            bgmSlider.value = bgmAudioSource.volume;

            // 添加事件监听
            bgmSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void SetVolume(float value)
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = value;
        }
    }
}
