using RotateRings.Services.Account;

namespace RotateRings.Account;

public static class AccountBootstrap
{
	public static void EnsureInitialized()
	{
		LoginManager.Init();
	}
}
