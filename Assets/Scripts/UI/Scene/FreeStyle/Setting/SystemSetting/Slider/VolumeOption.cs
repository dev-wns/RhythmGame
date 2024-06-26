using UnityEngine;

public class VolumeOption : OptionSlider
{
    public ChannelType groupType = ChannelType.Master;

    private void OnEnable()
    {
        curValue = groupType == ChannelType.BGM ? Mathf.RoundToInt( SoundManager.Inst.Volume * 100f ) : Mathf.RoundToInt( SoundManager.Inst.GetVolume( groupType ) * 100f );
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        SoundManager.Inst.SetVolume( _value * .01f, groupType );
        UpdateText( _value );
    }

    public override void Process()
    {
        switch ( groupType )
        {
            case ChannelType.BGM:
            SoundManager.Inst.StopFadeEffect();
            SoundManager.Inst.Volume = curValue * .01f;
            SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume, .5f );
            break;


            default:
            SoundManager.Inst.SetVolume( curValue * .01f, groupType );
            break;
        }
    }
}
