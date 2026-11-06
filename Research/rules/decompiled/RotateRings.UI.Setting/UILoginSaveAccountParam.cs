using System;
using RotateRings.Services;

namespace RotateRings.UI.Setting;

public sealed class UILoginSaveAccountParam
{
	public readonly CloudSaveSelectionSource Source;

	public readonly CloudSaveDataSnapshot PreviewSnapshot;

	public readonly CloudSaveDataSnapshot CloudSnapshot;

	public readonly Action CompleteAction;

	public UILoginSaveAccountParam(CloudSaveSelectionSource source, CloudSaveDataSnapshot previewSnapshot, CloudSaveDataSnapshot cloudSnapshot, Action completeAction)
	{
		Source = source;
		PreviewSnapshot = previewSnapshot;
		CloudSnapshot = cloudSnapshot;
		CompleteAction = completeAction;
	}
}
