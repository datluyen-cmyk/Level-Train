using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.WeakGuide.Data;

[CreateAssetMenu(fileName = "WeakGuideSteps", menuName = "RotateRings/WeakGuide/Guide Steps Asset")]
public sealed class GuideStepsAsset : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public int LevelId;

		public string GuideIndex = "default";

		[TextArea(3, 12)]
		public string StepHandlesJson = "";
	}

	[Tooltip("各关卡引导步骤数据。运行时按 LevelId + GuideIndex 索引。")]
	public List<Entry> Entries = new List<Entry>();

	public Entry FindEntry(int levelId, string guideIndex = null)
	{
		if (Entries == null)
		{
			return null;
		}
		string text = (string.IsNullOrEmpty(guideIndex) ? "default" : guideIndex);
		for (int i = 0; i < Entries.Count; i++)
		{
			Entry entry = Entries[i];
			if (entry != null && entry.LevelId == levelId && !((string.IsNullOrEmpty(entry.GuideIndex) ? "default" : entry.GuideIndex) != text))
			{
				if (string.IsNullOrWhiteSpace(entry.StepHandlesJson))
				{
					return null;
				}
				return entry;
			}
		}
		return null;
	}
}
