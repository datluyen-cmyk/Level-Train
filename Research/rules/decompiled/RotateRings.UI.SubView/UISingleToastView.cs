using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.UI.Toast;
using TMPro;
using UnityEngine;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.SubView;

public class UISingleToastView : GameWidgetView, IToastWidgetView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CHide_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UISingleToastView _003C_003E4__this;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UISingleToastView uISingleToastView = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00b1;
				}
				string clipName = "anim_SingleToast_close";
				AnimationClip val2 = uISingleToastView.ResolveCloseClip(ref clipName);
				if (!((Object)(object)uISingleToastView.ani == (Object)null) && !((Object)(object)val2 == (Object)null))
				{
					uISingleToastView.PlayAnimation(clipName);
					UniTask val3 = UniTask.Delay(TimeSpan.FromSeconds(val2.length), false, (PlayerLoopTiming)8, cancellationToken);
					val = ((UniTask)(ref val3)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CHide_003Ed__6>(ref val, ref this);
						return;
					}
					goto IL_00b1;
				}
				goto end_IL_000e;
				IL_00b1:
				((Awaiter)(ref val)).GetResult();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CShow_003Ed__5 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UISingleToastView _003C_003E4__this;

		public CancellationToken cancellationToken;

		public string content;

		public float duration;

		private CancellationToken _003Ctoken_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UISingleToastView uISingleToastView = _003C_003E4__this;
			try
			{
				Awaiter val;
				UniTask val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_016d;
					}
					uISingleToastView.CancelShow();
					uISingleToastView.showFlowCts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { cancellationToken });
					_003Ctoken_003E5__2 = uISingleToastView.showFlowCts.Token;
					uISingleToastView.SetContent(content);
					if ((Object)(object)uISingleToastView.Node != (Object)null)
					{
						uISingleToastView.Node.SetActive(true);
					}
					uISingleToastView.PlayAnimation("anim_SingleToast_open");
					float num2 = Mathf.Max(0f, duration);
					if (!(num2 > 0f))
					{
						goto IL_0107;
					}
					val2 = UniTask.Delay(TimeSpan.FromSeconds(num2), false, (PlayerLoopTiming)8, _003Ctoken_003E5__2);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CShow_003Ed__5>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val)).GetResult();
				goto IL_0107;
				IL_0107:
				_003Ctoken_003E5__2.ThrowIfCancellationRequested();
				val2 = uISingleToastView.Hide(_003Ctoken_003E5__2);
				val = ((UniTask)(ref val2)).GetAwaiter();
				if (!((Awaiter)(ref val)).IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__1 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CShow_003Ed__5>(ref val, ref this);
					return;
				}
				goto IL_016d;
				IL_016d:
				((Awaiter)(ref val)).GetResult();
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ctoken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ctoken_003E5__2 = default(CancellationToken);
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

	private const string OpenAnim = "anim_SingleToast_open";

	private const string CloseAnim = "anim_SingleToast_close";

	private CancellationTokenSource showFlowCts;

	public GameObject Node;

	public TMP_Text txtTip;

	public Animation ani;

	public void SetContent(string content)
	{
		if ((Object)(object)txtTip != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtTip, content ?? string.Empty);
		}
	}

	public void RefreshTextMesh()
	{
		if (!((Object)(object)txtTip == (Object)null))
		{
			string text = txtTip.text;
			TMPTextExtensions.SetLocalizedTextValue(txtTip, text);
			txtTip.ForceMeshUpdate(true, true);
		}
	}

	[AsyncStateMachine(typeof(_003CShow_003Ed__5))]
	public UniTask Show(string content, float duration, CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CShow_003Ed__5 _003CShow_003Ed__ = default(_003CShow_003Ed__5);
		_003CShow_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CShow_003Ed__._003C_003E4__this = this;
		_003CShow_003Ed__.content = content;
		_003CShow_003Ed__.duration = duration;
		_003CShow_003Ed__.cancellationToken = cancellationToken;
		_003CShow_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CShow_003Ed__._003C_003Et__builder)).Start<_003CShow_003Ed__5>(ref _003CShow_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CShow_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CHide_003Ed__6))]
	public UniTask Hide(CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CHide_003Ed__6 _003CHide_003Ed__ = default(_003CHide_003Ed__6);
		_003CHide_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CHide_003Ed__._003C_003E4__this = this;
		_003CHide_003Ed__.cancellationToken = cancellationToken;
		_003CHide_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CHide_003Ed__._003C_003Et__builder)).Start<_003CHide_003Ed__6>(ref _003CHide_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CHide_003Ed__._003C_003Et__builder)).Task;
	}

	public void CancelShow()
	{
		if (showFlowCts != null)
		{
			showFlowCts.Cancel();
			showFlowCts.Dispose();
			showFlowCts = null;
		}
	}

	private void PlayAnimation(string animName)
	{
		if (!((Object)(object)ani == (Object)null) && !((Object)(object)ani.GetClip(animName) == (Object)null))
		{
			ani.Stop();
			ani.Rewind(animName);
			ani.Play(animName, (PlayMode)4);
		}
	}

	private AnimationClip ResolveCloseClip(ref string clipName)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		if ((Object)(object)ani == (Object)null)
		{
			return null;
		}
		AnimationClip clip = ani.GetClip(clipName);
		if ((Object)(object)clip != (Object)null)
		{
			return clip;
		}
		foreach (AnimationState item in ani)
		{
			AnimationState val = item;
			if (!((Object)(object)val.clip == (Object)null) && !(((Object)val.clip).name == "anim_SingleToast_open"))
			{
				clipName = ((Object)val.clip).name;
				return val.clip;
			}
		}
		return null;
	}

	public override void Dispose()
	{
		CancelShow();
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Node = behaviour.Get<GameObject>("Node");
		txtTip = behaviour.Get<TMP_Text>("txtTip");
		ani = behaviour.Get<Animation>("ani");
	}
}
