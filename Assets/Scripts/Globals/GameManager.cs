using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Dictionary<string /* sound name */, SoundData> SoundInfomations = new Dictionary<string, SoundData>();

    public delegate void InitLoading();
    public static event InitLoading GameInit;

    private void Initialize()
    {
        // Osu Parsing
        FileReader parser = new FileReader();
        foreach ( var path in FileReader.GetFiles( "/Songs", "*.osu" ) )
        {
            SoundData soundInfo = parser.Read( path );
            if ( ReferenceEquals( null, soundInfo ) )
            {
                Debug.Log( "parsing failed. no data was created. #Path : " + path );
            }

            SoundInfomations.Add( soundInfo.preview.name, soundInfo );
        }

        Debug.Log( "GamaManager Initizlize Successful." );
    }

    private void Awake()
    {
        GameInit += Initialize;
    }

    private void Start()
    {
        GameInit();
        SceneChanger.Inst.Change( "Lobby" );
    }
}
