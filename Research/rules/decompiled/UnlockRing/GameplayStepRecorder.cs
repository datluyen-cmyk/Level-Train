using System;
using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class GameplayStepRecorder
{
	private sealed class StepBuilder
	{
		private readonly List<LegacyOperationSource> operationSources = new List<LegacyOperationSource>(2);

		private readonly List<GameplayReleasedPart> releasedParts = new List<GameplayReleasedPart>(2);

		public PartId PartId { get; }

		public GameplayStepState State { get; set; }

		public int OperationCount { get; private set; }

		public int ReleasedPartCount { get; private set; }

		public StepBuilder(PartId partId)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			PartId = partId;
			State = GameplayStepState.NoEffect;
		}

		public void Add(GameplayOperationResult operation)
		{
			OperationCount++;
			ReleasedPartCount += operation.ReleasedPartCount;
			for (int i = 0; i < operation.ReleasedParts.Count; i++)
			{
				releasedParts.Add(operation.ReleasedParts[i]);
			}
			if (!operationSources.Contains(operation.OperationSource))
			{
				operationSources.Add(operation.OperationSource);
			}
		}

		public GameplayStepRecord CreateRecord(int stepIndex)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return new GameplayStepRecord(stepIndex, PartId, State, OperationCount, ReleasedPartCount, Array.AsReadOnly(releasedParts.ToArray()), Array.AsReadOnly(operationSources.ToArray()));
		}
	}

	private const string TAG = "[UnlockRing][GameplayStepRecorder] ";

	private readonly List<GameplayStepRecord> completedSteps = new List<GameplayStepRecord>();

	private readonly Action<GameplayStepRecord> onStepCommitted;

	private StepBuilder currentStep;

	private int nextStepIndex = 1;

	public IReadOnlyList<GameplayStepRecord> CompletedSteps => completedSteps;

	public GameplayStepRecord CurrentStep
	{
		get
		{
			if (currentStep != null)
			{
				return currentStep.CreateRecord(nextStepIndex);
			}
			return null;
		}
	}

	public GameplayStepRecorder(Action<GameplayStepRecord> onStepCommitted = null)
	{
		this.onStepCommitted = onStepCommitted;
	}

	public void Record(GameplayOperationResult operation)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		CommitPendingStepBeforeRecord(operation);
		if (currentStep == null)
		{
			currentStep = new StepBuilder(operation.PartId);
		}
		currentStep.Add(operation);
		ApplyOperationOutcome(operation);
	}

	private void CommitPendingStepBeforeRecord(GameplayOperationResult operation)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (currentStep != null)
		{
			PartId partId = currentStep.PartId;
			if (!((PartId)(ref partId)).Equals(operation.PartId))
			{
				CommitCurrentStep();
			}
			else if (IsPropOperationSource(operation.OperationSource))
			{
				CommitCurrentStep();
			}
		}
	}

	private static bool IsPropOperationSource(LegacyOperationSource source)
	{
		if (source != LegacyOperationSource.Hammer && source != LegacyOperationSource.Bomb)
		{
			return source == LegacyOperationSource.ColorLock;
		}
		return true;
	}

	private void ApplyOperationOutcome(GameplayOperationResult operation)
	{
		if (currentStep.ReleasedPartCount > 0)
		{
			currentStep.State = GameplayStepState.UnlockRelease;
			CommitCurrentStep();
		}
		else if (operation.Completion == GameplayOperationCompletion.Blocked)
		{
			currentStep.State = GameplayStepState.Error;
			CommitCurrentStep();
		}
		else if (currentStep.State != GameplayStepState.Error)
		{
			currentStep.State = GameplayStepState.NoEffect;
		}
	}

	public GameplayStepRecord Flush()
	{
		return CommitCurrentStep();
	}

	public void Reset()
	{
		completedSteps.Clear();
		currentStep = null;
		nextStepIndex = 1;
	}

	private GameplayStepRecord CommitCurrentStep()
	{
		if (currentStep == null)
		{
			return null;
		}
		GameplayStepRecord gameplayStepRecord = currentStep.CreateRecord(nextStepIndex);
		completedSteps.Add(gameplayStepRecord);
		nextStepIndex++;
		currentStep = null;
		onStepCommitted?.Invoke(gameplayStepRecord);
		return gameplayStepRecord;
	}

	private string DescribeCurrentStep()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (currentStep == null)
		{
			return "none";
		}
		return $"partId={currentStep.PartId},state={currentStep.State},operationCount={currentStep.OperationCount},releasedPartCount={currentStep.ReleasedPartCount}";
	}
}
