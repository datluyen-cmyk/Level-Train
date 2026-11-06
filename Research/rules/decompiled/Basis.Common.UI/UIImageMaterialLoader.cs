using Script.Tools;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Aot;

namespace Basis.Common.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class UIImageMaterialLoader : MonoBehaviour
{
	[Header("资源标识")]
	[Tooltip("材质文件名(不含路径与扩展名)。运行时优先按当前换肤配置解析，未命中时按资源名加载。")]
	[SerializeField]
	private string materialName;

	[Header("加载选项")]
	[Tooltip("OnEnable 时自动加载;关闭则需手动调用 LoadMaterial()。")]
	[SerializeField]
	private bool loadOnEnable = true;

	[HideInInspector]
	[SerializeField]
	private string defaultPath;

	private const string LogTag = "UIImageMaterialLoader";

	private Image cachedImage;

	public string MaterialName => materialName;

	private Image Image
	{
		get
		{
			if ((Object)(object)cachedImage == (Object)null)
			{
				cachedImage = ((Component)this).GetComponent<Image>();
			}
			return cachedImage;
		}
	}

	private void OnEnable()
	{
		if (loadOnEnable)
		{
			LoadMaterial();
		}
	}

	private void OnDisable()
	{
		UnloadMaterial();
	}

	private void OnDestroy()
	{
		ClearImageMaterial(Image, markLoaderDirty: false);
		cachedImage = null;
	}

	public void SetMaterial(string newMaterialName)
	{
		materialName = newMaterialName;
		UnloadMaterial();
		if (((Behaviour)this).isActiveAndEnabled)
		{
			LoadMaterial();
		}
	}

	public void LoadMaterial()
	{
		Image image = Image;
		if (!((Object)(object)image == (Object)null))
		{
			Material val = ResolveRuntimeMaterial();
			if (!((Object)(object)val == (Object)null))
			{
				((Graphic)image).material = val;
			}
		}
	}

	public void UnloadMaterial()
	{
		ClearImageMaterial(Image, markLoaderDirty: false);
	}

	public bool ClearMaterialReferenceForBuild()
	{
		return ClearImageMaterial(Image, markLoaderDirty: true);
	}

	public bool EnsureMaterialClearedForStageClose()
	{
		return ClearMaterialReferenceForBuild();
	}

	private bool ClearImageMaterial(Image image, bool markLoaderDirty)
	{
		if ((Object)(object)image == (Object)null || !HasCustomMaterial(image))
		{
			return false;
		}
		((Graphic)image).material = null;
		return true;
	}

	private static bool HasCustomMaterial(Image image)
	{
		if ((Object)(object)image != (Object)null && (Object)(object)((Graphic)image).material != (Object)null)
		{
			return (Object)(object)((Graphic)image).material != (Object)(object)((Graphic)image).defaultMaterial;
		}
		return false;
	}

	private Material ResolveRuntimeMaterial()
	{
		Material val = LoadDefaultRuntimeMaterial();
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return val;
	}

	private Material LoadDefaultRuntimeMaterial()
	{
		if (!string.IsNullOrEmpty(materialName))
		{
			string text = Define.ResGroup + materialName;
			if (GameApp.Res.HasAsset(text))
			{
				return GameApp.Res.LoadAsset<Material>(text, "ImageMaterialStyleResGroupTag");
			}
		}
		if (!string.IsNullOrEmpty(defaultPath))
		{
			return GameApp.Res.LoadAsset<Material>(defaultPath, "ImageMaterialStyleResGroupTag");
		}
		return null;
	}
}
