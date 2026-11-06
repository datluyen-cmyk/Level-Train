using System.Collections.Generic;
using RotateRings.Common;
using Unity.Collections;
using UnityEngine;

namespace XY.Common.ParticleImage;

public class XYParticle
{
	public struct TrailPoint
	{
		public Vector2 point;

		public float time;

		public TrailPoint(Vector2 p, float t)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			point = p;
			time = t;
		}
	}

	private XYParticleImage _source;

	private Transform _transform;

	private Vector2 _modifiedPosition;

	private Vector2 _position;

	private Vector2 _startVelocity;

	private Vector2 _velocity;

	private Vector3 _startRotation;

	private Vector3 _startSize;

	private float _startAlpha = 1f;

	private float _time;

	private float _normalizedTime;

	private Color _startColor;

	private float _lifetime;

	private Vector3 _size;

	private Color _color;

	private Vector3 _rotation;

	private float _handoffDuration;

	private float _handoffBlend;

	private Vector2 _bezierPathStart;

	private bool _bezierPhaseStarted;

	private float _sizeLerp;

	private float _alphaLerp;

	private float _colorLerp;

	private float _rotateLerp;

	private float _attractorLerp;

	private float _frameOverTimeLerp;

	private float _velocityLerp;

	private float _speedLerp;

	private float _startFrameLerp;

	private float _ratioRandom;

	private Vector2 _attractorTargetPoint;

	private Vector3 _lastTransformPosition;

	private Quaternion _lastTransformRotation;

	private Vector3 _transformDeltaRotation;

	private Vector2 _lastPosition;

	private Vector2 _deltaPosition;

	private Vector3 _direction;

	private Vector2 _trailLastPos;

	private Vector2 _trailDeltaPos;

	private bool _hasTrail;

	private float _frameDelta;

	private int _frameId;

	private int _sheetId;

	private int _burstOrder;

	private List<TrailPoint> _trailPoints = new List<TrailPoint>(128);

	private Vector2[] _points = (Vector2[])(object)new Vector2[4];

	private Vector2[] _rotations = (Vector2[])(object)new Vector2[4];

	private RectTransform _prefabInstance;

	private CanvasGroup _prefabCanvasGroup;

	private Vector3 _prefabSizeMultiplier = Vector3.one;

	private float _prefabAlpha = 1f;

	private Vector2 lastTrailPoint;

	public List<TrailPoint> trailPoints => _trailPoints;

	public Vector2[] points => _points;

	public int GetSheetId
	{
		get
		{
			if (_source.textureSheetEnabled)
			{
				return _sheetId;
			}
			return 0;
		}
	}

	public Vector2 Position => _position;

	public Vector2 Velocity => _velocity;

	public Vector2 Size => Vector2.op_Implicit(_size);

	public float TimeSinceBorn => _time;

	public float Lifetime => _lifetime;

	public Color Color => _color;

	public XYParticle(XYParticleImage source)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		_source = source;
		_transform = ((Component)source).transform;
		_trailLastPos = _position;
	}

	public void Initialize(Vector2 startPosition, Vector2 startVelocity, Vector3 startRotation, Color startColor, Vector3 startSize, float startAlpha, float lifetime, float startTime = 0f, Vector2? attractorTargetPoint = null, int burstOrder = 0)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		_burstOrder = burstOrder;
		_sizeLerp = Random.value;
		_alphaLerp = Random.value;
		_colorLerp = Random.value;
		_rotateLerp = Random.value;
		_attractorLerp = Random.value;
		_startFrameLerp = Random.value;
		_frameOverTimeLerp = Random.value;
		_velocityLerp = Random.value;
		_speedLerp = Random.value;
		_ratioRandom = Random.value;
		_attractorTargetPoint = (Vector2)(((_003F?)attractorTargetPoint) ?? new Vector2(Random.value, Random.value));
		_position = startPosition;
		_startVelocity = startVelocity;
		_startColor = startColor;
		_startSize = startSize;
		_startAlpha = startAlpha;
		_startRotation = startRotation;
		_lifetime = lifetime;
		_rotation = _startRotation;
		_lastTransformPosition = _transform.position;
		_modifiedPosition = _position;
		_velocity = Vector2.zero;
		_deltaPosition = Vector2.zero;
		_transformDeltaRotation = Vector3.zero;
		_direction = Vector3.zero;
		_color = _startColor;
		_size = _startSize;
		_prefabSizeMultiplier = Vector3.one;
		_prefabAlpha = _startAlpha;
		_lastPosition = _position;
		_time = startTime;
		_normalizedTime = 0f;
		_frameId = 0;
		_bezierPhaseStarted = false;
		_bezierPathStart = startPosition;
		float num = Mathf.Max(lifetime - _source.GetRuntimeFixedArriveDelay(_burstOrder), 0.0001f);
		if (_source.attractorPathMode == AttractorPathMode.Bezier)
		{
			_handoffDuration = Mathf.Min(_source.bezierHandoffDuration, num);
			_handoffBlend = Mathf.Min(_source.bezierHandoffBlend, Mathf.Max(0f, num - _handoffDuration));
		}
		else
		{
			_handoffDuration = 0f;
			_handoffBlend = 0f;
		}
		int frameId = _frameId;
		MinMaxCurve textureSheetStartFrame = _source.textureSheetStartFrame;
		_frameId = frameId + (int)((MinMaxCurve)(ref textureSheetStartFrame)).Evaluate(_time.Remap(0f, _lifetime, 0f, 1f), _startFrameLerp);
		_rotations[0] = new Vector2(_size.x / 2f, _size.y / 2f);
		_rotations[1] = new Vector2((0f - _size.x) / 2f, _size.y / 2f);
		_rotations[2] = new Vector2((0f - _size.x) / 2f, (0f - _size.y) / 2f);
		_rotations[3] = new Vector2(_size.x / 2f, (0f - _size.y) / 2f);
		if (!_source.IsPrefabMode && _source.trailsEnabled)
		{
			_trailPoints.Clear();
			_trailPoints.Add(new TrailPoint(_position, 0f));
			lastTrailPoint = _position;
			_hasTrail = _ratioRandom <= _source.trailRatio;
		}
	}

	public void BindPrefabInstance(RectTransform instance)
	{
		_prefabInstance = instance;
		_prefabCanvasGroup = ((Component)instance).GetComponent<CanvasGroup>();
		if ((Object)(object)_prefabCanvasGroup == (Object)null)
		{
			_prefabCanvasGroup = ((Component)instance).gameObject.AddComponent<CanvasGroup>();
		}
		SyncPrefabTransform();
	}

	public RectTransform UnbindPrefabInstance()
	{
		RectTransform prefabInstance = _prefabInstance;
		if ((Object)(object)_prefabCanvasGroup != (Object)null)
		{
			_prefabCanvasGroup.alpha = 1f;
		}
		_prefabCanvasGroup = null;
		_prefabInstance = null;
		return prefabInstance;
	}

	private void SyncPrefabTransform()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_prefabInstance == (Object)null))
		{
			((Transform)_prefabInstance).localPosition = new Vector3(_position.x, _position.y, 0f);
			((Transform)_prefabInstance).localEulerAngles = _rotation;
			Vector3 val = (((Object)(object)_source.particlePrefab != (Object)null) ? ((Transform)_source.particlePrefab).localScale : Vector3.one);
			((Transform)_prefabInstance).localScale = Vector3.Scale(val, _prefabSizeMultiplier);
			if ((Object)(object)_prefabCanvasGroup != (Object)null)
			{
				_prefabCanvasGroup.alpha = _prefabAlpha;
			}
		}
	}

	private float GetArrivePauseStart()
	{
		if (_source.attractorPathMode != AttractorPathMode.Bezier)
		{
			return 0f;
		}
		return _handoffDuration + _handoffBlend;
	}

	private static float ComputeVisualTime(float time, float pauseStart, float arriveDelay)
	{
		if (arriveDelay <= 0f)
		{
			return time;
		}
		if (time < pauseStart)
		{
			return time;
		}
		if (time < pauseStart + arriveDelay)
		{
			return pauseStart;
		}
		return time - arriveDelay;
	}

	public void Simulate(float deltaTime)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_08af: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c6: Expected I4, but got Unknown
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_090c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0911: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_0949: Unknown result type (might be due to invalid IL or missing references)
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Invalid comparison between Unknown and I4
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0958: Unknown result type (might be due to invalid IL or missing references)
		//IL_096f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0974: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_097d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0987: Unknown result type (might be due to invalid IL or missing references)
		//IL_098c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0991: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_074e: Invalid comparison between Unknown and I4
		//IL_0a3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Invalid comparison between Unknown and I4
		//IL_0a9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0851: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aaa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aaf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0868: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Unknown result type (might be due to invalid IL or missing references)
		//IL_087f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_0888: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1088: Unknown result type (might be due to invalid IL or missing references)
		//IL_108d: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_10bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b50: Unknown result type (might be due to invalid IL or missing references)
		//IL_1167: Unknown result type (might be due to invalid IL or missing references)
		//IL_116c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1171: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_11aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_11da: Unknown result type (might be due to invalid IL or missing references)
		//IL_11df: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1213: Unknown result type (might be due to invalid IL or missing references)
		//IL_1218: Unknown result type (might be due to invalid IL or missing references)
		//IL_121d: Unknown result type (might be due to invalid IL or missing references)
		//IL_122a: Unknown result type (might be due to invalid IL or missing references)
		//IL_123c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1248: Unknown result type (might be due to invalid IL or missing references)
		//IL_124d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1252: Unknown result type (might be due to invalid IL or missing references)
		//IL_125f: Unknown result type (might be due to invalid IL or missing references)
		//IL_126b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1270: Unknown result type (might be due to invalid IL or missing references)
		//IL_1275: Unknown result type (might be due to invalid IL or missing references)
		//IL_1282: Unknown result type (might be due to invalid IL or missing references)
		//IL_128e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1293: Unknown result type (might be due to invalid IL or missing references)
		//IL_1298: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_12bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f08: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e00: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e12: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dcf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dd7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ddc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d51: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d22: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0faa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0faf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fe4: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("XYParticle.Simulate"))
		{
			_time += deltaTime;
			float num = ((_source.attractorEnabled && Object.op_Implicit((Object)(object)_source.attractorTarget)) ? _source.GetRuntimeFixedArriveDelay(_burstOrder) : 0f);
			float arrivePauseStart = GetArrivePauseStart();
			float num2 = ComputeVisualTime(_time, arrivePauseStart, num);
			float num3 = Mathf.Max(_lifetime - num, 0.0001f);
			_normalizedTime = num2.Remap(0f, num3, 0f, 1f);
			bool flag = num > 0f && _time >= arrivePauseStart && _time < arrivePauseStart + num;
			Vector2 startVelocity = _startVelocity;
			MinMaxCurve val = _source.speedOverLifetime;
			_velocity = startVelocity * ((MinMaxCurve)(ref val)).Evaluate(_normalizedTime, _speedLerp);
			Quaternion val3;
			if (_source.space == Simulation.World)
			{
				Vector3 val2 = _transform.InverseTransformPoint(_lastTransformPosition);
				_modifiedPosition += new Vector2(val2.x, val2.y);
				val3 = Quaternion.Inverse(_transform.rotation);
				Vector3 eulerAngles = ((Quaternion)(ref val3)).eulerAngles;
				val3 = Quaternion.Inverse(_lastTransformRotation);
				_transformDeltaRotation = eulerAngles - ((Quaternion)(ref val3)).eulerAngles;
				_modifiedPosition = RotatePointAroundCenter(_modifiedPosition, _transformDeltaRotation);
				_startVelocity = RotatePointAroundCenter(_startVelocity, _transformDeltaRotation);
				_lastTransformPosition = _transform.position;
				_lastTransformRotation = _transform.rotation;
			}
			if (_source.TryGetVelocityOverLifetimeXY(_normalizedTime, _velocityLerp, out var delta))
			{
				_velocity += delta;
			}
			if (!flag)
			{
				_modifiedPosition += _velocity * (deltaTime * 100f);
			}
			if (_source.attractorEnabled && Object.op_Implicit((Object)(object)_source.attractorTarget))
			{
				if (!(_source.attractorTarget is RectTransform))
				{
					_source.attractorType = AttractorType.Pivot;
				}
				Vector2 val4 = _source.ResolveAttractorEndPoint(_attractorTargetPoint);
				if (_source.attractorPathMode == AttractorPathMode.Bezier)
				{
					if (num2 < _handoffDuration)
					{
						_position = _modifiedPosition;
					}
					else
					{
						if (!_bezierPhaseStarted)
						{
							_bezierPhaseStarted = true;
							_bezierPathStart = _modifiedPosition;
						}
						float num4 = Mathf.Max(num3 - _handoffDuration, 0.0001f);
						float normalizedTime = Mathf.Clamp(num2 - _handoffDuration, 0f, num4).Remap(0f, num4, 0f, 1f);
						float t = _source.EvaluateBezierProgress(normalizedTime, _attractorLerp);
						Vector2 control = _source.ComputeBezierControlPoint(_bezierPathStart, val4);
						Vector2 val5 = XYParticleImage.EvaluateQuadraticBezier(_bezierPathStart, control, val4, t);
						float num5 = _handoffDuration + _handoffBlend;
						if (_handoffBlend > 0f && num2 < num5)
						{
							float num6 = Mathf.Clamp01((num2 - _handoffDuration) / _handoffBlend);
							num6 = Mathf.SmoothStep(0f, 1f, num6);
							_position = Vector2.Lerp(_modifiedPosition, val5, num6);
						}
						else
						{
							_position = val5;
						}
					}
				}
				else if (flag)
				{
					_position = _modifiedPosition;
				}
				else
				{
					val = _source.attractorLerp;
					float num7 = ((MinMaxCurve)(ref val)).Evaluate(_normalizedTime, _attractorLerp);
					_position = Vector2.LerpUnclamped(_modifiedPosition, val4, num7);
				}
			}
			else
			{
				_position = _modifiedPosition;
			}
			_deltaPosition = _position - _lastPosition;
			_lastPosition = _position;
			float num8 = ((Vector2)(ref _deltaPosition)).magnitude * (1f / deltaTime) / 100f;
			if (float.IsNaN(num8))
			{
				num8 = 0f;
			}
			MinMaxGradient val6;
			if (_source.IsPrefabMode)
			{
				val = _source.alphaOverLifetime;
				float num9 = ((MinMaxCurve)(ref val)).Evaluate(_normalizedTime, _alphaLerp);
				val = _source.alphaBySpeed;
				float num10 = ((MinMaxCurve)(ref val)).Evaluate(num8.Remap(_source.alphaSpeedRange.from, _source.alphaSpeedRange.to, 0f, 1f), _alphaLerp);
				_prefabAlpha = Mathf.Clamp01(_startAlpha * num9 * num10);
			}
			else
			{
				val6 = _source.colorOverLifetime;
				Color val7 = ((MinMaxGradient)(ref val6)).Evaluate(_normalizedTime, _colorLerp);
				Color val8 = _startColor * val7;
				val6 = _source.colorBySpeed;
				_color = val8 * ((MinMaxGradient)(ref val6)).Evaluate(num8.Remap(_source.colorSpeedRange.from, _source.colorSpeedRange.to, 0f, 1f), _colorLerp);
			}
			XYSeparatedMinMaxCurve xYSeparatedMinMaxCurve = _source.sizeOverLifetime;
			Vector3 val9 = xYSeparatedMinMaxCurve.Evaluate(_normalizedTime, _sizeLerp);
			xYSeparatedMinMaxCurve = _source.sizeBySpeed;
			Vector3 val10 = xYSeparatedMinMaxCurve.Evaluate(num8.Remap(_source.sizeSpeedRange.from, _source.sizeSpeedRange.to, 0f, 1f), _sizeLerp);
			_size = Vector3.Scale(_startSize, Vector3.Scale(val10, val9));
			_size = _source.FitParticleSizeToSpriteAspect(_size);
			_prefabSizeMultiplier = Vector3.Scale(val10, val9);
			_direction = Vector2.op_Implicit(_deltaPosition);
			if (((Vector3)(ref _direction)).magnitude == 0f)
			{
				_direction = Vector2.op_Implicit(_velocity);
			}
			_direction = ((Vector3)(ref _direction)).normalized;
			Vector3 val11 = Vector3.zero;
			float num11;
			float num12;
			float num13;
			if (_source.rotationOverLifetime.separated)
			{
				num11 = 0f;
				num12 = 0f;
				num13 = 0f;
				xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
				if ((int)((MinMaxCurve)(ref xYSeparatedMinMaxCurve.xCurve)).mode != 0)
				{
					xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
					if ((int)((MinMaxCurve)(ref xYSeparatedMinMaxCurve.xCurve)).mode != 3)
					{
						xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
						num11 = ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.xCurve)).Evaluate(_normalizedTime, _rotateLerp);
						goto IL_0719;
					}
				}
				xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
				num11 = num2.Remap(0f, num3, 0f, ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.xCurve)).Evaluate(_rotateLerp, _rotateLerp));
				goto IL_0719;
			}
			xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
			ParticleSystemCurveMode mode = ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.mainCurve)).mode;
			switch ((int)mode)
			{
			case 0:
			case 3:
				xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
				val11 = new Vector3(0f, 0f, num2.Remap(0f, num3, 0f, ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.mainCurve)).Evaluate(_normalizedTime, _rotateLerp)));
				break;
			case 1:
			case 2:
				xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
				val11 = new Vector3(0f, 0f, ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.mainCurve)).Evaluate(_normalizedTime, _rotateLerp));
				break;
			}
			if (!_source.alignToDirection)
			{
				Vector3 val12 = val11;
				val3 = Quaternion.Inverse(((Component)_source).transform.rotation);
				val11 = val12 + new Vector3(0f, 0f, ((Quaternion)(ref val3)).eulerAngles.z);
			}
			goto IL_0993;
			IL_084b:
			val11 = new Vector3(num11, num12, num13);
			if (!_source.alignToDirection)
			{
				Vector3 val13 = val11;
				val3 = Quaternion.Inverse(((Component)_source).transform.rotation);
				val11 = val13 + ((Quaternion)(ref val3)).eulerAngles;
			}
			goto IL_0993;
			IL_0993:
			Vector3 val14;
			if (_source.rotationBySpeed.separated)
			{
				xYSeparatedMinMaxCurve = _source.rotationBySpeed;
				val14 = xYSeparatedMinMaxCurve.Evaluate(num8.Remap(_source.rotationSpeedRange.from, _source.rotationSpeedRange.to, 0f, 1f), _rotateLerp);
			}
			else
			{
				xYSeparatedMinMaxCurve = _source.rotationBySpeed;
				val14 = xYSeparatedMinMaxCurve.EvaluateZ(num8.Remap(_source.rotationSpeedRange.from, _source.rotationSpeedRange.to, 0f, 1f), _rotateLerp);
			}
			if (_source.alignToDirection)
			{
				Quaternion val15 = Quaternion.FromToRotation(Vector3.up, _direction);
				Vector3 startRotation = _startRotation;
				val3 = Quaternion.Euler(new Vector3(0f, 0f, ((Quaternion)(ref val15)).eulerAngles.z));
				_rotation = startRotation + ((Quaternion)(ref val3)).eulerAngles;
			}
			else
			{
				_rotation = _startRotation;
			}
			_rotation += val11 + val14;
			if (_source.IsPrefabMode)
			{
				SyncPrefabTransform();
				return;
			}
			if (_source.trailsEnabled && _trailPoints.Count > 0)
			{
				NativeArray<Vector3> val16 = default(NativeArray<Vector3>);
				val16._002Ector(_trailPoints.Count * 2, (Allocator)2, (NativeArrayOptions)1);
				NativeArray<Color> cols = default(NativeArray<Color>);
				cols._002Ector(_trailPoints.Count * 2, (Allocator)2, (NativeArrayOptions)1);
				NativeArray<int> tris = default(NativeArray<int>);
				tris._002Ector((_trailPoints.Count - 1) * 6, (Allocator)2, (NativeArrayOptions)1);
				try
				{
					if (_time < _lifetime)
					{
						TrailPoint value = new TrailPoint(_position, _time);
						_trailPoints[_trailPoints.Count - 1] = value;
					}
					if (trailPoints.Count > 1)
					{
						TrailPoint value2 = _trailPoints[0];
						value2.point = Vector2.Lerp(trailPoints[1].point, lastTrailPoint, Mathf.Abs(_time.Remap(_trailPoints[0].time + _source.trailLifetime, _trailPoints[1].time + _source.trailLifetime, 0f, 1f)));
						_trailPoints[0] = value2;
					}
					float to = Vector3.Distance(Vector2.op_Implicit(_trailPoints[0].point), Vector2.op_Implicit(_position));
					for (int i = 0; i < _trailPoints.Count; i++)
					{
						Vector2 val17 = _trailPoints[i].point;
						float num14 = ((i > 0) ? Vector3.Distance(Vector2.op_Implicit(_trailPoints[0].point), Vector2.op_Implicit(_trailPoints[i].point)).Remap(0f, to, 1f, 0f) : 1f);
						float x = _size.x;
						val = _source.trailWidth;
						float num15 = x * ((MinMaxCurve)(ref val)).Evaluate(num14, _sizeLerp);
						if (_trailPoints.Count > 1)
						{
							val17 = ((i >= _trailPoints.Count - 1) ? (_trailPoints[i].point - _trailPoints[i - 1].point) : (_trailPoints[i + 1].point - _trailPoints[i].point));
						}
						Vector2 point = _trailPoints[i].point;
						Vector2 val18 = Vector2.Perpendicular(((Vector2)(ref val17)).normalized);
						val6 = _source.trailColorOverTrail;
						Color val19 = ((MinMaxGradient)(ref val6)).Evaluate(num14, _colorLerp);
						val6 = _source.trailColorOverLifetime;
						Color val20 = val19 * ((MinMaxGradient)(ref val6)).Evaluate(_normalizedTime, _colorLerp);
						if (_source.inheritParticleColor)
						{
							val20 *= _color;
						}
						val16[i * 2 + 1] = Vector2.op_Implicit(point + val18 * num15 / 2f);
						val16[i * 2] = Vector2.op_Implicit(point - val18 * num15 / 2f);
						cols[i * 2] = val20;
						cols[i * 2 + 1] = val20;
					}
					for (int j = 0; j < trailPoints.Count - 1; j++)
					{
						tris[j * 6] = j * 2;
						tris[j * 6 + 1] = j * 2 + 1;
						tris[j * 6 + 2] = j * 2 + 2;
						tris[j * 6 + 3] = j * 2 + 2;
						tris[j * 6 + 4] = j * 2 + 1;
						tris[j * 6 + 5] = j * 2 + 3;
					}
					_source.particleTrailRenderer?.UpdateMeshData(val16, tris, cols);
				}
				finally
				{
					if (val16.IsCreated)
					{
						val16.Dispose();
					}
					if (cols.IsCreated)
					{
						cols.Dispose();
					}
					if (tris.IsCreated)
					{
						tris.Dispose();
					}
				}
				if (_time >= _trailPoints[0].time + _source.trailLifetime)
				{
					_trailPoints.RemoveAt(0);
					lastTrailPoint = _trailPoints[0].point;
				}
				if (_time < _lifetime && _hasTrail)
				{
					_trailDeltaPos = _trailLastPos - _position;
					if (((Vector2)(ref _trailDeltaPos)).magnitude > _source.minimumVertexDistance)
					{
						_trailLastPos = _position;
						_trailPoints.Add(new TrailPoint(_position, _time));
					}
				}
			}
			SpriteSheet[] sheets = _source.Sheets;
			int num16 = ((sheets == null || sheets.Length == 0) ? 1 : sheets.Length);
			switch (_source.textureSheetType)
			{
			case SheetType.Speed:
				_frameId = (int)((Vector2)(ref _velocity)).magnitude.Remap(_source.textureSheetFrameSpeedRange.from, _source.textureSheetFrameSpeedRange.to, 0f, num16);
				break;
			case SheetType.Lifetime:
			{
				val = _source.textureSheetFrameOverTime;
				int num18 = (int)(((MinMaxCurve)(ref val)).Evaluate(_normalizedTime, _frameOverTimeLerp) * (float)_source.textureSheetCycles);
				val = _source.textureSheetStartFrame;
				_frameId = num18 + (int)((MinMaxCurve)(ref val)).Evaluate(_normalizedTime, _startFrameLerp);
				break;
			}
			case SheetType.FPS:
				if (!flag)
				{
					float num17 = 1f / (float)_source.textureSheetFPS;
					_frameDelta += deltaTime;
					while (_frameDelta >= num17)
					{
						_frameDelta -= num17;
						_frameId++;
					}
				}
				break;
			}
			_sheetId = (int)Mathf.Repeat((float)_frameId, (float)num16);
			_rotations[0] = Vector2.op_Implicit(new Vector3(_size.x / 2f, _size.y / 2f));
			_rotations[1] = Vector2.op_Implicit(new Vector3((0f - _size.x) / 2f, _size.y / 2f));
			_rotations[2] = Vector2.op_Implicit(new Vector3((0f - _size.x) / 2f, (0f - _size.y) / 2f));
			_rotations[3] = Vector2.op_Implicit(new Vector3(_size.x / 2f, (0f - _size.y) / 2f));
			RotatePointsAroundCenter(_rotations, _rotation);
			_points[0] = _position + _rotations[0];
			_points[1] = _position + _rotations[1];
			_points[2] = _position + _rotations[2];
			_points[3] = _position + _rotations[3];
			return;
			IL_0719:
			xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
			if ((int)((MinMaxCurve)(ref xYSeparatedMinMaxCurve.yCurve)).mode != 0)
			{
				xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
				if ((int)((MinMaxCurve)(ref xYSeparatedMinMaxCurve.yCurve)).mode != 3)
				{
					xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
					num12 = ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.yCurve)).Evaluate(_normalizedTime, _rotateLerp);
					goto IL_07b2;
				}
			}
			xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
			num12 = num2.Remap(0f, num3, 0f, ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.yCurve)).Evaluate(_rotateLerp, _rotateLerp));
			goto IL_07b2;
			IL_07b2:
			xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
			if ((int)((MinMaxCurve)(ref xYSeparatedMinMaxCurve.zCurve)).mode != 0)
			{
				xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
				if ((int)((MinMaxCurve)(ref xYSeparatedMinMaxCurve.zCurve)).mode != 3)
				{
					xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
					num13 = ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.zCurve)).Evaluate(_normalizedTime, _rotateLerp);
					goto IL_084b;
				}
			}
			xYSeparatedMinMaxCurve = _source.rotationOverLifetime;
			num13 = num2.Remap(0f, num3, 0f, ((MinMaxCurve)(ref xYSeparatedMinMaxCurve.zCurve)).Evaluate(_rotateLerp, _rotateLerp));
			goto IL_084b;
		}
	}

	private void RotatePointsAroundCenter(Vector2[] points, Vector3 angles)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Euler(angles);
		for (int i = 0; i < points.Length; i++)
		{
			points[i] = Vector2.op_Implicit(val * Vector2.op_Implicit(points[i]));
		}
	}

	private Vector2 RotatePointAroundCenter(Vector2 point, Vector3 angles)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(Quaternion.Euler(angles) * Vector2.op_Implicit(point));
	}
}
