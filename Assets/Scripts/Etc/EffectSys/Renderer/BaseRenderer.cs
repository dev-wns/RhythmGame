namespace WNS.Time.Control
{
    using UnityEngine;

    public abstract class BaseRenderer : BaseSequence
    {
        protected SpriteRenderer rdr;
        protected float end;
        protected float elapsed;
        protected float offset;
        protected bool isFadeIn;
        protected bool isDuplicateValue;

        public BaseRenderer( SpriteRenderer _rdr, float _end, float _duration )
                             : base( _duration )
        {
            rdr = _rdr;
            end = _end;
        }
    }
}