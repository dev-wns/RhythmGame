using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalScrollBase : MonoBehaviour
{
    public GameObject[] contents;

    public bool IsDuplicate { get; private set; }
    protected GameObject curOption { get; private set; }
    private int curIndex;

    protected virtual void Awake()
    {
        if ( contents.Length > 0 )
        {
            curIndex = 0;
            curOption = contents[0];
        }
    }

    protected virtual void PrevMove()
    {
        if ( curIndex == 0 )
        {
            IsDuplicate = true;
            return;
        }

        curOption = contents[--curIndex];
        IsDuplicate = false;
    }

    protected virtual void NextMove()
    {
        if ( curIndex == contents.Length - 1 )
        {
            IsDuplicate = true;
            return;
        }

        curOption = contents[++curIndex];
        IsDuplicate = false;
    }
}
