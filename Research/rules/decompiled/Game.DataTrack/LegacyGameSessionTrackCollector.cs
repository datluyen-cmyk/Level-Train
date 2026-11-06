using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Services;
using UnityEngine;

namespace Game.DataTrack;

public sealed class LegacyGameSessionTrackCollector
{
	private const float IdleCapSeconds = 30f;

	private const int MaxMoveTimeCount = 256;

	private readonly List<int> assemblyClearIds = new List<int>();

	private readonly List<double> assemblyClearTimes = new List<double>();

	private readonly List<int> hammerSteps = new List<int>();

	private readonly List<int> hammerAssemblies = new List<int>();

	private readonly List<int> bombSteps = new List<int>();

	private readonly List<int> bombNums = new List<int>();

	private readonly List<int> bombAssemblies = new List<int>();

	private readonly List<double> moveTimes = new List<double>();

	private readonly List<int> currentBombAssemblyIds = new List<int>();

	private bool hasBoardZoomBaseline;

	private double boardZoomInitial;

	private double boardZoomEnd;

	private int sessionCameraRange;

	private int restoreButtonAppearCount;

	private int boardZoomAdjustCount;

	private float sessionStartRealtime;

	private float lastInputRealtime;

	private float accumulatedRealSeconds;

	private bool isActive;

	private bool hasRecordedFirstOperation;

	private int firstOperationTimeSeconds;

	private int clearStep;

	private bool isCollectingBombBatch;

	private float lastValidMoveUpRealtime;

	public IReadOnlyList<int> AssemblyClearIds => assemblyClearIds;

	public IReadOnlyList<double> AssemblyClearTimes => assemblyClearTimes;

	public IReadOnlyList<int> HammerSteps => hammerSteps;

	public IReadOnlyList<int> HammerAssemblies => hammerAssemblies;

	public IReadOnlyList<int> BombSteps => bombSteps;

	public IReadOnlyList<int> BombNums => bombNums;

	public IReadOnlyList<int> BombAssemblies => bombAssemblies;

	public int FirstOperationTimeSeconds => firstOperationTimeSeconds;

	public void EndMoveGesture(IGameplayObject gameplayObject)
	{
		if (isActive && GameplayMoveTrackThresholds.ShouldRecordMoveTime(gameplayObject))
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = ((moveTimes.Count == 0) ? sessionStartRealtime : lastValidMoveUpRealtime);
			float num2 = Mathf.Max(0f, realtimeSinceStartup - num);
			double item = Math.Round((num2 <= 30f) ? num2 : 30f, 2);
			if (moveTimes.Count < 256)
			{
				moveTimes.Add(item);
			}
			lastValidMoveUpRealtime = realtimeSinceStartup;
		}
	}

	public double[] GetMoveTimeArray()
	{
		if (moveTimes.Count != 0)
		{
			return moveTimes.ToArray();
		}
		return null;
	}

	public void BeginSession()
	{
		Reset();
		sessionStartRealtime = Time.realtimeSinceStartup;
		lastInputRealtime = sessionStartRealtime;
		isActive = true;
	}

	public void EndSession()
	{
		isActive = false;
	}

	public void RecordFirstOperation()
	{
		if (isActive && !hasRecordedFirstOperation)
		{
			hasRecordedFirstOperation = true;
			float num = Mathf.Max(0f, Time.realtimeSinceStartup - sessionStartRealtime);
			firstOperationTimeSeconds = Mathf.CeilToInt(num);
		}
	}

	public void RecordAssemblyCleared(int assemblyId, LegacyOperationSource source)
	{
		if (!isActive || assemblyId <= 0)
		{
			return;
		}
		RecordPlayerInput();
		clearStep++;
		assemblyClearIds.Add(assemblyId);
		double stepRealSeconds = GetStepRealSeconds();
		assemblyClearTimes.Add(stepRealSeconds);
		switch (source)
		{
		case LegacyOperationSource.Hammer:
			hammerSteps.Add(clearStep);
			hammerAssemblies.Add(assemblyId);
			break;
		case LegacyOperationSource.Bomb:
			if (!isCollectingBombBatch)
			{
				isCollectingBombBatch = true;
				currentBombAssemblyIds.Clear();
			}
			currentBombAssemblyIds.Add(assemblyId);
			break;
		}
	}

	public void CommitBombBatch()
	{
		if (!isCollectingBombBatch || currentBombAssemblyIds.Count == 0)
		{
			isCollectingBombBatch = false;
			currentBombAssemblyIds.Clear();
			return;
		}
		int item = clearStep - currentBombAssemblyIds.Count + 1;
		bombSteps.Add(item);
		bombNums.Add(currentBombAssemblyIds.Count);
		bombAssemblies.AddRange(currentBombAssemblyIds);
		isCollectingBombBatch = false;
		currentBombAssemblyIds.Clear();
	}

	public void CopyTo(out int[] clearIds, out double[] clearTimes, out int[] hammerStepArray, out int[] hammerAssemblyArray, out int[] bombStepArray, out int[] bombNumArray, out int[] bombAssemblyArray)
	{
		clearIds = ((assemblyClearIds.Count == 0) ? null : assemblyClearIds.ToArray());
		clearTimes = ((assemblyClearTimes.Count == 0) ? null : assemblyClearTimes.ToArray());
		hammerStepArray = ((hammerSteps.Count == 0) ? null : hammerSteps.ToArray());
		hammerAssemblyArray = ((hammerAssemblies.Count == 0) ? null : hammerAssemblies.ToArray());
		bombStepArray = ((bombSteps.Count == 0) ? null : bombSteps.ToArray());
		bombNumArray = ((bombNums.Count == 0) ? null : bombNums.ToArray());
		bombAssemblyArray = ((bombAssemblies.Count == 0) ? null : bombAssemblies.ToArray());
	}

	public void SetBoardZoomInitial(float orthographicSize)
	{
		if (isActive && !hasBoardZoomBaseline && !(orthographicSize <= 0f))
		{
			boardZoomEnd = (boardZoomInitial = ToBoardZoomRatio(orthographicSize));
			sessionCameraRange = EncodeCameraRange(orthographicSize);
			hasBoardZoomBaseline = true;
		}
	}

	public int GetSessionCameraRange()
	{
		return sessionCameraRange;
	}

	public void RecordRestoreButtonAppeared()
	{
		if (isActive)
		{
			restoreButtonAppearCount++;
		}
	}

	public int GetRestoreButtonAppearCount()
	{
		return restoreButtonAppearCount;
	}

	private static int EncodeCameraRange(float orthographicSize)
	{
		return Mathf.RoundToInt(orthographicSize * 10f);
	}

	public void FinalizeBoardZoomEnd(float orthographicSize)
	{
		if (hasBoardZoomBaseline && !(orthographicSize <= 0f))
		{
			boardZoomEnd = ToBoardZoomRatio(orthographicSize);
		}
	}

	public void RecordBoardZoomPinchEnded(float sizeAtGestureStart, float sizeAtGestureEnd)
	{
		if (isActive && hasBoardZoomBaseline && !(sizeAtGestureStart <= 0f) && !(sizeAtGestureEnd <= 0f) && !Mathf.Approximately(sizeAtGestureStart, sizeAtGestureEnd))
		{
			boardZoomAdjustCount++;
		}
	}

	public double[] GetBoardScaleArray()
	{
		if (!hasBoardZoomBaseline)
		{
			return null;
		}
		return new double[3] { boardZoomInitial, boardZoomEnd, boardZoomAdjustCount };
	}

	private static double ToBoardZoomRatio(float orthographicSize)
	{
		return Math.Round(RotateCameraAdaptation.InitialSize / orthographicSize, 2);
	}

	private void RecordPlayerInput()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!isActive)
		{
			lastInputRealtime = realtimeSinceStartup;
			return;
		}
		if (lastInputRealtime > 0f)
		{
			float num = realtimeSinceStartup - lastInputRealtime;
			accumulatedRealSeconds += ((num <= 30f) ? num : 30f);
		}
		lastInputRealtime = realtimeSinceStartup;
	}

	private double GetStepRealSeconds()
	{
		RecordPlayerInput();
		if (assemblyClearTimes.Count == 0)
		{
			return Math.Round(accumulatedRealSeconds, 2);
		}
		double num = accumulatedRealSeconds;
		for (int i = 0; i < assemblyClearTimes.Count; i++)
		{
			num -= assemblyClearTimes[i];
		}
		return Math.Round(Math.Max(0.0, num), 2);
	}

	private void Reset()
	{
		assemblyClearIds.Clear();
		assemblyClearTimes.Clear();
		hammerSteps.Clear();
		hammerAssemblies.Clear();
		bombSteps.Clear();
		bombNums.Clear();
		bombAssemblies.Clear();
		moveTimes.Clear();
		currentBombAssemblyIds.Clear();
		hasBoardZoomBaseline = false;
		boardZoomInitial = 0.0;
		boardZoomEnd = 0.0;
		sessionCameraRange = 0;
		restoreButtonAppearCount = 0;
		boardZoomAdjustCount = 0;
		sessionStartRealtime = 0f;
		lastInputRealtime = 0f;
		accumulatedRealSeconds = 0f;
		isActive = false;
		hasRecordedFirstOperation = false;
		firstOperationTimeSeconds = 0;
		clearStep = 0;
		isCollectingBombBatch = false;
		lastValidMoveUpRealtime = 0f;
	}
}
