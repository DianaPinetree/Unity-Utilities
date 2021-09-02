using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Database")]
public class SoundDatabase : SingletonSO<SoundDatabase>
{
    public List<SoundDef> sounds = new List<SoundDef>();
}
