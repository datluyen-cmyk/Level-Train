using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Example.Cfg;
using Game.DataTrack;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.UI.BuySuccess;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

[UIPath("UIShop")]
public class UIShopController : GameBaseController<UIShopView>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayOpenAnimationSequence_003Ed__22 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float openAnimationStartTime;

		public CancellationToken token;

		public UIShopController _003C_003E4__this;

		private int _003CpropIndex_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private List<ShopItemView>.Enumerator _003C_003E7__wrap2;

		private ShopItemView _003Citem_003E5__4;

		private void MoveNext()
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIShopController uIShopController = _003C_003E4__this;
			try
			{
				bool flag;
				UniTask val2;
				Awaiter<bool> val;
				switch (num)
				{
				default:
				{
					float num2 = 0.1f - (Time.unscaledTime - openAnimationStartTime);
					flag = num2 > 0f;
					if (flag)
					{
						val2 = UniTask.Delay(TimeSpan.FromSeconds(num2), true, (PlayerLoopTiming)8, token);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayOpenAnimationSequence_003Ed__22>(ref val, ref this);
							return;
						}
						goto IL_00b3;
					}
					goto IL_00bc;
				}
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00b3;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_0192;
				case 2:
					break;
					IL_0192:
					if (!val.GetResult())
					{
						goto IL_01a0;
					}
					goto end_IL_000e;
					IL_01a0:
					_003CpropIndex_003E5__2 = 0;
					_003C_003E7__wrap2 = uIShopController.spawnedItems.GetEnumerator();
					break;
					IL_00b3:
					flag = val.GetResult();
					goto IL_00bc;
					IL_00bc:
					if (!flag)
					{
						bool flag2 = false;
						List<ShopItemView>.Enumerator enumerator = uIShopController.spawnedItems.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								ShopItemView current = enumerator.Current;
								if (current is ItemSpecialOfferView)
								{
									flag2 = true;
									current.PlayAppearAnim("anim_UItemSpecialOffer_appear");
								}
							}
						}
						finally
						{
							if (num < 0)
							{
								((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
							}
						}
						if (flag2)
						{
							val2 = UniTask.Delay(TimeSpan.FromSeconds(0.05000000074505806), true, (PlayerLoopTiming)8, token);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayOpenAnimationSequence_003Ed__22>(ref val, ref this);
								return;
							}
							goto IL_0192;
						}
						goto IL_01a0;
					}
					goto end_IL_000e;
				}
				try
				{
					if (num != 2)
					{
						goto IL_029f;
					}
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_026b;
					IL_026b:
					if (!val.GetResult())
					{
						goto IL_0276;
					}
					goto end_IL_000e;
					IL_029f:
					while (_003C_003E7__wrap2.MoveNext())
					{
						_003Citem_003E5__4 = _003C_003E7__wrap2.Current;
						if (!(_003Citem_003E5__4 is ItemPropView))
						{
							continue;
						}
						goto IL_01e6;
					}
					goto end_IL_01b9;
					IL_0276:
					_003Citem_003E5__4.PlayAppearAnim("anim_UItemSpecialOffer_appear");
					_003CpropIndex_003E5__2++;
					_003Citem_003E5__4 = null;
					goto IL_029f;
					IL_01e6:
					if (_003CpropIndex_003E5__2 > 0)
					{
						val2 = UniTask.Delay(TimeSpan.FromSeconds(0.05000000074505806), true, (PlayerLoopTiming)8, token);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayOpenAnimationSequence_003Ed__22>(ref val, ref this);
							return;
						}
						goto IL_026b;
					}
					goto IL_0276;
					end_IL_01b9:;
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)_003C_003E7__wrap2/*cast due to .constrained prefix*/).Dispose();
					}
				}
				_003C_003E7__wrap2 = default(List<ShopItemView>.Enumerator);
				end_IL_000e:;
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
	private struct _003CRefreshDisplayAsync_003Ed__20 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIShopController _003C_003E4__this;

		public float openAnimationStartTime;

		private int _003CcurrentGeneration_003E5__2;

		private CancellationToken _003Ctoken_003E5__3;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIShopController uIShopController = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					_003CcurrentGeneration_003E5__2 = ++uIShopController.generation;
					uIShopController.isInitializing = true;
					_003Ctoken_003E5__3 = uIShopController.cts?.Token ?? CancellationToken.None;
				}
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = UniTask.WhenAll((UniTask[])(object)new UniTask[2]
						{
							uIShopController.SpawnGroupAsync(EGiftType.SpecialOffer, (Transform)(object)(((Object)(object)((BaseController<UIShopView>)uIShopController).view.FirstAreaScrollView != (Object)null) ? ((BaseController<UIShopView>)uIShopController).view.FirstAreaScrollView.content : null), _003Ctoken_003E5__3),
							uIShopController.SpawnGroupAsync(EGiftType.Prop, ((Object)(object)((BaseController<UIShopView>)uIShopController).view.ThirdAreaGrid != (Object)null) ? ((Component)((BaseController<UIShopView>)uIShopController).view.ThirdAreaGrid).transform : null, _003Ctoken_003E5__3)
						});
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRefreshDisplayAsync_003Ed__20>(ref val2, ref this);
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
					if (!_003Ctoken_003E5__3.IsCancellationRequested && _003CcurrentGeneration_003E5__2 == uIShopController.generation)
					{
						uIShopController.SetFirstAreaVisible(HasShopItems((Transform)(object)(((Object)(object)((BaseController<UIShopView>)uIShopController).view.FirstAreaScrollView != (Object)null) ? ((BaseController<UIShopView>)uIShopController).view.FirstAreaScrollView.content : null)));
						uIShopController.spawnedItems.Sort((ShopItemView a, ShopItemView b) => a.order.CompareTo(b.order));
						((BaseController<UIShopView>)uIShopController).view.RebuildLayout();
						UniTaskVoid val3 = uIShopController.PlayOpenAnimationSequence(openAnimationStartTime, _003Ctoken_003E5__3);
						((UniTaskVoid)(ref val3)).Forget();
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception)
				{
				}
				finally
				{
					if (num < 0 && _003CcurrentGeneration_003E5__2 == uIShopController.generation)
					{
						uIShopController.isInitializing = false;
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ctoken_003E5__3 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ctoken_003E5__3 = default(CancellationToken);
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
	private struct _003CSpawnGroupAsync_003Ed__21 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public Transform parent;

		public UIShopController _003C_003E4__this;

		public EGiftType giftType;

		public CancellationToken token;

		private int _003Csequence_003E5__2;

		private List<int>.Enumerator _003C_003E7__wrap2;

		private int _003CshopItemId_003E5__4;

		private ShopConfig _003Cconfig_003E5__5;

		private Awaiter<ShopItemView> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIShopController uIShopController = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_004d;
				}
				if (!((Object)(object)parent == (Object)null) && uIShopController.shopIdsByGiftType.TryGetValue((int)giftType, out var value))
				{
					_003Csequence_003E5__2 = 0;
					_003C_003E7__wrap2 = value.GetEnumerator();
					goto IL_004d;
				}
				goto end_IL_000e;
				IL_004d:
				try
				{
					if (num != 0)
					{
						goto IL_01aa;
					}
					Awaiter<ShopItemView> val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<ShopItemView>);
					num = (_003C_003E1__state = -1);
					goto IL_0139;
					IL_0139:
					ShopItemView result = val.GetResult();
					if (!((Object)(object)result == (Object)null) && !token.IsCancellationRequested)
					{
						result.Initialize(_003CshopItemId_003E5__4);
						result.order = _003Cconfig_003E5__5.giftType * 100 + _003Csequence_003E5__2++;
						result.SetCanvasGroupAlpha(0f);
						uIShopController.spawnedItems.Add(result);
						_003Cconfig_003E5__5 = null;
					}
					goto IL_01aa;
					IL_01aa:
					while (_003C_003E7__wrap2.MoveNext())
					{
						_003CshopItemId_003E5__4 = _003C_003E7__wrap2.Current;
						if (!token.IsCancellationRequested && !((Object)(object)parent == (Object)null))
						{
							_003Cconfig_003E5__5 = GameApp.Config.GetTable<ExampleTables>()?.TbShopConfig?.GetOrDefault(_003CshopItemId_003E5__4);
							if (_003Cconfig_003E5__5 == null)
							{
								continue;
							}
							val = ((BaseController<UIShopView>)uIShopController).view.SpawnShopItemAsync((ItemObjectType)_003Cconfig_003E5__5.objectType, parent, token).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<ShopItemView>, _003CSpawnGroupAsync_003Ed__21>(ref val, ref this);
								return;
							}
							goto IL_0139;
						}
						goto end_IL_000e;
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)_003C_003E7__wrap2/*cast due to .constrained prefix*/).Dispose();
					}
				}
				_003C_003E7__wrap2 = default(List<int>.Enumerator);
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

	private const string Tag = "UIShopController";

	private const string ItemAppearAnimName = "anim_UItemSpecialOffer_appear";

	private const string ItemCloseAnimName = "anim_UItemSpecialOffer_close";

	private const float ItemsAppearDelay = 0.1f;

	private const float ItemAppearInterval = 0.05f;

	private readonly ShopItemLayoutPlanner layoutPlanner = new ShopItemLayoutPlanner();

	private readonly List<ShopItemView> spawnedItems = new List<ShopItemView>();

	private SortedDictionary<int, List<int>> shopIdsByGiftType = new SortedDictionary<int, List<int>>();

	private CancellationTokenSource cts;

	private bool isInitializing;

	private bool isClosing;

	private int closeTimerId = -1;

	private int generation;

	public UIShopController(UIShopView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		ShopService.Init();
		ShopService.OnShopOpened();
		cts = new CancellationTokenSource();
		((UnityEvent)((BaseController<UIShopView>)this).view.btnBack.onClick).AddListener(new UnityAction(OnCloseClick));
		LoginEvents();
		RefreshDisplay();
		AudioService.ActivateExperiment("variant_277508c6");
		AudioService.PlayUiSound(AudioKeys.OpenShop.Value);
		string clickType = (GameApp.UI.IsOpen("UIGame") ? "盘面-商城 " : "主页-商城");
		RRDataTrack.Instance.ReportBtnClick(clickType);
	}

	public override void OnRemove()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		PurchaseService.ForceClear();
		UnregisterEvents();
		cts?.Cancel();
		cts?.Dispose();
		cts = null;
		if (closeTimerId >= 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
		((UnityEvent)((BaseController<UIShopView>)this).view.btnBack.onClick).RemoveListener(new UnityAction(OnCloseClick));
		ClearSpawnedItems();
		base.OnRemove();
	}

	public override void OnEscape()
	{
		if (!isInitializing && !isClosing)
		{
			OnCloseClick();
		}
	}

	private void LoginEvents()
	{
		GameApp.Event.Listen<ShopBuyResultEvent>((Action<ShopBuyResultEvent>)OnShopBuyResult);
		GameApp.Event.Listen<ShopFillOrderSuccessEvent>((Action<ShopFillOrderSuccessEvent>)OnShopFillOrderSuccess);
		GameApp.Event.Listen<SkuDetailsUpdatedEvent>((Action<SkuDetailsUpdatedEvent>)OnSkuDetailsUpdated);
		GameApp.Event.Listen<PurchaseLoadingEvent>((Action<PurchaseLoadingEvent>)OnPurchaseLoadingChanged);
		GameApp.Event.Listen<PurchaseStateChangedEvent>((Action<PurchaseStateChangedEvent>)OnPurchaseStateChanged);
		GameApp.Event.Listen<TrackPurchaseEventEvent>((Action<TrackPurchaseEventEvent>)OnTrackPurchaseEvent);
	}

	private void UnregisterEvents()
	{
		GameApp.Event.Unlisten<ShopBuyResultEvent>((Action<ShopBuyResultEvent>)OnShopBuyResult);
		GameApp.Event.Unlisten<ShopFillOrderSuccessEvent>((Action<ShopFillOrderSuccessEvent>)OnShopFillOrderSuccess);
		GameApp.Event.Unlisten<SkuDetailsUpdatedEvent>((Action<SkuDetailsUpdatedEvent>)OnSkuDetailsUpdated);
		GameApp.Event.Unlisten<PurchaseLoadingEvent>((Action<PurchaseLoadingEvent>)OnPurchaseLoadingChanged);
		GameApp.Event.Unlisten<PurchaseStateChangedEvent>((Action<PurchaseStateChangedEvent>)OnPurchaseStateChanged);
		GameApp.Event.Unlisten<TrackPurchaseEventEvent>((Action<TrackPurchaseEventEvent>)OnTrackPurchaseEvent);
	}

	private void RefreshDisplay()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		cts?.Cancel();
		cts?.Dispose();
		cts = new CancellationTokenSource();
		ClearSpawnedItems();
		ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
		shopIdsByGiftType = layoutPlanner.BuildGroups(table?.TbShopConfig?.DataList);
		int contentCount = layoutPlanner.GetContentCount(shopIdsByGiftType, EGiftType.SpecialOffer);
		((BaseController<UIShopView>)this).view.RefreshCarouselView(contentCount);
		SetFirstAreaVisible(visible: false);
		float unscaledTime = Time.unscaledTime;
		((BaseController<UIShopView>)this).view.PlayOpenAnimation();
		UniTaskVoid val = RefreshDisplayAsync(unscaledTime);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CRefreshDisplayAsync_003Ed__20))]
	private UniTaskVoid RefreshDisplayAsync(float openAnimationStartTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRefreshDisplayAsync_003Ed__20 _003CRefreshDisplayAsync_003Ed__ = default(_003CRefreshDisplayAsync_003Ed__20);
		_003CRefreshDisplayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRefreshDisplayAsync_003Ed__._003C_003E4__this = this;
		_003CRefreshDisplayAsync_003Ed__.openAnimationStartTime = openAnimationStartTime;
		_003CRefreshDisplayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRefreshDisplayAsync_003Ed__._003C_003Et__builder)).Start<_003CRefreshDisplayAsync_003Ed__20>(ref _003CRefreshDisplayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRefreshDisplayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CSpawnGroupAsync_003Ed__21))]
	private UniTask SpawnGroupAsync(EGiftType giftType, Transform parent, CancellationToken token)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CSpawnGroupAsync_003Ed__21 _003CSpawnGroupAsync_003Ed__ = default(_003CSpawnGroupAsync_003Ed__21);
		_003CSpawnGroupAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CSpawnGroupAsync_003Ed__._003C_003E4__this = this;
		_003CSpawnGroupAsync_003Ed__.giftType = giftType;
		_003CSpawnGroupAsync_003Ed__.parent = parent;
		_003CSpawnGroupAsync_003Ed__.token = token;
		_003CSpawnGroupAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CSpawnGroupAsync_003Ed__._003C_003Et__builder)).Start<_003CSpawnGroupAsync_003Ed__21>(ref _003CSpawnGroupAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CSpawnGroupAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPlayOpenAnimationSequence_003Ed__22))]
	private UniTaskVoid PlayOpenAnimationSequence(float openAnimationStartTime, CancellationToken token)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayOpenAnimationSequence_003Ed__22 _003CPlayOpenAnimationSequence_003Ed__ = default(_003CPlayOpenAnimationSequence_003Ed__22);
		_003CPlayOpenAnimationSequence_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayOpenAnimationSequence_003Ed__._003C_003E4__this = this;
		_003CPlayOpenAnimationSequence_003Ed__.openAnimationStartTime = openAnimationStartTime;
		_003CPlayOpenAnimationSequence_003Ed__.token = token;
		_003CPlayOpenAnimationSequence_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayOpenAnimationSequence_003Ed__._003C_003Et__builder)).Start<_003CPlayOpenAnimationSequence_003Ed__22>(ref _003CPlayOpenAnimationSequence_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayOpenAnimationSequence_003Ed__._003C_003Et__builder)).Task;
	}

	private int GetContentCount(EGiftType giftType)
	{
		return layoutPlanner.GetContentCount(shopIdsByGiftType, giftType);
	}

	private void SetFirstAreaVisible(bool visible)
	{
		if ((Object)(object)((BaseController<UIShopView>)this).view.FirstAreaScrollView != (Object)null)
		{
			GameObject gameObject = ((Component)((BaseController<UIShopView>)this).view.FirstAreaScrollView).gameObject;
			if (gameObject.activeSelf != visible)
			{
				gameObject.SetActive(visible);
			}
		}
		if ((Object)(object)((BaseController<UIShopView>)this).view.SecondAreaVL != (Object)null)
		{
			GameObject gameObject2 = ((Component)((BaseController<UIShopView>)this).view.SecondAreaVL).gameObject;
			if (gameObject2.activeSelf != visible)
			{
				gameObject2.SetActive(visible);
			}
		}
	}

	private static bool HasShopItems(Transform content)
	{
		if ((Object)(object)content == (Object)null)
		{
			return false;
		}
		for (int i = 0; i < content.childCount; i++)
		{
			if ((Object)(object)((Component)content.GetChild(i)).GetComponentInChildren<ShopItemView>(true) != (Object)null)
			{
				return true;
			}
		}
		return false;
	}

	private void ClearSpawnedItems()
	{
		foreach (ShopItemView spawnedItem in spawnedItems)
		{
			if (!((Object)(object)spawnedItem == (Object)null))
			{
				((BaseController<UIShopView>)this).view.DisposeShopItem(spawnedItem);
			}
		}
		spawnedItems.Clear();
		((BaseController<UIShopView>)this).view.ClearScrollData();
	}

	private void OnCloseClick()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		cts?.Cancel();
		((Selectable)((BaseController<UIShopView>)this).view.btnBack).interactable = false;
		float num = ((BaseController<UIShopView>)this).view.PlayCloseAnimation();
		foreach (ShopItemView spawnedItem in spawnedItems)
		{
			spawnedItem?.PlayCloseAnim("anim_UItemSpecialOffer_close");
		}
		if (num <= 0f)
		{
			((BaseController<UIShopView>)this).CloseView(false);
			return;
		}
		closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIShopView>)this).view).Behaviour, num, (Action)delegate
		{
			closeTimerId = -1;
			((BaseController<UIShopView>)this).CloseView(false);
		});
	}

	private void OnShopBuyResult(ShopBuyResultEvent result)
	{
		PurchaseService.ForceClear();
		if (!result.IsSuccess)
		{
			((BaseController<UIShopView>)this).view.ResumeCarouselAfterExternalOverlay();
			return;
		}
		ShopBaseGoods shopBaseGoods = GameApp.Config.GetTable<ExampleTables>()?.TbShopBaseGoods?.GetOrDefault(result.BuyGoodsId);
		if (shopBaseGoods == null || shopBaseGoods.LimitedCount > 0 || result.ExclusionId > 0)
		{
			RefreshDisplay();
		}
		else
		{
			((BaseController<UIShopView>)this).view.ResumeCarouselAfterExternalOverlay();
		}
	}

	private void OnShopFillOrderSuccess(ShopFillOrderSuccessEvent result)
	{
		PurchaseService.ForceClear();
		if (result.Rewards != null && result.Rewards.Count > 0)
		{
			GameApp.UI.Open("UIBuySuccess", (UILayer)2, (object)new UIBuySuccessParam
			{
				Rewards = result.Rewards
			}, (Action)null);
		}
	}

	private void OnSkuDetailsUpdated(SkuDetailsUpdatedEvent result)
	{
		foreach (ShopItemView spawnedItem in spawnedItems)
		{
			spawnedItem?.RefreshSellPrice();
		}
	}

	private void OnPurchaseLoadingChanged(PurchaseLoadingEvent result)
	{
		if (result.IsShowing)
		{
			((BaseController<UIShopView>)this).view.PauseCarouselForExternalOverlay();
		}
		else
		{
			((BaseController<UIShopView>)this).view.ResumeCarouselAfterExternalOverlay();
		}
	}

	private void OnPurchaseStateChanged(PurchaseStateChangedEvent result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		string text = ((object)result.NewState/*cast due to .constrained prefix*/).ToString();
		switch (text)
		{
		default:
			if (!(text == "Polling"))
			{
				PurchaseService.ForceClear();
				((BaseController<UIShopView>)this).view.ResumeCarouselAfterExternalOverlay();
				break;
			}
			goto case "Creating";
		case "Creating":
		case "Paying":
		case "Delivering":
			((BaseController<UIShopView>)this).view.PauseCarouselForExternalOverlay();
			break;
		}
	}

	private void OnTrackPurchaseEvent(TrackPurchaseEventEvent result)
	{
	}
}
