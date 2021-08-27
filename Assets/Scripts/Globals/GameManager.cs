using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static List<MetaData> datas = new List<MetaData>();
    public static List<Sprite> backgrounds = new List<Sprite>();
    public static List<Sprite> previewBGs = new List<Sprite>();

    public delegate void OnLoad( float _offset );
    public static OnLoad loadProgress;

    public static bool isDone { get; private set; } = false;

    private void Awake()
    {
        // Osu Parsing
        FileReader parser = new FileReader();
        
        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo( Application.streamingAssetsPath + "/Songs" );
        foreach ( var dir in info.GetDirectories() )
        {
            foreach ( var file in dir.GetFiles( "*.osu" ) )
            {
                MetaData data = parser.Read( file.FullName );
                if ( ReferenceEquals( null, data ) )
                {
                    Debug.Log( "parsing failed. no data was created. #Path : " + file.FullName );
                }

                datas.Add( data );
            }
        }
        Debug.Log( "Osu FileParsing Finish" );

        StartCoroutine( BackgroundsLoad() );
    }

    private IEnumerator LobbySceneAsyncLoad()
    {
        AsyncOperation oper = SceneManager.LoadSceneAsync( "Lobby" );
        while( !oper.isDone )
        {
            yield return null;
        }
    }

    private IEnumerator BackgroundsLoad()
    {
        foreach ( var data in datas )
        {
            // backgrounds
            UnityWebRequest www = UnityWebRequestTexture.GetTexture( data.imgPath );
            yield return www.SendWebRequest();
            if ( www.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( www.error );
            }
            else
            {
                Texture2D tex = ( ( DownloadHandlerTexture )www.downloadHandler ).texture;

                Sprite sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ) );
                Sprite sprite2 = Sprite.Create( tex, new Rect( tex.width / 6, tex.height / 6, tex.width - ( tex.width / 6 * 2 ), tex.height - ( tex.height / 6 * 2 ) ), new Vector2( 0.5f, 0.5f ) );

                backgrounds.Add( sprite );
                previewBGs.Add( sprite2 );
            }

            loadProgress( 1f / datas.Count );
        }

        isDone = true;
        Debug.Log( "BackgroundsLoad Finish." );
    }
}
