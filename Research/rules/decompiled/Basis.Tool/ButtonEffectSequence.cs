using System;
using System.Collections.Generic;
using UnityEngine;

namespace Basis.Tool;

[Serializable]
public class ButtonEffectSequence
{
	[Tooltip("缩放关键帧列表")]
	public List<ScaleKeyFrame> scaleKeyFrames = new List<ScaleKeyFrame>();

	[Tooltip("透明度关键帧列表")]
	public List<AlphaKeyFrame> alphaKeyFrames = new List<AlphaKeyFrame>();
}
