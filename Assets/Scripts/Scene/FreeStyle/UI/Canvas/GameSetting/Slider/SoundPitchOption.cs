using System;
using TMPro;
using UnityEngine;

public class SoundPitchOption : OptionSlider
{
    public event Action<float/* Pitch */> OnUpdatePitch;
    public TextMeshProUGUI previewText;

    protected override void Awake()
    {
        base.Awake();

        curValue = Mathf.RoundToInt( GameSetting.SoundPitch * 100f );
        UpdateValue( curValue );
    }

	public override void ChangedValue( float _value )
	{
        GameSetting.SoundPitch = ( float ) Math.Round( _value * .01f, 2 );
        AudioManager.Inst.Pitch  = GameSetting.SoundPitch;

        OnUpdatePitch?.Invoke( GameSetting.SoundPitch );
        previewText.SetText( "x{0:2}", GameSetting.SoundPitch );
        previewText.color = GameSetting.SoundPitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.SoundPitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.SoundPitch = ( float ) Math.Round( curValue * .01f, 2 );
        AudioManager.Inst.Pitch  = GameSetting.SoundPitch;

        //OnUpdatePitch?.Invoke( GameSetting.SoundPitch );
        previewText.SetText( "x{0:2}", GameSetting.SoundPitch );
        previewText.color = GameSetting.SoundPitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.SoundPitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
