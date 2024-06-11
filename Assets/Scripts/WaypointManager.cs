using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoSingleton<WaypointManager>
{
    private bool highlightPath = false;
    private Vector3 target;
    [SerializeField]
    private GameObject PathTrailPrefab;
    private float TrailSpeed = 5f;
    public List<Vector2S> Points { get; set; }
    private int waypointIndex = 0;
    private GameObject PathTrail;
    public GameObject PathLineOuter;
    public GameObject ExitWaypoint;
    public GameObject EntranceWaypoint;
    public GameObject WaypointPrefab;
    private Vector4 ExitHDRColor = new Vector4(32, 0, 0, 0);
    private Vector4 EntranceHDRColor = new Vector4(0, 32, 0, 0);

    void Start()
    {
        PathTrail = Instantiate(PathTrailPrefab, Vector3.zero, Quaternion.identity);
        PathTrail.SetActive(true);
        ResetPathTrailHighlightToStart();
    }

    public void PlacePoints()
    {
        //Remove any pre-existing line 
        foreach (Transform child in this.transform) Destroy(child.gameObject);

        //Copy Points from the levelmanager and lay them outl special treatment for 1st and last>entrance and exit
        Points = LevelManager.Instance.CurrentLevel.PathPoints;
        GameObject tempEntrance = Instantiate(EntranceWaypoint, Points[0].ToVector3(), Quaternion.identity, this.transform);
        for (int i = 0; i < Points.Count; i++)
        {
            if (i>0 && i < Points.Count) 
                Instantiate(WaypointPrefab, Points[i].ToVector3(), Quaternion.identity, this.transform);
        }
        GameObject tempExit = Instantiate(ExitWaypoint, Points[Points.Count-1].ToVector3(), Quaternion.identity, this.transform);

        //Rotate Entrance and Exit to point towards their neighbour waypoint
        Vector3 entranceDelta = Points[1].ToVector3() - tempEntrance.transform.position;
        float rotZ = Mathf.Atan2(entranceDelta.y, entranceDelta.x) * Mathf.Rad2Deg;
        tempEntrance.transform.rotation = Quaternion.Euler(0, 0, rotZ);
        Vector3 exitDelta = Points[Points.Count-2].ToVector3() - tempExit.transform.position;
        float rotZa = Mathf.Atan2(exitDelta.y, exitDelta.x) * Mathf.Rad2Deg;
        tempExit.transform.rotation = Quaternion.Euler(0, 0, rotZa);

        //Change the exit/entrance colours
        tempEntrance.GetComponentInChildren<SpriteRenderer>().material.SetColor("Color_FB31EF4", EntranceHDRColor);
        tempExit.GetComponentInChildren<SpriteRenderer>().material.SetColor("Color_FB31EF4", ExitHDRColor);

        //Due to the LateUpdate issue, we drawpath then do it again a little later to fool the renderer. Not great, but this is only called once so no cost
        DrawPath();
        Invoke("DrawPath", 0.5f);
    }

    void DrawPath()
    {
        DrawLineOnPath(PathLineOuter);
        PathLineOuter.SetActive(true);
    }

    public void HighlightPath()
    {
        ResetPathTrailHighlightToStart();
        target = Points[0].ToVector3();
        PathTrail.transform.position = target;
        highlightPath = true;
        PathTrail.SetActive(true);
    }

    private void Update()
    {
        if (highlightPath) MoveTrail();
    }

    private void MoveTrail()
    {
        Vector3 dir = target - PathTrail.transform.position;
        float distThisFrame = TrailSpeed * Time.deltaTime;
        dir.Normalize();
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        PathTrail.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        PathTrail.transform.Translate(dir.normalized * distThisFrame, Space.World);
        if (Vector3.Distance(PathTrail.transform.position, target) <= dir.normalized.magnitude * TrailSpeed * Time.deltaTime)
        {
            GetNextWayPoint();
        }
    }

    void ResetPathTrailHighlightToStart()
    {
        highlightPath = false;
        PathTrail.SetActive(false); //TODO how to delay this by the TRAIL lifetime
        Destroy(PathTrail.gameObject);
        PathTrail = Instantiate(PathTrailPrefab, Vector3.zero, Quaternion.identity);
        waypointIndex = 0;
    }

    void DrawLineOnPath(GameObject linePrefab)
    {
        LineRenderer lineRenderer = linePrefab.GetComponent<LineRenderer>();
        lineRenderer.positionCount = Points.Count;

        //set up a wee array to hold the points
        Vector3[] point = new Vector3[Points.Count];
        for (int i = 0; i < Points.Count; i++)
        {
            point[i] = Points[i].ToVector3();            
        }
        lineRenderer.SetPositions(point);

        lineRenderer.enabled = true;
        lineRenderer.loop = false;
    }

    void GetNextWayPoint()
    {
        if (waypointIndex >= Points.Count - 1)
        {
            ResetPathTrailHighlightToStart();
            return;
        }
        waypointIndex++;
        target = Points[waypointIndex].ToVector3();
    }
}
