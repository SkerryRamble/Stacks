using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementPopupController : MonoBehaviour
{
    //TODO: make this a list as we might score multiple achievements at once and we want the player to be aware of each 
    //also maybe inlcude a summary of this stuff at game end

    public Image AchievementIcon;
    public TextMeshProUGUI AchievementText;

    Animator anim;

    void Awake()
    {
        PlayerHistory.AchievementGranted += Popup;

        anim = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        PlayerHistory.AchievementGranted -= Popup;
    }

    void Popup(AchievementData item)
    {
        //anim.StopPlayback();
        AchievementText.text = item.name;
        //we want the artwork, stored in the SO version of the achievementdata
        AchievementDataSO SOitem = GlobalObjectScript.Instance.firstRunAchievementsList.Find(x => x.AchievementName == item.name);


        AchievementIcon.sprite = SOitem.Artwork;
        anim.SetTrigger("Popup");
    }

    
}
