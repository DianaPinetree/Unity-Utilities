using UnityEngine;
public class MinMaxSliderAttribute : PropertyAttribute
{
    public float _min, _max;
    public float Min => _min;
    public float Max => _max;
    public MinMaxSliderAttribute(float min, float max)
    {
        _min = min;
        _max = max;
    }
}