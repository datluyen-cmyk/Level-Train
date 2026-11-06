using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using MemoryPack;
using MemoryPack.Compression;
using XingYun.Cfg;
using XingYun.Config;

namespace Example.Cfg;

public class ExampleTables : ConfigTablesInterface
{
	private static FileStream[] bundleFileStreams = new FileStream[98];

	private static Func<string, FileStream> GetBundleFileStreamFunc;

	private static string bundleSubPath;

	private static int[] file2BundleIndex = new int[98]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 2, 2, 2, 2, 2, 2, 2
	};

	private static string[] bundleFileNames = new string[3] { "default", "expt_android", "expt_ios" };

	private TbAudioConfig _tbAudioConfig;

	private TbAudioGroupConfig _tbAudioGroupConfig;

	private TbCameraConfig _tbCameraConfig;

	private TbDailyFirstWinConfig _tbDailyFirstWinConfig;

	private TbDailyFirstWinPuzzleConfig _tbDailyFirstWinPuzzleConfig;

	private TbDailyFirstWinPuzzleRotation _tbDailyFirstWinPuzzleRotation;

	private TbEndLessLevelConfig0 _tbEndLessLevelConfig0;

	private TbEndLessLevelConfig1 _tbEndLessLevelConfig1;

	private TbEndLessLevelConfig10 _tbEndLessLevelConfig10;

	private TbEndLessLevelConfig11 _tbEndLessLevelConfig11;

	private TbEndLessLevelConfig12 _tbEndLessLevelConfig12;

	private TbEndLessLevelConfig13 _tbEndLessLevelConfig13;

	private TbEndLessLevelConfig14 _tbEndLessLevelConfig14;

	private TbEndLessLevelConfig15 _tbEndLessLevelConfig15;

	private TbEndLessLevelConfig16 _tbEndLessLevelConfig16;

	private TbEndLessLevelConfig17 _tbEndLessLevelConfig17;

	private TbEndLessLevelConfig18 _tbEndLessLevelConfig18;

	private TbEndLessLevelConfig19 _tbEndLessLevelConfig19;

	private TbEndLessLevelConfig2 _tbEndLessLevelConfig2;

	private TbEndLessLevelConfig20 _tbEndLessLevelConfig20;

	private TbEndLessLevelConfig3 _tbEndLessLevelConfig3;

	private TbEndLessLevelConfig4 _tbEndLessLevelConfig4;

	private TbEndLessLevelConfig5 _tbEndLessLevelConfig5;

	private TbEndLessLevelConfig6 _tbEndLessLevelConfig6;

	private TbEndLessLevelConfig7 _tbEndLessLevelConfig7;

	private TbEndLessLevelConfig8 _tbEndLessLevelConfig8;

	private TbEndLessLevelConfig9 _tbEndLessLevelConfig9;

	private TbExptFunction _tbExptFunction;

	private TbGameWinIQCalcRules _tbGameWinIQCalcRules;

	private TbGameWinIQLevelSuffixCoe _tbGameWinIQLevelSuffixCoe;

	private TbGameWinIQMaxDecay _tbGameWinIQMaxDecay;

	private TbGameWinIQRange _tbGameWinIQRange;

	private TbHalloweenLevelConfig _tbHalloweenLevelConfig;

	private TbHeadConfig _tbHeadConfig;

	private TbLevelConfig0 _tbLevelConfig0;

	private TbLevelConfig1 _tbLevelConfig1;

	private TbLevelConfig10 _tbLevelConfig10;

	private TbLevelConfig11 _tbLevelConfig11;

	private TbLevelConfig12 _tbLevelConfig12;

	private TbLevelConfig13 _tbLevelConfig13;

	private TbLevelConfig14 _tbLevelConfig14;

	private TbLevelConfig15 _tbLevelConfig15;

	private TbLevelConfig16 _tbLevelConfig16;

	private TbLevelConfig17 _tbLevelConfig17;

	private TbLevelConfig18 _tbLevelConfig18;

	private TbLevelConfig19 _tbLevelConfig19;

	private TbLevelConfig2 _tbLevelConfig2;

	private TbLevelConfig20 _tbLevelConfig20;

	private TbLevelConfig3 _tbLevelConfig3;

	private TbLevelConfig4 _tbLevelConfig4;

	private TbLevelConfig5 _tbLevelConfig5;

	private TbLevelConfig6 _tbLevelConfig6;

	private TbLevelConfig7 _tbLevelConfig7;

	private TbLevelConfig8 _tbLevelConfig8;

	private TbLevelConfig9 _tbLevelConfig9;

	private TbLevelDifficult _tbLevelDifficult;

	private TbLevelMatchConfig _tbLevelMatchConfig;

	private TbLevelStepLimitConfig _tbLevelStepLimitConfig;

	private TbLevelStepLimitSchema _tbLevelStepLimitSchema;

	private TbNewElementConfig _tbNewElementConfig;

	private TbOperationEvaluation _tbOperationEvaluation;

	private TbOperationScoreConfig _tbOperationScoreConfig;

	private TbPerformanceLevelConfig _tbPerformanceLevelConfig;

	private TbPlayerNameSensitiveWordConfig _tbPlayerNameSensitiveWordConfig;

	private TbPropConfig _tbPropConfig;

	private TbRankListActivity _tbRankListActivity;

	private TbRankListReward _tbRankListReward;

	private TbRankRobotNicknamePool1 _tbRankRobotNicknamePool1;

	private TbRankRobotNicknamePool2 _tbRankRobotNicknamePool2;

	private TbRankRobotNicknamePool3 _tbRankRobotNicknamePool3;

	private TbRestartRetentionConfig _tbRestartRetentionConfig;

	private TbRewardConfig _tbRewardConfig;

	private TbRewardedLuckyDrawProbability _tbRewardedLuckyDrawProbability;

	private TbRingColorConfig _tbRingColorConfig;

	private TbRobotNameConfig _tbRobotNameConfig;

	private TbRoleNameConfig _tbRoleNameConfig;

	private TbSettingLanguage _tbSettingLanguage;

	private TbShopBaseGoods _tbShopBaseGoods;

	private TbShopConfig _tbShopConfig;

	private TbStageReward _tbStageReward;

	private TbTextConfig _tbTextConfig;

	private TbVibrationFuncPoint _tbVibrationFuncPoint;

	private TbExptBaseboard_Android _tbExptBaseboard_Android;

	private TbExptBoardCondition_Android _tbExptBoardCondition_Android;

	private TbExptConfig_Android _tbExptConfig_Android;

	private TbExptFeatureRes _tbExptFeatureRes;

	private TbExptPureCondition_Android _tbExptPureCondition_Android;

	private TbExptPure_Android _tbExptPure_Android;

	private TbExptTraffic_Android _tbExptTraffic_Android;

	private TbVibrationConfig_Android _tbVibrationConfig_Android;

	private TbVibrationLevel_Android _tbVibrationLevel_Android;

	private TbExptBaseboard_IOS _tbExptBaseboard_IOS;

	private TbExptBoardCondition_IOS _tbExptBoardCondition_IOS;

	private TbExptConfig_IOS _tbExptConfig_IOS;

	private TbExptPureCondition_IOS _tbExptPureCondition_IOS;

	private TbExptPure_IOS _tbExptPure_IOS;

	private TbExptTraffic_IOS _tbExptTraffic_IOS;

	private TbVibrationConfig_IOS _tbVibrationConfig_IOS;

	private List<TableInfo> tableInfos;

	private BrotliDecompressor _dcp;

	public TbAudioConfig TbAudioConfig
	{
		get
		{
			if (_tbAudioConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBAUDIOCONFIG);
				_tbAudioConfig = new TbAudioConfig(tableInfos[0], ref contentFileStream, ref _dcp);
			}
			return _tbAudioConfig;
		}
	}

	public TbAudioGroupConfig TbAudioGroupConfig
	{
		get
		{
			if (_tbAudioGroupConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBAUDIOGROUPCONFIG);
				_tbAudioGroupConfig = new TbAudioGroupConfig(tableInfos[1], ref contentFileStream, ref _dcp);
			}
			return _tbAudioGroupConfig;
		}
	}

	public TbCameraConfig TbCameraConfig
	{
		get
		{
			if (_tbCameraConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBCAMERACONFIG);
				_tbCameraConfig = new TbCameraConfig(tableInfos[2], ref contentFileStream, ref _dcp);
			}
			return _tbCameraConfig;
		}
	}

	public TbDailyFirstWinConfig TbDailyFirstWinConfig
	{
		get
		{
			if (_tbDailyFirstWinConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBDAILYFIRSTWINCONFIG);
				_tbDailyFirstWinConfig = new TbDailyFirstWinConfig(tableInfos[3], ref contentFileStream, ref _dcp);
			}
			return _tbDailyFirstWinConfig;
		}
	}

	public TbDailyFirstWinPuzzleConfig TbDailyFirstWinPuzzleConfig
	{
		get
		{
			if (_tbDailyFirstWinPuzzleConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBDAILYFIRSTWINPUZZLECONFIG);
				_tbDailyFirstWinPuzzleConfig = new TbDailyFirstWinPuzzleConfig(tableInfos[4], ref contentFileStream, ref _dcp);
			}
			return _tbDailyFirstWinPuzzleConfig;
		}
	}

	public TbDailyFirstWinPuzzleRotation TbDailyFirstWinPuzzleRotation
	{
		get
		{
			if (_tbDailyFirstWinPuzzleRotation == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBDAILYFIRSTWINPUZZLEROTATION);
				_tbDailyFirstWinPuzzleRotation = new TbDailyFirstWinPuzzleRotation(tableInfos[5], ref contentFileStream, ref _dcp);
			}
			return _tbDailyFirstWinPuzzleRotation;
		}
	}

	public TbEndLessLevelConfig0 TbEndLessLevelConfig0
	{
		get
		{
			if (_tbEndLessLevelConfig0 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG0);
				_tbEndLessLevelConfig0 = new TbEndLessLevelConfig0(tableInfos[6], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig0;
		}
	}

	public TbEndLessLevelConfig1 TbEndLessLevelConfig1
	{
		get
		{
			if (_tbEndLessLevelConfig1 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG1);
				_tbEndLessLevelConfig1 = new TbEndLessLevelConfig1(tableInfos[7], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig1;
		}
	}

	public TbEndLessLevelConfig10 TbEndLessLevelConfig10
	{
		get
		{
			if (_tbEndLessLevelConfig10 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG10);
				_tbEndLessLevelConfig10 = new TbEndLessLevelConfig10(tableInfos[8], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig10;
		}
	}

	public TbEndLessLevelConfig11 TbEndLessLevelConfig11
	{
		get
		{
			if (_tbEndLessLevelConfig11 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG11);
				_tbEndLessLevelConfig11 = new TbEndLessLevelConfig11(tableInfos[9], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig11;
		}
	}

	public TbEndLessLevelConfig12 TbEndLessLevelConfig12
	{
		get
		{
			if (_tbEndLessLevelConfig12 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG12);
				_tbEndLessLevelConfig12 = new TbEndLessLevelConfig12(tableInfos[10], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig12;
		}
	}

	public TbEndLessLevelConfig13 TbEndLessLevelConfig13
	{
		get
		{
			if (_tbEndLessLevelConfig13 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG13);
				_tbEndLessLevelConfig13 = new TbEndLessLevelConfig13(tableInfos[11], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig13;
		}
	}

	public TbEndLessLevelConfig14 TbEndLessLevelConfig14
	{
		get
		{
			if (_tbEndLessLevelConfig14 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG14);
				_tbEndLessLevelConfig14 = new TbEndLessLevelConfig14(tableInfos[12], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig14;
		}
	}

	public TbEndLessLevelConfig15 TbEndLessLevelConfig15
	{
		get
		{
			if (_tbEndLessLevelConfig15 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG15);
				_tbEndLessLevelConfig15 = new TbEndLessLevelConfig15(tableInfos[13], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig15;
		}
	}

	public TbEndLessLevelConfig16 TbEndLessLevelConfig16
	{
		get
		{
			if (_tbEndLessLevelConfig16 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG16);
				_tbEndLessLevelConfig16 = new TbEndLessLevelConfig16(tableInfos[14], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig16;
		}
	}

	public TbEndLessLevelConfig17 TbEndLessLevelConfig17
	{
		get
		{
			if (_tbEndLessLevelConfig17 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG17);
				_tbEndLessLevelConfig17 = new TbEndLessLevelConfig17(tableInfos[15], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig17;
		}
	}

	public TbEndLessLevelConfig18 TbEndLessLevelConfig18
	{
		get
		{
			if (_tbEndLessLevelConfig18 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG18);
				_tbEndLessLevelConfig18 = new TbEndLessLevelConfig18(tableInfos[16], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig18;
		}
	}

	public TbEndLessLevelConfig19 TbEndLessLevelConfig19
	{
		get
		{
			if (_tbEndLessLevelConfig19 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG19);
				_tbEndLessLevelConfig19 = new TbEndLessLevelConfig19(tableInfos[17], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig19;
		}
	}

	public TbEndLessLevelConfig2 TbEndLessLevelConfig2
	{
		get
		{
			if (_tbEndLessLevelConfig2 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG2);
				_tbEndLessLevelConfig2 = new TbEndLessLevelConfig2(tableInfos[18], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig2;
		}
	}

	public TbEndLessLevelConfig20 TbEndLessLevelConfig20
	{
		get
		{
			if (_tbEndLessLevelConfig20 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG20);
				_tbEndLessLevelConfig20 = new TbEndLessLevelConfig20(tableInfos[19], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig20;
		}
	}

	public TbEndLessLevelConfig3 TbEndLessLevelConfig3
	{
		get
		{
			if (_tbEndLessLevelConfig3 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG3);
				_tbEndLessLevelConfig3 = new TbEndLessLevelConfig3(tableInfos[20], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig3;
		}
	}

	public TbEndLessLevelConfig4 TbEndLessLevelConfig4
	{
		get
		{
			if (_tbEndLessLevelConfig4 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG4);
				_tbEndLessLevelConfig4 = new TbEndLessLevelConfig4(tableInfos[21], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig4;
		}
	}

	public TbEndLessLevelConfig5 TbEndLessLevelConfig5
	{
		get
		{
			if (_tbEndLessLevelConfig5 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG5);
				_tbEndLessLevelConfig5 = new TbEndLessLevelConfig5(tableInfos[22], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig5;
		}
	}

	public TbEndLessLevelConfig6 TbEndLessLevelConfig6
	{
		get
		{
			if (_tbEndLessLevelConfig6 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG6);
				_tbEndLessLevelConfig6 = new TbEndLessLevelConfig6(tableInfos[23], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig6;
		}
	}

	public TbEndLessLevelConfig7 TbEndLessLevelConfig7
	{
		get
		{
			if (_tbEndLessLevelConfig7 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG7);
				_tbEndLessLevelConfig7 = new TbEndLessLevelConfig7(tableInfos[24], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig7;
		}
	}

	public TbEndLessLevelConfig8 TbEndLessLevelConfig8
	{
		get
		{
			if (_tbEndLessLevelConfig8 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG8);
				_tbEndLessLevelConfig8 = new TbEndLessLevelConfig8(tableInfos[25], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig8;
		}
	}

	public TbEndLessLevelConfig9 TbEndLessLevelConfig9
	{
		get
		{
			if (_tbEndLessLevelConfig9 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBENDLESSLEVELCONFIG9);
				_tbEndLessLevelConfig9 = new TbEndLessLevelConfig9(tableInfos[26], ref contentFileStream, ref _dcp);
			}
			return _tbEndLessLevelConfig9;
		}
	}

	public TbExptFunction TbExptFunction
	{
		get
		{
			if (_tbExptFunction == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTFUNCTION);
				_tbExptFunction = new TbExptFunction(tableInfos[27], ref contentFileStream, ref _dcp);
			}
			return _tbExptFunction;
		}
	}

	public TbGameWinIQCalcRules TbGameWinIQCalcRules
	{
		get
		{
			if (_tbGameWinIQCalcRules == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBGAMEWINIQCALCRULES);
				_tbGameWinIQCalcRules = new TbGameWinIQCalcRules(tableInfos[28], ref contentFileStream, ref _dcp);
			}
			return _tbGameWinIQCalcRules;
		}
	}

	public TbGameWinIQLevelSuffixCoe TbGameWinIQLevelSuffixCoe
	{
		get
		{
			if (_tbGameWinIQLevelSuffixCoe == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBGAMEWINIQLEVELSUFFIXCOE);
				_tbGameWinIQLevelSuffixCoe = new TbGameWinIQLevelSuffixCoe(tableInfos[29], ref contentFileStream, ref _dcp);
			}
			return _tbGameWinIQLevelSuffixCoe;
		}
	}

	public TbGameWinIQMaxDecay TbGameWinIQMaxDecay
	{
		get
		{
			if (_tbGameWinIQMaxDecay == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBGAMEWINIQMAXDECAY);
				_tbGameWinIQMaxDecay = new TbGameWinIQMaxDecay(tableInfos[30], ref contentFileStream, ref _dcp);
			}
			return _tbGameWinIQMaxDecay;
		}
	}

	public TbGameWinIQRange TbGameWinIQRange
	{
		get
		{
			if (_tbGameWinIQRange == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBGAMEWINIQRANGE);
				_tbGameWinIQRange = new TbGameWinIQRange(tableInfos[31], ref contentFileStream, ref _dcp);
			}
			return _tbGameWinIQRange;
		}
	}

	public TbHalloweenLevelConfig TbHalloweenLevelConfig
	{
		get
		{
			if (_tbHalloweenLevelConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBHALLOWEENLEVELCONFIG);
				_tbHalloweenLevelConfig = new TbHalloweenLevelConfig(tableInfos[32], ref contentFileStream, ref _dcp);
			}
			return _tbHalloweenLevelConfig;
		}
	}

	public TbHeadConfig TbHeadConfig
	{
		get
		{
			if (_tbHeadConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBHEADCONFIG);
				_tbHeadConfig = new TbHeadConfig(tableInfos[33], ref contentFileStream, ref _dcp);
			}
			return _tbHeadConfig;
		}
	}

	public TbLevelConfig0 TbLevelConfig0
	{
		get
		{
			if (_tbLevelConfig0 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG0);
				_tbLevelConfig0 = new TbLevelConfig0(tableInfos[34], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig0;
		}
	}

	public TbLevelConfig1 TbLevelConfig1
	{
		get
		{
			if (_tbLevelConfig1 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG1);
				_tbLevelConfig1 = new TbLevelConfig1(tableInfos[35], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig1;
		}
	}

	public TbLevelConfig10 TbLevelConfig10
	{
		get
		{
			if (_tbLevelConfig10 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG10);
				_tbLevelConfig10 = new TbLevelConfig10(tableInfos[36], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig10;
		}
	}

	public TbLevelConfig11 TbLevelConfig11
	{
		get
		{
			if (_tbLevelConfig11 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG11);
				_tbLevelConfig11 = new TbLevelConfig11(tableInfos[37], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig11;
		}
	}

	public TbLevelConfig12 TbLevelConfig12
	{
		get
		{
			if (_tbLevelConfig12 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG12);
				_tbLevelConfig12 = new TbLevelConfig12(tableInfos[38], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig12;
		}
	}

	public TbLevelConfig13 TbLevelConfig13
	{
		get
		{
			if (_tbLevelConfig13 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG13);
				_tbLevelConfig13 = new TbLevelConfig13(tableInfos[39], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig13;
		}
	}

	public TbLevelConfig14 TbLevelConfig14
	{
		get
		{
			if (_tbLevelConfig14 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG14);
				_tbLevelConfig14 = new TbLevelConfig14(tableInfos[40], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig14;
		}
	}

	public TbLevelConfig15 TbLevelConfig15
	{
		get
		{
			if (_tbLevelConfig15 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG15);
				_tbLevelConfig15 = new TbLevelConfig15(tableInfos[41], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig15;
		}
	}

	public TbLevelConfig16 TbLevelConfig16
	{
		get
		{
			if (_tbLevelConfig16 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG16);
				_tbLevelConfig16 = new TbLevelConfig16(tableInfos[42], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig16;
		}
	}

	public TbLevelConfig17 TbLevelConfig17
	{
		get
		{
			if (_tbLevelConfig17 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG17);
				_tbLevelConfig17 = new TbLevelConfig17(tableInfos[43], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig17;
		}
	}

	public TbLevelConfig18 TbLevelConfig18
	{
		get
		{
			if (_tbLevelConfig18 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG18);
				_tbLevelConfig18 = new TbLevelConfig18(tableInfos[44], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig18;
		}
	}

	public TbLevelConfig19 TbLevelConfig19
	{
		get
		{
			if (_tbLevelConfig19 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG19);
				_tbLevelConfig19 = new TbLevelConfig19(tableInfos[45], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig19;
		}
	}

	public TbLevelConfig2 TbLevelConfig2
	{
		get
		{
			if (_tbLevelConfig2 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG2);
				_tbLevelConfig2 = new TbLevelConfig2(tableInfos[46], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig2;
		}
	}

	public TbLevelConfig20 TbLevelConfig20
	{
		get
		{
			if (_tbLevelConfig20 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG20);
				_tbLevelConfig20 = new TbLevelConfig20(tableInfos[47], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig20;
		}
	}

	public TbLevelConfig3 TbLevelConfig3
	{
		get
		{
			if (_tbLevelConfig3 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG3);
				_tbLevelConfig3 = new TbLevelConfig3(tableInfos[48], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig3;
		}
	}

	public TbLevelConfig4 TbLevelConfig4
	{
		get
		{
			if (_tbLevelConfig4 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG4);
				_tbLevelConfig4 = new TbLevelConfig4(tableInfos[49], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig4;
		}
	}

	public TbLevelConfig5 TbLevelConfig5
	{
		get
		{
			if (_tbLevelConfig5 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG5);
				_tbLevelConfig5 = new TbLevelConfig5(tableInfos[50], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig5;
		}
	}

	public TbLevelConfig6 TbLevelConfig6
	{
		get
		{
			if (_tbLevelConfig6 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG6);
				_tbLevelConfig6 = new TbLevelConfig6(tableInfos[51], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig6;
		}
	}

	public TbLevelConfig7 TbLevelConfig7
	{
		get
		{
			if (_tbLevelConfig7 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG7);
				_tbLevelConfig7 = new TbLevelConfig7(tableInfos[52], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig7;
		}
	}

	public TbLevelConfig8 TbLevelConfig8
	{
		get
		{
			if (_tbLevelConfig8 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG8);
				_tbLevelConfig8 = new TbLevelConfig8(tableInfos[53], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig8;
		}
	}

	public TbLevelConfig9 TbLevelConfig9
	{
		get
		{
			if (_tbLevelConfig9 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELCONFIG9);
				_tbLevelConfig9 = new TbLevelConfig9(tableInfos[54], ref contentFileStream, ref _dcp);
			}
			return _tbLevelConfig9;
		}
	}

	public TbLevelDifficult TbLevelDifficult
	{
		get
		{
			if (_tbLevelDifficult == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELDIFFICULT);
				_tbLevelDifficult = new TbLevelDifficult(tableInfos[55], ref contentFileStream, ref _dcp);
			}
			return _tbLevelDifficult;
		}
	}

	public TbLevelMatchConfig TbLevelMatchConfig
	{
		get
		{
			if (_tbLevelMatchConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELMATCHCONFIG);
				_tbLevelMatchConfig = new TbLevelMatchConfig(tableInfos[56], ref contentFileStream, ref _dcp);
			}
			return _tbLevelMatchConfig;
		}
	}

	public TbLevelStepLimitConfig TbLevelStepLimitConfig
	{
		get
		{
			if (_tbLevelStepLimitConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELSTEPLIMITCONFIG);
				_tbLevelStepLimitConfig = new TbLevelStepLimitConfig(tableInfos[57], ref contentFileStream, ref _dcp);
			}
			return _tbLevelStepLimitConfig;
		}
	}

	public TbLevelStepLimitSchema TbLevelStepLimitSchema
	{
		get
		{
			if (_tbLevelStepLimitSchema == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBLEVELSTEPLIMITSCHEMA);
				_tbLevelStepLimitSchema = new TbLevelStepLimitSchema(tableInfos[58], ref contentFileStream, ref _dcp);
			}
			return _tbLevelStepLimitSchema;
		}
	}

	public TbNewElementConfig TbNewElementConfig
	{
		get
		{
			if (_tbNewElementConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBNEWELEMENTCONFIG);
				_tbNewElementConfig = new TbNewElementConfig(tableInfos[59], ref contentFileStream, ref _dcp);
			}
			return _tbNewElementConfig;
		}
	}

	public TbOperationEvaluation TbOperationEvaluation
	{
		get
		{
			if (_tbOperationEvaluation == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBOPERATIONEVALUATION);
				_tbOperationEvaluation = new TbOperationEvaluation(tableInfos[60], ref contentFileStream, ref _dcp);
			}
			return _tbOperationEvaluation;
		}
	}

	public TbOperationScoreConfig TbOperationScoreConfig
	{
		get
		{
			if (_tbOperationScoreConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBOPERATIONSCORECONFIG);
				_tbOperationScoreConfig = new TbOperationScoreConfig(tableInfos[61], ref contentFileStream, ref _dcp);
			}
			return _tbOperationScoreConfig;
		}
	}

	public TbPerformanceLevelConfig TbPerformanceLevelConfig
	{
		get
		{
			if (_tbPerformanceLevelConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBPERFORMANCELEVELCONFIG);
				_tbPerformanceLevelConfig = new TbPerformanceLevelConfig(tableInfos[62], ref contentFileStream, ref _dcp);
			}
			return _tbPerformanceLevelConfig;
		}
	}

	public TbPlayerNameSensitiveWordConfig TbPlayerNameSensitiveWordConfig
	{
		get
		{
			if (_tbPlayerNameSensitiveWordConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBPLAYERNAMESENSITIVEWORDCONFIG);
				_tbPlayerNameSensitiveWordConfig = new TbPlayerNameSensitiveWordConfig(tableInfos[63], ref contentFileStream, ref _dcp);
			}
			return _tbPlayerNameSensitiveWordConfig;
		}
	}

	public TbPropConfig TbPropConfig
	{
		get
		{
			if (_tbPropConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBPROPCONFIG);
				_tbPropConfig = new TbPropConfig(tableInfos[64], ref contentFileStream, ref _dcp);
			}
			return _tbPropConfig;
		}
	}

	public TbRankListActivity TbRankListActivity
	{
		get
		{
			if (_tbRankListActivity == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRANKLISTACTIVITY);
				_tbRankListActivity = new TbRankListActivity(tableInfos[65], ref contentFileStream, ref _dcp);
			}
			return _tbRankListActivity;
		}
	}

	public TbRankListReward TbRankListReward
	{
		get
		{
			if (_tbRankListReward == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRANKLISTREWARD);
				_tbRankListReward = new TbRankListReward(tableInfos[66], ref contentFileStream, ref _dcp);
			}
			return _tbRankListReward;
		}
	}

	public TbRankRobotNicknamePool1 TbRankRobotNicknamePool1
	{
		get
		{
			if (_tbRankRobotNicknamePool1 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRANKROBOTNICKNAMEPOOL1);
				_tbRankRobotNicknamePool1 = new TbRankRobotNicknamePool1(tableInfos[67], ref contentFileStream, ref _dcp);
			}
			return _tbRankRobotNicknamePool1;
		}
	}

	public TbRankRobotNicknamePool2 TbRankRobotNicknamePool2
	{
		get
		{
			if (_tbRankRobotNicknamePool2 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRANKROBOTNICKNAMEPOOL2);
				_tbRankRobotNicknamePool2 = new TbRankRobotNicknamePool2(tableInfos[68], ref contentFileStream, ref _dcp);
			}
			return _tbRankRobotNicknamePool2;
		}
	}

	public TbRankRobotNicknamePool3 TbRankRobotNicknamePool3
	{
		get
		{
			if (_tbRankRobotNicknamePool3 == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRANKROBOTNICKNAMEPOOL3);
				_tbRankRobotNicknamePool3 = new TbRankRobotNicknamePool3(tableInfos[69], ref contentFileStream, ref _dcp);
			}
			return _tbRankRobotNicknamePool3;
		}
	}

	public TbRestartRetentionConfig TbRestartRetentionConfig
	{
		get
		{
			if (_tbRestartRetentionConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRESTARTRETENTIONCONFIG);
				_tbRestartRetentionConfig = new TbRestartRetentionConfig(tableInfos[70], ref contentFileStream, ref _dcp);
			}
			return _tbRestartRetentionConfig;
		}
	}

	public TbRewardConfig TbRewardConfig
	{
		get
		{
			if (_tbRewardConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBREWARDCONFIG);
				_tbRewardConfig = new TbRewardConfig(tableInfos[71], ref contentFileStream, ref _dcp);
			}
			return _tbRewardConfig;
		}
	}

	public TbRewardedLuckyDrawProbability TbRewardedLuckyDrawProbability
	{
		get
		{
			if (_tbRewardedLuckyDrawProbability == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBREWARDEDLUCKYDRAWPROBABILITY);
				_tbRewardedLuckyDrawProbability = new TbRewardedLuckyDrawProbability(tableInfos[72], ref contentFileStream, ref _dcp);
			}
			return _tbRewardedLuckyDrawProbability;
		}
	}

	public TbRingColorConfig TbRingColorConfig
	{
		get
		{
			if (_tbRingColorConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBRINGCOLORCONFIG);
				_tbRingColorConfig = new TbRingColorConfig(tableInfos[73], ref contentFileStream, ref _dcp);
			}
			return _tbRingColorConfig;
		}
	}

	public TbRobotNameConfig TbRobotNameConfig
	{
		get
		{
			if (_tbRobotNameConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBROBOTNAMECONFIG);
				_tbRobotNameConfig = new TbRobotNameConfig(tableInfos[74], ref contentFileStream, ref _dcp);
			}
			return _tbRobotNameConfig;
		}
	}

	public TbRoleNameConfig TbRoleNameConfig
	{
		get
		{
			if (_tbRoleNameConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBROLENAMECONFIG);
				_tbRoleNameConfig = new TbRoleNameConfig(tableInfos[75], ref contentFileStream, ref _dcp);
			}
			return _tbRoleNameConfig;
		}
	}

	public TbSettingLanguage TbSettingLanguage
	{
		get
		{
			if (_tbSettingLanguage == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBSETTINGLANGUAGE);
				_tbSettingLanguage = new TbSettingLanguage(tableInfos[76], ref contentFileStream, ref _dcp);
			}
			return _tbSettingLanguage;
		}
	}

	public TbShopBaseGoods TbShopBaseGoods
	{
		get
		{
			if (_tbShopBaseGoods == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBSHOPBASEGOODS);
				_tbShopBaseGoods = new TbShopBaseGoods(tableInfos[77], ref contentFileStream, ref _dcp);
			}
			return _tbShopBaseGoods;
		}
	}

	public TbShopConfig TbShopConfig
	{
		get
		{
			if (_tbShopConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBSHOPCONFIG);
				_tbShopConfig = new TbShopConfig(tableInfos[78], ref contentFileStream, ref _dcp);
			}
			return _tbShopConfig;
		}
	}

	public TbStageReward TbStageReward
	{
		get
		{
			if (_tbStageReward == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBSTAGEREWARD);
				_tbStageReward = new TbStageReward(tableInfos[79], ref contentFileStream, ref _dcp);
			}
			return _tbStageReward;
		}
	}

	public TbTextConfig TbTextConfig
	{
		get
		{
			if (_tbTextConfig == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBTEXTCONFIG);
				_tbTextConfig = new TbTextConfig(tableInfos[80], ref contentFileStream, ref _dcp);
			}
			return _tbTextConfig;
		}
	}

	public TbVibrationFuncPoint TbVibrationFuncPoint
	{
		get
		{
			if (_tbVibrationFuncPoint == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBVIBRATIONFUNCPOINT);
				_tbVibrationFuncPoint = new TbVibrationFuncPoint(tableInfos[81], ref contentFileStream, ref _dcp);
			}
			return _tbVibrationFuncPoint;
		}
	}

	public TbExptBaseboard_Android TbExptBaseboard_Android
	{
		get
		{
			if (_tbExptBaseboard_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTBASEBOARD_ANDROID);
				_tbExptBaseboard_Android = new TbExptBaseboard_Android(tableInfos[82], ref contentFileStream, ref _dcp);
			}
			return _tbExptBaseboard_Android;
		}
	}

	public TbExptBoardCondition_Android TbExptBoardCondition_Android
	{
		get
		{
			if (_tbExptBoardCondition_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTBOARDCONDITION_ANDROID);
				_tbExptBoardCondition_Android = new TbExptBoardCondition_Android(tableInfos[83], ref contentFileStream, ref _dcp);
			}
			return _tbExptBoardCondition_Android;
		}
	}

	public TbExptConfig_Android TbExptConfig_Android
	{
		get
		{
			if (_tbExptConfig_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTCONFIG_ANDROID);
				_tbExptConfig_Android = new TbExptConfig_Android(tableInfos[84], ref contentFileStream, ref _dcp);
			}
			return _tbExptConfig_Android;
		}
	}

	public TbExptFeatureRes TbExptFeatureRes
	{
		get
		{
			if (_tbExptFeatureRes == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTFEATURERES);
				_tbExptFeatureRes = new TbExptFeatureRes(tableInfos[85], ref contentFileStream, ref _dcp);
			}
			return _tbExptFeatureRes;
		}
	}

	public TbExptPureCondition_Android TbExptPureCondition_Android
	{
		get
		{
			if (_tbExptPureCondition_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTPURECONDITION_ANDROID);
				_tbExptPureCondition_Android = new TbExptPureCondition_Android(tableInfos[86], ref contentFileStream, ref _dcp);
			}
			return _tbExptPureCondition_Android;
		}
	}

	public TbExptPure_Android TbExptPure_Android
	{
		get
		{
			if (_tbExptPure_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTPURE_ANDROID);
				_tbExptPure_Android = new TbExptPure_Android(tableInfos[87], ref contentFileStream, ref _dcp);
			}
			return _tbExptPure_Android;
		}
	}

	public TbExptTraffic_Android TbExptTraffic_Android
	{
		get
		{
			if (_tbExptTraffic_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTTRAFFIC_ANDROID);
				_tbExptTraffic_Android = new TbExptTraffic_Android(tableInfos[88], ref contentFileStream, ref _dcp);
			}
			return _tbExptTraffic_Android;
		}
	}

	public TbVibrationConfig_Android TbVibrationConfig_Android
	{
		get
		{
			if (_tbVibrationConfig_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBVIBRATIONCONFIG_ANDROID);
				_tbVibrationConfig_Android = new TbVibrationConfig_Android(tableInfos[89], ref contentFileStream, ref _dcp);
			}
			return _tbVibrationConfig_Android;
		}
	}

	public TbVibrationLevel_Android TbVibrationLevel_Android
	{
		get
		{
			if (_tbVibrationLevel_Android == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBVIBRATIONLEVEL_ANDROID);
				_tbVibrationLevel_Android = new TbVibrationLevel_Android(tableInfos[90], ref contentFileStream, ref _dcp);
			}
			return _tbVibrationLevel_Android;
		}
	}

	public TbExptBaseboard_IOS TbExptBaseboard_IOS
	{
		get
		{
			if (_tbExptBaseboard_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTBASEBOARD_IOS);
				_tbExptBaseboard_IOS = new TbExptBaseboard_IOS(tableInfos[91], ref contentFileStream, ref _dcp);
			}
			return _tbExptBaseboard_IOS;
		}
	}

	public TbExptBoardCondition_IOS TbExptBoardCondition_IOS
	{
		get
		{
			if (_tbExptBoardCondition_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTBOARDCONDITION_IOS);
				_tbExptBoardCondition_IOS = new TbExptBoardCondition_IOS(tableInfos[92], ref contentFileStream, ref _dcp);
			}
			return _tbExptBoardCondition_IOS;
		}
	}

	public TbExptConfig_IOS TbExptConfig_IOS
	{
		get
		{
			if (_tbExptConfig_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTCONFIG_IOS);
				_tbExptConfig_IOS = new TbExptConfig_IOS(tableInfos[93], ref contentFileStream, ref _dcp);
			}
			return _tbExptConfig_IOS;
		}
	}

	public TbExptPureCondition_IOS TbExptPureCondition_IOS
	{
		get
		{
			if (_tbExptPureCondition_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTPURECONDITION_IOS);
				_tbExptPureCondition_IOS = new TbExptPureCondition_IOS(tableInfos[94], ref contentFileStream, ref _dcp);
			}
			return _tbExptPureCondition_IOS;
		}
	}

	public TbExptPure_IOS TbExptPure_IOS
	{
		get
		{
			if (_tbExptPure_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTPURE_IOS);
				_tbExptPure_IOS = new TbExptPure_IOS(tableInfos[95], ref contentFileStream, ref _dcp);
			}
			return _tbExptPure_IOS;
		}
	}

	public TbExptTraffic_IOS TbExptTraffic_IOS
	{
		get
		{
			if (_tbExptTraffic_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBEXPTTRAFFIC_IOS);
				_tbExptTraffic_IOS = new TbExptTraffic_IOS(tableInfos[96], ref contentFileStream, ref _dcp);
			}
			return _tbExptTraffic_IOS;
		}
	}

	public TbVibrationConfig_IOS TbVibrationConfig_IOS
	{
		get
		{
			if (_tbVibrationConfig_IOS == null)
			{
				FileStream contentFileStream = GetBundleFileStream(TableTypeEnum.TBVIBRATIONCONFIG_IOS);
				_tbVibrationConfig_IOS = new TbVibrationConfig_IOS(tableInfos[97], ref contentFileStream, ref _dcp);
			}
			return _tbVibrationConfig_IOS;
		}
	}

	public List<ExptBaseboard> GetExptBaseboardDataList => TbExptBaseboard_Android.DataList;

	public Dictionary<string, ExptBaseboard> GetExptBaseboardDataMap => TbExptBaseboard_Android.DataMap;

	public List<ExptBoardCondition> GetExptBoardConditionDataList => TbExptBoardCondition_Android.DataList;

	public Dictionary<int, ExptBoardCondition> GetExptBoardConditionDataMap => TbExptBoardCondition_Android.DataMap;

	public List<ExperimentConfig> GetExptConfigDataList => TbExptConfig_Android.DataList;

	public Dictionary<string, ExperimentConfig> GetExptConfigDataMap => TbExptConfig_Android.DataMap;

	public List<ExptBoardCondition> GetExptPureConditionDataList => TbExptPureCondition_Android.DataList;

	public Dictionary<int, ExptBoardCondition> GetExptPureConditionDataMap => TbExptPureCondition_Android.DataMap;

	public List<ExptPure> GetExptPureDataList => TbExptPure_Android.DataList;

	public Dictionary<string, ExptPure> GetExptPureDataMap => TbExptPure_Android.DataMap;

	public List<ExptTrafficConfig> GetExptTrafficDataList => TbExptTraffic_Android.DataList;

	public Dictionary<string, ExptTrafficConfig> GetExptTrafficDataMap => TbExptTraffic_Android.DataMap;

	public static FileStream GetBundleFileStream(TableTypeEnum tableTypeEnum)
	{
		if (bundleFileStreams[file2BundleIndex[(int)tableTypeEnum]] == null)
		{
			bundleFileStreams[file2BundleIndex[(int)tableTypeEnum]] = GetBundleFileStreamFunc(bundleFileNames[file2BundleIndex[(int)tableTypeEnum]]);
		}
		return bundleFileStreams[file2BundleIndex[(int)tableTypeEnum]];
	}

	public void Initialize(Func<string, FileStream> fileStreamFunc)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		GetBundleFileStreamFunc = fileStreamFunc;
		_dcp = default(BrotliDecompressor);
		FileStream fileStream = fileStreamFunc("meta");
		byte[] array = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
		try
		{
			fileStream.Read(array, 0, (int)fileStream.Length);
			ReadOnlySequence<byte> readOnlySequence = ((BrotliDecompressor)(ref _dcp)).Decompress((ReadOnlySpan<byte>)array);
			tableInfos = MemoryPackSerializer.Deserialize<List<TableInfo>>(ref readOnlySequence, (MemoryPackSerializerOptions)null);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
			fileStream.Dispose();
		}
		((BrotliDecompressor)(ref _dcp)).Dispose();
	}

	public void DisposeLogic()
	{
		for (int i = 0; i < bundleFileStreams.Length; i++)
		{
			if (bundleFileStreams[i] != null)
			{
				bundleFileStreams[i].Dispose();
				bundleFileStreams[i] = null;
			}
		}
		if (_tbAudioConfig != null)
		{
			_tbAudioConfig.DisposeLogic();
			_tbAudioConfig = null;
		}
		if (_tbAudioGroupConfig != null)
		{
			_tbAudioGroupConfig.DisposeLogic();
			_tbAudioGroupConfig = null;
		}
		if (_tbCameraConfig != null)
		{
			_tbCameraConfig.DisposeLogic();
			_tbCameraConfig = null;
		}
		if (_tbDailyFirstWinConfig != null)
		{
			_tbDailyFirstWinConfig.DisposeLogic();
			_tbDailyFirstWinConfig = null;
		}
		if (_tbDailyFirstWinPuzzleConfig != null)
		{
			_tbDailyFirstWinPuzzleConfig.DisposeLogic();
			_tbDailyFirstWinPuzzleConfig = null;
		}
		if (_tbDailyFirstWinPuzzleRotation != null)
		{
			_tbDailyFirstWinPuzzleRotation.DisposeLogic();
			_tbDailyFirstWinPuzzleRotation = null;
		}
		if (_tbEndLessLevelConfig0 != null)
		{
			_tbEndLessLevelConfig0.DisposeLogic();
			_tbEndLessLevelConfig0 = null;
		}
		if (_tbEndLessLevelConfig1 != null)
		{
			_tbEndLessLevelConfig1.DisposeLogic();
			_tbEndLessLevelConfig1 = null;
		}
		if (_tbEndLessLevelConfig10 != null)
		{
			_tbEndLessLevelConfig10.DisposeLogic();
			_tbEndLessLevelConfig10 = null;
		}
		if (_tbEndLessLevelConfig11 != null)
		{
			_tbEndLessLevelConfig11.DisposeLogic();
			_tbEndLessLevelConfig11 = null;
		}
		if (_tbEndLessLevelConfig12 != null)
		{
			_tbEndLessLevelConfig12.DisposeLogic();
			_tbEndLessLevelConfig12 = null;
		}
		if (_tbEndLessLevelConfig13 != null)
		{
			_tbEndLessLevelConfig13.DisposeLogic();
			_tbEndLessLevelConfig13 = null;
		}
		if (_tbEndLessLevelConfig14 != null)
		{
			_tbEndLessLevelConfig14.DisposeLogic();
			_tbEndLessLevelConfig14 = null;
		}
		if (_tbEndLessLevelConfig15 != null)
		{
			_tbEndLessLevelConfig15.DisposeLogic();
			_tbEndLessLevelConfig15 = null;
		}
		if (_tbEndLessLevelConfig16 != null)
		{
			_tbEndLessLevelConfig16.DisposeLogic();
			_tbEndLessLevelConfig16 = null;
		}
		if (_tbEndLessLevelConfig17 != null)
		{
			_tbEndLessLevelConfig17.DisposeLogic();
			_tbEndLessLevelConfig17 = null;
		}
		if (_tbEndLessLevelConfig18 != null)
		{
			_tbEndLessLevelConfig18.DisposeLogic();
			_tbEndLessLevelConfig18 = null;
		}
		if (_tbEndLessLevelConfig19 != null)
		{
			_tbEndLessLevelConfig19.DisposeLogic();
			_tbEndLessLevelConfig19 = null;
		}
		if (_tbEndLessLevelConfig2 != null)
		{
			_tbEndLessLevelConfig2.DisposeLogic();
			_tbEndLessLevelConfig2 = null;
		}
		if (_tbEndLessLevelConfig20 != null)
		{
			_tbEndLessLevelConfig20.DisposeLogic();
			_tbEndLessLevelConfig20 = null;
		}
		if (_tbEndLessLevelConfig3 != null)
		{
			_tbEndLessLevelConfig3.DisposeLogic();
			_tbEndLessLevelConfig3 = null;
		}
		if (_tbEndLessLevelConfig4 != null)
		{
			_tbEndLessLevelConfig4.DisposeLogic();
			_tbEndLessLevelConfig4 = null;
		}
		if (_tbEndLessLevelConfig5 != null)
		{
			_tbEndLessLevelConfig5.DisposeLogic();
			_tbEndLessLevelConfig5 = null;
		}
		if (_tbEndLessLevelConfig6 != null)
		{
			_tbEndLessLevelConfig6.DisposeLogic();
			_tbEndLessLevelConfig6 = null;
		}
		if (_tbEndLessLevelConfig7 != null)
		{
			_tbEndLessLevelConfig7.DisposeLogic();
			_tbEndLessLevelConfig7 = null;
		}
		if (_tbEndLessLevelConfig8 != null)
		{
			_tbEndLessLevelConfig8.DisposeLogic();
			_tbEndLessLevelConfig8 = null;
		}
		if (_tbEndLessLevelConfig9 != null)
		{
			_tbEndLessLevelConfig9.DisposeLogic();
			_tbEndLessLevelConfig9 = null;
		}
		if (_tbExptFunction != null)
		{
			_tbExptFunction.DisposeLogic();
			_tbExptFunction = null;
		}
		if (_tbGameWinIQCalcRules != null)
		{
			_tbGameWinIQCalcRules.DisposeLogic();
			_tbGameWinIQCalcRules = null;
		}
		if (_tbGameWinIQLevelSuffixCoe != null)
		{
			_tbGameWinIQLevelSuffixCoe.DisposeLogic();
			_tbGameWinIQLevelSuffixCoe = null;
		}
		if (_tbGameWinIQMaxDecay != null)
		{
			_tbGameWinIQMaxDecay.DisposeLogic();
			_tbGameWinIQMaxDecay = null;
		}
		if (_tbGameWinIQRange != null)
		{
			_tbGameWinIQRange.DisposeLogic();
			_tbGameWinIQRange = null;
		}
		if (_tbHalloweenLevelConfig != null)
		{
			_tbHalloweenLevelConfig.DisposeLogic();
			_tbHalloweenLevelConfig = null;
		}
		if (_tbHeadConfig != null)
		{
			_tbHeadConfig.DisposeLogic();
			_tbHeadConfig = null;
		}
		if (_tbLevelConfig0 != null)
		{
			_tbLevelConfig0.DisposeLogic();
			_tbLevelConfig0 = null;
		}
		if (_tbLevelConfig1 != null)
		{
			_tbLevelConfig1.DisposeLogic();
			_tbLevelConfig1 = null;
		}
		if (_tbLevelConfig10 != null)
		{
			_tbLevelConfig10.DisposeLogic();
			_tbLevelConfig10 = null;
		}
		if (_tbLevelConfig11 != null)
		{
			_tbLevelConfig11.DisposeLogic();
			_tbLevelConfig11 = null;
		}
		if (_tbLevelConfig12 != null)
		{
			_tbLevelConfig12.DisposeLogic();
			_tbLevelConfig12 = null;
		}
		if (_tbLevelConfig13 != null)
		{
			_tbLevelConfig13.DisposeLogic();
			_tbLevelConfig13 = null;
		}
		if (_tbLevelConfig14 != null)
		{
			_tbLevelConfig14.DisposeLogic();
			_tbLevelConfig14 = null;
		}
		if (_tbLevelConfig15 != null)
		{
			_tbLevelConfig15.DisposeLogic();
			_tbLevelConfig15 = null;
		}
		if (_tbLevelConfig16 != null)
		{
			_tbLevelConfig16.DisposeLogic();
			_tbLevelConfig16 = null;
		}
		if (_tbLevelConfig17 != null)
		{
			_tbLevelConfig17.DisposeLogic();
			_tbLevelConfig17 = null;
		}
		if (_tbLevelConfig18 != null)
		{
			_tbLevelConfig18.DisposeLogic();
			_tbLevelConfig18 = null;
		}
		if (_tbLevelConfig19 != null)
		{
			_tbLevelConfig19.DisposeLogic();
			_tbLevelConfig19 = null;
		}
		if (_tbLevelConfig2 != null)
		{
			_tbLevelConfig2.DisposeLogic();
			_tbLevelConfig2 = null;
		}
		if (_tbLevelConfig20 != null)
		{
			_tbLevelConfig20.DisposeLogic();
			_tbLevelConfig20 = null;
		}
		if (_tbLevelConfig3 != null)
		{
			_tbLevelConfig3.DisposeLogic();
			_tbLevelConfig3 = null;
		}
		if (_tbLevelConfig4 != null)
		{
			_tbLevelConfig4.DisposeLogic();
			_tbLevelConfig4 = null;
		}
		if (_tbLevelConfig5 != null)
		{
			_tbLevelConfig5.DisposeLogic();
			_tbLevelConfig5 = null;
		}
		if (_tbLevelConfig6 != null)
		{
			_tbLevelConfig6.DisposeLogic();
			_tbLevelConfig6 = null;
		}
		if (_tbLevelConfig7 != null)
		{
			_tbLevelConfig7.DisposeLogic();
			_tbLevelConfig7 = null;
		}
		if (_tbLevelConfig8 != null)
		{
			_tbLevelConfig8.DisposeLogic();
			_tbLevelConfig8 = null;
		}
		if (_tbLevelConfig9 != null)
		{
			_tbLevelConfig9.DisposeLogic();
			_tbLevelConfig9 = null;
		}
		if (_tbLevelDifficult != null)
		{
			_tbLevelDifficult.DisposeLogic();
			_tbLevelDifficult = null;
		}
		if (_tbLevelMatchConfig != null)
		{
			_tbLevelMatchConfig.DisposeLogic();
			_tbLevelMatchConfig = null;
		}
		if (_tbLevelStepLimitConfig != null)
		{
			_tbLevelStepLimitConfig.DisposeLogic();
			_tbLevelStepLimitConfig = null;
		}
		if (_tbLevelStepLimitSchema != null)
		{
			_tbLevelStepLimitSchema.DisposeLogic();
			_tbLevelStepLimitSchema = null;
		}
		if (_tbNewElementConfig != null)
		{
			_tbNewElementConfig.DisposeLogic();
			_tbNewElementConfig = null;
		}
		if (_tbOperationEvaluation != null)
		{
			_tbOperationEvaluation.DisposeLogic();
			_tbOperationEvaluation = null;
		}
		if (_tbOperationScoreConfig != null)
		{
			_tbOperationScoreConfig.DisposeLogic();
			_tbOperationScoreConfig = null;
		}
		if (_tbPerformanceLevelConfig != null)
		{
			_tbPerformanceLevelConfig.DisposeLogic();
			_tbPerformanceLevelConfig = null;
		}
		if (_tbPlayerNameSensitiveWordConfig != null)
		{
			_tbPlayerNameSensitiveWordConfig.DisposeLogic();
			_tbPlayerNameSensitiveWordConfig = null;
		}
		if (_tbPropConfig != null)
		{
			_tbPropConfig.DisposeLogic();
			_tbPropConfig = null;
		}
		if (_tbRankListActivity != null)
		{
			_tbRankListActivity.DisposeLogic();
			_tbRankListActivity = null;
		}
		if (_tbRankListReward != null)
		{
			_tbRankListReward.DisposeLogic();
			_tbRankListReward = null;
		}
		if (_tbRankRobotNicknamePool1 != null)
		{
			_tbRankRobotNicknamePool1.DisposeLogic();
			_tbRankRobotNicknamePool1 = null;
		}
		if (_tbRankRobotNicknamePool2 != null)
		{
			_tbRankRobotNicknamePool2.DisposeLogic();
			_tbRankRobotNicknamePool2 = null;
		}
		if (_tbRankRobotNicknamePool3 != null)
		{
			_tbRankRobotNicknamePool3.DisposeLogic();
			_tbRankRobotNicknamePool3 = null;
		}
		if (_tbRestartRetentionConfig != null)
		{
			_tbRestartRetentionConfig.DisposeLogic();
			_tbRestartRetentionConfig = null;
		}
		if (_tbRewardConfig != null)
		{
			_tbRewardConfig.DisposeLogic();
			_tbRewardConfig = null;
		}
		if (_tbRewardedLuckyDrawProbability != null)
		{
			_tbRewardedLuckyDrawProbability.DisposeLogic();
			_tbRewardedLuckyDrawProbability = null;
		}
		if (_tbRingColorConfig != null)
		{
			_tbRingColorConfig.DisposeLogic();
			_tbRingColorConfig = null;
		}
		if (_tbRobotNameConfig != null)
		{
			_tbRobotNameConfig.DisposeLogic();
			_tbRobotNameConfig = null;
		}
		if (_tbRoleNameConfig != null)
		{
			_tbRoleNameConfig.DisposeLogic();
			_tbRoleNameConfig = null;
		}
		if (_tbSettingLanguage != null)
		{
			_tbSettingLanguage.DisposeLogic();
			_tbSettingLanguage = null;
		}
		if (_tbShopBaseGoods != null)
		{
			_tbShopBaseGoods.DisposeLogic();
			_tbShopBaseGoods = null;
		}
		if (_tbShopConfig != null)
		{
			_tbShopConfig.DisposeLogic();
			_tbShopConfig = null;
		}
		if (_tbStageReward != null)
		{
			_tbStageReward.DisposeLogic();
			_tbStageReward = null;
		}
		if (_tbTextConfig != null)
		{
			_tbTextConfig.DisposeLogic();
			_tbTextConfig = null;
		}
		if (_tbVibrationFuncPoint != null)
		{
			_tbVibrationFuncPoint.DisposeLogic();
			_tbVibrationFuncPoint = null;
		}
		if (_tbExptBaseboard_Android != null)
		{
			_tbExptBaseboard_Android.DisposeLogic();
			_tbExptBaseboard_Android = null;
		}
		if (_tbExptBoardCondition_Android != null)
		{
			_tbExptBoardCondition_Android.DisposeLogic();
			_tbExptBoardCondition_Android = null;
		}
		if (_tbExptConfig_Android != null)
		{
			_tbExptConfig_Android.DisposeLogic();
			_tbExptConfig_Android = null;
		}
		if (_tbExptFeatureRes != null)
		{
			_tbExptFeatureRes.DisposeLogic();
			_tbExptFeatureRes = null;
		}
		if (_tbExptPureCondition_Android != null)
		{
			_tbExptPureCondition_Android.DisposeLogic();
			_tbExptPureCondition_Android = null;
		}
		if (_tbExptPure_Android != null)
		{
			_tbExptPure_Android.DisposeLogic();
			_tbExptPure_Android = null;
		}
		if (_tbExptTraffic_Android != null)
		{
			_tbExptTraffic_Android.DisposeLogic();
			_tbExptTraffic_Android = null;
		}
		if (_tbVibrationConfig_Android != null)
		{
			_tbVibrationConfig_Android.DisposeLogic();
			_tbVibrationConfig_Android = null;
		}
		if (_tbVibrationLevel_Android != null)
		{
			_tbVibrationLevel_Android.DisposeLogic();
			_tbVibrationLevel_Android = null;
		}
		if (_tbExptBaseboard_IOS != null)
		{
			_tbExptBaseboard_IOS.DisposeLogic();
			_tbExptBaseboard_IOS = null;
		}
		if (_tbExptBoardCondition_IOS != null)
		{
			_tbExptBoardCondition_IOS.DisposeLogic();
			_tbExptBoardCondition_IOS = null;
		}
		if (_tbExptConfig_IOS != null)
		{
			_tbExptConfig_IOS.DisposeLogic();
			_tbExptConfig_IOS = null;
		}
		if (_tbExptPureCondition_IOS != null)
		{
			_tbExptPureCondition_IOS.DisposeLogic();
			_tbExptPureCondition_IOS = null;
		}
		if (_tbExptPure_IOS != null)
		{
			_tbExptPure_IOS.DisposeLogic();
			_tbExptPure_IOS = null;
		}
		if (_tbExptTraffic_IOS != null)
		{
			_tbExptTraffic_IOS.DisposeLogic();
			_tbExptTraffic_IOS = null;
		}
		if (_tbVibrationConfig_IOS != null)
		{
			_tbVibrationConfig_IOS.DisposeLogic();
			_tbVibrationConfig_IOS = null;
		}
	}
}
