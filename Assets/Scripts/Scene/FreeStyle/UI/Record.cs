using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Record : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;

    public GameObject noRecord;
    public GameObject infomation;

    private RectTransform rt;

    [Header("Infomation")]
    public Image rankImage;
    public TextMeshProUGUI chaos;
    public TextMeshProUGUI score;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI date;

    [Header( "Rank Image" )]
    public Sprite RankS;
    public Sprite RankA;
    public Sprite RankB;
    public Sprite RankC;
    public Sprite RankD;

    [Header("Movement Effect")]
    public float startPosX;
    private float endPosX;
    private readonly float duration = .25f;

    private float time;

    private void Awake()
    {
        mainScroll.OnSelectSong += UpdateRecord;
        rt = transform as RectTransform;

        endPosX = rt.anchoredPosition.x;
    }

    private void Update()
    {
        time += Time.deltaTime;

        float posX = Global.Math.Lerp( startPosX, endPosX, time / duration );
        rt.anchoredPosition = new Vector2( Global.Math.Clamp( posX, startPosX, endPosX ), rt.anchoredPosition.y );
    }

    public void UpdateRecord( Song _song )
    {
        time = 0f;

        if ( !DataStorage.Inst.UpdateRecord() )
        {
            noRecord.SetActive( true );
            infomation.SetActive( false );
        }
        else
        {
            noRecord.SetActive( false );
            infomation.SetActive( true );

            RecordData data = DataStorage.CurrentRecord;
            chaos.SetText( data.random.ToString().Replace( '_', ' ' ) );
            score.text = $"{data.score:N0}";
            rate.SetText( "x{0:1}", data.pitch );
            accuracy.SetText( "{0:2}%", ( float )data.accuracy );
            date.SetText( data.date );

            rankImage.sprite = data.accuracy >= 95d ? RankS :
                               data.accuracy >= 90d ? RankA :
                               data.accuracy >= 85d ? RankB :
                               data.accuracy >= 80d ? RankC :
                                                      RankD;
        }
        
        rt.anchoredPosition = new Vector2( startPosX, rt.anchoredPosition.y );
    }
}