using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private InGame scene;
    private List<KeySound> samples = new ();
    private int bgmIndex;
    

    private void Awake()
    {
        scene.OnLoadInThread += Load;
        scene.OnLoadEnd   += Sort;
        scene.OnClear     += Clear;
        NowPlaying.OnUpdateInThread += Play;

    }

    private void OnDestroy()
    {
        scene.OnLoadInThread -= Load;
        scene.OnLoadEnd   -= Sort;
        scene.OnClear     -= Clear;
        NowPlaying.OnUpdateInThread -= Play;
    }

    public void AddSample( KeySound _keySound )
    {
        samples.Add( _keySound );
        DataStorage.Inst.LoadSound( _keySound.name );
    }

    private void Load( in Song _song )
    {
        if ( _song.isOnlyKeySound )
        {
            samples.AddRange( DataStorage.Inst.Samples );
            foreach ( KeySound sample in samples )
            {
                DataStorage.Inst.LoadSound( sample.name );
            }
        }
        else
        {
            AddSample( new KeySound( -50 + GameSetting.SoundOffset, _song.audioName, 1f ) );
        }
    }

    private void Sort()
    {
        // 특정모드 선택으로 키음이 추가될 수 있다. ( 시간 오름차순 정렬 )
        samples.Sort( delegate ( KeySound _left, KeySound _right )
        {
            if      ( _left.time > _right.time ) return  1;
            else if ( _left.time < _right.time ) return -1;
            else                                 return  0;
        } );
    }

    private void Clear()
    {
        bgmIndex = 0;
    }

    private void Play( double _playback )
    {
        // 배경음 처리( 시간의 흐름에 따라 자동재생 )
        //var samples = DataStorage.Inst.ConvertedSamples;
        while ( bgmIndex < samples.Count && samples[bgmIndex].time <= _playback )
        {
            if ( DataStorage.Inst.GetSound( samples[bgmIndex].name, out FMOD.Sound sound ) )
                 AudioManager.Inst.Play( sound, samples[bgmIndex].volume );

            bgmIndex += 1;
        }
    }
}
