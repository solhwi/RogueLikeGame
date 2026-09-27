using UnityEngine;
using RogueLike.Core;

namespace RogueLike.Managers
{
    public class AudioManager : Singleton<AudioManager>
    {
        private const string SfxVolumeKey = "audio_sfx_volume";
        private const string BgmVolumeKey = "audio_bgm_volume";

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        public float SfxVolume { get; private set; } = 1f;
        public float BgmVolume { get; private set; } = 1f;

        protected override void Awake()
        {
            base.Awake();

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            musicSource.volume = BgmVolume;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }
            sfxSource.PlayOneShot(clip, SfxVolume);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || musicSource.clip == clip)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = BgmVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        }

        public void SetBgmVolume(float volume)
        {
            BgmVolume = Mathf.Clamp01(volume);
            musicSource.volume = BgmVolume;
            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
        }
    }
}
