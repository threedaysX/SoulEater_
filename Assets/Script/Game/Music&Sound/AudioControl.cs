using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
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

    public class FMOD : Singleton<FMOD>
    {
        #region Basic Settings.
        private static Bus Master;
        private static Bus Bgm;
        private Dictionary<string, EventInstance> audioEvents = new Dictionary<string, EventInstance>();
        #endregion

        private const string eventPrefix = "event:/";
        private const string Promenence_Ifrit_EventName = "Music/Prominence/Boss/Ifrit";
        private const string LionArd_Theme_EventName = "Music/LionArd/Theme";

        private void Awake()
        {
            Master = RuntimeManager.GetBus("bus:/Master");
            Bgm = RuntimeManager.GetBus("bus:/Master/Music");
        }

        public void Startup(Music music, bool reset = false)
        {
            string eventName = GetMusicPath(music);
            if (reset)
            {
                Release(music);
                audioEvents.Add(eventName, RuntimeManager.CreateInstance(eventName));
            }
            else
            {
                if (!audioEvents.ContainsKey(eventName))
                    audioEvents.Add(eventName, RuntimeManager.CreateInstance(eventName));
            }

            audioEvents[eventName].start();
        }

        public void Setup(Music music, string parameterName, float value)
        {
            string eventName = GetMusicPath(music);
            if (!audioEvents.ContainsKey(eventName))
                return;

            var desc = RuntimeManager.GetEventDescription(eventName);
            desc.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION pd);
            audioEvents[eventName].setParameterByID(pd.id, value);
        }

        public void Release(Music music)
        {
            string eventName = GetMusicPath(music);
            audioEvents[eventName].release();
            audioEvents.Remove(eventName);
        }

        public string GetMusicPath(Music music)
        {
            string name = "";
            switch (music)
            {
                case Music.Ifrit:
                    name = Promenence_Ifrit_EventName;
                    break;
                case Music.None:
                default:
                    break;
            }
            return eventPrefix + name;
        }

        public void AdjustMasterVolume(float volume)
        {
            Master.setVolume(volume);
        }

        public void AdjustBgmVolume(float volume) 
        {
            Bgm.setVolume(volume);
        }
    }
}

public enum Music
{
    None,
    Ifrit,
}

