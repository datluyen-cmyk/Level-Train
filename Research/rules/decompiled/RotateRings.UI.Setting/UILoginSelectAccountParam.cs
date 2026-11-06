using System;
using RotateRings.Services;

namespace RotateRings.UI.Setting;

public sealed class UILoginSelectAccountParam
{
	public readonly CloudSaveDataSnapshot CloudSnapshot;

	public readonly CloudSaveDataSnapshot LocalSnapshot;

	public readonly Action CompleteAction;

	public readonly Action CancelAction;

	public UILoginSelectAccountParam(CloudSaveDataSnapshot cloudSnapshot, CloudSaveDataSnapshot localSnapshot, Action completeAction, Action cancelAction)
	{
		CloudSnapshot = cloudSnapshot;
		LocalSnapshot = localSnapshot;
		CompleteAction = completeAction;
		CancelAction = cancelAction;
	}
}
