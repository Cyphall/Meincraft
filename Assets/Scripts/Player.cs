using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Camera))]
public class Player : MonoBehaviour
{
	private Camera _camera;
	private World _world;
	private BlockType[] _blocks;
	private CharacterController _controller;
	private DynamicFOV _dynamicFOV;

	public float speed = 4.317f;
	public float sprintCoef = 1.3f;
	public float jumpSpeed = 8.5f;
	public float rotationSpeed = 1.5f;
	public float gravity = 28f;

	public Vector3 spawnPos;
	
	private int _currentBlockIndex;
	private Vector3 _moveDirection = Vector3.zero;

	public void setWorld(World world)
	{
		_world = world;
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;

		_camera = transform.GetChild(0).GetComponent<Camera>();
		_dynamicFOV = _camera.GetComponent<DynamicFOV>();

		spawnPos = transform.position;
		
		_controller = GetComponent<CharacterController>();
		
		_blocks = new[]{BlockType.GRASS, BlockType.IRON, BlockType.WOOD};
	}

	private void Update()
	{
		
		// Changement du bloc tenu
		_currentBlockIndex -= Convert.ToInt32(Input.GetAxis("Mouse ScrollWheel"));
		if (_currentBlockIndex < 0) _currentBlockIndex = _blocks.Length-1;
		if (_currentBlockIndex >= _blocks.Length) _currentBlockIndex = 0;
		
		
		// Mouvements de caméra
		Vector3 angle = _camera.transform.eulerAngles;
		if (angle.x > 180)
			angle.x -= 360;
		angle.x = Mathf.Clamp(angle.x + Input.GetAxis("Mouse Y") * rotationSpeed, -89f, 89f);
		_camera.transform.eulerAngles = angle;
		transform.Rotate(0, Input.GetAxis("Mouse X") * rotationSpeed, 0);

		
		// Détection du bloc visé
		Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		bool lookingAtBlock = Physics.Raycast(ray, out RaycastHit raycastHit, 5, 256);
		if (Input.GetMouseButtonDown(0) && lookingAtBlock)
		{
			_world.placeBlock(getBlockLookedAt(raycastHit), BlockType.AIR);
		}
		if (Input.GetMouseButtonDown(1) && lookingAtBlock)
		{
			Vector3Int newPos = getBlockLookedAt(raycastHit) + Vector3Int.RoundToInt(raycastHit.normal);
			_world.placeBlock(newPos, _blocks[_currentBlockIndex]);
		}
		
		
		// Déplacements
		if (_controller.isGrounded)
		{
			_moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			_moveDirection = transform.TransformDirection(_moveDirection);
			_moveDirection *= speed;
			
			if (Input.GetButton("Sprint"))
			{
				_moveDirection *= sprintCoef;
				_dynamicFOV.sprinting = true;
			}
			else
			{
				_dynamicFOV.sprinting = false;
			}
			
			if (Input.GetButton("Jump"))
			{
				_moveDirection.y = jumpSpeed;
			}
		}
		else
		{
			Vector3 tempDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			tempDirection = transform.TransformDirection(tempDirection);
			tempDirection *= speed;
		
			_moveDirection += Time.deltaTime * 2 * tempDirection;
		}

		_moveDirection.x /= 1 + 2 * Time.deltaTime;
		_moveDirection.y -= gravity * Time.deltaTime;
		_moveDirection.z /= 1 + 2 * Time.deltaTime;

		Vector3 posBefore = transform.position;

		float moveDeltaTime = Time.deltaTime;
		_controller.Move(_moveDirection * moveDeltaTime);
		// ReSharper disable once Unity.InefficientPropertyAccess
		_moveDirection = (transform.position - posBefore) / moveDeltaTime;
		
		
		// Zoom
		_dynamicFOV.zooming = Input.GetButton("Zoom");
	}

	private static Vector3Int getBlockLookedAt(RaycastHit raycastHit)
	{
		if (raycastHit.normal.x + raycastHit.normal.y + raycastHit.normal.z < 0)
			return new Vector3Int((int) Math.Floor(Math.Round(raycastHit.point.x, 5)), (int) Math.Floor(Math.Round(raycastHit.point.y, 5)), (int) Math.Floor(Math.Round(raycastHit.point.z, 5)));
		return new Vector3Int((int) Math.Floor(Math.Round(raycastHit.point.x - raycastHit.normal.x, 5)), (int) Math.Floor(Math.Round(raycastHit.point.y - raycastHit.normal.y, 5)), (int) Math.Floor(Math.Round(raycastHit.point.z - raycastHit.normal.z, 5)));
	}
}