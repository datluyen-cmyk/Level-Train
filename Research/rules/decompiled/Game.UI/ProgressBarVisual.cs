using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI;

[DisallowMultipleComponent]
public sealed class ProgressBarVisual : MonoBehaviour
{
	[SerializeField]
	private Slider progressSlider;

	[SerializeField]
	private Slider shortProgressSlider;

	[SerializeField]
	private RectTransform normalProgressFill;

	[SerializeField]
	private RectTransform shortProgressRoot;

	[SerializeField]
	[Range(0f, 1f)]
	private float shortProgressThreshold = 0.12f;

	public float value
	{
		get
		{
			if (!((Object)(object)progressSlider != (Object)null))
			{
				if (!((Object)(object)shortProgressSlider != (Object)null))
				{
					return 0f;
				}
				return shortProgressSlider.normalizedValue;
			}
			return progressSlider.normalizedValue;
		}
	}

	public void SetValueWithoutNotify(float normalizedValue)
	{
		SetSliderValueWithoutNotify(progressSlider, normalizedValue);
		SetSliderValueWithoutNotify(shortProgressSlider, normalizedValue);
		ApplyVisual();
	}

	public void SetProgress(float normalizedValue)
	{
		SetSliderValue(progressSlider, normalizedValue);
		SetSliderValue(shortProgressSlider, normalizedValue);
		ApplyVisual();
	}

	public void SetValue(float currentValue, float maxValue)
	{
		SetProgress((maxValue > 0f) ? (currentValue / maxValue) : 0f);
	}

	private void OnEnable()
	{
		if ((Object)(object)progressSlider != (Object)null)
		{
			((UnityEvent<float>)(object)progressSlider.onValueChanged).AddListener((UnityAction<float>)HandleProgressValueChanged);
		}
		if ((Object)(object)shortProgressSlider != (Object)null)
		{
			((UnityEvent<float>)(object)shortProgressSlider.onValueChanged).AddListener((UnityAction<float>)HandleProgressValueChanged);
		}
	}

	private void Start()
	{
		ApplyVisual();
	}

	private void OnDisable()
	{
		if ((Object)(object)progressSlider != (Object)null)
		{
			((UnityEvent<float>)(object)progressSlider.onValueChanged).RemoveListener((UnityAction<float>)HandleProgressValueChanged);
		}
		if ((Object)(object)shortProgressSlider != (Object)null)
		{
			((UnityEvent<float>)(object)shortProgressSlider.onValueChanged).RemoveListener((UnityAction<float>)HandleProgressValueChanged);
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		if (Application.isPlaying)
		{
			ApplyVisual();
		}
	}

	private void HandleProgressValueChanged(float value)
	{
		ApplyVisual();
	}

	private void ApplyVisual()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)normalProgressFill == (Object)null || (Object)(object)shortProgressRoot == (Object)null)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			Scene scene = ((Component)this).gameObject.scene;
			if (!((Scene)(ref scene)).IsValid())
			{
				return;
			}
		}
		float num = value;
		bool flag = num > 0f && num < shortProgressThreshold;
		SetActive(normalProgressFill, !flag);
		SetActive(shortProgressRoot, flag);
	}

	private static void SetSliderValueWithoutNotify(Slider slider, float normalizedValue)
	{
		if ((Object)(object)slider != (Object)null)
		{
			slider.SetValueWithoutNotify(Mathf.Clamp01(normalizedValue));
		}
	}

	private static void SetSliderValue(Slider slider, float normalizedValue)
	{
		if ((Object)(object)slider != (Object)null)
		{
			slider.normalizedValue = Mathf.Clamp01(normalizedValue);
		}
	}

	private static void SetActive(RectTransform target, bool isActive)
	{
		if (!((Object)(object)target == (Object)null) && ((Component)target).gameObject.activeSelf != isActive)
		{
			((Component)target).gameObject.SetActive(isActive);
		}
	}
}
