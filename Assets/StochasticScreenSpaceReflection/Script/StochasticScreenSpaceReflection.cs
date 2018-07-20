using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//////Enum Property//////

[System.Serializable]
public enum RenderResolution
{
    Full = 1,
    Half = 2,
};

[System.Serializable]
public enum DepthResolution
{
    Full = 1,
    Half = 2,
    Quad = 4,
    Oct = 8,
};
//////Enum Property//////

[RequireComponent(typeof(Camera))]
public class StochasticScreenSpaceReflection : MonoBehaviour
{

    //////////////////////////////////////////////////////////////////////////////Property Star///////////////////////////////////////////////////////////////////////////////////////////

    //////BackDepthProperty Star//////
    private Shader BackDepthShader;
    private Camera BackDepthRenderCamera;
    private RenderTexture BackDepthRT;
    //////BackDepthProperty End//////

    /* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* */

    //////SSR Property Star//////
    private Vector2 jitterSample = new Vector2(1, 1);
    private Vector2 CameraSize;
    private Matrix4x4 projectionMatrix;
    private Matrix4x4 viewProjectionMatrix;
    private Matrix4x4 _lastFrame_viewProjectionMatrix;
    private Matrix4x4 inverseViewProjectionMatrix;
    private Matrix4x4 worldToCameraMatrix;
    private Matrix4x4 cameraToWorldMatrix;
    private Camera RenderCamera;
    private CommandBuffer ScreenSpaceReflectionBuffer;
    private RenderTexture temporalRT;
    //////SSR Property End//////

    /* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* */

    //////ShaderID Star//////
    private static int _Noise = Shader.PropertyToID("_Noise");
    private static int _NoiseSize = Shader.PropertyToID("_NoiseSize");
    private static int _BRDFBias = Shader.PropertyToID("_BRDFBias");
    private static int _NumSteps = Shader.PropertyToID("_NumSteps");
    private static int _ScreenFade = Shader.PropertyToID("_ScreenFade");
    private static int _Thickness = Shader.PropertyToID("_Thickness");
    private static int _TScale = Shader.PropertyToID("_TScale");
    private static int _TResponse = Shader.PropertyToID("_TResponse");

    private static int _RayCastSize = Shader.PropertyToID("_RayCastSize");
    private static int _ResolveSize = Shader.PropertyToID("_ResolveSize");
    private static int _ResolverNum = Shader.PropertyToID("_ResolverNum");

    private static int _BackDepthTexture = Shader.PropertyToID("_backDepthTexture");
    private static int _SceneColor_RT = Shader.PropertyToID("_SceneColor_RT");
    private static int _CombienReflection_RT = Shader.PropertyToID("_CombienReflection_RT");
    private static int _Resolve_RT = Shader.PropertyToID("_Resolve_RT");
    private static int _TemporalBefore = Shader.PropertyToID("_TemporalBefore");
    private static int _TemporalAfter = Shader.PropertyToID("_TemporalAfter");
    private static int _RayCastRT = Shader.PropertyToID("_RayCastRT");
    private static int _RayMask_RT = Shader.PropertyToID("_RayMask_RT");
    private RenderTexture[] _RayCast_RayMask_RT = new RenderTexture[2];
    private RenderTargetIdentifier[] _RayCast_RayMask_ID = new RenderTargetIdentifier[2];

    private static int _JitterSizeAndOffset = Shader.PropertyToID("_JitterSizeAndOffset");
    private static int _ProjectionMatrix = Shader.PropertyToID("_ProjectionMatrix");
    private static int _ViewProjectionMatrix = Shader.PropertyToID("_ViewProjectionMatrix");
    private static int _LastFrameViewProjectionMatrix = Shader.PropertyToID("_LastFrameViewProjectionMatrix");
    private static int _InverseProjectionMatrix = Shader.PropertyToID("_InverseProjectionMatrix");
    private static int _InverseViewProjectionMatrix = Shader.PropertyToID("_InverseViewProjectionMatrix");
    private static int _WorldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");
    private static int _CameraToWorldMatrix = Shader.PropertyToID("_CameraToWorldMatrix");
    private static int _RayStepSize = Shader.PropertyToID("_RayStepSize");
    private static int _MaxRayTraceDistance = Shader.PropertyToID("_MaxRayTraceDistance");
    private static int _AllowBackwardsRays = Shader.PropertyToID("_AllowBackwardsRays");
    private static int _TraceBehindObjects = Shader.PropertyToID("_TraceBehindObjects");
    private static int _ProjectToPixelMatrix = Shader.PropertyToID("_ProjectToPixelMatrix");
    private static int _ProjInfo = Shader.PropertyToID("_ProjInfo");
    private static int _CameraClipInfo = Shader.PropertyToID("_CameraClipInfo");

    //////ShaderID End//////

    /* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* *//* */
                                                                                                                                                                                                                                               //////Control Property Star//////
    [Header("TraceProperty")]

    //[SerializeField]
    bool AllowBackwardsRays = true;

    //[SerializeField]
    bool TraceBehindObjects = true;

    [SerializeField]
    bool DeNoiseing = true;


    [SerializeField]
    RenderResolution RayCastingResolution = RenderResolution.Full;


    [SerializeField]
    RenderResolution ResolveResolution = RenderResolution.Full;


    [Range(1, 512)]
    [SerializeField]
    int MarchingDistance = 128;


    [Range(1, 512)]
    [SerializeField]
    int RayMarchingSteps = 256;


    [Range(5, 12)]
    [SerializeField]
    int StepSize = 12;


    //[Range(0, 1)]
    //[SerializeField]
    float BRDFBias = 0.7f;


    [Range(0.1f, 2)]
    [SerializeField]
    float Thickness = 0.1f;


    [Range(0, 1)]
    [SerializeField]
    float FadeSize = 0.1f;


    private Material StochasticScreenSpaceReflectionMaterial;

    [Header("TemporalProperty")]

    [Range(1, 5)]
    [SerializeField]
    float TemporalScale = 5;

    [Range(0, 1)]
    [SerializeField]
    float TemporalResponse = 0.97f;

    Texture noise;

    [Header("DeBugProperty")]

    [SerializeField]
    bool RunTimeDebugMod = false;

    [SerializeField]
    bool DisableFPSLimit = false;

    //////Control Property End//////

    //////////////////////////////////////////////////////////////////////////////Property End///////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        RenderCamera = gameObject.GetComponent<Camera>();
        StochasticScreenSpaceReflectionMaterial = new Material(Shader.Find("Hidden/StochasticScreenSpaceReflection"));
        BackDepthShader = Resources.Load("RenderBackDepth") as Shader;
        noise = Resources.Load("BlueNoise") as Texture2D;

        //////Install RenderBuffer//////
        InstallRenderCommandBuffer();

#if UNITY_EDITOR
        //////DeBug FPS//////
        SetFPSFrame(DisableFPSLimit, -1);
#endif
        //////Update don't need Tick Refresh Variable//////

        UpdateVariable();
    }

    void OnPreRender()
    {
        jitterSample = GenerateRandomOffset();
        UpdateVariable();
        //////RayTrace Buffer//////
        SSRBuffer();
    }

    private void OnEnable()
    {
        RenderCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, ScreenSpaceReflectionBuffer);
    }

    void OnDisable()
    {
        RenderCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, ScreenSpaceReflectionBuffer);
    }

    private void OnDestroy()
    {
        RelaseVariable();
        if (BackDepthRT != null) BackDepthRT.Release();
    }

    private float GetHaltonValue(int index, int radix)
    {
        float result = 0f;
        float fraction = 1f / radix;

        while (index > 0)
        {
            result += (index % radix) * fraction;
            index /= radix;
            fraction /= radix;
        }
        return result;
    }

    private int m_SampleIndex = 0;
    private const int k_SampleCount = 64;
    private Vector2 GenerateRandomOffset()
    {
        var offset = new Vector2(GetHaltonValue(m_SampleIndex & 1023, 2), GetHaltonValue(m_SampleIndex & 1023, 3));
        if (m_SampleIndex++ >= k_SampleCount)
            m_SampleIndex = 0;
        return offset;
    }

    private void UpdateUniformVariable()
    {
        RenderTexture currentRT;
        if ((currentRT = _RayCast_RayMask_RT[0]) != null)
        {
            RenderTexture.ReleaseTemporary(currentRT);
        }

        if ((currentRT = _RayCast_RayMask_RT[1]) != null)
        {
            RenderTexture.ReleaseTemporary(currentRT);
        }
        if (temporalRT != null)
        {
            temporalRT.Release();
        }
        temporalRT = RenderTexture.GetTemporary(RenderCamera.pixelWidth, RenderCamera.pixelHeight, 16, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);
        Debug.Log("!");
        _RayCast_RayMask_RT[0] = RenderTexture.GetTemporary(RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, RenderTextureFormat.ARGBHalf);
        _RayCast_RayMask_RT[1] = RenderTexture.GetTemporary(RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, RenderTextureFormat.RHalf);
        _RayCast_RayMask_ID[0] = _RayCast_RayMask_RT[0].colorBuffer;
        _RayCast_RayMask_ID[1] = _RayCast_RayMask_RT[1].colorBuffer;

        StochasticScreenSpaceReflectionMaterial.SetVector(_RayCastSize, CameraSize / (int)RayCastingResolution);
        StochasticScreenSpaceReflectionMaterial.SetVector(_ResolveSize, CameraSize / (int)ResolveResolution);
        StochasticScreenSpaceReflectionMaterial.SetTexture(_Noise, noise);
        StochasticScreenSpaceReflectionMaterial.SetVector(_NoiseSize, new Vector2(noise.width, noise.height));
        StochasticScreenSpaceReflectionMaterial.SetFloat(_BRDFBias, BRDFBias);
        StochasticScreenSpaceReflectionMaterial.SetInt(_NumSteps, RayMarchingSteps);
        StochasticScreenSpaceReflectionMaterial.SetFloat(_ScreenFade, FadeSize);
        StochasticScreenSpaceReflectionMaterial.SetFloat(_Thickness, Thickness);
        StochasticScreenSpaceReflectionMaterial.SetFloat(_TScale, TemporalScale);
        StochasticScreenSpaceReflectionMaterial.SetFloat(_RayStepSize, StepSize);
        StochasticScreenSpaceReflectionMaterial.SetFloat(_MaxRayTraceDistance, MarchingDistance);
        StochasticScreenSpaceReflectionMaterial.SetInt(_AllowBackwardsRays, AllowBackwardsRays ? 1 : 0);
        StochasticScreenSpaceReflectionMaterial.SetInt(_TraceBehindObjects, TraceBehindObjects ? 1 : 0);
        if (DeNoiseing)
        {
            StochasticScreenSpaceReflectionMaterial.SetInt(_ResolverNum, 4);
            StochasticScreenSpaceReflectionMaterial.SetFloat(_TResponse, TemporalResponse);
        }
        else
        {
            StochasticScreenSpaceReflectionMaterial.SetInt(_ResolverNum, 1);
            StochasticScreenSpaceReflectionMaterial.SetFloat(_TResponse, 0);
        }
    }

    private void UpdateVariable()
    {
        Vector2 currentCameraSize = new Vector2(RenderCamera.pixelWidth, RenderCamera.pixelHeight);
        if (CameraSize != currentCameraSize)
        {
            CameraSize = currentCameraSize;
            UpdateUniformVariable();
        }
#if UNITY_EDITOR
        if (RunTimeDebugMod)
        {
            UpdateUniformVariable();
        }
#endif
        StochasticScreenSpaceReflectionMaterial.SetVector(_JitterSizeAndOffset, new Vector4((float)CameraSize.x / (float)noise.width, (float)CameraSize.y / (float)noise.height, jitterSample.x, jitterSample.y));
        worldToCameraMatrix = RenderCamera.worldToCameraMatrix;
        cameraToWorldMatrix = worldToCameraMatrix.inverse;
        projectionMatrix = GL.GetGPUProjectionMatrix(RenderCamera.projectionMatrix, false);
        viewProjectionMatrix = projectionMatrix * worldToCameraMatrix;
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_ProjectionMatrix, projectionMatrix);
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_ViewProjectionMatrix, viewProjectionMatrix);
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_InverseProjectionMatrix, projectionMatrix.inverse);
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_InverseViewProjectionMatrix, viewProjectionMatrix.inverse);
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_WorldToCameraMatrix, worldToCameraMatrix);
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_CameraToWorldMatrix, cameraToWorldMatrix);
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_LastFrameViewProjectionMatrix, _lastFrame_viewProjectionMatrix);

        //////////////////////////////////////////////////////////////////////////////
        float sWidth = CameraSize.x;
        float sHeight = CameraSize.y;
        float sx = sWidth / 2;
        float sy = sHeight / 2;
        Matrix4x4 warpToScreenSpaceMatrix = Matrix4x4.identity;
        warpToScreenSpaceMatrix.m00 = sx;
        warpToScreenSpaceMatrix.m03 = sx;
        warpToScreenSpaceMatrix.m11 = sy;
        warpToScreenSpaceMatrix.m13 = sy;

        Matrix4x4 projectToPixelMatrix = warpToScreenSpaceMatrix * projectionMatrix;
        StochasticScreenSpaceReflectionMaterial.SetMatrix(_ProjectToPixelMatrix, projectToPixelMatrix);
        Vector4 projInfo = new Vector4
                ((-2.0f / (sWidth * projectionMatrix[0])),
                (-2.0f / (sHeight * projectionMatrix[5])),
                ((1.0f - projectionMatrix[2]) / projectionMatrix[0]),
                ((1.0f + projectionMatrix[6]) / projectionMatrix[5]));
        StochasticScreenSpaceReflectionMaterial.SetVector(_ProjInfo, projInfo);
        Vector3 cameraClipInfo = (float.IsPositiveInfinity(RenderCamera.farClipPlane)) ?
                new Vector3(RenderCamera.nearClipPlane, -1, 1) :
                new Vector3(RenderCamera.nearClipPlane * RenderCamera.farClipPlane, RenderCamera.nearClipPlane - RenderCamera.farClipPlane, RenderCamera.farClipPlane);
        StochasticScreenSpaceReflectionMaterial.SetVector(_CameraClipInfo, cameraClipInfo);
        //////////////////////////////////////////////////////////////////////////////

    }

    private void InstallRenderCommandBuffer()
    {
        //////Install RayCastingBuffer//////
        ScreenSpaceReflectionBuffer = new CommandBuffer();
        ScreenSpaceReflectionBuffer.name = "StochasticScreenSpaceReflection";

        //////Install MultiRT on RayCast Buffer//////
        _RayCast_RayMask_RT[0] = RenderTexture.GetTemporary(RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, RenderTextureFormat.ARGBHalf);
        _RayCast_RayMask_RT[1] = RenderTexture.GetTemporary(RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, RenderTextureFormat.RHalf);
        _RayCast_RayMask_ID[0] = _RayCast_RayMask_RT[0].colorBuffer;
        _RayCast_RayMask_ID[1] = _RayCast_RayMask_RT[1].colorBuffer;
    }

    private void SSRBuffer()
    {
        ScreenSpaceReflectionBuffer.Clear();

        //////Set SceneColor//////
        ScreenSpaceReflectionBuffer.GetTemporaryRT(_SceneColor_RT, RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        ScreenSpaceReflectionBuffer.Blit(BuiltinRenderTextureType.CameraTarget, _SceneColor_RT);
        //ScreenSpaceReflectionBuffer.Blit(_SceneColor_RT, BuiltinRenderTextureType.CameraTarget, StochasticScreenSpaceReflectionMaterial, 0);
        //ScreenSpaceReflectionBuffer.Blit(BuiltinRenderTextureType.CameraTarget, _SceneColor_RT);
        ScreenSpaceReflectionBuffer.SetGlobalTexture(_RayCastRT, _RayCast_RayMask_RT[0]);
        ScreenSpaceReflectionBuffer.SetGlobalTexture(_RayMask_RT, _RayCast_RayMask_RT[1]);
        ScreenSpaceReflectionBuffer.BlitMRT(_RayCast_RayMask_ID, BuiltinRenderTextureType.CameraTarget, StochasticScreenSpaceReflectionMaterial, 0);

        //////Resolve rayCasting at 4 time//////
        ScreenSpaceReflectionBuffer.GetTemporaryRT(_Resolve_RT, RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        ScreenSpaceReflectionBuffer.Blit(null, _Resolve_RT, StochasticScreenSpaceReflectionMaterial, 1);

        //////Use temporalFiltter//////
        ScreenSpaceReflectionBuffer.SetGlobalTexture(_TemporalBefore, temporalRT);
        ScreenSpaceReflectionBuffer.GetTemporaryRT(_TemporalAfter, RenderCamera.pixelWidth, RenderCamera.pixelHeight, 16, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        ScreenSpaceReflectionBuffer.Blit(null, _TemporalAfter, StochasticScreenSpaceReflectionMaterial, 2);
 
        ScreenSpaceReflectionBuffer.Blit(_TemporalAfter, temporalRT);

        //////Combien Reflection//////
        ScreenSpaceReflectionBuffer.GetTemporaryRT(_CombienReflection_RT, RenderCamera.pixelWidth, RenderCamera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        ScreenSpaceReflectionBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, StochasticScreenSpaceReflectionMaterial, 3);

        //////Set Last Frame ViewProjection//////
        _lastFrame_viewProjectionMatrix = viewProjectionMatrix;
    }

    private void RelaseVariable()
    {
        if (_RayCast_RayMask_RT[0] != null)
        {
            RenderTexture.ReleaseTemporary(_RayCast_RayMask_RT[0]);
            _RayCast_RayMask_RT[0] = null;
        }
        if (_RayCast_RayMask_RT[1] != null)
        {
            RenderTexture.ReleaseTemporary(_RayCast_RayMask_RT[1]);
            _RayCast_RayMask_RT[1] = null;
        }
        if (temporalRT != null)
        {
            temporalRT.Release();
            temporalRT = null;
        }

        ScreenSpaceReflectionBuffer.Dispose();
    }

    private void SetFPSFrame(bool UseHighFPS, int TargetFPS)
    {
        if (UseHighFPS == true)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFPS;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
        }
    }

    private void RenderBackDepth()
    {
        if (BackDepthRenderCamera == null)
        {
            var BackCamera = new GameObject("BackCamera", typeof(Camera));
            BackCamera.transform.SetParent(RenderCamera.transform);
            BackCamera.hideFlags = HideFlags.HideAndDontSave;
            BackDepthRenderCamera = BackCamera.GetComponent<Camera>();
            BackDepthRenderCamera.CopyFrom(RenderCamera);
            BackDepthRenderCamera.enabled = false;
            BackDepthRenderCamera.clearFlags = CameraClearFlags.SolidColor;
            BackDepthRenderCamera.backgroundColor = Color.black;
            BackDepthRenderCamera.renderingPath = RenderingPath.Forward;
            BackDepthRenderCamera.SetReplacementShader(BackDepthShader, "RenderType");

            BackDepthRT = new RenderTexture(RenderCamera.pixelWidth, RenderCamera.pixelHeight, 8, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            BackDepthRT.filterMode = FilterMode.Point;
            BackDepthRenderCamera.targetTexture = BackDepthRT;
            Shader.SetGlobalTexture(_BackDepthTexture, BackDepthRT);
        }
        BackDepthRenderCamera.Render();
    }

}
