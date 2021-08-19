using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbySelector : MonoBehaviour
{
    public Image fadein;
    public Image fadeout;
    public Sprite[] images;
    public RectTransform textpos;

    private struct LBSeclect
    {
        public Sprite sprite;
        public int pos;

        public LBSeclect( Sprite _sprite, int _pos )
        {
            sprite = _sprite;
            pos = _pos;
        }
    }

    private List<LBSeclect> selects = new List<LBSeclect>();
    private int curIdx = 0;

    public delegate void SelectDel();
    public static event SelectDel LobbySelectEvent;

    void Start()
    {
        DOTween.Init();

        for ( int idx = 0; idx < 4; ++idx )
        {
            selects.Add( new LBSeclect( images[ idx ], 1400 - ( 1400 * idx ) ) );
        }
    }

    void Update()
    {
        if ( Input.GetKeyDown( KeyCode.LeftArrow ) )
        {
            if ( curIdx - 1 <= -1 )
            {
                curIdx = 0;
                return;
            }

            fadeout.DOFade( 1, 0 );
            fadeout.sprite = selects[ curIdx ].sprite;
            fadeout.DOFade( 0, 1 );

            --curIdx;

            fadein.DOFade( 0, 0 );
            fadein.sprite = selects[ curIdx ].sprite;
            fadein.DOFade( 1, 1 );

            textpos.DOLocalMoveX( 1400 - ( 1400 * curIdx ), 0.5f ).SetEase( Ease.OutBounce );
        }
        
        if ( Input.GetKeyDown( KeyCode.RightArrow ) )
        {
            if ( curIdx + 1 >= 4 )
            {
                curIdx = 3;
                return;
            }

            fadeout.DOFade( 1, 0 );
            fadeout.sprite = selects[ curIdx ].sprite;
            fadeout.DOFade( 0, 1 );

            ++curIdx;

            fadein.DOFade( 0, 0 );
            fadein.sprite = selects[ curIdx ].sprite;
            fadein.DOFade( 1, 1 );

            textpos.DOLocalMoveX( 1400 - ( 1400 * curIdx ), 0.5f ).SetEase( Ease.OutBounce );
        }
    }
}
