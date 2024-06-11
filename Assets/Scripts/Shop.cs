using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    //BuildManager buildManager;




    //void Start()
    //{
    //    buildManager = BuildManager.Instance;
    //}

    ////TODO work out how to make a PurchaseTOWER generic method to allow for the tower array being expandable
    ////for now we hard code the tower elements [0] and [1]

    //public void PurchaseStandardTurret()
    //{
    //    buildManager.SetTowerToBuild(buildManager.Towers[0]);
    //    //TODO expand button a bit to make it clear its our choice > probably better to access button script and do it there
    //    //var d = this.transform.childCount;
    //    //var e = this.transform.GetChild(0).name;
    //    //transform.GetChild(0).transform.localScale = new Vector3(3, 3, 1);
    //    //Debug.Log("Set to build a bullettower : " + this + " : " + this.GetComponentInChildren<Transform>().name);
    //}

    //public void PurchaseMissileTurret()
    //{
    //    buildManager.SetTowerToBuild(buildManager.Towers[1]);
    //}

    //public void SellTower()
    //{
    //    //TODO no ideas for this yet
    //    //maybe refund cash percentage and return to buildablesprite tile method...
    //}
}
