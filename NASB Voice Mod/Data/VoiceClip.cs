using UnityEngine;
using VoiceMod.Managers;
using Nick;

namespace VoiceMod.Data
{
    [System.Serializable]
    public class Voiceclip
    {
        public string id;
        public string path;
        public float volume = 1f;

        public AudioClip clip;

        public void Play()
        {
            var soundOptions = OptionsSystem.Instance.Sound;

            VoicepackManager.Instance.PlayClip(clip, volume * soundOptions.SoundVolume * soundOptions.MasterVolume);
        }
    }
}
