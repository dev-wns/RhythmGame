using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameDebug : MonoBehaviour
{
    private InGame scene;
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI channelsInUse;
    public TextMeshProUGUI backgroundType;
    public TextMeshProUGUI background, foreground;
    public TextMeshProUGUI keySoundCount;

    public void SetBackgroundType( BackgroundType _type, int _count = 0 )
    {
        switch ( _type )
        {
            case BackgroundType.None:
            case BackgroundType.Image:
            case BackgroundType.Video:
            backgroundType.text = $"{_type}";
            break;

            case BackgroundType.Sprite:
            backgroundType.text = $"{_type} ( {_count} )";
            break;
        }
        
    }

    public void SetSpriteCount( int _back, int _fore )
    {
        background.text = $"{_back}";
        foreground.text = $"{_fore}";
    }

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();

        //StartCoroutine( CalcFrameRate() );
        StartCoroutine( UpdateKeySoundCount() );
    }

    private IEnumerator UpdateKeySoundCount()
    {
        while ( !NowPlaying.Inst.IsStart )
        {
            keySoundCount.text = $"{SoundManager.Inst.KeySoundCount} ( {SoundManager.Inst.TotalKeySoundCount} )";
            yield return YieldCache.WaitForEndOfFrame;
        }
    }

    private IEnumerator CalcFrameRate()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );

            //fpsText.text = $"{( int )FPS} ( {( deltaTime * 1000f ):F1} ms )";
            //channelsInUse.text = $"{SoundManager.Inst.UseChannelCount}";
        }
    }
}
