using UnityEngine;

namespace Moe
{
    public class AssetController:MonoBehaviour
    {
        public static AssetController instance
        {
            get;
            protected set;
        }
        public void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  //防止销毁自己
        }
        public void Update()
        {
            AssetManager.instance.Update();
        }
        public void OnDestroy()
        {
            instance = null;
        }
    }
}
