using System;
using RotateRings.DailyFirstWin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.DailyFirstWin;

[UIPath("UIDailyFirstWinNewPiece")]
public class UIDailyFirstWinNewPieceController : BaseController<UIDailyFirstWinNewPieceView>
{
	private DailyFirstWinNewPieceOpenData openData;

	private bool isClosing;

	public UIDailyFirstWinNewPieceController(UIDailyFirstWinNewPieceView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		openData = param as DailyFirstWinNewPieceOpenData;
		isClosing = false;
		if ((Object)(object)base.view.btnContinue != (Object)null)
		{
			((UnityEvent)base.view.btnContinue.onClick).AddListener(new UnityAction(OnContinueClick));
		}
		base.view.ApplyPiece(openData?.PieceSpriteName);
		base.view.ApplyPieceDayTextures(openData?.PieceDay ?? 0);
		base.view.PrepareOpenFirstFrame();
		base.view.PlayOpen();
	}

	private void OnContinueClick()
	{
		if (!isClosing)
		{
			isClosing = true;
			if ((Object)(object)base.view.btnContinue != (Object)null)
			{
				((Selectable)base.view.btnContinue).interactable = false;
			}
			Action onContinue = openData?.OnContinue;
			base.view.PlayClose(delegate
			{
				onContinue?.Invoke();
				openData = null;
				base.CloseView(false);
			});
		}
	}

	public override void OnRemove()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if ((Object)(object)base.view.btnContinue != (Object)null)
		{
			((UnityEvent)base.view.btnContinue.onClick).RemoveListener(new UnityAction(OnContinueClick));
		}
		openData = null;
		isClosing = false;
	}

	public override void OnEscape()
	{
		OnContinueClick();
	}
}
