using System;
using System.Collections.Generic;

namespace UnlockRing.Solver;

internal sealed class MinPriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
	private readonly struct Entry
	{
		public TElement Element { get; }

		public TPriority Priority { get; }

		public Entry(TElement element, TPriority priority)
		{
			Element = element;
			Priority = priority;
		}
	}

	private readonly List<Entry> _entries = new List<Entry>();

	public void Enqueue(TElement element, TPriority priority)
	{
		_entries.Add(new Entry(element, priority));
		SiftUp(_entries.Count - 1);
	}

	public bool TryDequeue(out TElement element, out TPriority priority)
	{
		if (_entries.Count == 0)
		{
			element = default(TElement);
			priority = default(TPriority);
			return false;
		}
		Entry entry = _entries[0];
		int num = _entries.Count - 1;
		Entry value = _entries[num];
		_entries.RemoveAt(num);
		if (num != 0)
		{
			_entries[0] = value;
			SiftDown(0);
		}
		element = entry.Element;
		priority = entry.Priority;
		return true;
	}

	private void SiftUp(int index)
	{
		while (index > 0)
		{
			int num = (index - 1) / 2;
			if (_entries[num].Priority.CompareTo(_entries[index].Priority) <= 0)
			{
				break;
			}
			Swap(num, index);
			index = num;
		}
	}

	private void SiftDown(int index)
	{
		while (true)
		{
			int num = index * 2 + 1;
			if (num >= _entries.Count)
			{
				break;
			}
			int num2 = num + 1;
			int num3 = ((num2 < _entries.Count && _entries[num2].Priority.CompareTo(_entries[num].Priority) < 0) ? num2 : num);
			if (_entries[index].Priority.CompareTo(_entries[num3].Priority) <= 0)
			{
				break;
			}
			Swap(index, num3);
			index = num3;
		}
	}

	private void Swap(int first, int second)
	{
		Entry value = _entries[first];
		_entries[first] = _entries[second];
		_entries[second] = value;
	}
}
