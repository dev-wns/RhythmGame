using UnityEngine;

public class SystemSettingActivator : CanvasActivator
{
    public override void DisableCanvas()
    {
        base.DisableCanvas();

        Config.Inst.Write( ConfigType.Resolution,   ( int )SystemSetting.CurrentResolution   );
        Config.Inst.Write( ConfigType.FrameLimit,   ( int )SystemSetting.CurrentFrameRate    );
        Config.Inst.Write( ConfigType.AntiAliasing, ( int )SystemSetting.CurrentAntiAliasing );
        Config.Inst.Write( ConfigType.ScreenMode,   ( int )SystemSetting.CurrentScreenMode   );

        Config.Inst.Write( ConfigType.SoundBuffer,  ( int )SystemSetting.CurrentSoundBuffer           );
        Config.Inst.Write( ConfigType.Master,       AudioManager.Inst.GetVolume( ChannelType.Master ) );
        Config.Inst.Write( ConfigType.BGM,          AudioManager.Inst.GetVolume( ChannelType.BGM    ) );
        Config.Inst.Write( ConfigType.SFX,          AudioManager.Inst.GetVolume( ChannelType.SFX    ) );
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.SystemSetting, KeyCode.Escape, DisableCanvas );
    }
}
