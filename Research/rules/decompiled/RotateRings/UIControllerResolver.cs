using System;
using Game.UI.Language;
using Game.UI.RankListPopup;
using RotateRings.UI.AboutUs;
using RotateRings.UI.AdPop;
using RotateRings.UI.BuyReorder;
using RotateRings.UI.BuySuccess;
using RotateRings.UI.ClaimReward;
using RotateRings.UI.ConnectionLost;
using RotateRings.UI.DailyFirstWin;
using RotateRings.UI.Game;
using RotateRings.UI.GameDifficultyToast;
using RotateRings.UI.GameEnterToast;
using RotateRings.UI.Home;
using RotateRings.UI.HowToPlay;
using RotateRings.UI.LevelEnd;
using RotateRings.UI.LevelStepLimit;
using RotateRings.UI.LockOverlay;
using RotateRings.UI.MultiTouchGuide;
using RotateRings.UI.NewElementIntro;
using RotateRings.UI.Prop;
using RotateRings.UI.PropGuide;
using RotateRings.UI.PropUnlock;
using RotateRings.UI.Rating;
using RotateRings.UI.ReissueReward;
using RotateRings.UI.RestartRetention;
using RotateRings.UI.RewardedLuckyDraw;
using RotateRings.UI.Setting;
using RotateRings.UI.SettingGame;
using RotateRings.UI.Shop;
using RotateRings.UI.Tips;
using RotateRings.UI.Toast;
using RotateRings.UI.User;
using RotateRings.UI.WeakGuide;
using Rotaterings.UI.RankList;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings;

public static class UIControllerResolver
{
	public static Tuple<IController, UIBaseView> Resolve(string path)
	{
		switch (path)
		{
		case "UIAboutUs":
		{
			UIAboutUsView uIAboutUsView2 = new UIAboutUsView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIAboutUsController(uIAboutUsView2), (UIBaseView)(object)uIAboutUsView2);
		}
		case "UIAboutUsV2":
		{
			UIAboutUsView uIAboutUsView = new UIAboutUsView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIAboutUsController(uIAboutUsView), (UIBaseView)(object)uIAboutUsView);
		}
		case "UIClaimReward":
		{
			UIClaimRewardView uIClaimRewardView = new UIClaimRewardView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIClaimRewardController(uIClaimRewardView), (UIBaseView)(object)uIClaimRewardView);
		}
		case "UIDailyFirstWin":
		{
			UIDailyFirstWinView uIDailyFirstWinView = new UIDailyFirstWinView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIDailyFirstWinController(uIDailyFirstWinView), (UIBaseView)(object)uIDailyFirstWinView);
		}
		case "UIDailyFirstWinNewPiece":
		{
			UIDailyFirstWinNewPieceView uIDailyFirstWinNewPieceView = new UIDailyFirstWinNewPieceView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIDailyFirstWinNewPieceController(uIDailyFirstWinNewPieceView), (UIBaseView)(object)uIDailyFirstWinNewPieceView);
		}
		case "UIDailyFirstWinPuzzle":
		{
			UIDailyFirstWinPuzzleView uIDailyFirstWinPuzzleView2 = new UIDailyFirstWinPuzzleView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIDailyFirstWinPuzzleController(uIDailyFirstWinPuzzleView2), (UIBaseView)(object)uIDailyFirstWinPuzzleView2);
		}
		case "UIDailyFirstWinPuzzleHW":
		{
			UIDailyFirstWinPuzzleView uIDailyFirstWinPuzzleView = new UIDailyFirstWinPuzzleView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIDailyFirstWinPuzzleController(uIDailyFirstWinPuzzleView), (UIBaseView)(object)uIDailyFirstWinPuzzleView);
		}
		case "UIGame":
		{
			UIGameView uIGameView = new UIGameView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIGameController(uIGameView), (UIBaseView)(object)uIGameView);
		}
		case "UIGameDifficultyToastView":
		{
			UIGameDifficultyToastView uIGameDifficultyToastView2 = new UIGameDifficultyToastView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIGameDifficultyToastController(uIGameDifficultyToastView2), (UIBaseView)(object)uIGameDifficultyToastView2);
		}
		case "UIGameDifficultyToastView_1":
		case "UIGameDifficultyToastView_2":
		{
			UIGameDifficultyToastView uIGameDifficultyToastView = new UIGameDifficultyToastView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIGameDifficultyToastController(uIGameDifficultyToastView), (UIBaseView)(object)uIGameDifficultyToastView);
		}
		case "UIGameEnterToastView1":
		case "UIGameEnterToastView2":
		case "UIGameEnterToastViewShape1":
		case "UIGameEnterToastViewShape2":
		case "UIGameEnterToastView_Halloween":
		{
			UIGameEnterToastView uIGameEnterToastView = new UIGameEnterToastView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIGameEnterToastController(uIGameEnterToastView), (UIBaseView)(object)uIGameEnterToastView);
		}
		case "UIHome":
		{
			UIHomeView uIHomeView = new UIHomeView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIHomeController(uIHomeView), (UIBaseView)(object)uIHomeView);
		}
		case "UIHowToPlayView":
		{
			UIHowToPlayView uIHowToPlayView = new UIHowToPlayView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIHowToPlayController(uIHowToPlayView), (UIBaseView)(object)uIHowToPlayView);
		}
		case "UILanguage":
		{
			UILanguageView uILanguageView = new UILanguageView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILanguageController(uILanguageView), (UIBaseView)(object)uILanguageView);
		}
		case "UILevelAdPop":
		{
			UILevelAdPopView uILevelAdPopView = new UILevelAdPopView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILevelAdPopController(uILevelAdPopView), (UIBaseView)(object)uILevelAdPopView);
		}
		case "UILevelStepLimitPop":
		{
			UILevelStepLimitPopView uILevelStepLimitPopView = new UILevelStepLimitPopView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILevelStepLimitPopController(uILevelStepLimitPopView), (UIBaseView)(object)uILevelStepLimitPopView);
		}
		case "UILevelEndCelebrate":
		{
			UILevelEndCelebrateView uILevelEndCelebrateView = new UILevelEndCelebrateView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILevelEndCelebrateController(uILevelEndCelebrateView), (UIBaseView)(object)uILevelEndCelebrateView);
		}
		case "UILevelWin":
		{
			UILevelWinView uILevelWinView = new UILevelWinView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILevelWinController(uILevelWinView), (UIBaseView)(object)uILevelWinView);
		}
		case "UIGetItem":
		{
			UIGetItemView uIGetItemView = new UIGetItemView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIGetItemController(uIGetItemView), (UIBaseView)(object)uIGetItemView);
		}
		case "UILockOverlay":
		{
			UILockOverlayView uILockOverlayView = new UILockOverlayView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILockOverlayController(uILockOverlayView), (UIBaseView)(object)uILockOverlayView);
		}
		case "UINewElementIntro":
		{
			UINewElementIntroView uINewElementIntroView = new UINewElementIntroView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UINewElementIntroController(uINewElementIntroView), (UIBaseView)(object)uINewElementIntroView);
		}
		case "UIPropGuide":
		{
			UIPropGuideView uIPropGuideView = new UIPropGuideView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIPropGuideController(uIPropGuideView), (UIBaseView)(object)uIPropGuideView);
		}
		case "UIPropUnlock":
		{
			UIPropUnlockView uIPropUnlockView = new UIPropUnlockView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIPropUnlockController(uIPropUnlockView), (UIBaseView)(object)uIPropUnlockView);
		}
		case "UIPropAward":
		{
			UIPropAwardView uIPropAwardView = new UIPropAwardView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIPropAwardController(uIPropAwardView), (UIBaseView)(object)uIPropAwardView);
		}
		case "UIRankList":
		{
			UIRankListView uIRankListView2 = new UIRankListView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRankListController(uIRankListView2), (UIBaseView)(object)uIRankListView2);
		}
		case "UIRankListHalloween":
		{
			UIRankListView uIRankListView = new UIRankListView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRankListController(uIRankListView), (UIBaseView)(object)uIRankListView);
		}
		case "UIRankListPopup":
		{
			UIRankListPopupView uIRankListPopupView2 = new UIRankListPopupView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRankListPopupController(uIRankListPopupView2), (UIBaseView)(object)uIRankListPopupView2);
		}
		case "UIRankListPopupHalloween":
		{
			UIRankListPopupView uIRankListPopupView = new UIRankListPopupView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRankListPopupController(uIRankListPopupView), (UIBaseView)(object)uIRankListPopupView);
		}
		case "UIRankListTip":
		{
			UIRankListTipView uIRankListTipView2 = new UIRankListTipView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRankListTipController(uIRankListTipView2), (UIBaseView)(object)uIRankListTipView2);
		}
		case "UIRankListTipHalloween":
		{
			UIRankListTipView uIRankListTipView = new UIRankListTipView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRankListTipController(uIRankListTipView), (UIBaseView)(object)uIRankListTipView);
		}
		case "UIRating":
		{
			UIRatingView uIRatingView = new UIRatingView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRatingController(uIRatingView), (UIBaseView)(object)uIRatingView);
		}
		case "UIRatingFeedback":
		{
			UIRatingFeedbackView uIRatingFeedbackView = new UIRatingFeedbackView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRatingFeedbackController(uIRatingFeedbackView), (UIBaseView)(object)uIRatingFeedbackView);
		}
		case "UIReissueReward":
		{
			UIReissueRewardView uIReissueRewardView = new UIReissueRewardView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIReissueRewardController(uIReissueRewardView), (UIBaseView)(object)uIReissueRewardView);
		}
		case "UIRestartRetention":
		{
			UIRestartRetentionView uIRestartRetentionView = new UIRestartRetentionView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRestartRetentionController(uIRestartRetentionView), (UIBaseView)(object)uIRestartRetentionView);
		}
		case "RocketFastUseExperiment1":
		{
			UIRocketFastUseExperiment1View uIRocketFastUseExperiment1View = new UIRocketFastUseExperiment1View();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRocketFastUseExperiment1Controller(uIRocketFastUseExperiment1View), (UIBaseView)(object)uIRocketFastUseExperiment1View);
		}
		case "UISettingGame":
		{
			UISettingGameView uISettingGameView = new UISettingGameView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UISettingGameController(uISettingGameView), (UIBaseView)(object)uISettingGameView);
		}
		case "UISettingHome":
		{
			UISettingHomeView uISettingHomeView = new UISettingHomeView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UISettingHomeController(uISettingHomeView), (UIBaseView)(object)uISettingHomeView);
		}
		case "UISettingHomeV2":
		{
			UISettingHomeV2View uISettingHomeV2View = new UISettingHomeV2View();
			return new Tuple<IController, UIBaseView>((IController)(object)new UISettingHomeV2Controller(uISettingHomeV2View), (UIBaseView)(object)uISettingHomeV2View);
		}
		case "UISettingInGame":
		{
			UISettingInGameV2View uISettingInGameV2View = new UISettingInGameV2View();
			return new Tuple<IController, UIBaseView>((IController)(object)new UISettingInGameV2Controller(uISettingInGameV2View), (UIBaseView)(object)uISettingInGameV2View);
		}
		case "UISettingInGame_DarkModeBtnLocationExperiment":
		{
			UISettingInGameV2DarkView uISettingInGameV2DarkView = new UISettingInGameV2DarkView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UISettingInGameV2DarkController(uISettingInGameV2DarkView), (UIBaseView)(object)uISettingInGameV2DarkView);
		}
		case "UITips":
		{
			UITipsView uITipsView = new UITipsView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UITipsController(uITipsView), (UIBaseView)(object)uITipsView);
		}
		case "UIToast":
		{
			UIToastView uIToastView = new UIToastView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIToastController(uIToastView), (UIBaseView)(object)uIToastView);
		}
		case "UIUser":
		{
			UIUserView uIUserView2 = new UIUserView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIUserController(uIUserView2), (UIBaseView)(object)uIUserView2);
		}
		case "UIUserV2":
		{
			UIUserView uIUserView = new UIUserView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIUserController(uIUserView), (UIBaseView)(object)uIUserView);
		}
		case "WeakGuideView":
		{
			UIWeakGuideView uIWeakGuideView = new UIWeakGuideView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIWeakGuideController(uIWeakGuideView), (UIBaseView)(object)uIWeakGuideView);
		}
		case "UIMultiTouchGuide":
		{
			UIMultiTouchGuideView uIMultiTouchGuideView = new UIMultiTouchGuideView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIMultiTouchGuideController(uIMultiTouchGuideView), (UIBaseView)(object)uIMultiTouchGuideView);
		}
		case "UIBuyReorder":
		{
			UIBuyReorderView uIBuyReorderView = new UIBuyReorderView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIBuyReorderController(uIBuyReorderView), (UIBaseView)(object)uIBuyReorderView);
		}
		case "UIBuySuccess":
		{
			UIBuySuccessView uIBuySuccessView = new UIBuySuccessView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIBuySuccessController(uIBuySuccessView), (UIBaseView)(object)uIBuySuccessView);
		}
		case "UIConnectionLost":
		{
			UIConnectionLostView uIConnectionLostView = new UIConnectionLostView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIConnectionLostController(uIConnectionLostView), (UIBaseView)(object)uIConnectionLostView);
		}
		case "UIShop":
		{
			UIShopView uIShopView = new UIShopView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIShopController(uIShopView), (UIBaseView)(object)uIShopView);
		}
		case "UILoginDeleteAccount":
		{
			UILoginDeleteAccountView uILoginDeleteAccountView = new UILoginDeleteAccountView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILoginDeleteAccountController(uILoginDeleteAccountView), (UIBaseView)(object)uILoginDeleteAccountView);
		}
		case "UILoginSaveAccount":
		{
			UILoginSaveAccountView uILoginSaveAccountView = new UILoginSaveAccountView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILoginSaveAccountController(uILoginSaveAccountView), (UIBaseView)(object)uILoginSaveAccountView);
		}
		case "UILoginSelectAccount":
		{
			UILoginSelectAccountView uILoginSelectAccountView = new UILoginSelectAccountView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILoginSelectAccountController(uILoginSelectAccountView), (UIBaseView)(object)uILoginSelectAccountView);
		}
		case "UILoginSignInWindow":
		{
			UILoginSignInView uILoginSignInView = new UILoginSignInView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILoginSignInController(uILoginSignInView), (UIBaseView)(object)uILoginSignInView);
		}
		case "UILoginSignOutWindow":
		{
			UILoginSignOutView uILoginSignOutView = new UILoginSignOutView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UILoginSignOutController(uILoginSignOutView), (UIBaseView)(object)uILoginSignOutView);
		}
		case "UIRewardedLuckyDrawStatic":
		{
			UIRewardedLuckyDrawStaticView uIRewardedLuckyDrawStaticView = new UIRewardedLuckyDrawStaticView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRewardedLuckyDrawStaticController(uIRewardedLuckyDrawStaticView), (UIBaseView)(object)uIRewardedLuckyDrawStaticView);
		}
		case "UIRewardedLuckyDrawWheel":
		{
			UIRewardedLuckyDrawWheelView uIRewardedLuckyDrawWheelView = new UIRewardedLuckyDrawWheelView();
			return new Tuple<IController, UIBaseView>((IController)(object)new UIRewardedLuckyDrawWheelController(uIRewardedLuckyDrawWheelView), (UIBaseView)(object)uIRewardedLuckyDrawWheelView);
		}
		default:
			return null;
		}
	}

	public static void Register()
	{
		UIManager.SetControllerResolverFunc((Func<string, Tuple<IController, UIBaseView>>)Resolve);
	}
}
