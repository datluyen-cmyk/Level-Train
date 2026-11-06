using System;
using Cysharp.Threading.Tasks;
using RotateRings.WeakGuide.Setup;
using UnityEngine;

namespace RotateRings.WeakGuide.StepHandles;

public abstract class WeakGuideStepHandleBase : IWeakGuideStepHandle
{
	protected virtual string Tag => "[WeakGuide][Handle] ";

	public abstract string HandleId { get; }

	public virtual string HandleName => HandleId;

	public abstract Type ParametersType { get; }

	public virtual string DefaultParametersJson => "{}";

	public abstract UniTask<GuideStep> BuildGuideStepAsync(WeakGuideBuildContext ctx, string paramsJson);

	public virtual void OnStepStart(WeakGuideManager manager, GuideStep step)
	{
	}

	public virtual void OnStepFinish(WeakGuideManager manager, GuideStep step)
	{
	}

	public virtual bool TryFinishStep(WeakGuideStepTrigger trigger, object args)
	{
		return false;
	}

	protected T ParseParams<T>(string paramsJson) where T : class, new()
	{
		if (string.IsNullOrWhiteSpace(paramsJson))
		{
			return new T();
		}
		try
		{
			T val = new T();
			JsonUtility.FromJsonOverwrite(paramsJson, (object)val);
			return val;
		}
		catch (Exception)
		{
			return new T();
		}
	}

	protected void LogInfo(string message)
	{
	}

	protected void LogWarning(string message)
	{
	}

	protected void LogError(string message)
	{
	}
}
