using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;

#endif

public delegate void LoadCompleteCallback(NTexture texture);

public delegate void LoadErrorCallback(string error);

/// <summary>
/// Use to load icons from asset bundle, and pool them
/// </summary>
public class IconManager : MonoBehaviour
{
    private static IconManager _instance;

    public static IconManager inst
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("IconManager");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<IconManager>();
            }

            return _instance;
        }
    }

    public const int POOL_CHECK_TIME = 30;
    public const int MAX_POOL_SIZE = 10;

    private List<LoadItem> _items;
    private bool _started;
    private Hashtable _pool;
    private string _basePath;

    private void Awake()
    {
        _items = new List<LoadItem>();
        _pool = new Hashtable();
        _basePath = Application.streamingAssetsPath.Replace("\\", "/") + "/fairygui-examples/";
        if (Application.platform != RuntimePlatform.Android)
            _basePath = "file:///" + _basePath;

        StartCoroutine(FreeIdleIcons());
    }

    public void LoadIcon(string url,
        LoadCompleteCallback onSuccess,
        LoadErrorCallback onFail)
    {
        var item = new LoadItem();
        item.url = url;
        item.onSuccess = onSuccess;
        item.onFail = onFail;
        _items.Add(item);
        if (!_started)
            StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        _started = true;

        LoadItem item = null;
        while (true)
        {
            if (_items.Count > 0)
            {
                item = _items[0];
                _items.RemoveAt(0);
            }
            else
            {
                break;
            }

            if (_pool.ContainsKey(item.url))
            {
                //Debug.Log("hit " + item.url);

                var texture = (NTexture) _pool[item.url];
                texture.refCount++;

                if (item.onSuccess != null)
                    item.onSuccess(texture);

                continue;
            }

            var url = _basePath + item.url + ".ab";
#if UNITY_2017_2_OR_NEWER
#if UNITY_2018_1_OR_NEWER
            var www = UnityWebRequestAssetBundle.GetAssetBundle(url);
#else
            UnityWebRequest www = UnityWebRequest.GetAssetBundle(url);
#endif
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                var bundle = DownloadHandlerAssetBundle.GetContent(www);
#else
            WWW www = new WWW(url);
            yield return www;
            
            if (string.IsNullOrEmpty(www.error))
            {
                AssetBundle bundle = www.assetBundle;
#endif

                if (bundle == null)
                {
                    Debug.LogWarning("Run Window->Build FairyGUI example Bundles first.");
                    if (item.onFail != null)
                        item.onFail(www.error);
                    continue;
                }
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
                var texture = new NTexture(bundle.LoadAllAssets<Texture2D>()[0]);
#else
                NTexture texture = new NTexture((Texture2D)bundle.mainAsset);
#endif
                texture.refCount++;
                bundle.Unload(false);

                _pool[item.url] = texture;

                if (item.onSuccess != null)
                    item.onSuccess(texture);
            }
            else
            {
                if (item.onFail != null)
                    item.onFail(www.error);
            }
        }

        _started = false;
    }

    private IEnumerator FreeIdleIcons()
    {
        yield return new WaitForSeconds(POOL_CHECK_TIME); //check the pool every 30 seconds

        var cnt = _pool.Count;
        if (cnt > MAX_POOL_SIZE)
        {
            ArrayList toRemove = null;
            foreach (DictionaryEntry de in _pool)
            {
                var key = (string) de.Key;
                var texture = (NTexture) de.Value;
                if (texture.refCount == 0)
                {
                    if (toRemove == null)
                        toRemove = new ArrayList();
                    toRemove.Add(key);
                    texture.Dispose();

                    //Debug.Log("free icon " + de.Key);

                    cnt--;
                    if (cnt <= 8)
                        break;
                }
            }

            if (toRemove != null)
                foreach (string key in toRemove)
                    _pool.Remove(key);
        }
    }
}

internal class LoadItem
{
    public string url;
    public LoadCompleteCallback onSuccess;
    public LoadErrorCallback onFail;
}