using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sound = FMOD.Sound;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class FreeStyle : MonoBehaviour
{
    public GameObject prefab; // sound infomation prefab
    public Transform scrollSoundsContent;
    public VerticalScrollSnap snap;

    public TextMeshProUGUI time, bpm, combo, record, rate;
    public Image background, previewBG;

    private List<Sprite> backgrounds = new List<Sprite>();
    private List<Sprite> previewBGs = new List<Sprite>();

    private struct PreviewSound
    {
        public Sound sound;
        public int time;

        public PreviewSound( Sound _sound, int _time)
        {
            sound = _sound;
            time = _time;
        }
    }

    private List<PreviewSound> previewSounds = new List<PreviewSound>();
    Sound sound;
    #region unity callbacks
    protected void Start()
    {
        SoundManager.SoundRelease += Release;
        SoundManager.Inst.Volume = 0.1f;

        StartCoroutine( SpriteLoad() );
        SoundLoadAsync();

        previewSounds.Capacity = GameManager.songs.Count;
        foreach ( var data in GameManager.songs )
        {
            // scrollview song contents
            GameObject obj = Instantiate( prefab, scrollSoundsContent );
            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();
            info[0].text = data.preview.name;
            info[1].text = data.preview.artist;
            obj.name = data.preview.name;



            //byte[] byteTex = System.IO.File.ReadAllBytes( data.preview.img );
            //if ( byteTex.Length > 0 )
            //{
            //    Texture2D tex = new Texture2D( 0, 0 );
            //    tex.LoadImage( byteTex );
            //    Sprite sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ) );
            //    Sprite sprite2 = Sprite.Create( tex, new Rect( tex.width / 6, tex.height / 6, tex.width - ( tex.width / 6 * 2 ), tex.height - ( tex.height / 6 * 2 ) ), new Vector2( 0.5f, 0.5f ) );

            //    backgrounds.Add( sprite );
            //    previewBGs.Add( sprite2 );
            //}
        }

        // details
        if ( GameManager.songs.Count > 0 )
        {
            ChangePreview();
        }
    }

    private async void SoundLoadAsync()
    {
        await Task.Run( () =>
        {
            foreach ( var data in GameManager.songs )
            {
                // preview sounds loading
                Sound sound = SoundManager.Inst.Load( data.preview.path, true );
                previewSounds.Add( new PreviewSound( sound, data.preview.time ) );
                Debug.Log( "Load : " + data.preview.name );
            }
        } );
    }

    private IEnumerator SpriteLoad()
    {
        foreach ( var data in GameManager.songs )
        {
            // backgrounds
            UnityWebRequest www = UnityWebRequestTexture.GetTexture( data.preview.img );
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
        }
    }

    private void Load( int _idx )
    {
        sound = SoundManager.Inst.Load( GameManager.songs[_idx].preview.path, true );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.UpArrow ) ) 
        {
            snap.SnapUp();
            ChangePreview();
        }
        if ( Input.GetKeyDown( KeyCode.DownArrow ) ) 
        {
            snap.SnapDown();
            ChangePreview();
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            SceneChanger.Inst.Change( "Lobby" );
        }
    }

    private void ChangePreview()
    {
        if ( snap.IsDuplicateKeyCheck ) return;
        ChangePreviewInfo();
        PreviewSoundPlay();
    }

    private void ChangePreviewInfo()
    {
        Song song = GameManager.songs[snap.SelectIndex];
        bpm.text = song.timings[0].bpm.ToString();

        background.sprite = backgrounds[snap.SelectIndex];
        previewBG.sprite = previewBGs[snap.SelectIndex];
    }

    private void PreviewSoundPlay()
    {
        SoundManager.Inst.Stop();
        SoundManager.Inst.Play( previewSounds[snap.SelectIndex].sound );

        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );

        int time = previewSounds[snap.SelectIndex].time;
        if ( time <= 0 )
        {
            uint length = 0;
            previewSounds[snap.SelectIndex].sound.getLength( out length, FMOD.TIMEUNIT.MS );
            time = ( int )( length / 3.65f );
        }

        channel.setPosition( ( uint )time, FMOD.TIMEUNIT.MS );
    }

    private void Release()
    {
        foreach ( var music in previewSounds )
        {
            FMOD.RESULT res = music.sound.release();
            if ( res != FMOD.RESULT.OK )
            {
                Debug.LogError( "sound release failed." );
            }
        }
        Debug.Log( "FreeStyle preview Sounds release" );
    }
    #endregion
}