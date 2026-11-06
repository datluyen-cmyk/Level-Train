using System;
using System.Collections.Generic;

namespace Game.Common.Services;

public class ActivityPipeline<T> where T : IActivityStep
{
	private readonly List<T> _registered = new List<T>();

	private readonly Queue<T> _active = new Queue<T>();

	public bool IsEmpty => _active.Count == 0;

	public void Register(T step)
	{
		_registered.Add(step);
	}

	public void Build()
	{
		_active.Clear();
		_registered.Sort((T a, T b) => a.Priority.CompareTo(b.Priority));
		foreach (T item in _registered)
		{
			_active.Enqueue(item);
		}
	}

	public void BuildFromSteps(params T[] steps)
	{
		_active.Clear();
		Array.Sort(steps, (T a, T b) => a.Priority.CompareTo(b.Priority));
		foreach (T item in steps)
		{
			_active.Enqueue(item);
		}
	}

	public bool TryDequeue(out T step)
	{
		while (_active.Count > 0)
		{
			T val = _active.Dequeue();
			if (val.ShouldRun())
			{
				step = val;
				return true;
			}
		}
		step = default(T);
		return false;
	}

	public void Clear()
	{
		_active.Clear();
	}

	public void Reset()
	{
		_registered.Clear();
		_active.Clear();
	}
}
