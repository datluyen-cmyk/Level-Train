using System;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services.Prop.Handlers;
using RotateRings.Prop;
using UnityEngine;
using UnlockRing.Prop.Handlers;

namespace RotateRings.UI.PropGuide;

public class HammerGuideStrategy : IPropGuideStrategy
{
	private Action onDismiss;

	public void OnRegister(UIPropGuideView view, PropGuideOpenData data, Action onDismiss)
	{
		this.onDismiss = onDismiss;
		GameApp.Event.Listen<GameplayPointerDownEvent>((Action<GameplayPointerDownEvent>)OnPointerDown);
		view.PrepareRingPreviewLayout();
	}

	public void OnEnterSecondPhase(UIPropGuideView view)
	{
		view.ConfigureMaskInteraction(enableInteraction: false);
		RenderTexture val = CreatePreviewRT();
		if ((Object)(object)val != (Object)null)
		{
			view.ShowHintRingPreview(val);
		}
	}

	public void OnRemove(UIPropGuideView view)
	{
		GameApp.Event.Unlisten<GameplayPointerDownEvent>((Action<GameplayPointerDownEvent>)OnPointerDown);
		ReleasePreviewRT();
		view.ClearHintRingPreview();
	}

	private void OnPointerDown(GameplayPointerDownEvent e)
	{
		if (e?.PointerHandler == null && !IsHammerExecuting())
		{
			onDismiss?.Invoke();
		}
	}

	private bool IsHammerExecuting()
	{
		IGameplayManager active = Gameplay.Active;
		if (!(active is RingGameManager ringGameManager))
		{
			if (active is GameplayManager gameplayManager)
			{
				return gameplayManager.PropUseService?.TryGetHandler(EPropType.Hammer) is RotateRingHammerPropHandler rotateRingHammerPropHandler && rotateRingHammerPropHandler.IsHammerExecuting;
			}
			return false;
		}
		return ringGameManager.PropUseService?.TryGetHandler(EPropType.Hammer) is HammerPropHandler hammerPropHandler && hammerPropHandler.IsHammerExecuting;
	}

	private RenderTexture CreatePreviewRT()
	{
		IGameplayManager active = Gameplay.Active;
		if (!(active is RingGameManager ringGameManager))
		{
			if (active is GameplayManager gameplayManager)
			{
				return gameplayManager.GuideRT?.CreateAllBoardPartGuideRT();
			}
			return null;
		}
		return ringGameManager.GuideRT?.CreateAllBoardPartGuideRT();
	}

	private void ReleasePreviewRT()
	{
		IGameplayManager active = Gameplay.Active;
		if (!(active is RingGameManager ringGameManager))
		{
			if (active is GameplayManager gameplayManager)
			{
				gameplayManager.GuideRT?.ReleaseGuideRenderTexture();
			}
		}
		else
		{
			ringGameManager.GuideRT?.ReleaseGuideRenderTexture();
		}
	}
}
