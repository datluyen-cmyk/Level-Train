using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MahjongJade.Shader;

[RequireComponent(typeof(MaskableGraphic))]
[ExecuteInEditMode]
public class SpineGraphicDingXiangRongJieController : MonoBehaviour
{
	[Header("组件引用")]
	[SerializeField]
	private MaskableGraphic targetGraphic;

	private Material materialInstance;

	private bool isInitialized;

	private bool isSpineShader;

	[Header("主纹理设置")]
	[SerializeField]
	[ColorUsage(true, true)]
	private Color mainColor = Color.white;

	[SerializeField]
	private float mainRotator;

	[SerializeField]
	private float mainUSpeed;

	[SerializeField]
	private float mainVSpeed;

	[Header("溶解设置")]
	[SerializeField]
	private bool enableDissolve;

	[SerializeField]
	private Texture2D dissolveTex;

	[SerializeField]
	[Range(0f, 2f)]
	private float dissolvePower;

	[SerializeField]
	[Range(0f, 1f)]
	private float dissolveSmooth = 0.1f;

	[SerializeField]
	private float dissolveTexRotator;

	[SerializeField]
	private float dissolveTexUSpeed;

	[SerializeField]
	private float dissolveTexVSpeed;

	[SerializeField]
	private Vector2 dissolveTexTiling = Vector2.one;

	[SerializeField]
	private Vector2 dissolveTexOffset = Vector2.zero;

	[Header("溶解描边设置")]
	[SerializeField]
	private bool enableDissolveEdge;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color dissolveEdgeColor = Color.white;

	[SerializeField]
	[Range(0f, 1f)]
	private float dissolveEdgeWide = 0.05f;

	[SerializeField]
	[Range(0.001f, 0.5f)]
	private float dissolveEdgeSoftness = 0.1f;

	[Header("方向溶解设置")]
	[SerializeField]
	private bool enableDirectionalDissolve;

	[SerializeField]
	private Texture2D directionalDissolveTex;

	[SerializeField]
	private bool enableDirectionalBlend = true;

	[SerializeField]
	private bool enableScaleFromCenter = true;

	[SerializeField]
	private float directionalDissolveScale = 1f;

	[SerializeField]
	private float directionalDissolveRotator;

	[SerializeField]
	private float directionalDissolveUSpeed;

	[SerializeField]
	private float directionalDissolveVSpeed;

	[SerializeField]
	[Range(1f, 7f)]
	private float dissolveTexDivide = 1f;

	[SerializeField]
	private Vector2 directionalDissolveTexTiling = Vector2.one;

	[SerializeField]
	private Vector2 directionalDissolveTexOffset = Vector2.zero;

	[Header("扰动设置")]
	[SerializeField]
	private bool enableDistortion;

	[SerializeField]
	private Texture2D distortionTex;

	[SerializeField]
	private float distortionPower;

	[SerializeField]
	private float distortionUSpeed;

	[SerializeField]
	private float distortionVSpeed;

	[SerializeField]
	private Vector2 distortionTexTiling = Vector2.one;

	[SerializeField]
	private Vector2 distortionTexOffset = Vector2.zero;

	[Header("遮罩设置")]
	[SerializeField]
	private bool enableMask;

	[SerializeField]
	private Texture2D maskTex;

	[SerializeField]
	private float maskRotator;

	[SerializeField]
	private float maskUSpeed;

	[SerializeField]
	private float maskVSpeed;

	[SerializeField]
	private Vector2 maskTexTiling = Vector2.one;

	[SerializeField]
	private Vector2 maskTexOffset = Vector2.zero;

	[Header("Gamma设置")]
	[SerializeField]
	private bool enableGamma;

	[SerializeField]
	private Texture2D gammaTex;

	[SerializeField]
	private float gammaRotator;

	[SerializeField]
	private float gammaUSpeed;

	[SerializeField]
	private float gammaVSpeed;

	[SerializeField]
	private Vector2 gammaTexTiling = Vector2.one;

	[SerializeField]
	private Vector2 gammaTexOffset = Vector2.zero;

	private void Awake()
	{
		Initialize();
	}

	private void OnEnable()
	{
		Initialize();
	}

	private void Initialize()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		if (!isInitialized)
		{
			if ((Object)(object)targetGraphic == (Object)null)
			{
				targetGraphic = ((Component)this).GetComponent<MaskableGraphic>();
			}
			if ((Object)(object)targetGraphic != (Object)null && (Object)(object)((Graphic)targetGraphic).material != (Object)null)
			{
				materialInstance = new Material(((Graphic)targetGraphic).material);
				((Graphic)targetGraphic).material = materialInstance;
				isSpineShader = ((Object)materialInstance.shader).name.Contains("Spine") || ((Object)materialInstance.shader).name.Contains("SkeletonGraphic");
				isInitialized = true;
				ApplyProperties();
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)materialInstance != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)materialInstance);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)materialInstance);
			}
		}
	}

	private void Update()
	{
		if (!isInitialized)
		{
			Initialize();
		}
		ApplyProperties();
	}

	private void ApplyProperties()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0800: Unknown result type (might be due to invalid IL or missing references)
		//IL_0816: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0967: Unknown result type (might be due to invalid IL or missing references)
		//IL_097d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)materialInstance == (Object)null)
		{
			return;
		}
		if (materialInstance.HasProperty("_Maincolor"))
		{
			materialInstance.SetColor("_Maincolor", mainColor);
		}
		if (materialInstance.HasProperty("_mainRotator"))
		{
			materialInstance.SetFloat("_mainRotator", mainRotator);
		}
		if (materialInstance.HasProperty("_Main_U_Speed"))
		{
			materialInstance.SetFloat("_Main_U_Speed", mainUSpeed);
		}
		if (materialInstance.HasProperty("_Main_V_Speed"))
		{
			materialInstance.SetFloat("_Main_V_Speed", mainVSpeed);
		}
		bool flag = enableDissolve || enableDirectionalDissolve;
		if (flag)
		{
			materialInstance.EnableKeyword("_USE_DISSOLVE");
			if (materialInstance.HasProperty("_UseDissolve"))
			{
				materialInstance.SetFloat("_UseDissolve", 1f);
			}
			if ((Object)(object)dissolveTex != (Object)null && materialInstance.HasProperty("_DissolveTex"))
			{
				materialInstance.SetTexture("_DissolveTex", (Texture)(object)dissolveTex);
			}
			if (materialInstance.HasProperty("_DissolvePower"))
			{
				materialInstance.SetFloat("_DissolvePower", dissolvePower);
			}
			if (materialInstance.HasProperty("_DissolveSmooth"))
			{
				materialInstance.SetFloat("_DissolveSmooth", dissolveSmooth);
			}
			if (materialInstance.HasProperty("_DissolveTexRotator"))
			{
				materialInstance.SetFloat("_DissolveTexRotator", dissolveTexRotator);
			}
			if (materialInstance.HasProperty("_DissolveTex_u_speed"))
			{
				materialInstance.SetFloat("_DissolveTex_u_speed", dissolveTexUSpeed);
			}
			if (materialInstance.HasProperty("_DissolveTex_v_speed"))
			{
				materialInstance.SetFloat("_DissolveTex_v_speed", dissolveTexVSpeed);
			}
			if (materialInstance.HasProperty("_DissolveTex"))
			{
				materialInstance.SetTextureScale("_DissolveTex", dissolveTexTiling);
				materialInstance.SetTextureOffset("_DissolveTex", dissolveTexOffset);
			}
		}
		else
		{
			materialInstance.DisableKeyword("_USE_DISSOLVE");
			if (materialInstance.HasProperty("_UseDissolve"))
			{
				materialInstance.SetFloat("_UseDissolve", 0f);
			}
		}
		if (enableDissolveEdge && flag)
		{
			materialInstance.EnableKeyword("_USE_DISSOLVE_EDGE");
			if (materialInstance.HasProperty("_UseDissolveEdge"))
			{
				materialInstance.SetFloat("_UseDissolveEdge", 1f);
			}
			if (materialInstance.HasProperty("_DissloveEdgeColor"))
			{
				materialInstance.SetColor("_DissloveEdgeColor", dissolveEdgeColor);
			}
			if (materialInstance.HasProperty("_DissolveEdgeWide"))
			{
				materialInstance.SetFloat("_DissolveEdgeWide", dissolveEdgeWide);
			}
			if (materialInstance.HasProperty("_DissolveEdgeSoftness"))
			{
				materialInstance.SetFloat("_DissolveEdgeSoftness", dissolveEdgeSoftness);
			}
		}
		else
		{
			materialInstance.DisableKeyword("_USE_DISSOLVE_EDGE");
			if (materialInstance.HasProperty("_UseDissolveEdge"))
			{
				materialInstance.SetFloat("_UseDissolveEdge", 0f);
			}
		}
		if (enableDirectionalDissolve)
		{
			materialInstance.EnableKeyword("_USE_DIRECTIONAL_DISSOLVE");
			if (materialInstance.HasProperty("_UseDirectionalDissolve"))
			{
				materialInstance.SetFloat("_UseDirectionalDissolve", 1f);
			}
			if ((Object)(object)directionalDissolveTex != (Object)null && materialInstance.HasProperty("_DissloveTexPlus"))
			{
				materialInstance.SetTexture("_DissloveTexPlus", (Texture)(object)directionalDissolveTex);
			}
			if (materialInstance.HasProperty("_IFDirectionalDissolve"))
			{
				materialInstance.SetFloat("_IFDirectionalDissolve", enableDirectionalBlend ? 1f : 0f);
			}
			if (materialInstance.HasProperty("_OneScale"))
			{
				materialInstance.SetFloat("_OneScale", enableScaleFromCenter ? 1f : 0f);
			}
			if (materialInstance.HasProperty("_Scale"))
			{
				materialInstance.SetFloat("_Scale", directionalDissolveScale);
			}
			if (materialInstance.HasProperty("_DissolveTexPlus_Rotator"))
			{
				materialInstance.SetFloat("_DissolveTexPlus_Rotator", directionalDissolveRotator);
			}
			if (materialInstance.HasProperty("_DissolveTexPlus_u_speed"))
			{
				materialInstance.SetFloat("_DissolveTexPlus_u_speed", directionalDissolveUSpeed);
			}
			if (materialInstance.HasProperty("_DissolveTexPlus_v_speed"))
			{
				materialInstance.SetFloat("_DissolveTexPlus_v_speed", directionalDissolveVSpeed);
			}
			if (materialInstance.HasProperty("_DissolveTexDivide"))
			{
				materialInstance.SetFloat("_DissolveTexDivide", dissolveTexDivide);
			}
			if (materialInstance.HasProperty("_DissloveTexPlus"))
			{
				materialInstance.SetTextureScale("_DissloveTexPlus", directionalDissolveTexTiling);
				materialInstance.SetTextureOffset("_DissloveTexPlus", directionalDissolveTexOffset);
			}
		}
		else
		{
			materialInstance.DisableKeyword("_USE_DIRECTIONAL_DISSOLVE");
			if (materialInstance.HasProperty("_UseDirectionalDissolve"))
			{
				materialInstance.SetFloat("_UseDirectionalDissolve", 0f);
			}
		}
		if (enableDistortion)
		{
			materialInstance.EnableKeyword("_USE_DISTORTION");
			if (materialInstance.HasProperty("_UseDistortion"))
			{
				materialInstance.SetFloat("_UseDistortion", 1f);
			}
			if ((Object)(object)distortionTex != (Object)null && materialInstance.HasProperty("_NIUQU_Tex"))
			{
				materialInstance.SetTexture("_NIUQU_Tex", (Texture)(object)distortionTex);
			}
			if (materialInstance.HasProperty("_NIUQU_Power"))
			{
				materialInstance.SetFloat("_NIUQU_Power", distortionPower);
			}
			if (materialInstance.HasProperty("_Niuqu_U_speed"))
			{
				materialInstance.SetFloat("_Niuqu_U_speed", distortionUSpeed);
			}
			if (materialInstance.HasProperty("_Niuqu_V_speed"))
			{
				materialInstance.SetFloat("_Niuqu_V_speed", distortionVSpeed);
			}
			if (materialInstance.HasProperty("_NIUQU_Tex"))
			{
				materialInstance.SetTextureScale("_NIUQU_Tex", distortionTexTiling);
				materialInstance.SetTextureOffset("_NIUQU_Tex", distortionTexOffset);
			}
		}
		else
		{
			materialInstance.DisableKeyword("_USE_DISTORTION");
			if (materialInstance.HasProperty("_UseDistortion"))
			{
				materialInstance.SetFloat("_UseDistortion", 0f);
			}
		}
		if (enableMask)
		{
			materialInstance.EnableKeyword("_USE_MASK");
			if (materialInstance.HasProperty("_UseMask"))
			{
				materialInstance.SetFloat("_UseMask", 1f);
			}
			if ((Object)(object)maskTex != (Object)null && materialInstance.HasProperty("_MASKTEX"))
			{
				materialInstance.SetTexture("_MASKTEX", (Texture)(object)maskTex);
			}
			if (materialInstance.HasProperty("_MASKRotator"))
			{
				materialInstance.SetFloat("_MASKRotator", maskRotator);
			}
			if (materialInstance.HasProperty("_MASK_u_speed"))
			{
				materialInstance.SetFloat("_MASK_u_speed", maskUSpeed);
			}
			if (materialInstance.HasProperty("_MASK_v_speed"))
			{
				materialInstance.SetFloat("_MASK_v_speed", maskVSpeed);
			}
			if (materialInstance.HasProperty("_MASKTEX"))
			{
				materialInstance.SetTextureScale("_MASKTEX", maskTexTiling);
				materialInstance.SetTextureOffset("_MASKTEX", maskTexOffset);
			}
		}
		else
		{
			materialInstance.DisableKeyword("_USE_MASK");
			if (materialInstance.HasProperty("_UseMask"))
			{
				materialInstance.SetFloat("_UseMask", 0f);
			}
		}
		if (enableGamma)
		{
			materialInstance.EnableKeyword("_USE_GAMMA");
			if (materialInstance.HasProperty("_UseGamma"))
			{
				materialInstance.SetFloat("_UseGamma", 1f);
			}
			if ((Object)(object)gammaTex != (Object)null && materialInstance.HasProperty("_Gam"))
			{
				materialInstance.SetTexture("_Gam", (Texture)(object)gammaTex);
			}
			if (materialInstance.HasProperty("_GAMRotator"))
			{
				materialInstance.SetFloat("_GAMRotator", gammaRotator);
			}
			if (materialInstance.HasProperty("_Gam_u_speed"))
			{
				materialInstance.SetFloat("_Gam_u_speed", gammaUSpeed);
			}
			if (materialInstance.HasProperty("_Gam_v_speed"))
			{
				materialInstance.SetFloat("_Gam_v_speed", gammaVSpeed);
			}
			if (materialInstance.HasProperty("_Gam"))
			{
				materialInstance.SetTextureScale("_Gam", gammaTexTiling);
				materialInstance.SetTextureOffset("_Gam", gammaTexOffset);
			}
		}
		else
		{
			materialInstance.DisableKeyword("_USE_GAMMA");
			if (materialInstance.HasProperty("_UseGamma"))
			{
				materialInstance.SetFloat("_UseGamma", 0f);
			}
		}
	}

	public void SetMainColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		mainColor = color;
	}

	public void SetMainRotator(float rotator)
	{
		mainRotator = rotator;
	}

	public void SetMainUVSpeed(float uSpeed, float vSpeed)
	{
		mainUSpeed = uSpeed;
		mainVSpeed = vSpeed;
	}

	public void SetDissolveTexture(Texture2D texture)
	{
		dissolveTex = texture;
	}

	public void SetDissolveTextureTiling(Vector2 tiling)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		dissolveTexTiling = tiling;
	}

	public void SetDissolveTextureOffset(Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		dissolveTexOffset = offset;
	}

	public void SetDissolvePower(float power)
	{
		dissolvePower = Mathf.Clamp(power, 0f, 2f);
		if (!enableDissolve)
		{
			enableDissolve = true;
		}
	}

	public void SetDissolveEnabled(bool enabled)
	{
		enableDissolve = enabled;
	}

	public void SetDissolveEdgeColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		dissolveEdgeColor = color;
	}

	public void SetDissolveEdgeWide(float wide)
	{
		dissolveEdgeWide = Mathf.Clamp01(wide);
	}

	public void SetDissolveEdgeEnabled(bool enabled)
	{
		enableDissolveEdge = enabled;
	}

	public void SetDirectionalDissolveTexture(Texture2D texture)
	{
		directionalDissolveTex = texture;
	}

	public void SetDirectionalDissolveTextureTiling(Vector2 tiling)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		directionalDissolveTexTiling = tiling;
	}

	public void SetDirectionalDissolveTextureOffset(Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		directionalDissolveTexOffset = offset;
	}

	public void SetDirectionalDissolveScale(float scale)
	{
		directionalDissolveScale = scale;
	}

	public void SetDirectionalDissolveUVSpeed(float uSpeed, float vSpeed)
	{
		directionalDissolveUSpeed = uSpeed;
		directionalDissolveVSpeed = vSpeed;
	}

	public void SetDirectionalDissolveEnabled(bool enabled)
	{
		enableDirectionalDissolve = enabled;
		if (enabled && !enableDissolve)
		{
			enableDissolve = true;
		}
	}

	public void SetDirectionalBlendEnabled(bool enabled)
	{
		enableDirectionalBlend = enabled;
	}

	public void SetScaleFromCenterEnabled(bool enabled)
	{
		enableScaleFromCenter = enabled;
	}

	public void SetDistortionTexture(Texture2D texture)
	{
		distortionTex = texture;
	}

	public void SetDistortionTextureTiling(Vector2 tiling)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		distortionTexTiling = tiling;
	}

	public void SetDistortionTextureOffset(Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		distortionTexOffset = offset;
	}

	public void SetDistortionPower(float power)
	{
		distortionPower = power;
		if (!enableDistortion)
		{
			enableDistortion = true;
		}
	}

	public void SetDistortionEnabled(bool enabled)
	{
		enableDistortion = enabled;
	}

	public void SetMaskTexture(Texture2D texture)
	{
		maskTex = texture;
	}

	public void SetMaskTextureTiling(Vector2 tiling)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		maskTexTiling = tiling;
	}

	public void SetMaskTextureOffset(Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		maskTexOffset = offset;
	}

	public void SetGammaTexture(Texture2D texture)
	{
		gammaTex = texture;
	}

	public void SetGammaTextureTiling(Vector2 tiling)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		gammaTexTiling = tiling;
	}

	public void SetGammaTextureOffset(Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		gammaTexOffset = offset;
	}

	public void DissolveOut(float duration)
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(DissolveOutCoroutine(duration));
		}
	}

	public void DissolveIn(float duration)
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(DissolveInCoroutine(duration));
		}
	}

	private IEnumerator DissolveOutCoroutine(float duration)
	{
		enableDissolve = true;
		float elapsed = 0f;
		float startValue = dissolvePower;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			dissolvePower = Mathf.Lerp(startValue, 2f, elapsed / duration);
			yield return null;
		}
		dissolvePower = 2f;
	}

	private IEnumerator DissolveInCoroutine(float duration)
	{
		enableDissolve = true;
		float elapsed = 0f;
		float startValue = dissolvePower;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			dissolvePower = Mathf.Lerp(startValue, 0f, elapsed / duration);
			yield return null;
		}
		dissolvePower = 0f;
	}
}
