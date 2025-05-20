using System;
using TMPro;
using UnityEngine;

public class SoundPitchOption : OptionSlider
{
    public event Action<float/* Pitch */> OnPitchUpdate;
    public TextMeshProUGUI previewText;

    protected override void Awake()
    {
        base.Awake();

        curValue = Mathf.RoundToInt( GameSetting.CurrentPitch * 100f );
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        GameSetting.CurrentPitch = _value;
        AudioManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );

        OnPitchUpdate?.Invoke( GameSetting.CurrentPitch );
        previewText.text = $"x{GameSetting.CurrentPitch:F2}";
        previewText.color = GameSetting.CurrentPitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.CurrentPitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.CurrentPitch = curValue;
        AudioManager.Inst.SetPitch( GameSetting.CurrentPitch, ChannelType.BGM );

        OnPitchUpdate?.Invoke( GameSetting.CurrentPitch );
        previewText.text = $"x{GameSetting.CurrentPitch:F2}";
        previewText.color = GameSetting.CurrentPitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.CurrentPitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
