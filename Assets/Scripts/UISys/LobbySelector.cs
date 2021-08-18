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
    private int curIdx = 0;
    private Sequence fadeIn, fadeOut;

    private struct LBSeclect
    {
        public Sprite sprite;
        public float pos;

        public LBSeclect( Sprite _sprite, float _pos )
        {
            sprite = _sprite;
            pos = _pos;
        }
    }
    private List<LBSeclect> selects = new List<LBSeclect>();

    void Start()
    {
        DOTween.Init();
        fadeIn = DOTween.Sequence();
        fadeOut = DOTween.Sequence();

        for ( int idx = 0; idx < 4; ++idx )
        {
            selects.Add( new LBSeclect( images[ idx ], -1400f + ( 1400f * idx ) ) );
        }

        fadeIn.OnStart( () =>
        {
            if ( curIdx < -1 && curIdx > 4 )
                return;

            fadein.DOFade( 0, 0 );
            fadein.sprite = selects[ curIdx ].sprite;
            fadeout.sprite = selects[ curIdx ].sprite;
        } );
        fadeIn.Append( fadein.DOFade( 1, 1 ) );

        fadeOut.OnStart( () =>
        {
            if ( curIdx < -1 && curIdx > 4 )
                return;

            fadeout.DOFade( 1, 0 );
            fadein.sprite = selects[ curIdx ].sprite;
            fadeout.sprite = selects[ curIdx ].sprite;
        } );
        fadeIn.Append( fadein.DOFade( 0, 1 ) );
    }

    // Update is called once per frame
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
        }
    }
}
