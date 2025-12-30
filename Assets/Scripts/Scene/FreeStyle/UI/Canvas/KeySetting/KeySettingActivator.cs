using UnityEngine;

public class KeySettingActivator : CanvasActivator
{
    [SerializeField] private OptionController option;

    public override void DisableCanvas()
    {
        base.DisableCanvas();

        Config.Inst.Write( ConfigType._4K, InputManager.Keys[GameKeyCount._4] );
        Config.Inst.Write( ConfigType._6K, InputManager.Keys[GameKeyCount._6] );
        Config.Inst.Write( ConfigType._7K, InputManager.Keys[GameKeyCount._7] );
    }

    private void MoveToPrevOption()
    {
        option.PrevMove();
        AudioManager.Inst.Play( SFX.MenuSelect );
    }

    private void MoveToNextOption()
    {
        option.NextMove();
        AudioManager.Inst.Play( SFX.MenuSelect );
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        // KeySetting
        _input.Bind( SceneAction.KeySetting, KeyCode.RightArrow, MoveToNextOption );
        _input.Bind( SceneAction.KeySetting, KeyCode.LeftArrow,  MoveToPrevOption );
        _input.Bind( SceneAction.KeySetting, KeyCode.Escape,     DisableCanvas    );
    }
}
