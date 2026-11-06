using System;
using System.Collections;
using System.Collections.Generic;
using UniLog;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace Game.UI;

public abstract class GameBaseController<TView> : BaseController<TView> where TView : GameBaseView
{
	private readonly List<IUIBaseService> uiServices = new List<IUIBaseService>();

	private bool uiServicesDisposed;

	public TView View => base.view;

	protected GameBaseController(TView view)
		: base(view)
	{
	}

	internal void RegisterUIService(IUIBaseService service)
	{
		uiServices.Add(service);
	}

	public override void OnRemove()
	{
		DisposeAllUIServices();
	}

	private void DisposeAllUIServices()
	{
		if (uiServicesDisposed)
		{
			return;
		}
		uiServicesDisposed = true;
		for (int num = uiServices.Count - 1; num >= 0; num--)
		{
			try
			{
				uiServices[num].Dispose();
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[GameBaseController] Dispose UIService failed: {0}", new object[1] { ex.Message });
			}
		}
		uiServices.Clear();
	}

	public Coroutine StartCoroutine(IEnumerator coroutine)
	{
		return ((MonoBehaviour)((UIBaseView)base.view).Behaviour).StartCoroutine(coroutine);
	}

	public Coroutine StartCoroutine(string methodName)
	{
		return ((MonoBehaviour)((UIBaseView)base.view).Behaviour).StartCoroutine(methodName);
	}

	public void StopCoroutine(Coroutine coroutine)
	{
		((MonoBehaviour)((UIBaseView)base.view).Behaviour).StopCoroutine(coroutine);
	}

	public void StopCoroutine(string methodName)
	{
		((MonoBehaviour)((UIBaseView)base.view).Behaviour).StopCoroutine(methodName);
	}

	public void StopCoroutine(IEnumerator coroutine)
	{
		((MonoBehaviour)((UIBaseView)base.view).Behaviour).StopCoroutine(coroutine);
	}

	public void StopAllCoroutines()
	{
		((MonoBehaviour)((UIBaseView)base.view).Behaviour).StopAllCoroutines();
	}

	public override void OnUnloadAsset()
	{
		DisposeAllUIServices();
		base.view.DetachAllWidgets();
		base.view.OnUnloadAsset();
		GameApp.Res.UnloadAsset(((UIBaseView)base.view).Location, base.view.LoadAssetGroup, 1);
	}
}
