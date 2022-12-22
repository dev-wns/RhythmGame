namespace WNS.Time.Control
{
    using UnityEngine;

    public abstract class BaseTransform : BaseSequence
    {
        protected Transform tf;
        protected Vector3 end;
        protected Vector3 elapsed;
        protected Vector3 offset;

        public BaseTransform( Transform _tf, Vector3 _end, float _duration )
                              : base( _duration )
        {
            tf = _tf;
            end = _end;
        }
    }
}