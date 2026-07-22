using System;
using System.Diagnostics;
using FlaxEngine;
using Game.Interfaces;
using Game.Interactables;

namespace Game.Player;

public class PlayerController : Script
{
    public CharacterController Player;
    public Camera CameraTarget;

    public Model SphereModel;

    public float CameraSmoothing = 20.0f;

    public bool CanJump = false;
    public bool UseMouse = true;
    public float JumpForce = 800;
    public float InteractDistance = 150.0f;
    public LayersMask InteractableLayer;

    public float Friction = 8.0f;
    public float GroundAccelerate = 5000;
    public float AirAccelerate = 10000;
    public float MaxVelocityGround = 400;
    public float MaxVelocityAir = 200;

    private Vector3 _velocity;
    private bool _jump;
    private float _pitch;
    private float _yaw;
    private float _horizontal;
    private float _vertical;

    /// <summary>
    /// Adds the movement and rotation to the camera (as input).
    /// </summary>
    /// <param name="horizontal">The horizontal input.</param>
    /// <param name="vertical">The vertical input.</param>
    /// <param name="pitch">The pitch rotation input.</param>
    /// <param name="yaw">The yaw rotation input.</param>
    public void AddMovementRotation(float horizontal, float vertical, float pitch, float yaw)
    {
        _pitch += pitch;
        _yaw += yaw;
        _horizontal += horizontal;
        _vertical += vertical;
    }

    public override void OnUpdate()
    {
        if (UseMouse)
        {
            // Cursor
            Screen.CursorVisible = false;
            Screen.CursorLock = CursorLockMode.Locked;

            // Mouse
            var mouseDelta = new Float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            _pitch = Mathf.Clamp(_pitch + mouseDelta.Y, -88, 88);
            _yaw += mouseDelta.X;
        }

        // Jump
        if (CanJump && Input.GetAction("Jump"))
            _jump = true;

        // Interact with objects
        var ray = new Ray(CameraTarget.Position, CameraTarget.Direction);
        if (Physics.RayCast(ray.Position, ray.Direction, out var hit, InteractDistance, InteractableLayer))
        {
            var interactable = hit.Collider.Parent?.GetScript<IInteractable>();

            // FlaxEngine.Debug.Log("Interactable hit: " + hit.Collider.Parent?.Name);

            if (interactable != null && Input.GetAction("Interact"))
            {
                interactable.Interact();
            }
        }
    }

    private Vector3 Horizontal(Vector3 v)
    {
        return new Vector3(v.X, 0, v.Z);
    }

    public override void OnFixedUpdate()
    {
        // Update camera
        var camTrans = CameraTarget.Transform;
        var camFactor = Mathf.Saturate(CameraSmoothing * Time.DeltaTime);
        CameraTarget.LocalOrientation = Quaternion.Lerp(CameraTarget.LocalOrientation, Quaternion.Euler(_pitch, _yaw, 0), camFactor);
        //CameraTarget.LocalOrientation = Quaternion.Euler(pitch, yaw, 0);
        camTrans.Translation = Vector3.Lerp(camTrans.Translation, CameraTarget.Position, camFactor);
        camTrans.Orientation = CameraTarget.Orientation;
        CameraTarget.Transform = camTrans;

        var inputH = Input.GetAxis("Horizontal") + _horizontal;
        var inputV = Input.GetAxis("Vertical") + _vertical;
        _horizontal = 0;
        _vertical = 0;

        var velocity = new Vector3(inputH, 0.0f, inputV);
        velocity.Normalize();
        Vector3 rotation = CameraTarget.LocalEulerAngles;
        rotation.X = 0;
        rotation.Z = 0;
        velocity = Vector3.Transform(velocity, Quaternion.Euler(rotation));

        if (Player.IsGrounded)
        {
            velocity = MoveGround(velocity.Normalized, Horizontal(_velocity));
            velocity.Y = -Mathf.Abs(Physics.Gravity.Y * 0.5f);
        }
        else
        {
            velocity = MoveAir(velocity.Normalized, Horizontal(_velocity));
            velocity.Y = _velocity.Y;
        }

        // Fix direction
        if (velocity.Length < 0.05f)
            velocity = Vector3.Zero;

        // Jump
        if (_jump && Player.IsGrounded)
            velocity.Y = JumpForce;
        _jump = false;

        // Apply gravity
        velocity.Y += -Mathf.Abs(Physics.Gravity.Y * 2.5f) * Time.DeltaTime;

        // Check if player is not blocked by something above head
        if ((Player.Flags & CharacterController.CollisionFlags.Above) != 0)
        {
            if (velocity.Y > 0)
            {
                // Player head hit something above, zero the gravity acceleration
                velocity.Y = 0;
            }
        }

        // Move
        Player.Move(velocity * Time.DeltaTime);
        _velocity = velocity;
    }

    // accelDir: normalized direction that the player has requested to move (taking into account the movement keys and look direction)
    // prevVelocity: The current velocity of the player, before any additional calculations
    // accelerate: The server-defined player acceleration value
    // maxVelocity: The server-defined maximum player velocity (this is not strictly adhered to due to strafejumping)
    private Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float maxVelocity)
    {
        float projVel = (float)Vector3.Dot(prevVelocity, accelDir); // Vector projection of Current velocity onto accelDir
        float accelVel = accelerate * Time.DeltaTime; // Accelerated velocity in direction of movement

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed max velocity
        if (projVel + accelVel > maxVelocity)
            accelVel = maxVelocity - projVel;

        return prevVelocity + accelDir * accelVel;
    }

    private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity)
    {
        // Apply Friction
        var speed = prevVelocity.Length;
        if (Math.Abs(speed) > 0.01f) // To avoid divide by zero errors
        {
            var drop = speed * Friction * Time.DeltaTime;
            prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction
        }

        // GroundAccelerate and MaxVelocityGround are server-defined movement variables
        return Accelerate(accelDir, prevVelocity, GroundAccelerate, MaxVelocityGround);
    }

    private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity)
    {
        // air_accelerate and max_velocity_air are server-defined movement variables
        return Accelerate(accelDir, prevVelocity, AirAccelerate, MaxVelocityAir);
    }

    public override void OnDebugDraw()
    {
        var trans = Player.Transform;
        DebugDraw.DrawWireCapsule(trans.Translation, trans.Orientation * Quaternion.Euler(90, 0, 0), Player.Radius, Player.Height, Color.Blue);
    }
}
