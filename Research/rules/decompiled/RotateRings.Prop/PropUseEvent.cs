using UnityEngine;

namespace RotateRings.Prop;

public class PropUseEvent
{
	public EPropType PropType;

	public RectTransform SourceAnchor;

	public PropUseEntrySource EntrySource;

	public bool IsFromUnlockView;
}
