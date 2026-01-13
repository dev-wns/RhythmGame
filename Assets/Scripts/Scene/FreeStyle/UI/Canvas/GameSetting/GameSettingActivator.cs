using UnityEngine;

public class GameSettingActivator : CanvasActivator
{
    public override void DisableCanvas()
    {
        base.DisableCanvas();

        // Game Mode
        Config.Inst.Write( ConfigType.Random,       ( int )GameSetting.CurrentRandom     );
        Config.Inst.Write( ConfigType.GameMode,     ( int )GameSetting.CurrentGameMode   );
        Config.Inst.Write( ConfigType.VisualFag,    ( int )GameSetting.CurrentVisualFlag );

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
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.GameSetting, KeyCode.Escape, DisableCanvas );
    }
}
