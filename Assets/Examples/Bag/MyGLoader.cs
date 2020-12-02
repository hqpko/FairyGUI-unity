using UnityEngine;
using FairyGUI;
using System.IO;

/// <summary>
/// Extend the ability of GLoader
/// </summary>
public class MyGLoader : GLoader
{
    protected override void LoadExternal()
    {
        IconManager.inst.LoadIcon(url, OnLoadSuccess, OnLoadFail);
    }

    protected override void FreeExternal(NTexture texture)
    {
        texture.refCount--;
    }

    private void OnLoadSuccess(NTexture texture)
    {
        if (string.IsNullOrEmpty(url))
            return;

        onExternalLoadSuccess(texture);
    }

    private void OnLoadFail(string error)
    {
        Debug.Log("load " + url + " failed: " + error);
        onExternalLoadFailed();
    }
}