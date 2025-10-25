using UnityEngine;

public class TempButton : MonoBehaviour
{
    public FinishGame OnFinishGameEffect;
    
    private void Start()
    {
        OnFinishGameEffect.OnEvent += SetButtonActive;
    }

    private void OnDestroy()
    {
        OnFinishGameEffect.OnEvent -= SetButtonActive;
    }

    private void SetButtonActive()
    {
	
    }
}
