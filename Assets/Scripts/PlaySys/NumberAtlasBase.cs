using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class NumberAtlasBase : MonoBehaviour
{
    [Header("Number Atlas")]
    public SpriteAtlas atlas;
    public List<Image> images      = new List<Image>();
    protected List<Sprite> sprites = new List<Sprite>();

    protected virtual void Awake()
    {
        if ( images.Count == 0 )
             Debug.LogError( $"The number of images is 0. Please add an image to the list." );

        images.Reverse();

        Sprite[] spriteArray = new Sprite[atlas.spriteCount];
        atlas.GetSprites( spriteArray );
        sprites.AddRange( spriteArray );

        sprites.Sort( ( Sprite A, Sprite B ) => A.name.CompareTo( B.name ) );
    }
}
