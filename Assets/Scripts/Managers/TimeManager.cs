using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class TimeManager : MonoBehaviour
    {
        private List<TimedEvent> events = new List<TimedEvent>();
        public delegate void Callback();

        void Update()
        {
            if (events.Count == 0)
            {
                return;
            }
            for (int i = 0; i < events.Count; i++)
            {
                TimedEvent timedEvent = events[i];
                if (timedEvent.TimeToExecute <= Time.time)
                {
                    timedEvent.Method();
                    events.Remove(timedEvent);
                }
            }
        }
        public void Add(Callback method, float inSeconds)
        {
            events.Add(new TimedEvent
            {
                Method = method,
                TimeToExecute = Time.time + inSeconds
            });
        }

        private class TimedEvent
        {
            public float TimeToExecute;
            public Callback Method;
        }
    }
}