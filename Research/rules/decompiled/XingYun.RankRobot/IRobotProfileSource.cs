using System;
using System.Collections.Generic;

namespace XingYun.RankRobot;

public interface IRobotProfileSource
{
	List<int> GetAllNameIds();

	RobotProfile GetRandomProfile(int nameId, Random rng);
}
