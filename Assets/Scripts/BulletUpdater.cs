using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletUpdater : MonoBehaviour
{

	////This looks after the update for all teh bullets
	//private static BulletUpdater _instance;
	//public static BulletUpdater Instance
	//{
	//	get
	//	{
	//		if (_instance == null) Debug.LogError("BulletUpdater is NULL");
	//		return _instance;
	//	}
	//}

	//public BulletScript[] arrayOfBullets;
	//private List<BulletScript> tempList;
	
	//private void Awake()
	//{
	//	_instance = this;
	//	tempList = new List<BulletScript>();
	//}

	//public void AddToList(BulletScript temp)
	//{
	//	tempList.Add(temp);
		
	//}

	//public void ConvertListToArray()
	//{
	//	//can on ly be called after we're sure the list is complete, as c# arrays are rigid
		
	//	arrayOfBullets = tempList.ToArray();
	//}

	////private void Update()
	////{
	////	var count = arrayOfBullets.Length;
	////	for (var i = 0; i < count; i++)
	////	{
	////		arrayOfBullets[i].ManagedFixedUpdate();
	////	}
	////}
}
