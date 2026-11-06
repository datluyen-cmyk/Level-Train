using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

public class UIShopView : GameBaseView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CSpawnShopItemAsync_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<ShopItemView> _003C_003Et__builder;

		public Transform parent;

		public CancellationToken ct;

		public ItemObjectType objectType;

		public UIShopView _003C_003E4__this;

		private string _003Clocation_003E5__2;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIShopView uIShopView = _003C_003E4__this;
			ShopItemView result;
			try
			{
				Awaiter<GameObject> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
					goto IL_00bc;
				}
				if ((Object)(object)parent == (Object)null || ct.IsCancellationRequested)
				{
					result = null;
				}
				else
				{
					_003Clocation_003E5__2 = ResolveItemLocation(objectType);
					if (!string.IsNullOrEmpty(_003Clocation_003E5__2))
					{
						val = ((GameBaseView)uIShopView).LoadAssetAsync<GameObject>(_003Clocation_003E5__2).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CSpawnShopItemAsync_003Ed__12>(ref val, ref this);
							return;
						}
						goto IL_00bc;
					}
					result = null;
				}
				goto end_IL_000e;
				IL_00bc:
				GameObject result2 = val.GetResult();
				if ((Object)(object)result2 == (Object)null || ct.IsCancellationRequested || (Object)(object)parent == (Object)null)
				{
					result = null;
				}
				else
				{
					GameObject val2 = Object.Instantiate<GameObject>(result2, parent, false);
					if ((Object)(object)val2 == (Object)null || ct.IsCancellationRequested || (Object)(object)parent == (Object)null)
					{
						if ((Object)(object)val2 != (Object)null)
						{
							Object.Destroy((Object)(object)val2);
						}
						result = null;
					}
					else
					{
						ShopItemView shopItemView = objectType switch
						{
							ItemObjectType.SpecialOffer => val2.GetComponent<ItemSpecialOfferView>(), 
							ItemObjectType.Prop => val2.GetComponent<ItemPropView>(), 
							_ => null, 
						};
						if ((Object)(object)shopItemView == (Object)null)
						{
							Log.LogError("UIShopView", "Item 预制体缺少对应组件: " + _003Clocation_003E5__2);
							Object.Destroy((Object)(object)val2);
							result = null;
						}
						else
						{
							val2.SetActive(true);
							val2.transform.SetAsLastSibling();
							if ((Object)(object)uIShopView.FirstAreaScrollView != (Object)null && (Object)(object)parent == (Object)(object)uIShopView.FirstAreaScrollView.content)
							{
								ScrollRect[] componentsInChildren = val2.GetComponentsInChildren<ScrollRect>(true);
								for (int i = 0; i < componentsInChildren.Length; i++)
								{
									((Behaviour)componentsInChildren[i]).enabled = false;
								}
							}
							result = shopItemView;
						}
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Clocation_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Clocation_003E5__2 = null;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string Tag = "UIShopView";

	private const string OpenAnimName = "anim_UIShop_appear";

	private const string CloseAnimName = "anim_UIShop_close";

	private readonly ShopCarousel carousel = new ShopCarousel();

	private bool carouselSetup;

	public Animation UIAnimation;

	public Button btnBack;

	public ScrollRect ShowScrollView;

	public ScrollRect FirstAreaScrollView;

	public VerticalLayoutGroup SecondAreaVL;

	public GridLayoutGroup ThirdAreaGrid;

	public void PlayOpenAnimation()
	{
		PlayAnimation("anim_UIShop_appear");
	}

	public float PlayCloseAnimation()
	{
		if ((Object)(object)UIAnimation == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = UIAnimation.GetClip("anim_UIShop_close");
		if ((Object)(object)clip == (Object)null)
		{
			return 0f;
		}
		PlayAnimation("anim_UIShop_close");
		return clip.length;
	}

	public void RefreshCarouselView(int pageCount)
	{
		ClearDynamicAreaChildren();
		EnsureCarouselSetup();
		carousel.Build(pageCount);
	}

	public void RebuildLayout()
	{
		RebuildContent(((Object)(object)FirstAreaScrollView != (Object)null) ? FirstAreaScrollView.content : null);
		RebuildContent((RectTransform)(((Object)(object)ThirdAreaGrid != (Object)null) ? /*isinst with value type is only supported in some contexts*/: null));
		RebuildContent(((Object)(object)ShowScrollView != (Object)null) ? ShowScrollView.content : null);
		carousel.SyncLayout();
	}

	public void PauseCarouselForExternalOverlay()
	{
		carousel.PauseForExternalOverlay();
	}

	public void ResumeCarouselAfterExternalOverlay()
	{
		carousel.ResumeAfterExternalOverlay();
	}

	public void ClearScrollData()
	{
		carousel.Clear();
	}

	[AsyncStateMachine(typeof(_003CSpawnShopItemAsync_003Ed__12))]
	public UniTask<ShopItemView> SpawnShopItemAsync(ItemObjectType objectType, Transform parent, CancellationToken ct = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CSpawnShopItemAsync_003Ed__12 _003CSpawnShopItemAsync_003Ed__ = default(_003CSpawnShopItemAsync_003Ed__12);
		_003CSpawnShopItemAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<ShopItemView>.Create();
		_003CSpawnShopItemAsync_003Ed__._003C_003E4__this = this;
		_003CSpawnShopItemAsync_003Ed__.objectType = objectType;
		_003CSpawnShopItemAsync_003Ed__.parent = parent;
		_003CSpawnShopItemAsync_003Ed__.ct = ct;
		_003CSpawnShopItemAsync_003Ed__._003C_003E1__state = -1;
		_003CSpawnShopItemAsync_003Ed__._003C_003Et__builder.Start<_003CSpawnShopItemAsync_003Ed__12>(ref _003CSpawnShopItemAsync_003Ed__);
		return _003CSpawnShopItemAsync_003Ed__._003C_003Et__builder.Task;
	}

	public void DisposeShopItem(ShopItemView item)
	{
		if (!((Object)(object)item == (Object)null))
		{
			item.Dispose();
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	public override void Dispose()
	{
		carousel.Dispose();
		((UIBaseView)this).Dispose();
	}

	private void EnsureCarouselSetup()
	{
		if (!carouselSetup)
		{
			carousel.Setup(FirstAreaScrollView, ShowScrollView);
			carouselSetup = true;
		}
	}

	private void PlayAnimation(string animName)
	{
		if (!((Object)(object)UIAnimation == (Object)null) && !((Object)(object)UIAnimation.GetClip(animName) == (Object)null))
		{
			UIAnimation.Stop();
			UIAnimation.Rewind(animName);
			UIAnimation.Play(animName);
		}
	}

	private static void RebuildContent(RectTransform content)
	{
		if ((Object)(object)content != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		}
	}

	private void ClearDynamicAreaChildren()
	{
		ClearChildren((Transform)(object)(((Object)(object)FirstAreaScrollView != (Object)null) ? FirstAreaScrollView.content : null));
		ClearChildren(((Object)(object)ThirdAreaGrid != (Object)null) ? ((Component)ThirdAreaGrid).transform : null);
	}

	private static void ClearChildren(Transform parent)
	{
		if (!((Object)(object)parent == (Object)null))
		{
			for (int num = parent.childCount - 1; num >= 0; num--)
			{
				Transform child = parent.GetChild(num);
				child.SetParent((Transform)null, false);
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
	}

	private static string ResolveItemLocation(ItemObjectType objectType)
	{
		return objectType switch
		{
			ItemObjectType.SpecialOffer => "ItemSpecialOffer", 
			ItemObjectType.Prop => "ItemProp", 
			_ => null, 
		};
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		btnBack = behaviour.Get<Button>("btnBack");
		ShowScrollView = behaviour.Get<ScrollRect>("ShowScrollView");
		FirstAreaScrollView = behaviour.Get<ScrollRect>("FirstAreaScrollView");
		SecondAreaVL = behaviour.Get<VerticalLayoutGroup>("SecondAreaVL");
		ThirdAreaGrid = behaviour.Get<GridLayoutGroup>("ThirdAreaGrid");
	}
}
