using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.PartStyle.Jelly;
using RotateRings.Services;
using UnityEngine;
using Vibration;

namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleJellyHandler : PartStyleHandlerBase
{
	private static readonly int IdAtlasPadding = Shader.PropertyToID("_AtlasPaddingPixels");

	private static readonly List<MeshRenderer> RendererBuffer = new List<MeshRenderer>(4);

	private readonly List<JellyShakeAnimator> activeJellyAnimators = new List<JellyShakeAnimator>(8);

	private const string RingMaterialPath = "mat_ring_jelly";

	private const string PullMaterialPath = "mat_ring_jelly";

	private const string FixedMaterialPath = "mat_catch_jelly";

	private const string SolveFragmentAddonEffectPath = "eff_cycle_boom_style_fragment_gp5_yeti";

	public override PartStyleType Style => PartStyleType.Jelly;

	internal void ClearJellyEffects()
	{
		for (int i = 0; i < activeJellyAnimators.Count; i++)
		{
			activeJellyAnimators[i].StopAndReset();
		}
		activeJellyAnimators.Clear();
	}

	protected override string GetRingMaterialPath()
	{
		return "mat_ring_jelly";
	}

	protected override string GetPullMaterialPath()
	{
		return "mat_ring_jelly";
	}

	protected override string GetFixedMaterialPath()
	{
		return "mat_catch_jelly";
	}

	public override string GetSolveFragmentAddonEffectPath()
	{
		return "eff_cycle_boom_style_fragment_gp5_yeti";
	}

	public override void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.JellyRingBreak.Value);
		VibrateUtility.Vibrate(1102);
	}

	public override void OnPartDragBegin(IGameplayObject gameplayObject)
	{
		CollectMeshRenderers(gameplayObject, RendererBuffer);
		TriggerJelly((IReadOnlyList<MeshRenderer>)RendererBuffer);
		RendererBuffer.Clear();
	}

	public override void OnDragCollisionStop(RingDragCollisionStopEvent evt)
	{
		AudioService.PlaySound(AudioKeys.JellyBlocked.Value);
		CollectMeshRenderers(evt?.MovingRoot, RendererBuffer);
		TriggerJelly((IReadOnlyList<MeshRenderer>)RendererBuffer);
		RendererBuffer.Clear();
	}

	private void TriggerJelly(MeshRenderer renderer)
	{
		if ((Object)(object)renderer == (Object)null)
		{
			return;
		}
		for (int i = 0; i < activeJellyAnimators.Count; i++)
		{
			JellyShakeAnimator jellyShakeAnimator = activeJellyAnimators[i];
			if ((Object)(object)jellyShakeAnimator.Renderer == (Object)(object)renderer)
			{
				jellyShakeAnimator.Trigger();
				return;
			}
		}
		JellyShakeAnimator jellyShakeAnimator2 = new JellyShakeAnimator(renderer, delegate
		{
			RemoveJellyByRenderer(renderer);
		});
		jellyShakeAnimator2.Trigger();
		if (jellyShakeAnimator2.IsPlaying)
		{
			activeJellyAnimators.Add(jellyShakeAnimator2);
		}
	}

	private void TriggerJelly(IReadOnlyList<MeshRenderer> renderers)
	{
		if (renderers != null && renderers.Count != 0)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				TriggerJelly(renderers[i]);
			}
		}
	}

	private void RemoveJellyByRenderer(MeshRenderer renderer)
	{
		for (int num = activeJellyAnimators.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)activeJellyAnimators[num].Renderer == (Object)(object)renderer)
			{
				activeJellyAnimators.RemoveAt(num);
			}
		}
	}

	private void CollectMeshRenderers(IGameplayObject gameplayObject, List<MeshRenderer> buffer)
	{
		Component val = (Component)((gameplayObject is Component) ? gameplayObject : null);
		CollectMeshRenderers((val != null) ? val : null, buffer);
	}

	private void CollectMeshRenderers(Component root, List<MeshRenderer> buffer)
	{
		buffer.Clear();
		if ((Object)(object)root == (Object)null)
		{
			return;
		}
		root.GetComponentsInChildren<MeshRenderer>(true, buffer);
		for (int num = buffer.Count - 1; num >= 0; num--)
		{
			MeshRenderer val = buffer[num];
			Material val2 = (((Object)(object)val != (Object)null) ? ((Renderer)val).sharedMaterial : null);
			if ((Object)(object)val2 == (Object)null || !val2.HasProperty(IdAtlasPadding))
			{
				buffer.RemoveAt(num);
			}
		}
	}
}
