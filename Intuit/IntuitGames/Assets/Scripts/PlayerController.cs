using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	//as we only have 2 players, I figured a bool would be best to determine what player the character is
	public bool IsPlayer1;

	public float Speed;

	public float DashForce;

	private string horizInput;
	private string vertInput;
	private string jumpInput;
	private string dashInput;
	private string heavyInput;
	private string pauseInput;

	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();

		//set our Inputs depending on what player we are
		if(IsPlayer1) {
			horizInput = "Horizontal_P1";
			vertInput = "Vertical_P1";
			jumpInput = "Jump_P1";
			dashInput = "Dash_P1";
			heavyInput = "Heavy_P1";
			pauseInput = "Submit_P1";
		}
		else {
			horizInput = "Horizontal_P2";
			vertInput = "Vertical_P2";
			jumpInput = "Jump_P2";
			dashInput = "Dash_P2";
			heavyInput = "Heavy_P2";
			pauseInput = "Submit_P2";
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float moveHorizontal = Input.GetAxis (horizInput);
		float moveVertical = Input.GetAxis (vertInput);
		
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		
		rb.AddForce(movement*Speed*Time.deltaTime);

		if(Input.GetButton(dashInput)) {
			rb.AddForce(transform.forward * DashForce);
		}
	
	}
}
