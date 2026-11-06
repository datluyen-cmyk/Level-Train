using RotateRings.Services;
using UniLog;
using XingYun.Aot.Lang;
using XingYun.Command;

namespace RotateRings.UI.User;

public class UserSaveNameCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		if (!(param is UserSaveNameRequest userSaveNameRequest))
		{
			Log.LogError("User", "[UserSaveNameCommand] Invalid parameter type");
		}
		else
		{
			if (string.IsNullOrWhiteSpace(userSaveNameRequest.PlayerName))
			{
				return;
			}
			string text = userSaveNameRequest.PlayerName.Trim();
			if (!userSaveNameRequest.SkipValidation)
			{
				if (UserInfoService.IsEnglishFreeNamingEnabled())
				{
					if (!UserInfoService.TryValidate(text, out var _))
					{
						return;
					}
				}
				else if (text.Length < 2 || text.Length > 12)
				{
					return;
				}
			}
			userSaveNameRequest.PlayerName = text;
			((BaseCommand)this).GetProxy<UserProxy>().SaveNameInfo(userSaveNameRequest.PlayerName);
			((BaseCommand)this).SendEvent<UserNameSaveSuccess>(new UserNameSaveSuccess
			{
				PlayerName = userSaveNameRequest.PlayerName
			});
			StyleFontFallbackService.RebuildWithPlayerName(LanguageContext.CurrentLanguage);
		}
	}
}
