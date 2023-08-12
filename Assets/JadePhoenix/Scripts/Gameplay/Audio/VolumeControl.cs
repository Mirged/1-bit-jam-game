using UnityEngine;
using UnityEngine.UI;

namespace JadePhoenix.Gameplay
{
    public class VolumeControl : MonoBehaviour
    {
        public Slider volumeSlider;

        protected virtual void Start()
        {
            // Initialize the slider value
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        public virtual void SetVolume(float volume)
        {
            AudioListener.volume = volume;
        }
    }
}

