using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject Source;
    public GameObject Target_1;
    public GameObject Target_2;
    public GameObject smokeTrail;
    public float speed = 100;
    public float waitFor = 0.1f;
    private Vector3 directionOfTravel2;
    private Vector3 directionOfTravel1;
    bool isFiring1 = false;
    bool isFiring2 = false;
    private GameObject temp1;
    private GameObject temp2;
    float distTotal;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && temp1 == null)
        {
            //directionOfTravel1 =  (Target_1.transform.position - Source.transform.position);
            //directionOfTravel2 = (Target_2.transform.position - Source.transform.position);

            //Fire2();
            //Fire1();

            //linerendereder thing
            doline();
        }

        //if(isFiring1) movebull1();
        //if (isFiring2) movebull2();
    }


    void doline()
    {
        
        temp1 = Instantiate(smokeTrail, Source.transform.position, Quaternion.identity, this.transform);
        LineRenderer lr = temp1.GetComponent<LineRenderer>();
        Vector3[] pos = { Source.transform.position, Target_1.transform.position };
        lr.SetPositions(pos);
        Destroy(temp1.gameObject, 2f);
    }

    void Fire2()
    {
        //Debug.DrawLine(Source.transform.position, Target_1.transform.position);
        temp2 = Instantiate(smokeTrail, Source.transform.position, Quaternion.identity, this.transform);
        var t =temp2.GetComponent<ParticleSystem>();
        
        isFiring2 = true;
    }

    void Fire1()
    {
        temp1 = Instantiate(smokeTrail, Source.transform.position, Quaternion.identity, this.transform);
        var t = temp1.GetComponent<ParticleSystem>();
        isFiring1 = true;
    }

    void movebull2() {

        temp2.transform.Translate(directionOfTravel2.normalized * speed * Time.deltaTime, Space.World);
        

        if (Vector2.Distance(temp2.transform.position,Target_2.transform.position) <= directionOfTravel2.normalized.magnitude * speed * Time.deltaTime)
        {
            isFiring2 = false;
        }
    }
    void movebull1()
    {

        temp1.transform.Translate(directionOfTravel1.normalized * speed * Time.deltaTime, Space.World);


        if (Vector2.Distance(temp1.transform.position, Target_1.transform.position) <= directionOfTravel1.normalized.magnitude * speed * Time.deltaTime)
        {
            isFiring1 = false;
        }
    }



}
