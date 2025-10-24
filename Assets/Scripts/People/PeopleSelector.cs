using UnityEngine;
using UnityEngine.EventSystems;

public class PeopleSelector : MonoBehaviour
{
    public PeopleActorEventChannelSO OnPeopleSelectedChannel;
    public VoidEventChannelSO OnDeselectedChannel;
    public DraggableSkullEventChannelSO OnSkullSelectedChannel;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 폐하의 왼손 손짓을 감시하는 임무는 그대로 유지하되,
        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            RaycastHit2D hit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(Input.mousePosition));
            
            if (hit.collider != null)
            {
                // ★★★ "살아있는 백성에 대한 월권 행위를 멈추라"는 어명을 내립니다. ★★★
                // 아래의 구절을 삭제하거나 주석 처리하여 권한을 박탈하시옵소서.
                /*
                // 1. 살아있는 백성을 먼저 확인
                PeopleActor actor = hit.collider.GetComponent<PeopleActor>();
                if (actor != null)
                {
                    OnPeopleSelectedChannel.RaiseEvent(actor);
                    return; // 보고했으니 임무 종료
                }
                */

                // 2. 백성이 아니라면, 유골인지 확인 (이 임무는 그대로 유지합니다)
                DraggableSkull skull = hit.collider.GetComponent<DraggableSkull>();
                if (skull != null)
                {
                    OnSkullSelectedChannel.RaiseEvent(skull);
                    return; 
                }
            }
            
            // 3. 아무것도 해당되지 않으면 선택 해제 방송 (이 임무 또한 유지합니다)
            OnDeselectedChannel.RaiseEvent();
        }
    }
}