using System;
using RotateRings.Prop;
using UnityEngine;

namespace RotateRings.UI.PropGuide;

public class PropGuideOpenData
{
	public EPropType PropType;

	public RectTransform SourceAnchor;

	public Action OnClosed;

	public bool IsFirstGuide;

	public Action OnItemTipConfirmed;

	public PropUseEntrySource EntrySource;

	public bool IsFromUnlockView;

	public Action OnUseButtonClicked;

	public bool FireOnItemTipConfirm;
}
