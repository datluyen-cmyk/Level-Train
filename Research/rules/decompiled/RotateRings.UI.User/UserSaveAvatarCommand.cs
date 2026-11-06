using Game.DataTrack;
using RotateRings.Common;
using UniLog;
using XingYun.Command;

namespace RotateRings.UI.User;

public class UserSaveAvatarCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		if (!(param is UserSaveAvatarRequest userSaveAvatarRequest))
		{
			Log.LogError("User", "[UserSaveAvatarCommand] Invalid parameter type");
			return;
		}
		int headId = PlayerService.GetHeadId();
		((BaseCommand)this).GetProxy<UserProxy>().SaveAvatarInfo(userSaveAvatarRequest.HeadId, userSaveAvatarRequest.HeadFrameId);
		if (userSaveAvatarRequest.HeadId != headId)
		{
			RRDataTrack.Instance.ReportUserSettingAvatar(UserSettingTrackHelper.ToAvatarTrackId(userSaveAvatarRequest.HeadId));
		}
		((BaseCommand)this).SendEvent<UserAvatarSaveSuccess>(new UserAvatarSaveSuccess
		{
			HeadId = userSaveAvatarRequest.HeadId,
			HeadFrameId = userSaveAvatarRequest.HeadFrameId,
			HeadSprite = PlayerService.GetHeadSprite(userSaveAvatarRequest.HeadId),
			HeadFrameSprite = PlayerService.GetHeadFrameSprite(userSaveAvatarRequest.HeadFrameId)
		});
	}
}
