using System;
using RotateRings.Prop;
using RotateRings.UI.PropItem;
using UnityEngine;

namespace RotateRings.UI.PropGuide;

public class RocketFastUseOpenData
{
	public EPropType PropType;

	public RectTransform SourceAnchor;

	public Action OnCloseAnimationStart;

	public IPropBadgeProvider BadgeProvider;
}
