using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RatioButtonTutorialNotifier : MonoBehaviour
{
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(HandleClicked);
    }

    private void OnDestroy()
    {
        if (btn != null) btn.onClick.RemoveListener(HandleClicked);
    }

    private void HandleClicked()
    {
        if (TutorialManagerNew.Instance != null)
        {
            TutorialManagerNew.Instance.NotifyRatioButtonClicked();
        }
    }
    
    
}