using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoSingleton<BuildManager>
{
    //Need to arrange the order of script execution in the project settings
    // BuildManager
    // GameManager
    // SpawnManager

    [SerializeField] private StaticEnums.BuildMode buildMode;
    public StaticEnums.BuildMode BuildMode
    {
        get { return buildMode; }
        set { buildMode = value; }
    }

    [SerializeField]
    //private GameObject[] _towers;
    //public GameObject[] Towers { get => _towers; set => _towers = value; }

    private GameObject towerToBuild;

    public override void Init()
    {        
        buildMode = StaticEnums.BuildMode.Nothing;
    }

    public GameObject GetTowerToBuild() { return towerToBuild; }

    public void SetTowerToBuild(GameObject tower)
    {
        towerToBuild = tower;
        buildMode = StaticEnums.BuildMode.Building;        
    }

    public void SellTower()
    {
        buildMode = StaticEnums.BuildMode.Selling;
    }

    public void UpgradeTower()
    {
        buildMode = StaticEnums.BuildMode.Upgrading;
    }

    
}
