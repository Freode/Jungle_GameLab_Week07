using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Out Float Event Channel")]
public class OutFloatEventChannelSO : ScriptableObject
{
    public Func<float> OnEventRaised;

    public float RaiseEvent()
    {
        return OnEventRaised.Invoke();
    }
}
