using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomVerticalLayoutGroup : CustomLayoutGroup
{
    public override void Initialize()
    {
        base.Initialize();
        rectChildren.Reverse();
    }

    public override void SetLayoutVertical()
    {
        if ( rectChildren.Count < 1 )
             return;

        float childrenMaxWidth  = 0f;
        float childrenMaxHeight = 0f;
        float maxSpacing        = 0f;
        for ( int i = 0; i < rectChildren.Count; i++ )
        {
            if ( !rectChildren[i].gameObject.activeInHierarchy )
                continue;

            childrenMaxWidth  += rectChildren[i].sizeDelta.x;
            childrenMaxHeight += rectChildren[i].sizeDelta.y;
            maxSpacing        += spacing * anchor.y;
        }

        float childrenHeightOffset = childrenMaxHeight * anchor.y;
        float firstArgHalf         = rectChildren[0].sizeDelta.y * .5f;

        float widthOffset  = -rectChildren[0].sizeDelta.x * ( anchor.x - .5f );
        float heightOffset = firstArgHalf - childrenHeightOffset - maxSpacing;
        for ( int i = 0; i < rectChildren.Count; i++ )
        {
            var child = rectChildren[i];
            if ( !child.gameObject.activeInHierarchy )
                continue;

            child.anchoredPosition = new Vector2( widthOffset + padding.left - padding.right,
                                                  heightOffset + padding.bottom - padding.top );

            heightOffset += ( rectChildren[i].sizeDelta.y * .5f ) + spacing;
            if ( i + 1 < rectChildren.Count )
                heightOffset += ( rectChildren[i + 1].sizeDelta.y * .5f );
        }
    }

    public override void SetLayoutHorizontal() { }
}
