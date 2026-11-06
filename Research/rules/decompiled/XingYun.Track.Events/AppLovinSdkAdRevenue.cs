using System;

namespace XingYun.Track.Events;

[Serializable]
public class AppLovinSdkAdRevenue : BaseTrackConfig
{
	public const string EventName = "appLovin_sdk_ad_revenue";

	public double revenue;

	public string adFormat;

	public string s_network_state;

	public string s_ad_create_id;

	public string networkName;

	public string adUnitId;

	public string countryCode;

	public string s_ad_review_id;

	public string sceneID;

	public string placement;

	public string s_event_list;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		revenue = 0.0;
		adFormat = null;
		s_network_state = null;
		s_ad_create_id = null;
		networkName = null;
		adUnitId = null;
		countryCode = null;
		s_ad_review_id = null;
		sceneID = null;
		placement = null;
		s_event_list = null;
	}
}
