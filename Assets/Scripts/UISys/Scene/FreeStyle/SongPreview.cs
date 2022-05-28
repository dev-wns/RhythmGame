using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SongPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;

    public TextMeshProUGUI time;
    public TextMeshProUGUI noteCount;
    public TextMeshProUGUI sliderCount;
    public TextMeshProUGUI bpm;

    private void Awake()
    {
        scroller.OnSelectSong += SelectChangedSoundInfo;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        noteCount.text   = _song.noteCount.ToString();
        sliderCount.text = _song.sliderCount.ToString();

        int second = ( int )( _song.totalTime * .001f );
        int minute = second / 60;
        second = second % 60;
        time.text = $"{minute:00}:{second:00}";

        int medianBpm = ( int )_song.medianBpm;
        if ( _song.minBpm == _song.maxBpm ) bpm.text = medianBpm.ToString();
        else                                bpm.text = $"{medianBpm} ({_song.minBpm} ~ {_song.maxBpm})";
    }
}
