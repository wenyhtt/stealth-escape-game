using System;
using System.Collections.Generic;
using FlaxEngine;
using Game.Interfaces;

namespace Game.Interactables;

/// <summary>
/// Door Script.
/// </summary>
public class Door : Script, IInteractable
{
    public void Interact()
    {
        Debug.Log("This is Door!");
    }
}
