using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DG.Tweening;

public class Lobby : Scene
{
    #region structures
    private struct ImageInfo
    {
        public Sprite sprite;
        public int pos; // 이동해야할 위치

        public ImageInfo( Sprite _sprite, int _pos )
        {
            sprite = _sprite;
            pos = _pos;
        }
    }
    #endregion

    #region variables
    public Sprite[] images;       // background에 사용될 texture
    public Image fadein, fadeout; // background image 2장
    public RectTransform textpos;  

    private List<ImageInfo> selects = new List<ImageInfo>();
    private int curIdx = 0;
    #endregion

    #region unity callback function
    protected override void Awake()
    {
        base.Awake();

        for ( int idx = 0; idx < 4; ++idx )
        {
            selects.Add( new ImageInfo( images[ idx ], 1400 - ( 1400 * idx ) ) );
        }
    }

    protected override void Start()
    {
        base.Start();

        DOTween.Init();
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.RightArrow ) )
        {
            if ( curIdx + 1 >= 4 )
            {
                curIdx = 3;
                return;
            }

            fadeout.sprite = selects[ curIdx ].sprite;
            FadeOut( fadeout );

            fadein.sprite = selects[ ++curIdx ].sprite;
            FadeIn( fadein );

            textpos.DOLocalMoveX( 1400 - ( 1400 * curIdx ), 0.5f ).SetEase( Ease.OutBounce );
            SfxPlay( clips.move );
        }

        if ( Input.GetKeyDown( KeyCode.LeftArrow ) )
        {

            if ( curIdx - 1 <= -1 )
            {
                curIdx = 0;
                return;
            }

            fadeout.sprite = selects[ curIdx ].sprite;
            FadeOut( fadeout );

            fadein.sprite = selects[ --curIdx ].sprite;
            FadeIn( fadein );

            textpos.DOLocalMoveX( 1400 - ( 1400 * curIdx ), 0.5f ).SetEase( Ease.OutBounce );
            SfxPlay( clips.move );
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            SceneChanger.Inst.Change( ( ( SceneType )curIdx ).ToString() );
        }
    }
    #endregion

    #region customize function
    private void FadeOut( Image _obj )
    {
        _obj.DOFade( 1, 0 );
        _obj.DOFade( 0, 1 );
    }

    private void FadeIn( Image _obj )
    {
        _obj.DOFade( 0, 0 );
        _obj.DOFade( 1, 1 );
    }
    #endregion
}
