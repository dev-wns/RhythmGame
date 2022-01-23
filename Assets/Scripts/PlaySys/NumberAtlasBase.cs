using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class NumberAtlasBase : MonoBehaviour
{
    [Header("Number Atlas")]
    public SpriteAtlas atlas;
    public List<Image> images   = new List<Image>();
    public List<Sprite> sprites = new List<Sprite>();

    protected virtual void Awake()
    {
        if ( images.Count > 0 )
             images.Reverse();

        //Sprite[] spriteArray = new Sprite[atlas.spriteCount];
        //atlas.GetSprites( spriteArray );
        //sprites.AddRange( spriteArray );

        //sprites.Sort( ( Sprite A, Sprite B ) =>
        //{
        //    int AValue = int.Parse( Regex.Replace( A.name, @"[^0-9]", "" ) );
        //    int BValue = int.Parse( Regex.Replace( B.name, @"[^0-9]", "" ) );

        //    if ( AValue < BValue )      return -1;
        //    else if ( AValue > BValue ) return 1;
        //    else                        return 0;
        //} );
    }
}
