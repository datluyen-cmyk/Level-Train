using System;
using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using UnityEngine.UI;
using Vibration;
using XY.Common.ParticleImage;

namespace RotateRings.UI.DailyFirstWin;

public static class DailyFirstWinViewUtil
{
	public sealed class DailyFirstWinPieceLandFlyRuntime
	{
		public GameObject FlyEffectPrefab;

		public GameObject ActiveFlyInstance;
	}

	public readonly struct DailyFirstWinPieceLandFlyPlayArgs
	{
		public readonly Image FlyTemplate;

		public readonly string SpriteName;

		public readonly RectTransform FlyTarget;

		public readonly string LoadAssetGroup;

		public readonly Action OnFirstParticleFinished;

		public readonly Action OnLastParticleFinished;

		public DailyFirstWinPieceLandFlyPlayArgs(Image flyTemplate, string spriteName, RectTransform flyTarget, string loadAssetGroup, Action onFirstParticleFinished, Action onLastParticleFinished)
		{
			FlyTemplate = flyTemplate;
			SpriteName = spriteName;
			FlyTarget = flyTarget;
			LoadAssetGroup = loadAssetGroup;
			OnFirstParticleFinished = onFirstParticleFinished;
			OnLastParticleFinished = onLastParticleFinished;
		}
	}

	private const float PuzzleCompleteLandAnimFps = 60f;

	private const string LogTag = "UI";

	private const string PieceLandLogPrefix = "[DailyFirstWinViewUtil] ";

	public const string PieceDropAnimName = "anim_dailyfirstwinnewpiece_drop";

	public const string PieceCompleteAnimName = "anim_dailyfirstwinnewpiece_complete";

	public const int PuzzleCompleteStampFrame = 75;

	public const int PuzzleCompleteResultFrame = 90;

	private const string PieceFlyEffectLocation = "eff_piece_fly_01";

	private const float PieceFlyLandStartDelaySec = 0f;

	public static bool TryGetAnimation(Component owner, out Animation animation)
	{
		animation = (((Object)(object)owner != (Object)null) ? owner.GetComponent<Animation>() : null);
		return (Object)(object)animation != (Object)null;
	}

	public static void PlayLoopClip(Animation animation, string clipName)
	{
		if (!((Object)(object)animation == (Object)null) && !string.IsNullOrEmpty(clipName) && !((Object)(object)animation.GetClip(clipName) == (Object)null))
		{
			animation.Rewind(clipName);
			AnimationState val = animation[clipName];
			if ((TrackedReference)(object)val != (TrackedReference)null)
			{
				val.wrapMode = (WrapMode)2;
			}
			animation.Play(clipName, (PlayMode)4);
		}
	}

	public static bool SampleAnimationFrame(Animation animation, string clipName, float time)
	{
		if ((Object)(object)animation == (Object)null || string.IsNullOrEmpty(clipName) || (Object)(object)animation.GetClip(clipName) == (Object)null)
		{
			return false;
		}
		animation.Stop();
		AnimationState val = animation[clipName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return false;
		}
		val.enabled = true;
		val.weight = 1f;
		val.time = Mathf.Clamp(time, 0f, val.length);
		animation.Sample();
		val.enabled = false;
		return true;
	}

	public static void PlayClipWithRewind(Component owner, string clipName)
	{
		if (TryGetAnimation(owner, out var animation) && !string.IsNullOrEmpty(clipName))
		{
			AnimationState val = animation[clipName];
			if (!((TrackedReference)(object)val == (TrackedReference)null))
			{
				animation.Stop(clipName);
				val.time = 0f;
				animation.Sample();
				val.wrapMode = (WrapMode)8;
				animation.Play(clipName);
			}
		}
	}

	public static void PlayParticleSystemsUnder(GameObject root)
	{
		if (!((Object)(object)root == (Object)null))
		{
			ParticleSystem[] componentsInChildren = root.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Simulate(0f, false, true);
				componentsInChildren[i].Play();
			}
		}
	}

	public static int ResolveTargetDay(int currentDay, int periodDays, bool hasFirstWinToday)
	{
		int num = (hasFirstWinToday ? currentDay : (currentDay + 1));
		if (currentDay <= 0)
		{
			num = 1;
		}
		return Mathf.Clamp(num, 1, periodDays);
	}

	public static HashSet<int> BuildMilestoneSet(IReadOnlyList<int> milestoneDays, int periodDays)
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (milestoneDays == null)
		{
			return hashSet;
		}
		for (int i = 0; i < milestoneDays.Count; i++)
		{
			int num = milestoneDays[i];
			if (num > 0 && num <= periodDays)
			{
				hashSet.Add(num);
			}
		}
		return hashSet;
	}

	public static void PreparePieceFlyTemplate(Image flyTemplate, string spriteName)
	{
		if (!((Object)(object)flyTemplate == (Object)null))
		{
			if (!string.IsNullOrEmpty(spriteName))
			{
				NewPieceEffectTexturesUtil.TryApplyPieceSprite(flyTemplate, spriteName, setNativeSize: true);
			}
			((Component)flyTemplate).gameObject.SetActive(false);
		}
	}

	private static void ConfigurePieceFlyZeroStartRotation(Image flyTemplate, XYParticleImage particle)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)flyTemplate != (Object)null)
		{
			((Transform)((Graphic)flyTemplate).rectTransform).localRotation = Quaternion.identity;
		}
		if (!((Object)(object)particle == (Object)null))
		{
			particle.startRotation = new XYSeparatedMinMaxCurve(0f);
		}
	}

	private static void RevealPieceFlyTemplateUntilParticleVisible(Image flyTemplate, XYParticleImage particle, GameObject flyInstanceOwner)
	{
		if ((Object)(object)flyTemplate == (Object)null)
		{
			return;
		}
		if ((Object)(object)particle != (Object)null && particle.particleCount > 0)
		{
			((Component)flyTemplate).gameObject.SetActive(false);
			return;
		}
		((Component)flyTemplate).gameObject.SetActive(true);
		Object val = (Object)(((Object)(object)flyInstanceOwner != (Object)null) ? ((object)flyInstanceOwner) : ((object)flyTemplate));
		GameApp.Timer.NextFrame(val, (Action)delegate
		{
			if ((Object)(object)flyTemplate != (Object)null)
			{
				((Component)flyTemplate).gameObject.SetActive(false);
			}
		});
	}

	public static bool TryPlayPieceLandFly(DailyFirstWinPieceLandFlyRuntime runtime, in DailyFirstWinPieceLandFlyPlayArgs args)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (runtime == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(args.SpriteName))
		{
			return false;
		}
		if ((Object)(object)args.FlyTemplate == (Object)null)
		{
			return false;
		}
		Transform parent = ((Transform)((Graphic)args.FlyTemplate).rectTransform).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val == (Object)null || !TryGetPieceFlySpawnLocalPosition(args.FlyTemplate, val, out var localPosition))
		{
			return false;
		}
		if ((Object)(object)args.FlyTarget == (Object)null)
		{
			return false;
		}
		if (runtime.FlyEffectPrefab == null)
		{
			runtime.FlyEffectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_piece_fly_01", args.LoadAssetGroup);
		}
		if ((Object)(object)runtime.FlyEffectPrefab == (Object)null)
		{
			return false;
		}
		GameObject val2 = GameApp.Pool.Spawn(runtime.FlyEffectPrefab, (Transform)(object)val);
		if ((Object)(object)val2 == (Object)null)
		{
			return false;
		}
		Transform transform = val2.transform;
		RectTransform val3 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (val3 != null)
		{
			val3.anchoredPosition = localPosition;
		}
		else
		{
			transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
		}
		transform.localRotation = Quaternion.identity;
		transform.localScale = runtime.FlyEffectPrefab.transform.localScale;
		val2.SetActive(false);
		runtime.ActiveFlyInstance = val2;
		XYParticleImage component = val2.GetComponent<XYParticleImage>();
		if ((Object)(object)component == (Object)null)
		{
			RecyclePieceLandFly(runtime);
			return false;
		}
		component.PlayMode = PlayMode.None;
		component.particlePrefab = ((Graphic)args.FlyTemplate).rectTransform;
		component.startDelay = 0f;
		Sprite sprite = args.FlyTemplate.sprite;
		if ((Object)(object)sprite == (Object)null && !NewPieceEffectTexturesUtil.TryApplyPieceSprite(args.FlyTemplate, args.SpriteName, setNativeSize: true))
		{
			RecyclePieceLandFly(runtime);
			return false;
		}
		sprite = args.FlyTemplate.sprite;
		if ((Object)(object)sprite == (Object)null)
		{
			RecyclePieceLandFly(runtime);
			return false;
		}
		val2.SetActive(true);
		Canvas.ForceUpdateCanvases();
		ConfigurePieceFlyZeroStartRotation(args.FlyTemplate, component);
		component.Stop(stopAndClear: true);
		component.Play((IReadOnlyList<Sprite>)(object)new Sprite[1] { sprite }, (Transform)(object)args.FlyTarget, args.OnFirstParticleFinished, null, args.OnLastParticleFinished);
		if (!component.isPlaying && !component.isEmitting)
		{
			RecyclePieceLandFly(runtime);
			return false;
		}
		Canvas.ForceUpdateCanvases();
		RevealPieceFlyTemplateUntilParticleVisible(args.FlyTemplate, component, val2);
		return true;
	}

	public static void RecyclePieceLandFly(DailyFirstWinPieceLandFlyRuntime runtime)
	{
		if (runtime != null && !((Object)(object)runtime.ActiveFlyInstance == (Object)null))
		{
			GameObject activeFlyInstance = runtime.ActiveFlyInstance;
			runtime.ActiveFlyInstance = null;
			XYParticleImage component = activeFlyInstance.GetComponent<XYParticleImage>();
			if ((Object)(object)component != (Object)null)
			{
				component.Stop(stopAndClear: true);
			}
			GameApp.Pool.Recycle(activeFlyInstance);
		}
	}

	public static bool TryGetPieceFlySpawnLocalPosition(Image flyTemplate, RectTransform flyParent, out Vector2 localPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		localPosition = Vector2.zero;
		if ((Object)(object)flyParent == (Object)null || (Object)(object)flyTemplate == (Object)null)
		{
			return false;
		}
		RectTransform rectTransform = ((Graphic)flyTemplate).rectTransform;
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector3 val = (array[0] + array[2]) * 0.5f;
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint((Camera)null, val);
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(flyParent, val2, (Camera)null, ref localPosition);
	}

	public static float PlayPieceLandLegacyClip(Animation anim, string clipName)
	{
		if ((Object)(object)anim == (Object)null || string.IsNullOrEmpty(clipName))
		{
			return 0f;
		}
		AnimationClip clip = anim.GetClip(clipName);
		if ((Object)(object)clip == (Object)null)
		{
			return 0f;
		}
		anim.Stop();
		anim.Rewind(clipName);
		anim.Play(clipName, (PlayMode)4);
		return clip.length;
	}

	public static void KillPuzzleCompleteCelebrationTimers(ref int stampTimerId, ref int resultTimerId)
	{
		if (stampTimerId >= 0)
		{
			GameApp.Timer.Kill(stampTimerId);
			stampTimerId = -1;
		}
		if (resultTimerId >= 0)
		{
			GameApp.Timer.Kill(resultTimerId);
			resultTimerId = -1;
		}
	}

	public static void SchedulePuzzleCompleteCelebrationSounds(Component timerHost, ref int stampTimerId, ref int resultTimerId)
	{
		KillPuzzleCompleteCelebrationTimers(ref stampTimerId, ref resultTimerId);
		if (!((Object)(object)timerHost == (Object)null))
		{
			stampTimerId = GameApp.Timer.Delay((Object)(object)timerHost, PuzzleCompleteFrameToSec(75), (Action)PlayPuzzleStamp);
			resultTimerId = GameApp.Timer.Delay((Object)(object)timerHost, PuzzleCompleteFrameToSec(90), (Action)PlayPuzzleResult);
		}
	}

	public static void AlignRectTransformToPiece(RectTransform effect, RectTransform piece)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)effect == (Object)null) && !((Object)(object)piece == (Object)null))
		{
			if ((Object)(object)((Transform)effect).parent == (Object)(object)((Transform)piece).parent)
			{
				effect.anchoredPosition = piece.anchoredPosition;
				return;
			}
			Rect rect = piece.rect;
			((Transform)effect).position = ((Transform)piece).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
		}
	}

	public static Vector2 GetRectCenterInParent(RectTransform rect, RectTransform parent)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rect == (Object)null || (Object)(object)parent == (Object)null)
		{
			return Vector2.zero;
		}
		Rect rect2 = rect.rect;
		Vector3 val = ((Transform)rect).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect2)).center));
		return Vector2.op_Implicit(((Transform)parent).InverseTransformPoint(val));
	}

	public static void PlayPuzzlePieceReveal()
	{
		AudioService.PlayUiSound(AudioKeys.DailyPuzzlePieceDone.Value);
		VibrateUtility.Vibrate(1504);
	}

	public static void PlayPuzzleAllDone()
	{
		AudioService.PlayUiSound(AudioKeys.DailyPuzzleAllDone.Value);
	}

	public static void PlayPuzzleStamp()
	{
		AudioService.PlayUiSound(AudioKeys.DailyPuzzleStamp.Value);
	}

	public static void PlayPuzzleResult()
	{
		AudioService.PlayUiSound(AudioKeys.DailyPuzzleResult.Value);
	}

	public static float PuzzleCompleteFrameToSec(int frame)
	{
		return (float)frame / 60f;
	}
}
