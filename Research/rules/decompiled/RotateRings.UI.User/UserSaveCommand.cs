using Game.DataTrack;
using RotateRings.Common;
using RotateRings.Services;
using UniLog;
using XingYun.Aot.Lang;
using XingYun.Command;

namespace RotateRings.UI.User;

public class UserSaveCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		if (!(param is UserSaveRequest userSaveRequest))
		{
			Log.LogError("User", "[UserSaveCommand] Invalid parameter type");
			return;
		}
		int headId = PlayerService.GetHeadId();
		((BaseCommand)this).GetProxy<UserProxy>().SaveUserInfo(userSaveRequest.HeadId, userSaveRequest.HeadFrameId, userSaveRequest.PlayerName);
		if (userSaveRequest.HeadId != headId)
		{
			RRDataTrack.Instance.ReportUserSettingAvatar(UserSettingTrackHelper.ToAvatarTrackId(userSaveRequest.HeadId));
		}
		((BaseCommand)this).SendEvent<UserSaveSuccess>(new UserSaveSuccess
		{
			PlayerName = userSaveRequest.PlayerName,
			HeadId = userSaveRequest.HeadId,
			HeadFrameId = userSaveRequest.HeadFrameId,
			HeadSprite = PlayerService.GetHeadSprite(userSaveRequest.HeadId),
			HeadFrameSprite = PlayerService.GetHeadFrameSprite(userSaveRequest.HeadFrameId)
		});
		StyleFontFallbackService.RebuildWithPlayerName(LanguageContext.CurrentLanguage);
	}
}
