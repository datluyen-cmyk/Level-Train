using System;
using UnityEngine;

namespace Basis.Common.UI;

public class UIEffectLoader : UIResourceLoaderBase
{
	private Animator cachedAnimator;

	private ParticleSystem[] cachedParticles;

	protected override string LogTag => "UIEffectLoader";

	public void SetEffect(string newEffectName)
	{
		SetResource(newEffectName);
	}

	protected override string ResolvePath(string name)
	{
		return name;
	}

	protected override void OnInstanceChanged(GameObject newInstance)
	{
		cachedAnimator = newInstance.GetComponent<Animator>();
		cachedParticles = newInstance.GetComponentsInChildren<ParticleSystem>(true);
	}

	protected override void OnInstanceReady(GameObject readyInstance)
	{
		if (base.ShouldPlayOnReady)
		{
			RestartEffect();
		}
	}

	protected override void OnPlayWithInstance()
	{
		RestartEffect();
	}

	private void RestartEffect()
	{
		Animator animator = cachedAnimator;
		if ((Object)(object)animator != (Object)null)
		{
			((Behaviour)animator).enabled = false;
			GameApp.Timer.Delay((Object)(object)((Component)this).transform, 0.02f, (Action)delegate
			{
				if (!((Object)(object)animator == (Object)null))
				{
					((Behaviour)animator).enabled = true;
					animator.Rebind();
					animator.Update(0f);
				}
			});
		}
		if (cachedParticles == null)
		{
			return;
		}
		for (int num = 0; num < cachedParticles.Length; num++)
		{
			ParticleSystem val = cachedParticles[num];
			if (!((Object)(object)val == (Object)null))
			{
				val.Clear(true);
				val.Play(true);
			}
		}
	}
}
