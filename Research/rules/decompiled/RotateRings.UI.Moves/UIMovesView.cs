using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.Theme;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Moves;

public class UIMovesView : GameWidgetView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CAddStepAfterDelay_003Ed__13 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float delay;

		public CancellationToken destroyCt;

		public UIMovesView _003C_003E4__this;

		public int value;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIMovesView uIMovesView = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(delay), false, (PlayerLoopTiming)8, destroyCt);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CAddStepAfterDelay_003Ed__13>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult())
				{
					uIMovesView.SetRemaining(value);
					uIMovesView.PlayAnimation("anim_Movesnormal_start");
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayTextFlySequence_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIMovesView _003C_003E4__this;

		public int targetRemaining;

		public int times;

		public Action onComplete;

		private CancellationToken _003CdestroyCt_003E5__2;

		private float _003Cduration_003E5__3;

		private int _003CstartValue_003E5__4;

		private int _003Ci_003E5__5;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIMovesView uIMovesView = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num != 0)
				{
					if (num != 1)
					{
						_003CdestroyCt_003E5__2 = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)((UIBaseView)uIMovesView).Behaviour);
						_003Cduration_003E5__3 = uIMovesView.TextFly.GetClip("anim_MovesTextfly_start").length;
						_003CstartValue_003E5__4 = targetRemaining - times;
						uIMovesView.SetRemaining(_003CstartValue_003E5__4);
						_003Ci_003E5__5 = 0;
						goto IL_015b;
					}
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_01df;
				}
				val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter<bool>);
				num = (_003C_003E1__state = -1);
				goto IL_012f;
				IL_0149:
				_003Ci_003E5__5++;
				goto IL_015b;
				IL_015b:
				UniTask val3;
				if (_003Ci_003E5__5 < times)
				{
					uIMovesView.SpawnTextFlyInstance(_003Cduration_003E5__3);
					UniTaskVoid val2 = uIMovesView.AddStepAfterDelay(_003CstartValue_003E5__4 + _003Ci_003E5__5 + 1, 0.35f, _003CdestroyCt_003E5__2);
					((UniTaskVoid)(ref val2)).Forget();
					if (_003Ci_003E5__5 < times - 1)
					{
						val3 = UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224), false, (PlayerLoopTiming)8, _003CdestroyCt_003E5__2);
						val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayTextFlySequence_003Ed__11>(ref val, ref this);
							return;
						}
						goto IL_012f;
					}
					goto IL_0149;
				}
				val3 = UniTask.Delay(TimeSpan.FromSeconds(0.3499999940395355), false, (PlayerLoopTiming)8, _003CdestroyCt_003E5__2);
				val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__1 = val;
					((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayTextFlySequence_003Ed__11>(ref val, ref this);
					return;
				}
				goto IL_01df;
				IL_012f:
				if (!val.GetResult())
				{
					goto IL_0149;
				}
				uIMovesView.InvokeOnComplete(onComplete);
				goto end_IL_000e;
				IL_01df:
				val.GetResult();
				uIMovesView.InvokeOnComplete(onComplete);
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CdestroyCt_003E5__2 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CdestroyCt_003E5__2 = default(CancellationToken);
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRecycleAfterDelay_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float delaySeconds;

		public GameObject instance;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), false, (PlayerLoopTiming)8, UniTaskCancellationExtensions.GetCancellationTokenOnDestroy(instance));
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRecycleAfterDelay_003Ed__15>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult())
				{
					GameApp.Pool.Recycle(instance);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string MoveNormalAnim = "anim_Movesnormal_start";

	private const string MoveOnly5Anim = "anim_Movesnonly5_start";

	private const string MoveTextFlyAnim = "anim_MovesTextfly_start";

	private const float TextFlyInterval = 0.2f;

	private const float TextFlyToNumberDelay = 0.35f;

	private const int LowMovesThreshold = 5;

	private static readonly Color LowMovesColor = new Color(0.9254902f, 0.19215687f, 0.16862746f, 1f);

	private int displayedRemaining = -1;

	public RectTransform Node;

	public TMP_Text TextMoveNum;

	public TMP_Text TextMove;

	public Animation TextFly;

	public void SetRemaining(int remaining)
	{
		if (remaining != displayedRemaining)
		{
			displayedRemaining = remaining;
			SetRemainingText(remaining);
		}
	}

	public void PlayMoveAnimation(int remaining)
	{
		SetRemaining(remaining);
		PlayAnimation((remaining > 5) ? "anim_Movesnormal_start" : "anim_Movesnonly5_start");
	}

	public void PlayAddStepsFlyAnimation(int times, int targetRemaining, Action onComplete = null)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (times <= 0 || (Object)(object)TextFly == (Object)null || (Object)(object)TextFly.GetClip("anim_MovesTextfly_start") == (Object)null || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			InvokeOnComplete(onComplete);
			return;
		}
		UniTaskVoid val = PlayTextFlySequence(times, targetRemaining, onComplete);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CPlayTextFlySequence_003Ed__11))]
	private UniTaskVoid PlayTextFlySequence(int times, int targetRemaining, Action onComplete)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayTextFlySequence_003Ed__11 _003CPlayTextFlySequence_003Ed__ = default(_003CPlayTextFlySequence_003Ed__11);
		_003CPlayTextFlySequence_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayTextFlySequence_003Ed__._003C_003E4__this = this;
		_003CPlayTextFlySequence_003Ed__.times = times;
		_003CPlayTextFlySequence_003Ed__.targetRemaining = targetRemaining;
		_003CPlayTextFlySequence_003Ed__.onComplete = onComplete;
		_003CPlayTextFlySequence_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayTextFlySequence_003Ed__._003C_003Et__builder)).Start<_003CPlayTextFlySequence_003Ed__11>(ref _003CPlayTextFlySequence_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayTextFlySequence_003Ed__._003C_003Et__builder)).Task;
	}

	private void InvokeOnComplete(Action callback)
	{
		if (callback == null)
		{
			return;
		}
		try
		{
			callback();
		}
		catch (Exception ex)
		{
			Log.LogError((object)this, "加步飘字序列回调执行异常: {0}", new object[1] { ex });
		}
	}

	[AsyncStateMachine(typeof(_003CAddStepAfterDelay_003Ed__13))]
	private UniTaskVoid AddStepAfterDelay(int value, float delay, CancellationToken destroyCt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CAddStepAfterDelay_003Ed__13 _003CAddStepAfterDelay_003Ed__ = default(_003CAddStepAfterDelay_003Ed__13);
		_003CAddStepAfterDelay_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CAddStepAfterDelay_003Ed__._003C_003E4__this = this;
		_003CAddStepAfterDelay_003Ed__.value = value;
		_003CAddStepAfterDelay_003Ed__.delay = delay;
		_003CAddStepAfterDelay_003Ed__.destroyCt = destroyCt;
		_003CAddStepAfterDelay_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CAddStepAfterDelay_003Ed__._003C_003Et__builder)).Start<_003CAddStepAfterDelay_003Ed__13>(ref _003CAddStepAfterDelay_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CAddStepAfterDelay_003Ed__._003C_003Et__builder)).Task;
	}

	private void SpawnTextFlyInstance(float duration)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameApp.Pool.Spawn(((Component)TextFly).gameObject, ((Component)TextFly).transform.parent);
		Animation component = val.GetComponent<Animation>();
		component.Rewind("anim_MovesTextfly_start");
		component.Play("anim_MovesTextfly_start");
		UniTaskVoid val2 = RecycleAfterDelay(val, duration);
		((UniTaskVoid)(ref val2)).Forget();
	}

	[AsyncStateMachine(typeof(_003CRecycleAfterDelay_003Ed__15))]
	private UniTaskVoid RecycleAfterDelay(GameObject instance, float delaySeconds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRecycleAfterDelay_003Ed__15 _003CRecycleAfterDelay_003Ed__ = default(_003CRecycleAfterDelay_003Ed__15);
		_003CRecycleAfterDelay_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRecycleAfterDelay_003Ed__.instance = instance;
		_003CRecycleAfterDelay_003Ed__.delaySeconds = delaySeconds;
		_003CRecycleAfterDelay_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleAfterDelay_003Ed__._003C_003Et__builder)).Start<_003CRecycleAfterDelay_003Ed__15>(ref _003CRecycleAfterDelay_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRecycleAfterDelay_003Ed__._003C_003Et__builder)).Task;
	}

	private void PlayAnimation(string animName)
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour == (Object)null))
		{
			Animation componentInChildren = ((Component)((UIBaseView)this).Behaviour).GetComponentInChildren<Animation>();
			if (!((Object)(object)componentInChildren == (Object)null) && !((Object)(object)componentInChildren.GetClip(animName) == (Object)null))
			{
				componentInChildren.Stop();
				componentInChildren.Rewind(animName);
				componentInChildren.Play(animName, (PlayMode)4);
			}
		}
	}

	private void SetRemainingText(int remaining)
	{
		if ((Object)(object)TextMoveNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TextMoveNum, remaining.ToString());
			ApplyLowMovesColor(remaining);
		}
	}

	private void ApplyLowMovesColor(int remaining)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TextMoveNum == (Object)null)
		{
			return;
		}
		if (remaining <= 5)
		{
			((Graphic)TextMoveNum).color = LowMovesColor;
			return;
		}
		ThemeTextElement component = ((Component)TextMoveNum).GetComponent<ThemeTextElement>();
		if ((Object)(object)component != (Object)null)
		{
			component.ApplyTheme(ThemeService.CurrentThemeId);
		}
	}

	public void RefreshMovesColor()
	{
		if (displayedRemaining >= 0)
		{
			ApplyLowMovesColor(displayedRemaining);
		}
	}

	public void SetupLocalizedText()
	{
		if (!((Object)(object)TextMove == (Object)null))
		{
			TMPTextExtensions.SetLocalizedText(TextMove, "Common_Moves", Array.Empty<object>());
		}
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Node = behaviour.Get<RectTransform>("Node");
		TextMoveNum = behaviour.Get<TMP_Text>("TextMoveNum");
		TextMove = behaviour.Get<TMP_Text>("TextMove");
		TextFly = behaviour.Get<Animation>("TextFly");
	}
}
