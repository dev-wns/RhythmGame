using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SongInfomation : OptionButton
{
    public TextMeshProUGUI title, artist;
    public Song song;

    public RectTransform rt { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        rt = transform as RectTransform;
    }

    public void SetInfo( Song _song )
    {
        song = _song;
        title.text = _song.title;
        artist.text = _song.artist;
    }
}
