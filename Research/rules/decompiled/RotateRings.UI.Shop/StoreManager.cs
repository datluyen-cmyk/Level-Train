using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Example.Cfg;
using RotateRings.Model;
using RotateRings.UI.ConnectionLost;
using UniLog;
using UnityEngine;
using XingYun.Aot;
using XingYun.Core.Singletons;
using XingYun.IAP.Core;
using XingYun.IAP.SDK;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.Shop;

public sealed class StoreManager : Singleton<StoreManager>, IStoreManager
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBuyShop_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public StoreManager _003C_003E4__this;

		public int shopItemId;

		public int count;

		public string linkUniqueId;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			StoreManager storeManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00b8;
				}
				if (Singleton<IAPManager>.Instance.CanStartNewPurchase())
				{
					if (storeManager.HasNetwork)
					{
						UniTask val2 = Singleton<IAPManager>.Instance.BuyProduct(shopItemId, count, false, default(CancellationToken), linkUniqueId, (IAPProductType)0);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CBuyShop_003Ed__41>(ref val, ref this);
							return;
						}
						goto IL_00b8;
					}
					storeManager.ShowPaymentErrorTips((XingYunIAPErrorCode)2001, "No network connection");
				}
				goto end_IL_000e;
				IL_00b8:
				((Awaiter)(ref val)).GetResult();
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
	private struct _003CBuyVIPShop_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public IAPProductType productType;

		public StoreManager _003C_003E4__this;

		public int shopItemId;

		public string linkUniqueId;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			StoreManager storeManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00c4;
				}
				if ((int)productType != 0 && Singleton<IAPManager>.Instance.CanStartNewPurchase())
				{
					if (storeManager.HasNetwork)
					{
						UniTask val2 = Singleton<IAPManager>.Instance.BuyVIPProduct(shopItemId, productType, false, default(CancellationToken), linkUniqueId);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CBuyVIPShop_003Ed__42>(ref val, ref this);
							return;
						}
						goto IL_00c4;
					}
					storeManager.ShowPaymentErrorTips((XingYunIAPErrorCode)2001, "No network connection");
				}
				goto end_IL_000e;
				IL_00c4:
				((Awaiter)(ref val)).GetResult();
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
	private struct _003CQueryVIPStatus_003Ed__28 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public StoreManager _003C_003E4__this;

		public IAPProductType productType;

		private Awaiter<XingYunVIPStatusResponse> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			StoreManager storeManager = _003C_003E4__this;
			try
			{
				Awaiter<XingYunVIPStatusResponse> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<XingYunVIPStatusResponse>);
					num = (_003C_003E1__state = -1);
					goto IL_007e;
				}
				if (storeManager.HasNetwork)
				{
					val = Singleton<IAPManager>.Instance.QueryVIPStatusAsync(productType, 10).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<XingYunVIPStatusResponse>, _003CQueryVIPStatus_003Ed__28>(ref val, ref this);
						return;
					}
					goto IL_007e;
				}
				goto end_IL_000e;
				IL_007e:
				val.GetResult();
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
	private struct _003CRestoreVIPEntitlement_003Ed__43 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public StoreManager _003C_003E4__this;

		public IAPProductType productType;

		private Awaiter<XingYunVIPStatusResponse> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			StoreManager storeManager = _003C_003E4__this;
			try
			{
				Awaiter<XingYunVIPStatusResponse> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<XingYunVIPStatusResponse>);
					num = (_003C_003E1__state = -1);
					goto IL_008e;
				}
				if (storeManager.HasNetwork)
				{
					val = Singleton<IAPManager>.Instance.RestoreVIPEntitlementAsync(productType, 15).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<XingYunVIPStatusResponse>, _003CRestoreVIPEntitlement_003Ed__43>(ref val, ref this);
						return;
					}
					goto IL_008e;
				}
				storeManager.ShowPaymentErrorTips((XingYunIAPErrorCode)2001, "No network connection");
				goto end_IL_000e;
				IL_008e:
				val.GetResult();
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
	private struct _003CStartNetworkMonitoring_003Ed__48 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public StoreManager _003C_003E4__this;

		private CancellationToken _003Ctoken_003E5__2;

		private Awaiter _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			StoreManager storeManager = _003C_003E4__this;
			try
			{
				if ((uint)num > 1u)
				{
					_003Ctoken_003E5__2 = storeManager.monitoringCts.Token;
				}
				try
				{
					Awaiter<bool> val;
					if (num != 0)
					{
						if (num != 1)
						{
							goto IL_0115;
						}
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_010a;
					}
					Awaiter val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0094;
					IL_010a:
					if (!val.GetResult())
					{
						goto IL_0115;
					}
					goto end_IL_0024;
					IL_0115:
					UniTask val3;
					if (!_003Ctoken_003E5__2.IsCancellationRequested)
					{
						val3 = Singleton<IAPManager>.Instance.NetworkMonitoringHandle(_003Ctoken_003E5__2);
						val2 = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CStartNetworkMonitoring_003Ed__48>(ref val2, ref this);
							return;
						}
						goto IL_0094;
					}
					goto end_IL_0024;
					IL_0094:
					((Awaiter)(ref val2)).GetResult();
					val3 = UniTask.Delay(2000, false, (PlayerLoopTiming)8, _003Ctoken_003E5__2);
					val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CStartNetworkMonitoring_003Ed__48>(ref val, ref this);
						return;
					}
					goto IL_010a;
					end_IL_0024:;
				}
				catch (Exception)
				{
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ctoken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ctoken_003E5__2 = default(CancellationToken);
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

	private const string TAG = "StoreManager";

	private const int NetworkCheckIntervalMilliseconds = 2000;

	private readonly Dictionary<string, int> skuToShopItemIdMap = new Dictionary<string, int>();

	private readonly HashSet<string> handlingErrorOrders = new HashSet<string>();

	private CancellationTokenSource monitoringCts;

	private bool isInitialized;

	public bool HasNetwork => (int)Application.internetReachability > 0;

	public void Init()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialized)
		{
			BuildSkuIdReverseMap();
			Singleton<IAPManager>.Instance.Init(new IAPSupport(true), (IStoreManager)(object)this);
			ConfigureNonConsumableSkus();
			UniTaskExtensions.Forget(Singleton<IAPManager>.Instance.FetchVIPSkuDetails((IAPProductType)2, default(CancellationToken)));
			monitoringCts = new CancellationTokenSource();
			UniTaskVoid val = StartNetworkMonitoring();
			((UniTaskVoid)(ref val)).Forget();
			isInitialized = true;
		}
	}

	public void RefreshSkuConfiguration()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		BuildSkuIdReverseMap();
		Singleton<IAPManager>.Instance.ClearSkuCache();
		UniTaskExtensions.Forget(Singleton<IAPManager>.Instance.FetchCurrentStoreSkus((IAPProductType)0, default(CancellationToken)));
		UniTaskExtensions.Forget(Singleton<IAPManager>.Instance.FetchVIPSkuDetails((IAPProductType)2, default(CancellationToken)));
	}

	public void Cleanup()
	{
		monitoringCts?.Cancel();
		monitoringCts = null;
		Singleton<IAPManager>.Instance.ForceSaveOrders();
		isInitialized = false;
	}

	public PurchaseOrder CreateOrder(int shopItemId, int count, string linkUniqueId = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		PurchaseOrder val = new PurchaseOrder(shopItemId, count)
		{
			linkUniqueId = linkUniqueId
		};
		Singleton<IAPManager>.Instance.AddPendingOrder(val);
		Singleton<IAPManager>.Instance.ForceSaveOrders();
		return val;
	}

	public PurchaseOrder CreateOrderFromSku(string skuId, int count)
	{
		int shopItemIdFromSkuId = GetShopItemIdFromSkuId(skuId);
		if (shopItemIdFromSkuId > 0)
		{
			return CreateOrder(shopItemIdFromSkuId, count);
		}
		return null;
	}

	public bool DeliverGoods(int shopItemId, int count, PurchaseOrder order)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		PurchaseResultParam purchaseResultParam = new PurchaseResultParam
		{
			Order = order,
			ProductType = (IAPProductType)0
		};
		try
		{
			GameApp.Command.Execute<PurchaseResultCommand>((object)purchaseResultParam);
		}
		catch (Exception ex)
		{
			Log.LogError("StoreManager", "发货处理异常，shopItemId={0} error={1}", new object[2] { shopItemId, ex.Message });
		}
		return purchaseResultParam.IsDelivered;
	}

	public void OnPurchaseSuccess(PurchaseOrder order)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (order == null)
		{
			return;
		}
		RefreshCurrentStoreSkus();
		if (order.productType == 2)
		{
			PurchaseResultParam purchaseResultParam = new PurchaseResultParam
			{
				Order = order,
				ProductType = (IAPProductType)2
			};
			GameApp.Command.Execute<PurchaseResultCommand>((object)purchaseResultParam);
			if (!purchaseResultParam.IsDelivered)
			{
				Log.LogError("StoreManager", "非消耗品订单权益同步失败，orderId={0}", new object[1] { order.orderId });
			}
		}
	}

	public void OnPurchaseStateChanged(PurchaseState newState, PurchaseState oldState, string orderId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		GameApp.Event.Send<PurchaseStateChangedEvent>(new PurchaseStateChangedEvent
		{
			NewState = newState,
			OldState = oldState,
			OrderId = orderId
		});
	}

	public void OnFillOrderSuccess(List<PurchaseOrder> orders)
	{
		if (orders == null || orders.Count == 0)
		{
			return;
		}
		List<ShopRewardItem> list = new List<ShopRewardItem>();
		foreach (PurchaseOrder order in orders)
		{
			ShopBaseGoods storeItemConfig = GetStoreItemConfig(order.shopItemId);
			if (storeItemConfig?.BuyItems == null)
			{
				continue;
			}
			foreach (KeyValuePair<int, int> buyItem in storeItemConfig.BuyItems)
			{
				list.Add(new ShopRewardItem
				{
					PropId = buyItem.Key,
					Count = buyItem.Value
				});
			}
		}
		if (list.Count > 0)
		{
			GameApp.Event.Send<ShopFillOrderSuccessEvent>(new ShopFillOrderSuccessEvent
			{
				Rewards = list
			});
		}
	}

	public void HandleUncompletedOrder(XingYunUncompletedOrderItem orderItem, int shopItemId, Func<XingYunUncompletedOrderItem, bool> onCompleted)
	{
		GameApp.Command.Execute<BuyReorderCommand>((object)new BuyReorderCommand.BuyReorderParams
		{
			ShopItemId = shopItemId,
			OnConfirm = delegate
			{
				onCompleted?.Invoke(orderItem);
			}
		});
	}

	public string GetSkuIdFromShopItemId(int shopItemId)
	{
		return ShopService.GetSkuId(GetStoreItemConfig(shopItemId));
	}

	public int GetShopItemIdFromSkuId(string skuId)
	{
		if (!string.IsNullOrEmpty(skuId))
		{
			return skuToShopItemIdMap.GetValueOrDefault(skuId);
		}
		return 0;
	}

	public List<string> GetAllStoreSkuIds()
	{
		return new List<string>(skuToShopItemIdMap.Keys);
	}

	public List<string> GetStoreSkuIds(IAPProductType productType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)productType == 0)
		{
			return GetAllStoreSkuIds();
		}
		return new List<string>();
	}

	public void OnSkuDetailsUpdated(List<XingYunSkuDetailItem> skuDetails)
	{
		ShopService.SetSkuDetailItem(skuDetails.ToDictionary((XingYunSkuDetailItem x) => x.skuId, (XingYunSkuDetailItem x) => x));
		GameApp.Event.Send<SkuDetailsUpdatedEvent>(new SkuDetailsUpdatedEvent
		{
			SkuDetails = skuDetails
		});
	}

	public void HandlePaymentNetworkError(string orderId, Action onFailure)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (handlingErrorOrders.Add(orderId))
		{
			GameApp.UI.Open("UIConnectionLost", (UILayer)2, (object)new ConnectionLostParam
			{
				ErrorCode = (XingYunIAPErrorCode)2001,
				OnConfirm = delegate
				{
					handlingErrorOrders.Remove(orderId);
				},
				OnCancel = delegate
				{
					handlingErrorOrders.Remove(orderId);
					onFailure?.Invoke();
				}
			}, (Action)null);
		}
	}

	public void ShowPaymentErrorTips(XingYunIAPErrorCode errorCode, string errorMessage)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		GameApp.UI.Open("UIConnectionLost", (UILayer)2, (object)errorCode, (Action)null);
		PurchaseService.Hide("ShopPurchase");
	}

	public void ShowPaymentSuccessUI(string orderId, bool showSucPanel)
	{
	}

	public void OnBuyShopFinished()
	{
		PurchaseService.Hide("ShopPurchase");
	}

	public void OnVIPStatusUpdated(IAPProductType productType, XingYunVIPStatusResponse status)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (status != null)
		{
			GameApp.Event.Send<ShopVipStatusUpdatedEvent>(new ShopVipStatusUpdatedEvent
			{
				ProductType = productType,
				Status = status
			});
		}
	}

	[AsyncStateMachine(typeof(_003CQueryVIPStatus_003Ed__28))]
	public UniTaskVoid QueryVIPStatus(IAPProductType productType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CQueryVIPStatus_003Ed__28 _003CQueryVIPStatus_003Ed__ = default(_003CQueryVIPStatus_003Ed__28);
		_003CQueryVIPStatus_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CQueryVIPStatus_003Ed__._003C_003E4__this = this;
		_003CQueryVIPStatus_003Ed__.productType = productType;
		_003CQueryVIPStatus_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CQueryVIPStatus_003Ed__._003C_003Et__builder)).Start<_003CQueryVIPStatus_003Ed__28>(ref _003CQueryVIPStatus_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CQueryVIPStatus_003Ed__._003C_003Et__builder)).Task;
	}

	public XingYunVIPStatusResponse GetVIPStatusSync(IAPProductType productType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return Singleton<IAPManager>.Instance.GetCachedVIPStatus(productType) ?? Singleton<IAPManager>.Instance.GetVIPStatusSync(productType);
	}

	public bool IsVIP(IAPProductType productType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return Singleton<IAPManager>.Instance.IsVIP(productType);
	}

	public void OpenAppStoreSubManagePage()
	{
		Singleton<IAPManager>.Instance.OpenAppStoreSubManagePage();
	}

	public void OnRestoreVIPCompleted(IAPProductType productType, XingYunVIPStatusResponse status)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		ShopVipRestoreCompletedEvent shopVipRestoreCompletedEvent = new ShopVipRestoreCompletedEvent
		{
			ProductType = productType,
			Status = status
		};
		GameApp.Event.Send<ShopVipRestoreCompletedEvent>(shopVipRestoreCompletedEvent);
		if ((int)productType == 2)
		{
			GameApp.Command.Execute<RestorePurchasesResultCommand>((object)shopVipRestoreCompletedEvent);
		}
	}

	public void TrackCreateOrder(PurchaseOrder order, string skuId)
	{
		ShopBaseGoods storeItemConfig = GetStoreItemConfig(order.shopItemId);
		string skuType = ((storeItemConfig != null) ? storeItemConfig.ID.ToString() : "0");
		string label = "";
		ShopConfig orDefault = GameApp.Config.GetTable<ExampleTables>().TbShopConfig.GetOrDefault(order.shopItemId);
		if (orDefault != null)
		{
			label = $"popular {orDefault.tag},discount {orDefault.discount}";
		}
		XingYunSkuDetailItem cachedSkuInfo = Singleton<IAPManager>.Instance.GetCachedSkuInfo(order.shopItemId);
		string price = ((cachedSkuInfo != null) ? cachedSkuInfo.formattedPrice : "N/A");
		string currencyCode = ((cachedSkuInfo != null) ? cachedSkuInfo.priceCurrencyCode : "USD");
		Singleton<IAPTrackerProvider>.Instance.TrackGameIapCreateOrder(skuId, skuType, label, price, currencyCode, order.orderId, order.shopItemId, order.linkUniqueId);
	}

	public void TrackDeliverySuccess(PurchaseOrder order)
	{
		try
		{
			ShopBaseGoods storeItemConfig = GetStoreItemConfig(order.shopItemId);
			if (storeItemConfig != null)
			{
				string platformStoreId = GetPlatformStoreId(storeItemConfig);
				string skuType = storeItemConfig.ID.ToString();
				string label = "";
				ShopConfig orDefault = GameApp.Config.GetTable<ExampleTables>().TbShopConfig.GetOrDefault(order.shopItemId);
				if (orDefault != null)
				{
					label = $"popular {orDefault.tag},discount {orDefault.discount}";
				}
				double price = 0.0;
				XingYunSkuDetailItem cachedSkuInfo = Singleton<IAPManager>.Instance.GetCachedSkuInfo(order.shopItemId);
				if (cachedSkuInfo != null && !string.IsNullOrEmpty(cachedSkuInfo.formattedPrice))
				{
					price = ParsePriceFromSku(cachedSkuInfo.formattedPrice);
				}
				Singleton<IAPTrackerProvider>.Instance.TrackIapPurchase(platformStoreId, skuType, "purchase_dispatched", label, price, order.orderId, order.paymentTransactionId, order.count, order.shopItemId, order.linkUniqueId);
				GameApp.Event.Send<TrackPurchaseEventEvent>(new TrackPurchaseEventEvent
				{
					GoodsId = order.shopItemId,
					OrderId = order.orderId,
					PaymentTransactionId = order.paymentTransactionId,
					Result = "purchase_dispatched"
				});
			}
		}
		catch (Exception)
		{
		}
	}

	public void TrackSkuLoadSuccess(int shopItemId, XingYunSkuDetailItem skuInfo)
	{
		try
		{
			ShopBaseGoods storeItemConfig = GetStoreItemConfig(shopItemId);
			if (storeItemConfig != null)
			{
				string platformStoreId = GetPlatformStoreId(storeItemConfig);
				string skuType = storeItemConfig.ID.ToString();
				Singleton<IAPTrackerProvider>.Instance.TrackIapPageLoad("success", platformStoreId, skuType);
			}
		}
		catch (Exception)
		{
		}
	}

	public void TrackPurchaseEvent(string gameOrderId, string result)
	{
		try
		{
			PurchaseOrder val = Singleton<IAPManager>.Instance.FindOrderById(gameOrderId);
			if (val == null)
			{
				return;
			}
			ShopBaseGoods storeItemConfig = GetStoreItemConfig(val.shopItemId);
			if (storeItemConfig != null)
			{
				string platformStoreId = GetPlatformStoreId(storeItemConfig);
				string skuType = storeItemConfig.ID.ToString();
				string label = "";
				ShopConfig orDefault = GameApp.Config.GetTable<ExampleTables>().TbShopConfig.GetOrDefault(val.shopItemId);
				if (orDefault != null)
				{
					label = $"popular {orDefault.tag},discount {orDefault.discount}";
				}
				double price = 0.0;
				XingYunSkuDetailItem cachedSkuInfo = Singleton<IAPManager>.Instance.GetCachedSkuInfo(val.shopItemId);
				if (cachedSkuInfo != null && !string.IsNullOrEmpty(cachedSkuInfo.formattedPrice))
				{
					price = ParsePriceFromSku(cachedSkuInfo.formattedPrice);
				}
				Singleton<IAPTrackerProvider>.Instance.TrackIapPurchase(platformStoreId, skuType, result, label, price, gameOrderId, val.orderId, val.count, val.shopItemId, val.linkUniqueId);
				GameApp.Event.Send<TrackPurchaseEventEvent>(new TrackPurchaseEventEvent
				{
					GoodsId = val.shopItemId,
					OrderId = val.orderId,
					PaymentTransactionId = val.paymentTransactionId,
					Result = result
				});
			}
		}
		catch (Exception)
		{
		}
	}

	public void TrackUserPaid(PurchaseOrder order)
	{
	}

	public void TrackSkuLoadFailed(string skus, string status)
	{
		try
		{
			Singleton<IAPTrackerProvider>.Instance.TrackIapPageLoad(status, skus, "all");
		}
		catch (Exception)
		{
		}
	}

	public double ParsePriceFromSku(string formattedPrice)
	{
		if (string.IsNullOrEmpty(formattedPrice))
		{
			return 0.0;
		}
		double result = default(double);
		if (Parser.TryParse(Regex.Replace(formattedPrice, "[^\\d\\.]", ""), ref result))
		{
			return result;
		}
		return 0.0;
	}

	public void SetIsHistory_paid_user(bool isHistoryPaidUser)
	{
		PlayerModelData orCreate = GameApp.Save.GetOrCreate<PlayerModelData>();
		if (orCreate != null)
		{
			orCreate.IsPaidUser = isHistoryPaidUser;
		}
	}

	[AsyncStateMachine(typeof(_003CBuyShop_003Ed__41))]
	public UniTaskVoid BuyShop(int shopItemId, int count, string linkUniqueId = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CBuyShop_003Ed__41 _003CBuyShop_003Ed__ = default(_003CBuyShop_003Ed__41);
		_003CBuyShop_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CBuyShop_003Ed__._003C_003E4__this = this;
		_003CBuyShop_003Ed__.shopItemId = shopItemId;
		_003CBuyShop_003Ed__.count = count;
		_003CBuyShop_003Ed__.linkUniqueId = linkUniqueId;
		_003CBuyShop_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CBuyShop_003Ed__._003C_003Et__builder)).Start<_003CBuyShop_003Ed__41>(ref _003CBuyShop_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CBuyShop_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CBuyVIPShop_003Ed__42))]
	public UniTaskVoid BuyVIPShop(int shopItemId, IAPProductType productType, string linkUniqueId = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CBuyVIPShop_003Ed__42 _003CBuyVIPShop_003Ed__ = default(_003CBuyVIPShop_003Ed__42);
		_003CBuyVIPShop_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CBuyVIPShop_003Ed__._003C_003E4__this = this;
		_003CBuyVIPShop_003Ed__.shopItemId = shopItemId;
		_003CBuyVIPShop_003Ed__.productType = productType;
		_003CBuyVIPShop_003Ed__.linkUniqueId = linkUniqueId;
		_003CBuyVIPShop_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CBuyVIPShop_003Ed__._003C_003Et__builder)).Start<_003CBuyVIPShop_003Ed__42>(ref _003CBuyVIPShop_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CBuyVIPShop_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRestoreVIPEntitlement_003Ed__43))]
	public UniTaskVoid RestoreVIPEntitlement(IAPProductType productType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRestoreVIPEntitlement_003Ed__43 _003CRestoreVIPEntitlement_003Ed__ = default(_003CRestoreVIPEntitlement_003Ed__43);
		_003CRestoreVIPEntitlement_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRestoreVIPEntitlement_003Ed__._003C_003E4__this = this;
		_003CRestoreVIPEntitlement_003Ed__.productType = productType;
		_003CRestoreVIPEntitlement_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRestoreVIPEntitlement_003Ed__._003C_003Et__builder)).Start<_003CRestoreVIPEntitlement_003Ed__43>(ref _003CRestoreVIPEntitlement_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRestoreVIPEntitlement_003Ed__._003C_003Et__builder)).Task;
	}

	public ShopBaseGoods GetStoreItemConfig(int shopItemId)
	{
		return GameApp.Config.GetTable<ExampleTables>()?.TbShopBaseGoods?.GetOrDefault(shopItemId);
	}

	public static string GetPlatformStoreId(ShopBaseGoods config)
	{
		if (config == null)
		{
			return null;
		}
		return ShopService.GetSkuId(config);
	}

	private void BuildSkuIdReverseMap()
	{
		skuToShopItemIdMap.Clear();
		List<ShopBaseGoods> list = GameApp.Config.GetTable<ExampleTables>()?.TbShopBaseGoods?.DataList;
		if (list == null)
		{
			return;
		}
		foreach (ShopBaseGoods item in list)
		{
			string skuId = ShopService.GetSkuId(item);
			if (!string.IsNullOrEmpty(skuId))
			{
				skuToShopItemIdMap[skuId] = item.ID;
			}
		}
	}

	private void ConfigureNonConsumableSkus()
	{
		string nonConsumptionSkus = ShopService.GetNonConsumptionSkus();
		if (!string.IsNullOrEmpty(nonConsumptionSkus))
		{
			SDKManager.SetSkuConfig(2, nonConsumptionSkus);
		}
	}

	[AsyncStateMachine(typeof(_003CStartNetworkMonitoring_003Ed__48))]
	public UniTaskVoid StartNetworkMonitoring()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CStartNetworkMonitoring_003Ed__48 _003CStartNetworkMonitoring_003Ed__ = default(_003CStartNetworkMonitoring_003Ed__48);
		_003CStartNetworkMonitoring_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CStartNetworkMonitoring_003Ed__._003C_003E4__this = this;
		_003CStartNetworkMonitoring_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CStartNetworkMonitoring_003Ed__._003C_003Et__builder)).Start<_003CStartNetworkMonitoring_003Ed__48>(ref _003CStartNetworkMonitoring_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CStartNetworkMonitoring_003Ed__._003C_003Et__builder)).Task;
	}

	private void RefreshCurrentStoreSkus()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Singleton<IAPManager>.Instance.ClearSkuCache();
		UniTaskExtensions.Forget(Singleton<IAPManager>.Instance.FetchCurrentStoreSkus((IAPProductType)0, default(CancellationToken)));
	}

	internal void DebugSimulateRestoreResult(bool isVip, int shopItemId, XingYunIAPErrorCode errorCode = (XingYunIAPErrorCode)1)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		string text = (isVip ? GetSkuIdFromShopItemId(shopItemId) : string.Empty);
		OnRestoreVIPCompleted((IAPProductType)2, new XingYunVIPStatusResponse
		{
			code = (int)errorCode,
			isVip = isVip,
			skuId = (text ?? string.Empty)
		});
	}
}
