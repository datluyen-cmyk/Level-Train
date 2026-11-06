using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Hot.Model;
using RotateRing.GamePlay;
using RotateRings.Ad;
using RotateRings.Ad.CommerceLtv;
using RotateRings.Common;
using RotateRings.GamePlay.Services;
using RotateRings.GameWinIQ;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.Services.Account;
using RotateRings.SpecialLevel;
using UnityEngine;
using XingYun.Account;
using XingYun.Aot;
using XingYun.Core.Singletons;
using XingYun.Level;
using XingYun.Track;
using XingYun.Track.Events;

namespace Game.DataTrack;

public class RRDataTrack
{
	public static class SettingAccountAction
	{
		public const string Login = "login";

		public const string Bind = "bind";

		public const string Unbind = "unbind";

		public const string Logout = "logout";

		public const string DeleteAccount = "delete_account";
	}

	public static class SettingAccountProvider
	{
		public const string Apple = "apple";

		public const string Google = "google";

		public const string Device = "device";
	}

	public static class SettingAccountResult
	{
		public const string Success = "success";

		public const string Cancel = "cancel";

		public const string NoNetwork = "no_network";

		public const string Timeout = "timeout";

		public const string Forbidden = "forbidden";
	}

	public static class SettingAccountFailReason
	{
		public const string SdkCancel = "sdk_cancel";

		public const string SdkPasswordError = "sdk_password_error";

		public const string NetworkFail = "network_fail";

		public const string ServerError = "server_error";

		public const string Unknown = "unknown";
	}

	public static class SettingAccountStatus
	{
		public const string GuestOffline = "guest_offline";

		public const string GuestOnline = "guest_online";

		public const string Bound = "bound";

		public const string ResetRequired = "reset_required";
	}

	public static class SettingAccountDeleteType
	{
		public const string ServerData = "server_data";

		public const string FullAccount = "full_account";
	}

	public static class SettingAccountLocalDataAction
	{
		public const string Keep = "keep";

		public const string Clear = "clear";
	}

	public enum AdAction
	{
		Click,
		Show
	}

	public enum ADWindowOperationType
	{
		Show,
		Close,
		Retry,
		Cancel
	}

	public enum AdVisitType
	{
		Inter,
		Reward
	}

	public static class SavedataSyncTriggerNode
	{
		public const string ColdStart = "cold_start";

		public const string LevelEnd = "level_end";

		public const string PurchaseBefore = "purchase_before";

		public const string PurchaseAfter = "purchase_after";

		public const string HomeEntry = "home_entry";

		public const string Login = "log_in";

		public const string OutsideLevelButton = "outside_level_button";
	}

	public static class SavedataSyncResult
	{
		public const string Success = "success";

		public const string Fail = "fail";

		public const string Timeout = "timeout";

		public const string Skipped = "skipped";
	}

	public static class SavedataSyncSaveSource
	{
		public const string Local = "local";

		public const string Server = "server";
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CReportEventAsyncImp_003Ed__56<T> : IAsyncStateMachine where T : BaseTrackConfig
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public T data;

		public RRDataTrack _003C_003E4__this;

		public string eventName;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RRDataTrack rRDataTrack = _003C_003E4__this;
			try
			{
				try
				{
					Awaiter val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_00c6;
					}
					if (data == null)
					{
						rRDataTrack.ReportEvent<BaseTrackConfig>(eventName, new BaseTrackConfig());
					}
					else
					{
						string text = GameApp.Json.Serialize<T>(data);
						if (eventName == "g_game_log" && text.Length > 10000)
						{
							UniTask val2 = rRDataTrack.ReportEventInBatchesAsync(eventName, text);
							val = ((UniTask)(ref val2)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CReportEventAsyncImp_003Ed__56<T>>(ref val, ref this);
								return;
							}
							goto IL_00c6;
						}
						GameApp.Track.ReportEvent(eventName, text);
					}
					goto end_IL_0011;
					IL_00c6:
					((Awaiter)(ref val)).GetResult();
					end_IL_0011:;
				}
				catch (Exception)
				{
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CReportEventInBatchesAsync_003Ed__57 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string json;

		public string eventName;

		private int _003CtotalLength_003E5__2;

		private int _003CbatchCount_003E5__3;

		private int _003Ci_003E5__4;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num != 0)
				{
					_003CtotalLength_003E5__2 = json.Length;
					_003CbatchCount_003E5__3 = (_003CtotalLength_003E5__2 + 10000 - 1) / 10000;
					_003Ci_003E5__4 = 0;
					goto IL_0103;
				}
				Awaiter val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter);
				num = (_003C_003E1__state = -1);
				goto IL_00ea;
				IL_00f1:
				_003Ci_003E5__4++;
				goto IL_0103;
				IL_00ea:
				((Awaiter)(ref val)).GetResult();
				goto IL_00f1;
				IL_0103:
				if (_003Ci_003E5__4 < _003CbatchCount_003E5__3)
				{
					int num2 = _003Ci_003E5__4 * 10000;
					int length = Math.Min(10000, _003CtotalLength_003E5__2 - num2);
					string text = json.Substring(num2, length);
					GameApp.Track.ReportEvent(eventName, text);
					if (_003Ci_003E5__4 < _003CbatchCount_003E5__3 - 1)
					{
						YieldAwaitable val2 = UniTask.Yield();
						val = ((YieldAwaitable)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CReportEventInBatchesAsync_003Ed__57>(ref val, ref this);
							return;
						}
						goto IL_00ea;
					}
					goto IL_00f1;
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private static RRDataTrack instance;

	private GBtnClick btnClickData;

	private GTutorial tutorialData;

	private GGameOpen gameOpenData;

	private GGameStart gameStartData;

	private GGameEnd gameEndData;

	private GGameLog gameLogData;

	private GGetItem getItemData;

	private GUseItem useItemData;

	private LevelModelData levelModelData;

	private GAdRewardEnter adRewardEnterData;

	private GAdVisit adVisitData;

	private SGameAdRevenue gameAdRevenueData;

	private GGameBannerRevenue gameBannerRevenueData;

	private GScreenData screenData;

	private GPageShow pageShowData;

	private GUserSetting userSettingData;

	private GSettingAccountRequestresult settingAccountRequestResultData;

	private GSavedataSyncRequestresult savedataSyncRequestResultData;

	private GChampaignStart champaignStartData;

	private GChampaignEnd champaignEndData;

	private SP1Action p1ActionData;

	private const string AccountTrackLogTag = "[RRDataTrack][Account]";

	private static bool accountUserPropertySyncInited;

	private GamePartTrackSnapshot currentSessionSnapshot;

	private string currentSessionRingMaterial;

	private readonly LegacyGameSessionTrackCollector legacySessionCollector = new LegacyGameSessionTrackCollector();

	private const string LevelEntryBannerShowEventName = "level_entry_banner_show";

	private LevelEntryBannerShowData levelEntryBannerShowData;

	private const string PrivacyVersion = "tu2.0_pp2.0";

	private const string StageAgree = "agree";

	private const string StageTermsClick = "privacy_ser_click";

	private const string StagePrivacyClick = "privacy_click";

	private GameUserRatingPopupShow ratingPopupShowData;

	private GameUserRatingPopupClose ratingPopupCloseData;

	private GameUserRatingStarSelected ratingStarSelectedData;

	private GameUserRatingStarSubmit ratingStarSubmitData;

	private GameUserRatingFeedbackSubmit ratingFeedbackSubmitData;

	private GameUserRatingFeedbackClose ratingFeedbackCloseData;

	public static RRDataTrack Instance => instance ?? (instance = new RRDataTrack());

	private LevelModelData LevelModelData
	{
		get
		{
			if (levelModelData == null)
			{
				levelModelData = GameApp.Proxy.Get<LevelModelProxy>()?.Data;
			}
			return levelModelData;
		}
	}

	private GameModeData GameModeData => LevelModelData?.GameModeData;

	public GBtnClick BtnClickData => GetOrReset(ref btnClickData);

	public GTutorial TutorialData => GetOrReset(ref tutorialData);

	public GGameOpen GameOpenData => GetOrReset(ref gameOpenData);

	public GGameStart GameStartData => GetOrReset(ref gameStartData);

	public GGameEnd GameEndData => GetOrReset(ref gameEndData);

	public GGameLog GameLogData => GetOrReset(ref gameLogData);

	public GGetItem GetItemData => GetOrReset(ref getItemData);

	public GUseItem UseItemData => GetOrReset(ref useItemData);

	public GAdRewardEnter AdRewardEnterData => GetOrReset(ref adRewardEnterData);

	public GAdVisit AdVisitData => GetOrReset(ref adVisitData);

	public GGameBannerRevenue GameBannerRevenueData => GetOrReset(ref gameBannerRevenueData);

	public GScreenData ScreenData => GetOrReset(ref screenData);

	public LegacyGameSessionTrackCollector LegacySessionCollector => legacySessionCollector;

	public bool HasClearedAnyThisSession => legacySessionCollector.AssemblyClearIds.Count > 0;

	public void ReportUserJson(string eventName, string json)
	{
		GameApp.Track.ReportUserJson(eventName, json);
	}

	public void ReportUserInt(string key, int value)
	{
		GameApp.Track.ReportUserInt(key, value);
	}

	public void ReportUserString(string key, string value)
	{
		GameApp.Track.ReportUser(key, value);
	}

	private void ReportEventAsync<T>(string eventName, T data) where T : BaseTrackConfig
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = ReportEventAsyncImp(eventName, data);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CReportEventAsyncImp_003Ed__56<>))]
	private UniTaskVoid ReportEventAsyncImp<T>(string eventName, T data) where T : BaseTrackConfig
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CReportEventAsyncImp_003Ed__56<T> _003CReportEventAsyncImp_003Ed__ = default(_003CReportEventAsyncImp_003Ed__56<T>);
		_003CReportEventAsyncImp_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CReportEventAsyncImp_003Ed__._003C_003E4__this = this;
		_003CReportEventAsyncImp_003Ed__.eventName = eventName;
		_003CReportEventAsyncImp_003Ed__.data = data;
		_003CReportEventAsyncImp_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CReportEventAsyncImp_003Ed__._003C_003Et__builder)).Start<_003CReportEventAsyncImp_003Ed__56<T>>(ref _003CReportEventAsyncImp_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CReportEventAsyncImp_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CReportEventInBatchesAsync_003Ed__57))]
	private UniTask ReportEventInBatchesAsync(string eventName, string json)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CReportEventInBatchesAsync_003Ed__57 _003CReportEventInBatchesAsync_003Ed__ = default(_003CReportEventInBatchesAsync_003Ed__57);
		_003CReportEventInBatchesAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CReportEventInBatchesAsync_003Ed__.eventName = eventName;
		_003CReportEventInBatchesAsync_003Ed__.json = json;
		_003CReportEventInBatchesAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CReportEventInBatchesAsync_003Ed__._003C_003Et__builder)).Start<_003CReportEventInBatchesAsync_003Ed__57>(ref _003CReportEventInBatchesAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CReportEventInBatchesAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ReportEvent<T>(string eventName, T data) where T : BaseTrackConfig
	{
		GameApp.Track.ReportEvent<T>(eventName, data);
	}

	private T GetOrReset<T>(ref T data) where T : BaseTrackConfig, new()
	{
		if (data == null)
		{
			data = new T();
		}
		else
		{
			((BaseTrackConfig)data/*cast due to .constrained prefix*/).Reset();
		}
		return data;
	}

	public float GetSessionTime()
	{
		return SessionManager.Instance.GetElapsedSeconds();
	}

	public void StopSessionTime()
	{
		SessionManager.Instance.Stop();
	}

	private static string Escape(string raw)
	{
		if (!string.IsNullOrEmpty(raw))
		{
			return raw.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
		return string.Empty;
	}

	public static void InitAccountUserPropertySync()
	{
		if (LoginManager.IsFeatureEnabled && !accountUserPropertySyncInited)
		{
			AccountComponent account = GameApp.Account;
			if (!((Object)(object)account == (Object)null))
			{
				accountUserPropertySyncInited = true;
				account.OnLoginSuccess += OnAccountChangedForUserProperty;
				account.OnLogout += ReportAccountUserProperties;
				account.OnUnregisterSuccess += OnUnregisterChangedForUserProperty;
				ReportAccountUserProperties();
			}
		}
	}

	private static void OnAccountChangedForUserProperty(LoginResult _)
	{
		ReportAccountUserProperties();
	}

	private static void OnUnregisterChangedForUserProperty(ulong _)
	{
		ReportAccountUserProperties();
	}

	public static void ReportAccountUserProperties()
	{
		if (LoginManager.IsFeatureEnabled)
		{
			GameApp.Track.ReportUser("nebula_user_id", GetNebulaUserId());
			GameApp.Track.ReportUser("account_status", GetAccountStatus());
			GameApp.Track.ReportUser("bound_account_types", GetBoundAccountTypes());
			GameApp.Track.ReportUser("bound_account_id", GetBoundAccountId());
		}
	}

	public static string GetNebulaUserId()
	{
		AccountComponent account = GameApp.Account;
		if (!((Object)(object)account == (Object)null) && account.Guid != 0L)
		{
			return account.Guid.ToString();
		}
		return string.Empty;
	}

	public static string GetAccountStatus()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account == (Object)null)
		{
			return "guest_offline";
		}
		if ((int)account.CurrentIdentity == 1 && account.IsLoggedIn)
		{
			return "bound";
		}
		if (!account.IsLoggedIn)
		{
			return "guest_offline";
		}
		return "guest_online";
	}

	public static string GetBoundAccountTypes()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account == (Object)null || (int)account.CurrentIdentity != 1)
		{
			return string.Empty;
		}
		return account.PlatformType;
	}

	public static string GetBoundAccountId()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account == (Object)null || (int)account.CurrentIdentity != 1)
		{
			return string.Empty;
		}
		return account.PlatformId;
	}

	public static string GetCurrentProvider()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account == (Object)null || (int)account.CurrentIdentity != 1)
		{
			return "device";
		}
		return NormalizeProvider(account.PlatformType);
	}

	public static string NormalizeProvider(string platformType)
	{
		if (string.IsNullOrEmpty(platformType))
		{
			return "device";
		}
		if (string.Equals(platformType, "google", StringComparison.OrdinalIgnoreCase))
		{
			return "google";
		}
		if (string.Equals(platformType, "device", StringComparison.OrdinalIgnoreCase))
		{
			return "device";
		}
		return "apple";
	}

	public static string GetProviderFromThirdPartyType(ThirdPartyType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)type != 1)
		{
			if (type - 3 <= 1)
			{
				return "google";
			}
			return "device";
		}
		return "apple";
	}

	public static void ReportSettingAccountRequestResult(string action, string result, string statusBefore, string statusAfter, string provider = null, string failReason = null, bool isFirstBind = false, string deleteType = null, string localDataAction = null)
	{
		if (LoginManager.IsFeatureEnabled)
		{
			Instance.ReportSettingAccountRequestResultInternal(action, result, statusBefore, statusAfter, provider, failReason, isFirstBind, deleteType, localDataAction);
		}
	}

	private void ReportSettingAccountRequestResultInternal(string action, string result, string statusBefore, string statusAfter, string provider, string failReason, bool isFirstBind, string deleteType, string localDataAction)
	{
		GSettingAccountRequestresult orReset = GetOrReset(ref settingAccountRequestResultData);
		orReset.account_action = action;
		orReset.provider = NormalizeProvider(provider ?? GetCurrentProvider());
		orReset.result = result;
		orReset.fail_reason = ((result == "success") ? null : failReason);
		orReset.account_status_before = statusBefore;
		orReset.account_status_after = statusAfter;
		orReset.is_first_bind = action == "bind" && isFirstBind;
		orReset.delete_type = ((action == "delete_account") ? deleteType : null);
		orReset.local_data_action = ((action == "delete_account") ? localDataAction : null);
		ReportEvent("g_setting_account_requestresult", orReset);
		ReportAccountUserProperties();
	}

	public void ReportGameAdRevenue(string adType, double revenue, string adPosition)
	{
		if (!(revenue <= 0.0) && !string.IsNullOrEmpty(adType))
		{
			SGameAdRevenue orReset = GetOrReset(ref gameAdRevenueData);
			orReset.ad_type = adType;
			orReset.revenue = revenue;
			orReset.ad_position = adPosition ?? string.Empty;
			if (LevelModelData != null && GameModeData != null)
			{
				orReset.game_mode = GameModeData.game_mode;
				orReset.mode_num = GameModeData.mode_num;
				orReset.mode_num_all = LevelModelData.GetAllModeNum();
				orReset.game_score = GameModeData.game_score;
				orReset.game_time = Mathf.RoundToInt(MonoSingleton<GameTimeTracker>.Instance.TotalGameTime);
				orReset.game_time_real = Mathf.RoundToInt(MonoSingleton<GameTimeTracker>.Instance.ActualGameTime);
			}
			ReportEvent("s_game_ad_revenue", orReset);
		}
	}

	public void ReportAdRewardEnter(string adEnterAction, string adEnterScene, bool adIsReady, int rewardAdNum)
	{
		GAdRewardEnter orReset = GetOrReset(ref adRewardEnterData);
		orReset.ad_enter_action = adEnterAction;
		orReset.ad_enter_scene = adEnterScene;
		orReset.ad_is_ready = (adIsReady ? 1 : 0);
		orReset.session_time = Mathf.RoundToInt(GetSessionTime());
		orReset.reward_ad_num = rewardAdNum;
		if (LevelModelData != null && GameModeData != null)
		{
			orReset.game_mode = GameModeData.game_mode;
			orReset.level = GameModeData.level;
			orReset.mode_num = GameModeData.mode_num;
			orReset.mode_num_all = LevelModelData.GetAllModeNum();
			orReset.mode_winnum = GameModeData.mode_winnum;
			orReset.mode_winnum_all = LevelModelData.GetAllModeWinnum();
			orReset.game_time = Mathf.RoundToInt(MonoSingleton<GameTimeTracker>.Instance.TotalGameTime);
			orReset.game_time_real = Mathf.RoundToInt(MonoSingleton<GameTimeTracker>.Instance.ActualGameTime);
		}
		ReportEvent("g_ad_reward_enter", orReset);
	}

	public void ADWindowOperation(ADWindowOperationType operationType)
	{
		string text = operationType switch
		{
			ADWindowOperationType.Show => "弹窗展现", 
			ADWindowOperationType.Close => "关闭", 
			ADWindowOperationType.Retry => "重试", 
			ADWindowOperationType.Cancel => "取消", 
			_ => string.Empty, 
		};
		if (operationType == ADWindowOperationType.Retry)
		{
			int isAdReady = (GameApp.Ad.IsRewardAdReady ? 1 : 0);
			ReportBtnClick("无法加载广告-主关卡-" + text, isAdReady);
		}
		else
		{
			ReportBtnClick("无法加载广告-主关卡-" + text);
		}
	}

	public void ReportAdRewardEnter(AdAction action, AdPopPosition adPopPosition, bool fromWaitPopup = false)
	{
		string text = action switch
		{
			AdAction.Click => "click", 
			AdAction.Show => "show", 
			_ => null, 
		};
		if (text != null)
		{
			string adEnterScene = (fromWaitPopup ? AdPositionMap.GetRewardWaitAdPosition(adPopPosition) : AdPositionMap.GetAdPosition(adPopPosition));
			int valueOrDefault = (GameApp.Proxy.Get<PlayerModelProxy>()?.Data?.GetAdRewardCount).GetValueOrDefault();
			ReportAdRewardEnter(text, adEnterScene, GameApp.Ad.IsRewardAdReady, valueOrDefault);
		}
	}

	private int GetAdIsReady(AdVisitType adVisitType)
	{
		return adVisitType switch
		{
			AdVisitType.Inter => GameApp.Ad.IsInterAdReady ? 1 : 0, 
			AdVisitType.Reward => GameApp.Ad.IsRewardAdReady ? 1 : 0, 
			_ => -1, 
		};
	}

	private string GetAdVisitAction(AdVisitType adVisitType)
	{
		return adVisitType switch
		{
			AdVisitType.Inter => "inter", 
			AdVisitType.Reward => "reward", 
			_ => string.Empty, 
		};
	}

	public void ReportAdVisit(AdVisitType adVisitType, string adVisitScene, string adVisitAction = "play")
	{
		GAdVisit orReset = GetOrReset(ref adVisitData);
		orReset.ad_type = GetAdVisitAction(adVisitType);
		orReset.ad_visit_scene = adVisitScene;
		orReset.ad_visit_action = adVisitAction;
		orReset.ad_is_ready = GetAdIsReady(adVisitType);
		orReset.session_time = Mathf.RoundToInt(GetSessionTime());
		if (LevelModelData != null && GameModeData != null)
		{
			orReset.game_mode = GameModeData.game_mode;
			orReset.level = GameModeData.level;
			orReset.mode_num = GameModeData.mode_num;
			orReset.mode_num_all = LevelModelData.GetAllModeNum();
			orReset.mode_winnum = GameModeData.mode_winnum;
			orReset.mode_winnum_all = LevelModelData.GetAllModeWinnum();
			orReset.game_time = Mathf.RoundToInt(MonoSingleton<GameTimeTracker>.Instance.TotalGameTime);
			orReset.game_time_real = Mathf.RoundToInt(MonoSingleton<GameTimeTracker>.Instance.ActualGameTime);
		}
		ReportEvent("g_ad_visit", orReset);
	}

	public void ReportBannerRevenue()
	{
		double bannerRevenueSum = SDKManager.GetBannerRevenueSum();
		if (!(bannerRevenueSum <= 0.0))
		{
			GGameBannerRevenue orReset = GetOrReset(ref gameBannerRevenueData);
			orReset.revenue = bannerRevenueSum;
			orReset.game_mode = GameModeData?.game_mode ?? 0;
			ReportEvent("g_game_banner_revenue", orReset);
		}
	}

	public void ReportBtnClick(string clickType, int isAdReady = -1)
	{
		GBtnClick orReset = GetOrReset(ref btnClickData);
		orReset.click_type = clickType;
		orReset.ad_is_ready = isAdReady;
		ReportEvent("g_btn_click", orReset);
	}

	public void ReportChestClaimPopupShow()
	{
		ReportBtnClick("关卡宝箱-[" + GetChestModeDisplayName() + "]-领取奖励-弹窗展现");
	}

	public void ReportChestClaimByAd()
	{
		ReportBtnClick("关卡宝箱-[" + GetChestModeDisplayName() + "]-领取奖励-看广告领取");
	}

	public void ReportChestClaimNormal()
	{
		ReportBtnClick("关卡宝箱-[" + GetChestModeDisplayName() + "]-领取奖励-普通领取");
	}

	public void ReportBoardPropClick(EPropType propType)
	{
		ReportBtnClick("盘面-[" + GetGameModeDisplayName() + "]-" + GetPropDisplayName(propType) + "-选择");
	}

	public void ReportCancelUsePropClick(EPropType propType)
	{
		ReportBtnClick("盘面-[" + GetGameModeDisplayName() + "]-" + GetPropDisplayName(propType) + "-取消");
	}

	private static string GetPropDisplayName(EPropType propType)
	{
		return propType switch
		{
			EPropType.Hammer => "锤子", 
			EPropType.Skyrocket => "小礼炮", 
			_ => propType.ToString(), 
		};
	}

	private static string GetGameModeDisplayName()
	{
		if (GameInfo.CurrentGameMode == GameMode.Main)
		{
			return "主关卡";
		}
		if (GameInfo.CurrentGameMode == GameMode.Endless)
		{
			return "无尽模式";
		}
		if (!SpecialLevelModule.TryGetDisplayName(GameInfo.CurrentGameMode, out var displayName))
		{
			return string.Empty;
		}
		return displayName;
	}

	private static string GetChestModeDisplayName()
	{
		if (GameInfo.CurrentGameMode == GameMode.Main)
		{
			return "主关卡模式";
		}
		if (GameInfo.CurrentGameMode == GameMode.Endless)
		{
			return "无尽模式";
		}
		if (!SpecialLevelModule.TryGetDisplayName(GameInfo.CurrentGameMode, out var displayName))
		{
			return string.Empty;
		}
		return displayName;
	}

	public void ReportGameEnd(string status)
	{
		ReportGameEnd(status, default(GamePartTrackSnapshot));
	}

	public void ReportGameEnd(string status, GamePartTrackSnapshot remainingSnapshot)
	{
		if (!(status != "成功") || !(status != "重新开始") || !(status != "中途退出") || !(status != "失败"))
		{
			legacySessionCollector.CommitBombBatch();
			SingletonComponent<GameplayManager>.Instance?.GetGameService<RotateCameraGestureController>()?.OnLegacySessionTrackEnding();
			legacySessionCollector.EndSession();
			LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
			if (status == "成功")
			{
				GameTrackStatsHelper.IncrementModeWinStreakOnSuccess(levelModelProxy?.Data);
				GameTrackStatsHelper.IncrementModeWinStreakDayOnSuccess(levelModelProxy?.Data);
			}
			else if (status == "重新开始")
			{
				GameTrackStatsHelper.ResetModeWinStreakOnRestart(levelModelProxy?.Data);
				GameTrackStatsHelper.ResetModeWinStreakDayOnRestart(levelModelProxy?.Data);
			}
			GGameEnd gGameEnd = GameEndData;
			gGameEnd.status = status;
			FillCommonGameFields((BaseTrackConfig)(object)gGameEnd);
			FillEndPartFields(gGameEnd, remainingSnapshot);
			if (levelModelProxy != null)
			{
				gGameEnd.game_score = levelModelProxy.GetCurrentScore();
			}
			GameTrackStatsHelper.ApplyEndGameMaxScores(gGameEnd, gGameEnd.game_score);
			FillGameTimeFields(out var gameTime, out var gameTimeReal);
			gGameEnd.game_time = gameTime;
			gGameEnd.game_time_real = gameTimeReal;
			StopGameTimeTracking(status);
			levelModelProxy?.SetGameEndState();
			gGameEnd.iq = GameWinIQService.Instance.GetTrackBaseAndDisplayScore();
			ReportEvent("g_game_end", gGameEnd);
			CommerceLtvCooldownHooks.OnGameEnded();
			GameTrackStatsHelper.RecordSettledGame(levelModelProxy?.Data, status == "成功", gGameEnd.game_score);
		}
	}

	internal static bool ShouldSkipDuplicateGameEnd(string status, bool isLevelCompleted, LevelModelProxy levelProxy)
	{
		if (status == "成功")
		{
			return false;
		}
		if (isLevelCompleted)
		{
			return true;
		}
		return levelProxy?.IsGameEndState() ?? false;
	}

	public void ReportGameFailure(GamePartTrackSnapshot remainingSnapshot)
	{
	}

	public void ReportGameOpen()
	{
		GGameOpen gGameOpen = GameOpenData;
		if (GameModeData != null)
		{
			gGameOpen.game_mode = GameModeData.game_mode;
		}
		gGameOpen.is_first_open = (GameApp.Proxy.Get<LevelModelProxy>().IsFirstEnterCurrentLevel() ? 1 : 0);
		ReportEvent("g_game_open", gGameOpen);
	}

	public void ReportGameStart(GamePartTrackSnapshot partSnapshot, bool isNewGame = true, string status = null)
	{
		currentSessionSnapshot = partSnapshot;
		currentSessionRingMaterial = ResolveRingMaterial();
		legacySessionCollector.BeginSession();
		SingletonComponent<GameplayManager>.Instance?.GetGameService<RotateCameraGestureController>()?.OnLegacySessionTrackBegan();
		GGameStart gGameStart = GameStartData;
		gGameStart.status = ResolveGameStartStatus(isNewGame, status);
		FillCommonGameFields((BaseTrackConfig)(object)gGameStart);
		FillStartPartFields(gGameStart, partSnapshot);
		gGameStart.camera_range = legacySessionCollector.GetSessionCameraRange();
		GameApp.Proxy.Get<LevelModelProxy>()?.SetGameBeginState();
		BeginGameTimeTracking(gGameStart.status == "继续");
		ReportEvent("g_game_start", gGameStart);
	}

	public void ReportGameStart()
	{
		ReportGameStart(default(GamePartTrackSnapshot));
	}

	private void FillCommonGameFields(BaseTrackConfig data)
	{
		LevelModelData levelModelData = LevelModelData;
		GameModeData gameModeData = GameModeData;
		if (gameModeData == null || levelModelData == null)
		{
			return;
		}
		GameTrackStatsHelper.FillModeStats(levelModelData, out var modeWinnum, out var modeWinnumAll, out var modeLast10WinRate, out var modeWinRate, out var winRate, out var modeWinStreak, out var modeLast10MaxScore, out var modeMaxScore, out var maxScore, out var modeWinStreakDay);
		if (data is GGameStart gGameStart)
		{
			gGameStart.level = gameModeData.level;
			gGameStart.level_id = LevelConfigService.GetCoreLevelPathKey(gameModeData.level);
			gGameStart.puzzle_library = ResolvePuzzleLibrary(gameModeData.level);
			gGameStart.game_mode = GameTrackStatsHelper.GetGameMode(gameModeData);
			gGameStart.game_mode_name = GameTrackStatsHelper.GetGameModeName(gameModeData);
			gGameStart.game_play_method = 0;
			gGameStart.mode_num = gameModeData.mode_num;
			gGameStart.mode_num_all = levelModelData.GetAllModeNum();
			gGameStart.mode_winnum = modeWinnum;
			gGameStart.mode_winnum_all = modeWinnumAll;
			gGameStart.mode_last_10_win_rate = modeLast10WinRate;
			gGameStart.mode_win_rate = modeWinRate;
			gGameStart.win_rate = winRate;
			gGameStart.mode_win_streak = modeWinStreak;
			gGameStart.mode_last_10_max_score = modeLast10MaxScore;
			gGameStart.mode_max_score = modeMaxScore;
			gGameStart.max_score = maxScore;
			gGameStart.hammer_num = GetPropCount(EPropType.Hammer);
			gGameStart.skyrocket_num = GetPropCount(EPropType.Skyrocket);
			gGameStart.ring_material = currentSessionRingMaterial;
			if (gameModeData.StepLimitInitialSteps > 0)
			{
				gGameStart.initial_steps = gameModeData.StepLimitInitialSteps;
			}
		}
		else if (data is GGameEnd gGameEnd)
		{
			gGameEnd.level = gameModeData.level;
			gGameEnd.level_id = LevelConfigService.GetCoreLevelPathKey(gameModeData.level);
			gGameEnd.puzzle_library = ResolvePuzzleLibrary(gameModeData.level);
			gGameEnd.game_mode = GameTrackStatsHelper.GetGameMode(gameModeData);
			gGameEnd.game_mode_name = GameTrackStatsHelper.GetGameModeName(gameModeData);
			gGameEnd.game_play_method = 0;
			gGameEnd.mode_num = gameModeData.mode_num;
			gGameEnd.mode_num_all = levelModelData.GetAllModeNum();
			gGameEnd.mode_winnum = modeWinnum;
			gGameEnd.mode_winnum_all = modeWinnumAll;
			gGameEnd.mode_last_10_win_rate = modeLast10WinRate;
			gGameEnd.mode_win_rate = modeWinRate;
			gGameEnd.win_rate = winRate;
			gGameEnd.mode_win_streak = modeWinStreak;
			gGameEnd.mode_win_streak_day = modeWinStreakDay;
			gGameEnd.mode_last_10_max_score = modeLast10MaxScore;
			gGameEnd.mode_max_score = modeMaxScore;
			gGameEnd.max_score = maxScore;
			gGameEnd.hammer_num = GetPropCount(EPropType.Hammer);
			gGameEnd.hammer_use_num = GetPropUseCount(EPropType.Hammer);
			gGameEnd.hammer_ad_num = GetPropUseCount(EPropType.Hammer, TrackPropUseReason.Ad);
			gGameEnd.skyrocket_num = GetPropCount(EPropType.Skyrocket);
			gGameEnd.skyrocket_use_num = GetPropUseCount(EPropType.Skyrocket);
			gGameEnd.peak_trial_streak = -1;
			gGameEnd.ring_material = currentSessionRingMaterial;
		}
	}

	private static void FillStartPartFields(GGameStart startData, GamePartTrackSnapshot snapshot)
	{
		startData.assembly_num_start = snapshot.AssemblyNumStart;
		startData.assembly_num_array_start = ((snapshot.AssemblyNumStart > 0) ? snapshot.BuildAssemblyNumArray() : null);
		startData.level_element = snapshot.LevelElementArray;
	}

	private void FillEndPartFields(GGameEnd endData, GamePartTrackSnapshot remainingSnapshot)
	{
		endData.assembly_num_start = currentSessionSnapshot.AssemblyNumStart;
		endData.assembly_num_end = remainingSnapshot.AssemblyNumEnd;
		endData.assembly_num_array_start = ((currentSessionSnapshot.AssemblyNumStart > 0) ? currentSessionSnapshot.BuildAssemblyNumArray() : null);
		endData.assembly_num_array_end = ((remainingSnapshot.AssemblyNumEnd > 0) ? remainingSnapshot.BuildAssemblyNumArray() : null);
		endData.level_element = currentSessionSnapshot.LevelElementArray;
		legacySessionCollector.CopyTo(out var clearIds, out var clearTimes, out var hammerStepArray, out var hammerAssemblyArray, out var bombStepArray, out var bombNumArray, out var bombAssemblyArray);
		endData.assembly_clear_id_array = clearIds;
		endData.assembly_clear_time_array = clearTimes;
		endData.move_time_array = legacySessionCollector.GetMoveTimeArray();
		endData.hammer_step_array = hammerStepArray;
		endData.hammer_assembly_array = hammerAssemblyArray;
		endData.bomb_step_array = bombStepArray;
		endData.bomb_num_array = bombNumArray;
		endData.bomb_assembly_array = bombAssemblyArray;
		endData.first_operation_time = legacySessionCollector.FirstOperationTimeSeconds;
		endData.board_scale = legacySessionCollector.GetBoardScaleArray();
		endData.camera_range = legacySessionCollector.GetSessionCameraRange();
		endData.restore_num = legacySessionCollector.GetRestoreButtonAppearCount();
		GameModeData gameModeData = GameModeData;
		if (gameModeData != null && gameModeData.StepLimitInitialSteps > 0)
		{
			endData.initial_steps = gameModeData.StepLimitInitialSteps;
			endData.used_steps = gameModeData.StepLimitUsedSteps;
			endData.add_steps_times = gameModeData.StepLimitAdAddSteps;
		}
	}

	private static string ResolvePuzzleLibrary(int level)
	{
		return RandomSelectLevelService.GetMatchedCsvFor(level);
	}

	private static string ResolveRingMaterial()
	{
		GameplayManager gameplayManager = SingletonComponent<GameplayManager>.Instance;
		if ((Object)(object)gameplayManager == (Object)null)
		{
			return null;
		}
		return gameplayManager.PartStyle?.GetTrackRingMaterialName();
	}

	private static int GetPropCount(EPropType propType)
	{
		return GameApp.Proxy.Get<PropModelProxy>()?.GetPropCount(propType) ?? 0;
	}

	private static int GetPropUseCount(EPropType propType)
	{
		return GameApp.Proxy.Get<LevelModelProxy>()?.GetPropUseCount(propType) ?? 0;
	}

	private static int GetPropUseCount(EPropType propType, TrackPropUseReason reason)
	{
		return GameApp.Proxy.Get<LevelModelProxy>()?.GetPropUseCount(propType, reason) ?? 0;
	}

	private string ResolveGameStartStatus(bool isNewGame, string status)
	{
		if (!string.IsNullOrEmpty(status))
		{
			return status;
		}
		if (!isNewGame)
		{
			return "继续";
		}
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null || !levelModelProxy.IsFirstEnterCurrentLevel())
		{
			return "重新开始";
		}
		return "新开";
	}

	private void BeginGameTimeTracking(bool continueSession)
	{
		string text = GameModeData?.game_mode.ToString() ?? "0";
		if (continueSession)
		{
			MonoSingleton<GameTimeTracker>.Instance.ContinueTracking(text);
		}
		else
		{
			MonoSingleton<GameTimeTracker>.Instance.StartTracking(text);
		}
		bool num = MainStageGameTimeGate.IsValidDurationGameMode(text);
		MainStageGameTimeGate.SetMainLevelSessionTracking(num, tracking: true);
		if (num)
		{
			AdColdLaunchGameTimeTracker.BeginAttempt(MonoSingleton<GameTimeTracker>.Instance.ActualGameTime);
		}
	}

	private void StopGameTimeTracking(string status)
	{
		bool flag = status == "成功";
		string text = GameModeData?.game_mode.ToString() ?? "0";
		bool isMainLevelSessionTracking = MainStageGameTimeGate.IsMainLevelSessionTracking;
		MainStageGameTimeGate.SetMainLevelSessionTracking(isMainLevelSessionTracking, tracking: false);
		MonoSingleton<GameTimeTracker>.Instance.StopTracking(flag, text);
		if (isMainLevelSessionTracking)
		{
			AdColdLaunchGameTimeTracker.EndAttempt(MonoSingleton<GameTimeTracker>.Instance.ActualGameTime);
		}
	}

	private static void FillGameTimeFields(out int gameTime, out int gameTimeReal)
	{
		gameTime = Mathf.CeilToInt(MonoSingleton<GameTimeTracker>.Instance.TotalGameTime);
		gameTimeReal = Mathf.CeilToInt(MonoSingleton<GameTimeTracker>.Instance.ActualGameTime);
	}

	public void ReportLevelEntryBannerShow(int level, ELevelDifficulty difficulty, int styleId, string langKey)
	{
		LevelEntryBannerShowData orReset = GetOrReset(ref levelEntryBannerShowData);
		orReset.level = level;
		orReset.difficulty = difficulty.ToString();
		orReset.style_id = styleId;
		orReset.banner_key = langKey ?? string.Empty;
		ReportEvent("level_entry_banner_show", orReset);
	}

	public void ReportP1Action(string stage, string version = null)
	{
		SP1Action orReset = GetOrReset(ref p1ActionData);
		orReset.s_stage = stage;
		orReset.version = version;
		ReportEvent("s_p1_action", orReset);
	}

	public void ReportPrivacyAgree()
	{
		ReportP1Action("agree", "tu2.0_pp2.0");
	}

	public void ReportPrivacyTermsClick()
	{
		ReportP1Action("privacy_ser_click");
	}

	public void ReportPrivacyPolicyClick()
	{
		ReportP1Action("privacy_click");
	}

	public void ReportPageShow(string show_type)
	{
		GPageShow orReset = GetOrReset(ref pageShowData);
		orReset.show_type = show_type;
		ReportEvent("g_page_show", orReset);
	}

	public void ReportChampaignStart(int champaignNum, int initActiveDays, int initTotalNum, int champaignInitRank)
	{
		GChampaignStart orReset = GetOrReset(ref champaignStartData);
		orReset.champaign_num = champaignNum;
		orReset.init_active_days = initActiveDays;
		orReset.init_total_num = initTotalNum;
		orReset.champaign_init_rank = champaignInitRank;
		ReportEvent("g_champaign_start", orReset);
	}

	public void ReportChampaignEnd(int champaignNum, int endActiveDays, int endTotalNum, int champaignEndRank, int champaignEndScore, int champaignEndScore1st, int champaignEndScore2nd, int champaignEndScore3rd, int champaignEndScore10th, int champaignEndGameCnt, int champaignEndCollectCardsNum)
	{
		GChampaignEnd orReset = GetOrReset(ref champaignEndData);
		orReset.champaign_num = champaignNum;
		orReset.end_active_days = endActiveDays;
		orReset.end_total_num = endTotalNum;
		orReset.champaign_end_rank = champaignEndRank;
		orReset.champaign_end_score = champaignEndScore;
		orReset.champaign_end_score_1st = champaignEndScore1st;
		orReset.champaign_end_score_2nd = champaignEndScore2nd;
		orReset.champaign_end_score_3rd = champaignEndScore3rd;
		orReset.champaign_end_score_10th = champaignEndScore10th;
		orReset.champaign_end_game_cnt = champaignEndGameCnt;
		orReset.champaign_end_collect_cards_num = champaignEndCollectCardsNum;
		ReportEvent("g_champaign_end", orReset);
	}

	public void ReportRatingPopupShow(int animalStyle = 1)
	{
		GameUserRatingPopupShow orReset = GetOrReset(ref ratingPopupShowData);
		ReportEvent("game_user_rating_popup_show", orReset);
	}

	public void ReportRatingPopupClose(int animalStyle = 1)
	{
		GameUserRatingPopupClose orReset = GetOrReset(ref ratingPopupCloseData);
		ReportEvent("game_user_rating_popup_close", orReset);
	}

	public void ReportRatingStarSelected(int rating)
	{
		GameUserRatingStarSelected orReset = GetOrReset(ref ratingStarSelectedData);
		orReset.rating = rating;
		ReportEvent("game_user_rating_star_selected", orReset);
	}

	public void ReportRatingStarSubmit(int rating)
	{
		GameUserRatingStarSubmit orReset = GetOrReset(ref ratingStarSubmitData);
		orReset.rating = rating;
		ReportEvent("game_user_rating_star_submit", orReset);
	}

	public void ReportRatingFeedbackSubmit(string comment)
	{
		GameUserRatingFeedbackSubmit orReset = GetOrReset(ref ratingFeedbackSubmitData);
		orReset.comment = comment;
		ReportEvent("game_user_rating_feedback_submit", orReset);
	}

	public void ReportRatingFeedbackClose(string comment)
	{
		GameUserRatingFeedbackClose orReset = GetOrReset(ref ratingFeedbackCloseData);
		orReset.comment = comment;
		ReportEvent("game_user_rating_feedback_close", orReset);
	}

	public void ReportSavedataSyncRequestResult(string triggerNode, string result, int durationMs, CloudSaveDataSnapshot localSnapshot = null, CloudSaveDataSnapshot cloudSnapshot = null, CloudSaveDataSnapshot finalSnapshot = null, string selectedSaveSource = null, string failReason = null, bool isConflict = false)
	{
		GSavedataSyncRequestresult orReset = GetOrReset(ref savedataSyncRequestResultData);
		orReset.trigger_node = triggerNode;
		orReset.result = result;
		orReset.fail_reason = failReason;
		orReset.duration_ms = Math.Max(0, durationMs);
		orReset.is_conflict = isConflict;
		orReset.selected_save_source = selectedSaveSource;
		orReset.local_level = localSnapshot?.MainLevelId ?? 0;
		orReset.cloud_level = cloudSnapshot?.MainLevelId ?? 0;
		orReset.final_level = finalSnapshot?.MainLevelId ?? 0;
		orReset.local_prop = FormatProps(localSnapshot);
		orReset.cloud_prop = FormatProps(cloudSnapshot);
		orReset.final_prop = FormatProps(finalSnapshot);
		ReportEvent("g_savedata_sync_requestresult", orReset);
	}

	private static string FormatProps(CloudSaveDataSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (snapshot.PropCounts != null)
		{
			for (int i = 0; i < snapshot.PropCounts.Count; i++)
			{
				CloudSavePropCount cloudSavePropCount = snapshot.PropCounts[i];
				if (cloudSavePropCount != null)
				{
					AppendCount(stringBuilder, cloudSavePropCount.PropId, cloudSavePropCount.Count, stringBuilder.Length == 0);
				}
			}
		}
		stringBuilder.Append('$');
		int length = stringBuilder.Length;
		if (snapshot.SyncedPurchaseCounts != null)
		{
			for (int j = 0; j < snapshot.SyncedPurchaseCounts.Count; j++)
			{
				CloudSavePurchaseCount cloudSavePurchaseCount = snapshot.SyncedPurchaseCounts[j];
				if (cloudSavePurchaseCount != null)
				{
					AppendCount(stringBuilder, cloudSavePurchaseCount.PropId, cloudSavePurchaseCount.Count, stringBuilder.Length == length);
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static void AppendCount(StringBuilder builder, int propId, int count, bool isFirst)
	{
		if (!isFirst)
		{
			builder.Append('|');
		}
		builder.Append(propId).Append(':').Append(count);
	}

	public void ReportScreenData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		PhysicsScreenSize physicsScreenSize = SDKManager.GetPhysicsScreenSize();
		GScreenData gScreenData = ScreenData;
		gScreenData.physics_width = physicsScreenSize.PhysicsWidth;
		gScreenData.physics_height = physicsScreenSize.PhysicsHeight;
		gScreenData.physics_unit_per100 = physicsScreenSize.PhysicsUnitPer100;
		gScreenData.screen_unit_per100 = physicsScreenSize.ScreenUnitPer100;
		gScreenData.screen_data_is_trust = (physicsScreenSize.IsTrust ? "true" : "false");
		ReportEvent("g_screen_data", gScreenData);
	}

	public void ReportTutorial(int stage, int stageType, string stageName, int gameMode = 0)
	{
		GTutorial gTutorial = TutorialData;
		gTutorial.game_mode = gameMode;
		gTutorial.stage = stage;
		gTutorial.stage_type = stageType;
		gTutorial.stage_name = stageName;
		ReportEvent("g_tutorial", gTutorial);
	}

	public void ReportUseItem(string itemId, int number, int beforeNum, int afterNum)
	{
		GUseItem orReset = GetOrReset(ref useItemData);
		orReset.item_id = itemId;
		orReset.number = number;
		orReset.before_num = beforeNum;
		orReset.after_num = afterNum;
		LevelModelData levelModelData = GameApp.Proxy.Get<LevelModelProxy>()?.Data;
		GameModeData gameModeData = levelModelData?.GameModeData;
		if (levelModelData != null && gameModeData != null)
		{
			orReset.game_mode = gameModeData.game_mode;
			orReset.level = gameModeData.level;
			orReset.mode_num = gameModeData.mode_num;
			orReset.mode_num_all = levelModelData.GetAllModeNum();
		}
		ReportEvent("g_use_item", orReset);
	}

	public void ReportGetItem(string itemId, int number, int beforeNum, int afterNum, string reasonId, string reasonInfo)
	{
		GGetItem orReset = GetOrReset(ref getItemData);
		orReset.item_id = itemId;
		orReset.number = number;
		orReset.before_num = beforeNum;
		orReset.after_num = afterNum;
		orReset.reason_id = reasonId;
		orReset.reason_info = reasonInfo;
		LevelModelData levelModelData = GameApp.Proxy.Get<LevelModelProxy>()?.Data;
		GameModeData gameModeData = levelModelData?.GameModeData;
		if (levelModelData != null && gameModeData != null)
		{
			orReset.game_mode = gameModeData.game_mode;
			orReset.level = gameModeData.level;
			orReset.mode_num = gameModeData.mode_num;
			orReset.mode_num_all = levelModelData.GetAllModeNum();
		}
		ReportEvent("g_get_item", orReset);
	}

	public void ReportUserSettingLanguage(string languageCode)
	{
		ReportUserSetting(languageCode);
	}

	public void ReportUserSettingAudioSnapshot(int music, int sound, int vibration)
	{
		ReportUserSetting(null, music, sound, vibration);
	}

	public void ReportUserSettingHomeCloseSnapshot(int music, int sound, int vibration, string avatarId)
	{
		ReportUserSetting(null, music, sound, vibration, avatarId);
	}

	public void ReportUserSettingAvatar(string avatarId)
	{
		ReportUserSetting(null, null, null, null, avatarId);
	}

	public void ReportUserSettingDisplayMode(string displayMode)
	{
		ReportUserSetting(null, null, null, null, null, displayMode);
	}

	private void ReportUserSetting(string language = null, int? music = null, int? sound = null, int? vibration = null, string avatarId = null, string displayMode = null)
	{
		GUserSetting orReset = GetOrReset(ref userSettingData);
		if (language != null)
		{
			orReset.language = language;
		}
		if (music.HasValue)
		{
			orReset.music = music.Value;
		}
		if (sound.HasValue)
		{
			orReset.sound = sound.Value;
		}
		if (vibration.HasValue)
		{
			orReset.vibration = vibration.Value;
		}
		if (avatarId != null)
		{
			orReset.avatar_id = avatarId;
		}
		if (displayMode != null)
		{
			orReset.display_mode = displayMode;
		}
		ReportEvent("g_user_setting", orReset);
	}
}
