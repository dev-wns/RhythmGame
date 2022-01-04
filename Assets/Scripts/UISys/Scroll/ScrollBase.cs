using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBase : MonoBehaviour
{
    public bool IsDuplicate { get; private set; }
    public bool IsLoop { get; protected set; } = false;

    protected int curIndex;
    protected int minIndex;
    protected int maxIndex;

    public virtual void PrevMove()
    {
        if ( curIndex == 0 )
        {
            if ( IsLoop )
            {
                curIndex = maxIndex - 1;
                return;
            }

            IsDuplicate = true;
            return;
        }

        --curIndex;
        IsDuplicate = false;
    }

    public virtual void NextMove()
    {
        if ( curIndex == maxIndex - 1 )
        {
            if ( IsLoop )
            {
                curIndex = 0;
                return;
            }

            IsDuplicate = true;
            return;
        }

        ++curIndex;
        IsDuplicate = false;
    }
}
