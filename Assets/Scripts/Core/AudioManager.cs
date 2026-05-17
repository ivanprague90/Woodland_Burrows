// ============================================================
//  AudioManager.cs — Woodland Burrow
//  Singleton per la gestione di musica ambientale e SFX.
//  Assegna le AudioClip nell'Inspector; i metodi Play* possono
//  essere chiamati da qualsiasi script senza dipendenze.
// ============================================================

using UnityEngine;

namespace WoodlandBurrow
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Musica Ambientale")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip   ambientMusic;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.4f;

        [Header("SFX")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip   tapClip;
        [SerializeField] private AudioClip   upgradeClip;
        [SerializeField] private AudioClip   zoneUnlockClip;
        [SerializeField] private AudioClip   offlineBadgeClip;
        [SerializeField] private AudioClip   prestigeClip;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.8f;

        private const string MusicMutedKey = "WB_MusicMuted";
        private const string SfxMutedKey   = "WB_SfxMuted";

        // --------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (musicSource == null || ambientMusic == null) return;

            musicSource.clip   = ambientMusic;
            musicSource.loop   = true;
            musicSource.volume = musicVolume;
            musicSource.mute   = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;

            if (sfxSource != null)
                sfxSource.mute = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;

            musicSource.Play();
        }

        // --------------------------------------------------------
        //  SFX pubblici — chiamabili da qualsiasi script
        // --------------------------------------------------------

        public void PlayTap()          => PlaySFX(tapClip);
        public void PlayUpgrade()      => PlaySFX(upgradeClip);
        public void PlayZoneUnlock()   => PlaySFX(zoneUnlockClip);
        public void PlayOfflineBadge() => PlaySFX(offlineBadgeClip);
        public void PlayPrestige()     => PlaySFX(prestigeClip);

        // --------------------------------------------------------
        //  Controllo volume / mute
        // --------------------------------------------------------

        public void SetMusicVolume(float v)
        {
            musicVolume = v;
            if (musicSource != null) musicSource.volume = v;
        }

        public void ToggleMusic()
        {
            if (musicSource == null) return;
            musicSource.mute = !musicSource.mute;
            PlayerPrefs.SetInt(MusicMutedKey, musicSource.mute ? 1 : 0);
        }

        public void ToggleSfx()
        {
            if (sfxSource == null) return;
            sfxSource.mute = !sfxSource.mute;
            PlayerPrefs.SetInt(SfxMutedKey, sfxSource.mute ? 1 : 0);
        }

        public bool IsMusicMuted => musicSource != null && musicSource.mute;
        public bool IsSfxMuted   => sfxSource   != null && sfxSource.mute;

        // --------------------------------------------------------

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null || sfxSource.mute) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
