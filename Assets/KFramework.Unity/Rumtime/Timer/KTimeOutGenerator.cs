using UnityEngine;

namespace KFramework.MonoGame
{
    public class KTimeOutGenerator
    {
        float fLastUpdateTime = 0;
        float fInternalTime = 0;
        public static KTimeOutGenerator New(float fInternalTime)
        {
            var temp = new KTimeOutGenerator();
            temp.Init(fInternalTime);
            return temp;
        }

        public void Init(float fInternalTime = 1.0f)
        {
            this.fInternalTime = fInternalTime;
            this.Reset();
        }

        public void Reset()
        {
            this.fLastUpdateTime = Time.time;
        }

        public bool orTimeOut()
        {
            if ((Time.time - fLastUpdateTime) > fInternalTime)
            {
                this.Reset();
                return true;
            }

            return false;
        }

        public bool orTimeOutWithSpecialTime(float fInternalTime)
        {
            if (Time.time - fLastUpdateTime > fInternalTime)
            {
                this.Reset();
                return true;
            }

            return false;
        }
    }
}
