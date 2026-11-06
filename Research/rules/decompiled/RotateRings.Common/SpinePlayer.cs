using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Spine;
using Spine.Unity;
using UniLog;
using UnityEngine;

namespace RotateRings.Common;

public sealed class SpinePlayer : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass8_0
	{
		public TrackEntry trackEntry;

		public SpinePlayer _003C_003E4__this;

		public UniTaskCompletionSource completionSource;

		public CancellationToken cancellationToken;

		internal void _003CPlayOnceAsync_003Eb__0(TrackEntry _)
		{
			if (trackEntry == _003C_003E4__this.activeTrackEntry)
			{
				_003C_003E4__this.ClearCompletionHandler();
				completionSource.TrySetResult();
			}
		}

		internal void _003CPlayOnceAsync_003Eb__1()
		{
			_003C_003E4__this.ClearCompletionHandler();
			completionSource.TrySetCanceled(cancellationToken);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayOnceAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public SpinePlayer _003C_003E4__this;

		public CancellationToken cancellationToken;

		public string animationName;

		private CancellationTokenRegistration _003C_003E7__wrap1;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SpinePlayer spinePlayer = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_00cd;
				}
				_003C_003Ec__DisplayClass8_0 _003C_003Ec__DisplayClass8_ = new _003C_003Ec__DisplayClass8_0();
				_003C_003Ec__DisplayClass8_._003C_003E4__this = _003C_003E4__this;
				_003C_003Ec__DisplayClass8_.cancellationToken = cancellationToken;
				_003C_003Ec__DisplayClass8_.cancellationToken.ThrowIfCancellationRequested();
				if (!((Component)spinePlayer).gameObject.activeSelf)
				{
					((Component)spinePlayer).gameObject.SetActive(true);
				}
				_003C_003Ec__DisplayClass8_.trackEntry = spinePlayer.Play(animationName, loop: false);
				if (_003C_003Ec__DisplayClass8_.trackEntry != null)
				{
					_003C_003Ec__DisplayClass8_.completionSource = new UniTaskCompletionSource();
					TrackEntryDelegate val = null;
					val = new TrackEntryDelegate(_003C_003Ec__DisplayClass8_._003CPlayOnceAsync_003Eb__0);
					spinePlayer.activeTrackEntry = _003C_003Ec__DisplayClass8_.trackEntry;
					spinePlayer.activeCompleteHandler = val;
					_003C_003Ec__DisplayClass8_.trackEntry.Complete += val;
					_003C_003E7__wrap1 = _003C_003Ec__DisplayClass8_.cancellationToken.Register(_003C_003Ec__DisplayClass8_._003CPlayOnceAsync_003Eb__1);
					goto IL_00cd;
				}
				goto end_IL_000e;
				IL_00cd:
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask task = _003C_003Ec__DisplayClass8_.completionSource.Task;
						val2 = ((UniTask)(ref task)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayOnceAsync_003Ed__8>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
					}
					((Awaiter)(ref val2)).GetResult();
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)_003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
					}
				}
				_003C_003E7__wrap1 = default(CancellationTokenRegistration);
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string Tag = "[RotateRing][SpinePlayer] ";

	[SerializeField]
	private SkeletonGraphic skeletonGraphic;

	private TrackEntry activeTrackEntry;

	private TrackEntryDelegate activeCompleteHandler;

	public SkeletonGraphic SkeletonGraphic => skeletonGraphic;

	private void Awake()
	{
		if ((Object)(object)skeletonGraphic == (Object)null)
		{
			skeletonGraphic = ((Component)this).GetComponentInChildren<SkeletonGraphic>(true);
		}
		if ((Object)(object)skeletonGraphic != (Object)null)
		{
			try
			{
				if (skeletonGraphic.AnimationState != null)
				{
					skeletonGraphic.AnimationState.ClearTrack(0);
				}
				skeletonGraphic.startingAnimation = string.Empty;
			}
			catch (Exception)
			{
			}
		}
		if ((Object)(object)skeletonGraphic == (Object)null)
		{
			Log.LogError("[RotateRing][SpinePlayer] ", "未找到 SkeletonGraphic");
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public TrackEntry Play(string animationName, bool loop)
	{
		ClearCompletionHandler();
		if ((Object)(object)skeletonGraphic == (Object)null || skeletonGraphic.AnimationState == null)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(animationName) && !((Object)(object)skeletonGraphic.SkeletonDataAsset == (Object)null))
		{
			SkeletonData skeletonData = skeletonGraphic.SkeletonDataAsset.GetSkeletonData(false);
			if (((skeletonData != null) ? skeletonData.FindAnimation(animationName) : null) != null)
			{
				return skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
			}
		}
		return null;
	}

	[AsyncStateMachine(typeof(_003CPlayOnceAsync_003Ed__8))]
	public UniTask PlayOnceAsync(string animationName, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayOnceAsync_003Ed__8 _003CPlayOnceAsync_003Ed__ = default(_003CPlayOnceAsync_003Ed__8);
		_003CPlayOnceAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayOnceAsync_003Ed__._003C_003E4__this = this;
		_003CPlayOnceAsync_003Ed__.animationName = animationName;
		_003CPlayOnceAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPlayOnceAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayOnceAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayOnceAsync_003Ed__8>(ref _003CPlayOnceAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayOnceAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void Stop()
	{
		ClearCompletionHandler();
		if ((Object)(object)skeletonGraphic != (Object)null && skeletonGraphic.AnimationState != null)
		{
			skeletonGraphic.AnimationState.ClearTrack(0);
		}
	}

	private void ClearCompletionHandler()
	{
		if (activeTrackEntry != null && activeCompleteHandler != null)
		{
			activeTrackEntry.Complete -= activeCompleteHandler;
		}
		activeTrackEntry = null;
		activeCompleteHandler = null;
	}
}
