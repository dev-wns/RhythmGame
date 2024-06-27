using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongInfomation : OptionButton
{
    public TextMeshProUGUI title, version, artist;
    public Song song;

    public RectTransform rt { get; private set; }
    private Image panel;
    public Vector2 PosCached;

    private static Color SelectPanelColor   = new Color( .75f, 1f, .75f, 1f );
    private static Color SelectTitleColor   = new Color( 1f, 1f, .75f, 1f );
    private static Color SelectVersionColor = new Color( .75f, 1f, 1f, 1f );

    protected override void Awake()
    {
        base.Awake();
        rt = transform as RectTransform;
        panel = GetComponent<Image>();
        PosCached = rt.anchoredPosition;
    }

    public void MoveX( float _offset )
    {
        rt.DOAnchorPosX( PosCached.x + _offset, .5f );
    }

    public void Select( bool _isSelect )
    {
        if ( _isSelect )
        {
            panel.color   = SelectPanelColor;
            title.color   = SelectTitleColor;
            version.color = SelectVersionColor;
            //rt.DOAnchorPosX( PosCached.x - 125f, .5f );
        }
        else
        {
            panel.color   = Color.white;
            title.color   = Color.white;
            version.color = Color.white;
            //rt.DOAnchorPosX( PosCached.x, .5f );
        }
    }

    public void SetInfo( Song _song )
    {
        song = _song;
        title.text   = _song.title;
        version.text = _song.version;
        artist.text  = $"{_song.artist}";
    }

    public void PositionReset()
    {
        rt.anchoredPosition = PosCached;
    }
}
