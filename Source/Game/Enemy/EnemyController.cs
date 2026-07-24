using System;
using System.Diagnostics;
using FlaxEngine;
using Game.Player;

namespace Game.Enemy;

public class EnemyController : Script
{
    public StaticModel EnemyMesh;
    public PlayerController Player;
    public CharacterController Controller;
    public float Speed = 250.0f; 
    public float Range = 1500.0f;

    /// <summary>
    /// How close the enemy needs to get to a waypoint before advancing to the next one.
    /// </summary>
    public float WaypointThreshold = 50.0f;

    /// <summary>
    /// How often (in seconds) the path to the player is recalculated.
    /// </summary>
    public float PathRecalcInterval = 0.25f;

    /// <summary>
    /// How fast the enemy rotates to face the player (degrees per second). 
    /// Set to 0 for instant rotation.
    /// </summary>
    public float RotationSpeed = 360.0f;

    /// <summary>
    /// Minimum distance to the player before the enemy stops moving.
    /// </summary>
    public float StoppingDistance = 80.0f;

    private Vector3 _velocity;
    private Vector3[] _path;
    private int _currentWaypointIndex;
    private float _pathRecalcTimer;

    public override void OnFixedUpdate()
    {
        if (Player == null) return;

        Vector3 playerPos = Player.Actor.Position;
        Vector3 enemyPos = Controller != null ? Controller.Position : Actor.Position;
        Vector3 toPlayer = playerPos - enemyPos;
        float distanceToPlayer = toPlayer.Length;

        // Apply gravity
        _velocity.Y += -Mathf.Abs(Physics.Gravity.Y * 2.5f) * Time.DeltaTime;

        // --- NavMesh Pathfinding ---
        if (distanceToPlayer <= Range)
        {
            // Recalculate path on a timer so we don't call FindPath every frame
            _pathRecalcTimer -= Time.DeltaTime;
            if (_pathRecalcTimer <= 0f || _path == null)
            {
                _pathRecalcTimer = PathRecalcInterval;
                if (Navigation.FindPath(enemyPos, playerPos, out var newPath) && newPath != null && newPath.Length > 0)
                {
                    _path = newPath;
                    _currentWaypointIndex = 0;
                }
            }

            // Follow the path waypoints
            if (_path != null && _currentWaypointIndex < _path.Length && distanceToPlayer > StoppingDistance)
            {
                Vector3 target = _path[_currentWaypointIndex];
                Vector3 moveDir = target - enemyPos;
                moveDir.Y = 0; // Keep horizontal

                float distToWaypoint = moveDir.Length;

                // Advance to next waypoint if close enough
                if (distToWaypoint < WaypointThreshold)
                {
                    _currentWaypointIndex++;

                    // If we've reached the end of the path, stop horizontal movement
                    if (_currentWaypointIndex >= _path.Length)
                    {
                        _velocity.X = 0;
                        _velocity.Z = 0;
                    }
                }
                else
                {
                    moveDir.Normalize();
                    _velocity.X = moveDir.X * Speed;
                    _velocity.Z = moveDir.Z * Speed;
                }
            }
            else
            {
                // No path or within stopping distance — stop horizontal movement
                _velocity.X = 0;
                _velocity.Z = 0;
            }

            // --- Always rotate to look at the player ---
            if (EnemyMesh != null)
            {
                Vector3 lookDir = playerPos - enemyPos;
                lookDir.Y = 0;

                if (lookDir.Length > 0.01f)
                {
                    lookDir.Normalize();
                    float targetYaw = (float)Math.Atan2(lookDir.X, lookDir.Z) * Mathf.RadiansToDegrees;
                    Quaternion targetRot = Quaternion.Euler(0, targetYaw + 90, 0);

                    if (RotationSpeed <= 0f)
                    {
                        EnemyMesh.LocalOrientation = targetRot;
                    }
                    else
                    {
                        EnemyMesh.LocalOrientation = Quaternion.Slerp(
                            EnemyMesh.LocalOrientation,
                            targetRot,
                            Mathf.Saturate(RotationSpeed * Mathf.DegreesToRadians * Time.DeltaTime)
                        );
                    }
                }
            }
        }
        else
        {
            // Out of range — clear path and stop
            _velocity.X = 0;
            _velocity.Z = 0;
            _path = null;
        }

        // --- Apply movement ---
        if (Controller != null)
        {
            Controller.Move(_velocity * Time.DeltaTime);

            if (Controller.IsGrounded && _velocity.Y < 0)
            {
                _velocity.Y = -Mathf.Abs(Physics.Gravity.Y * 0.5f);
            }
        }
        else
        {
            // Fallback (no collision)
            Actor.Position += _velocity * Time.DeltaTime;
        }
    }
}
