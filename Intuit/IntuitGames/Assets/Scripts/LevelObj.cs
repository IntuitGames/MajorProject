using UnityEngine;
using UnityEditor;
using System.Collections;
[ExecuteInEditMode]
public class LevelObj : MonoBehaviour {

	[HideInInspector]
	public GameObject attachedTo;
	public float gridSize;

	public SnapTo snapto;
	public DIRECTIONS attachDir;

	private LEVELOBTYPE obtype;
	public LEVELOBTYPE ObType 
	{
		get { return obtype; }
		set 
		{
			if(obtype == LEVELOBTYPE.Bouncy) {
				DestroyImmediate(gameObject.GetComponent<Bouncy>());
			}
			if(obtype == LEVELOBTYPE.NoCollision) {
				gameObject.GetComponent<BoxCollider>().enabled = true;
			}

			obtype = value;
			
			if(obtype == LEVELOBTYPE.Bouncy) 
			{
				gameObject.AddComponent<Bouncy>();
			}
			if(obtype == LEVELOBTYPE.NoCollision) 
			{
				gameObject.GetComponent<BoxCollider>().enabled = false;
			}
			if(obtype == LEVELOBTYPE.Ground)
			{
				gameObject.tag = "Ground";
				gameObject.layer = LayerMask.NameToLayer("Ground");
			}
		}
	}

	void Update () 
	{
		if(snapto == SnapTo.PrevObj)
		{
			transform.localPosition = GetPointAtDistanceInDirection(attachedTo.transform.position, DirectionFromEnum(attachedTo.transform, attachDir), GetDistanceFromScale(attachedTo.transform.localScale, transform.localScale, attachDir));
		}
		else if(snapto == SnapTo.Grid) 
		{
			transform.localPosition = new Vector3(roundToGrid(transform.localPosition.x), roundToGrid(transform.localPosition.y), roundToGrid(transform.localPosition.z));
			transform.localScale = new Vector3(Mathf.Round (transform.localScale.x), Mathf.Round (transform.localScale.y), Mathf.Round (transform.localScale.z));
		}
	}

	float roundToGrid(float f) 
	{
		if(gridSize == 0) {
			return f;
		}
		return (Mathf.Round(f/gridSize) * gridSize);
	}

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
