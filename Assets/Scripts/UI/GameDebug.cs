using System.Collections;
using TMPro;
using UnityEngine;

public class GameDebug : MonoBehaviour
{
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
        StartCoroutine( UpdateKeySoundCount() );
    }

    private IEnumerator UpdateKeySoundCount()
    {
        while ( !NowPlaying.IsStart )
        {
            keySoundCount.text = $"{SoundManager.Inst.KeySoundCount} ( {SoundManager.Inst.TotalKeySoundCount} )";
            yield return null;
        }
    }
}
