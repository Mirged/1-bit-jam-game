using System.Collections.Generic;
using UnityEngine;

namespace JadePhoenix.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEffectController : MonoBehaviour
    {
        [Tooltip("List of audio clips with naming convention: CHARACTERNAME_CLIP (e.g., Pete_Attack1). The suffix will be used as the Key Identifier to pull the clip from a Dictionary.")]
        public List<AudioClip> AudioClips = new List<AudioClip>();

        protected AudioSource _audioSource;
        protected Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            // Populate the dictionary based on the naming convention
            foreach (var clip in AudioClips)
            {
                string clipName = clip.name;
                int underscoreIndex = clipName.LastIndexOf('_');
                if (underscoreIndex >= 0 && underscoreIndex < clipName.Length - 1)
                {
                    string key = clipName.Substring(underscoreIndex + 1);
                    clipDictionary[key] = clip;
                }
            }
        }

        public virtual void PlaySoundEffect(string clipKey)
        {
            if (clipDictionary.TryGetValue(clipKey, out AudioClip clip))
            {
                _audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Clip with key {clipKey} not found!");
            }
        }
    }
}
