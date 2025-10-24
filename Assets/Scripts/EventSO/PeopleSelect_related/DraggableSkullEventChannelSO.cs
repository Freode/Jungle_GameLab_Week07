using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/DraggableSkull Event Channel")]
public class DraggableSkullEventChannelSO : ScriptableObject
{
    public UnityAction<DraggableSkull> OnEventRaised;

    public void RaiseEvent(DraggableSkull skull)
    {
        OnEventRaised?.Invoke(skull);
    }
}