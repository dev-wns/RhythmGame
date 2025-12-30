using UnityEngine;

public class GameOverActivator : CanvasActivator
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
        _input.Bind( SceneAction.GameOver, KeyCode.DownArrow, MoveToNextOption );
        _input.Bind( SceneAction.GameOver, KeyCode.UpArrow,   MoveToPrevOption );
    }
}
