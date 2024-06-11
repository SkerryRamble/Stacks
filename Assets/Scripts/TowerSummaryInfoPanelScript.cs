using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSummaryInfoPanelScript : MonoSingleton<TowerSummaryInfoPanelScript>, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool mouseIsOver = false;
    public bool OpenedOnPurpose = false;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
        OpenedOnPurpose = false;
        //Debug.Log("Enabled");
    }

    public void SetSelected()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
        OpenedOnPurpose = false;
        //Debug.Log("set selected");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //Close the Window on Deselect only if a click occurred outside this panel
        if (!mouseIsOver && !OpenedOnPurpose)   //detect if just opened from clicking a tower
        {
            //Debug.Log("mouse not over and not opened on purpose");
            this.gameObject.SetActive(false);

        }
        else
        {
            OpenedOnPurpose = false;
            //Debug.Log("either mouse over or was opened on purpose");
            //need some way to reselect the object here
            Invoke("SetSelected", 0.1f);    //try to reselect this object just after its deselect has finished. it may be the 
            //case that we just clicked a towerbase and the uipanel appeared but we want to clicke it off immediately without
            //triggering either the pointerenter exit events
            //catch is we cant reselect an object inside its deselect method, so we add a little delay
            //not happy with this but it's taking too long and this seems to work as i need
        }
    }

   

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseIsOver = true;
        OpenedOnPurpose = false;
        EventSystem.current.SetSelectedGameObject(gameObject);
        //Debug.Log("mouse enter set selected");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseIsOver = false;
        OpenedOnPurpose = false;
        EventSystem.current.SetSelectedGameObject(gameObject);
        //Debug.Log("mouse EXIT set selected");
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

}
