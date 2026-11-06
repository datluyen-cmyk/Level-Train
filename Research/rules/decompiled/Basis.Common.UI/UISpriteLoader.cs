using System;
using Script.Tools;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace Basis.Common.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class UISpriteLoader : MonoBehaviour
{
	[Header("资源标识")]
	[Tooltip("图片文件名(不含路径与扩展名)。运行时配合 UIStyleCache 换肤查询;为空时回退 defaultPath。")]
	[SerializeField]
	private string spriteName;

	[Header("加载选项")]
	[Tooltip("true 走 Large/ 目录(UIStyleCache.GetStyledLargeSpritePath);false 走 Sprite/ 目录(GetStyledSpritePath)。")]
	[SerializeField]
	private bool isLarge = true;

	[Tooltip("运行时是否异步加载(默认同步,立即赋值)。")]
	[SerializeField]
	private bool useAsync;

	[Tooltip("OnEnable 时自动加载;关闭则需手动调用 LoadSprite()。")]
	[SerializeField]
	private bool loadOnEnable = true;

	[HideInInspector]
	[SerializeField]
	private string moduleName;

	[HideInInspector]
	[SerializeField]
	private string defaultPath;

	private const string LogTag = "UISpriteLoader";

	private const string LargeDirPrefix = "Assets/02_Res/Art/UI/Large/";

	private const string SpriteDirPrefix = "Assets/02_Res/Art/UI/Sprite/";

	private int loadGeneration;

	private Image cachedImage;

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

	public string SpriteName => spriteName;

	public bool IsLarge => isLarge;

	private void OnEnable()
	{
		if (loadOnEnable)
		{
			LoadSprite();
		}
	}

	private void OnDisable()
	{
		UnloadSprite();
	}

	private void OnDestroy()
	{
		loadGeneration++;
		cachedImage = null;
	}

	public void SetSprite(string newSpriteName, bool large)
	{
		spriteName = newSpriteName;
		isLarge = large;
		UnloadSprite();
		if (((Behaviour)this).isActiveAndEnabled)
		{
			LoadSprite();
		}
	}

	public void LoadSprite()
	{
		Image image = Image;
		if (!((Object)(object)image == (Object)null))
		{
			string text = ResolveRuntimePath();
			if (string.IsNullOrEmpty(text))
			{
				LogFlow("[加载跳过] " + GetNodeLabel() + " 路径为空");
				return;
			}
			LogFlow("[加载] " + GetNodeLabel() + " path=" + text);
			LoadAtRuntime(image, text);
		}
	}

	public void UnloadSprite()
	{
		loadGeneration++;
		Image image = Image;
		if ((Object)(object)image != (Object)null && (Object)(object)image.sprite != (Object)null)
		{
			string name = ((Object)image.sprite).name;
			image.sprite = null;
			LogFlow("[卸载] " + GetNodeLabel() + " sprite=" + name);
		}
	}

	public bool ClearSpriteReferenceForBuild()
	{
		Image image = Image;
		if ((Object)(object)image == (Object)null)
		{
			LogFlow("[清理跳过] " + GetNodeLabel() + " Image=null");
			return false;
		}
		if ((Object)(object)image.sprite == (Object)null)
		{
			LogFlow("[清理跳过] " + GetNodeLabel() + " sprite 已为 null");
			return false;
		}
		string name = ((Object)image.sprite).name;
		image.sprite = null;
		LogFlow("[清理] " + GetNodeLabel() + " 移除 sprite=" + name);
		return true;
	}

	public bool EnsureSpriteClearedForStageClose()
	{
		return ClearSpriteReferenceForBuild();
	}

	private string ResolveRuntimePath()
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			return defaultPath;
		}
		return spriteName;
	}

	private string ResolveStyledPath(string name, string styleName)
	{
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(styleName) || string.IsNullOrEmpty(moduleName))
		{
			return null;
		}
		string text = (isLarge ? "Assets/02_Res/Art/UI/Large/" : "Assets/02_Res/Art/UI/Sprite/");
		string text2 = text + styleName + "/" + moduleName + "/" + name + ".png";
		if (!YooAssets.CheckLocationValid(text2))
		{
			return null;
		}
		return text2;
	}

	private void LogFlow(string message)
	{
	}

	private string GetNodeLabel()
	{
		return (((Object)(object)((Component)this).gameObject != (Object)null) ? ((Object)((Component)this).gameObject).name : "null") + "(spriteName=" + spriteName + ")";
	}

	private void LoadAtRuntime(Image image, string path)
	{
		int generation = loadGeneration;
		((Behaviour)image).enabled = false;
		if (useAsync)
		{
			GameApp.Res.LoadAssetAsync<Sprite>(path, (Action<Sprite>)delegate(Sprite sprite)
			{
				if (generation == loadGeneration && !((Object)(object)image == (Object)null) && !((Object)(object)sprite == (Object)null))
				{
					image.sprite = sprite;
					((Behaviour)image).enabled = true;
				}
			}, (string)null);
		}
		else
		{
			Sprite val = SpriteManager.LoadSprite(path);
			if (!((Object)(object)val == (Object)null))
			{
				image.sprite = val;
				((Behaviour)image).enabled = true;
			}
		}
	}
}
