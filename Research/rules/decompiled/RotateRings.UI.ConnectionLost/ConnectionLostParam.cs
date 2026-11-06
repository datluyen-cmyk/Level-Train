using System;
using XingYun.IAP.Core;

namespace RotateRings.UI.ConnectionLost;

public sealed class ConnectionLostParam
{
	public XingYunIAPErrorCode ErrorCode;

	public Action OnConfirm;

	public Action OnCancel;
}
