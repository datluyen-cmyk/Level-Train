using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XY.Common.ParticleImage;

public class XYPrefabInstancePool
{
	private readonly Transform _parent;

	private RectTransform _template;

	private readonly List<RectTransform> _inactive = new List<RectTransform>();

	private readonly List<RectTransform> _allInstances = new List<RectTransform>();

	private readonly HashSet<RectTransform> _inactiveSet = new HashSet<RectTransform>();

	private Sprite _templateSprite;

	public XYPrefabInstancePool(Transform parent, RectTransform template)
	{
		_parent = parent;
		_template = template;
		CacheTemplateSprite();
	}

	public void SetTemplate(RectTransform template)
	{
		_template = template;
		CacheTemplateSprite();
	}

	public void EnsureCapacity(int minAvailable)
	{
		if ((Object)(object)_template == (Object)null || minAvailable <= 0)
		{
			return;
		}
		while (_inactive.Count < minAvailable)
		{
			RectTransform val = CreateInstance();
			((Component)val).gameObject.SetActive(false);
			if (_inactiveSet.Add(val))
			{
				_inactive.Add(val);
			}
		}
	}

	public RectTransform Get()
	{
		if ((Object)(object)_template == (Object)null)
		{
			return null;
		}
		RectTransform val;
		if (_inactive.Count > 0)
		{
			val = _inactive[_inactive.Count - 1];
			_inactive.RemoveAt(_inactive.Count - 1);
			_inactiveSet.Remove(val);
		}
		else
		{
			val = CreateInstance();
		}
		((Component)val).gameObject.SetActive(true);
		return val;
	}

	public void Release(RectTransform instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			RestoreTemplateSprite(instance);
			((Component)instance).gameObject.SetActive(false);
			CanvasGroup val = default(CanvasGroup);
			if (((Component)instance).TryGetComponent<CanvasGroup>(ref val))
			{
				val.alpha = 1f;
			}
			if (_inactiveSet.Add(instance))
			{
				_inactive.Add(instance);
			}
		}
	}

	public void DestroyAll()
	{
		for (int num = _allInstances.Count - 1; num >= 0; num--)
		{
			DestroyObject(((Component)_allInstances[num]).gameObject);
		}
		_allInstances.Clear();
		_inactive.Clear();
		_inactiveSet.Clear();
	}

	private void CacheTemplateSprite()
	{
		_templateSprite = null;
		if (!((Object)(object)_template == (Object)null))
		{
			Image componentInChildren = ((Component)_template).GetComponentInChildren<Image>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				_templateSprite = componentInChildren.sprite;
			}
		}
	}

	private void RestoreTemplateSprite(RectTransform instance)
	{
		if (!((Object)(object)_templateSprite == (Object)null) && !((Object)(object)instance == (Object)null))
		{
			Image componentInChildren = ((Component)instance).GetComponentInChildren<Image>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				componentInChildren.sprite = _templateSprite;
			}
		}
	}

	private RectTransform CreateInstance()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(((Component)_template).gameObject, _parent);
		RectTransform component = obj.GetComponent<RectTransform>();
		((Transform)component).SetParent(_parent, false);
		((Object)obj).hideFlags = GetInstanceHideFlags();
		EnsureCanvasGroup(obj);
		_allInstances.Add(component);
		return component;
	}

	private static void EnsureCanvasGroup(GameObject go)
	{
		CanvasGroup val = default(CanvasGroup);
		if (!go.TryGetComponent<CanvasGroup>(ref val))
		{
			go.AddComponent<CanvasGroup>();
		}
	}

	private static HideFlags GetInstanceHideFlags()
	{
		return (HideFlags)53;
	}

	private static void DestroyObject(GameObject go)
	{
		if (!((Object)(object)go == (Object)null))
		{
			Object.Destroy((Object)(object)go);
		}
	}
}
