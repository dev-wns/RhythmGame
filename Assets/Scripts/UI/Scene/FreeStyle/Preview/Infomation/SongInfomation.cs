using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongInfomation : OptionButton
{
    public TextMeshProUGUI title, version, artist;
    public Song song;

    public RectTransform rt { get; private set; }
    private Image panel;
    private Vector2 posCached;

    private static Color SelectPanelColor   = new Color( .75f, 1f, .75f, 1f );
    private static Color SelectTitleColor   = new Color( 1f, 1f, .75f, 1f );
    private static Color SelectVersionColor = new Color( .75f, 1f, 1f, 1f );

    protected override void Awake()
    {
        base.Awake();
        rt = transform as RectTransform;
        panel = GetComponent<Image>();
        posCached = rt.anchoredPosition;
    }

    public void SetSelectColor( bool _isSelect )
    {
        panel.color   = _isSelect ? SelectPanelColor   : Color.white;
        title.color   = _isSelect ? SelectTitleColor   : Color.white;
        version.color = _isSelect ? SelectVersionColor : Color.white;
    }

    public void SetInfo( Song _song )
    {
        song = _song;
        title.text   = _song.title;
        version.text = _song.version;
        artist.text  = $"{_song.artist} // {_song.creator}";
    }

    public void PositionReset()
    {
        rt.anchoredPosition = posCached;
    }
}
