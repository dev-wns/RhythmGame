using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SoundPreview : MonoBehaviour
{
    private FreeStyle scene;

    public TextMeshProUGUI time;
    public TextMeshProUGUI noteCount;
    public TextMeshProUGUI sliderCount;
    public TextMeshProUGUI bpm;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<FreeStyle>();
        scene.OnSelectSound += SelectChangedSoundInfo;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        noteCount.text   = _song.noteCount.ToString();
        sliderCount.text = _song.sliderCount.ToString();

        int second = ( int )( _song.totalTime * .001f );
        int minute = second / 60;
        second     = second % 60;
        time.text  = $"{minute:00}:{second:00}";
        
        if ( _song.minBpm == _song.maxBpm ) bpm.text = _song.medianBpm.ToString();
        else                                bpm.text = $"{_song.medianBpm} ({_song.minBpm} ~ {_song.maxBpm})";
    }
}
