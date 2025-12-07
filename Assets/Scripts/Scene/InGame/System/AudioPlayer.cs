using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private int bgmIndex;
    

    private void Awake()
    {
        NowPlaying.OnClear          += Clear;
        NowPlaying.OnUpdateInThread += Play;

    }

    private void OnDestroy()
    {
        NowPlaying.OnClear          -= Clear;
        NowPlaying.OnUpdateInThread -= Play;
    }

    private void Clear()
    {
        bgmIndex = 0;
    }

    private void Play( double _playback )
    {
        // 배경음 처리( 시간의 흐름에 따라 자동재생 )
        var samples = DataStorage.ConvertedSamples;
        while ( bgmIndex < samples.Count && samples[bgmIndex].time <= _playback )
        {
            if ( DataStorage.Inst.GetSound( samples[bgmIndex].name, out FMOD.Sound sound ) )
                 AudioManager.Inst.Play( sound, samples[bgmIndex].volume );

            bgmIndex += 1;
        }
    }
}
