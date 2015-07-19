using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	//as we only have 2 players, I figured a bool would be best to determine what controls the character will respond to
	public bool isPlayer1;

	public float speed;
	public float dashForce;
	public float rotationDamping;
	public float jumpForce;
	public float dashCooldown;
	public float heavyForce;

	private string horizInput;
	private string vertInput;
	private string jumpInput;
	private string dashInput;
	private string heavyInput;
	private string pauseInput;

	private bool grounded;
	private bool jump;
	private bool hasDashed;
	private bool heavy;
	private bool heavyForced;

	private float defaultMass;

	private float dashCooldownCounter;

	private Rigidbody rb;
	private Vector3 movement = Vector3.zero;
	private Vector3 vLookPos = Vector3.forward;
	private GameObject parent;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();

		dashCooldownCounter = dashCooldown;

		defaultMass = rb.mass;
		//set our Inputs depending on what player we are
		if(isPlayer1) {
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

	void Update () {
		int layermask = 1 << 8;
		grounded =  Physics.Raycast(transform.position, -transform.up, 1.1f, layermask);

		if (Input.GetButtonDown(jumpInput) && grounded) {
			jump = true;
		}

		if(hasDashed) {
			if (dashCooldownCounter > 0f) {
				dashCooldownCounter -= Time.deltaTime;
			}

			else {
				dashCooldownCounter = dashCooldown;
				hasDashed = false;
			}
		}

		if(Input.GetButtonDown(heavyInput)) {
			heavy = !heavy;
			if(heavy) { 
				heavyForced = true;
			}
		}
	}

	void FixedUpdate () {
		float moveHorizontal = Input.GetAxis (horizInput);
		float moveVertical = Input.GetAxis (vertInput);
		
		movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		
		rb.AddForce(movement*speed*Time.deltaTime);

		if(movement != Vector3.zero) {
			vLookPos = movement;		
		}

		//Make sure the character is always looking in the direction of the player's movement
		transform.rotation = Quaternion.Slerp(transform.rotation, 
		                                      Quaternion.LookRotation(vLookPos),
		                                      Time.deltaTime * rotationDamping);	

		if(Input.GetButton(dashInput) && !hasDashed) {
			rb.AddForce(transform.forward * dashForce);
			hasDashed = true;
		}

		if(jump) {
			rb.AddForce(transform.up * jumpForce);
			jump = false;
		}

		
		if(heavyForced) {
			rb.AddForce(-transform.up * heavyForce);
			heavyForced = false;
		}

		if(heavy) {
			if(rb.mass != defaultMass * 20f) {
				rb.mass = defaultMass * 20f;
				Debug.Log("heavy");
			}
		}

		if(!heavy) {
			if(rb.mass != defaultMass) {
				rb.mass = defaultMass;
			}

		}
	}
}
