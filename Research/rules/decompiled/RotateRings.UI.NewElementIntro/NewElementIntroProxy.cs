using System.Collections.Generic;
using XingYun.Proxy;

namespace RotateRings.UI.NewElementIntro;

public class NewElementIntroProxy : BaseProxy
{
	private readonly NewElementIntroData data;

	public IReadOnlyList<int> IntroducedElementIds => data.IntroducedElementIds;

	public NewElementIntroProxy()
		: base("NewElementIntroProxy")
	{
		data = GameApp.Save.GetOrCreate<NewElementIntroData>();
		NewElementIntroData newElementIntroData = data;
		if (newElementIntroData.IntroducedElementIds == null)
		{
			newElementIntroData.IntroducedElementIds = new List<int>();
		}
	}

	public bool HasIntroduced(int elementId)
	{
		if (elementId > 0)
		{
			return data.IntroducedElementIds.Contains(elementId);
		}
		return false;
	}

	public bool MarkIntroduced(int elementId)
	{
		if (elementId <= 0 || HasIntroduced(elementId))
		{
			return false;
		}
		data.IntroducedElementIds.Add(elementId);
		return true;
	}

	public void ResetIntroducedRecords()
	{
		data.IntroducedElementIds.Clear();
	}
}
