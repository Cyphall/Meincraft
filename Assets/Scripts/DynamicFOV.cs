using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DynamicFOV : MonoBehaviour
{
	private float _defaultFOV;
	private float _targetFOV;
    
	private Camera _cameraComponent;

	public bool sprinting;
	public bool zooming;
    
	private void Start()
	{
		_cameraComponent = GetComponent<Camera>();
		_targetFOV = _defaultFOV = _cameraComponent.fieldOfView;
	}
    
	private void Update()
	{
		if (zooming)
		{
			_targetFOV = 10;
		}
		else if (sprinting)
		{
			_targetFOV = _defaultFOV + 15;
		}
		else
		{
			_targetFOV = _defaultFOV;
		}

		if (Math.Abs(_targetFOV - _cameraComponent.fieldOfView) < 0.2f)
		{
			_cameraComponent.fieldOfView = _targetFOV;
			return;
		}
		
		// ReSharper disable once Unity.InefficientPropertyAccess
		_cameraComponent.fieldOfView += (_targetFOV - _cameraComponent.fieldOfView) * Time.deltaTime * 10;
	}
}