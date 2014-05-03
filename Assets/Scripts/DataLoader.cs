using UnityEngine;
using LitJson;
using System;
using System.Collections;

public class DataLoader : MonoBehaviour {

	public GameObject hydrantPrefab;
	public GameObject knoxPrefab;
	public GameObject cctvPrefab;
	public GameObject activePrefab;
	public GameObject inactivePrefab;

	public GameObject referenceSW;
	public GameObject referenceSE;
	public GameObject referenceNW;
	private ReferencePoint dataSE;
	private ReferencePoint dataNW;
	private ReferencePoint dataSW;

	private float metersPerDegreeX;
	private float metersPerDegreeY;

	private float lastUpdate = 0;
	private ArrayList poiObjects;

	// Use this for initialization
	void Start () {
		poiObjects = new ArrayList();

		dataSE = referenceSE.GetComponent<ReferencePoint>();
		dataNW = referenceNW.GetComponent<ReferencePoint>();
		dataSW = referenceSW.GetComponent<ReferencePoint>();

		DetermineXScale();
		DetermineYScale();

		lastUpdate = Time.timeSinceLevelLoad;
		StartCoroutine(DownloadFeed());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		if ((Time.timeSinceLevelLoad - lastUpdate) >= 30) {
			lastUpdate = Time.timeSinceLevelLoad;
			StartCoroutine(DownloadFeed());
		}
	}

	void ParseFeed(String data) {
		// delete old items
		foreach (GameObject itemInArray in poiObjects) {
			Destroy(itemInArray);
		}

		JsonData jsonData = JsonMapper.ToObject(data);

		int totalPOI = jsonData["tags"].Count;
		int totalUsers = jsonData["users"].Count;

		// list tags
		for (int i = 0; i < totalPOI; i++) {
			String poiType = jsonData["tags"][i]["type"].ToString();
			
			float latitude = float.Parse("" + jsonData["tags"][i]["latitude"], System.Globalization.CultureInfo.InvariantCulture);
			float longitude = float.Parse("" + jsonData["tags"][i]["longitude"], System.Globalization.CultureInfo.InvariantCulture);

			Vector3 targetPos = PlaceAtWorldLocation(latitude, longitude, 0.1819774f);
			

			GameObject prefabType;

			if (poiType == "Knox Box") {
				// knox
				prefabType = knoxPrefab;
				Quaternion rotation = Quaternion.Euler(0,180,0);
				GameObject newPOI = (GameObject) Instantiate(prefabType, targetPos, rotation);
				poiObjects.Add(newPOI);
			}
			else if (poiType == "Fire Hydrant") {
				// fire hydrant
				prefabType = hydrantPrefab;
				Quaternion rotation = Quaternion.Euler(0,180,0);
				GameObject newPOI = (GameObject) Instantiate(prefabType, targetPos, rotation);
				poiObjects.Add(newPOI);
			}
			else if (poiType == "Street Camera") {
				// street camera
				prefabType = cctvPrefab;
				Quaternion rotation = Quaternion.Euler(0,180,0);
				GameObject newPOI = (GameObject) Instantiate(prefabType, targetPos, rotation);
				poiObjects.Add(newPOI);
			}
			
		}

		for (int i = 0; i < totalUsers; i++) {
			
			float latitude = float.Parse("" + jsonData["users"][i]["latitude"], System.Globalization.CultureInfo.InvariantCulture);
			float longitude = float.Parse("" + jsonData["users"][i]["longitude"], System.Globalization.CultureInfo.InvariantCulture);

			Vector3 targetPos = PlaceAtWorldLocation(latitude, longitude, 0.03950083f);

			int timeSinceUpdate = Int32.Parse("" + jsonData["users"][i]["time"]);
			Debug.Log(timeSinceUpdate);

			if (timeSinceUpdate >= 300) {
				GameObject prefabType = inactivePrefab;
				Quaternion rotation = Quaternion.Euler(0,180,0);
				GameObject newPOI = (GameObject) Instantiate(prefabType, targetPos, rotation);
				poiObjects.Add(newPOI);
			}
			else {
				GameObject prefabType = activePrefab;
				Quaternion rotation = Quaternion.Euler(0,180,0);
				GameObject newPOI = (GameObject) Instantiate(prefabType, targetPos, rotation);
				poiObjects.Add(newPOI);
			}
		}


	}

	IEnumerator DownloadFeed() {
		Debug.Log("reload");
		// load in the JSON
		WWW feedDownload = new WWW("https://vast-atoll-5515.herokuapp.com/getAll");
		yield return feedDownload;
	    if (feedDownload.error == null)
	    {
	      //Sucessfully loaded the JSON string
	      ParseFeed(feedDownload.text);
	    }
	    else
	    {
	      Debug.Log("ERROR: " + feedDownload.error);
	    }
	}

	void DetermineXScale() {
		// determine x distance
		float eastMeter = referenceSE.transform.position.x;
		float westMeter = referenceSW.transform.position.x;

		// get distance between points in meters
		float distance = eastMeter - westMeter;

		// get difference in longitude degrees
		float distanceDegree = dataSE.longitude - dataSW.longitude;

		Debug.Log("X distance: " + distance + " X degree offset: " + distanceDegree);

		metersPerDegreeX = distance / distanceDegree;

	}

	void DetermineYScale() {
		// determine y distance
		float northMeter = referenceNW.transform.position.z;
		float southMeter = referenceSW.transform.position.z;

		// get distance between points in meters
		float distance = northMeter - southMeter;

		// get difference in latitude degrees
		float distanceDegree = dataNW.latitude - dataSW.latitude;

		// Debug.Log("Y distance: " + distance + " Y degree offset: " + distanceDegree);

		metersPerDegreeY = distance / distanceDegree;

	}

	Vector3 PlaceAtWorldLocation(float newLatitude, float newLongitude, float yPos) {
		float eastMeter = referenceSE.transform.position.x;
		float southMeter = referenceSW.transform.position.z;

		float distanceX = newLongitude - dataSE.longitude;
		float distanceY = newLatitude - dataSW.latitude;

		// Debug.Log(distanceY + " " + distanceX);



		Vector3 finalPos = new Vector3((distanceX * metersPerDegreeX) + eastMeter,yPos,(distanceY * metersPerDegreeY) + southMeter);
		
		// Debug.Log(finalPos.x + " " + finalPos.z);

		return finalPos;
	}
}
