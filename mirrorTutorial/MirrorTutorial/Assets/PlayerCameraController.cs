using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Vector2 _maxFollowOffset = new Vector2(-1, -6);
    [SerializeField] private Vector2 _cameraVelocity = new Vector2(4f, 0.25f);
    [SerializeField] private Transform _playerTransform = null;
    [SerializeField] private Camera _camera;

    private Controls _controls;
    public Controls Controls
    {
        get
        {
            if (_controls == null)
            {
                _controls = new Controls();
            }
            return _controls;
        }
    }

    public override void OnStartAuthority()
    {
        _camera.gameObject.SetActive(true);
        enabled = true;
        Controls.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());
    }

    [ClientCallback]

    private void OnEnable()
    {
        Controls.Enable();
    }

    [ClientCallback]

    private void OnDisable()
    {
        Controls.Disable();
    }

    private void Look(Vector2 lookAxis)
    {
        float deltaTime = Time.deltaTime;
        _playerTransform.Rotate(0f, lookAxis.x * _cameraVelocity.x * deltaTime, 0f);
    }
}
