using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class UIAchivementsPanel : MonoBehaviour
{
    public GameObject achievementPrefab;

    private void Start()
    {
        //get achievements from the allfile from GOS
        foreach (AchievementData item in GlobalObjectScript.Instance.achievementsList)
        {
            if (!item.IsHidden) // TODO:  || (item.IsAchieved && !Settings.Instance.DisplayObtainedAchievements))
            {
                GameObject temp = Instantiate(achievementPrefab, transform.position, Quaternion.identity, this.transform);
                AchievementGraphicBuilder tt = temp.GetComponent<AchievementGraphicBuilder>();
                tt.description.text = item.description;
                tt.name.text = item.name;
                AchievementDataSO SOitem = GlobalObjectScript.Instance.firstRunAchievementsList.Find(x => x.AchievementName == item.name);
                tt.artwork.sprite = SOitem.Artwork;
                tt.progressBar.transform.localScale = new Vector3(item.progress / item.successThreshold, 1f, 1f);

                if (item.IsAchieved)
                {
                    //TODO: make panel glow or something
                    tt.name.color = Color.yellow;

                }
            }
            
        }
    }

    public void ReturnToMainMenu()
    {
        GlobalObjectScript.Instance.BackToMenuButtonClicked();
        SceneManager.LoadScene("StartMenu");
    }
}
