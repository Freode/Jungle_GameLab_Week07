using UnityEngine;

public class FinishGameButtonObserver : MonoBehaviour
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
