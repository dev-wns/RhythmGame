namespace WNS.Time.Control
{
    using UnityEngine.UI;

    public abstract class BaseImage : BaseSequence
    {
        protected Image image;
        protected float end;
        protected float elapsed;
        protected float offset;
        protected bool  isFadeIn;

        public BaseImage( Image _image, float _end, float _duration )
                          : base( _duration )
        {
            image = _image;
            end = _end;
        }
    }
}