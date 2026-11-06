using System;
using UnityEngine;

public sealed class PullMoveGuardSubstepMover
{
	private const float StepDistanceEpsilon = 1E-05f;

	public float ClampFrameMoveDistance(float axisMoveAbs, float maxFrameMoveWorld)
	{
		return Mathf.Min(axisMoveAbs, maxFrameMoveWorld);
	}

	public void MoveInSteps(float totalDist, Func<float, float> tryMoveOneStep)
	{
		if (tryMoveOneStep == null || totalDist <= 1E-05f)
		{
			return;
		}
		float num = totalDist;
		while (num > 1E-05f)
		{
			float arg = Mathf.Min(PullMoveGuardConfig.MaxMoveStepWorld, num);
			float num2 = tryMoveOneStep(arg);
			if (!(num2 <= 1E-05f))
			{
				num -= num2;
				continue;
			}
			break;
		}
	}
}
