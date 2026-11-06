using XingYun.Proxy;

namespace RotateRings.Model;

public class GameBordPartStyleModelProxy : BaseProxy
{
	public GameBordPartStyleModelData Data { get; }

	public GameBordPartStyleModelProxy()
		: base("GameBordPartStyleModelProxy")
	{
		Data = GameApp.Save.GetOrCreate<GameBordPartStyleModelData>();
	}
}
