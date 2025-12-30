using UnityEngine;

public class SystemSettingActivator : CanvasActivator
{
    public override void DisableCanvas()
    {
        base.DisableCanvas();

        Config.Inst.Write( ConfigType.Resolution,   SystemSetting.CurrentResolution   );
        Config.Inst.Write( ConfigType.FrameLimit,   SystemSetting.CurrentFrameRate    );
        Config.Inst.Write( ConfigType.AntiAliasing, SystemSetting.CurrentAntiAliasing );
        Config.Inst.Write( ConfigType.ScreenMode,   SystemSetting.CurrentScreenMode   );
        Config.Inst.Write( ConfigType.PollingRate,  SystemSetting.CurrentPollingRate  );

        Config.Inst.Write( ConfigType.SoundBuffer, SystemSetting.CurrentSoundBuffer                  );
        Config.Inst.Write( ConfigType.Master,      AudioManager.Inst.GetVolume( ChannelType.Master ) );
        Config.Inst.Write( ConfigType.BGM,         AudioManager.Inst.GetVolume( ChannelType.BGM    ) );
        Config.Inst.Write( ConfigType.SFX,         AudioManager.Inst.GetVolume( ChannelType.SFX    ) );
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.SystemSetting, KeyCode.Escape, DisableCanvas );
    }
}
