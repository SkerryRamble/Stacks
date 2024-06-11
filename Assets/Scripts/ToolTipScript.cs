using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class ToolTipScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //simple class to activate the BODY of the tooltip when mouse over, and deactivate when not
    public GameObject BodyText;
    //private Animator fadeout;
    void Start()
    {
        //fadeout = this.GetComponent<Animator>();
        //fadeout.StopPlayback();
        BodyText.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BodyText.SetActive(true);    
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        Invoke("DeactivateThis", 1f);
    }

    public void DeactivateThis()
    {
       // fadeout.SetTrigger("Fade");
        BodyText.SetActive(false);
    }
}
