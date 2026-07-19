using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

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
