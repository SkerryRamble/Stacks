using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement")]
public class AchievementDataSO : ScriptableObject
{
    [SerializeField] private string achievementName;
    [SerializeField] private string description;
    [SerializeField] private Sprite artwork;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private bool isHidden = false;
    [SerializeField] private bool isAchieved = false;
    [SerializeField] private int reward = 1; //how many perks to give
    [SerializeField] private float progress = 0f;  //stat to increase
    [SerializeField] private float successThreshold;
    [SerializeField] private string statToMeasure;

    public string StatToMeasure { get => statToMeasure; set => statToMeasure = value; }
    public float SuccessThreshold { get => successThreshold; set => successThreshold = value; }
    public float Progress { get => progress; set => progress = value; }
    public int Reward { get => reward; set => reward = value; }
    public bool IsHidden { get => isHidden; set => isHidden = value; }
    public bool IsAchieved { get => isAchieved; set => isAchieved = value; }
    public bool IsLocked { get => isLocked; set => isLocked = value; }
    public Sprite Artwork { get => artwork; set => artwork = value; }
    public string Description { get => description; set => description = value; }
    public string AchievementName { get => achievementName; set => achievementName = value; }
}
