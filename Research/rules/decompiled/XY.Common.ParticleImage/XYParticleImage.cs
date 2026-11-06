using System;
using System.Collections.Generic;
using RotateRings.Common;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace XY.Common.ParticleImage;

[AddComponentMenu("UI/XY Particle Image/XY Particle Image")]
[RequireComponent(typeof(CanvasRenderer))]
public sealed class XYParticleImage : MaskableGraphic
{
	private struct BurstSpawnParams
	{
		public Vector2 startPosition;

		public Vector3 startVelocity;

		public Vector2 attractorRandom;

		public float rotationLerpA;

		public float rotationLerpB;

		public float colorLerpA;

		public float colorLerpB;

		public float sizeLerpA;

		public float sizeLerpB;

		public float alphaLerpA;

		public float alphaLerpB;
	}

	[SerializeField]
	private XYParticleImage _main;

	[SerializeField]
	private XYParticleImage[] _children;

	private RectTransform _canvasRect;

	private Mesh _mesh;

	[SerializeField]
	private Simulation _space;

	[SerializeField]
	private TimeScale _timeScale = TimeScale.Normal;

	[SerializeField]
	private EmitterShape _shape = EmitterShape.Circle;

	[SerializeField]
	private SpreadType _spread;

	[SerializeField]
	private float _spreadLoop = 1f;

	[SerializeField]
	private float _startDelay;

	[SerializeField]
	private float _radius = 50f;

	[SerializeField]
	private float _width = 100f;

	[SerializeField]
	private float _height = 100f;

	[SerializeField]
	private float _angle = 45f;

	[SerializeField]
	private EmitterDirectionMode _directionMode;

	[SerializeField]
	private float _length = 100f;

	[SerializeField]
	private bool _fitRect;

	[SerializeField]
	private bool _emitOnSurface = true;

	[SerializeField]
	private float _emitterThickness;

	[SerializeField]
	private bool _loop = true;

	[SerializeField]
	private bool _prewarm;

	[SerializeField]
	private float _duration = 5f;

	[SerializeField]
	private PlayMode _playMode = PlayMode.OnAwake;

	[SerializeField]
	private XYSeparatedMinMaxCurve _startSize = new XYSeparatedMinMaxCurve(40f);

	public const float MinStartSize = 1f;

	[SerializeField]
	private MinMaxGradient _startColor = new MinMaxGradient(Color.white);

	[SerializeField]
	private MinMaxCurve _startAlpha = new MinMaxCurve(1f);

	[SerializeField]
	private MinMaxCurve _lifetime = new MinMaxCurve(1f);

	[SerializeField]
	private XYLifetimeMode _lifetimeMode;

	[SerializeField]
	private float _flightLifetimeT0 = 0.2f;

	[SerializeField]
	private float _flightLifetimeTs = 500f;

	[SerializeField]
	private float _flightLifetimeTmax = 2f;

	[SerializeField]
	private MinMaxCurve _startSpeed = new MinMaxCurve(2f);

	[SerializeField]
	private MinMaxGradient _colorOverLifetime = new MinMaxGradient(new Gradient());

	[SerializeField]
	private MinMaxGradient _colorBySpeed = new MinMaxGradient(new Gradient());

	[SerializeField]
	private XYSpeedRange _colorSpeedRange = new XYSpeedRange(0f, 1f);

	[SerializeField]
	private MinMaxCurve _alphaOverLifetime = new MinMaxCurve(1f);

	[SerializeField]
	private MinMaxCurve _alphaBySpeed = new MinMaxCurve(1f);

	[SerializeField]
	private XYSpeedRange _alphaSpeedRange = new XYSpeedRange(0f, 1f);

	[SerializeField]
	private XYSeparatedMinMaxCurve _sizeOverLifetime = new XYSeparatedMinMaxCurve(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	}));

	[SerializeField]
	private XYSeparatedMinMaxCurve _sizeBySpeed = new XYSeparatedMinMaxCurve(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	}));

	[SerializeField]
	private XYSpeedRange _sizeSpeedRange = new XYSpeedRange(0f, 1f);

	[SerializeField]
	private XYSeparatedMinMaxCurve _startRotation = new XYSeparatedMinMaxCurve(0f);

	[SerializeField]
	private XYSeparatedMinMaxCurve _rotationOverLifetime = new XYSeparatedMinMaxCurve(0f);

	[SerializeField]
	private XYSeparatedMinMaxCurve _rotationBySpeed = new XYSeparatedMinMaxCurve(new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	}));

	[SerializeField]
	private XYSpeedRange _rotationSpeedRange = new XYSpeedRange(0f, 1f);

	[SerializeField]
	private MinMaxCurve _speedOverLifetime = new MinMaxCurve(1f);

	[SerializeField]
	private bool _alignToDirection;

	[SerializeField]
	private XYModule _targetModule = new XYModule(enabled: false);

	[SerializeField]
	private Transform _attractorTarget;

	[SerializeField]
	private MinMaxCurve _toTarget = new MinMaxCurve(1f, new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 0f),
		new Keyframe(1f, 1f)
	}));

	[SerializeField]
	private AttractorType _targetMode;

	[SerializeField]
	private AttractorPathMode _attractorPathMode;

	[SerializeField]
	private BezierControlPointScheme _bezierControlPointScheme;

	[SerializeField]
	private float _coinSettleEndXBlend = 0.2f;

	[SerializeField]
	private float _coinSettleStartYOffset = 100f;

	[FormerlySerializedAs("_propRewardPathLerp")]
	[SerializeField]
	private float _propRewardPathBlend = -0.06f;

	[SerializeField]
	private float _propRewardOffsetX;

	[SerializeField]
	private float _propRewardOffsetY;

	[SerializeField]
	private Vector2 _customBezierControlOffset;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("_bezierControlA")]
	[FormerlySerializedAs("_bezierControlX")]
	private float _legacyBezierControlA;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("_bezierControlB")]
	[FormerlySerializedAs("_bezierControlYOffset")]
	private float _legacyBezierControlB;

	[SerializeField]
	[HideInInspector]
	private bool _bezierControlLegacyMigrated;

	[FormerlySerializedAs("_bezierBurstDuration")]
	[SerializeField]
	private float _bezierHandoffDuration = 0.15f;

	[SerializeField]
	private float _bezierHandoffBlend = 0.08f;

	private const float ReferenceResolutionHeight = 1920f;

	[SerializeField]
	private XYModule _velocityModule = new XYModule(enabled: false);

	[SerializeField]
	private Simulation _velocitySpace;

	[SerializeField]
	private XYSeparatedMinMaxCurve _velocityOverLifetime = new XYSeparatedMinMaxCurve(0f, separated: true, separable: false);

	[SerializeField]
	private XYModule _sheetModule = new XYModule(enabled: false);

	[SerializeField]
	private Vector2Int _textureTile = Vector2Int.one;

	[SerializeField]
	private SheetType _sheetType = SheetType.FPS;

	private SpriteSheet[] _sheets;

	[SerializeField]
	private MinMaxCurve _frameOverTime;

	[SerializeField]
	private MinMaxCurve _startFrame = new MinMaxCurve(0f);

	[SerializeField]
	private XYSpeedRange _frameSpeedRange = new XYSpeedRange(0f, 1f);

	[SerializeField]
	private int _textureSheetFPS = 25;

	[SerializeField]
	private int _textureSheetCycles = 1;

	private List<XYParticle> _particles = new List<XYParticle>(128);

	private XYParticlePool _pool;

	private XYPrefabInstancePool _prefabPool;

	[SerializeField]
	private float _rate = 50f;

	[SerializeField]
	private float _rateOverLifetime;

	[SerializeField]
	private float _rateOverDistance;

	[SerializeField]
	private List<XYBurst> _bursts = new List<XYBurst>();

	[SerializeField]
	private XYEmitterMode _emitterMode;

	[SerializeField]
	private int _previewFlyEmitCount = 3;

	[SerializeField]
	[Min(0f)]
	private float _runtimeFixedArriveInterval;

	[FormerlySerializedAs("_trailRenderer")]
	[SerializeField]
	private XYParticleTrailRenderer _particleTrailRenderer;

	[SerializeField]
	private XYModule _trailModule;

	[SerializeField]
	private MinMaxCurve _trailWidth = new MinMaxCurve(1f, new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 0f)
	}));

	[SerializeField]
	private float _trailLifetime = 1f;

	[SerializeField]
	private float _minimumVertexDistance = 10f;

	[SerializeField]
	private MinMaxGradient _trailColorOverLifetime = new MinMaxGradient(Color.white);

	[SerializeField]
	private MinMaxGradient _trailColorOverTrail = new MinMaxGradient(Color.white);

	[SerializeField]
	private Material _trailMaterial;

	[SerializeField]
	private bool _inheritParticleColor;

	[SerializeField]
	private bool _dieWithParticle;

	[Range(0f, 1f)]
	[SerializeField]
	private float _trailRatio = 1f;

	private float _time;

	private float _playback;

	private float _loopTimer;

	private float _t;

	private float _t2;

	private float _burstTimer;

	private bool _emitting;

	private bool _playing;

	private bool _stopped;

	private bool _paused;

	[SerializeField]
	private UnityEvent _onStart = new UnityEvent();

	[SerializeField]
	private UnityEvent _onFirstParticleFinish = new UnityEvent();

	[SerializeField]
	private UnityEvent _onParticleFinish = new UnityEvent();

	[SerializeField]
	private UnityEvent _onLastParticleFinish = new UnityEvent();

	[SerializeField]
	private UnityEvent _onStop = new UnityEvent();

	private Vector3 _lastPosition;

	private Vector3 _deltaPosition;

	private Camera _camera;

	private bool _firstParticleFinished;

	private Action _runtimeOnFirstParticleFinished;

	private Action _runtimeOnAnyParticleFinished;

	private Action _runtimeOnLastParticleFinished;

	private bool _runtimeFirstInvoked;

	private bool _runtimeLastInvoked;

	private int _runtimeBurstCount = -1;

	private List<Sprite> _runtimeSprites;

	private bool _runtimeBurstUsed;

	private int _orderPerSec;

	private int _orderOverLife;

	private int _orderOverDistance;

	public bool moduleMainFoldout = true;

	public bool moduleEmitterFoldout = true;

	public bool moduleParticleFoldout = true;

	public bool moduleMovementFoldout;

	public bool moduleEventsFoldout;

	[SerializeField]
	private Sprite m_Sprite;

	[SerializeField]
	private bool _fitSpriteAspect = true;

	[SerializeField]
	private XYParticleRenderMode _renderMode;

	[SerializeField]
	private RectTransform _particlePrefab;

	private Sprite _defaultSprite;

	private string _runtimeLoadedSpriteLocation;

	public XYParticleImage[] children
	{
		get
		{
			return _children;
		}
		private set
		{
			_children = value;
		}
	}

	public XYParticleImage main
	{
		get
		{
			if ((Object)(object)_main == (Object)null)
			{
				_main = GetMain();
			}
			return _main;
		}
		private set
		{
			_main = value;
		}
	}

	public bool isMain => (Object)(object)main == (Object)(object)this;

	public RectTransform canvasRect
	{
		get
		{
			return _canvasRect;
		}
		set
		{
			_canvasRect = value;
		}
	}

	public Mesh mesh
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if ((Object)(object)_mesh == (Object)null)
			{
				_mesh = new Mesh();
				_mesh.MarkDynamic();
			}
			return _mesh;
		}
	}

	public Simulation space
	{
		get
		{
			return _space;
		}
		set
		{
			_space = value;
		}
	}

	public TimeScale timeScale
	{
		get
		{
			return _timeScale;
		}
		set
		{
			_timeScale = value;
		}
	}

	public EmitterShape shape
	{
		get
		{
			return _shape;
		}
		set
		{
			_shape = value;
		}
	}

	public SpreadType spreadType
	{
		get
		{
			return _spread;
		}
		set
		{
			_spread = value;
		}
	}

	public float spreadLoop
	{
		get
		{
			return _spreadLoop;
		}
		set
		{
			_spreadLoop = value;
		}
	}

	public float startDelay
	{
		get
		{
			return _startDelay;
		}
		set
		{
			_startDelay = value;
		}
	}

	public float circleRadius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public float rectWidth
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
		}
	}

	public float rectHeight
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	public float directionAngle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
		}
	}

	public EmitterDirectionMode directionMode
	{
		get
		{
			return _directionMode;
		}
		set
		{
			_directionMode = value;
		}
	}

	public float lineLength
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public bool fitRect
	{
		get
		{
			return _fitRect;
		}
		set
		{
			_fitRect = value;
			if (value)
			{
				FitRect();
			}
		}
	}

	public bool emitOnSurface
	{
		get
		{
			return _emitOnSurface;
		}
		set
		{
			_emitOnSurface = value;
		}
	}

	public float emitterThickness
	{
		get
		{
			return _emitterThickness;
		}
		set
		{
			_emitterThickness = value;
		}
	}

	public bool loop
	{
		get
		{
			return _loop;
		}
		set
		{
			_loop = value;
		}
	}

	public bool prewarm
	{
		get
		{
			return _prewarm;
		}
		set
		{
			_prewarm = value;
		}
	}

	public float duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = value;
		}
	}

	public PlayMode PlayMode
	{
		get
		{
			return _playMode;
		}
		set
		{
			_playMode = value;
			if (isMain && children != null)
			{
				XYParticleImage[] array = children;
				for (int i = 0; i < array.Length; i++)
				{
					array[i]._playMode = value;
				}
			}
			else if (!isMain)
			{
				main.PlayMode = value;
			}
		}
	}

	public XYSeparatedMinMaxCurve startSize
	{
		get
		{
			return _startSize;
		}
		set
		{
			_startSize = value;
		}
	}

	public MinMaxGradient startColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _startColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_startColor = value;
		}
	}

	public MinMaxCurve startAlpha
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _startAlpha;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_startAlpha = value;
		}
	}

	public MinMaxCurve lifetime
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _lifetime;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_lifetime = value;
		}
	}

	public XYLifetimeMode lifetimeMode
	{
		get
		{
			return _lifetimeMode;
		}
		set
		{
			_lifetimeMode = value;
		}
	}

	public float flightLifetimeT0
	{
		get
		{
			return _flightLifetimeT0;
		}
		set
		{
			_flightLifetimeT0 = Mathf.Max(0f, value);
		}
	}

	public float flightLifetimeTs
	{
		get
		{
			return _flightLifetimeTs;
		}
		set
		{
			_flightLifetimeTs = Mathf.Max(0f, value);
		}
	}

	public float flightLifetimeTmax
	{
		get
		{
			return _flightLifetimeTmax;
		}
		set
		{
			_flightLifetimeTmax = Mathf.Max(0f, value);
		}
	}

	public MinMaxCurve startSpeed
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _startSpeed;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_startSpeed = value;
		}
	}

	public MinMaxGradient colorOverLifetime
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _colorOverLifetime;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_colorOverLifetime = value;
		}
	}

	public MinMaxGradient colorBySpeed
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _colorBySpeed;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_colorBySpeed = value;
		}
	}

	public XYSpeedRange colorSpeedRange
	{
		get
		{
			return _colorSpeedRange;
		}
		set
		{
			_colorSpeedRange = value;
		}
	}

	public MinMaxCurve alphaOverLifetime
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _alphaOverLifetime;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_alphaOverLifetime = value;
		}
	}

	public MinMaxCurve alphaBySpeed
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _alphaBySpeed;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_alphaBySpeed = value;
		}
	}

	public XYSpeedRange alphaSpeedRange
	{
		get
		{
			return _alphaSpeedRange;
		}
		set
		{
			_alphaSpeedRange = value;
		}
	}

	public XYSeparatedMinMaxCurve sizeOverLifetime
	{
		get
		{
			return _sizeOverLifetime;
		}
		set
		{
			_sizeOverLifetime = value;
		}
	}

	public XYSeparatedMinMaxCurve sizeBySpeed
	{
		get
		{
			return _sizeBySpeed;
		}
		set
		{
			_sizeBySpeed = value;
		}
	}

	public XYSpeedRange sizeSpeedRange
	{
		get
		{
			return _sizeSpeedRange;
		}
		set
		{
			_sizeSpeedRange = value;
		}
	}

	public XYSeparatedMinMaxCurve startRotation
	{
		get
		{
			return _startRotation;
		}
		set
		{
			_startRotation = value;
		}
	}

	public XYSeparatedMinMaxCurve rotationOverLifetime
	{
		get
		{
			return _rotationOverLifetime;
		}
		set
		{
			_rotationOverLifetime = value;
		}
	}

	public XYSeparatedMinMaxCurve rotationBySpeed
	{
		get
		{
			return _rotationBySpeed;
		}
		set
		{
			_rotationBySpeed = value;
		}
	}

	public XYSpeedRange rotationSpeedRange
	{
		get
		{
			return _rotationSpeedRange;
		}
		set
		{
			_rotationSpeedRange = value;
		}
	}

	public MinMaxCurve speedOverLifetime
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _speedOverLifetime;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_speedOverLifetime = value;
		}
	}

	public bool alignToDirection
	{
		get
		{
			return _alignToDirection;
		}
		set
		{
			_alignToDirection = value;
		}
	}

	public bool attractorEnabled
	{
		get
		{
			return _targetModule.enabled;
		}
		set
		{
			_targetModule.enabled = value;
		}
	}

	public Transform attractorTarget
	{
		get
		{
			return _attractorTarget;
		}
		set
		{
			_attractorTarget = value;
		}
	}

	public MinMaxCurve attractorLerp
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _toTarget;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_toTarget = value;
		}
	}

	public AttractorType attractorType
	{
		get
		{
			if (!IsPrefabMode)
			{
				return _targetMode;
			}
			return AttractorType.Pivot;
		}
		set
		{
			if (!IsPrefabMode)
			{
				_targetMode = value;
			}
		}
	}

	public AttractorPathMode attractorPathMode
	{
		get
		{
			return _attractorPathMode;
		}
		set
		{
			_attractorPathMode = value;
		}
	}

	public BezierControlPointScheme bezierControlPointScheme
	{
		get
		{
			return _bezierControlPointScheme;
		}
		set
		{
			_bezierControlPointScheme = value;
		}
	}

	public float coinSettleEndXBlend
	{
		get
		{
			return _coinSettleEndXBlend;
		}
		set
		{
			_coinSettleEndXBlend = value;
		}
	}

	public float coinSettleStartYOffset
	{
		get
		{
			return _coinSettleStartYOffset;
		}
		set
		{
			_coinSettleStartYOffset = value;
		}
	}

	public float propRewardPathBlend
	{
		get
		{
			return _propRewardPathBlend;
		}
		set
		{
			_propRewardPathBlend = value;
		}
	}

	public float propRewardOffsetX
	{
		get
		{
			return _propRewardOffsetX;
		}
		set
		{
			_propRewardOffsetX = value;
		}
	}

	public float propRewardOffsetY
	{
		get
		{
			return _propRewardOffsetY;
		}
		set
		{
			_propRewardOffsetY = value;
		}
	}

	public Vector2 customBezierControlOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _customBezierControlOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_customBezierControlOffset = value;
		}
	}

	public float bezierHandoffDuration
	{
		get
		{
			return _bezierHandoffDuration;
		}
		set
		{
			_bezierHandoffDuration = value;
		}
	}

	public float bezierHandoffBlend
	{
		get
		{
			return _bezierHandoffBlend;
		}
		set
		{
			_bezierHandoffBlend = value;
		}
	}

	public bool velocityEnabled
	{
		get
		{
			return _velocityModule.enabled;
		}
		set
		{
			_velocityModule.enabled = value;
		}
	}

	public Simulation velocitySpace
	{
		get
		{
			return _velocitySpace;
		}
		set
		{
			_velocitySpace = value;
		}
	}

	public XYSeparatedMinMaxCurve velocityOverLifetime
	{
		get
		{
			return _velocityOverLifetime;
		}
		set
		{
			_velocityOverLifetime = value;
		}
	}

	public bool textureSheetEnabled
	{
		get
		{
			if (IsPrefabMode)
			{
				return false;
			}
			return _sheetModule.enabled;
		}
		set
		{
			if (!IsPrefabMode)
			{
				_sheetModule.enabled = value;
			}
		}
	}

	public Vector2Int textureTile
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _textureTile;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_textureTile = value;
		}
	}

	public SheetType textureSheetType
	{
		get
		{
			return _sheetType;
		}
		set
		{
			_sheetType = value;
		}
	}

	internal SpriteSheet[] Sheets => _sheets;

	public MinMaxCurve textureSheetFrameOverTime
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _frameOverTime;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_frameOverTime = value;
		}
	}

	public MinMaxCurve textureSheetStartFrame
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _startFrame;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_startFrame = value;
		}
	}

	public XYSpeedRange textureSheetFrameSpeedRange
	{
		get
		{
			return _frameSpeedRange;
		}
		set
		{
			_frameSpeedRange = value;
		}
	}

	public int textureSheetFPS
	{
		get
		{
			return _textureSheetFPS;
		}
		set
		{
			_textureSheetFPS = value;
		}
	}

	public int textureSheetCycles
	{
		get
		{
			return _textureSheetCycles;
		}
		set
		{
			_textureSheetCycles = value;
		}
	}

	public IReadOnlyList<XYParticle> particles => _particles;

	public XYParticlePool pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new XYParticlePool((int)(_rate + _rateOverLifetime + _rateOverDistance), this);
			}
			return _pool;
		}
	}

	public XYPrefabInstancePool prefabPool
	{
		get
		{
			if (_prefabPool == null)
			{
				_prefabPool = new XYPrefabInstancePool(((Component)this).transform, _particlePrefab);
			}
			else
			{
				_prefabPool.SetTemplate(_particlePrefab);
			}
			return _prefabPool;
		}
	}

	public bool IsPrefabMode => _renderMode == XYParticleRenderMode.Prefab;

	public int particleCount => _particles.Count;

	public float rateOverTime
	{
		get
		{
			return _rate;
		}
		set
		{
			_rate = value;
		}
	}

	public float rateOverLifetime
	{
		get
		{
			return _rateOverLifetime;
		}
		set
		{
			_rateOverLifetime = value;
		}
	}

	public float rateOverDistance
	{
		get
		{
			return _rateOverDistance;
		}
		set
		{
			_rateOverDistance = value;
		}
	}

	public XYEmitterMode emitterMode
	{
		get
		{
			return _emitterMode;
		}
		set
		{
			_emitterMode = value;
		}
	}

	public int previewFlyEmitCount
	{
		get
		{
			return _previewFlyEmitCount;
		}
		set
		{
			_previewFlyEmitCount = Mathf.Max(1, value);
		}
	}

	public bool usesRuntimeFixedCountMode => _emitterMode == XYEmitterMode.RuntimeFixedCount;

	public float runtimeFixedArriveInterval
	{
		get
		{
			return _runtimeFixedArriveInterval;
		}
		set
		{
			_runtimeFixedArriveInterval = Mathf.Max(0f, value);
		}
	}

	public XYParticleTrailRenderer particleTrailRenderer => _particleTrailRenderer;

	public bool trailsEnabled
	{
		get
		{
			if (IsPrefabMode)
			{
				return false;
			}
			return _trailModule.enabled;
		}
		set
		{
			if (!IsPrefabMode)
			{
				_trailModule.enabled = value;
				if (value)
				{
					EnsureTrailRenderer();
				}
				else
				{
					DestroyTrailRenderer();
				}
			}
		}
	}

	public MinMaxCurve trailWidth
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _trailWidth;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_trailWidth = value;
		}
	}

	public float trailLifetime
	{
		get
		{
			return _trailLifetime;
		}
		set
		{
			_trailLifetime = value;
		}
	}

	public float minimumVertexDistance
	{
		get
		{
			return _minimumVertexDistance;
		}
		set
		{
			_minimumVertexDistance = value;
		}
	}

	public MinMaxGradient trailColorOverLifetime
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _trailColorOverLifetime;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_trailColorOverLifetime = value;
		}
	}

	public MinMaxGradient trailColorOverTrail
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _trailColorOverTrail;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_trailColorOverTrail = value;
		}
	}

	public Material trailMaterial
	{
		get
		{
			return _trailMaterial;
		}
		set
		{
			_trailMaterial = value;
			if ((Object)(object)_particleTrailRenderer != (Object)null)
			{
				((Graphic)_particleTrailRenderer).material = value;
				((Graphic)_particleTrailRenderer).SetMaterialDirty();
			}
		}
	}

	public bool inheritParticleColor
	{
		get
		{
			return _inheritParticleColor;
		}
		set
		{
			_inheritParticleColor = value;
		}
	}

	public bool dieWithParticle
	{
		get
		{
			return _dieWithParticle;
		}
		set
		{
			_dieWithParticle = value;
		}
	}

	public float trailRatio
	{
		get
		{
			return _trailRatio;
		}
		set
		{
			_trailRatio = Mathf.Clamp01(value);
		}
	}

	public float time => _time;

	public float playback => _playback;

	public bool isEmitting
	{
		get
		{
			return _emitting;
		}
		private set
		{
			_emitting = value;
		}
	}

	public bool isPlaying
	{
		get
		{
			return _playing;
		}
		private set
		{
			_playing = value;
		}
	}

	public bool isStopped
	{
		get
		{
			return _stopped;
		}
		private set
		{
			_stopped = value;
		}
	}

	public bool isPaused
	{
		get
		{
			return _paused;
		}
		private set
		{
			_paused = value;
		}
	}

	public UnityEvent onParticleStarted => _onStart;

	public UnityEvent onFirstParticleFinished => _onFirstParticleFinish;

	public UnityEvent onAnyParticleFinished => _onParticleFinish;

	public UnityEvent onLastParticleFinished => _onLastParticleFinish;

	public UnityEvent onParticleStop => _onStop;

	public Vector3 deltaPosition => _deltaPosition;

	private Camera mainCamera
	{
		get
		{
			if ((Object)(object)_camera == (Object)null)
			{
				_camera = Camera.main;
			}
			return _camera;
		}
	}

	private bool CanStop
	{
		get
		{
			if (children != null)
			{
				bool result = true;
				XYParticleImage[] array = children;
				foreach (XYParticleImage xYParticleImage in array)
				{
					if (xYParticleImage.isEmitting || xYParticleImage.particleCount > 0)
					{
						result = false;
						break;
					}
				}
				return result;
			}
			return true;
		}
	}

	public override Material material
	{
		get
		{
			return ((Graphic)this).material;
		}
		set
		{
			if (!((Object)(object)((Graphic)this).m_Material == (Object)(object)value))
			{
				((Graphic)this).m_Material = value;
				((Graphic)this).SetMaterialDirty();
			}
		}
	}

	public XYParticleRenderMode renderMode
	{
		get
		{
			return _renderMode;
		}
		set
		{
			_renderMode = value;
		}
	}

	public RectTransform particlePrefab
	{
		get
		{
			return _particlePrefab;
		}
		set
		{
			_particlePrefab = value;
		}
	}

	public bool fitSpriteAspect
	{
		get
		{
			return _fitSpriteAspect;
		}
		set
		{
			_fitSpriteAspect = value;
		}
	}

	public Sprite sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			if (!((Object)(object)m_Sprite == (Object)(object)value))
			{
				m_Sprite = value;
				((Graphic)this).SetMaterialDirty();
			}
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if ((Object)(object)sprite != (Object)null)
			{
				return (Texture)(object)sprite.texture;
			}
			return (Texture)(object)Graphic.s_WhiteTexture;
		}
	}

	public float EvaluateBezierProgress(float normalizedTime, float randomLerp)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)((MinMaxCurve)(ref _toTarget)).mode == 1)
		{
			return ((MinMaxCurve)(ref _toTarget)).Evaluate(normalizedTime, randomLerp);
		}
		return normalizedTime;
	}

	private static MinMaxCurve CreateDefaultBezierSpeedCurve()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		return new MinMaxCurve(1f, new AnimationCurve((Keyframe[])(object)new Keyframe[2]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		}));
	}

	private void EnsureBezierSpeedCurveMode()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (_attractorPathMode == AttractorPathMode.Bezier)
		{
			if ((int)((MinMaxCurve)(ref _toTarget)).mode == 1)
			{
				EnsureAttractorCurveMultiplier();
			}
			else
			{
				_toTarget = CreateDefaultBezierSpeedCurve();
			}
		}
	}

	private void EnsureAttractorCurveMultiplier()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		MinMaxCurve toTarget = _toTarget;
		if (((int)((MinMaxCurve)(ref toTarget)).mode == 1 || (int)((MinMaxCurve)(ref toTarget)).mode == 2) && !Mathf.Approximately(((MinMaxCurve)(ref toTarget)).curveMultiplier, 1f))
		{
			((MinMaxCurve)(ref toTarget)).curveMultiplier = 1f;
			_toTarget = toTarget;
		}
	}

	public Vector3 SampleStartSize(float lerpA, float lerpB)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = EvaluateStartSize(lerpA, lerpB);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(1f, 1f, 1f);
		return FitParticleSizeToSpriteAspect(Vector3.Max(val, val2));
	}

	private Vector3 EvaluateStartSize(float lerpA, float lerpB)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (_startSize.separated)
		{
			return new Vector3(EvaluateSizeCurve(_startSize.xCurve, lerpA, lerpB), EvaluateSizeCurve(_startSize.yCurve, lerpA, lerpB), EvaluateSizeCurve(_startSize.zCurve, lerpA, lerpB));
		}
		float num = EvaluateSizeCurve(_startSize.mainCurve, lerpA, lerpB);
		return new Vector3(num, num, num);
	}

	private static float EvaluateSizeCurve(MinMaxCurve curve, float lerpA, float lerpB)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = ((MinMaxCurve)(ref curve)).Evaluate(lerpA, lerpB);
		if (num > 0f)
		{
			return num;
		}
		if ((int)((MinMaxCurve)(ref curve)).mode == 0 && ((MinMaxCurve)(ref curve)).constantMax > 0f)
		{
			return ((MinMaxCurve)(ref curve)).constantMax;
		}
		return num;
	}

	public Vector3 FitParticleSizeToSpriteAspect(Vector3 size)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (IsPrefabMode || !_fitSpriteAspect)
		{
			return size;
		}
		float aspect = (((Object)(object)m_Sprite != (Object)null) ? XYSpriteParticleDrawUtil.GetAspectRatio(m_Sprite) : 1f);
		return XYSpriteParticleDrawUtil.FitSizeToAspect(size, aspect);
	}

	private void RefreshSpriteAspectAndSheets()
	{
		UpdateSheets();
		((Graphic)this).SetMaterialDirty();
	}

	public float SampleStartAlpha(float lerpA, float lerpB)
	{
		return Mathf.Clamp01(((MinMaxCurve)(ref _startAlpha)).Evaluate(lerpA, lerpB));
	}

	private void MigrateBezierControlLegacyParams()
	{
		if (_bezierControlPointScheme > BezierControlPointScheme.Custom)
		{
			_bezierControlPointScheme = BezierControlPointScheme.PropReward;
		}
		if (_bezierControlLegacyMigrated)
		{
			return;
		}
		if (_legacyBezierControlA != 0f || _legacyBezierControlB != 0f)
		{
			if (_bezierControlPointScheme == BezierControlPointScheme.PropReward)
			{
				_propRewardPathBlend = _legacyBezierControlA;
			}
			else
			{
				_coinSettleEndXBlend = _legacyBezierControlA;
				_coinSettleStartYOffset = ((_legacyBezierControlB != 0f) ? _legacyBezierControlB : _coinSettleStartYOffset);
			}
		}
		_bezierControlLegacyMigrated = true;
	}

	private void MigrateEmitterMode()
	{
		switch ((int)_emitterMode)
		{
		case 5:
			_emitterMode = XYEmitterMode.RuntimeFixedCount;
			break;
		default:
			_emitterMode = XYEmitterMode.Standard;
			break;
		case 0:
		case 1:
			break;
		}
	}

	public float GetLifetimeUpperBound()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected I4, but got Unknown
		if (_lifetimeMode == XYLifetimeMode.ByFlightDistance)
		{
			return ComputeFlightDistanceLifetimeUpperBound();
		}
		ParticleSystemCurveMode mode = ((MinMaxCurve)(ref _lifetime)).mode;
		return (mode - 1) switch
		{
			2 => Mathf.Max(((MinMaxCurve)(ref _lifetime)).constantMin, ((MinMaxCurve)(ref _lifetime)).constantMax), 
			0 => ((MinMaxCurve)(ref _lifetime)).curve.Evaluate(1f) * ((MinMaxCurve)(ref _lifetime)).curveMultiplier, 
			1 => Mathf.Max(((MinMaxCurve)(ref _lifetime)).curveMax.Evaluate(1f), ((MinMaxCurve)(ref _lifetime)).curveMin.Evaluate(1f)) * ((MinMaxCurve)(ref _lifetime)).curveMultiplier, 
			_ => ((MinMaxCurve)(ref _lifetime)).constant, 
		};
	}

	public float GetEmitDuration()
	{
		return Mathf.Max(0f, startDelay) + Mathf.Max(0f, duration);
	}

	public float GetFlightDurationUpperBound()
	{
		return GetFlightDurationUpperBound(_attractorTarget);
	}

	public float GetFlightDurationUpperBound(Transform attractorTarget, Transform emitterTransform = null)
	{
		float num = ComputeFlightDurationUpperBoundForTargets(attractorTarget, emitterTransform);
		if (children == null)
		{
			return num;
		}
		XYParticleImage[] array = children;
		foreach (XYParticleImage xYParticleImage in array)
		{
			num = Mathf.Max(num, xYParticleImage.ComputeFlightDurationUpperBoundForTargets(attractorTarget, emitterTransform));
		}
		return num;
	}

	public float GetTotalPlaybackDuration()
	{
		return GetTotalPlaybackDurationForFly(null, _attractorTarget);
	}

	public float GetTotalPlaybackDurationForFly(Transform emitterTransform, Transform attractorTarget)
	{
		if (loop)
		{
			return Mathf.Max(duration, 0.01f);
		}
		float num = GetFlightDurationUpperBound(attractorTarget, emitterTransform);
		if (usesRuntimeFixedCountMode)
		{
			num += GetRuntimeFixedArriveStaggerDuration(GetRuntimeFixedEmitCountForEstimation());
		}
		return Mathf.Max(GetEmitDuration() + num, 0.01f);
	}

	private float ComputeFlightDurationUpperBoundForTargets(Transform attractorTarget, Transform emitterTransform)
	{
		if (_lifetimeMode != XYLifetimeMode.ByFlightDistance)
		{
			return GetLifetimeUpperBound();
		}
		if (!attractorEnabled || (Object)(object)attractorTarget == (Object)null)
		{
			return _flightLifetimeT0;
		}
		float num = ComputeMaxSpawnToAttractorDistanceForTargets(attractorTarget, emitterTransform);
		return Mathf.Min(_flightLifetimeT0 + _flightLifetimeTs * num / 1000f, _flightLifetimeTmax);
	}

	public float ComputeParticleLifetime(Vector2 startPosition, Vector2 attractorRandom)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (_lifetimeMode != XYLifetimeMode.ByFlightDistance)
		{
			return ((MinMaxCurve)(ref _lifetime)).Evaluate(Random.value, Random.value);
		}
		if (!attractorEnabled || (Object)(object)_attractorTarget == (Object)null)
		{
			return _flightLifetimeT0;
		}
		Vector2 val = ResolveAttractorEndPoint(attractorRandom);
		float num = Vector2.Distance(startPosition, val);
		return Mathf.Min(_flightLifetimeT0 + _flightLifetimeTs * num / 1000f, _flightLifetimeTmax);
	}

	private float ComputeFlightDistanceLifetimeUpperBound()
	{
		if (!attractorEnabled || (Object)(object)_attractorTarget == (Object)null)
		{
			return _flightLifetimeT0;
		}
		return ComputeFlightDurationUpperBoundForTargets(_attractorTarget, ((Component)this).transform);
	}

	private float ComputeMaxSpawnToAttractorDistanceForTargets(Transform attractorTarget, Transform emitterTransform)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Transform referenceTransform = (((Object)(object)emitterTransform != (Object)null) ? emitterTransform : ((Component)this).transform);
		Vector2 targetLocalPositionRelativeTo = GetTargetLocalPositionRelativeTo(attractorTarget, referenceTransform);
		float num = Vector2.Distance(Vector2.zero, targetLocalPositionRelativeTo);
		float emitterMaxRadius = GetEmitterMaxRadius();
		float attractorMaxRadiusForTarget = GetAttractorMaxRadiusForTarget(attractorTarget);
		return num + emitterMaxRadius + attractorMaxRadiusForTarget;
	}

	private float ComputeMaxSpawnToAttractorDistance()
	{
		return ComputeMaxSpawnToAttractorDistanceForTargets(_attractorTarget, ((Component)this).transform);
	}

	private float GetEmitterMaxRadius()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		switch (_shape)
		{
		case EmitterShape.Point:
		case EmitterShape.Directional:
		case EmitterShape.ToTarget:
			return 0f;
		case EmitterShape.Circle:
			return _radius;
		case EmitterShape.Rectangle:
		{
			Vector2 val = new Vector2(_width * 0.5f, _height * 0.5f);
			return ((Vector2)(ref val)).magnitude;
		}
		case EmitterShape.Line:
			return _length * 0.5f;
		default:
			return 0f;
		}
	}

	private float GetAttractorMaxRadius()
	{
		return GetAttractorMaxRadiusForTarget(_attractorTarget);
	}

	private float GetAttractorMaxRadiusForTarget(Transform attractorTarget)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (attractorType == AttractorType.Surface)
		{
			RectTransform val = (RectTransform)(object)((attractorTarget is RectTransform) ? attractorTarget : null);
			if (val != null)
			{
				Vector2 val2 = val.sizeDelta * 0.5f;
				return ((Vector2)(ref val2)).magnitude;
			}
		}
		return 0f;
	}

	private Vector2 GetTargetLocalPositionRelativeTo(Transform target, Transform referenceTransform)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null || (Object)(object)referenceTransform == (Object)null)
		{
			return Vector2.zero;
		}
		if (target is RectTransform)
		{
			return Vector2.op_Implicit(referenceTransform.InverseTransformPoint(target.position));
		}
		if ((Object)(object)((Graphic)this).canvas == (Object)null || (Object)(object)canvasRect == (Object)null)
		{
			Vector3 val = referenceTransform.InverseTransformPoint(target.position);
			return new Vector2(val.x, val.y);
		}
		Vector3 val2 = WorldToViewportPoint(target.position);
		Rect rect;
		if ((int)((Graphic)this).canvas.renderMode == 1)
		{
			float x = val2.x;
			rect = canvasRect.rect;
			float num = (x.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).width) - ((Transform)canvasRect).InverseTransformPoint(referenceTransform.position).x + ((Transform)canvasRect).localPosition.x) / referenceTransform.lossyScale.x * ((Transform)canvasRect).localScale.x;
			float y = val2.y;
			rect = canvasRect.rect;
			return new Vector2(num, (y.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).height) - ((Transform)canvasRect).InverseTransformPoint(referenceTransform.position).y + ((Transform)canvasRect).localPosition.y) / referenceTransform.lossyScale.y * ((Transform)canvasRect).localScale.y);
		}
		float x2 = val2.x;
		rect = canvasRect.rect;
		float num2 = (x2.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).width) - ((Transform)canvasRect).InverseTransformPoint(referenceTransform.position).x) / referenceTransform.lossyScale.x * ((Transform)canvasRect).localScale.x;
		float y2 = val2.y;
		rect = canvasRect.rect;
		return new Vector2(num2, (y2.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).height) - ((Transform)canvasRect).InverseTransformPoint(referenceTransform.position).y) / referenceTransform.lossyScale.y * ((Transform)canvasRect).localScale.y);
	}

	public bool TryGetVelocityOverLifetimeXY(float normalizedTime, float lerp, out Vector2 delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		delta = Vector2.zero;
		if (!_velocityModule.enabled)
		{
			return false;
		}
		if (_velocitySpace == Simulation.World)
		{
			Vector3 point = _velocityOverLifetime.Evaluate(normalizedTime, lerp);
			Quaternion val = Quaternion.Inverse(((Component)this).transform.rotation);
			Vector3 val2 = RotatePointAroundCenter(point, ((Quaternion)(ref val)).eulerAngles);
			delta = new Vector2(val2.x, val2.y);
		}
		else
		{
			delta = _velocityOverLifetime.EvaluateXY(normalizedTime, lerp);
		}
		return true;
	}

	public float GetRuntimeFixedArriveDelay(int order)
	{
		if (!usesRuntimeFixedCountMode || order <= 0)
		{
			return 0f;
		}
		return (float)order * Mathf.Max(0f, _runtimeFixedArriveInterval);
	}

	private int GetRuntimeFixedEmitCountForEstimation()
	{
		if (_runtimeBurstCount <= 0)
		{
			return Mathf.Max(1, _previewFlyEmitCount);
		}
		return _runtimeBurstCount;
	}

	private float GetRuntimeFixedArriveStaggerDuration(int emitCount)
	{
		if (!usesRuntimeFixedCountMode || emitCount <= 1)
		{
			return 0f;
		}
		return (float)(emitCount - 1) * Mathf.Max(0f, _runtimeFixedArriveInterval);
	}

	public void EnsureTrailRenderer()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (IsPrefabMode || !trailsEnabled)
		{
			return;
		}
		if ((Object)(object)_particleTrailRenderer != (Object)null)
		{
			_particleTrailRenderer.particle = this;
			return;
		}
		_particleTrailRenderer = ((Component)this).GetComponentInChildren<XYParticleTrailRenderer>();
		if ((Object)(object)_particleTrailRenderer != (Object)null)
		{
			_particleTrailRenderer.particle = this;
			return;
		}
		GameObject val = new GameObject("Trails");
		val.transform.SetParent(((Component)this).transform, false);
		val.transform.localPosition = Vector3.zero;
		val.transform.localScale = Vector3.one;
		val.transform.localEulerAngles = Vector3.zero;
		val.AddComponent<CanvasRenderer>();
		XYParticleTrailRenderer xYParticleTrailRenderer = val.AddComponent<XYParticleTrailRenderer>();
		((Graphic)xYParticleTrailRenderer).raycastTarget = false;
		xYParticleTrailRenderer.particle = this;
		_particleTrailRenderer = xYParticleTrailRenderer;
	}

	public void DestroyTrailRenderer()
	{
		if ((Object)(object)_particleTrailRenderer != (Object)null)
		{
			DestroyTrailRendererObject(((Component)_particleTrailRenderer).gameObject);
			_particleTrailRenderer = null;
			return;
		}
		XYParticleTrailRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<XYParticleTrailRenderer>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)((Component)componentsInChildren[i]).transform.parent == (Object)(object)((Component)this).transform)
			{
				DestroyTrailRendererObject(((Component)componentsInChildren[i]).gameObject);
			}
		}
	}

	private void DestroyTrailRendererObject(GameObject go)
	{
		if (!((Object)(object)go == (Object)null))
		{
			Object.Destroy((Object)(object)go);
		}
	}

	private void SyncTrailRendererState()
	{
		if (IsPrefabMode || !trailsEnabled)
		{
			DestroyTrailRenderer();
		}
	}

	protected override void Awake()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		((UIBehaviour)this).Awake();
		_defaultSprite = m_Sprite;
		((Graphic)this).raycastTarget = false;
		((MaskableGraphic)this).maskable = false;
		MigrateBezierControlLegacyParams();
		MigrateEmitterMode();
		EnsureAttractorCurveMultiplier();
		if (isMain)
		{
			children = GetChildren();
		}
		if (fitRect)
		{
			FitRect();
		}
		main = GetMain();
		main.children = main.GetChildren();
		_playMode = main.PlayMode;
		_lastPosition = ((Component)this).transform.position;
		if (Object.op_Implicit((Object)(object)((Graphic)this).canvas))
		{
			canvasRect = ((Component)((Graphic)this).canvas).gameObject.GetComponent<RectTransform>();
		}
		Clear();
		SyncTrailRendererState();
		if (_playMode == PlayMode.OnAwake && Application.isPlaying)
		{
			Play();
		}
	}

	protected override void OnEnable()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		((MaskableGraphic)this).OnEnable();
		if (isMain)
		{
			children = GetChildren();
		}
		if (fitRect)
		{
			FitRect();
		}
		main = GetMain();
		main.children = main.GetChildren();
		_playMode = main.PlayMode;
		_lastPosition = ((Component)this).transform.position;
		if (Object.op_Implicit((Object)(object)((Graphic)this).canvas) && (Object)(object)canvasRect == (Object)null)
		{
			canvasRect = ((Component)((Graphic)this).canvas).gameObject.GetComponent<RectTransform>();
		}
		if (_playMode == PlayMode.OnEnable && Application.isPlaying)
		{
			Stop(stopAndClear: true);
			Clear();
			Play();
		}
	}

	protected override void OnDisable()
	{
		Stop(stopAndClear: true);
		if (isMain)
		{
			ReleaseRuntimeLoadedSprite();
		}
		((MaskableGraphic)this).OnDisable();
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)_mesh != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_mesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_mesh);
			}
			_mesh = null;
		}
		if (_prefabPool != null)
		{
			_prefabPool.DestroyAll();
		}
		ReleaseRuntimeLoadedSprite();
		((Graphic)this).OnDestroy();
	}

	public XYParticleImage GetMain()
	{
		XYParticleImage xYParticleImage = default(XYParticleImage);
		if (Object.op_Implicit((Object)(object)((Component)this).transform.parent) && ((Component)((Component)this).transform.parent).TryGetComponent<XYParticleImage>(ref xYParticleImage))
		{
			return xYParticleImage.GetMain();
		}
		return this;
	}

	public XYParticleImage[] GetChildren()
	{
		if (((Component)this).transform.childCount <= 0)
		{
			return null;
		}
		XYParticleImage[] componentsInChildren = ((Component)this).GetComponentsInChildren<XYParticleImage>();
		int num = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)(object)this)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return null;
		}
		XYParticleImage[] array = new XYParticleImage[num];
		int num2 = 0;
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (!((Object)(object)componentsInChildren[j] == (Object)(object)this))
			{
				array[num2++] = componentsInChildren[j];
			}
		}
		return array;
	}

	private void OnTransformChildrenChanged()
	{
		main = GetMain();
		if (isMain)
		{
			children = GetChildren();
		}
	}

	protected override void OnTransformParentChanged()
	{
		((MaskableGraphic)this).OnTransformParentChanged();
		main = GetMain();
		if (isMain)
		{
			children = GetChildren();
		}
	}

	public void Play()
	{
		main.DoPlay();
	}

	public void Play(Transform attractorTarget, Action onFirstParticleFinished = null, Action onAnyParticleFinished = null, Action onLastParticleFinished = null)
	{
		main.DoPlay(attractorTarget, onFirstParticleFinished, onAnyParticleFinished, onLastParticleFinished);
	}

	public void Play(int emitCount, Transform attractorTarget, Action onFirstParticleFinished = null, Action onAnyParticleFinished = null, Action onLastParticleFinished = null)
	{
		if (main.ValidateRuntimeEmit(emitCount, null))
		{
			main.DoPlay(attractorTarget, onFirstParticleFinished, onAnyParticleFinished, onLastParticleFinished, emitCount);
		}
	}

	public void Play(IReadOnlyList<Sprite> sprites, Transform attractorTarget, Action onFirstParticleFinished = null, Action onAnyParticleFinished = null, Action onLastParticleFinished = null)
	{
		if (sprites != null && sprites.Count > 0 && main.ValidateRuntimeEmit(sprites.Count, sprites))
		{
			main.DoPlay(attractorTarget, onFirstParticleFinished, onAnyParticleFinished, onLastParticleFinished, sprites.Count, sprites);
		}
	}

	public void PlayEditorPreviewFly()
	{
		Play(Mathf.Max(1, _previewFlyEmitCount), _attractorTarget);
	}

	private void DoPlay(Transform attractorTarget = null, Action onFirstParticleFinished = null, Action onAnyParticleFinished = null, Action onLastParticleFinished = null, int runtimeEmitCount = -1, IReadOnlyList<Sprite> runtimeSprites = null)
	{
		RefreshSpriteAspectAndSheets();
		if (isMain)
		{
			PrepareForReplay();
			if (runtimeEmitCount > 0 && !TryPrepareRuntimeEmit(runtimeEmitCount, runtimeSprites))
			{
				return;
			}
			ResetFirstParticleFinishedFlags();
			if ((Object)(object)attractorTarget != (Object)null || onFirstParticleFinished != null || onAnyParticleFinished != null || onLastParticleFinished != null)
			{
				SetRuntimePlayCallbacks(attractorTarget, onFirstParticleFinished, onAnyParticleFinished, onLastParticleFinished);
			}
		}
		if (isMain && children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DoPlay();
			}
		}
		_t = ((_rate > 0f) ? (1f / _rate) : 0f);
		_time = 0f;
		_burstTimer = 0f;
		_runtimeBurstUsed = false;
		for (int j = 0; j < _bursts.Count; j++)
		{
			_bursts[j].used = false;
		}
		isEmitting = true;
		isPlaying = true;
		isPaused = false;
		isStopped = false;
		if (prewarm)
		{
			Prewarm();
		}
		Simulate((_timeScale == TimeScale.Normal) ? Time.deltaTime : Time.unscaledDeltaTime, prewarm);
		OnParticleStart();
	}

	private void PrepareForReplay()
	{
		if (NeedsStopBeforeReplay())
		{
			DoStop(stopAndClear: true);
		}
	}

	private bool NeedsStopBeforeReplay()
	{
		if (isPlaying || isPaused || _firstParticleFinished || particleCount > 0)
		{
			return true;
		}
		if (children == null)
		{
			return false;
		}
		XYParticleImage[] array = children;
		foreach (XYParticleImage xYParticleImage in array)
		{
			if (xYParticleImage.isPlaying || xYParticleImage.isPaused || xYParticleImage._firstParticleFinished || xYParticleImage.particleCount > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void ResetFirstParticleFinishedFlags()
	{
		_firstParticleFinished = false;
		if (children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ResetFirstParticleFinishedFlags();
			}
		}
	}

	private void SetRuntimePlayCallbacks(Transform attractorTarget, Action onFirstParticleFinished, Action onAnyParticleFinished, Action onLastParticleFinished)
	{
		if (!isMain)
		{
			main.SetRuntimePlayCallbacks(attractorTarget, onFirstParticleFinished, onAnyParticleFinished, onLastParticleFinished);
			return;
		}
		ClearRuntimePlayCallbacks();
		if ((Object)(object)attractorTarget != (Object)null)
		{
			this.attractorTarget = attractorTarget;
		}
		_runtimeOnFirstParticleFinished = onFirstParticleFinished;
		_runtimeOnAnyParticleFinished = onAnyParticleFinished;
		_runtimeOnLastParticleFinished = onLastParticleFinished;
		_runtimeFirstInvoked = false;
		_runtimeLastInvoked = false;
	}

	private void TryInvokeRuntimeFirst()
	{
		if (!_runtimeFirstInvoked && _runtimeOnFirstParticleFinished != null)
		{
			_runtimeFirstInvoked = true;
			_runtimeOnFirstParticleFinished();
		}
	}

	private void TryInvokeRuntimeAny()
	{
		_runtimeOnAnyParticleFinished?.Invoke();
	}

	private void TryInvokeRuntimeLast()
	{
		if (!_runtimeLastInvoked && _runtimeOnLastParticleFinished != null)
		{
			_runtimeLastInvoked = true;
			Action runtimeOnLastParticleFinished = _runtimeOnLastParticleFinished;
			ClearRuntimePlayCallbacks();
			ClearRuntimeEmitState();
			runtimeOnLastParticleFinished();
		}
	}

	private bool ValidateRuntimeEmit(int emitCount, IReadOnlyList<Sprite> sprites)
	{
		if (!isMain)
		{
			return main.ValidateRuntimeEmit(emitCount, sprites);
		}
		if (emitCount <= 0)
		{
			return false;
		}
		if (sprites != null && sprites.Count != emitCount)
		{
			return false;
		}
		if (sprites != null && !IsPrefabMode)
		{
			return false;
		}
		return true;
	}

	private bool TryPrepareRuntimeEmit(int emitCount, IReadOnlyList<Sprite> sprites)
	{
		if (!isMain)
		{
			return main.TryPrepareRuntimeEmit(emitCount, sprites);
		}
		if (!ValidateRuntimeEmit(emitCount, sprites))
		{
			return false;
		}
		_runtimeBurstCount = emitCount;
		_runtimeSprites = null;
		if (sprites != null)
		{
			_runtimeSprites = new List<Sprite>(sprites);
		}
		_runtimeBurstUsed = false;
		EnsureEmitPoolCapacity(emitCount);
		return true;
	}

	private void EnsureEmitPoolCapacity(int emitCount)
	{
		pool.EnsureCapacity(emitCount);
		if (IsPrefabMode && (Object)(object)_particlePrefab != (Object)null)
		{
			prefabPool.EnsureCapacity(emitCount);
		}
	}

	private void ClearRuntimeEmitState()
	{
		if (!isMain)
		{
			main.ClearRuntimeEmitState();
			return;
		}
		_runtimeBurstCount = -1;
		_runtimeSprites = null;
		_runtimeBurstUsed = false;
	}

	private void ClearRuntimePlayCallbacks()
	{
		if (!isMain)
		{
			main.ClearRuntimePlayCallbacks();
			return;
		}
		_runtimeOnFirstParticleFinished = null;
		_runtimeOnAnyParticleFinished = null;
		_runtimeOnLastParticleFinished = null;
		_runtimeFirstInvoked = false;
		_runtimeLastInvoked = false;
	}

	public void Pause()
	{
		main.DoPause();
	}

	private void DoPause()
	{
		if (isMain && children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DoPause();
			}
		}
		isEmitting = false;
		isPlaying = false;
		isPaused = true;
	}

	public void Resume()
	{
		main.DoResume();
	}

	private void DoResume()
	{
		if (isMain && children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DoResume();
			}
		}
		isEmitting = true;
		isPlaying = true;
		isPaused = false;
		isStopped = false;
	}

	public void Seek(float targetTime)
	{
		main.DoSeek(targetTime);
	}

	private void DoSeek(float targetTime)
	{
		if (isMain && children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DoSeek(targetTime);
			}
		}
		targetTime = Mathf.Max(0f, targetTime);
		bool flag = isPlaying;
		DoClear();
		_t = ((_rate > 0f) ? (1f / _rate) : 0f);
		_t2 = 0f;
		_time = 0f;
		_burstTimer = 0f;
		_loopTimer = 0f;
		_runtimeBurstUsed = false;
		isEmitting = true;
		isPlaying = true;
		isPaused = false;
		isStopped = false;
		int num = Mathf.CeilToInt(targetTime / 0.01f);
		for (int j = 0; j < num; j++)
		{
			float num2 = Mathf.Min(0.01f, targetTime - (float)j * 0.01f);
			if (num2 <= 0f)
			{
				break;
			}
			Simulate(num2, prewarming: true);
		}
		isEmitting = false;
		Simulate(0f);
		_playback = targetTime;
		if (!flag)
		{
			isEmitting = false;
			isPlaying = false;
			isPaused = true;
		}
	}

	public void Stop()
	{
		Stop(stopAndClear: false);
	}

	public void Stop(bool stopAndClear)
	{
		main.DoStop(stopAndClear);
	}

	private void DoStop(bool stopAndClear)
	{
		if (isMain && children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DoStop(stopAndClear);
			}
		}
		_orderPerSec = 0;
		_orderOverLife = 0;
		_orderOverDistance = 0;
		for (int j = 0; j < _bursts.Count; j++)
		{
			_bursts[j].used = false;
		}
		if (stopAndClear)
		{
			isStopped = true;
			isPlaying = false;
			Clear();
		}
		isEmitting = false;
		if (isPaused)
		{
			isPaused = false;
			isStopped = true;
			isPlaying = false;
			Clear();
		}
		for (int k = 0; k < _bursts.Count; k++)
		{
			_bursts[k].used = false;
		}
		_firstParticleFinished = false;
		if (isMain)
		{
			ClearRuntimePlayCallbacks();
			ClearRuntimeEmitState();
		}
		OnParticleStop();
	}

	public void Clear()
	{
		main.DoClear();
	}

	private void DoClear()
	{
		if (isMain && children != null)
		{
			XYParticleImage[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DoClear();
			}
		}
		for (int j = 0; j < _bursts.Count; j++)
		{
			_bursts[j].used = false;
		}
		_time = 0f;
		_playback = 0f;
		_burstTimer = 0f;
		if (IsPrefabMode)
		{
			for (int k = 0; k < _particles.Count; k++)
			{
				_particles[k].UnbindPrefabInstance();
			}
			prefabPool.DestroyAll();
		}
		_particles.Clear();
		mesh.Clear();
		((Graphic)this).canvasRenderer.SetMesh(mesh);
		((Graphic)this).SetMaterialDirty();
		if ((Object)(object)_particleTrailRenderer != (Object)null)
		{
			_particleTrailRenderer.Clear();
		}
	}

	private void ReleaseParticle(XYParticle particle)
	{
		if (IsPrefabMode)
		{
			RectTransform val = particle.UnbindPrefabInstance();
			if ((Object)(object)val != (Object)null)
			{
				prefabPool.Release(val);
			}
		}
		pool.Release(particle);
	}

	private void Update()
	{
		if (isPlaying || isEmitting)
		{
			Simulate((_timeScale == TimeScale.Normal) ? Time.deltaTime : Time.unscaledDeltaTime);
		}
	}

	private Vector3 RotatePointAroundCenter(Vector3 point, Vector3 angles)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(angles) * point;
	}

	private void UpdateSheets()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int val = textureTile;
		int x = ((Vector2Int)(ref val)).x;
		val = textureTile;
		int num = x * ((Vector2Int)(ref val)).y;
		if (_sheets == null || _sheets.Length != num)
		{
			_sheets = new SpriteSheet[num];
		}
		if (textureSheetEnabled)
		{
			val = textureTile;
			for (int num2 = ((Vector2Int)(ref val)).y - 1; num2 > -1; num2--)
			{
				int num3 = 0;
				while (true)
				{
					int num4 = num3;
					val = textureTile;
					if (num4 >= ((Vector2Int)(ref val)).x)
					{
						break;
					}
					SpriteSheet[] sheets = _sheets;
					val = textureTile;
					int num5 = ((Vector2Int)(ref val)).y - 1 - num2;
					val = textureTile;
					int num6 = num5 * ((Vector2Int)(ref val)).x + num3;
					val = textureTile;
					float num7 = 1f / (float)((Vector2Int)(ref val)).x * (float)(num3 + 1);
					val = textureTile;
					Vector2 s = new Vector2(num7, 1f / (float)((Vector2Int)(ref val)).y * (float)(num2 + 1));
					val = textureTile;
					float num8 = 1f / (float)((Vector2Int)(ref val)).x * (float)num3;
					val = textureTile;
					sheets[num6] = new SpriteSheet(s, new Vector2(num8, 1f / (float)((Vector2Int)(ref val)).y * (float)num2));
					num3++;
				}
			}
		}
		else
		{
			_sheets[0] = (((Object)(object)sprite != (Object)null) ? XYSpriteParticleDrawUtil.BuildSheet(sprite) : new SpriteSheet(new Vector2(1f, 1f), Vector2.zero));
		}
	}

	private void Prewarm()
	{
		int num = Mathf.FloorToInt(duration * 2f / 0.01f);
		for (int i = 0; i < num; i++)
		{
			Simulate(0.01f, prewarming: true);
		}
	}

	private void Simulate(float deltaTime, bool prewarming = false)
	{
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0909: Unknown result type (might be due to invalid IL or missing references)
		//IL_0916: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_083d: Unknown result type (might be due to invalid IL or missing references)
		//IL_085a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0877: Unknown result type (might be due to invalid IL or missing references)
		//IL_0894: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_0822: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("XYParticleImage.Simulate"))
		{
			if (isEmitting && !((Graphic)this).canvasRenderer.cull)
			{
				if (_runtimeBurstCount >= 0 && isMain)
				{
					if (!_runtimeBurstUsed && _burstTimer >= _startDelay)
					{
						EmitRuntimeBurstParticles();
						_runtimeBurstUsed = true;
					}
				}
				else if (_emitterMode != XYEmitterMode.RuntimeFixedCount)
				{
					if (_rate > 0f && (_time < _duration + _startDelay || _loop) && _time > _startDelay)
					{
						float num = 1f / _rate;
						_t += deltaTime;
						while (_t >= num)
						{
							_t -= num;
							_orderPerSec++;
							_particles.Add(GenerateParticle(_orderPerSec, 1, null, _t));
						}
					}
					if (_rateOverLifetime > 0f && _duration > 0f && (_time < _duration + _startDelay || _loop) && _time >= _startDelay)
					{
						float num2 = _duration / _rateOverLifetime;
						_t2 += deltaTime;
						while (_t2 >= num2)
						{
							_t2 -= num2;
							_orderOverLife++;
							_particles.Add(GenerateParticle(_orderOverLife, 2, null, _t2));
						}
					}
					if (_rateOverDistance > 0f && ((Vector3)(ref _deltaPosition)).magnitude > 1f / _rateOverDistance)
					{
						_orderOverDistance++;
						_particles.Add(GenerateParticle(_orderOverDistance, 3, null, 0f));
						_lastPosition = ((Component)this).transform.position;
					}
					if (_bursts != null)
					{
						for (int i = 0; i < _bursts.Count; i++)
						{
							if (_burstTimer >= _bursts[i].time + _startDelay && !_bursts[i].used)
							{
								for (int j = 0; j < _bursts[i].count; j++)
								{
									_particles.Add(GenerateParticle(j, 0, _bursts[i], 0f));
								}
								_bursts[i].used = true;
							}
						}
					}
				}
				if (_loop && _burstTimer >= _duration && _emitterMode == XYEmitterMode.Standard && _runtimeBurstCount < 0)
				{
					_burstTimer = 0f;
					for (int k = 0; k < _bursts.Count; k++)
					{
						_bursts[k].used = false;
					}
				}
				if (_time >= _duration + _startDelay && !_loop)
				{
					isEmitting = false;
				}
				if (_loop && _loopTimer >= _duration + _startDelay)
				{
					_loopTimer = 0f;
					_orderPerSec = 0;
					_orderOverLife = 0;
					_orderOverDistance = 0;
				}
			}
			if (isPlaying && particleCount <= 0 && !isEmitting && isMain && CanStop)
			{
				Stop(stopAndClear: true);
			}
			if (!isPlaying)
			{
				return;
			}
			_deltaPosition = ((Component)this).transform.position - _lastPosition;
			if (!prewarming)
			{
				_playback += deltaTime;
			}
			_time += deltaTime;
			_loopTimer += deltaTime;
			_burstTimer += deltaTime;
			if (IsPrefabMode)
			{
				for (int num3 = particles.Count - 1; num3 >= 0; num3--)
				{
					_particles[num3].Simulate(deltaTime);
				}
				mesh.Clear();
				((Graphic)this).canvasRenderer.SetMesh(mesh);
				for (int num4 = particles.Count - 1; num4 >= 0; num4--)
				{
					if (_particles[num4].TimeSinceBorn > _particles[num4].Lifetime && (_particles[num4].trailPoints.Count <= 1 || _dieWithParticle))
					{
						OnAnyParticleFinished();
						ReleaseParticle(_particles[num4]);
						_particles.RemoveAt(num4);
						if (!_firstParticleFinished)
						{
							_firstParticleFinished = true;
							OnFirstParticleFinished();
						}
						if (particleCount < 1)
						{
							OnLastParticleFinished();
						}
					}
				}
				return;
			}
			UpdateSheets();
			int num5 = 0;
			if (trailsEnabled)
			{
				EnsureTrailRenderer();
				for (int l = 0; l < _particles.Count; l++)
				{
					num5 += _particles[l].trailPoints.Count;
				}
				if (num5 > 0 && (Object)(object)_particleTrailRenderer != (Object)null)
				{
					_particleTrailRenderer.PrepareMeshData(num5, _particles.Count);
				}
				else if ((Object)(object)_particleTrailRenderer != (Object)null)
				{
					_particleTrailRenderer.Clear();
				}
			}
			NativeArray<Vector3> val = default(NativeArray<Vector3>);
			NativeArray<int> val2 = default(NativeArray<int>);
			NativeArray<Vector2> val3 = default(NativeArray<Vector2>);
			NativeArray<Color> val4 = default(NativeArray<Color>);
			int count = particles.Count;
			if (count > 0)
			{
				val._002Ector(count * 4, (Allocator)2, (NativeArrayOptions)1);
				val2._002Ector(count * 6, (Allocator)2, (NativeArrayOptions)1);
				val3._002Ector(count * 4, (Allocator)2, (NativeArrayOptions)1);
				val4._002Ector(count * 4, (Allocator)2, (NativeArrayOptions)1);
			}
			int num6 = 0;
			int num7 = 0;
			try
			{
				for (int num8 = particles.Count - 1; num8 >= 0; num8--)
				{
					_particles[num8].Simulate(deltaTime);
					if (!(_particles[num8].TimeSinceBorn > _particles[num8].Lifetime || prewarming) && val.IsCreated)
					{
						val[num6] = Vector2.op_Implicit(particles[num8].points[0]);
						val[num6 + 1] = Vector2.op_Implicit(particles[num8].points[1]);
						val[num6 + 2] = Vector2.op_Implicit(particles[num8].points[2]);
						val[num6 + 3] = Vector2.op_Implicit(particles[num8].points[3]);
						val2[num7] = num6;
						val2[num7 + 1] = num6 + 2;
						val2[num7 + 2] = num6 + 1;
						val2[num7 + 3] = num6;
						val2[num7 + 4] = num6 + 3;
						val2[num7 + 5] = num6 + 2;
						if (_sheets != null && _sheets.Length != 0)
						{
							SpriteSheet spriteSheet = _sheets[particles[num8].GetSheetId];
							val3[num6] = spriteSheet.size;
							val3[num6 + 1] = new Vector2(spriteSheet.pos.x, spriteSheet.size.y);
							val3[num6 + 2] = spriteSheet.pos;
							val3[num6 + 3] = new Vector2(spriteSheet.size.x, spriteSheet.pos.y);
						}
						val4[num6] = particles[num8].Color;
						val4[num6 + 1] = particles[num8].Color;
						val4[num6 + 2] = particles[num8].Color;
						val4[num6 + 3] = particles[num8].Color;
						num6 += 4;
						num7 += 6;
					}
				}
				mesh.Clear();
				if (num6 > 0)
				{
					Vector3[] array = (Vector3[])(object)new Vector3[num6];
					int[] array2 = new int[num7];
					Vector2[] array3 = (Vector2[])(object)new Vector2[num6];
					Color[] array4 = (Color[])(object)new Color[num6];
					NativeArray<Vector3>.Copy(val, 0, array, 0, num6);
					NativeArray<int>.Copy(val2, 0, array2, 0, num7);
					NativeArray<Vector2>.Copy(val3, 0, array3, 0, num6);
					NativeArray<Color>.Copy(val4, 0, array4, 0, num6);
					mesh.SetVertices(array);
					mesh.SetIndices(array2, (MeshTopology)0, 0);
					mesh.SetUVs(0, array3);
					mesh.SetColors(array4);
					mesh.RecalculateBounds();
				}
				((Graphic)this).canvasRenderer.SetMesh(mesh);
				if (trailsEnabled && num5 > 0 && (Object)(object)_particleTrailRenderer != (Object)null)
				{
					_particleTrailRenderer.SetMeshData();
				}
			}
			finally
			{
				if (val.IsCreated)
				{
					val.Dispose();
				}
				if (val2.IsCreated)
				{
					val2.Dispose();
				}
				if (val3.IsCreated)
				{
					val3.Dispose();
				}
				if (val4.IsCreated)
				{
					val4.Dispose();
				}
			}
			for (int num9 = particles.Count - 1; num9 >= 0; num9--)
			{
				if (_particles[num9].TimeSinceBorn > _particles[num9].Lifetime && (_particles[num9].trailPoints.Count <= 1 || _dieWithParticle))
				{
					OnAnyParticleFinished();
					ReleaseParticle(_particles[num9]);
					_particles.RemoveAt(num9);
					if (!_firstParticleFinished)
					{
						_firstParticleFinished = true;
						OnFirstParticleFinished();
					}
					if (particleCount < 1)
					{
						OnLastParticleFinished();
					}
				}
			}
		}
	}

	public void AddBurst(float time, int count)
	{
		_bursts.Add(new XYBurst(time, count));
	}

	public void RemoveBurst(int index)
	{
		_bursts.RemoveAt(index);
	}

	public void SetBurst(int index, float time, int count)
	{
		if (_bursts.Count > index)
		{
			_bursts[index] = new XYBurst(time, count);
		}
	}

	private Vector2 GetPointOnRect(float angle, float w, float h)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sin(angle);
		float num2 = Mathf.Cos(angle);
		float num3 = ((num > 0f) ? (h / 2f) : (h / -2f));
		float num4 = ((num2 > 0f) ? (w / 2f) : (w / -2f));
		if (Mathf.Abs(num4 * num) < Mathf.Abs(num3 * num2))
		{
			num3 = num4 * num / num2;
		}
		else
		{
			num4 = num3 * num2 / num;
		}
		return new Vector2(num4, num3);
	}

	private BurstSpawnParams ComputeBurstSpawnParams(int order, int source, XYBurst burst, float startTime)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0762: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Unknown result type (might be due to invalid IL or missing references)
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_076b: Unknown result type (might be due to invalid IL or missing references)
		//IL_077c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0781: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0738: Unknown result type (might be due to invalid IL or missing references)
		//IL_0752: Unknown result type (might be due to invalid IL or missing references)
		//IL_0757: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		switch (source)
		{
		case 0:
		{
			int num2 = burst?.count ?? _runtimeBurstCount;
			if (num2 > 0)
			{
				num = (float)order * (360f / (float)num2) * _spreadLoop;
			}
			break;
		}
		case 1:
			num = (float)order * (360f / _rate) / _duration * _spreadLoop;
			break;
		case 2:
			num = (float)order * (360f / _rateOverLifetime) * _spreadLoop;
			break;
		case 3:
			num = (float)order * (360f / _rateOverDistance) / _duration * _spreadLoop;
			break;
		}
		Vector2 startPosition = Vector2.zero;
		Vector2 insideUnitCircle;
		switch (_shape)
		{
		case EmitterShape.Point:
			startPosition = Vector2.zero;
			break;
		case EmitterShape.Circle:
			if (_emitOnSurface)
			{
				startPosition = ((_spread != SpreadType.Random) ? Vector2.op_Implicit(RotateOnAngle(new Vector3(0f, Random.Range(0f, 1f), 0f), num) * _radius) : (Random.insideUnitCircle * _radius));
			}
			else if (_spread == SpreadType.Random)
			{
				insideUnitCircle = Random.insideUnitCircle;
				Vector2 normalized = ((Vector2)(ref insideUnitCircle)).normalized;
				startPosition = Vector2.op_Implicit(Vector3.Lerp(Vector2.op_Implicit(normalized * _radius), Vector2.op_Implicit(normalized * (_radius - _emitterThickness)), Random.value));
			}
			else
			{
				startPosition = Vector2.op_Implicit(RotateOnAngle(new Vector3(0f, 1f, 0f), num) * Random.Range(_radius, _radius - _emitterThickness));
			}
			break;
		case EmitterShape.Rectangle:
		{
			if (_emitOnSurface)
			{
				startPosition = ((_spread != SpreadType.Uniform) ? Vector2.op_Implicit(new Vector3(Random.Range((0f - _width) / 2f, _width / 2f), Random.Range((0f - _height) / 2f, _height / 2f))) : Vector2.Lerp(GetPointOnRect(num * (MathF.PI / 180f), _width, _height), Vector2.one, Random.value));
				break;
			}
			float num3 = Random.Range(0f, 360f);
			if (_spread == SpreadType.Uniform)
			{
				num3 = num;
			}
			startPosition = Vector2.Lerp(GetPointOnRect(num3 * (MathF.PI / 180f), _width, _height), GetPointOnRect(num3 * (MathF.PI / 180f), _width - _emitterThickness, _height - _emitterThickness), Random.value);
			break;
		}
		case EmitterShape.Line:
			startPosition = ((_spread != SpreadType.Uniform) ? Vector2.op_Implicit(new Vector3(Random.Range((0f - _length) / 2f, _length / 2f), 0f)) : Vector2.op_Implicit(new Vector3(Mathf.Repeat(num, 361f).Remap(0f, 360f, (0f - _length) / 2f, _length / 2f), 0f)));
			break;
		case EmitterShape.Directional:
			startPosition = Vector2.op_Implicit(Vector3.zero);
			break;
		case EmitterShape.ToTarget:
			startPosition = Vector2.op_Implicit(Vector3.zero);
			break;
		}
		_ = space;
		_ = 1;
		Vector3 startVelocity = Vector3.zero;
		switch (_shape)
		{
		case EmitterShape.Point:
			if (_spread == SpreadType.Uniform)
			{
				startVelocity = RotateOnAngle(new Vector3(0f, 1f, 0f), num) * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value);
				break;
			}
			insideUnitCircle = Random.insideUnitCircle;
			startVelocity = Vector2.op_Implicit(((Vector2)(ref insideUnitCircle)).normalized * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value));
			break;
		case EmitterShape.Circle:
			startVelocity = Vector2.op_Implicit(((Vector2)(ref startPosition)).normalized * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value));
			break;
		case EmitterShape.Rectangle:
			startVelocity = Vector2.op_Implicit(((Vector2)(ref startPosition)).normalized * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value));
			break;
		case EmitterShape.Line:
			startVelocity = ((space == Simulation.World) ? ((Component)this).transform.up : Vector3.up) * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value);
			break;
		case EmitterShape.Directional:
		{
			float num6 = 0f;
			num6 = ((space == Simulation.World) ? ((_spread != SpreadType.Uniform) ? (Random.Range((0f - _angle) / 2f, _angle / 2f) - ((Component)this).transform.eulerAngles.z) : (Mathf.Repeat(num, 361f).Remap(0f, 360f, (0f - _angle) / 2f, _angle / 2f) - ((Component)this).transform.eulerAngles.z)) : ((_spread != SpreadType.Uniform) ? Random.Range((0f - _angle) / 2f, _angle / 2f) : Mathf.Repeat(num, 361f).Remap(0f, 360f, (0f - _angle) / 2f, _angle / 2f)));
			startVelocity = RotateOnAngle(num6) * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value);
			break;
		}
		case EmitterShape.ToTarget:
		{
			Vector2 val = Vector2.up;
			if ((Object)(object)_attractorTarget != (Object)null)
			{
				val = GetTransformLocalPosition(_attractorTarget);
				if (((Vector2)(ref val)).sqrMagnitude > 0.0001f)
				{
					((Vector2)(ref val)).Normalize();
				}
				else
				{
					val = Vector2.up;
				}
			}
			if (_directionMode == EmitterDirectionMode.Backward)
			{
				val = -val;
			}
			float num4 = Mathf.Atan2(val.x, val.y) * 57.29578f;
			float num5 = 0f;
			num5 = ((space == Simulation.World) ? ((_spread != SpreadType.Uniform) ? (Random.Range((0f - _angle) / 2f, _angle / 2f) - ((Component)this).transform.eulerAngles.z) : (Mathf.Repeat(num, 361f).Remap(0f, 360f, (0f - _angle) / 2f, _angle / 2f) - ((Component)this).transform.eulerAngles.z)) : ((_spread != SpreadType.Uniform) ? Random.Range((0f - _angle) / 2f, _angle / 2f) : Mathf.Repeat(num, 361f).Remap(0f, 360f, (0f - _angle) / 2f, _angle / 2f)));
			startVelocity = RotateOnAngle(num4 + num5) * ((MinMaxCurve)(ref _startSpeed)).Evaluate(Random.value, Random.value);
			break;
		}
		}
		return new BurstSpawnParams
		{
			startPosition = startPosition,
			startVelocity = startVelocity,
			attractorRandom = new Vector2(Random.value, Random.value),
			rotationLerpA = Random.value,
			rotationLerpB = Random.value,
			colorLerpA = Random.value,
			colorLerpB = Random.value,
			sizeLerpA = Random.value,
			sizeLerpB = Random.value,
			alphaLerpA = Random.value,
			alphaLerpB = Random.value
		};
	}

	private float ComputeBaseParticleLifetime(in BurstSpawnParams spawnParams)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ComputeParticleLifetime(spawnParams.startPosition, spawnParams.attractorRandom);
	}

	private void EmitRuntimeBurstParticles()
	{
		int runtimeBurstCount = _runtimeBurstCount;
		BurstSpawnParams[] array = new BurstSpawnParams[runtimeBurstCount];
		for (int i = 0; i < runtimeBurstCount; i++)
		{
			array[i] = ComputeBurstSpawnParams(i, 0, null, 0f);
		}
		float num = 0f;
		for (int j = 0; j < runtimeBurstCount; j++)
		{
			num = Mathf.Max(num, ComputeBaseParticleLifetime(in array[j]));
		}
		float num2 = Mathf.Max(0f, _runtimeFixedArriveInterval);
		for (int k = 0; k < runtimeBurstCount; k++)
		{
			float num3 = num + (float)k * num2;
			_particles.Add(CreateParticleFromSpawnParams(in array[k], k, num3, 0f));
		}
	}

	private XYParticle CreateParticleFromSpawnParams(in BurstSpawnParams spawnParams, int order, float lifetime, float startTime)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		XYParticle xYParticle = pool.Get();
		xYParticle.Initialize(spawnParams.startPosition, Vector2.op_Implicit(spawnParams.startVelocity), _startRotation.EvaluateZ(spawnParams.rotationLerpA, spawnParams.rotationLerpB), ((MinMaxGradient)(ref _startColor)).Evaluate(spawnParams.colorLerpA, spawnParams.colorLerpB), SampleStartSize(spawnParams.sizeLerpA, spawnParams.sizeLerpB), IsPrefabMode ? SampleStartAlpha(spawnParams.alphaLerpA, spawnParams.alphaLerpB) : 1f, lifetime, startTime, spawnParams.attractorRandom, order);
		if (IsPrefabMode && (Object)(object)_particlePrefab != (Object)null)
		{
			RectTransform val = prefabPool.Get();
			if ((Object)(object)val != (Object)null)
			{
				xYParticle.BindPrefabInstance(val);
				ApplyRuntimeSpriteToPrefabInstance(val, order);
			}
		}
		return xYParticle;
	}

	private XYParticle GenerateParticle(int order, int source, XYBurst burst, float startTime)
	{
		BurstSpawnParams spawnParams = ComputeBurstSpawnParams(order, source, burst, startTime);
		float num = ComputeBaseParticleLifetime(in spawnParams);
		return CreateParticleFromSpawnParams(in spawnParams, order, num, startTime);
	}

	private void ApplyRuntimeSpriteToPrefabInstance(RectTransform instance, int order)
	{
		if (_runtimeSprites == null || order < 0 || order >= _runtimeSprites.Count)
		{
			return;
		}
		Sprite val = _runtimeSprites[order];
		if (!((Object)(object)val == (Object)null))
		{
			Image componentInChildren = ((Component)instance).GetComponentInChildren<Image>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				componentInChildren.sprite = val;
			}
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)this).OnRectTransformDimensionsChange();
		if (fitRect)
		{
			FitRect();
		}
		if (!_emitOnSurface)
		{
			switch (_shape)
			{
			case EmitterShape.Circle:
				_emitterThickness = Mathf.Clamp(_emitterThickness, 0f, _radius);
				break;
			case EmitterShape.Rectangle:
				_emitterThickness = Mathf.Clamp(_emitterThickness, 0f, (((Graphic)this).rectTransform.sizeDelta.x < ((Graphic)this).rectTransform.sizeDelta.y) ? _width : _height);
				break;
			case EmitterShape.Line:
				_emitterThickness = Mathf.Clamp(_emitterThickness, 0f, _radius);
				break;
			}
		}
	}

	private void FitRect()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Rect rect;
		switch (_shape)
		{
		case EmitterShape.Circle:
		{
			rect = ((Graphic)this).rectTransform.rect;
			float width = ((Rect)(ref rect)).width;
			rect = ((Graphic)this).rectTransform.rect;
			if (width > ((Rect)(ref rect)).height)
			{
				rect = ((Graphic)this).rectTransform.rect;
				_radius = ((Rect)(ref rect)).height / 2f;
			}
			else
			{
				rect = ((Graphic)this).rectTransform.rect;
				_radius = ((Rect)(ref rect)).width / 2f;
			}
			break;
		}
		case EmitterShape.Rectangle:
			rect = ((Graphic)this).rectTransform.rect;
			_width = ((Rect)(ref rect)).width;
			rect = ((Graphic)this).rectTransform.rect;
			_height = ((Rect)(ref rect)).height;
			break;
		case EmitterShape.Line:
			rect = ((Graphic)this).rectTransform.rect;
			_length = ((Rect)(ref rect)).width;
			break;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
	}

	protected override void UpdateGeometry()
	{
	}

	private Vector3 RotateOnAngle(float angle)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		float num = angle * (MathF.PI / 180f);
		return new Vector3(Mathf.Sin(num), Mathf.Cos(num), 0f) * 1f;
	}

	private Vector3 RotateOnAngle(Vector3 p, float angle)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(new Vector3(0f, 0f, angle)) * p;
	}

	public Vector3 WorldToViewportPoint(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return mainCamera.WorldToViewportPoint(position);
	}

	public Vector2 GetTransformLocalPosition(Transform target)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return Vector2.zero;
		}
		if (target is RectTransform)
		{
			return Vector2.op_Implicit(((Component)this).transform.InverseTransformPoint(target.position));
		}
		Vector3 val = WorldToViewportPoint(target.position);
		Rect rect;
		if ((int)((Graphic)this).canvas.renderMode == 1)
		{
			float x = val.x;
			rect = canvasRect.rect;
			float num = (x.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).width) - ((Transform)canvasRect).InverseTransformPoint(((Component)this).transform.position).x + ((Transform)canvasRect).localPosition.x) / ((Component)this).transform.lossyScale.x * ((Transform)canvasRect).localScale.x;
			float y = val.y;
			rect = canvasRect.rect;
			return new Vector2(num, (y.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).height) - ((Transform)canvasRect).InverseTransformPoint(((Component)this).transform.position).y + ((Transform)canvasRect).localPosition.y) / ((Component)this).transform.lossyScale.y * ((Transform)canvasRect).localScale.y);
		}
		float x2 = val.x;
		rect = canvasRect.rect;
		float num2 = (x2.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).width) - ((Transform)canvasRect).InverseTransformPoint(((Component)this).transform.position).x) / ((Component)this).transform.lossyScale.x * ((Transform)canvasRect).localScale.x;
		float y2 = val.y;
		rect = canvasRect.rect;
		return new Vector2(num2, (y2.Remap(0.5f, 1.5f, 0f, ((Rect)(ref rect)).height) - ((Transform)canvasRect).InverseTransformPoint(((Component)this).transform.position).y) / ((Component)this).transform.lossyScale.y * ((Transform)canvasRect).localScale.y);
	}

	public Vector2 ResolveAttractorEndPoint(Vector2 surfaceRandom)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_attractorTarget == (Object)null)
		{
			return Vector2.zero;
		}
		Vector2 transformLocalPosition = GetTransformLocalPosition(_attractorTarget);
		if (attractorType == AttractorType.Surface)
		{
			Transform obj = _attractorTarget;
			RectTransform val = (RectTransform)(object)((obj is RectTransform) ? obj : null);
			if (val != null)
			{
				return new Vector2(transformLocalPosition.x + surfaceRandom.x.Remap(0f, 1f, (0f - val.sizeDelta.x) / 2f, val.sizeDelta.x / 2f), transformLocalPosition.y + surfaceRandom.y.Remap(0f, 1f, (0f - val.sizeDelta.y) / 2f, val.sizeDelta.y / 2f));
			}
		}
		return transformLocalPosition;
	}

	public Vector2 ComputeBezierControlPoint(Vector2 start, Vector2 end)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		switch (_bezierControlPointScheme)
		{
		case BezierControlPointScheme.Custom:
			return start + _customBezierControlOffset;
		case BezierControlPointScheme.PropReward:
		{
			float num2 = _propRewardPathBlend;
			return new Vector2((1f - num2) * start.x + num2 * end.x + _propRewardOffsetX, (1f - num2) * start.y + num2 * end.y + _propRewardOffsetY);
		}
		default:
		{
			float num = Mathf.Clamp01(_coinSettleEndXBlend);
			return new Vector2((1f - num) * start.x + num * end.x, start.y - ScaleReferencePixelY(_coinSettleStartYOffset));
		}
		}
	}

	private float ScaleReferencePixelY(float pixelY)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)canvasRect != (Object)null)
		{
			Rect rect = canvasRect.rect;
			return pixelY * (((Rect)(ref rect)).height / 1920f);
		}
		return pixelY;
	}

	public static Vector2 EvaluateQuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		return num * num * start + 2f * num * t * control + t * t * end;
	}

	private void OnParticleStart()
	{
		onParticleStarted.Invoke();
	}

	private void OnFirstParticleFinished()
	{
		onFirstParticleFinished.Invoke();
		main.TryInvokeRuntimeFirst();
	}

	private void OnAnyParticleFinished()
	{
		onAnyParticleFinished.Invoke();
		main.TryInvokeRuntimeAny();
	}

	private void OnLastParticleFinished()
	{
		onLastParticleFinished.Invoke();
		if (isMain)
		{
			TryInvokeRuntimeLast();
			ClearRuntimeEmitState();
			ReleaseRuntimeLoadedSprite();
		}
	}

	private void OnParticleStop()
	{
		onParticleStop.Invoke();
	}

	public bool LoadRuntimeSprite(string location)
	{
		if (string.IsNullOrEmpty(location))
		{
			return false;
		}
		ReleaseRuntimeLoadedSprite();
		Sprite val = GameApp.Res.LoadAsset<Sprite>(location, (string)null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		sprite = val;
		_runtimeLoadedSpriteLocation = location;
		RefreshSpriteRendering();
		return true;
	}

	private void ReleaseRuntimeLoadedSprite()
	{
		if (!string.IsNullOrEmpty(_runtimeLoadedSpriteLocation))
		{
			GameApp.Res.UnloadAsset(_runtimeLoadedSpriteLocation, (string)null, 1);
			_runtimeLoadedSpriteLocation = null;
			RestoreDefaultSprite();
			RefreshSpriteRendering();
		}
	}

	private void RestoreDefaultSprite()
	{
		m_Sprite = _defaultSprite;
	}

	private void RefreshSpriteRendering()
	{
		RefreshSpriteAspectAndSheets();
		if ((Object)(object)mesh != (Object)null)
		{
			mesh.Clear();
			((Graphic)this).canvasRenderer.SetMesh(mesh);
		}
	}
}
