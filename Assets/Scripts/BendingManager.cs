using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BendingManager : MonoBehaviour
{
    #region Constants

    private const string BENDING_FEATURE = "ENABLE_BENDING";

    private static readonly int BENDING_AMOUNT =
      Shader.PropertyToID("_BendingAmount");

    #endregion


    #region Inspector

    [SerializeField]
    [Range(0.005f, 0.1f)]
    private float bendingAmount = 0.015f;
    public Material[] bendMats;

    #endregion


    #region Fields

    private float _prevAmount;

    #endregion


    #region MonoBehaviour

    private void Awake()
    {
        if (Application.isPlaying)
            Shader.EnableKeyword(BENDING_FEATURE);
        else
            Shader.DisableKeyword(BENDING_FEATURE);

        UpdateBendingAmount();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void Update()
    {
        if (Math.Abs(_prevAmount - bendingAmount) > Mathf.Epsilon)
            UpdateBendingAmount();

        //For easy debugging this is in Update.  Once we decide on a good looking value for each level this can me moved to Start, OnEnable, or Awaken.
        for (int i = 0; i < bendMats.Length; i++)
        {
            bendMats[i].SetFloat("Vector1_6d63df4e19f04406a128658087b12187", bendingAmount);
        }
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    private void Start()
    {
        //Access the meta file attached to map to get that maps bending file.  
        //*NOTE* Commented out while this is implemented
        //bendingAmount = GameObject.Find("Map").GetComponent<MapInfo>().bendValue;
    }
    #endregion


    #region Methods

    private void UpdateBendingAmount()
    {
        _prevAmount = bendingAmount;
        Shader.SetGlobalFloat(BENDING_AMOUNT, bendingAmount);
    }

    private static void OnBeginCameraRendering(ScriptableRenderContext ctx,
                                                Camera cam)
    {
        cam.cullingMatrix = Matrix4x4.Ortho(-99, 99, -99, 99, 0.001f, 99) *
                            cam.worldToCameraMatrix;
    }

    private static void OnEndCameraRendering(ScriptableRenderContext ctx,
                                              Camera cam)
    {
        cam.ResetCullingMatrix();
    }

    #endregion
}