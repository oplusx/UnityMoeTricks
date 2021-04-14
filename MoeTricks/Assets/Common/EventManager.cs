using System.Collections.Generic;
namespace Moe
{
    public class EventData
    {
	    public EventData(int eventType)
	    {
		    gameEventType = eventType;
	    }
        public EventData(int eventType, UnityEngine.Object param)
        {
            gameEventType = eventType;
            paramUnityObj = param;
        }
        public EventData(int eventType, object param)
        {
            gameEventType = eventType;
            paramCSharpObj = param;
        }
        public int gameEventType;
        public UnityEngine.Object paramUnityObj;
        public object paramCSharpObj;
    }
    public class EventHandler
    {
        private List<EventData> m_data = new List<EventData>();
        private List<EventData> m_bck = new List<EventData>();
        public delegate void OnEvent(EventData eventData);
        public OnEvent onEvent;
        public void PostEvent(int eventType)
        {
            m_data.Add(new EventData(eventType));
        }
        public void PostEvent(int eventType, UnityEngine.Object param)
        {
            m_data.Add(new EventData(eventType, param));
        }
        public void PostEvent(int eventType, object csharpParam)
        {
            m_data.Add(new EventData(eventType, csharpParam));
        }
        public void SendEvent(int eventType)
        {
            if (onEvent != null)
            {
                EventData data = new EventData(eventType);
                onEvent(data);
            }
        }
        static List<EventData> tmp = null;
        public void DoEventUpdate()
        {
            tmp = m_data;
            m_data = m_bck;
            m_bck = tmp;
            if (onEvent != null)
            {
                for (int n = 0; n < m_bck.Count; ++n)
                {
                    onEvent(m_bck[n]);
                }
            }
            m_bck.Clear();
        }
    }
    public class EventManager : EventHandler
    {
	    private static readonly EventManager m_instace = new EventManager(); 
        /// <summary>
        /// 此类主要处理整个游戏产生的GameEventType枚举的事件
        /// </summary>
	    public static EventManager Instance
	    {
		    get
		    {
			    return m_instace;
		    }
	    }
    }

}
