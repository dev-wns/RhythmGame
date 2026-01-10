using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongScrollerItem : OptionButton
{
    [SerializeField] private TextMeshProUGUI title, version, artist;
    [SerializeField] private Vector2 posCache;

    public RectTransform rt { get; private set; }
    private Image panel;

    private static readonly Color CenterPanelColor    = new Color( .75f,   1f,  .75f, 1f );
    private static readonly Color CenterTitleColor    = new Color(   1f,   1f,  .75f, 1f );
    private static readonly Color CenterVersionColor  = new Color( .75f,   1f,    1f, 1f );
    private static readonly Color CenterArtistColor   = new Color( .69f, .675f, .65f, 1f );

    private static readonly Color DefaultPanelColor   = new Color(   1f,    1f,   1f, 1f );
    private static readonly Color DefaultTitleColor   = new Color(   1f,    1f,   1f, 1f );
    private static readonly Color DefaultVersionColor = new Color(   1f,    1f,   1f, 1f );
    private static readonly Color DefaultArtistColor  = new Color( .69f, .675f, .65f, 1f );

    [Header( "Fade Effect" )]
    private bool  isCenter;
    private int   maxIndex;
    private int   halfIndex;
    private float alpha;
    private float endAlpha;
    private static readonly float Duration = .15f;

    protected override void Awake()
    {
        base.Awake();
        rt = transform as RectTransform;
        panel = GetComponent<Image>();
        posCache = rt.anchoredPosition;
    }

    public void MoveX( float _offset )
    {
        rt.DOKill();
        rt.DOAnchorPosX( posCache.x + _offset, .5f );
    }

    public void Initialize( int _maxIndex )
    {
        maxIndex  = _maxIndex;
        halfIndex = maxIndex / 2;
        alpha     = 1f;
    }

    private Tween colorTween;
    public void UpdateColor( int _index, int _max )
    {
        endAlpha   = 1f - ( .215f * Global.Math.Abs( _index - halfIndex ) );
        isCenter   = _index == halfIndex;

        colorTween?.Kill();
        colorTween = DOTween.To( () => alpha, x => { alpha = x; SetAlpha( alpha ); }, endAlpha, Duration );
    }

    private void SetAlpha( float _alpha )
    {
        if ( isCenter )
        {
            panel.color   = new Color( CenterPanelColor.r,   CenterPanelColor.g,   CenterPanelColor.b,   alpha );
            title.color   = new Color( CenterTitleColor.r,   CenterTitleColor.g,   CenterTitleColor.b,   alpha );
            version.color = new Color( CenterVersionColor.r, CenterVersionColor.g, CenterVersionColor.b, alpha );
            artist.color  = new Color( CenterArtistColor.r,  CenterArtistColor.g,  CenterArtistColor.b,  alpha );
        }
        else
        {
            panel.color   = new Color( DefaultPanelColor.r,   DefaultPanelColor.g,   DefaultPanelColor.b,   alpha );
            title.color   = new Color( DefaultTitleColor.r,   DefaultTitleColor.g,   DefaultTitleColor.b,   alpha );
            version.color = new Color( DefaultVersionColor.r, DefaultVersionColor.g, DefaultVersionColor.b, alpha );
            artist.color  = new Color( DefaultArtistColor.r,  DefaultArtistColor.g,  DefaultArtistColor.b,  alpha );
        }  
    }

    public void SetInfo( Song _song )
    {
        title.SetText(   _song.title   );
        version.SetText( _song.version );
        artist.SetText(  _song.artist  );
    }

    public void ResetPosition()
    {
        rt.anchoredPosition = posCache;
    }
}
