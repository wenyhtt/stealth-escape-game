using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// BookViewUI Script.
/// </summary>
public class BookViewUI : Script
{
    public static BookViewUI Instance;

    public UIControl PageImageControl;
    public UIControl BackgroundDim;
    public PlayerScript Player;

    public override void OnEnable()
    {
        Instance = this;
        SetVisible(false);
    }

    public void Show(Texture pageTexture)
    {
        PageImageControl.Get<Image>().Brush = new TextureBrush(pageTexture);
        SetVisible(true);

        Screen.CursorVisible = true;
        Screen.CursorLock = CursorLockMode.None;

        Player.Enabled = false;
    }

    public void Hide()
    {
        SetVisible(false);

        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;

        Player.Enabled = true;
    }

    private void SetVisible(bool visible)
    {
        PageImageControl.Get<Control>().Visible = visible;
        if (BackgroundDim != null)
            BackgroundDim.Get<Control>().Visible = visible;
    }

    public override void OnUpdate()
    {
        if (PageImageControl.Get<Control>().Visible && Input.GetAction("Close"))
            Hide();
    }
}
