using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AudioControl : Singleton<AudioControl>
{
    public AudioSource masterSoundAudio;
    public AudioSource musicSoundAudio;
    public AudioSource effectSoundAudio;

    public PlayableDirector director;
    public AudioTrack[] musicTracks;

    [Header("BGM")]
    public PlayableAsset ifritMusic;

    private void Awake()
    {
        ResetVolume();
    }

    public void ResetVolume()
    {
        if (PlayerPrefs.HasKey(SoundStringData.masterSound))
        {
            masterSoundAudio.volume = PlayerPrefs.GetFloat(SoundStringData.masterSound);
        }
        if (PlayerPrefs.HasKey(SoundStringData.musicSound))
        {
            musicSoundAudio.volume = PlayerPrefs.GetFloat(SoundStringData.musicSound);
        }
        if (PlayerPrefs.HasKey(SoundStringData.effectSound))
        {
            effectSoundAudio.volume = PlayerPrefs.GetFloat(SoundStringData.effectSound);
        }
    }

    public void PlaySound(AudioClip sound)
    {
        effectSoundAudio.PlayOneShot(sound);
    }

    public class TimeLine : Singleton<TimeLine>
    {
        private AudioControl control;
        private PlayableDirector director;
        private TimelineAsset asset;

        private void Awake()
        {
            control = AudioControl.Instance;
            director = AudioControl.Instance.director;
        }

        public void PlayMusic(Music music, int trackIndex)
        {
            switch (music)
            {
                case Music.Ifrit:
                    director.playableAsset = control.ifritMusic;
                    break;
                case Music.None:
                default:
                    director.playableAsset = null;
                    break;
            }

            asset = (TimelineAsset)director.playableAsset;
            director.Play();
        }

        public void MuteTrack(int trackIndex, bool mute)
        {
            if (asset == null)
                return;
            // Get track from TimelineAsset
            TrackAsset track = asset.GetOutputTrack(trackIndex);

            // Change TimelineAsset's muted property value
            track.muted = mute;

            double t = director.time; // Store elapsed time
            director.RebuildGraph(); // Rebuild graph
            director.time = t; // Restore elapsed time
        }
    }
}

public enum Music
{
    None,
    Ifrit,
}
