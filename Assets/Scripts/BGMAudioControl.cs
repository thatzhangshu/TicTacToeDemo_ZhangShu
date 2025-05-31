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
            // ��ʼ�� Slider ��ʾΪ��ǰ����
            bgmSlider.value = bgmAudioSource.volume;

            // ����¼�����
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
