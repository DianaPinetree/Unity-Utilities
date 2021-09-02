using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SoundManager : MonoBehaviour
{
    public enum SoundType { Main, Music, Sfx, Sfx2, Voice, Ambient };

    public AudioMixerGroup mainMixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup sfx2Mixer;
    public AudioMixerGroup voiceMixer;
    public AudioMixerGroup ambientMixer;
    public SoundDatabase database;

    class Sound
    {
        public AudioSource audioSource;
        public bool sceneSound;
        public Coroutine fxThread;
        public SoundDef currentSound;
    }

    Dictionary<string, SoundDef> soundsByName;
    Dictionary<AudioSource, Sound> audioSources;
    Dictionary<SoundType, bool> enableSounds;

    static SoundManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        soundsByName = new Dictionary<string, SoundDef>();

        foreach (var s in database.sounds)
        {
            soundsByName.Add(s.name, s);
        }

        audioSources = new Dictionary<AudioSource, Sound>();
        enableSounds = new Dictionary<SoundType, bool>();
        foreach (var e in Enum.GetValues(typeof(SoundType)))
        {
            enableSounds.Add((SoundType)e, true);
        }

        SceneManager.sceneLoaded += StopAllSceneSounds;

    }

    private void StopAllSceneSounds(Scene arg0, LoadSceneMode arg1)
    {
        foreach (var s in audioSources)
        {
            if (s.Value.sceneSound)
            {
                s.Value.audioSource.Stop();
            }
        }
    }

    void _StopAllSounds()
    {
        foreach (var e in Enum.GetValues(typeof(SoundType)))
        {
            StopAllSounds((SoundType)e);
        }
    }

    void UpdateSoundChannelState()
    {
        foreach (var e in Enum.GetValues(typeof(SoundType)))
        {
            if (!enableSounds[(SoundType)e])
            {
                StopAllSounds((SoundType)e);
            }
        }
    }

    void StopAllSounds(SoundType t)
    {
        foreach (var sounds in audioSources.Values)
        {
            if ((sounds.audioSource.isPlaying) &&
                (sounds.currentSound != null) &&
                (sounds.currentSound.type == t))
            {
                sounds.audioSource.Stop();
                sounds.currentSound = null;
                if (sounds.fxThread != null) StopCoroutine(sounds.fxThread);
            }
        }
    }

    AudioSource _Play(string name, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        SoundDef soundDef;

        if (soundsByName.TryGetValue(name, out soundDef))
        {
            _Play(soundDef, position, volume, pitch);
        }
        else
        {
            Debug.LogWarning("Unknown sound " + name + "!");
        }

        return null;
    }

    AudioSource _Play(string name, float volume = 1.0f, float pitch = 1.0f)
    {
        SoundDef soundDef;

        if (soundsByName.TryGetValue(name, out soundDef))
        {
            _Play(soundDef, volume, pitch);
        }
        else
        {
            Debug.LogWarning("Unknown sound " + name + "!");
        }

        return null;
    }

    AudioSource _Play(SoundDef soundDef, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!enableSounds[soundDef.type]) return null;

        AudioSource audioSource = _Play(soundDef, volume, pitch);
        if (audioSource == null)
        {
            Debug.LogWarning("No sound channel available for sounds " + name);
            return null;
        }

        audioSource.transform.position = position;

        return audioSource;
    }

    AudioSource _Play(SoundDef soundDef, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!enableSounds[soundDef.type]) return null;

        AudioMixerGroup mixer = GetMixer(soundDef.type);
        if (mixer == null)
        {
            Debug.LogWarning("No mixer for sounds of type " + soundDef.type);
            return null;
        }
        AudioSource audioSource = GetSound(mixer, soundDef.is3D, soundDef.isScene);
        if (audioSource == null)
        {
            Debug.LogWarning("No sound channel available for sounds " + name);
            return null;
        }

        audioSource.clip = soundDef.GetRandomClip();
        audioSource.pitch = Mathf.Clamp(pitch + UnityEngine.Random.Range(-soundDef.pitchRange, soundDef.pitchRange), 0.0f, 2.0f);
        audioSource.volume = Mathf.Clamp(volume + UnityEngine.Random.Range(-soundDef.volumeRange, soundDef.volumeRange), 0.0f, 2.0f);
        audioSource.loop = soundDef.isLoop;
        audioSource.Play();

        Sound snd;
        if (audioSources.TryGetValue(audioSource, out snd))
        {
            if (snd.fxThread != null)
            {
                StopCoroutine(snd.fxThread);
                snd.fxThread = null;
            }
            snd.currentSound = soundDef;
        }

        return audioSource;
    }

    public void _SetVolume(SoundType type, float volume)
    {
        AudioMixerGroup grp = GetMixer(type);
        if (grp != null)
        {
            grp.audioMixer.SetFloat("Volume", Mathf.Clamp(20.0f * Mathf.Log10(volume), -80.0f, 20.0f));
        }
    }

    public void _EnableSounds(SoundType type, bool b)
    {
        StopAllSounds(type);
        if (enableSounds.ContainsKey(type))
        {
            enableSounds[type] = b;
        }
        else
        {
            enableSounds.Add(type, b);
        }
    }


    public float _GetVolume(SoundType type)
    {
        AudioMixerGroup grp = GetMixer(type);
        if (grp != null)
        {
            float val;
            grp.audioMixer.GetFloat("Volume", out val);

            return Mathf.Clamp01(Mathf.Pow(10.0f, val / 20.0f));
        }

        return 0.0f;
    }

    AudioMixerGroup GetMixer(SoundType type)
    {
        switch (type)
        {
            case SoundType.Main:
                return mainMixer;
            case SoundType.Music:
                return musicMixer;
            case SoundType.Sfx:
                return sfxMixer;
            case SoundType.Voice:
                return voiceMixer;
            case SoundType.Sfx2:
                return sfx2Mixer;
            case SoundType.Ambient:
                return ambientMixer;
        }

        return null;
    }

    AudioSource GetSound(AudioMixerGroup grp, bool is3d, bool isScene)
    {
        foreach (var s in audioSources.Values)
        {
            if (!s.audioSource.isPlaying)
            {
                if (s.audioSource.outputAudioMixerGroup == grp)
                {
                    if ((is3d) && (s.audioSource.spatialBlend == 0.0f)) continue;
                    if ((!is3d) && (s.audioSource.spatialBlend == 1.0f)) continue;
                    return s.audioSource;
                }
            }
        }

        GameObject go = new GameObject();
        go.transform.parent = transform;
        go.name = grp.name;
        AudioSource src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = grp;
        src.spatialBlend = (is3d) ? (1.0f) : (0.0f);
        src.playOnAwake = false;

        Sound snd = new Sound();
        snd.audioSource = src;
        snd.sceneSound = isScene;
        snd.fxThread = null;

        audioSources.Add(src, snd);

        return src;
    }

    void VolumeLerp(AudioSource audioSource, float startVolume, float endVolume, float time)
    {
        Sound snd = audioSources[audioSource];
        snd.fxThread = StartCoroutine(VolumeLerpCR(audioSource, startVolume, endVolume, time));
    }

    AudioSource _CrossFade(AudioSource audioSource, SoundDef sound, float volume, float time)
    {
        if (audioSource == null)
        {
            audioSource = Play(sound, 0.0f, 1.0f);
        }

        Sound snd = audioSources[audioSource];
        snd.fxThread = StartCoroutine(CrossFadeCR(audioSource, sound, volume, time));

        return audioSource;
    }

    IEnumerator VolumeLerpCR(AudioSource audioSource, float startVolume, float endVolume, float time)
    {
        audioSource.volume = startVolume;
        float inc = (endVolume - startVolume) / time;
        float timeElapse = 0.0f;

        while (timeElapse < time)
        {
            yield return null;

            audioSource.volume += inc * Time.deltaTime;

            timeElapse += Time.deltaTime;
        }

        audioSource.volume = endVolume;

        Sound snd = audioSources[audioSource];
        snd.fxThread = null;
    }

    IEnumerator CrossFadeCR(AudioSource audioSource, SoundDef sound, float targetVolume, float time)
    {
        float inc, timeElapse;

        if (audioSource.volume > 0.0f)
        {
            // Fade out
            inc = (0.0f - targetVolume) / time;
            timeElapse = 0.0f;

            while (timeElapse < time)
            {
                yield return null;

                audioSource.volume += inc * Time.deltaTime;

                timeElapse += Time.deltaTime;
            }
        }

        audioSource.clip = sound.GetRandomClip();
        audioSource.volume = 0.0f;
        audioSource.Play();

        inc = targetVolume / time;
        timeElapse = 0.0f;

        while (timeElapse < time)
        {
            yield return null;

            audioSource.volume += inc * Time.deltaTime;

            timeElapse += Time.deltaTime;
        }

        audioSource.volume = targetVolume;

        Sound snd = audioSources[audioSource];
        snd.fxThread = null;
    }

    public static AudioSource Play(string name, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        return instance._Play(name, volume, pitch);
    }

    public static AudioSource Play(SoundDef snd, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        return instance._Play(snd, volume, pitch);
    }


    public static AudioSource Play(string name, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        return instance._Play(name, position, volume, pitch);
    }


    public static AudioSource Play(SoundDef snd, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        return instance._Play(snd, position, volume, pitch);
    }

    public static AudioSource FadeIn(string soundName, Vector3 position, float time, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        AudioSource audio = instance._Play(soundName, position, volume, pitch);
        if (audio == null) return null;

        instance.VolumeLerp(audio, 0.0f, volume, time);

        return audio;
    }

    public static AudioSource FadeIn(string soundName, float time, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        AudioSource audio = instance._Play(soundName, volume, pitch);
        if (audio == null) return null;

        instance.VolumeLerp(audio, 0.0f, volume, time);

        return audio;
    }


    public static AudioSource FadeIn(SoundDef snd, Vector3 position, float time, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        AudioSource audio = instance._Play(snd, position, volume, pitch);
        if (audio == null) return null;

        instance.VolumeLerp(audio, 0.0f, volume, time);

        return audio;
    }

    public static AudioSource FadeIn(SoundDef snd, float time, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        AudioSource audio = instance._Play(snd, volume, pitch);
        if (audio == null) return null;

        instance.VolumeLerp(audio, 0.0f, volume, time);

        return audio;
    }

    public static void SetVolume(SoundType type, float volume)
    {
        if (instance == null) return;

        instance._SetVolume(type, volume);
    }

    public static float GetVolume(SoundType type)
    {
        if (instance == null) return 0.0f;

        return instance._GetVolume(type);
    }

    public static void EnableSounds(SoundType type, bool b)
    {
        if (instance == null) return;

        instance._EnableSounds(type, b);
    }

    public static bool IsPlaying(AudioSource audioSource, SoundDef sound)
    {
        if (audioSource == null) return false;
        if (!audioSource.isPlaying) return false;

        if (sound.HasAudioClip(audioSource.clip))
        {
            return true;
        }

        return false;
    }

    //Addition by FIL
    public static bool IsPlaying( string name )
    {
        return instance._IsPlaying( name );
    }

    public bool _IsPlaying( string name )
    {
        SoundDef soundDef;

        if (soundsByName.TryGetValue(name, out soundDef))
        {
            foreach( AudioSource source in audioSources.Keys )
            {
                foreach( AudioClip c in soundDef.audioClip )
                {
                    if( source.clip == c && source.isPlaying ) return true;
                }                
            }
        }

        return false;
    }

    public static AudioSource CrossFade(AudioSource audioSource, SoundDef sound, float time, float volume = 1.0f)
    {
        if (instance == null) return null;

        return instance._CrossFade(audioSource, sound, time, volume);
    }

    public static void FadeOut(string soundName, float time)
    {
        if (instance == null) return;

        AudioSource audioSource = null;
        SoundDef soundDef;
        if (instance.soundsByName.TryGetValue(soundName, out soundDef))
        {
            foreach (var s in instance.audioSources.Values)
            {
                if (s.audioSource.isPlaying)
                {
                    foreach (AudioClip clip in soundDef.audioClip)
                    {
                        if( clip == s.audioSource.clip )
                        {
                            audioSource = s.audioSource;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Unknown sound " + soundName + "!");
        }
        if (audioSource == null) return;

        instance.VolumeLerp(audioSource, audioSource.volume, 0.0f, time);
    }

    public static void FadeOut(AudioSource audioSource, float time)
    {
        if (instance == null) return;

        instance.VolumeLerp(audioSource, audioSource.volume, 0.0f, time);
    }

    public static void StopAllSounds()
    {
        if (instance == null) return;

        instance._StopAllSounds();
    }
}