using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Moe
{
    public enum AssetType
    {
        UI = 0,
        MapData,
        Audio,
        Other,
    }
    public delegate void LoadFinishCallback(AssetBundleInfo abi);
    public sealed class AssetManager
    {
        #region Singleton
        private static readonly AssetManager s_Instance = new AssetManager();
        public static AssetManager instance { get { return s_Instance; } }
        static AssetManager()
        {
        }
        private AssetManager()
        {
            ConcurrentTaskMax = 5;
            ConcurrentNormalTaskMax = 3;
        }
        #endregion

        public enum UnloadType : int
        {
            Auto = 0,       //unload with ref count and lasttime
            Manual = 1,     //unload manually or while scene loading
            Everlasting = 2, //never unload
            Max = 3,
        }
        class AssetLoadTask
        {
            public UnloadType eUnloadType;
            public uint ID;
            public string FileName;
            public LoadFinishCallback onLoadFinishActions;
            public Coroutine coroutine;
        }

        public static string GetBundlePath(string fileName)
        {
            return Application.streamingAssetsPath + "/" + fileName;
        }

        private const float CacheTime = 5;

        public int ConcurrentTaskMax { get; set; }
        public int ConcurrentNormalTaskMax { get; set; }

        private Dictionary<string, AssetBundleInfo>[] m_aryDicABInfo = new Dictionary<string, AssetBundleInfo>[(int)UnloadType.Max];
        private Dictionary<string, AssetBundleInfo> dicABInfoEverlasting
        {
            get
            {
                return m_aryDicABInfo[(int)UnloadType.Everlasting];
            }
        }
        private Dictionary<string, AssetBundleInfo> dicABInfoAuto
        {
            get
            {
                return m_aryDicABInfo[(int)UnloadType.Auto];
            }
        }
        private Dictionary<string, AssetBundleInfo> dicABInfoManual
        {
            get
            {
                return m_aryDicABInfo[(int)UnloadType.Manual];
            }
        }
        private Dictionary<string, AssetLoadTask> m_dicLoadingTask = new Dictionary<string, AssetLoadTask>();
        private Dictionary<uint, AssetLoadTask> m_dicTask = new Dictionary<uint, AssetLoadTask>();
        private Queue<AssetLoadTask> m_queDelayTask = new Queue<AssetLoadTask>();
        private float m_fLastCleanCacheTime = 0;
        private int m_nCountLoadTaskInFrame = 0;
        private uint m_uiTaskId = 0;
        private uint getNewTaskId()
        {
            return ++m_uiTaskId;
        }

        public void Init()
        {
            for (int i = 0 , iMax = (int)UnloadType.Max; i < iMax; ++i)
            {
                m_aryDicABInfo[i] = new Dictionary<string, AssetBundleInfo>();
            }
        }
        public int GetCachedCount()
        {
            return dicABInfoAuto.Count;
        }
        public void Update()
        {
            if (m_queDelayTask.Count > 0)
            {
                while (m_queDelayTask.Count > 0 && m_nCountLoadTaskInFrame <= ConcurrentTaskMax)
                {
                    AssetLoadTask task = m_queDelayTask.Dequeue();
                    StartLoad(task);
                }
            }
            if (Time.realtimeSinceStartup > m_fLastCleanCacheTime + 10)
            {
                m_fLastCleanCacheTime = Time.realtimeSinceStartup;
                ReleaseAssetBundleInfo(UnloadType.Auto);
            }
        }
        public bool IsIdle()
        {
            return m_nCountLoadTaskInFrame == 0;
        }
        public uint LoadBundle(string fileName, LoadFinishCallback action, UnloadType unloadType = UnloadType.Auto, bool isSync = false)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return 0;
            }
            AssetBundleInfo asi = null;
            for (int i = 0, iMax = (int)UnloadType.Max; i < iMax; ++i)
            {
                if (m_aryDicABInfo[i].TryGetValue(fileName, out asi))
                {
                    if (action != null)
                    {
                        action(asi);
                    }
                    if ((int)unloadType > i)
                    {
                        m_aryDicABInfo[i].Remove(fileName);
                        m_aryDicABInfo[(int)unloadType][fileName] = asi;
                    }
                    return 0;
                }
            }
            AssetLoadTask oldTask = null;
            if (m_dicLoadingTask.TryGetValue(fileName, out oldTask))
            {
                if (oldTask != null)
                {
                    oldTask.onLoadFinishActions += action;
                }
                if ((int)unloadType > (int)oldTask.eUnloadType)
                {
                    oldTask.eUnloadType = unloadType;
                }
                return oldTask.ID;
            }
            if (isSync)
            {
                string strPath = GetBundlePath(fileName);
                AssetBundle ab = AssetBundle.LoadFromFile(strPath);
                if (ab)
                {
                    asi = new AssetBundleInfo();
                    asi.assetBundleFile = ab;
                    asi.assetBundleName = fileName;
                    m_aryDicABInfo[(int)unloadType][fileName] = asi;
                    if (action != null)
                    {
                        action(asi);
                    }
                }
                else
                {
                    LogManager.LogWarn("Load Bundle failed " + strPath);
                }
                return 0;
            }
            AssetLoadTask task = new AssetLoadTask();
            uint id = getNewTaskId();
            task.ID = id;
            task.FileName = fileName;
            task.onLoadFinishActions += action;
            task.eUnloadType = unloadType;
            m_dicLoadingTask[fileName] = task;
            m_dicTask[task.ID] = task;
            StartLoad(task);
            return id;

        }
        private void StartLoad(AssetLoadTask task)
        {
            if(m_nCountLoadTaskInFrame < ConcurrentTaskMax)
            {
                task.coroutine = AssetController.instance.StartCoroutine(loadBundleAsync(task));
                ++m_nCountLoadTaskInFrame;
            }
            else
            {
                m_queDelayTask.Enqueue(task);
            }
            
        }
        public void RemoveLoad(uint taskId, LoadFinishCallback callback)
        {
            AssetLoadTask oldTask = null;
            if (m_dicTask.TryGetValue(taskId, out oldTask))
            {
                if (null != callback)
                {
                    oldTask.onLoadFinishActions -= callback;
                }
            }
        }
        private IEnumerator loadBundleAsync(AssetLoadTask task)
        {
            string strPath = GetBundlePath(task.FileName);
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(strPath);
            yield return abcr;
            AssetBundleRequest assetBundleReq = null;
            if (abcr.assetBundle != null)
            {
                assetBundleReq = abcr.assetBundle.LoadAllAssetsAsync();
                yield return assetBundleReq;
            }
            m_nCountLoadTaskInFrame--;
            m_dicLoadingTask.Remove(task.FileName);
            m_dicTask.Remove(task.ID);
            if (abcr.isDone && assetBundleReq != null && assetBundleReq.isDone)
            {
                AssetBundleInfo asi = new AssetBundleInfo();
                asi.assetBundleFile = abcr.assetBundle;
                asi.assetBundleName = task.FileName;
                m_aryDicABInfo[(int)task.eUnloadType][task.FileName] = asi;
                if(task.onLoadFinishActions !=null)
                {
                    task.onLoadFinishActions(asi);
                }
            }
        }
        List<string> tempList = new List<string>();
        public void ReleaseAssetBundleInfo(UnloadType type)
        {
            var dic = m_aryDicABInfo[(int)type];
            if (dic.Count > 0)
            {
                m_fLastCleanCacheTime = Time.realtimeSinceStartup;
                float currentTime = Time.realtimeSinceStartup;
                tempList.Clear();
                foreach (var pair in dic)
                {
                    if (currentTime > pair.Value.lastTime + CacheTime)
                    {
                        if (pair.Value.IsUnused)
                        {
                            pair.Value.dispose();
                            tempList.Add(pair.Key);
                        }
                    }
                }
                foreach (string filename in tempList)
                {
                    dic.Remove(filename);
                }
            }
        }
        public void ReleaseAssetBundleInfo(UnloadType type, string mapName)
        {
            var dic = m_aryDicABInfo[(int)type];
            if(dic.TryGetValue(mapName ,out var abi))
            {
                abi.dispose();
                dic.Remove(mapName);
            }
        }
        public void IncAssetBundleRefCount(string fileName)
        {
            if (fileName == null)
            {
                return;
            }
            AssetBundleInfo asi = null;
            for (int i = 0, iMax = (int)UnloadType.Max; i < iMax; ++i)
            {
                if (m_aryDicABInfo[i].TryGetValue(fileName, out asi))
                {
                    asi.retain();
                    asi.updateLastUsedTime();
                }
            }
        }
        public void DecAssetBundleRefCount(string fileName)
        {
            if(fileName == null)
            {
                return;
            }
            AssetBundleInfo asi = null;
            for (int i = 0, iMax = (int)UnloadType.Max; i < iMax; ++i)
            {
                if (m_aryDicABInfo[i].TryGetValue(fileName, out asi))
                {
                    asi.release();
                    asi.updateLastUsedTime();
                }
            }
        }
    }
}
