using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class ComboSystem : MonoBehaviour
{
    public SpriteAtlas atlas;
    public List<Image>  images = new List<Image>();
    public Judgement judge;
    private int combo;

    private void Awake()
    {
        judge.OnJudge += ComboImageUpdate;
    }

    private void ComboImageUpdate( bool _hasJudge )
    {
        combo = _hasJudge ? combo += 1 : 0;
        if ( combo == 0 )
        {
            images[6].sprite = atlas.GetSprite( $"score-0" );
            for ( int i = 5; i >= 0; i-- )
            {
                if ( images[i].isActiveAndEnabled )
                     images[i].gameObject.SetActive( false );
            }

            return;
        }


        float calcCombo = combo;
        int num = ( int )Mathf.Log10( calcCombo ) + 1;
        Debug.Log( num );
        for ( int i = 6; i >= 0; i-- )
        {
            if ( i < 7 - num )
            {
                if ( images[i].isActiveAndEnabled )
                     images[i].gameObject.SetActive( false );
            }
            else
            {
                if ( !images[i].isActiveAndEnabled )
                     images[i].gameObject.SetActive( true );
                images[i].sprite = atlas.GetSprite($"score-{( int )calcCombo % 10}");
                calcCombo *= .1f;
            }
        }
    }
}
