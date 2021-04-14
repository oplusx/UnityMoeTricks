using UnityEngine;

namespace Moe
{
    public static class MoeDefine
    {
        public static string GetNativeFilePath()
        {
            if (Application.isEditor)
            {
                return Application.dataPath + "/Config";
            }
            return Application.streamingAssetsPath + "/config";
        }
        public static string GetSaveDataPath()
        {
            if(Application.isEditor)
            {
                return Application.dataPath + "/../UserData";
            }
            return Application.persistentDataPath + "/UserData";
        }
    }
}