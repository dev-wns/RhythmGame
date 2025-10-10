using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongUI : OptionButton
{
    public TextMeshProUGUI title, version, artist;
    public Song song;

    public RectTransform rt { get; private set; }
    private Image panel;
    public Vector2 PosCached;

    private static Color CenterPanelColor   = new Color( .75f, 1f, .75f, 1f );
    private static Color CenterTitleColor   = new Color( 1f, 1f, .75f, 1f );
    private static Color CenterVersionColor = new Color( .75f, 1f, 1f, 1f );
    private static Color CenterArtistColor  = new Color( .69f, .675f, .65f, 1f );

    private static Color DefaultPanelColor   = new Color( 1f, 1f, 1f, 1f );
    private static Color DefaultTitleColor   = new Color( 1f, 1f, 1f, 1f );
    private static Color DefaultVersionColor = new Color( 1f, 1f, 1f, 1f );
    private static Color DefaultArtistColor  = new Color( .69f, .675f, .65f, 1f );

    [Header( "Alpha Loop Effect" )]
    public float  minAlpha;
    public float  loopSpeed;
    private float time;
    private Coroutine corLoopAlpha;

    [Header( "Fade Effect" )]
    private bool  isCenter;
    private int   maxIndex;
    private int   halfIndex;
    private float alpha;
    private float startAlpha;
    private float endAlpha;
    private static readonly float Duration = .15f;
    private Coroutine corFadeAlpha;

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

    public void Initialize( int _maxIndex )
    {
        maxIndex  = _maxIndex;
        halfIndex = maxIndex / 2;
        alpha     = 1f;
    }

    public void UpdateColor( int _index, int _max )
    {
        startAlpha = alpha;
        endAlpha   = 1f - ( .215f * Global.Math.Abs( _index - halfIndex ) );
        isCenter   = _index == halfIndex;

        if ( corFadeAlpha is not null )
        {
            StopCoroutine( corFadeAlpha );
            corFadeAlpha = null;
        }
        corFadeAlpha = StartCoroutine( Fade() );
    }

    private IEnumerator Fade()
    {
        if ( Global.Math.Abs( startAlpha - endAlpha ) < float.Epsilon )
        {
            alpha = endAlpha;
            SetAlpha( alpha );
            yield break;
        }

        float offset = endAlpha - startAlpha;
        while( startAlpha < endAlpha ? alpha < endAlpha : // FadeIn
                                       alpha > endAlpha ) // FadeOut
        {
            yield return YieldCache.WaitForEndOfFrame;
            alpha += ( offset * Time.deltaTime ) / Duration;
            SetAlpha( alpha );
        }

        alpha = endAlpha;
        SetAlpha( alpha );
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

    private IEnumerator UpdateAlpha()
    {
        while ( true )
        {
            yield return null;
            time += Time.deltaTime;

            float alpha = minAlpha + ( ( 1f + Mathf.Cos( time * loopSpeed ) ) * .5f * ( 1f - minAlpha ) );
            panel.color = new Color( CenterPanelColor.r, CenterPanelColor.g, CenterPanelColor.b, alpha );
        }
    }

    public void SetInfo( Song _song )
    {
        song         = _song;
        title.text   = _song.title;
        version.text = _song.version;
        artist.text  = $"{_song.artist}";
    }

    public void PositionReset()
    {
        rt.anchoredPosition = PosCached;
    }
}
