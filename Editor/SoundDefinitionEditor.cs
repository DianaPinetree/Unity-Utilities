using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FP.Audio
{
    [CustomEditor(typeof(SoundDef))]
    public class SoundDefinitionEditor : Editor
    {
        // Preview debug audio source
        [SerializeField] private AudioSource previewSource;


        // When entering editing in this SO, create special hidden audio source
        private void OnEnable()
        {
            previewSource = EditorUtility.CreateGameObjectWithHideFlags("Audio Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        // Destroy audio source when editing as ended
        private void OnDisable()
        {
            DestroyImmediate(previewSource);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            {
                if (GUILayout.Button("Preview Audio"))
                {
                    PlaySoundDef((SoundDef)target);
                }

                if (((SoundDef)target).isLoop)
                {
                    if (GUILayout.Button("Stop"))
                    {
                        previewSource.Stop();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void PlaySoundDef(SoundDef audio)
        {
            previewSource.clip = audio.GetRandomClip();
            previewSource.pitch = Mathf.Clamp(1f + UnityEngine.Random.Range(-audio.pitchRange, audio.pitchRange), 0.0f, 2.0f);
            previewSource.volume = Mathf.Clamp(1f + UnityEngine.Random.Range(-audio.volumeRange, audio.volumeRange), 0.0f, 2.0f);
            previewSource.loop = audio.isLoop;
            previewSource.Play();
        }
    }
}