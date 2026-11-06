using System;
using UnityEngine;

public abstract class GuidMotion : MonoBehaviour
{
	public MotionStatus Status { get; private set; }

	public bool IsMoving => Status == MotionStatus.Running;

	public event Action<MotionStatus> MotionStatusChanged;

	public event Action OnMotionCompleted;

	protected void NotifyMotionCompleted()
	{
		Action action = this.OnMotionCompleted;
		this.OnMotionCompleted = null;
		action?.Invoke();
	}

	protected void ClearMotionCompletedHandlers()
	{
		this.OnMotionCompleted = null;
	}

	public virtual void StartMove()
	{
		TransitionStatus(MotionStatus.Running);
	}

	public virtual void PauseMove()
	{
		if (Status == MotionStatus.Running)
		{
			TransitionStatus(MotionStatus.Paused);
		}
	}

	public virtual void ResumeMove()
	{
		if (Status == MotionStatus.Paused)
		{
			TransitionStatus(MotionStatus.Running);
		}
	}

	public virtual void StopMove()
	{
		ClearMotionCompletedHandlers();
		TransitionStatus(MotionStatus.None);
	}

	public virtual void Finish()
	{
		TransitionStatus(MotionStatus.Finished);
		((Component)this).gameObject.SetActive(false);
	}

	private void TransitionStatus(MotionStatus next)
	{
		if (Status != next)
		{
			Status = next;
			this.MotionStatusChanged?.Invoke(next);
		}
	}
}
