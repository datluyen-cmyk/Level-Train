using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class LayerArea : MonoBehaviour
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDelayedDisableAsync_003Ed__14 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float delay;

		public LayerArea _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LayerArea layerArea = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(delay), false, (PlayerLoopTiming)8, default(CancellationToken));
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CDelayedDisableAsync_003Ed__14>(ref val2, ref this);
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
				layerArea.DisableGameObject();
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

	[SerializeField]
	protected Collider2D activeCollider;

	[SerializeField]
	protected Overlap2DHandler overlap2DHandler;

	[SerializeField]
	protected SpriteRenderer visual;

	[SerializeField]
	protected List<SpriteRenderer> visualExtends;

	[SerializeField]
	protected float delaySolve;

	[SerializeField]
	protected float moveOutDuration;

	[SerializeField]
	protected int blockedLayer;

	protected List<IGameplayObject> objectsInLayer;

	protected List<IGameplayObject> objectsBlockByLayer;

	[SerializeField]
	private int solveObjectCount;

	private bool isSolved;

	public int BlockedLayer => blockedLayer;

	public void Solve()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (isSolved)
		{
			return;
		}
		isSolved = true;
		if ((Object)(object)activeCollider != (Object)null)
		{
			((Behaviour)activeCollider).enabled = false;
		}
		if ((Object)(object)visual != (Object)null)
		{
			TweenSettingsExtensions.SetDelay<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleSprite.DOFade(visual, 0f, moveOutDuration), delaySolve);
		}
		if (visualExtends != null)
		{
			visualExtends.ForEach(delegate(SpriteRenderer sr)
			{
				if ((Object)(object)sr != (Object)null)
				{
					TweenSettingsExtensions.SetDelay<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleSprite.DOFade(sr, 0f, moveOutDuration), delaySolve);
				}
			});
		}
		UniTaskVoid val = DelayedDisableAsync(delaySolve + moveOutDuration * 0.5f);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CDelayedDisableAsync_003Ed__14))]
	private UniTaskVoid DelayedDisableAsync(float delay)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CDelayedDisableAsync_003Ed__14 _003CDelayedDisableAsync_003Ed__ = default(_003CDelayedDisableAsync_003Ed__14);
		_003CDelayedDisableAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDelayedDisableAsync_003Ed__._003C_003E4__this = this;
		_003CDelayedDisableAsync_003Ed__.delay = delay;
		_003CDelayedDisableAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDelayedDisableAsync_003Ed__._003C_003Et__builder)).Start<_003CDelayedDisableAsync_003Ed__14>(ref _003CDelayedDisableAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDelayedDisableAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void DisableGameObject()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnGameplayObjectSolved(IGameplayObject gameplayObject)
	{
		if (objectsInLayer != null && objectsInLayer.Contains(gameplayObject))
		{
			solveObjectCount++;
			if (solveObjectCount >= objectsInLayer.Count)
			{
				Solve();
			}
		}
	}

	private void Start()
	{
		if (overlap2DHandler == null || (Object)(object)visual == (Object)null)
		{
			return;
		}
		Collider2D[] array = (Collider2D[])(object)new Collider2D[32];
		int overlapColliders = overlap2DHandler.GetOverlapColliders(array);
		if (overlapColliders < 1)
		{
			return;
		}
		Transform transform = ((Component)this).transform;
		for (int i = 0; i < overlapColliders && i < array.Length; i++)
		{
			Collider2D val = array[i];
			if ((Object)(object)val == (Object)null)
			{
				break;
			}
			IGameplayObject component = ((Component)val).GetComponent<IGameplayObject>();
			if (component == null)
			{
				continue;
			}
			Transform transform2 = ((Component)val).transform;
			if ((Object)(object)transform2 == (Object)null)
			{
				break;
			}
			if ((Object)(object)transform2.parent == (Object)(object)transform)
			{
				if (objectsInLayer != null)
				{
					objectsInLayer.Add(component);
				}
				component.RegisterOnSolved(OnGameplayObjectSolved);
			}
			else
			{
				if (objectsBlockByLayer != null)
				{
					objectsBlockByLayer.Add(component);
				}
				component.SetBlockedByLayer(this, isBlocked: true);
			}
		}
	}
}
