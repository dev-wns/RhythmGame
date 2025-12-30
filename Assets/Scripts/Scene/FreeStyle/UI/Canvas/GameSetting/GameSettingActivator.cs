using UnityEngine;

public class GameSettingActivator : CanvasActivator
{
    public override void DisableCanvas()
    {
        base.DisableCanvas();

        // Game Mode
        Config.Inst.Write( ConfigType.Random,       ( int )GameSetting.CurrentRandom );
        Config.Inst.Write( ConfigType.AutoPlay,     GameSetting.HasFlag( GameMode.AutoPlay   ) );
        Config.Inst.Write( ConfigType.NoFailed,     GameSetting.HasFlag( GameMode.NoFail     ) );
        Config.Inst.Write( ConfigType.NoSlider,     GameSetting.HasFlag( GameMode.NoSlider   ) );
        Config.Inst.Write( ConfigType.FixedBPM,     GameSetting.HasFlag( GameMode.FixedBPM   ) );
        Config.Inst.Write( ConfigType.ConvertKey,   GameSetting.HasFlag( GameMode.ConvertKey ) );

        // Game Setting
        Config.Inst.Write( ConfigType.NoteModel,    ( int )GameSetting.CurrentNoteModel );
        Config.Inst.Write( ConfigType.ScrollSpeed,  GameSetting.ScrollSpeed  );
        Config.Inst.Write( ConfigType.JudgeOffset,  GameSetting.JudgeOffset  );
        Config.Inst.Write( ConfigType.SliderOffset, GameSetting.SliderOffset );

        // Sound
        Config.Inst.Write( ConfigType.SoundOffset,  GameSetting.SoundOffset );

        // UI
        Config.Inst.Write( ConfigType.BGAOpacity,   GameSetting.BGAOpacity   );
        Config.Inst.Write( ConfigType.PanelOpacity, GameSetting.PanelOpacity );
        Config.Inst.Write( ConfigType.GearOffsetX,  GameSetting.GearOffsetX  );
        Config.Inst.Write( ConfigType.Measure,      GameSetting.HasFlag( VisualFlag.ShowMeasure ) );
        Config.Inst.Write( ConfigType.HitEffect,    GameSetting.HasFlag( VisualFlag.HitEffect   ) );
        Config.Inst.Write( ConfigType.LaneEffect,   GameSetting.HasFlag( VisualFlag.LaneEffect  ) );
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.GameSetting, KeyCode.Escape, DisableCanvas );
    }
}
