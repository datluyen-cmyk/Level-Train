using System;
using UnityEngine;

namespace XingYun.RingGame;

[Serializable]
public struct RingAttachment
{
	public enum AttachType
	{
		HeadedBar,
		TBar
	}

	public AttachType type;

	[Range(0f, 360f)]
	[Tooltip("Angle position on the ring (degrees).")]
	public float angleDeg;

	[Tooltip("Width of the thin stem extending from the ring.")]
	public float stemWidth;

	[Tooltip("Length of the thin stem (radial extension beyond the ring band).")]
	public float stemLength;

	[Tooltip("How far the stem inserts back into the ring (covered by ring).")]
	public float stemInsertOffset;

	[Tooltip("True when the pin mounts on the inner side and extends toward the ring center.")]
	public bool isInnerMount;

	[Tooltip("HeadedBar: head length at stem tip. TBar: cross bar width (thickness).")]
	public float crossWidth;

	[Tooltip("HeadedBar: not used (head mapped via UV). TBar: cross bar length.")]
	public float crossLength;

	[Tooltip("Style row index for this attachment.")]
	public int styleIndex;

	[Tooltip("TBar: head/tail cap length (symmetric). 0 = no caps. Auto-compressed by shader if too long.")]
	public float crossCapLength;
}
