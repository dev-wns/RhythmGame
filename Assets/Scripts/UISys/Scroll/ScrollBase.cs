using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBase : MonoBehaviour, IScroll
{
    public bool IsDuplicate { get; private set; }
    public bool IsLoop { get; set; } = false;

    public int curIndex  { get; protected set; }
    public int prevIndex { get; protected set; }
    public int maxCount  { get; protected set; }

    public int Index;

    protected virtual void SelectPosition( int _pos )
    {
        if ( maxCount <= 0 ) return;

        curIndex = _pos;
    }

    public virtual void PrevMove()
    {
        if ( curIndex == 0 )
        {
            if ( IsLoop )
            {
                curIndex = maxCount - 1;
                return;
            }

            IsDuplicate = true;
            return;
        }

        prevIndex = curIndex--;
        IsDuplicate = false;
        Index = curIndex;
    }

    public virtual void NextMove()
    {
        if ( curIndex == maxCount - 1 )
        {
            if ( IsLoop )
            {
                curIndex = 0;
                return;
            }

            IsDuplicate = true;
            return;
        }

        prevIndex = curIndex++;
        IsDuplicate = false;
        Index = curIndex;
    }
}
