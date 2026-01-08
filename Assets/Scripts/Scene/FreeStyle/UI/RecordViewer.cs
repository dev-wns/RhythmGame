using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordViewer : MonoBehaviour
{
    [SerializeField] private FreeStyle scene;
    [SerializeField] private RankImageSO rankImageSO;

    [SerializeField] private GameObject noRecord;
    [SerializeField] private GameObject infomation;

    private RectTransform rt;

    [Header("Information")]
    [SerializeField] private Image rankImage;
    [SerializeField] private TextMeshProUGUI chaos;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI rate;
    [SerializeField] private TextMeshProUGUI accuracy;
    [SerializeField] private TextMeshProUGUI date;

    [Header("Movement Effect")]
    [SerializeField] private float startPosX;
    private float endPosX;
    private readonly float duration = .25f;

    private float time;
    private Record record;

    private void Awake()
    {
        record = NowPlaying.Inst.CurrentRecord;
        scene.OnStart += UpdateRecord;
        rt = transform as RectTransform;

        endPosX = rt.anchoredPosition.x;
    }

    private void OnDestroy()
    {
        scene.OnStart -= UpdateRecord;
    }

    private void Update()
    {
        time += Time.deltaTime;

        float posX = Global.Math.Lerp( startPosX, endPosX, time / duration );
        rt.anchoredPosition = new Vector2( Global.Math.Clamp( posX, startPosX, endPosX ), rt.anchoredPosition.y );
    }

    public void UpdateRecord( in Song _song )
    {
        time = 0f;

        if ( !record.UpdateRecord() )
        {
            noRecord.SetActive( true );
            infomation.SetActive( false );
        }
        else
        {
            noRecord.SetActive( false );
            infomation.SetActive( true );

            RecordData data = record.CurrentData;
            chaos.SetText( data.random.ToString().Replace( '_', ' ' ) );
            score.text = $"{data.score:N0}";
            rate.SetText( "x{0:1}", data.pitch );
            accuracy.SetText( "{0:2}%", ( float )data.accuracy );
            date.SetText( data.date );

            rankImage.sprite = rankImageSO.GetSprite( data.accuracy );
        }
        
        rt.anchoredPosition = new Vector2( startPosX, rt.anchoredPosition.y );
    }
}