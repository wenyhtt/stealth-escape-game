using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;
using Game.Player;

namespace Game.Interactables;

/// <summary>
/// SecurityDoorViewUI script.
/// </summary>
public class SecurityDoorViewUI : Script
{
    public static SecurityDoorViewUI Instance;

    public UIControl TextBoxController;
    public UIControl ImageCorrect;
    public UIControl BackgroundDim;
    public PlayerController Player;

    public string CorrectCode = "1234";
    private TextBox _textBox;

    public override void OnStart()
    {
        Instance = this;

        if (TextBoxController != null)
        {
            _textBox = TextBoxController.Get<TextBox>();
            if (_textBox != null)
                _textBox.EditEnd += OnTextSubmitted; // fires on Enter / focus loss
            else
                Debug.LogWarning($"{nameof(SecurityDoorViewUI)}: TextBoxController is not a TextBox.");
        }
    }

    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnTextSubmitted()
    {
        CheckCode(_textBox.Text);
    }

    private void CheckCode(string input)
    {
        if (input == CorrectCode)
        {
            OnCodeCorrect();
        }
        else
        {
            Debug.Log("Incorrect code.");
            _textBox.Text = string.Empty;
        }
    }

    private void OnCodeCorrect()
    {
        Debug.Log("Code correct, unlocking door.");
        Hide();

        if (ImageCorrect != null)
            ImageCorrect.Get<Control>().Visible = true;
    }

    public void Show()
    {
        SetVisible(true);

        Screen.CursorVisible = true;
        Screen.CursorLock = CursorLockMode.None;

        if (Player != null)
            Player.Enabled = false;
        else
            Debug.LogWarning("Player is not assigned in the editor!");

        if (_textBox != null)
            _textBox.Focus();
    }

    public void Hide()
    {
        SetVisible(false);

        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;

        if (Player != null)
            Player.Enabled = true;
        else
            Debug.LogWarning("Player is not assigned in the editor!");
    }

    private void SetVisible(bool visible)
    {
        if (TextBoxController != null)
            TextBoxController.Get<Control>().Visible = visible;
        if (BackgroundDim != null)
            BackgroundDim.Get<Control>().Visible = visible;
    }

    public override void OnUpdate()
    {
        if (TextBoxController.Get<Control>().Visible && Input.GetAction("Close"))
            Hide();

        if (Input.GetKeyDown(KeyboardKeys.Return))
            CheckCode(_textBox.Text);
    }
}
