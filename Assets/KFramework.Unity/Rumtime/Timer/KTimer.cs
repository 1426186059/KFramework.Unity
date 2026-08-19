using System;
using UnityEngine;

namespace KFramework.MonoGame
{
    public class KTimer
    {
        bool unscaled = false;
        int loop = 0;
        float duration = 0f;
        float time = 0f;
        bool running = false;
        Action func;
        public GameObject go;

        public static KTimer New(GameObject go, Action func, float duration, int loop = 1, bool unscaled = false)
        {
            var o = new KTimer();
            o.go = go;
            o.func = func;
            o.duration = duration;
            o.time = duration;
            o.loop = loop;
            o.unscaled = unscaled;
            o.running = false;
            return o;
        }

        public static KTimer New(Action func, float duration, int loop = 1, bool unscaled = false)
        {
            var o = new KTimer();
            o.go = KUpdateMgr.Instance.gameObject;
            o.func = func;
            o.duration = duration;
            o.time = duration;
            o.loop = loop;
            o.unscaled = unscaled;
            o.running = false;
            return o;
        }

        public void Start()
        {
            this.running = true;
            KUpdateMgr.Instance.AddListener(this.Update);
        }

        public void Stop()
        {
            this.running = false;
            KUpdateMgr.Instance.RemoveListener(this.Update);
        }
        
        private void Update()
        {
            if (!this.running)
            {
                return;
            }

            if (go == null)
            {
                this.Stop();
                return;
            }

            float delta = this.unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            this.time = this.time - delta;

            if (this.time <= 0)
            {
                this.func();

                if (this.loop > 0)
                {
                    this.loop = this.loop - 1;
                    this.time = this.time + this.duration;
                }

                if (this.loop == 0)
                {
                    this.Stop();
                }
                else if (this.loop < 0)
                {
                    this.time = this.time + this.duration;
                }
            }
        }
    }
}
