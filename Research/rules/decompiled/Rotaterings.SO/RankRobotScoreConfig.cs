using UnityEngine;
using XingYun.RankRobot;

namespace Rotaterings.SO;

[CreateAssetMenu(fileName = "RankRobotScoreConfig", menuName = "UI/RobotScoreConfig")]
public class RankRobotScoreConfig : ScriptableObject, IRankRobotScoreConfig
{
	private static RankRobotScoreConfig _instance;

	[Header("核心")]
	[Tooltip("机器人总数，用于 botPointMax 公式：GRAD_COEF * (BOTS_NUM - botRank)^2")]
	public int BOTS_NUM = 50;

	[Tooltip("分数梯度系数，控制不同排名机器人的积分上限差距")]
	public float GRAD_COEF = 0.018f;

	[Header("期望局数")]
	[Tooltip("默认期望局数，当活跃天数不足时使用此值")]
	public int EXP_GAMES_DEFAULT = 11;

	[Tooltip("活跃度统计天数，读取前 N 个自然日（不含当天）的通关数据")]
	public int ACTIVE_DAYS_COUNT = 2;

	[Tooltip("期望局数对数底数，公式：Log(avg) / Log(BASE) + OFFSET + 1")]
	public float EXP_GAMES_ACTIVE_BASE = 1.8f;

	[Tooltip("期望局数偏移值，加在对数计算结果上")]
	public float EXP_GAMES_ACTIVE_OFFSET = 6.5f;

	[Header("初始化积分权重 {分数:权重}")]
	[Tooltip("初始化积分可选值数组，与 WEIGHTS 一一对应")]
	public int[] INIT_POINTS_KEYS = new int[3] { 0, 2, 4 };

	[Tooltip("初始化积分权重数组，对应 KEYS 中每个值的随机概率权重")]
	public int[] INIT_POINTS_WEIGHTS = new int[3] { 4, 3, 3 };

	[Header("偷跑（首次通关）")]
	[Tooltip("偷跑系数，公式：botPointMax * (JUMP_COEF / (botRank + OFFSET_X) + OFFSET_Y)")]
	public float JUMP_COEF = 0.4f;

	[Tooltip("偷跑横向偏移，避免排名靠前的机器人偷跑值过大（分母加数）")]
	public float JUMP_OFFSET_X = 1f;

	[Tooltip("偷跑纵向偏移，为所有机器人提供一个基础偷跑比例")]
	public float JUMP_OFFSET_Y = 0.2f;

	[Header("追赶（非首次通关）")]
	[Tooltip("机器人每轮追赶局数，level_gap<=0 时用于计算追赶窗口")]
	public int BOT_CHASING_COUNT = 5;

	[Tooltip("机器人每轮追赶间隔，与 CHASING_COUNT 组成追赶周期")]
	public int BOT_CHASING_INTERVAL = 4;

	[Tooltip("追赶机器人单次加分上限，超过此值则 bot_add = 此值 - 2")]
	public int CHASING_BOTS_MAX_POINT = 6;

	[Header("顶分区得分权重 {分数:权重}")]
	[Tooltip("顶分区机器人加分可选值数组")]
	public int[] ADD_POINTS_TOP_KEYS = new int[5] { 2, 3, 4, 6, 8 };

	[Tooltip("顶分区加分权重数组，对应 KEYS 中每个值的随机概率权重")]
	public int[] ADD_POINTS_TOP_WEIGHTS = new int[5] { 3, 2, 5, 3, 1 };

	[Tooltip("顶分区机器人加分倍率可选值数组")]
	public int[] ODDS_LIST = new int[6] { 1, 2, 4, 6, 8, 10 };

	[Tooltip("顶分区加分倍率权重数组，对应 ODDS_LIST 中每个值的随机概率权重")]
	public int[] oddsWeight = new int[6] { 30, 40, 15, 3, 2, 0 };

	[Header("摆烂得分权重 {分数:权重}")]
	[Tooltip("摆烂机器人加分可选值数组")]
	public int[] STAG_POINTS_KEYS = new int[2] { 0, 2 };

	[Tooltip("摆烂加分权重数组，对应 KEYS 中每个值的随机概率权重")]
	public int[] STAG_POINTS_WEIGHTS = new int[2] { 1, 1 };

	[Header("非通关时间阈值")]
	[Tooltip("触发摆烂得分的最小时间间隔（秒），time_home - timeRefresh > 此值才触发")]
	public long STAG_TIME_INTERVAL = 3600L;

	[Range(0f, 1f)]
	[Tooltip("防偷家时间阈值比例：活动剩余时间高于实验周期的 STAG_TIME_CONTROL 比例时才允许摆烂，防止活动尾声机器人翻盘")]
	public float STAG_TIME_CONTROL = 0.6f;

	public static RankRobotScoreConfig Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_instance))
			{
				_instance = GameApp.Res.LoadAsset<RankRobotScoreConfig>("RankRobotScoreConfig", (string)null);
				if (!Object.op_Implicit((Object)(object)_instance))
				{
					_instance = ScriptableObject.CreateInstance<RankRobotScoreConfig>();
				}
			}
			return _instance;
		}
	}

	int IRankRobotScoreConfig.BOTS_NUM => BOTS_NUM;

	float IRankRobotScoreConfig.GRAD_COEF => GRAD_COEF;

	int IRankRobotScoreConfig.EXP_GAMES_DEFAULT => EXP_GAMES_DEFAULT;

	float IRankRobotScoreConfig.EXP_GAMES_ACTIVE_BASE => EXP_GAMES_ACTIVE_BASE;

	float IRankRobotScoreConfig.EXP_GAMES_ACTIVE_OFFSET => EXP_GAMES_ACTIVE_OFFSET;

	int[] IRankRobotScoreConfig.INIT_POINTS_KEYS => INIT_POINTS_KEYS;

	int[] IRankRobotScoreConfig.INIT_POINTS_WEIGHTS => INIT_POINTS_WEIGHTS;

	float IRankRobotScoreConfig.JUMP_COEF => JUMP_COEF;

	float IRankRobotScoreConfig.JUMP_OFFSET_X => JUMP_OFFSET_X;

	float IRankRobotScoreConfig.JUMP_OFFSET_Y => JUMP_OFFSET_Y;

	int IRankRobotScoreConfig.BOT_CHASING_COUNT => BOT_CHASING_COUNT;

	int IRankRobotScoreConfig.BOT_CHASING_INTERVAL => BOT_CHASING_INTERVAL;

	int IRankRobotScoreConfig.CHASING_BOTS_MAX_POINT => CHASING_BOTS_MAX_POINT;

	int[] IRankRobotScoreConfig.ADD_POINTS_TOP_KEYS => ADD_POINTS_TOP_KEYS;

	int[] IRankRobotScoreConfig.ADD_POINTS_TOP_WEIGHTS => ADD_POINTS_TOP_WEIGHTS;

	int[] IRankRobotScoreConfig.ODDS_LIST => ODDS_LIST;

	int[] IRankRobotScoreConfig.oddsWeight => oddsWeight;

	int[] IRankRobotScoreConfig.STAG_POINTS_KEYS => STAG_POINTS_KEYS;

	int[] IRankRobotScoreConfig.STAG_POINTS_WEIGHTS => STAG_POINTS_WEIGHTS;

	long IRankRobotScoreConfig.STAG_TIME_INTERVAL => STAG_TIME_INTERVAL;

	float IRankRobotScoreConfig.STAG_TIME_CONTROL => STAG_TIME_CONTROL;
}
