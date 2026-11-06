using System;
using System.Linq;
using RotateRings.GamePlay.Objects;
using UniLog;
using UnityEngine;
using UnlockRing.Core.Authoring;
using UnlockRing.Core.Geometry;

public class ButterflyComponentBuilder
{
	private const string TAG = "[UnlockRing][ButterflyComponentBuilder] ";

	private readonly string physicCatchButterflyPrefabPath;

	private readonly ButterflyColliderBuilder colliderBuilder;

	public ButterflyComponentBuilder(string physicCatchButterflyPrefabPath = "PhysicCatchButterfly")
	{
		this.physicCatchButterflyPrefabPath = physicCatchButterflyPrefabPath;
		colliderBuilder = new ButterflyColliderBuilder();
	}

	public LevelComponentFactory.CreateResult Create(ButterflyClipDraft draft, Transform parent, LevelComponentFactory.CoordSpace cs)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Resources.Load<GameObject>(physicCatchButterflyPrefabPath);
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][ButterflyComponentBuilder] ", "Cannot load {0}.prefab from Resources.", new object[1] { physicCatchButterflyPrefabPath });
			return LevelComponentFactory.CreateResult.Empty;
		}
		ButterflyGuideAttachmentDraft[] array = ((EditablePartDraft)draft).Components.OfType<ButterflyGuideAttachmentDraft>().ToArray();
		double num = 1.0;
		double num2 = 1.0;
		string guideIdForSlot = null;
		string guideIdForSlot2 = null;
		foreach (ButterflyGuideAttachmentDraft val2 in array)
		{
			num = Math.Max(num, val2.RailWidth);
			num2 = Math.Max(num2, val2.RailLength);
			if (val2.MountingPosition == 0)
			{
				guideIdForSlot = ((EditableBodyComponentDraft)val2).Id;
			}
			else if (val2.MountingPosition == 1)
			{
				guideIdForSlot2 = ((EditableBodyComponentDraft)val2).Id;
			}
		}
		GameObject val3 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val3).name = "Butterfly_" + ((EditablePartDraft)draft).Id;
		Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditablePartDraft)draft).X, ((EditablePartDraft)draft).Y));
		localPosition.z = -1f;
		val3.transform.localPosition = localPosition;
		val3.transform.localRotation = Quaternion.Euler(0f, 0f, cs.ToUnityRotationDeg(((EditablePartDraft)draft).RotationDegrees));
		double value = Math.Max(draft.BarWidth, num) * 0.5;
		float num3 = cs.ToUnityScalar(value) * 2f * 1.6f;
		val3.transform.localScale = new Vector3(num3, num3, 1f);
		FixedButterflyObject component = val3.GetComponent<FixedButterflyObject>();
		if (component != null)
		{
			component.PartData = GameplayPartData.Create(((EditablePartDraft)draft).Id, ((EditablePartDraft)draft).AssemblyId);
			component.SetSlotGuidePartIds(guideIdForSlot, guideIdForSlot2);
		}
		FixedButterflyMeshController component2 = val3.GetComponent<FixedButterflyMeshController>();
		if (component2 != null)
		{
			float num4 = ((num3 > 0f) ? (1f / num3) : 1f);
			float scale = cs.Scale;
			component2.ConfigureGeometry((float)(draft.BarLength + num * 2.0) * scale * num4, (float)draft.BarWidth * scale * num4, (float)num2 * scale * num4, (float)num * scale * num4);
			FixedButterflyObject component3 = val3.GetComponent<FixedButterflyObject>();
			if (component3 != null)
			{
				colliderBuilder.BuildPhysic(component3, component2, (float)num2 * scale * num4, (float)num * scale * num4, (float)draft.BarLength * scale * num4, (float)draft.BarWidth * scale * num4);
			}
		}
		float num5 = (float)Math.Max(draft.BarLength + num * 2.0, num2) * 0.5f;
		Aabb value2 = default(Aabb);
		((Aabb)(ref value2))._002Ector(new Vector2((double)((float)((EditablePartDraft)draft).X - num5), (double)((float)((EditablePartDraft)draft).Y - num5)), new Vector2((double)((float)((EditablePartDraft)draft).X + num5), (double)((float)((EditablePartDraft)draft).Y + num5)));
		return new LevelComponentFactory.CreateResult(val3, value2);
	}
}
