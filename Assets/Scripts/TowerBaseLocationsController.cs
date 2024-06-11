using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBaseLocationsController : MonoSingleton<TowerBaseLocationsController>
{
    public GameObject TowerBasePrefab;
    private List<Vector2S> _points;
    public List<Vector2S> Points { get => _points; set => _points = value; }

    public void PlacePoints()
    {

        //get rid of the existing waypoints
        if (this.transform.childCount > 0)
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Transform temp = this.transform.GetChild(i);
                Destroy(temp.gameObject);
            }
        }

        //LevelManager.Instance.LoadLevelLocations();//TODO: ensure we're getting the correct levelid to load the waypoints in
        //_points = new Vector2S[LevelManager.Instance.CurrentLevel.PathPoints.Count];

        _points = LevelManager.Instance.CurrentLevel.TowerBasePoints;

        for (int i = 0; i < _points.Count; i++)
        {
            Instantiate(TowerBasePrefab, _points[i].ToVector3(), Quaternion.identity, this.transform);
        }
        
    }




}
