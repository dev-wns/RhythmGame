using UnityEngine;

public class CustomHorizontalLayoutGroup : CustomLayoutGroup
{
    public override void SetLayoutHorizontal()
    {
        if ( rectChildren.Count < 1 )
            return;

        float childrenMaxWidth  = 0f;
        float childrenMaxHeight = 0f;
        float maxSpacing        = 0f;
        for ( int i = 0; i < rectChildren.Count; i++ )
        {
            if ( !ShouldIncludeDisabledObject && !rectChildren[i].gameObject.activeSelf )
                continue;

            childrenMaxWidth += rectChildren[i].sizeDelta.x;
            childrenMaxHeight += rectChildren[i].sizeDelta.y;
            maxSpacing += spacing * anchor.x;
        }

        float childrenWidthOffset = childrenMaxWidth * anchor.x;
        float firstArgHalf        = rectChildren[0].sizeDelta.x * .5f;

        float widthOffset  = firstArgHalf - childrenWidthOffset - maxSpacing + ( spacing * .5f );
        float heightOffset = -rectChildren[0].sizeDelta.y * ( anchor.y - .5f );
        for ( int i = 0; i < rectChildren.Count; i++ )
        {
            var child = rectChildren[i];
            if ( !ShouldIncludeDisabledObject && !child.gameObject.activeInHierarchy )
                continue;

            child.anchoredPosition = new Vector2( widthOffset + padding.left - padding.right,
                                                  heightOffset + padding.bottom - padding.top );

            widthOffset += ( rectChildren[i].sizeDelta.x * .5f ) + spacing;
            if ( i + 1 < rectChildren.Count )
                widthOffset += ( rectChildren[i + 1].sizeDelta.x * .5f );
        }
    }

    public override void SetLayoutVertical() { }
}
