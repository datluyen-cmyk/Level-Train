using System;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class BehaviorDto
{
	public string kind;

	public VecDto localAxis;

	public double clearance;

	public string compatibilityTag;

	public BehaviorDefinition ToDomain()
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Expected O, but got Unknown
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected O, but got Unknown
		return (BehaviorDefinition)(kind switch
		{
			"rotatable" => (object)new RotatableBehaviorDefinition(0), 
			"axisSlidable" => (object)new AxisSlidableBehaviorDefinition(localAxis?.ToDomain() ?? Vector2D.Zero), 
			"linearGuide" => (object)new LinearGuideBehaviorDefinition(localAxis?.ToDomain() ?? Vector2D.Zero, clearance), 
			"threadable" => (object)new ThreadableBehaviorDefinition(compatibilityTag ?? ""), 
			"catch" => (object)new CatchBehaviorDefinition(compatibilityTag ?? ""), 
			"hook" => (object)new HookBehaviorDefinition(compatibilityTag ?? ""), 
			"bomb" => (object)new BombBehaviorDefinition(), 
			"forceRemovable" => (object)new ForceRemovableBehaviorDefinition(), 
			"releasable" => (object)new ReleasableBehaviorDefinition(), 
			_ => throw new InvalidOperationException("Unknown behavior kind: " + kind), 
		});
	}

	public static BehaviorDto FromDomain(BehaviorDefinition b)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		BehaviorDto behaviorDto = new BehaviorDto
		{
			kind = b.Kind
		};
		AxisSlidableBehaviorDefinition val = (AxisSlidableBehaviorDefinition)(object)((b is AxisSlidableBehaviorDefinition) ? b : null);
		if (val == null)
		{
			LinearGuideBehaviorDefinition val2 = (LinearGuideBehaviorDefinition)(object)((b is LinearGuideBehaviorDefinition) ? b : null);
			if (val2 == null)
			{
				ThreadableBehaviorDefinition val3 = (ThreadableBehaviorDefinition)(object)((b is ThreadableBehaviorDefinition) ? b : null);
				if (val3 == null)
				{
					CatchBehaviorDefinition val4 = (CatchBehaviorDefinition)(object)((b is CatchBehaviorDefinition) ? b : null);
					if (val4 == null)
					{
						HookBehaviorDefinition val5 = (HookBehaviorDefinition)(object)((b is HookBehaviorDefinition) ? b : null);
						if (val5 != null)
						{
							behaviorDto.compatibilityTag = val5.CompatibilityTag;
						}
					}
					else
					{
						behaviorDto.compatibilityTag = val4.CompatibilityTag;
					}
				}
				else
				{
					behaviorDto.compatibilityTag = val3.CompatibilityTag;
				}
			}
			else
			{
				behaviorDto.localAxis = VecDto.FromDomain(val2.LocalAxis);
				behaviorDto.clearance = val2.Clearance;
			}
		}
		else
		{
			behaviorDto.localAxis = VecDto.FromDomain(val.LocalAxis);
		}
		return behaviorDto;
	}
}
