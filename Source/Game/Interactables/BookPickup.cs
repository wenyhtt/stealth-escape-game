using System;
using System.Collections.Generic;
using FlaxEngine;

/// <summary>
/// BookPickup Script.
/// </summary>
public class BookPickup : Script, IInteractable
{
    public void Interact()
        {
            Debug.Log("Book collected!");
            // TODO: add to inventory / code-tracking system here
            Actor.IsActive = false; // hide it; or Destroy(Actor) to remove entirely
        }
}
