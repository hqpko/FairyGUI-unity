using FairyGUI;
using FairyGUI.Utils;
using System.Collections;
using UnityEngine;

public class RenderImage
{
    public Transform modelRoot { get; private set; }

    private Camera _camera;
    private Image _image;
    private Transform _root;
    private Transform _background;
    private Transform _model;
    private RenderTexture _renderTexture;
    private int _width;
    private int _height;
    private bool _cacheTexture;
    private float _rotating;

    private const int RENDER_LAYER = 0;
    private const int HIDDEN_LAYER = 10;

    public RenderImage(GGraph holder)
    {
        _width = (int) holder.width;
        _height = (int) holder.height;
        _cacheTexture = true;

        _image = new Image();
        holder.SetNativeObject(_image);

        var prefab = Resources.Load("RenderTexture/RenderImageCamera");
        var go = (GameObject) Object.Instantiate(prefab);
        _camera = go.GetComponent<Camera>();
        _camera.transform.position = new Vector3(0, 1000, 0);
        _camera.cullingMask = 1 << RENDER_LAYER;
        _camera.enabled = false;
        Object.DontDestroyOnLoad(_camera.gameObject);

        _root = new GameObject("RenderImage").transform;
        _root.SetParent(_camera.transform, false);
        SetLayer(_root.gameObject, HIDDEN_LAYER);

        modelRoot = new GameObject("model_root").transform;
        modelRoot.SetParent(_root, false);

        _background = new GameObject("background").transform;
        _background.SetParent(_root, false);

        _image.onAddedToStage.Add(OnAddedToStage);
        _image.onRemovedFromStage.Add(OnRemoveFromStage);

        if (_image.stage != null)
            OnAddedToStage();
        else
            _camera.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        Object.Destroy(_camera.gameObject);
        DestroyTexture();

        _image.Dispose();
        _image = null;
    }

    /// <summary>
    /// The rendertexture is not transparent. So if you want to the UI elements can be seen in the back of the models/particles in rendertexture,
    /// you can set a maximunm two images for background.
    /// Be careful if your image is 9 grid scaling, you must make sure the place holder is inside the middle box(dont cover from border to middle).
    /// </summary>
    /// <param name="image"></param>
    public void SetBackground(GObject image)
    {
        SetBackground(image, null);
    }

    /// <summary>
    /// The rendertexture is not transparent. So if you want to the UI elements can be seen in the back of the models/particles in rendertexture,
    /// you can set a maximunm two images for background.
    /// </summary>
    /// <param name="image1"></param>
    /// <param name="image2"></param>
    public void SetBackground(GObject image1, GObject image2)
    {
        var source1 = (Image) image1.displayObject;
        var source2 = image2 != null ? (Image) image2.displayObject : null;

        var pos = _background.position;
        pos.z = _camera.farClipPlane;
        _background.position = pos;

        var uv = new Vector2[4];
        Vector2[] uv2 = null;

        var rect = _image.TransformRect(new Rect(0, 0, _width, _height), source1);
        var uvRect = GetImageUVRect(source1, rect, uv);

        if (source2 != null)
        {
            rect = _image.TransformRect(new Rect(0, 0, _width, _height), source2);
            uv2 = new Vector2[4];
            GetImageUVRect(source2, rect, uv2);
        }

        var vertices = new Vector3[4];
        for (var i = 0; i < 4; i++)
        {
            var v = uv[i];
            vertices[i] = new Vector3((v.x - uvRect.x) / uvRect.width * 2 - 1,
                (v.y - uvRect.y) / uvRect.height * 2 - 1, 0);
        }

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        if (uv2 != null)
            mesh.uv2 = uv2;
        mesh.colors32 = new Color32[] {Color.white, Color.white, Color.white, Color.white};
        mesh.triangles = new int[] {0, 1, 2, 2, 3, 0};

        var meshFilter = _background.gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = _background.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        var meshRenderer = _background.gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = _background.gameObject.AddComponent<MeshRenderer>();
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
        meshRenderer.castShadows = false;
#endif
        meshRenderer.receiveShadows = false;
        var shader = Shader.Find("Game/FullScreen");
        var mat = new Material(shader);
        mat.mainTexture = source1.texture.nativeTexture;
        if (source2 != null)
            mat.SetTexture("_Tex2", source2.texture.nativeTexture);
        meshRenderer.material = mat;
    }

    private Rect GetImageUVRect(Image image, Rect localRect, Vector2[] uv)
    {
        var imageRect = new Rect(0, 0, image.size.x, image.size.y);
        var bound = ToolSet.Intersection(ref imageRect, ref localRect);
        var uvRect = image.texture.uvRect;

        if (image.scale9Grid != null)
        {
            var gridRect = (Rect) image.scale9Grid;
            float sourceW = image.texture.width;
            float sourceH = image.texture.height;
            uvRect = Rect.MinMaxRect(Mathf.Lerp(uvRect.xMin, uvRect.xMax, gridRect.xMin / sourceW),
                Mathf.Lerp(uvRect.yMin, uvRect.yMax, (sourceH - gridRect.yMax) / sourceH),
                Mathf.Lerp(uvRect.xMin, uvRect.xMax, gridRect.xMax / sourceW),
                Mathf.Lerp(uvRect.yMin, uvRect.yMax, (sourceH - gridRect.yMin) / sourceH));

            var vw = imageRect.width - (sourceW - gridRect.width);
            var vh = imageRect.height - (sourceH - gridRect.height);
            uvRect = Rect.MinMaxRect(Mathf.Lerp(uvRect.xMin, uvRect.xMax, (bound.x - gridRect.x) / vw),
                Mathf.Lerp(uvRect.yMin, uvRect.yMax, (imageRect.height - bound.yMax - (sourceH - gridRect.yMax)) / vh),
                Mathf.Lerp(uvRect.xMin, uvRect.xMax, (bound.xMax - gridRect.x) / vw),
                Mathf.Lerp(uvRect.yMin, uvRect.yMax, (imageRect.height - bound.yMin - gridRect.y) / vh));
        }
        else
        {
            uvRect = Rect.MinMaxRect(Mathf.Lerp(uvRect.xMin, uvRect.xMax, bound.xMin / imageRect.width),
                Mathf.Lerp(uvRect.yMin, uvRect.yMax, (imageRect.height - bound.yMax) / imageRect.height),
                Mathf.Lerp(uvRect.xMin, uvRect.xMax, bound.xMax / imageRect.width),
                Mathf.Lerp(uvRect.yMin, uvRect.yMax, (imageRect.height - bound.yMin) / imageRect.height));
        }

        uv[0] = uvRect.position;
        uv[1] = new Vector2(uvRect.xMin, uvRect.yMax);
        uv[2] = new Vector2(uvRect.xMax, uvRect.yMax);
        uv[3] = new Vector2(uvRect.xMax, uvRect.yMin);

        if (image.texture.rotated)
            ToolSet.RotateUV(uv, ref image.texture.uvRect);

        return uvRect;
    }

    public void LoadModel(string model)
    {
        UnloadModel();

        var prefab = Resources.Load(model);
        var go = (GameObject) Object.Instantiate(prefab);
        _model = go.transform;
        _model.SetParent(modelRoot, false);
    }

    public void UnloadModel()
    {
        if (_model != null)
        {
            Object.Destroy(_model.gameObject);
            _model = null;
        }

        _rotating = 0;
    }

    public void StartRotate(float delta)
    {
        _rotating = delta;
    }

    public void StopRotate()
    {
        _rotating = 0;
    }

    private void CreateTexture()
    {
        if (_renderTexture != null)
            return;

        _renderTexture = new RenderTexture(_width, _height, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 1,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0,
            useMipMap = false
        };
        _image.texture = new NTexture(_renderTexture);
        _image.blendMode = BlendMode.Off;
    }

    private void DestroyTexture()
    {
        if (_renderTexture != null)
        {
            Object.Destroy(_renderTexture);
            _renderTexture = null;
            _image.texture = null;
        }
    }

    private void OnAddedToStage()
    {
        if (_renderTexture == null)
            CreateTexture();

        Timers.inst.AddUpdate(Render);
        _camera.gameObject.SetActive(true);

        Render();
    }

    private void OnRemoveFromStage()
    {
        if (!_cacheTexture)
            DestroyTexture();

        Timers.inst.Remove(Render);
        _camera.gameObject.SetActive(false);
    }

    private void Render(object param = null)
    {
        if (_rotating != 0 && modelRoot != null)
        {
            var localRotation = modelRoot.localRotation.eulerAngles;
            localRotation.y += _rotating;
            modelRoot.localRotation = Quaternion.Euler(localRotation);
        }

        SetLayer(_root.gameObject, RENDER_LAYER);

        _camera.targetTexture = _renderTexture;
        var old = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        GL.Clear(true, true, Color.clear);
        _camera.Render();
        RenderTexture.active = old;

        SetLayer(_root.gameObject, HIDDEN_LAYER);
    }

    private void SetLayer(GameObject go, int layer)
    {
        var transforms = go.GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms) t.gameObject.layer = layer;
    }
}