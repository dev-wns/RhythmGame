using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static List<MetaData> datas = new List<MetaData>();
    public static Dictionary<string, Sprite> backgrounds = new Dictionary<string, Sprite>();
    public static List<FMOD.Sound> sounds = new List<FMOD.Sound>();

    public delegate void OnLoad( float _offset );
    public static OnLoad loadProgress;

    public static MetaData SelectData = null;

    public static bool isDone { get; private set; } = false;

    private void Awake()
    {
        Application.targetFrameRate = 144;
        SoundManager.SoundRelease += Release;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

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
            StartCoroutine( BackgroundsLoad() );
        }
        Debug.Log( "Data Parsing Finish" );

        for ( int i = 0; i < datas.Count; i++ )
        {
            sounds.Add( SoundManager.Inst.Load( datas[i].audioPath, true ) );
        }
        Debug.Log( "Sounds Load Finish" );
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
                if ( backgrounds.ContainsKey( data.imgName ) ) continue;

                Texture2D tex = ( ( DownloadHandlerTexture )www.downloadHandler ).texture;

                Sprite sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ) );

                backgrounds.Add( data.imgName, sprite );
            }

            loadProgress( 1f / datas.Count );
        }

        isDone = true;
        Debug.Log( "Backgrounds Load Finish." );
    }


    private void Release()
    {
        foreach( var data in sounds )
        {
            data.release();
        }
    }
}
