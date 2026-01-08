using UnityEngine;

public class PauseActivator : CanvasActivator
{
    [SerializeField] private OptionController controller;

    private void MoveToPrevOption()
    {
        controller.PrevMove();
        AudioManager.Inst.Play( SFX.MenuSelect );
    }

    private void MoveToNextOption()
    {
        controller.NextMove();
        AudioManager.Inst.Play( SFX.MenuSelect );
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Pause, KeyCode.DownArrow, MoveToNextOption );
        _input.Bind( SceneAction.Pause, KeyCode.UpArrow,   MoveToPrevOption );

        _input.Bind( SceneAction.Pause, KeyCode.F3, new InputRepeater( () => { GameSetting.ScrollSpeed -= 1; } ) );
        _input.Bind( SceneAction.Pause, KeyCode.F4, new InputRepeater( () => { GameSetting.ScrollSpeed += 1; } ) );
    }
}
