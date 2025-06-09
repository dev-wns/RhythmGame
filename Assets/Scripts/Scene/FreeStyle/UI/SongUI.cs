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

    private static Color SelectPanelColor   = new Color( .75f, 1f, .75f, 1f );
    private static Color SelectTitleColor   = new Color( 1f, 1f, .75f, 1f );
    private static Color SelectVersionColor = new Color( .75f, 1f, 1f, 1f );

    [Header( "Alpha Loop Effect" )]
    public float  minAlpha;
    public float  loopSpeed;
    private float time;
    private Coroutine corLoopAlpha;

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
            if ( !ReferenceEquals( corLoopAlpha, null ) )
            {
                StopCoroutine( corLoopAlpha );
                corLoopAlpha = null;
            }

           corLoopAlpha  = StartCoroutine( UpdateAlpha() );
        }
        else
        {
            time = 0;

            panel.color   = Color.white;
            title.color   = Color.white;
            version.color = Color.white;
            if ( !ReferenceEquals( corLoopAlpha, null ) )
            {
                StopCoroutine( corLoopAlpha );
                corLoopAlpha = null;
            }
        }
    }

    private IEnumerator UpdateAlpha()
    {
        while ( true )
        {
            yield return null;
            time += Time.deltaTime;

            float alpha = minAlpha + ( ( 1f + Mathf.Cos( time * loopSpeed ) ) * .5f * ( 1f - minAlpha ) );
            panel.color = new Color( SelectPanelColor.r, SelectPanelColor.g, SelectPanelColor.b, alpha );
        }
    }

    public void SetInfo( Song _song )
    {
        song = _song;
        title.text = _song.title;
        version.text = _song.version;
        artist.text = $"{_song.artist}";
    }

    public void PositionReset()
    {
        rt.anchoredPosition = PosCached;
    }
}
