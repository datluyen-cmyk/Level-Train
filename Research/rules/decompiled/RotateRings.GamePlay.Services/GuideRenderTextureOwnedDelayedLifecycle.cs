using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class GuideRenderTextureOwnedDelayedLifecycle : GuideRenderTextureLifecycleBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDelayedDestroyAsync_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		public GuideRenderTextureOwnedDelayedLifecycle _003C_003E4__this;

		public Camera renderCamera;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GuideRenderTextureOwnedDelayedLifecycle guideRenderTextureOwnedDelayedLifecycle = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(10.0), false, (PlayerLoopTiming)8, cancellationToken);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CDelayedDestroyAsync_003Ed__17>(ref val2, ref this);
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
					if (guideRenderTextureOwnedDelayedLifecycle.isHostInactive())
					{
						RenderTexture texture = guideRenderTextureOwnedDelayedLifecycle.texture;
						guideRenderTextureOwnedDelayedLifecycle.texture = null;
						guideRenderTextureOwnedDelayedLifecycle.DestroyOwned(renderCamera, texture);
					}
					else
					{
						RenderTexture texture2 = guideRenderTextureOwnedDelayedLifecycle.texture;
						guideRenderTextureOwnedDelayedLifecycle.texture = null;
						guideRenderTextureOwnedDelayedLifecycle.DestroyOwned(renderCamera, texture2);
					}
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

	private const string Tag = "[GamePlay][GuideRT][Owned] ";

	private const string GuideRenderTextureName = "GuideRT";

	private const float DelayedDestroySeconds = 10f;

	private readonly Func<bool> isHostInactive;

	private RenderTexture texture;

	private CancellationTokenSource delayedDestroyCts;

	public override RenderTexture Texture => texture;

	public GuideRenderTextureOwnedDelayedLifecycle(Func<bool> isHostInactive)
	{
		this.isHostInactive = isHostInactive ?? ((Func<bool>)(() => false));
	}

	public override bool CreateRT(int width, int height, Camera renderCamera)
	{
		if ((Object)(object)texture != (Object)null)
		{
			CancelDelayedDestroyTimerOnly();
			if (((Texture)texture).width == width && ((Texture)texture).height == height)
			{
				return true;
			}
			DestroyOwned(renderCamera, texture);
			texture = null;
		}
		texture = CreateOwned(width, height);
		return (Object)(object)texture != (Object)null;
	}

	public override void Release(bool scheduleDelayedDestroy, Camera renderCamera)
	{
		if (!((Object)(object)texture == (Object)null))
		{
			GuideRenderTextureLifecycleBase.DetachCameraTarget(renderCamera, texture);
			if (scheduleDelayedDestroy)
			{
				CancelDelayedDestroyTimerOnly();
				ScheduleDelayedDestroy(renderCamera);
			}
			else
			{
				CancelDelayedDestroyTimerOnly();
				DestroyOwned(renderCamera, texture);
				texture = null;
			}
		}
	}

	public override void Flush(Camera renderCamera)
	{
		CancelDelayedDestroyTimerOnly();
		if (!((Object)(object)texture == (Object)null))
		{
			DestroyOwned(renderCamera, texture);
			texture = null;
		}
	}

	public override void Dispose()
	{
		CancelDelayedDestroyTimerOnly();
		if (!((Object)(object)texture == (Object)null))
		{
			DestroyOwned(null, texture);
			texture = null;
		}
	}

	private RenderTexture CreateOwned(int width, int height)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		RenderTexture val = new RenderTexture(width, height, 24, (RenderTextureFormat)0);
		ApplyCommonSettings(val);
		if (!val.Create())
		{
			Object.Destroy((Object)(object)val);
			return null;
		}
		return val;
	}

	private void DestroyOwned(Camera renderCamera, RenderTexture rt)
	{
		if (!((Object)(object)rt == (Object)null))
		{
			GuideRenderTextureLifecycleBase.DetachCameraTarget(renderCamera, rt);
			if (rt.IsCreated())
			{
				rt.Release();
			}
			Object.Destroy((Object)(object)rt);
		}
	}

	private void ApplyCommonSettings(RenderTexture rt)
	{
		((Object)rt).name = "GuideRT";
		rt.useMipMap = false;
		rt.autoGenerateMips = false;
		rt.antiAliasing = 1;
	}

	private void ScheduleDelayedDestroy(Camera renderCamera)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		delayedDestroyCts = new CancellationTokenSource();
		UniTaskVoid val = DelayedDestroyAsync(renderCamera, delayedDestroyCts.Token);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CDelayedDestroyAsync_003Ed__17))]
	private UniTaskVoid DelayedDestroyAsync(Camera renderCamera, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CDelayedDestroyAsync_003Ed__17 _003CDelayedDestroyAsync_003Ed__ = default(_003CDelayedDestroyAsync_003Ed__17);
		_003CDelayedDestroyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDelayedDestroyAsync_003Ed__._003C_003E4__this = this;
		_003CDelayedDestroyAsync_003Ed__.renderCamera = renderCamera;
		_003CDelayedDestroyAsync_003Ed__.cancellationToken = cancellationToken;
		_003CDelayedDestroyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDelayedDestroyAsync_003Ed__._003C_003Et__builder)).Start<_003CDelayedDestroyAsync_003Ed__17>(ref _003CDelayedDestroyAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDelayedDestroyAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void CancelDelayedDestroyTimerOnly()
	{
		if (delayedDestroyCts != null)
		{
			delayedDestroyCts.Cancel();
			delayedDestroyCts.Dispose();
			delayedDestroyCts = null;
		}
	}
}
