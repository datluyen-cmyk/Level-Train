using Game.UI;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public class UILevelWinHalloweenTitleView : GameWidgetView
{
	private const string Tag = "[UILevelWinHalloweenTitleView] ";

	private const string OpenAnimationName = "open";

	private const string IdleAnimationName = "idle";

	private const int SpineTrackIndex = 0;

	private TrackEntry openTrackEntry;

	private TrackEntryDelegate openCompleteHandler;

	private SkeletonGraphic effSpine;

	private TMP_Text textTitle;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		effSpine = behaviour.Get<SkeletonGraphic>("eff_spine");
		textTitle = behaviour.Get<TMP_Text>("textTitle");
	}

	public void SetTitle(string title)
	{
		if ((Object)(object)textTitle != (Object)null)
		{
			textTitle.text = title ?? string.Empty;
		}
	}

	public void PlayOpenAnimation()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		if ((Object)(object)effSpine == (Object)null)
		{
			return;
		}
		effSpine.Initialize(false);
		AnimationState animationState = effSpine.AnimationState;
		Skeleton skeleton = effSpine.Skeleton;
		SkeletonData skeletonData = ((skeleton != null) ? skeleton.Data : null);
		if (animationState == null || skeletonData == null)
		{
			return;
		}
		if (skeletonData.FindAnimation("open") == null)
		{
			PlayIdleAnimation(animationState, skeletonData);
			return;
		}
		UnsubscribeOpenComplete();
		ResetSpinePoseForReplay(effSpine);
		openTrackEntry = animationState.SetAnimation(0, "open", false);
		if (openTrackEntry == null)
		{
			PlayIdleAnimation(animationState, skeletonData);
			return;
		}
		openCompleteHandler = (TrackEntryDelegate)delegate(TrackEntry completedTrack)
		{
			if (completedTrack == openTrackEntry)
			{
				UnsubscribeOpenComplete();
				PlayIdleAnimation(animationState, skeletonData);
			}
		};
		openTrackEntry.Complete += openCompleteHandler;
		ApplySpineFirstFrame(effSpine);
		if (openTrackEntry.IsComplete)
		{
			openCompleteHandler.Invoke(openTrackEntry);
		}
	}

	public override void Dispose()
	{
		UnsubscribeOpenComplete();
		if ((Object)(object)effSpine != (Object)null)
		{
			AnimationState animationState = effSpine.AnimationState;
			if (animationState != null)
			{
				animationState.ClearTracks();
			}
			Skeleton skeleton = effSpine.Skeleton;
			if (skeleton != null)
			{
				skeleton.SetToSetupPose();
			}
		}
		effSpine = null;
		textTitle = null;
		base.Dispose();
	}

	private void PlayIdleAnimation(AnimationState animationState, SkeletonData skeletonData)
	{
		if (skeletonData.FindAnimation("idle") != null)
		{
			animationState.SetAnimation(0, "idle", true);
			ApplySpineFirstFrame(effSpine);
		}
	}

	private void UnsubscribeOpenComplete()
	{
		if (openTrackEntry != null && openCompleteHandler != null)
		{
			openTrackEntry.Complete -= openCompleteHandler;
		}
		openTrackEntry = null;
		openCompleteHandler = null;
	}

	private static void ResetSpinePoseForReplay(SkeletonGraphic graphic)
	{
		if (!((Object)(object)graphic == (Object)null))
		{
			AnimationState animationState = graphic.AnimationState;
			Skeleton skeleton = graphic.Skeleton;
			if (animationState != null && skeleton != null)
			{
				animationState.ClearTracks();
				skeleton.SetToSetupPose();
				animationState.Apply(skeleton);
			}
		}
	}

	private static void ApplySpineFirstFrame(SkeletonGraphic graphic)
	{
		if (!((Object)(object)graphic == (Object)null))
		{
			graphic.Update(0f);
			graphic.UpdateMesh();
		}
	}
}
