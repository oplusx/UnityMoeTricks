using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Moe
{
    public class AssetBundleInfo
    {
        public AssetBundle assetBundleFile;
        public string assetBundleName;
        public float lastTime;
        public bool IsUnused
        {
            get
            {
                return assetBundleFile != null && mRefCount <= 0 && updateOwnerReference() == 0;
            }
        }
        private int mRefCount;
        public int RefCount
        {
            get
            {
                return mRefCount;
            }
        }
        public List<System.WeakReference> ReferenceOwnerList
        {
            get
            {
                return mReferenceOwnerList;
            }
        }
        public UnityEngine.Object mainAsset
        {
            get {
                if(assetBundleFile != null)
                {
                    UnityEngine.Object[] objs = assetBundleFile.LoadAllAssets();
                    if(objs!= null && objs.Length > 0)
                    {
                        return objs[0];
                    }
                }
                return null;
            }
        }
        private List<WeakReference> mReferenceOwnerList = new List<WeakReference>();
        private int updateOwnerReference()
        {
            for (int i = 0; i < mReferenceOwnerList.Count; i++)
            {
                UnityEngine.Object o = (UnityEngine.Object)mReferenceOwnerList[i].Target;
                if (!o)
                {
                    mReferenceOwnerList.RemoveAt(i);
                    i--;
                }
            }
            return mReferenceOwnerList.Count;
        }
        private void unloadAssetBundle()
        {
            LogManager.LogInfo(string.Format("unload assetbundle:{0}", assetBundleName));
            if (assetBundleFile != null)
            {
                assetBundleFile.Unload(true);
            }
            assetBundleFile = null;
        }
        public void retain()
        {
            mRefCount++;
        }
        public void release()
        {
            mRefCount = Mathf.Max(0, mRefCount - 1);
        }
        public void dispose()
        {
            unloadAssetBundle();
            lastTime = 0.0f;
        }
        public void updateLastUsedTime()
        {
            lastTime = Time.time;
        }
        public void retainOwner(UnityEngine.Object owner)
        {
            if (owner == null)
            {
                return;
            }
            foreach (var referenceowner in mReferenceOwnerList)
            {
                if (owner.Equals(referenceowner))
                {
                    return;
                }
            }
            System.WeakReference wr = new System.WeakReference(owner);
            mReferenceOwnerList.Add(wr);
        }
        public T getAsset<T>(UnityEngine.Object owner, string assetname) where T : UnityEngine.Object
        {
            if (owner != null)
            {
                var asset = loadAsset<T>(assetname);
                if (asset != null)
                {
                    retainOwner(owner);
                    updateLastUsedTime();
                    return asset;
                }
            }
            return null;
        }
        public T loadAsset<T>(string assetname) where T : UnityEngine.Object
        {
            if (assetBundleFile == null)
            {
                return null;
            }
            return assetBundleFile.LoadAsset<T>(assetname);
        }


        static StringBuilder s_sb = new StringBuilder();
        public string getDebugInfo()
        {
            s_sb.Clear();
            s_sb.AppendFormat("Name: {0} RefCount:{1}\n", assetBundleName, mRefCount);
            if (mReferenceOwnerList.Count > 0)
            {
                for (int i = 0, length = mReferenceOwnerList.Count; i < length; i++)
                {
                    if (mReferenceOwnerList[i].Target != null)
                    {
                        s_sb.AppendFormat("owner[{0}] : {1}\n", i, mReferenceOwnerList[i].Target.ToString());
                    }
                }
            }
            s_sb.AppendFormat("Last Time: {0}\n", lastTime);
            return s_sb.ToString();
        }
    }
}
