using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using TMPro;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.SubView;

public class UIRankStarCollectionView : GameWidgetView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CHide_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		public UIRankStarCollectionView _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIRankStarCollectionView uIRankStarCollectionView = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string currentClose = uIRankStarCollectionView.CurrentClose;
					uIRankStarCollectionView.PlayAnimation(currentClose);
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(uIRankStarCollectionView.GetClipDuration(currentClose)), false, (PlayerLoopTiming)8, cancellationToken);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CHide_003Ed__16>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				val2.GetResult();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CShow_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		public UIRankStarCollectionView _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIRankStarCollectionView uIRankStarCollectionView = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if ((Object)(object)((UIBaseView)uIRankStarCollectionView).NodeRect != (Object)null)
					{
						((Component)((UIBaseView)uIRankStarCollectionView).NodeRect).gameObject.SetActive(true);
					}
					string currentOpen = uIRankStarCollectionView.CurrentOpen;
					uIRankStarCollectionView.PlayAnimation(currentOpen);
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(uIRankStarCollectionView.GetClipDuration(currentOpen)), false, (PlayerLoopTiming)8, cancellationToken);
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CShow_003Ed__15>(ref val2, ref this);
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

	private const string AnimOpen = "GoCollection_open";

	private const string AnimClose = "GoCollection_close";

	private const string AnimOpenRight = "anim_RankStarCollection_open_r_gp3";

	private const string AnimCloseRight = "anim_RankStarCollection_close_r_gp3";

	private const string AnimStarReceive = "anim_RankStarCollection_star_receive";

	private ParticleSystem[] feedbackParticles;

	private bool isRightSide;

	private bool sideConfigured;

	private Vector2 originalPivot;

	public Animation RankStarCollection;

	public RectTransform Node;

	public RectTransform star;

	public TMP_Text txtCollectNum;

	public GameObject Effect_GoCollection_JieShou;

	public Animation starAni;

	private string CurrentOpen
	{
		get
		{
			if (!isRightSide)
			{
				return "GoCollection_open";
			}
			return "anim_RankStarCollection_open_r_gp3";
		}
	}

	private string CurrentClose
	{
		get
		{
			if (!isRightSide)
			{
				return "GoCollection_close";
			}
			return "anim_RankStarCollection_close_r_gp3";
		}
	}

	protected override void OnWidgetFirstLoaded()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		originalPivot = ((UIBaseView)this).NodeRect.pivot;
		SetNum(0);
	}

	public bool ConfigureSide(bool right)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (sideConfigured && isRightSide == right)
		{
			return true;
		}
		if ((Object)(object)RankStarCollection == (Object)null || (Object)(object)RankStarCollection.GetClip(right ? "anim_RankStarCollection_open_r_gp3" : "GoCollection_open") == (Object)null || (Object)(object)RankStarCollection.GetClip(right ? "anim_RankStarCollection_close_r_gp3" : "GoCollection_close") == (Object)null)
		{
			return false;
		}
		StopAnimation();
		isRightSide = right;
		sideConfigured = true;
		((UIBaseView)this).NodeRect.pivot = (Vector2)(right ? new Vector2(1f, originalPivot.y) : originalPivot);
		return true;
	}

	[AsyncStateMachine(typeof(_003CShow_003Ed__15))]
	public UniTask Show(CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CShow_003Ed__15 _003CShow_003Ed__ = default(_003CShow_003Ed__15);
		_003CShow_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CShow_003Ed__._003C_003E4__this = this;
		_003CShow_003Ed__.cancellationToken = cancellationToken;
		_003CShow_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CShow_003Ed__._003C_003Et__builder)).Start<_003CShow_003Ed__15>(ref _003CShow_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CShow_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CHide_003Ed__16))]
	public UniTask Hide(CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CHide_003Ed__16 _003CHide_003Ed__ = default(_003CHide_003Ed__16);
		_003CHide_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CHide_003Ed__._003C_003E4__this = this;
		_003CHide_003Ed__.cancellationToken = cancellationToken;
		_003CHide_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CHide_003Ed__._003C_003Et__builder)).Start<_003CHide_003Ed__16>(ref _003CHide_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CHide_003Ed__._003C_003Et__builder)).Task;
	}

	public void StopHideAnimation()
	{
		StopAnimation();
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			((Component)((UIBaseView)this).NodeRect).gameObject.SetActive(true);
		}
	}

	public void ResetToHiddenImmediate()
	{
		StopAnimation();
		HideCollectFeedback();
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			((Component)((UIBaseView)this).NodeRect).gameObject.SetActive(false);
		}
	}

	public void SetNum(int num)
	{
		if ((Object)(object)txtCollectNum != (Object)null)
		{
			txtCollectNum.SetText("{0}", (float)num);
		}
	}

	public void PlayCollectFeedback()
	{
		if ((Object)(object)Effect_GoCollection_JieShou != (Object)null)
		{
			Effect_GoCollection_JieShou.SetActive(true);
			if (feedbackParticles == null)
			{
				feedbackParticles = Effect_GoCollection_JieShou.GetComponentsInChildren<ParticleSystem>(true);
			}
			for (int i = 0; i < feedbackParticles.Length; i++)
			{
				feedbackParticles[i].Simulate(0f, false, true);
				feedbackParticles[i].Play();
			}
		}
		PlayStarReceiveAnimation();
	}

	private void PlayStarReceiveAnimation()
	{
		if (!((Object)(object)starAni == (Object)null) && !((Object)(object)starAni.GetClip("anim_RankStarCollection_star_receive") == (Object)null))
		{
			((Behaviour)starAni).enabled = true;
			starAni.Stop();
			starAni.Rewind("anim_RankStarCollection_star_receive");
			starAni.Play("anim_RankStarCollection_star_receive");
		}
	}

	public void HideCollectFeedback()
	{
		if ((Object)(object)Effect_GoCollection_JieShou != (Object)null)
		{
			Effect_GoCollection_JieShou.SetActive(false);
		}
	}

	private void StopAnimation()
	{
		if ((Object)(object)RankStarCollection != (Object)null)
		{
			RankStarCollection.Stop();
		}
	}

	private void PlayAnimation(string animName)
	{
		if (!((Object)(object)RankStarCollection == (Object)null) && !((Object)(object)RankStarCollection.GetClip(animName) == (Object)null))
		{
			((Behaviour)RankStarCollection).enabled = true;
			RankStarCollection[animName].wrapMode = (WrapMode)8;
			RankStarCollection[animName].speed = 1f;
			RankStarCollection.Rewind(animName);
			RankStarCollection.Play(animName);
		}
	}

	private float GetClipDuration(string animName)
	{
		if ((Object)(object)RankStarCollection == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = RankStarCollection.GetClip(animName);
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		RankStarCollection = behaviour.Get<Animation>("RankStarCollection");
		Node = behaviour.Get<RectTransform>("Node");
		star = behaviour.Get<RectTransform>("star");
		txtCollectNum = behaviour.Get<TMP_Text>("txtCollectNum");
		Effect_GoCollection_JieShou = behaviour.Get<GameObject>("Effect_GoCollection_JieShou");
		starAni = behaviour.Get<Animation>("starAni");
	}
}
