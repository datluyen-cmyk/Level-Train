using System;

namespace XingYun.Track.Events;

[Serializable]
public class GSettingAccountRequestresult : BaseTrackConfig
{
	public const string EventName = "g_setting_account_requestresult";

	public string account_action;

	public string provider;

	public string result;

	public string fail_reason;

	public string account_status_before;

	public string account_status_after;

	public bool is_first_bind;

	public string delete_type;

	public string local_data_action;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		account_action = null;
		provider = null;
		result = null;
		fail_reason = null;
		account_status_before = null;
		account_status_after = null;
		is_first_bind = false;
		delete_type = null;
		local_data_action = null;
	}
}
