using System;

namespace RotateRings.UI.Toast;

public class UIToastOpenParam
{
	public EToastType ToastType;

	public string Content;

	public float Duration = 2f;

	public Action OnClosed;

	public Action OnEscape;

	public Action OnDismissed;

	public ToastVerticalAlign VerticalAlign;

	public float Offset;
}
