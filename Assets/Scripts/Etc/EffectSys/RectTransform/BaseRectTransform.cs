namespace WNS.Time.Control
{
    using UnityEngine;

    public abstract class BaseRectTransform : BaseSequence
    {
        protected RectTransform tf;
        protected Vector2 end;
        protected Vector2 elapsed;
        protected Vector2 offset;

        public BaseRectTransform( RectTransform _tf, Vector2 _end, float _duration )
                                  : base( _duration )
        {
            tf = _tf;
            end = _end;
        }
    }
}