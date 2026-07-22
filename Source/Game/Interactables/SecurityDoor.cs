using System;
using System.Collections.Generic;
using FlaxEngine;
using Game.Interfaces;

namespace Game.Interactables;

/// <summary>
/// SecurityDoor Script.
/// </summary>
public class SecurityDoor : Script, IInteractable
{
    public void Interact()
    {
        Debug.Log("This is Security Door!");
        SecurityDoorViewUI.Instance.Show();
    }
}
