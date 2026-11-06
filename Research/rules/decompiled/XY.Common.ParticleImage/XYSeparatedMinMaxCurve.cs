using System;
using UnityEngine;

namespace XY.Common.ParticleImage;

[Serializable]
public struct XYSeparatedMinMaxCurve
{
	[SerializeField]
	private bool separable;

	public bool separated;

	public MinMaxCurve mainCurve;

	public MinMaxCurve xCurve;

	public MinMaxCurve yCurve;

	public MinMaxCurve zCurve;

	public XYSeparatedMinMaxCurve(float startValue, bool separated = false, bool separable = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		mainCurve = new MinMaxCurve(startValue);
		xCurve = new MinMaxCurve(startValue);
		yCurve = new MinMaxCurve(startValue);
		zCurve = new MinMaxCurve(startValue);
		this.separated = separated;
		this.separable = separable;
	}

	public XYSeparatedMinMaxCurve(AnimationCurve startValue, bool separated = false, bool separable = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		mainCurve = new MinMaxCurve(1f, new AnimationCurve(startValue.keys));
		xCurve = new MinMaxCurve(1f, new AnimationCurve(startValue.keys));
		yCurve = new MinMaxCurve(1f, new AnimationCurve(startValue.keys));
		zCurve = new MinMaxCurve(1f, new AnimationCurve(startValue.keys));
		this.separated = separated;
		this.separable = separable;
	}

	public Vector3 Evaluate(float time, float lerp)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (separated)
		{
			return new Vector3(((MinMaxCurve)(ref xCurve)).Evaluate(time, lerp), ((MinMaxCurve)(ref yCurve)).Evaluate(time, lerp), ((MinMaxCurve)(ref zCurve)).Evaluate(time, lerp));
		}
		return new Vector3(((MinMaxCurve)(ref mainCurve)).Evaluate(time, lerp), ((MinMaxCurve)(ref mainCurve)).Evaluate(time, lerp), ((MinMaxCurve)(ref mainCurve)).Evaluate(time, lerp));
	}

	public Vector3 EvaluateZ(float time, float lerp)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (separated)
		{
			return new Vector3(0f, 0f, ((MinMaxCurve)(ref zCurve)).Evaluate(time, lerp));
		}
		return new Vector3(0f, 0f, ((MinMaxCurve)(ref mainCurve)).Evaluate(time, lerp));
	}

	public Vector2 EvaluateXY(float time, float lerp)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (separated)
		{
			return new Vector2(((MinMaxCurve)(ref xCurve)).Evaluate(time, lerp), ((MinMaxCurve)(ref yCurve)).Evaluate(time, lerp));
		}
		return new Vector2(((MinMaxCurve)(ref mainCurve)).Evaluate(time, lerp), ((MinMaxCurve)(ref mainCurve)).Evaluate(time, lerp));
	}
}
