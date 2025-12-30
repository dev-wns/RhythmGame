using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Global.UI;

[RequireComponent( typeof( SpriteRenderer ) )]
public class SceneFader : MonoBehaviour
{
    private SpriteRenderer rdr;
    private static readonly float FadeTime     = .65f;
    private static readonly float FadeWaitTime = .025f;

    private static Sprite FadeSprite;

    public void Initialize()
    {
        rdr = GetComponent<SpriteRenderer>();

        if ( FadeSprite == null )
        {
            Texture2D tex = Texture2D.whiteTexture;
            Vector2 pivot = Alignment.GetPivotVector( Pivot.Center );
            FadeSprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), pivot, 100, 0, SpriteMeshType.FullRect );
        }

        rdr.sprite       = FadeSprite;
        rdr.sortingOrder = 100;
        rdr.drawMode     = SpriteDrawMode.Sliced;
        rdr.color        = Color.black;
        rdr.size         = new Vector2( 10000f, 10000f );

        transform.localScale = Vector3.one;
    }

    public async UniTask FadeIn()
    {
        rdr.color   = Color.black;
        rdr.enabled = true;

        await UniTask.WaitForSeconds( FadeWaitTime );
        await rdr.DOFade( 0f, FadeTime + ( FadeWaitTime * 2 ) );

        rdr.color   = Color.clear;
        rdr.enabled = false;
    }

    public async UniTask FadeOut()
    {
        rdr.color   = Color.clear;
        rdr.enabled = true;

        await UniTask.WaitForSeconds( FadeWaitTime );
        await rdr.DOFade( 1f, FadeTime + ( FadeWaitTime * 2 ) );

        rdr.color = Color.black;
    }
}
