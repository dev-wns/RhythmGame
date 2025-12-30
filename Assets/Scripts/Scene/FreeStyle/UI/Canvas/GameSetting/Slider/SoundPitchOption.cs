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

        curValue = Mathf.RoundToInt( GameSetting.Pitch * 100f );
        UpdateValue( curValue );
    }

	public override void ChangedValue( float _value )
	{
        GameSetting.Pitch = ( float ) Math.Round( _value * .01f, 2 );
        AudioManager.Inst.Pitch  = GameSetting.Pitch;

        OnUpdatePitch?.Invoke( GameSetting.Pitch );
        previewText.SetText( "x{0:2}", GameSetting.Pitch );
        previewText.color = GameSetting.Pitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.Pitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdateText( _value );
    }

    public override void Process()
    {
        GameSetting.Pitch = ( float ) Math.Round( curValue * .01f, 2 );
        AudioManager.Inst.Pitch  = GameSetting.Pitch;

        //OnUpdatePitch?.Invoke( GameSetting.Pitch );
        previewText.SetText( "x{0:2}", GameSetting.Pitch );
        previewText.color = GameSetting.Pitch < 1 ? new Color( .5f, .5f, 1f ) :
                            GameSetting.Pitch > 1 ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
