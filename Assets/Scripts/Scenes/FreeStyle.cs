using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FreeStyle : Scene
{
    [Serializable]
    public struct PreviewTexts
    {
        public TextMeshProUGUI time;
        public TextMeshProUGUI bpm;
        public TextMeshProUGUI combo;
        public TextMeshProUGUI record;
        public TextMeshProUGUI rate;
    }

    public PreviewTexts previewTexts;
    public Image background, previewBG;
    public GameObject prefab; // sound infomation prefab
    public RectTransform contents; // prefab parent

    private readonly float moveDuration = 105f;

    public short selectIdx = 0;
    public short focusIdx = 0;

    private struct PreviewData
    {
        public Sprite background;
        public GameObject soundElement;
        public PreviewData( Sprite _sprite, GameObject _object )
        {
            background = _sprite;
            soundElement = _object;
        }
    }
    private List<PreviewData> soundList = new List<PreviewData>();

    private PreviewData selectObject;
    private Transform selectTransform { get { return selectObject.soundElement.transform; } }
    public float movePos = 0f; // position shall be moved
    #region unity callbacks

    protected override void Awake()
    {
        base.Awake();

        DOTween.Init();
        foreach ( var data in GameManager.SoundInfomations )
        {
            GameObject obj = Instantiate( prefab, contents );
            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();
            info[ 0 ].text = data.Value.preview.name;
            info[ 1 ].text = data.Value.preview.artist;
            obj.name = data.Value.preview.name;

            SoundData soundData = GameManager.SoundInfomations[ obj.name ];
            byte[] byteTex = System.IO.File.ReadAllBytes( soundData.preview.img );
            if ( byteTex.Length > 0 )
            {
                Texture2D tex = new Texture2D( 0, 0 );
                tex.LoadImage( byteTex );
                Sprite sprite = Sprite.Create( tex, new Rect( new Vector2( 0f, 0f ), new Vector2( tex.width, tex.height ) ), new Vector2( 0.5f, 0.5f ) );

                soundList.Add( new PreviewData( sprite, obj ) );
            }
        }

        float width = contents.rect.width;
        contents.sizeDelta = new Vector2( width, soundList.Count * 105f );
        selectObject = soundList[ 0 ];
        selectObject.soundElement.transform.DOScale( new Vector3( 1f, 1f, 1f ), 0f );
        movePos = contents.localPosition.y;
        ShowSelectInfo();
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.UpArrow ) )
        {
            selectTransform.DOScale( new Vector3( 1f, 1f, 1f ), 0.1f );

            if ( focusIdx > 0 ) --focusIdx;
            if ( focusIdx == 0 && selectIdx > 0 )
            {
                movePos -= moveDuration;
                contents.DOLocalMoveY( movePos, 0.1f );
            }
            if ( selectIdx > 0 ) --selectIdx;

            selectObject = soundList[ selectIdx ];
            ShowSelectInfo();
            selectTransform.DOScale( new Vector3( 1.1f, 1.1f, 1f ), 0.1f );
        }

        if ( Input.GetKeyDown( KeyCode.DownArrow ) )
        {
            selectTransform.DOScale( new Vector3( 1f, 1f, 1f ), 0.1f );
            if ( focusIdx < 8 ) ++focusIdx;
            if ( focusIdx == 8 && selectIdx < soundList.Count - 1 )
            {
                movePos += moveDuration;
                contents.DOLocalMoveY( movePos, 0.1f );
            }
            if ( selectIdx < soundList.Count - 1 ) ++selectIdx;

            selectObject = soundList[ selectIdx ];
            ShowSelectInfo();
            selectTransform.DOScale( new Vector3( 1.1f, 1.1f, 1f ), 0.1f );
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            SceneChanger.Inst.Change( ( SceneType.Lobby ).ToString() );
        }
    }
#endregion

    private void ShowSelectInfo()
    {
        SoundData soundData = GameManager.SoundInfomations[ selectObject.soundElement.name ];
        previewTexts.bpm.text = soundData.timings[ 0 ].bpm.ToString();

        background.sprite = selectObject.background;
        previewBG.sprite = selectObject.background;
    }
}