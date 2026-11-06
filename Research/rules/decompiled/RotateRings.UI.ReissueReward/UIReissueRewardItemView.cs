using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using Game.UI;
using Script.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.ReissueReward;

public class UIReissueRewardItemView : GameWidgetView
{
	public Image imgBg;

	public TMP_Text txtCount;

	public Image imgItem;

	public Animation anim;

	private CanvasGroup _canvasGroup;

	private const string Appear = "anim_ReissueRewardItem_start";

	private const float AppearDelay = 0.2f;

	private static readonly string[] SimulationSpaceNodeNames = new string[2] { "xx1_1", "dd_1" };

	private UIParticle _uiParticle;

	private List<ParticleSystem> _simulationSpaceSystems;

	private Particle[] _particleBuffer;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		imgBg = behaviour.Get<Image>("imgBg");
		imgItem = behaviour.Get<Image>("imgItem");
		txtCount = behaviour.Get<TMP_Text>("txtCount");
		anim = behaviour.Get<Animation>("anim");
		_canvasGroup = ((Component)behaviour).GetComponent<CanvasGroup>();
		_uiParticle = ((Component)behaviour).GetComponentInChildren<UIParticle>(true);
	}

	public void OnRender(string itemSpriteName, int itemCount)
	{
		imgItem.SetAtlasSprite("Prop", itemSpriteName);
		txtCount.text = itemCount.ToString();
		ApplySimulationSpace((ParticleSystemSimulationSpace)0);
		if ((Object)(object)_canvasGroup != (Object)null)
		{
			_canvasGroup.alpha = 0f;
		}
		if (!((Object)(object)anim == (Object)null))
		{
			GameApp.Timer.Delay((Object)(object)anim, 0.2f, (Action)PlayAppearAnimation);
		}
	}

	public void EnterFlySimulationSpace()
	{
		ApplySimulationSpace((ParticleSystemSimulationSpace)1);
	}

	private List<ParticleSystem> GetSimulationSpaceSystems()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (_simulationSpaceSystems != null)
		{
			return _simulationSpaceSystems;
		}
		List<ParticleSystem> list = new List<ParticleSystem>(SimulationSpaceNodeNames.Length);
		IReadOnlyList<ParticleSystem> readOnlyList;
		if (!((Object)(object)_uiParticle != (Object)null))
		{
			IReadOnlyList<ParticleSystem> componentsInChildren = ((Component)((UIBaseView)this).Behaviour).GetComponentsInChildren<ParticleSystem>(true);
			readOnlyList = componentsInChildren;
		}
		else
		{
			IReadOnlyList<ParticleSystem> componentsInChildren = _uiParticle.particles;
			readOnlyList = componentsInChildren;
		}
		IReadOnlyList<ParticleSystem> readOnlyList2 = readOnlyList;
		int num = 0;
		for (int i = 0; i < readOnlyList2.Count; i++)
		{
			ParticleSystem val = readOnlyList2[i];
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			string name = ((Object)((Component)val).gameObject).name;
			for (int j = 0; j < SimulationSpaceNodeNames.Length; j++)
			{
				if (!(name != SimulationSpaceNodeNames[j]))
				{
					list.Add(val);
					int num2 = num;
					MainModule main = val.main;
					num = Mathf.Max(num2, ((MainModule)(ref main)).maxParticles);
					break;
				}
			}
		}
		if (num > 0 && (_particleBuffer == null || _particleBuffer.Length < num))
		{
			_particleBuffer = (Particle[])(object)new Particle[Mathf.NextPowerOfTwo(num)];
		}
		_simulationSpaceSystems = list;
		return _simulationSpaceSystems;
	}

	private void ApplySimulationSpace(ParticleSystemSimulationSpace space)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		List<ParticleSystem> simulationSpaceSystems = GetSimulationSpaceSystems();
		for (int i = 0; i < simulationSpaceSystems.Count; i++)
		{
			ParticleSystem val = simulationSpaceSystems[i];
			if (!((Object)(object)val == (Object)null))
			{
				MainModule main = val.main;
				if (((MainModule)(ref main)).simulationSpace != space)
				{
					ConvertLiveParticles(val, ((MainModule)(ref main)).simulationSpace, space);
					((MainModule)(ref main)).simulationSpace = space;
				}
			}
		}
	}

	private void ConvertLiveParticles(ParticleSystem system, ParticleSystemSimulationSpace from, ParticleSystemSimulationSpace to)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)from == 2 || (int)to == 2)
		{
			return;
		}
		int particleCount = system.particleCount;
		if (particleCount <= 0)
		{
			return;
		}
		if (_particleBuffer == null || _particleBuffer.Length < particleCount)
		{
			_particleBuffer = (Particle[])(object)new Particle[Mathf.NextPowerOfTwo(particleCount)];
		}
		int particles = system.GetParticles(_particleBuffer, particleCount);
		if (particles > 0)
		{
			Vector3 val = GetSimulationSpaceOffset(system);
			if ((int)to == 0)
			{
				val = -val;
			}
			for (int i = 0; i < particles; i++)
			{
				Particle val2 = _particleBuffer[i];
				((Particle)(ref val2)).position = ((Particle)(ref val2)).position + val;
				_particleBuffer[i] = val2;
			}
			system.SetParticles(_particleBuffer, particles);
		}
	}

	private Vector3 GetSimulationSpaceOffset(ParticleSystem system)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Vector3 uIParticleRenderScale = GetUIParticleRenderScale(system);
		if (Mathf.Approximately(uIParticleRenderScale.x, 0f) || Mathf.Approximately(uIParticleRenderScale.y, 0f) || Mathf.Approximately(uIParticleRenderScale.z, 0f))
		{
			return Vector3.zero;
		}
		Vector3 position = ((Component)system).transform.position;
		return new Vector3(position.x / uIParticleRenderScale.x, position.y / uIParticleRenderScale.y, position.z / uIParticleRenderScale.z);
	}

	private Vector3 GetUIParticleRenderScale(ParticleSystem system)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_uiParticle == (Object)null)
		{
			return Vector3.one;
		}
		Vector3 val = Vector3.Scale(_uiParticle.scale3DForCalc, _uiParticle.parentScale);
		if ((int)_uiParticle.autoScalingMode == 1)
		{
			MainModule main = system.main;
			if ((int)((MainModule)(ref main)).scalingMode == 1 && (Object)(object)((Graphic)_uiParticle).canvas != (Object)null)
			{
				val = Vector3.Scale(val, ((Component)((Graphic)_uiParticle).canvas.rootCanvas).transform.localScale);
			}
		}
		return val;
	}

	private void PlayAppearAnimation()
	{
		if ((Object)(object)anim == (Object)null)
		{
			return;
		}
		AnimationState val = anim["anim_ReissueRewardItem_start"];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (anim.IsPlaying("anim_ReissueRewardItem_start"))
			{
				anim.Stop("anim_ReissueRewardItem_start");
				val.time = 0f;
				anim.Sample();
			}
			val.wrapMode = (WrapMode)8;
			anim.Play("anim_ReissueRewardItem_start");
		}
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
