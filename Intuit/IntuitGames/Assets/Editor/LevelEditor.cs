using UnityEngine;
using UnityEditor;
using System.Collections;

class LevelEditor : EditorWindow {

	static GameObject greyCube;
	static GameObject greybox;
	
	//When placing a new piece of the greybox, the user will have the choice of extending the new piece either from the currently selected object on the scene or the most recently placed 
	Transform lastPlaced;
	GameObject selected;


	int lastPlacedCount = 0;
	private DIRECTIONS directions;

	[MenuItem ("Level Editor/Level Editor")]
	public static void ShowWindow() 
	{
		EditorWindow.GetWindow(typeof(LevelEditor));
		if(!GameObject.Find("Greybox")) 
		{
			greybox = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity) as GameObject;
			greybox.name = "Greybox";
		}
		else
		{
			greybox = GameObject.Find("Greybox");
		}
		if(greyCube == null)
		{
			greyCube = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Level Editor/Greybox/Chunk.prefab") as GameObject;
		}
	}

	void OnGUI () 
	{

		directions = (DIRECTIONS) EditorGUILayout.EnumPopup("Add in direction", directions);
		if(GUILayout.Button("AddCube"))
		{
			AddCube();
		}
	}

	void AddCube() 
	{
		if(lastPlaced == null) 
		{
			lastPlaced = (Instantiate(greyCube, GameObject.Find("Greybox").transform.position, Quaternion.identity) as GameObject).transform;
		}
		else 
		{
			GameObject go = Instantiate(greyCube, GetPointAtDistanceInDirection(lastPlaced.position, DirectionFromEnum(lastPlaced,directions), GetDistanceFromScale(lastPlaced.localScale,greyCube.transform.localScale, directions)), lastPlaced.rotation) as GameObject;
			go.transform.parent = greybox.transform;
			LevelObj levelobj = go.GetComponent<LevelObj>();
			levelobj.attachedTo = lastPlaced.gameObject;
			levelobj.attachDir = directions;
			levelobj.snapto = SnapTo.PrevObj;
			lastPlaced = go.transform;
		}
		lastPlaced.gameObject.name = "Cube" + lastPlacedCount.ToString();
		lastPlacedCount++;
		lastPlaced.parent = greybox.transform;
	}

	//use the distance formula to find a new point on the line (transform.position to transform.direction) that is a specified distance from the start point
	Vector3 GetPointAtDistanceInDirection(Vector3 start, Vector3 direction ,float dist)
	{
		Debug.Log("Distance entered = " + dist.ToString());
		//get the second point of the line (as transform.direction is in local units, add it to the start point)
		Vector3 pointAtDir = start + direction;
		Vector3 pointAtDistance = new Vector3();
		float d1 = Mathf.Sqrt((Mathf.Pow(pointAtDir.x - start.x,2)) + (Mathf.Pow(pointAtDir.y - start.y,2)) + (Mathf.Pow(pointAtDir.z - start.z,2)));
		Debug.Log("D1 = " + d1.ToString());
		if(d1 !=0) {
			pointAtDistance.x = start.x + dist * (pointAtDir.x - start.x)/d1;
			pointAtDistance.y = start.y + dist * (pointAtDir.y - start.y)/d1;
			pointAtDistance.z = start.z + dist * (pointAtDir.z - start.z)/d1;
		}
		else 
		{
			pointAtDistance.x = start.x + dist * (pointAtDir.x - start.x);
			pointAtDistance.y = start.y + dist * (pointAtDir.y - start.y);
			pointAtDistance.z = start.z + dist * (pointAtDir.z - start.z);
		}
		Debug.Log (pointAtDistance.ToString());
		return pointAtDistance;

	}

	Vector3 DirectionFromEnum(Transform t, DIRECTIONS dir) 
	{
		Vector3 directionVector;
		switch(dir)
		{
		case(DIRECTIONS.forward):
			directionVector = t.forward;
			break;
		case(DIRECTIONS.backward):
			directionVector = -t.forward;
			break;
		case(DIRECTIONS.up):
			directionVector = t.up;
			break;
		case(DIRECTIONS.down):
			directionVector = -t.up;
			break;
		case(DIRECTIONS.left):
			directionVector = -t.right;
			break;
		case(DIRECTIONS.right):
			directionVector = t.right;
			break;
		default:
			directionVector = t.forward;
			Debug.LogError("Incorrect DIRECTIONS enum entered");
			break;
		}
		return directionVector;
	}

	//get the distance of 2 objects given based of their scale, assuming they are to be placed side by side
	float GetDistanceFromScale(Vector3 ObjScale1, Vector3 ObjScale2, DIRECTIONS dir)
	{
		float dist = 0f; 
		switch(dir)
		{
		case(DIRECTIONS.forward):
		case(DIRECTIONS.backward):
			dist = ((ObjScale1.z + ObjScale2.z)/2);
			break;
		case(DIRECTIONS.up):
		case(DIRECTIONS.down):
			dist = ((ObjScale1.y + ObjScale2.y)/2);
			break;
		case(DIRECTIONS.left):
		case(DIRECTIONS.right):
			dist = ((ObjScale1.x + ObjScale2.x)/2);
			break;
		default:
			Debug.LogError("Incorrect DIRECTIONS enum entered");
			break;
		}
		return dist;
	}
}
