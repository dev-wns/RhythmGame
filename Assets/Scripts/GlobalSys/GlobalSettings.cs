using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public enum KeyAction { _0, _1, _2, _3, _4, _5, KEYCOUNT };
public enum Resolution { _1366_768, _1400_900, _1600_900, _1600_1024, _1920_1080 };

[System.Serializable]
public static class KeySetting
{
    public static Dictionary<KeyAction, KeyCode> keys = new Dictionary<KeyAction, KeyCode>();
}

public class GlobalSettings : MonoBehaviour
{
    public static Dictionary<string, Sprite> spriteMap;

    public static int[] judgeMS = { 0, 1, 2, 3 };

    public static float scrollSpeed = 2.4f;
    public static float stagePosX = 0.0f;
    public static float stagePosY = 0.85f;
    public static float colWidth = 0.85f;
    public static float globalOffset = 0.0f;
    
    public static float hpKoolRecover;
    public static float hpCoolRecover;
    public static float hpBadDamage;
    public static float hpMissDamage;

    public static string sortSearch = "";
    public static string playerName = "Guest";

    public static string folderPath = Path.Combine( Application.streamingAssetsPath, "Songs" );

    public static float volume = 1.0f;

    public static int decide;
    public static int diffSelection;
    public static int sortSelection;
    public static int modSelection;
    public static int specialSelection;

    public static int keyCount = 6;
    public static int UID = 2;

    public static bool isFixedScroll = true;
    public static bool isPlayVideo   = true;
    public static bool isFullScreen  = false;
    public static bool isMirror      = false;
    public static bool isRandom      = false;
    public static bool isAutoPlay    = false;

    public int res;
    public int fps;

    private KeyCode[] defaultKeys = new KeyCode[] { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.K, KeyCode.L, KeyCode.Semicolon };
    private KeyCode[] newKeys     = new KeyCode[] { KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.K, KeyCode.L, KeyCode.Semicolon };

    private void Awake ()
    {
        // 저장값 불러오기
        if ( PlayerPrefs.HasKey( "K0" ) )
        {
            newKeys[ 0 ] = ( KeyCode )PlayerPrefs.GetInt( "K0" );
            newKeys[ 1 ] = ( KeyCode )PlayerPrefs.GetInt( "K1" );
            newKeys[ 2 ] = ( KeyCode )PlayerPrefs.GetInt( "K2" );
            newKeys[ 3 ] = ( KeyCode )PlayerPrefs.GetInt( "K3" );
            newKeys[ 4 ] = ( KeyCode )PlayerPrefs.GetInt( "K4" );
            newKeys[ 5 ] = ( KeyCode )PlayerPrefs.GetInt( "K5" );

            for ( int i = 0; i < ( int )KeyAction.KEYCOUNT; ++i )
            {
                KeySetting.keys.Add( ( KeyAction )i, newKeys[ i ] );
            }
        }
        else
        {
            // 초기값
            for ( int i = 0; i < ( int )KeyAction.KEYCOUNT; ++i )
            {
                KeySetting.keys.Add( ( KeyAction )i, defaultKeys[ i ] );
            }
        }

        fps = 3;
        res = ( int )Resolution._1920_1080;

        hpKoolRecover = 0.003f; // Kool 회복량
        hpCoolRecover = 0.001f; // Cool 회복량
        hpBadDamage = 0.013f;
        hpMissDamage = 0.026f;
    }

    public static void SaveControlKeyBinds ()
    {
        int key;
        key = ( int )KeySetting.keys[ KeyAction._0 ];
        PlayerPrefs.SetInt( "K0", key );
        key = ( int )KeySetting.keys[ KeyAction._1 ];
        PlayerPrefs.SetInt( "K1", key );
        key = ( int )KeySetting.keys[ KeyAction._2 ];
        PlayerPrefs.SetInt( "K2", key );
        key = ( int )KeySetting.keys[ KeyAction._3 ];
        PlayerPrefs.SetInt( "K3", key );
        key = ( int )KeySetting.keys[ KeyAction._4 ];
        PlayerPrefs.SetInt( "K4", key );
        key = ( int )KeySetting.keys[ KeyAction._5 ];
        PlayerPrefs.SetInt( "K5", key );

        PlayerPrefs.Save();
    }

    public void SaveSetting ()
    {
        PlayerPrefs.SetInt( "SCROLL", Global.BoolToInt( isFixedScroll ) );
        PlayerPrefs.SetInt( "VIDEO", Global.BoolToInt( isPlayVideo ) );
        PlayerPrefs.SetInt( "FULLSCREEN", Global.BoolToInt( isFullScreen ) );
        PlayerPrefs.SetInt( "RESOLUTION", res );
        PlayerPrefs.SetInt( "FPS", fps );
        PlayerPrefs.SetFloat( "VOLUME", volume );
        PlayerPrefs.SetFloat( "GOFFSET", globalOffset );
        PlayerPrefs.SetFloat( "CW", colWidth );
        PlayerPrefs.SetFloat( "XX", stagePosX );
        PlayerPrefs.SetFloat( "YY", stagePosY );
        PlayerPrefs.SetString( "FOLDER", folderPath );
    }

    public void SaveSelection()
    {
        PlayerPrefs.SetFloat( "SPEED", scrollSpeed );
        PlayerPrefs.SetInt( "MOD", modSelection );
    }

    private void LoadSetting()
    {
        if ( PlayerPrefs.HasKey( "SCROLL" ) )
        {
            isFixedScroll = Global.IntToBool( PlayerPrefs.GetInt( "SCROLL" ) );
            isPlayVideo = Global.IntToBool( PlayerPrefs.GetInt( "VIDEO" ) );
            isFullScreen = Global.IntToBool( PlayerPrefs.GetInt( "FULLSCREEN" ) );
            res = PlayerPrefs.GetInt( "RESOLUTION" );
            fps = PlayerPrefs.GetInt( "FPS" );
            volume = PlayerPrefs.GetFloat( "VOLUME" );
            globalOffset = PlayerPrefs.GetFloat( "GOFFSET", 0 );
            colWidth = PlayerPrefs.GetFloat( "CW", 0.85f );
            stagePosX = PlayerPrefs.GetFloat( "XX", 0 );
            stagePosY = PlayerPrefs.GetFloat( "YY", 0 );
        }

        SwitchResolution();
        SwitchFrameRate();

        if ( isFullScreen )
            Screen.fullScreen = true;
        else
            Screen.fullScreen = false;
    }

    private void LoadSelection()
    {
        if ( PlayerPrefs.HasKey( "SPEED" ) )
        {
            scrollSpeed = PlayerPrefs.GetFloat( "SPPED" );
            modSelection = PlayerPrefs.GetInt( "MOD" );
        }
    }

    private void SwitchResolution()
    {
        switch ( res )
        {
            case ( int )Resolution._1366_768:
                Screen.SetResolution( 1366, 768, isFullScreen );
                break;
            case ( int )Resolution._1400_900:
                Screen.SetResolution( 1400, 900, isFullScreen );
                break;
            case ( int )Resolution._1600_900:
                Screen.SetResolution( 1600, 900, isFullScreen );
                break;
            case ( int )Resolution._1600_1024:
                Screen.SetResolution( 1600, 1024, isFullScreen );
                break;
            case ( int )Resolution._1920_1080:
                Screen.SetResolution( 1920, 1080, isFullScreen );
                break;
        }
    }

    private void SwitchFrameRate()
    {
        switch ( fps )
        {
            case 0:
                Application.targetFrameRate = 60;
                break;
            case 1:
                Application.targetFrameRate = 144;
                break;
            case 2:
                Application.targetFrameRate = 240;
                break;
            case 3:
                Application.targetFrameRate = 1000;
                break;
        }
    }
}
