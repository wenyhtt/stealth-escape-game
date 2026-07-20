using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;
using Game.Interfaces;

namespace Game.Interactables;

/// <summary>
/// BookPickup Script.
/// </summary>
public class BookPickup : Script, IInteractable
{
    public Texture PageImage;

    public void Interact()
    {
        Debug.Log("Book collected!");
        BookViewUI.Instance.Show(PageImage);
        Actor.IsActive = false;
    }
}
