using System;

namespace RotateRings.UI.Setting;

public class UILoginSignInParam
{
	public readonly UILoginSignInResultType ResultType;

	public readonly Action RetryAction;

	public readonly Action CancelAction;

	public UILoginSignInParam(UILoginSignInResultType resultType, Action retryAction = null, Action cancelAction = null)
	{
		ResultType = resultType;
		RetryAction = retryAction;
		CancelAction = cancelAction;
	}
}
