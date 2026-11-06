using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Res;
using XingYun.UI.Core;

namespace Game.UI;

public abstract class GameBaseView : UIBaseView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CAttachWidgetAsync_003Ed__14<TWidget> : IAsyncStateMachine where TWidget : GameWidgetView, new()
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<WidgetHandle> _003C_003Et__builder;

		public GameBaseView _003C_003E4__this;

		public string widgetLocation;

		public Transform parent;

		public object injectParam;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameBaseView gameBaseView = _003C_003E4__this;
			WidgetHandle result2;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					val = gameBaseView.LoadAssetAsync<GameObject>(widgetLocation).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CAttachWidgetAsync_003Ed__14<TWidget>>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
				}
				GameObject result = val.GetResult();
				if ((Object)(object)result == (Object)null)
				{
					result2 = null;
				}
				else if ((Object)(object)((UIBaseView)gameBaseView).Behaviour == (Object)null)
				{
					result2 = null;
				}
				else
				{
					parent = parent ?? ((Component)((UIBaseView)gameBaseView).Behaviour).transform;
					GameObject val2 = Object.Instantiate<GameObject>(result, parent, false);
					UIBehaviour component = val2.GetComponent<UIBehaviour>();
					if ((Object)(object)component == (Object)null)
					{
						Log.LogError("GameBaseView", "Widget Prefab 缺少 UIBehaviour: " + widgetLocation);
						Object.Destroy((Object)(object)val2);
						result2 = null;
					}
					else
					{
						TWidget val3 = new TWidget();
						((UIBaseView)val3).Location = widgetLocation;
						((UIBaseView)val3).Behaviour = component;
						_003F val4 = val3;
						Transform transform = val2.transform;
						((UIBaseView)val4).NodeRect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
						val3.m_LoadAssetGroup = gameBaseView.LoadAssetGroup;
						InitializeWidgetAfterInject(val3, component);
						WidgetHandle widgetHandle = new WidgetHandle
						{
							Location = widgetLocation,
							View = val3,
							GameObject = val2,
							Param = injectParam
						};
						if (gameBaseView.m_WidgetHandles == null)
						{
							gameBaseView.m_WidgetHandles = new List<WidgetHandle>();
						}
						gameBaseView.m_WidgetHandles.Add(widgetHandle);
						result2 = widgetHandle;
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result2);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadAssetAsync_003Ed__6<T> : IAsyncStateMachine where T : Object
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<T> _003C_003Et__builder;

		public string location;

		public GameBaseView _003C_003E4__this;

		private Awaiter<T> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameBaseView gameBaseView = _003C_003E4__this;
			T result;
			try
			{
				Awaiter<T> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<T>);
					num = (_003C_003E1__state = -1);
					goto IL_0090;
				}
				if (!string.IsNullOrEmpty(location))
				{
					val = GameApp.Res.LoadAssetAsync<T>(location, gameBaseView.LoadAssetGroup).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<T>, _003CLoadAssetAsync_003Ed__6<T>>(ref val, ref this);
						return;
					}
					goto IL_0090;
				}
				result = default(T);
				goto end_IL_000e;
				IL_0090:
				T result2 = val.GetResult();
				if ((Object)(object)result2 != (Object)null)
				{
					gameBaseView.RecordAsset(location);
				}
				result = result2;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
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

	private Dictionary<string, int> loadedAssetRecords;

	private string m_LoadAssetGroup;

	private List<WidgetHandle> m_WidgetHandles;

	public string LoadAssetGroup
	{
		get
		{
			if (m_LoadAssetGroup == null)
			{
				ILocationResolver locationResolver = ResManager.LocationResolver;
				m_LoadAssetGroup = ((locationResolver != null) ? locationResolver.Resolve(((UIBaseView)this).Location) : null) ?? ((UIBaseView)this).Location;
			}
			return m_LoadAssetGroup;
		}
	}

	protected T LoadAsset<T>(string location) where T : Object
	{
		if (string.IsNullOrEmpty(location))
		{
			return default(T);
		}
		T val = GameApp.Res.LoadAsset<T>(location, LoadAssetGroup);
		if ((Object)(object)val != (Object)null)
		{
			RecordAsset(location);
		}
		return val;
	}

	[AsyncStateMachine(typeof(_003CLoadAssetAsync_003Ed__6<>))]
	protected unsafe UniTask<T> LoadAssetAsync<T>(string location) where T : Object
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadAssetAsync_003Ed__6<T> _003CLoadAssetAsync_003Ed__ = default(_003CLoadAssetAsync_003Ed__6<T>);
		_003CLoadAssetAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<T>.Create();
		_003CLoadAssetAsync_003Ed__._003C_003E4__this = this;
		_003CLoadAssetAsync_003Ed__.location = location;
		_003CLoadAssetAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder<_003CLoadAssetAsync_003Ed__6<T>>*)(&_003CLoadAssetAsync_003Ed__._003C_003Et__builder))->Start<_003CLoadAssetAsync_003Ed__6<T>>(ref _003CLoadAssetAsync_003Ed__);
		return _003CLoadAssetAsync_003Ed__._003C_003Et__builder.Task;
	}

	private void RecordAsset(string location)
	{
		if (loadedAssetRecords == null)
		{
			loadedAssetRecords = new Dictionary<string, int>();
		}
		loadedAssetRecords.TryGetValue(location, out var value);
		loadedAssetRecords[location] = value + 1;
	}

	public void OnUnloadAsset()
	{
		if (loadedAssetRecords == null || loadedAssetRecords.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, int> loadedAssetRecord in loadedAssetRecords)
		{
			GameApp.Res.UnloadAsset(loadedAssetRecord.Key, LoadAssetGroup, loadedAssetRecord.Value);
		}
		loadedAssetRecords.Clear();
	}

	private static void InitializeWidgetAfterInject<TWidget>(TWidget widget, UIBehaviour behaviour) where TWidget : GameWidgetView, new()
	{
		((UIBaseView)widget).Inject(behaviour);
		widget.NotifyWidgetFirstLoaded();
	}

	public WidgetHandle AttachWidget<TWidget>(string widgetLocation, Transform parent = null, object injectParam = null) where TWidget : GameWidgetView, new()
	{
		GameObject val = LoadAsset<GameObject>(widgetLocation);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		object obj = parent;
		if (obj == null)
		{
			UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
			obj = ((behaviour != null) ? ((Component)behaviour).transform : null);
		}
		parent = (Transform)obj;
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		UIBehaviour component = val2.GetComponent<UIBehaviour>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("GameBaseView", "Widget Prefab 缺少 UIBehaviour: " + widgetLocation);
			Object.Destroy((Object)(object)val2);
			return null;
		}
		TWidget val3 = new TWidget();
		((UIBaseView)val3).Location = widgetLocation;
		((UIBaseView)val3).Behaviour = component;
		_003F val4 = val3;
		Transform transform = val2.transform;
		((UIBaseView)val4).NodeRect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		val3.m_LoadAssetGroup = LoadAssetGroup;
		InitializeWidgetAfterInject(val3, component);
		WidgetHandle widgetHandle = new WidgetHandle
		{
			Location = widgetLocation,
			View = val3,
			GameObject = val2,
			Param = injectParam
		};
		if (m_WidgetHandles == null)
		{
			m_WidgetHandles = new List<WidgetHandle>();
		}
		m_WidgetHandles.Add(widgetHandle);
		return widgetHandle;
	}

	public WidgetHandle AttachWidget<TWidget>(GameObject template, Transform parent = null, object injectParam = null) where TWidget : GameWidgetView, new()
	{
		if ((Object)(object)template == (Object)null)
		{
			return null;
		}
		object obj = parent;
		if (obj == null)
		{
			UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
			obj = ((behaviour != null) ? ((Component)behaviour).transform : null);
		}
		parent = (Transform)obj;
		GameObject val = Object.Instantiate<GameObject>(template, parent, false);
		UIBehaviour component = val.GetComponent<UIBehaviour>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("GameBaseView", "Widget 模板缺少 UIBehaviour");
			Object.Destroy((Object)(object)val);
			return null;
		}
		TWidget val2 = new TWidget();
		((UIBaseView)val2).Behaviour = component;
		_003F val3 = val2;
		Transform transform = val.transform;
		((UIBaseView)val3).NodeRect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		val2.m_LoadAssetGroup = LoadAssetGroup;
		InitializeWidgetAfterInject(val2, component);
		WidgetHandle widgetHandle = new WidgetHandle
		{
			View = val2,
			GameObject = val,
			Param = injectParam
		};
		if (m_WidgetHandles == null)
		{
			m_WidgetHandles = new List<WidgetHandle>();
		}
		m_WidgetHandles.Add(widgetHandle);
		return widgetHandle;
	}

	public WidgetHandle AttachWidget<TWidget>(UIBehaviour behaviour, object injectParam = null) where TWidget : GameWidgetView, new()
	{
		if ((Object)(object)behaviour == (Object)null)
		{
			return null;
		}
		TWidget val = new TWidget();
		((UIBaseView)val).Behaviour = behaviour;
		_003F val2 = val;
		Transform transform = ((Component)behaviour).transform;
		((UIBaseView)val2).NodeRect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		val.m_LoadAssetGroup = LoadAssetGroup;
		InitializeWidgetAfterInject(val, behaviour);
		WidgetHandle widgetHandle = new WidgetHandle
		{
			View = val,
			GameObject = ((Component)behaviour).gameObject,
			Param = injectParam,
			OwnsGameObject = false
		};
		if (m_WidgetHandles == null)
		{
			m_WidgetHandles = new List<WidgetHandle>();
		}
		m_WidgetHandles.Add(widgetHandle);
		return widgetHandle;
	}

	[AsyncStateMachine(typeof(_003CAttachWidgetAsync_003Ed__14<>))]
	public UniTask<WidgetHandle> AttachWidgetAsync<TWidget>(string widgetLocation, Transform parent = null, object injectParam = null) where TWidget : GameWidgetView, new()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CAttachWidgetAsync_003Ed__14<TWidget> _003CAttachWidgetAsync_003Ed__ = default(_003CAttachWidgetAsync_003Ed__14<TWidget>);
		_003CAttachWidgetAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<WidgetHandle>.Create();
		_003CAttachWidgetAsync_003Ed__._003C_003E4__this = this;
		_003CAttachWidgetAsync_003Ed__.widgetLocation = widgetLocation;
		_003CAttachWidgetAsync_003Ed__.parent = parent;
		_003CAttachWidgetAsync_003Ed__.injectParam = injectParam;
		_003CAttachWidgetAsync_003Ed__._003C_003E1__state = -1;
		_003CAttachWidgetAsync_003Ed__._003C_003Et__builder.Start<_003CAttachWidgetAsync_003Ed__14<TWidget>>(ref _003CAttachWidgetAsync_003Ed__);
		return _003CAttachWidgetAsync_003Ed__._003C_003Et__builder.Task;
	}

	protected TWidget EnsureWidget<TWidget>(ref WidgetHandle handle, UIBehaviour behaviour) where TWidget : GameWidgetView, new()
	{
		if (handle != null && !handle.IsDisposed)
		{
			return handle.GetView<TWidget>();
		}
		if ((Object)(object)behaviour == (Object)null)
		{
			return null;
		}
		handle = AttachWidget<TWidget>(behaviour);
		if (handle == null)
		{
			Log.LogError("GameBaseView", "[{0}] AttachWidget failed: {1}", new object[2]
			{
				((object)this).GetType().Name,
				typeof(TWidget).Name
			});
			return null;
		}
		return handle.GetView<TWidget>();
	}

	protected TWidget EnsureWidget<TWidget>(ref WidgetHandle handle, string widgetLocation, Transform parent) where TWidget : GameWidgetView, new()
	{
		if (handle != null && !handle.IsDisposed)
		{
			return handle.GetView<TWidget>();
		}
		if ((Object)(object)parent == (Object)null)
		{
			return null;
		}
		handle = AttachWidget<TWidget>(widgetLocation, parent);
		if (handle == null)
		{
			Log.LogError("GameBaseView", "[{0}] AttachWidget failed: {1}", new object[2]
			{
				((object)this).GetType().Name,
				widgetLocation
			});
			return null;
		}
		return handle.GetView<TWidget>();
	}

	public void DetachWidget(WidgetHandle handle)
	{
		if (handle != null)
		{
			handle.Dispose();
			m_WidgetHandles?.Remove(handle);
		}
	}

	public void DetachAllWidgets()
	{
		if (m_WidgetHandles != null && m_WidgetHandles.Count != 0)
		{
			for (int num = m_WidgetHandles.Count - 1; num >= 0; num--)
			{
				m_WidgetHandles[num].Dispose();
			}
			m_WidgetHandles.Clear();
		}
	}

	public void SetSprite(Image img, string spriteName)
	{
		if (!string.IsNullOrEmpty(spriteName))
		{
			Sprite val = LoadAsset<Sprite>(spriteName);
			if ((Object)(object)val != (Object)null)
			{
				img.sprite = val;
			}
		}
	}
}
