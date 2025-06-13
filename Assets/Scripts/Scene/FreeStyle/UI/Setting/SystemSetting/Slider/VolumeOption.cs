using UnityEngine;

public class VolumeOption : OptionSlider
{
    public ChannelType groupType = ChannelType.Master;

    private void OnEnable()
    {
        curValue = Mathf.RoundToInt( AudioManager.Inst.GetVolume( groupType ) * 100f );
        UpdateValue( curValue );
    }

    public void InputProcess( float _value )
    {
        AudioManager.Inst.SetVolume( _value * .01f, groupType );
        UpdateText( _value );
    }

    public override void Process()
    {
        switch ( groupType )
        {
            case ChannelType.BGM:
            //AudioManager.Inst.StopFadeEffect();
            //AudioManager.Inst.Volume = curValue * .01f;
            //AudioManager.Inst.FadeVolume( AudioManager.Inst.GetVolume( ChannelType.BGM ), AudioManager.Inst.Volume, .5f );
            break;


            default:
            break;
        }
            AudioManager.Inst.SetVolume( curValue * .01f, groupType );
    }
}
