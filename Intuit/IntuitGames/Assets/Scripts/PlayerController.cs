using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	//as we only have 2 players, I figured a bool would be best to determine what controls the character will respond to
	public bool IsPlayer1;

	public float Speed;
	public float DashForce;
	public float rotationDamping;
	public float JumpForce;
	public float dashCooldown;

	private string horizInput;
	private string vertInput;
	private string jumpInput;
	private string dashInput;
	private string heavyInput;
	private string pauseInput;

	private bool grounded;
	private bool jump;
	private bool hasDashed;

	private float dashCooldownCounter;

	private Rigidbody rb;
	private Vector3 movement = Vector3.zero;
	private Vector3 vLookPos = Vector3.forward;
	private RaycastHit hit;

//	// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
//	grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  
//	
//	// If the jump button is pressed and the player is grounded then the player should jump.
//	if(Input.GetButtonDown("Jump") && grounded)
//		jump = true;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();

		dashCooldownCounter = dashCooldown;

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

	void Update () {
		int layermask = 1 << 8;
		grounded =  Physics.Raycast(transform.position, -transform.up, 1.5f, layermask);

		if (Input.GetButton(jumpInput) && grounded) {
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
	}

	void FixedUpdate () {
		float moveHorizontal = Input.GetAxis (horizInput);
		float moveVertical = Input.GetAxis (vertInput);
		
		movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		
		rb.AddForce(movement*Speed*Time.deltaTime);

		if(movement != Vector3.zero) {
			vLookPos = movement;		
		}

		//Make sure the character is always looking in the direction of the player's movement
		transform.rotation = Quaternion.Slerp(transform.rotation, 
		                                      Quaternion.LookRotation(vLookPos),
		                                      Time.deltaTime * rotationDamping);	

		if(Input.GetButton(dashInput) && !hasDashed) {
			rb.AddForce(transform.forward * DashForce);
			hasDashed = true;
		}

		if(jump) {
			rb.AddForce(transform.up * JumpForce);
			jump = false;
		}
	
	}
}
