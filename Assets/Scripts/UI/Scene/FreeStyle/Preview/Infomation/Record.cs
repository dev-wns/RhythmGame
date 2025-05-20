using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class Record : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;

    public GameObject noRecord;
    public GameObject infomation;

    private RectTransform rt;

    [Header("Infomation")]
    public Image rankImage;
    public SpriteAtlas rankAtlas;
    public TextMeshProUGUI chaos;
    public TextMeshProUGUI score;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI date;

    [Header("Effect")]
    private Sequence sequence;
    private float startPosX = -2700f;

    private void Awake()
    {
        mainScroll.OnSelectSong += UpdateRecord;
        rt = transform as RectTransform;

        sequence = DOTween.Sequence().Pause().SetAutoKill( false ).SetEase( Ease.OutBack );
        sequence.Append( rt.DOAnchorPosX( rt.anchoredPosition.x, 1f ) );
    }

    public void UpdateRecord( Song _song )
    {
        if ( !NowPlaying.Inst.UpdateRecord() )
        {
            noRecord.SetActive( true );
            infomation.SetActive( false );
        }
        else
        {
            noRecord.SetActive( false );
            infomation.SetActive( true );

            RecordData data = NowPlaying.CurrentRecord;
            chaos.text = $"{( ( GameRandom )data.random ).ToString().Replace( '_', ' ' )}";
            score.text = $"{data.score:N0}";
            rate.text = $"x{data.pitch:N1}";
            accuracy.text = $"{( data.accuracy * .01f ):N2}%";
            date.text = data.date;

            rankImage.sprite = data.accuracy >= 9500 ? rankAtlas.GetSprite( "Ranking-S" ) :
                               data.accuracy >= 9000 ? rankAtlas.GetSprite( "Ranking-A" ) :
                               data.accuracy >= 8500 ? rankAtlas.GetSprite( "Ranking-B" ) :
                               data.accuracy >= 8000 ? rankAtlas.GetSprite( "Ranking-C" ) :
                                                       rankAtlas.GetSprite( "Ranking-D" );

        }
        rt.anchoredPosition = new Vector2( startPosX, rt.anchoredPosition.y );
        sequence.Restart();
    }
}