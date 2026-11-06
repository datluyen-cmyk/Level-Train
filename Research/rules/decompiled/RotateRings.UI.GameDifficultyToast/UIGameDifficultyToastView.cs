using Game.UI;
using HsTileMahJong;
using RotateRings.Common;
using Spine;
using Spine.Unity;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.GameDifficultyToast;

public class UIGameDifficultyToastView : GameBaseView
{
	private const string BaselineVeryHardAnim = "anim_UIGameDifficultyToastView_01_start";

	private const string BaselineHardAnim = "anim_UIGameDifficultyToastView_02_start";

	private const string SquirrelHardAnim = "anim_UIGameDifficultyToastView2_start";

	private const string SquirrelVeryHardAnim = "anim_UIGameDifficultyToastView2super_start";

	private const string SquirrelHardSpineAnim = "hard_level";

	private const string SquirrelVeryHardSpineAnim = "super_hard";

	private const string DemonRingHardAnim = "anim_uigamedifficultytoastviewhard_start";

	private const string DemonRingSuperHardAnim = "anim_uigamedifficultytoastviewsuperhard_start";

	private const string DemonRingBgOpenAnim = "anim_bgmask_open";

	private const string DemonRingBgCloseAnim = "anim_bgmask_close";

	private int currentStyleId = 1;

	private Animation demonRingBgAnimation;

	private GameObject squirrelHardNode;

	private GameObject squirrelVeryHardNode;

	private AutoPlaySpineAnimation squirrelHardSpine;

	private AutoPlaySpineAnimation squirrelVeryHardSpine;

	private AnimationState squirrelSpineState;

	private string squirrelSpineClipName;

	public Animation Animation;

	public void SetStyleId(int styleId)
	{
		StopSquirrelSpineSynchronization();
		currentStyleId = styleId;
		demonRingBgAnimation = null;
		squirrelHardNode = null;
		squirrelVeryHardNode = null;
		squirrelHardSpine = null;
		squirrelVeryHardSpine = null;
		switch (styleId)
		{
		case 2:
			squirrelHardNode = ((UIBaseView)this).Behaviour.Get<GameObject>("Hard");
			squirrelVeryHardNode = ((UIBaseView)this).Behaviour.Get<GameObject>("SuperHard");
			squirrelHardSpine = (((Object)(object)squirrelHardNode != (Object)null) ? squirrelHardNode.GetComponentInChildren<AutoPlaySpineAnimation>(true) : null);
			squirrelVeryHardSpine = (((Object)(object)squirrelVeryHardNode != (Object)null) ? squirrelVeryHardNode.GetComponentInChildren<AutoPlaySpineAnimation>(true) : null);
			break;
		case 3:
		{
			Transform val = (Transform)(((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((object)((Component)((UIBaseView)this).Behaviour).gameObject.transform) : ((object)(((Object)(object)((UIBaseView)this).NodeRect != (Object)null) ? ((UIBaseView)this).NodeRect : null)));
			Transform val2 = (((Object)(object)val != (Object)null) ? val.Find("bg") : null);
			if (!((Object)(object)val2 == (Object)null))
			{
				demonRingBgAnimation = ((Component)val2).GetComponent<Animation>();
			}
			break;
		}
		}
	}

	public void SetDifficulty(ELevelDifficulty difficulty, int styleId)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		StopSquirrelSpineSynchronization();
		if (styleId == 2)
		{
			if ((Object)(object)Animation != (Object)null)
			{
				Animation.Stop();
			}
			if ((Object)(object)squirrelHardNode != (Object)null)
			{
				squirrelHardNode.SetActive(false);
			}
			if ((Object)(object)squirrelVeryHardNode != (Object)null)
			{
				squirrelVeryHardNode.SetActive(false);
			}
			AutoPlaySpineAnimation autoPlaySpineAnimation = ((difficulty == ELevelDifficulty.VeryHard) ? squirrelVeryHardSpine : squirrelHardSpine);
			string animationName = ((difficulty == ELevelDifficulty.VeryHard) ? "super_hard" : "hard_level");
			if (difficulty == ELevelDifficulty.Hard || difficulty == ELevelDifficulty.VeryHard)
			{
				ResetSquirrelSpine(autoPlaySpineAnimation, animationName);
				if ((Object)(object)autoPlaySpineAnimation != (Object)null && (Object)(object)autoPlaySpineAnimation.skeletonGraphic != (Object)null && autoPlaySpineAnimation.skeletonGraphic.IsValid)
				{
					squirrelSpineState = autoPlaySpineAnimation.skeletonGraphic.AnimationState;
					squirrelSpineClipName = ResolveClipName(difficulty, styleId);
					squirrelSpineState.Start += new TrackEntryDelegate(OnSquirrelSpineStart);
				}
			}
		}
		string text = ResolveClipName(difficulty, styleId);
		if (text == null || (Object)(object)Animation == (Object)null)
		{
			return;
		}
		if ((TrackedReference)(object)Animation[text] == (TrackedReference)null)
		{
			if (styleId == 2 || styleId == 3)
			{
				return;
			}
			string text2 = ResolveBaselineClip(difficulty);
			if (text2 == null || !((TrackedReference)(object)Animation[text2] != (TrackedReference)null))
			{
				return;
			}
			text = text2;
		}
		PlayAnimation(text);
		if (currentStyleId == 3 && (Object)(object)demonRingBgAnimation != (Object)null && (TrackedReference)(object)demonRingBgAnimation["anim_bgmask_open"] != (TrackedReference)null)
		{
			PlayBgAnimation("anim_bgmask_open");
		}
	}

	public void SetDifficulty(ELevelDifficulty difficulty)
	{
		SetDifficulty(difficulty, 1);
	}

	public void StopSquirrelSpineSynchronization()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if (squirrelSpineState != null)
		{
			squirrelSpineState.Start -= new TrackEntryDelegate(OnSquirrelSpineStart);
			squirrelSpineState = null;
		}
		squirrelSpineClipName = null;
	}

	private void OnSquirrelSpineStart(TrackEntry entry)
	{
		if (!((Object)(object)Animation == (Object)null) && squirrelSpineClipName != null)
		{
			AnimationState val = Animation[squirrelSpineClipName];
			if (!((TrackedReference)(object)val == (TrackedReference)null) && Animation.IsPlaying(squirrelSpineClipName))
			{
				entry.TrackTime = Mathf.Max(0f, val.time);
			}
		}
	}

	private static void ResetSquirrelSpine(AutoPlaySpineAnimation spine, string animationName)
	{
		if (!((Object)(object)spine == (Object)null) && !((Object)(object)spine.skeletonGraphic == (Object)null))
		{
			SkeletonGraphic skeletonGraphic = spine.skeletonGraphic;
			skeletonGraphic.startingAnimation = animationName;
			skeletonGraphic.startingLoop = spine.loop;
			skeletonGraphic.Initialize(false);
			skeletonGraphic.AnimationState.ClearTracks();
			skeletonGraphic.Skeleton.SetToSetupPose();
			spine.PlaySpine();
			skeletonGraphic.Update(0f);
		}
	}

	private void PlayAnimation(string clipName)
	{
		if ((Object)(object)Animation == (Object)null)
		{
			return;
		}
		AnimationState val = Animation[clipName];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (Animation.IsPlaying(clipName))
			{
				Animation.Stop(clipName);
				val.time = 0f;
				Animation.Sample();
			}
			val.wrapMode = (WrapMode)8;
			Animation.Play(clipName);
		}
	}

	private void PlayBgAnimation(string clipName)
	{
		if ((Object)(object)demonRingBgAnimation == (Object)null)
		{
			return;
		}
		AnimationState val = demonRingBgAnimation[clipName];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (demonRingBgAnimation.IsPlaying(clipName))
			{
				demonRingBgAnimation.Stop(clipName);
				val.time = 0f;
				demonRingBgAnimation.Sample();
			}
			val.wrapMode = (WrapMode)8;
			demonRingBgAnimation.Play(clipName);
		}
	}

	private static string ResolveClipName(ELevelDifficulty difficulty, int styleId)
	{
		switch (styleId)
		{
		case 2:
			return difficulty switch
			{
				ELevelDifficulty.Hard => "anim_UIGameDifficultyToastView2_start", 
				ELevelDifficulty.VeryHard => "anim_UIGameDifficultyToastView2super_start", 
				_ => null, 
			};
		case 3:
			if (difficulty != ELevelDifficulty.VeryHard)
			{
				return "anim_uigamedifficultytoastviewhard_start";
			}
			return "anim_uigamedifficultytoastviewsuperhard_start";
		default:
			return ResolveBaselineClip(difficulty);
		}
	}

	private static string ResolveBaselineClip(ELevelDifficulty difficulty)
	{
		return difficulty switch
		{
			ELevelDifficulty.VeryHard => "anim_UIGameDifficultyToastView_01_start", 
			ELevelDifficulty.Hard => "anim_UIGameDifficultyToastView_02_start", 
			_ => null, 
		};
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Animation = behaviour.Get<Animation>("Animation");
	}
}
