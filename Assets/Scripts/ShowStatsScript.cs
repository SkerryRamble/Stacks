using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Reflection;

public class ShowStatsScript : MonoSingleton<ShowStatsScript>
{
    public GameObject statPrefab;
    public GameObject warningPrefab;

    private void Start()
    {
        PopulateDataDisplay();
    }

    private void PopulateDataDisplay() 
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }


        //get achievements from the allfile from GOS
        GameStatistics gs = GlobalObjectScript.Instance.GlobalStats;


        foreach (FieldInfo prop in typeof(GameStatistics).GetFields())
        {
            GameObject GO = Instantiate(statPrefab, transform.position, Quaternion.identity, this.transform);
            StatCellBuilder scb = GO.GetComponent<StatCellBuilder>();
            scb.ID.text = prop.Name;
            scb.value.text = prop.GetValue(gs).ToString();            
        }
    }

    public void AreYouSure()
    {
        warningPrefab.SetActive(true);
    }

    public void WasNotSure()
    {
        warningPrefab.SetActive(false);
    }
    

    public void ResetStats()
    {
        warningPrefab.SetActive(false);
        GlobalObjectScript.Instance.ResetStats();
        PopulateDataDisplay();
    }

    public void ReturnToMainMenu()
    {
        GlobalObjectScript.Instance.BackToMenuButtonClicked();
        SceneManager.LoadScene("StartMenu");
    }
}
