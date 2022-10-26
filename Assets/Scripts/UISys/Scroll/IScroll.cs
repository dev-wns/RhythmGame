using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IScroll
{
    public bool IsDuplicate { get; }
    public bool IsLoop { get; }
    public int curIndex { get; }
    public int prevIndex { get; }
    public int maxCount { get; }

    public void PrevMove();
    public void NextMove();
}