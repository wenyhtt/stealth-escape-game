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

    private Vector3 _velocity;

    public override void OnFixedUpdate()
    {
        if (Player == null) return;

        Vector3 playerPos = Player.Actor.Position;
        // Use Controller's position if available, otherwise fallback to Actor
        Vector3 enemyPos = Controller != null ? Controller.Position : Actor.Position;
        
        Vector3 direction = playerPos - enemyPos;

        // Apply gravity
        _velocity.Y += -Mathf.Abs(Physics.Gravity.Y * 2.5f) * Time.DeltaTime;

        // Horizontal movement towards player
        if (direction.Length <= Range)
        {
            direction.Y = 0; // Ignore height difference for horizontal movement
            
            if (direction.Length > 0.05f)
            {
                direction.Normalize();
                
                _velocity.X = direction.X * Speed;
                _velocity.Z = direction.Z * Speed;

                // Handle rotation with the 90-degree offset
                if (EnemyMesh != null)
                {
                    float yaw = (float)Math.Atan2(direction.X, direction.Z) * (180.0f / (float)Math.PI);
                    EnemyMesh.LocalOrientation = Quaternion.Euler(0, yaw + 90, 0);
                }
            }
            else
            {
                _velocity.X = 0;
                _velocity.Z = 0;
            }
        }
        else
        {
            // Stop horizontally if out of range
            _velocity.X = 0;
            _velocity.Z = 0;
        }

        // Apply movement using the physics engine if a controller is attached
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
