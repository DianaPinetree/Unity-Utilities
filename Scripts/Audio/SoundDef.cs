using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SoundDef : ScriptableObject
{
    public SoundManager.SoundType type = SoundManager.SoundType.Sfx;
    public bool is3D = false;
    public bool isLoop = false;
    public bool isScene = true;
    public AudioClip[] audioClip;
    public float pitchRange = 0.0f; 
    public float volumeRange = 0.0f;

    public AudioClip GetRandomClip()
    {
        if (audioClip == null) return null;
        if (audioClip.Length == 0) return null;

        return audioClip[Random.Range(0, audioClip.Length)];
    }

    public bool HasAudioClip(AudioClip clip)
    {
        if (audioClip == null) return false;
        if (audioClip.Length == 0) return false;

        foreach (var s in audioClip)
        {
            if (s == clip) return true;
        }

        return false;
    }

#if UNITY_EDITOR
    static void ConvertToSoundDef(AudioClip snd)
    {
        SoundDef soundDef = ScriptableObject.CreateInstance<SoundDef>();
        soundDef.name = snd.name;
        soundDef.audioClip = new AudioClip[1] { snd };

        string path = AssetDatabase.GetAssetPath(snd);
        path = Path.ChangeExtension(path, "asset");

        AssetDatabase.CreateAsset(soundDef, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = soundDef;

        if (SoundDatabase.Instance != null)
        {
            SoundDatabase.Instance.sounds.Add(soundDef);
        }
        else
        {
            Debug.Log("No Sound database found: <b>" + soundDef.name + "</b> will not be added");
        }
    }

    [MenuItem("Assets/Convert/Sound Def")]
    private static void ConvertToSoundDef()
    {
        foreach (var o in Selection.objects)
        {
            ConvertToSoundDef((AudioClip)o);
        }
    }

    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/Convert/Sound Def", true)]
    private static bool NewMenuOptionValidation()
    {
        foreach (var o in Selection.objects)
        {
            if (o.GetType() != typeof(AudioClip)) return false;
        }

        return true;
    }

    private void OnDestroy() 
    {
        if (SoundDatabase.Instance != null && SoundDatabase.Instance.sounds.Contains(this))
        {
            Debug.Log("Removed: " + this.name + "from sounds database");
            SoundDatabase.Instance.sounds.Remove(this);
        }
    }
#endif
}