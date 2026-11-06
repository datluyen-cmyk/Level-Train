using System;
using Spine;
using Spine.Unity;
using UniLog;
using UnityEngine;
using XingYun.SpineAnimSequence;

namespace Basis.Common.UI;

public class UISpineLoader : UIResourceLoaderBase
{
	[SerializeField]
	[Tooltip("OnEnable 模式下加载后播放的动画名，为空则使用 Spine 预制体上的 startingAnimation。")]
	private string animationName;

	[SerializeField]
	[Tooltip("OnEnable 模式或未指定 isLoop 时使用的循环设置。")]
	private bool loop;

	[SerializeField]
	[Tooltip("勾选后允许通过 SetSkin 切换皮肤；未勾选时 SetSkin 与换肤逻辑均不执行。")]
	private bool enableSkinChange;

	[SerializeField]
	[Tooltip("显式指定要播放的 SpineAnimSequencePlayer；未指定时使用原有 Play 流程。")]
	private SpineAnimSequencePlayer embeddedSequencePlayer;

	private SkeletonGraphic cachedSkeletonGraphic;

	private bool pendingSequencePlay;

	private Action<GameObject> pendingSequenceInstanceReady;

	private string pendingAnimationName;

	private bool pendingLoop;

	private int pendingTrackIndex;

	private Action<GameObject> pendingInstanceReady;

	private Action pendingAnimationComplete;

	private TrackEntry activeCompleteTrack;

	private TrackEntryDelegate activeCompleteHandler;

	private string runtimeSkinName;

	private bool wasLoadedBeforeDisable;

	private string lastPlayedAnim;

	private bool lastPlayedLoop;

	private int lastPlayedTrack;

	private Action lastOnComplete;

	private Action<GameObject> lastOnInstanceReady;

	private bool suppressRestoreUntilDisable;

	protected override string LogTag => "UISpineLoader";

	public SkeletonGraphic SkeletonGraphic
	{
		get
		{
			if ((Object)(object)instance != (Object)null)
			{
				EnsureSkeletonGraphicCache(instance);
				return cachedSkeletonGraphic;
			}
			if ((Object)(object)cachedSkeletonGraphic != (Object)null)
			{
				return cachedSkeletonGraphic;
			}
			if ((Object)(object)embeddedSequencePlayer != (Object)null)
			{
				EnsureSkeletonGraphicCache(((Component)embeddedSequencePlayer).gameObject);
			}
			return cachedSkeletonGraphic;
		}
	}

	public SpineAnimSequencePlayer SequencePlayer => embeddedSequencePlayer;

	protected override bool HasDeferredLoad()
	{
		if (!base.HasDeferredLoad() && string.IsNullOrEmpty(pendingAnimationName) && (!enableSkinChange || string.IsNullOrEmpty(runtimeSkinName)))
		{
			if (wasLoadedBeforeDisable)
			{
				return (Object)(object)instance == (Object)null;
			}
			return false;
		}
		return true;
	}

	public override void SetResource(string newResourceName)
	{
		if (resourceName != newResourceName)
		{
			runtimeSkinName = null;
		}
		base.SetResource(newResourceName);
	}

	protected override void OnInstanceChanged(GameObject newInstance)
	{
		EnsureSkeletonGraphicCache(newInstance);
	}

	private void Start()
	{
		TryFlushPendingSequencePlay();
	}

	private void EnsureSkeletonGraphicCache(GameObject target)
	{
		if (!((Object)(object)target == (Object)null) && (!((Object)(object)cachedSkeletonGraphic != (Object)null) || !((Object)(object)((Component)cachedSkeletonGraphic).gameObject == (Object)(object)target)))
		{
			cachedSkeletonGraphic = target.GetComponent<SkeletonGraphic>();
			_ = (Object)(object)cachedSkeletonGraphic == (Object)null;
		}
	}

	protected override void OnInstanceReady(GameObject readyInstance)
	{
		wasLoadedBeforeDisable = true;
		EnsureSkeletonGraphicCache(readyInstance);
		ApplySkinOnReady();
		if (!TryFlushPendingSequencePlay())
		{
			if (!string.IsNullOrEmpty(pendingAnimationName))
			{
				string animName = pendingAnimationName;
				bool isLoop = pendingLoop;
				int trackIndex = pendingTrackIndex;
				Action<GameObject> onInstanceReady = pendingInstanceReady;
				Action onComplete = pendingAnimationComplete;
				ClearPending();
				InvokeInstanceReady(onInstanceReady, readyInstance);
				PlayAnimation(animName, isLoop, trackIndex, onComplete);
			}
			else if (playTiming == UIResourceLoadTiming.OnEnable)
			{
				string animName2 = ((!string.IsNullOrEmpty(animationName)) ? animationName : cachedSkeletonGraphic?.startingAnimation);
				PlayAnimation(animName2, loop, 0, null);
			}
			else if (wasLoadedBeforeDisable && !suppressRestoreUntilDisable && !string.IsNullOrEmpty(lastPlayedAnim))
			{
				ClearAnimationCompleteHandler();
				PlayAnimation(lastPlayedAnim, lastPlayedLoop, lastPlayedTrack, lastOnComplete);
			}
		}
	}

	public void SetSkin(string name)
	{
		if (enableSkinChange && !string.IsNullOrEmpty(name))
		{
			runtimeSkinName = name;
			if ((Object)(object)instance != (Object)null)
			{
				ApplySkin(name);
			}
			else if (((Behaviour)this).isActiveAndEnabled)
			{
				Load();
			}
		}
	}

	public void Play(string animName, bool isLoop, int trackIndex = 0, Action onComplete = null, Action<GameObject> onInstanceReady = null)
	{
		if (string.IsNullOrEmpty(animName))
		{
			InvokeAnimationComplete(onComplete);
		}
		else if (onComplete != null || isLoop || activeCompleteTrack == null || !(lastPlayedAnim == animName) || lastPlayedLoop)
		{
			lastPlayedAnim = animName;
			lastPlayedLoop = isLoop;
			lastPlayedTrack = trackIndex;
			lastOnComplete = onComplete;
			lastOnInstanceReady = onInstanceReady;
			suppressRestoreUntilDisable = true;
			ClearAnimationCompleteHandler();
			pendingAnimationName = animName;
			pendingLoop = isLoop;
			pendingTrackIndex = trackIndex;
			pendingInstanceReady = onInstanceReady;
			pendingAnimationComplete = onComplete;
			SetPendingPlayRequest(value: true);
			if ((Object)(object)instance != (Object)null)
			{
				Action<GameObject> onInstanceReady2 = pendingInstanceReady;
				Action onComplete2 = pendingAnimationComplete;
				ClearPending();
				InvokeInstanceReady(onInstanceReady2, instance);
				PlayAnimation(animName, isLoop, trackIndex, onComplete2);
			}
			else if (((Behaviour)this).isActiveAndEnabled)
			{
				Load();
			}
		}
	}

	public void Play(string animName, int trackIndex = 0)
	{
		Play(animName, loop, trackIndex);
	}

	public void Play(string animName, Action onComplete)
	{
		Play(animName, loop, 0, onComplete);
	}

	public void Play(string animName, Action<GameObject> onInstanceReady)
	{
		Play(animName, loop, 0, null, onInstanceReady);
	}

	public void PlaySequence(Action<GameObject> onInstanceReady = null)
	{
		if (!((Behaviour)this).isActiveAndEnabled)
		{
			pendingSequencePlay = true;
			pendingSequenceInstanceReady = onInstanceReady;
		}
		else
		{
			PlaySequenceInternal(onInstanceReady);
		}
	}

	public void StopSequence()
	{
		pendingSequencePlay = false;
		pendingSequenceInstanceReady = null;
		SpineAnimSequencePlayer obj = embeddedSequencePlayer;
		if (obj != null)
		{
			obj.Stop();
		}
	}

	private void PlayAnimation(string animName, bool isLoop, int trackIndex, Action onComplete)
	{
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		if (string.IsNullOrEmpty(animName))
		{
			InvokeAnimationComplete(onComplete);
			return;
		}
		EnsureSkeletonGraphicCache(instance);
		if ((Object)(object)cachedSkeletonGraphic == (Object)null)
		{
			InvokeAnimationComplete(onComplete);
			return;
		}
		cachedSkeletonGraphic.Initialize(false);
		Skeleton skeleton = cachedSkeletonGraphic.Skeleton;
		SkeletonData val = ((skeleton != null) ? skeleton.Data : null);
		if (val == null)
		{
			InvokeAnimationComplete(onComplete);
			return;
		}
		if (val.FindAnimation(animName) == null)
		{
			InvokeAnimationComplete(onComplete);
			return;
		}
		AnimationState animationState = cachedSkeletonGraphic.AnimationState;
		if (animationState == null)
		{
			InvokeAnimationComplete(onComplete);
			return;
		}
		animationState.TimeScale = 1f;
		if (onComplete == null && !isLoop && activeCompleteTrack != null && lastPlayedAnim == animName && !lastPlayedLoop)
		{
			return;
		}
		if (!isLoop)
		{
			animationState.ClearTracks();
			Skeleton skeleton2 = cachedSkeletonGraphic.Skeleton;
			if (skeleton2 != null)
			{
				skeleton2.SetToSetupPose();
				animationState.Apply(skeleton2);
			}
		}
		TrackEntry val2 = animationState.SetAnimation(trackIndex, animName, isLoop);
		cachedSkeletonGraphic.Update(0f);
		cachedSkeletonGraphic.UpdateMesh();
		if (val2 == null || isLoop || onComplete == null)
		{
			return;
		}
		activeCompleteTrack = val2;
		activeCompleteHandler = (TrackEntryDelegate)delegate(TrackEntry completedTrack)
		{
			if (completedTrack == activeCompleteTrack)
			{
				ClearAnimationCompleteHandler();
				if (lastOnComplete == onComplete)
				{
					lastOnComplete = null;
				}
				InvokeAnimationComplete(onComplete);
			}
		};
		val2.Complete += activeCompleteHandler;
		if (val2.IsComplete)
		{
			activeCompleteHandler.Invoke(val2);
		}
	}

	private void ApplySkinOnReady()
	{
		if (enableSkinChange && !string.IsNullOrEmpty(runtimeSkinName))
		{
			ApplySkin(runtimeSkinName);
		}
	}

	private void ApplySkin(string name)
	{
		if (!enableSkinChange || string.IsNullOrEmpty(name))
		{
			return;
		}
		EnsureSkeletonGraphicCache(instance);
		if ((Object)(object)cachedSkeletonGraphic == (Object)null)
		{
			return;
		}
		cachedSkeletonGraphic.Initialize(false);
		Skeleton skeleton = cachedSkeletonGraphic.Skeleton;
		if (skeleton == null)
		{
			return;
		}
		Skin val = skeleton.Data.FindSkin(name);
		if (val != null)
		{
			skeleton.SetSkin(val);
			skeleton.SetSlotsToSetupPose();
			AnimationState animationState = cachedSkeletonGraphic.AnimationState;
			if (animationState != null)
			{
				animationState.Apply(skeleton);
			}
			cachedSkeletonGraphic.UpdateMesh();
		}
	}

	protected override void OnInstanceUnloading(GameObject unloadingInstance)
	{
		ClearAnimationCompleteHandler();
		EnsureSkeletonGraphicCache(unloadingInstance);
		if ((Object)(object)cachedSkeletonGraphic != (Object)null)
		{
			AnimationState animationState = cachedSkeletonGraphic.AnimationState;
			if (animationState != null)
			{
				animationState.TimeScale = 1f;
			}
			if (animationState != null)
			{
				animationState.ClearTracks();
			}
			Skeleton skeleton = cachedSkeletonGraphic.Skeleton;
			if (skeleton != null)
			{
				skeleton.SetToSetupPose();
			}
		}
		cachedSkeletonGraphic = null;
	}

	protected override void OnLoadFailed()
	{
		suppressRestoreUntilDisable = false;
		ClearPending();
	}

	protected override void OnLoadCancelled(bool isDeferredUnload)
	{
		suppressRestoreUntilDisable = false;
		if (!isDeferredUnload)
		{
			wasLoadedBeforeDisable = false;
			lastPlayedAnim = null;
			lastOnComplete = null;
			lastOnInstanceReady = null;
			ClearPending();
		}
		base.OnLoadCancelled(isDeferredUnload);
	}

	private void ClearPending()
	{
		pendingAnimationName = null;
		pendingLoop = false;
		pendingTrackIndex = 0;
		pendingInstanceReady = null;
		pendingAnimationComplete = null;
		SetPendingPlayRequest(value: false);
	}

	private bool PlaySequenceInternal(Action<GameObject> onInstanceReady)
	{
		if ((Object)(object)embeddedSequencePlayer == (Object)null)
		{
			return false;
		}
		GameObject gameObject = ((Component)embeddedSequencePlayer).gameObject;
		EnsureSkeletonGraphicCache(gameObject);
		InvokeInstanceReady(onInstanceReady, gameObject);
		embeddedSequencePlayer.Play();
		return true;
	}

	private bool TryFlushPendingSequencePlay()
	{
		if (!pendingSequencePlay || !((Behaviour)this).isActiveAndEnabled)
		{
			return false;
		}
		if (!PlaySequenceInternal(pendingSequenceInstanceReady))
		{
			return false;
		}
		pendingSequencePlay = false;
		pendingSequenceInstanceReady = null;
		return true;
	}

	private void InvokeInstanceReady(Action<GameObject> onInstanceReady, GameObject readyInstance)
	{
		if (onInstanceReady == null)
		{
			return;
		}
		try
		{
			onInstanceReady(readyInstance);
		}
		catch (Exception arg)
		{
			Log.LogError(LogTag, $"Spine 实例就绪回调异常: {arg}");
		}
	}

	private void ClearAnimationCompleteHandler()
	{
		if (activeCompleteTrack != null && activeCompleteHandler != null)
		{
			activeCompleteTrack.Complete -= activeCompleteHandler;
		}
		activeCompleteTrack = null;
		activeCompleteHandler = null;
	}

	private void InvokeAnimationComplete(Action onComplete)
	{
		if (onComplete == null)
		{
			return;
		}
		try
		{
			onComplete();
		}
		catch (Exception arg)
		{
			Log.LogError(LogTag, $"Spine 动画完成回调异常: {arg}");
		}
	}

	protected override string ResolvePath(string name)
	{
		return name;
	}
}
