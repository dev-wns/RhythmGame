using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.U2D;
using DG.Tweening;
using System;

public class ResultInfomation : MonoBehaviour
{
    public SpriteAtlas rankAtlas;

    [Header( "Song Infomation" )]
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;

    [Header( "Hit Count" )]
    public TextMeshProUGUI totalNotes;
    public TextMeshProUGUI perfect, great, good, bad, miss;

    [Header( "Fast Slow" )]
    public TextMeshProUGUI bpm;
    public TextMeshProUGUI fast, slow; 

    [Header( "Result" )]
    public TextMeshProUGUI maxCombo;
    public TextMeshProUGUI score;
    public TextMeshProUGUI accuracy;

    [Header( "Rank" )]
    public Image rank;

    [Header( "Today" )]
    public TextMeshProUGUI date;

    [Header( "Background" )]
    public Sprite defaultOrigin;
    public Image originBg;
    private Sprite spriteBg;
    private Texture2D texture;

    private readonly float duration = 1f;
    private void TextProgressEffect( in TextMeshProUGUI _text, int _value ) => _text.text = $"{_value:N0}";

    private void Awake()
    {
        Result scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Result>();

        var record = NowPlaying.Inst.MakeNewRecord();
        var song   = NowPlaying.CurrentSong;
        var result = NowPlaying.Inst.CurrentResult;
        // Song Infomation
        title.text  = song.title;
        artist.text = song.artist;

        // Hit Count 
        totalNotes.text = ( song.noteCount + song.sliderCount ).ToString();

        // fast slow
        fast.text = result.fast.ToString();
        slow.text = result.slow.ToString();

        // bpm
        var pitch = GameSetting.CurrentPitch;
        if ( Global.Math.Abs( pitch - 1f ) < .0001f )
        {
            int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm );
            if ( song.minBpm == song.maxBpm ) bpm.text = medianBpm.ToString();
            else                              bpm.text = $"{medianBpm} ({song.minBpm} ~ {song.maxBpm})";
        }
        else
        {
            int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm * pitch  );
            if ( song.minBpm == song.maxBpm ) bpm.text = medianBpm.ToString();
            else                              bpm.text = $"{medianBpm} ({Mathf.RoundToInt( song.minBpm * pitch )} ~ {Mathf.RoundToInt( song.maxBpm * pitch )})";
        }
        bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                    pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        // Score
        rank.sprite = result.accuracy >= 9500 ? rankAtlas.GetSprite( "Ranking-S" ) :
                      result.accuracy >= 9000 ? rankAtlas.GetSprite( "Ranking-A" ) :
                      result.accuracy >= 8500 ? rankAtlas.GetSprite( "Ranking-B" ) :
                      result.accuracy >= 8000 ? rankAtlas.GetSprite( "Ranking-C" ) :
                                                rankAtlas.GetSprite( "Ranking-D" );

        //date.text = DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" );
        date.text = record.date;

        // Background
        StartCoroutine( LoadBackground( NowPlaying.CurrentSong.imagePath ) );

        DOTween.To( () => 0, x => TextProgressEffect( perfect, x  ),     result.maximum + result.perfect,  duration );
        DOTween.To( () => 0, x => TextProgressEffect( great, x    ),     result.great,                     duration );
        DOTween.To( () => 0, x => TextProgressEffect( good, x     ),     result.good,                      duration );
        DOTween.To( () => 0, x => TextProgressEffect( bad, x      ),     result.bad,                       duration );
        DOTween.To( () => 0, x => TextProgressEffect( miss, x     ),     result.miss,                      duration );
        DOTween.To( () => 0, x => TextProgressEffect( maxCombo, x ),     result.combo,                     duration );
        DOTween.To( () => 0, x => TextProgressEffect( score, x    ),     result.score,                     duration );
        DOTween.To( () => 0, x => accuracy.text = $"{( x * .01d ):F2}%", result.accuracy,                  duration );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if ( texture != null )
             DestroyImmediate( texture );
    }

    private IEnumerator LoadBackground( string _path )
    {
        if ( !System.IO.File.Exists( _path ) )
        {
            originBg.sprite = defaultOrigin;
            originBg.rectTransform.sizeDelta = Global.Math.GetScreenRatio( defaultOrigin.texture, new Vector2( Screen.width, Screen.height ) );
            yield break;
        }

        var ext = System.IO.Path.GetExtension( _path );
        if ( ext.Contains( ".bmp" ) )
        {
            BMPLoader loader = new BMPLoader();
            BMPImage img = loader.LoadBMP( _path );
            texture = img.ToTexture2D();
            spriteBg = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
        }
        else
        {
            using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path ) )
            {
                www.method = UnityWebRequest.kHttpVerbGET;
                using ( DownloadHandlerTexture handler = new DownloadHandlerTexture() )
                {
                    www.downloadHandler = handler;
                    yield return www.SendWebRequest();

                    if ( www.result == UnityWebRequest.Result.ConnectionError ||
                         www.result == UnityWebRequest.Result.ProtocolError )
                    {
                        Debug.LogError( $"UnityWebRequest Error : {www.error}" );
                        throw new Exception( $"UnityWebRequest Error : {www.error}" );
                    }

                    texture = handler.texture;
                    spriteBg = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
                }
            }
        }

        originBg.sprite = spriteBg;
        originBg.rectTransform.sizeDelta = Global.Math.GetScreenRatio( spriteBg.texture, new Vector2( Screen.width, Screen.height ) );
    }
}