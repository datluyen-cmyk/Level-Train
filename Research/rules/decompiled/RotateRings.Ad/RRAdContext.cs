using System.Collections.Generic;
using RotateRings.Model;

namespace RotateRings.Ad;

public class RRAdContext
{
	public int Level { get; set; }

	public int GameMode { get; set; }

	public int ModeNum { get; set; }

	public int ModeNumAll { get; set; }

	public int GameScore { get; set; }

	public int GameTime { get; set; }

	public int GameTimeReal { get; set; }

	public int Step { get; set; }

	public Dictionary<string, object> ExtraData { get; set; }

	public static RRAdContext FromLevelProxy(int? levelOverride = null)
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy?.Data?.GameModeData == null)
		{
			return null;
		}
		GameModeData gameModeData = levelModelProxy.Data.GameModeData;
		return new RRAdContext
		{
			Level = (levelOverride ?? gameModeData.level),
			GameMode = gameModeData.game_mode,
			ModeNum = gameModeData.mode_num,
			ModeNumAll = gameModeData.mode_num,
			GameScore = gameModeData.game_score,
			GameTime = gameModeData.game_time,
			GameTimeReal = gameModeData.game_time_real,
			Step = levelModelProxy.GetAllStep()
		};
	}
}
