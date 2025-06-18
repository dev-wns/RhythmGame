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
            chaos.text = $"{( ( GameRandom )data.random ).ToString().Replace( '_', ' ' )}";
            score.text = $"{data.score:N0}";
            rate.text = $"x{data.pitch:N1}";
            accuracy.text = $"{( data.accuracy * .01f ):N2}%";
            date.text = data.date;

            rankImage.sprite = data.accuracy >= 9500 ? RankS :
                               data.accuracy >= 9000 ? RankA :
                               data.accuracy >= 8500 ? RankB :
                               data.accuracy >= 8000 ? RankC :
                                                       RankD;
        }
        
        rt.anchoredPosition = new Vector2( startPosX, rt.anchoredPosition.y );
    }
}