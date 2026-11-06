namespace RotateRings.Services.Account;

public sealed class EventLoginSucceed
{
	public bool IsUserInitiated;

	public bool IsLogout;

	public static void Send(bool isUserInitiated = false, bool isLogout = false)
	{
		GameApp.Event.Send<EventLoginSucceed>(new EventLoginSucceed
		{
			IsUserInitiated = isUserInitiated,
			IsLogout = isLogout
		});
	}
}
