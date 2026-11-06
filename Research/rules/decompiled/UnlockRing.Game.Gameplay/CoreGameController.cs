using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Rules;
using UnlockRing.Core.Runtime;
using UnlockRing.Core.Serialization;
using UnlockRing.IntegrationSample;

namespace UnlockRing.Game.Gameplay;

public sealed class CoreGameController
{
	private const string TAG = "[UnlockRing][CoreGameController] ";

	public const double QuickTranslationHorizon = 12.0;

	public const double QuickRotationHorizonDegrees = 20.0;

	private readonly Dictionary<ClientOperationKey, AvailableOperation> _operations = new Dictionary<ClientOperationKey, AvailableOperation>();

	private readonly Dictionary<ClientOperationKey, HeldGestureRange> _heldGestureRanges = new Dictionary<ClientOperationKey, HeldGestureRange>();

	private readonly HashSet<PartId> _fullPartOperations = new HashSet<PartId>();

	private readonly OperationQueryPlan _operationPlan;

	private LevelDefinition? _heldGestureLevel;

	private BoardState? _heldGestureState;

	private AvailableOperationsSnapshot? _installedSnapshot;

	private GameSession _session;

	private readonly Guid _controllerIdentity = Guid.NewGuid();

	private long _sessionEpoch;

	public LevelDefinition Level { get; }

	public BoardState State => _session.State;

	public bool IsGestureActive => _session.ActiveGesturePart.HasValue;

	public IReadOnlyDictionary<ClientOperationKey, AvailableOperation> Operations => _operations;

	public CoreGameController(LoadedPlayableLevel loadedPlayableLevel)
	{
		LevelDefinition level = loadedPlayableLevel.PlayableLevelPackLevel.CertifiedInitialState.Level;
		Level = level ?? throw new ArgumentNullException("level");
		_session = loadedPlayableLevel.Session;
		_operationPlan = CoreQueries.BuildOperationQueryPlan(level, _session.State, (GestureDomainOptions)null, (GestureDomainOptions)null, default(CancellationToken));
	}

	public CoreGameController(LevelDefinition level)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		Level = level ?? throw new ArgumentNullException("level");
		_session = new GameSession(level);
		_operationPlan = CoreQueries.BuildOperationQueryPlan(level, _session.State, (GestureDomainOptions)null, (GestureDomainOptions)null, default(CancellationToken));
	}

	public ReplayLog CreateReplay()
	{
		return _session.CreateReplay();
	}

	public Task<AvailableOperationsSnapshot> PrecomputeOperationsAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		LevelDefinition level = Level;
		BoardState state = State;
		AvailableOperationsSnapshot previousSnapshot = _installedSnapshot;
		Stopwatch sw = Stopwatch.StartNew();
		return Task.Run(delegate
		{
			try
			{
				return (previousSnapshot == null) ? CoreQueries.BuildAvailableOperationsSnapshot(level, state, _operationPlan, OperationQueryExecutionOptions.Default, cancellationToken) : CoreQueries.RefreshAvailableOperationsSnapshot(level, state, previousSnapshot, OperationQueryExecutionOptions.Default, cancellationToken);
			}
			finally
			{
				sw.Stop();
				LogProfileTiming("PrecomputeOperationsAsync", sw.Elapsed.TotalMilliseconds);
			}
		}, cancellationToken);
	}

	public Task<PartOperationsSnapshot> PrecomputePartOperationsAsync(PartId partId, CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return PrecomputePartOperationsAsync(partId, quick: false, cancellationToken);
	}

	public Task<PartOperationsSnapshot> PrecomputeQuickPartOperationsAsync(PartId partId, CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return PrecomputePartOperationsAsync(partId, quick: true, cancellationToken);
	}

	private Task<PartOperationsSnapshot> PrecomputePartOperationsAsync(PartId partId, bool quick, CancellationToken cancellationToken)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		LevelDefinition level = Level;
		BoardState state = State;
		long sessionEpoch = _sessionEpoch;
		AvailableOperationKind[] kinds = ContinuousKinds(level, partId);
		GestureDomainOptions translationOptions = ((!quick) ? ((GestureDomainOptions)null) : new GestureDomainOptions((MotionKind?)(MotionKind)1, (double?)QuickTranslationHorizonFor(level, state, partId), 360.0));
		GestureDomainOptions rotationOptions = ((!quick) ? ((GestureDomainOptions)null) : new GestureDomainOptions((MotionKind?)(MotionKind)0, (double?)null, 20.0));
		Stopwatch sw = Stopwatch.StartNew();
		return Task.Run(delegate
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				List<AvailableOperationResult> list = new List<AvailableOperationResult>(kinds.Length);
				Dictionary<ClientOperationKey, HeldGestureRange> dictionary = new Dictionary<ClientOperationKey, HeldGestureRange>();
				AvailableOperationKind[] array = kinds;
				foreach (AvailableOperationKind val in array)
				{
					cancellationToken.ThrowIfCancellationRequested();
					AvailableOperationResult result = CoreQueries.QueryAvailableOperation(level, state, partId, val, translationOptions, rotationOptions, cancellationToken, true);
					result = OperationDomainRefiner.RefineFirstTranslationRelease(level, state, result, cancellationToken);
					if (!quick)
					{
						result = OperationDomainRefiner.RecoverCertifiedTranslationRelease(level, state, result, cancellationToken);
						result = OperationDomainRefiner.ExtendCertifiedRotationRange(level, state, result, cancellationToken);
						result = OperationDomainRefiner.RecoverCertifiedRotationRelease(level, state, result, cancellationToken);
					}
					result = OperationDomainRefiner.ApplyElbowGuideStops(level, state, result, cancellationToken);
					list.Add(result);
					AvailableOperation operation = result.Operation;
					if (OperationDomainRefiner.HasReleasedBound((operation != null) ? operation.MotionDomain : null))
					{
						HeldGestureRange? heldGestureRange = OperationDomainRefiner.QueryHeldGestureRange(level, state, partId, val, translationOptions, rotationOptions, cancellationToken);
						if (heldGestureRange.HasValue)
						{
							HeldGestureRange valueOrDefault = heldGestureRange.GetValueOrDefault();
							AvailableOperation operation2 = result.Operation;
							GestureMotionDomain val2 = ((operation2 != null) ? operation2.MotionDomain : null);
							if (val2 != null && valueOrDefault.MinimumSignedAmount <= val2.MinimumSignedAmount + 1E-09 && valueOrDefault.MaximumSignedAmount >= val2.MaximumSignedAmount - 1E-09)
							{
								dictionary[new ClientOperationKey(partId, val)] = valueOrDefault;
							}
						}
					}
				}
				return new PartOperationsSnapshot(list, dictionary, _controllerIdentity, sessionEpoch, quick);
			}
			finally
			{
				sw.Stop();
				LogProfileTiming(quick ? "PrecomputeQuickPartOperations" : "PrecomputePartOperations", sw.Elapsed.TotalMilliseconds);
			}
		}, cancellationToken);
	}

	private static double QuickTranslationHorizonFor(LevelDefinition level, BoardState state, PartId partId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		PartDefinition part = level.Parts.Single(delegate(PartDefinition candidate)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			PartId id = candidate.Id;
			return ((PartId)(ref id)).Equals(partId);
		});
		PartState partState = state.Parts[partId];
		double num = (from feature in part.Features
			select ShapeConverter.Convert(part, partState, feature) into value
			where value.Conversion.IsExact
			select value).Select(delegate(ResolvedFeatureGeometry value)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			Aabb bounds = value.Conversion.Shape.Bounds;
			Vector2 size = ((Aabb)(ref bounds)).Size;
			return ((Vector2)(ref size)).Length;
		}).DefaultIfEmpty(0.0).Max();
		return Math.Max(12.0, num * 2.0);
	}

	public bool InstallPartOperations(PartOperationsSnapshot snapshot)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		if (snapshot == null)
		{
			throw new ArgumentNullException("snapshot");
		}
		if (snapshot.ControllerIdentity != _controllerIdentity || snapshot.SessionEpoch != _sessionEpoch)
		{
			return false;
		}
		IReadOnlyList<AvailableOperationResult> results = snapshot.Results;
		if (results.Count == 0)
		{
			return false;
		}
		BoardState state = State;
		string text = CanonicalJsonV1.ComputeSha256(state);
		Dictionary<ClientOperationKey, AvailableOperation> dictionary = new Dictionary<ClientOperationKey, AvailableOperation>(_operations);
		Dictionary<ClientOperationKey, HeldGestureRange> dictionary2 = new Dictionary<ClientOperationKey, HeldGestureRange>(_heldGestureRanges);
		HashSet<ClientOperationKey> hashSet = new HashSet<ClientOperationKey>();
		OperationKey key;
		foreach (AvailableOperationResult item in results)
		{
			key = item.Key;
			PartId partId = ((OperationKey)(ref key)).PartId;
			key = item.Key;
			dictionary2.Remove(new ClientOperationKey(partId, ((OperationKey)(ref key)).Kind));
		}
		foreach (AvailableOperationResult item2 in results)
		{
			if (!item2.BaseStateAccepted || item2.BaseStateRevision != state.Revision || !string.Equals(item2.BaseStateHash, text, StringComparison.Ordinal))
			{
				return false;
			}
			key = item2.Key;
			PartId partId2 = ((OperationKey)(ref key)).PartId;
			key = item2.Key;
			ClientOperationKey clientOperationKey = new ClientOperationKey(partId2, ((OperationKey)(ref key)).Kind);
			if (hashSet.Add(clientOperationKey))
			{
				key = item2.Key;
				if (IsContinuousGesture(((OperationKey)(ref key)).Kind))
				{
					if (item2.Operation == null)
					{
						dictionary.Remove(clientOperationKey);
						continue;
					}
					if (OperationMatchesState(item2.Operation, state, text))
					{
						PartId partId3 = item2.Operation.PartId;
						key = item2.Key;
						if (((PartId)(ref partId3)).Equals(((OperationKey)(ref key)).PartId))
						{
							AvailableOperationKind kind = item2.Operation.Kind;
							key = item2.Key;
							if (kind == ((OperationKey)(ref key)).Kind)
							{
								dictionary[clientOperationKey] = item2.Operation;
								continue;
							}
						}
					}
					return false;
				}
			}
			return false;
		}
		foreach (KeyValuePair<ClientOperationKey, HeldGestureRange> heldGestureRange in snapshot.HeldGestureRanges)
		{
			dictionary2[heldGestureRange.Key] = heldGestureRange.Value;
		}
		_operations.Clear();
		foreach (KeyValuePair<ClientOperationKey, AvailableOperation> item3 in dictionary)
		{
			_operations.Add(item3.Key, item3.Value);
		}
		_heldGestureRanges.Clear();
		foreach (KeyValuePair<ClientOperationKey, HeldGestureRange> item4 in dictionary2)
		{
			_heldGestureRanges.Add(item4.Key, item4.Value);
		}
		if (snapshot.IsQuick)
		{
			_fullPartOperations.ExceptWith(results.Select(delegate(AvailableOperationResult result)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				OperationKey key2 = result.Key;
				return ((OperationKey)(ref key2)).PartId;
			}));
		}
		else
		{
			_fullPartOperations.UnionWith(results.Select(delegate(AvailableOperationResult result)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				OperationKey key2 = result.Key;
				return ((OperationKey)(ref key2)).PartId;
			}));
		}
		return true;
	}

	public bool InstallOperations(AvailableOperationsSnapshot snapshot)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (snapshot == null)
		{
			throw new ArgumentNullException("snapshot");
		}
		BoardState state = State;
		string text = CanonicalJsonV1.ComputeSha256(state);
		if (!snapshot.BaseStateAccepted || snapshot.BaseStateRevision != state.Revision || !string.Equals(snapshot.BaseStateHash, text, StringComparison.Ordinal))
		{
			return false;
		}
		Dictionary<ClientOperationKey, AvailableOperation> dictionary = new Dictionary<ClientOperationKey, AvailableOperation>();
		foreach (AvailableOperation operation in snapshot.Operations)
		{
			ClientOperationKey key = new ClientOperationKey(operation.PartId, operation.Kind);
			if (!dictionary.TryAdd(key, operation))
			{
				return false;
			}
			if (!OperationMatchesState(operation, state, text))
			{
				return false;
			}
		}
		_operations.Clear();
		foreach (KeyValuePair<ClientOperationKey, AvailableOperation> item in dictionary)
		{
			_operations.Add(item.Key, item.Value);
		}
		_heldGestureRanges.Clear();
		_fullPartOperations.Clear();
		_fullPartOperations.UnionWith(_operations.Keys.Select((ClientOperationKey clientOperationKey) => clientOperationKey.PartId));
		_installedSnapshot = snapshot;
		return true;
	}

	public IReadOnlyList<AvailableOperation> GetOperations(PartId partId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return (from operation in _operations.Values.Where(delegate(AvailableOperation operation)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				PartId partId2 = operation.PartId;
				return ((PartId)(ref partId2)).Equals(partId);
			})
			orderby operation.Kind
			select operation).ToArray();
	}

	public double ClampHeldGestureAmount(GestureMotionDomain domain, double amount)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		AvailableOperationKind kind = (AvailableOperationKind)((int)domain.MotionKind == 0);
		if (_heldGestureRanges.TryGetValue(new ClientOperationKey(domain.PartId, kind), out var value))
		{
			return value.Clamp(amount);
		}
		return domain.ClampSignedAmount(amount);
	}

	public MotionPreview? PreviewGesture(GestureMotionDomain domain, double signedAmount)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (_session.ActiveGesturePart.HasValue)
		{
			PartId value = _session.ActiveGesturePart.Value;
			if (((PartId)(ref value)).Equals(domain.PartId))
			{
				return CoreRuntime.Preview(_heldGestureLevel ?? Level, _heldGestureState ?? State, CreateUnclampedRequest(domain, signedAmount));
			}
		}
		return null;
	}

	public Task<MotionPreview?> PreviewGestureAsync(GestureMotionDomain domain, double signedAmount, CancellationToken cancellationToken)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (_session.ActiveGesturePart.HasValue)
		{
			PartId value = _session.ActiveGesturePart.Value;
			if (((PartId)(ref value)).Equals(domain.PartId))
			{
				LevelDefinition level = _heldGestureLevel ?? Level;
				BoardState state = _heldGestureState ?? State;
				MotionRequest request = CreateUnclampedRequest(domain, signedAmount);
				return Task.Run(() => CoreRuntime.Preview(level, state, request, cancellationToken), cancellationToken);
			}
		}
		return Task.FromResult<MotionPreview>(null);
	}

	public bool HasCurrentPartOperations(PartId partId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		AvailableOperationKind[] array = ContinuousKinds(Level, partId);
		if (array.Length == 0)
		{
			return false;
		}
		BoardState state = State;
		string stateHash = CanonicalJsonV1.ComputeSha256(state);
		AvailableOperation value;
		return array.All((AvailableOperationKind kind) => _operations.TryGetValue(new ClientOperationKey(partId, kind), out value) && OperationMatchesState(value, state, stateHash));
	}

	public bool HasCurrentFullPartOperations(PartId partId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (_fullPartOperations.Contains(partId))
		{
			return HasCurrentPartOperations(partId);
		}
		return false;
	}

	public bool TryResolveGestureIntent(PartId partId, Vector2D pressWorld, Vector2D currentWorld, double minimumDistance, out AvailableOperationKind kind, out string reason)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected I4, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if (!double.IsFinite(minimumDistance) || minimumDistance < 0.0)
		{
			throw new ArgumentOutOfRangeException("minimumDistance");
		}
		kind = (AvailableOperationKind)0;
		AvailableOperation[] array = (from operation in GetOperations(partId)
			where (int)operation.Availability == 0 && operation.MotionDomain != null && IsContinuousGesture(operation.Kind)
			select operation).ToArray();
		if (array.Length == 0)
		{
			reason = DescribeOperations(partId);
			return false;
		}
		if (array.Length == 1)
		{
			kind = (AvailableOperationKind)(int)array[0].Kind;
			reason = string.Empty;
			return true;
		}
		Vector2D value = currentWorld - pressWorld;
		if (LengthSquared(value) < minimumDistance * minimumDistance)
		{
			reason = "该部件同时支持旋转与平移；沿导轨拖动可平移，绕中心切向拖动可旋转";
			return false;
		}
		Vector2D val = Normalize(value);
		AvailableOperation obj = array.Single((AvailableOperation operation) => (int)operation.Kind == 0);
		AvailableOperation val2 = array.Single((AvailableOperation operation) => (int)operation.Kind == 1);
		Vector2D value2 = obj.MotionDomain.TranslationWorldAxis.Value;
		double num = Math.Abs(Dot(val, value2));
		Vector2D value3 = pressWorld - val2.MotionDomain.RotationPivot.Value;
		double num2 = ((LengthSquared(value3) <= 1E-18) ? 0.0 : Math.Abs(Cross(Normalize(value3), val)));
		kind = (AvailableOperationKind)((num2 > num) ? 1 : 0);
		reason = string.Empty;
		return true;
	}

	public bool TryBeginGesture(PartId partId, AvailableOperationKind kind, out GestureMotionDomain? domain, out string reason)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Invalid comparison between Unknown and I4
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		domain = null;
		if (!_operations.TryGetValue(new ClientOperationKey(partId, kind), out var value))
		{
			reason = "该部件未声明" + KindLabel(kind) + "操作";
			return false;
		}
		if ((int)value.Availability == 2)
		{
			reason = KindLabel(kind) + "证书不完整，已安全禁用：" + ReasonOrFallback(value.Reason, "Core 无法证明安全操作域");
			return false;
		}
		if ((int)value.Availability == 1)
		{
			reason = KindLabel(kind) + "已阻挡：" + ReasonOrFallback(value.Reason, "当前没有非零安全域");
			return false;
		}
		GestureMotionDomain motionDomain = value.MotionDomain;
		if (motionDomain == null)
		{
			reason = KindLabel(kind) + "是即时操作，尚未映射为拖动手势";
			return false;
		}
		if (motionDomain.BaseStateRevision != State.Revision || !string.Equals(motionDomain.BaseStateHash, CanonicalJsonV1.ComputeSha256(State), StringComparison.Ordinal))
		{
			reason = "操作快照已经失效";
			return false;
		}
		MotionKind val = (MotionKind)((int)kind != 1);
		if (!_session.BeginGesture(partId, val))
		{
			reason = "当前已有手势，或部件已退出盘面";
			return false;
		}
		(LevelDefinition, BoardState) tuple = HeldGestureProjection.Create(Level, State);
		_heldGestureLevel = tuple.Item1;
		_heldGestureState = tuple.Item2;
		domain = motionDomain;
		reason = string.Empty;
		return true;
	}

	public string DescribeOperations(PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<AvailableOperation> operations = GetOperations(partId);
		if (operations.Count == 0)
		{
			return "该部件没有当前操作";
		}
		return string.Join("；", operations.Select(delegate(AvailableOperation operation)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			string text = KindLabel(operation.Kind);
			string text2 = AvailabilityLabel(operation.Availability);
			string text3;
			if ((int)operation.Availability != 0)
			{
				text3 = "（" + ReasonOrFallback(operation.Reason, "未提供原因") + "）";
			}
			else
			{
				GestureMotionDomain motionDomain = operation.MotionDomain;
				text3 = "（" + CompletenessLabel((motionDomain != null) ? new GestureMotionDomainCompleteness?(motionDomain.Completeness) : ((GestureMotionDomainCompleteness?)null)) + "）";
			}
			return text + text2 + text3;
		}));
	}

	public ForceRemoveResult? ForceRemove(PartId partId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (IsGestureActive)
		{
			return null;
		}
		ForceRemoveResult obj = _session.ForceRemove(partId);
		if ((int)obj.Status == 0)
		{
			ClearGestureCaches();
		}
		return obj;
	}

	public MotionResult? CommitGesture(GestureMotionDomain domain, double signedAmount)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (_session.ActiveGesturePart.HasValue)
		{
			PartId value = _session.ActiveGesturePart.Value;
			if (((PartId)(ref value)).Equals(domain.PartId))
			{
				if (Math.Abs(signedAmount) <= 1E-09)
				{
					_session.CancelGesture();
					_heldGestureLevel = null;
					_heldGestureState = null;
					return null;
				}
				MotionResult result = _session.CommitGesture(domain.CreateClampedRequest(signedAmount));
				ClearGestureCaches();
				return result;
			}
		}
		return null;
	}

	public MotionResult? CommitPreviewedGesture(GestureMotionDomain domain, double signedAmount)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (_session.ActiveGesturePart.HasValue)
		{
			PartId value = _session.ActiveGesturePart.Value;
			if (((PartId)(ref value)).Equals(domain.PartId))
			{
				if (Math.Abs(signedAmount) <= 1E-09)
				{
					_session.CancelGesture();
					_heldGestureLevel = null;
					_heldGestureState = null;
					return null;
				}
				double num = signedAmount;
				MotionResult val = null;
				for (int i = 0; i < 8; i++)
				{
					MotionResult val2 = _session.CommitGesture(CreateUnclampedRequest(domain, num));
					if (val2.State.Parts.TryGetValue(domain.PartId, out var value2) && (value2.Released || value2.Removed))
					{
						val = val2;
						break;
					}
					double num2 = (((int)domain.MotionKind == 0) ? val2.ActualAmount : ((double)Math.Sign(num) * Math.Abs(val2.ActualAmount)));
					if (Math.Abs(num2) <= 1E-09)
					{
						if (val == null)
						{
							val = val2;
						}
						break;
					}
					val = val2;
					if (!val2.Truncated)
					{
						break;
					}
					num -= num2;
					if (Math.Abs(num) <= 1E-09 || !_session.BeginGesture(domain.PartId, domain.MotionKind))
					{
						break;
					}
				}
				ClearGestureCaches();
				return val;
			}
		}
		return null;
	}

	private void ClearGestureCaches()
	{
		_heldGestureLevel = null;
		_heldGestureState = null;
		if (_session.ActiveGesturePart.HasValue)
		{
			_session.CancelGesture();
		}
		_operations.Clear();
		_heldGestureRanges.Clear();
		_fullPartOperations.Clear();
	}

	internal static MotionRequest CreateUnclampedRequest(GestureMotionDomain domain, double signedAmount)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!double.IsFinite(signedAmount))
		{
			throw new ArgumentOutOfRangeException("signedAmount");
		}
		if ((int)domain.MotionKind == 0)
		{
			return MotionRequest.Rotate(domain.PartId, signedAmount);
		}
		Vector2D val = domain.TranslationWorldAxis ?? throw new InvalidOperationException("Translation domain has no world axis.");
		if (signedAmount < 0.0)
		{
			val *= -1.0;
		}
		return MotionRequest.Translate(domain.PartId, Math.Abs(signedAmount), val);
	}

	public void CancelGesture()
	{
		_session.CancelGesture();
		_heldGestureLevel = null;
		_heldGestureState = null;
	}

	public void Reset()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		_session.CancelGesture();
		_heldGestureLevel = null;
		_heldGestureState = null;
		_session = new GameSession(Level);
		_sessionEpoch++;
		_operations.Clear();
		_heldGestureRanges.Clear();
		_fullPartOperations.Clear();
		_installedSnapshot = null;
	}

	private static bool IsContinuousGesture(AvailableOperationKind kind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		if ((int)kind != 0)
		{
			return (int)kind == 1;
		}
		return true;
	}

	private static AvailableOperationKind[] ContinuousKinds(LevelDefinition level, PartId partId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		PartDefinition? obj = ((IEnumerable<PartDefinition>)level.Parts).SingleOrDefault((Func<PartDefinition, bool>)delegate(PartDefinition part)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			PartId id = part.Id;
			return ((PartId)(ref id)).Equals(partId);
		});
		return ((obj != null) ? (from kind in obj.Behaviors.SelectMany(delegate(BehaviorDefinition behavior)
			{
				if (behavior is AxisSlidableBehaviorDefinition || behavior is LinearGuideBehaviorDefinition)
				{
					return (IEnumerable<AvailableOperationKind>)(object)new AvailableOperationKind[1];
				}
				return (IEnumerable<AvailableOperationKind>)((behavior is RotatableBehaviorDefinition) ? ((Array)new AvailableOperationKind[1] { (AvailableOperationKind)1 }) : ((Array)Array.Empty<AvailableOperationKind>()));
			}).Distinct()
			orderby kind
			select kind).ToArray() : null) ?? Array.Empty<AvailableOperationKind>();
	}

	private static bool KindMatchesDomain(AvailableOperationKind kind, MotionKind motionKind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if ((int)kind != 0 || (int)motionKind != 1)
		{
			if ((int)kind == 1)
			{
				return (int)motionKind == 0;
			}
			return false;
		}
		return true;
	}

	private static bool OperationMatchesState(AvailableOperation operation, BoardState state, string stateHash)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		GestureMotionDomain motionDomain = operation.MotionDomain;
		if (motionDomain == null)
		{
			return true;
		}
		if (motionDomain.BaseStateRevision == state.Revision && string.Equals(motionDomain.BaseStateHash, stateHash, StringComparison.Ordinal))
		{
			PartId partId = motionDomain.PartId;
			if (((PartId)(ref partId)).Equals(operation.PartId))
			{
				return KindMatchesDomain(operation.Kind, motionDomain.MotionKind);
			}
		}
		return false;
	}

	private static double LengthSquared(Vector2D value)
	{
		return ((Vector2D)(ref value)).X * ((Vector2D)(ref value)).X + ((Vector2D)(ref value)).Y * ((Vector2D)(ref value)).Y;
	}

	private static Vector2D Normalize(Vector2D value)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		double length = ((Vector2D)(ref value)).Length;
		if (!(length <= 1E-09))
		{
			return new Vector2D(((Vector2D)(ref value)).X / length, ((Vector2D)(ref value)).Y / length);
		}
		return Vector2D.Zero;
	}

	private static double Dot(Vector2D left, Vector2D right)
	{
		return ((Vector2D)(ref left)).X * ((Vector2D)(ref right)).X + ((Vector2D)(ref left)).Y * ((Vector2D)(ref right)).Y;
	}

	private static double Cross(Vector2D left, Vector2D right)
	{
		return ((Vector2D)(ref left)).X * ((Vector2D)(ref right)).Y - ((Vector2D)(ref left)).Y * ((Vector2D)(ref right)).X;
	}

	private unsafe static string KindLabel(AvailableOperationKind kind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		return (int)kind switch
		{
			0 => "平移", 
			1 => "旋转", 
			2 => "强制移除", 
			_ => ((object)(*(AvailableOperationKind*)(&kind))/*cast due to .constrained prefix*/).ToString(), 
		};
	}

	private unsafe static string AvailabilityLabel(OperationAvailability availability)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		return (int)availability switch
		{
			0 => "可用", 
			1 => "已阻挡", 
			2 => "证书不完整", 
			_ => ((object)(*(OperationAvailability*)(&availability))/*cast due to .constrained prefix*/).ToString(), 
		};
	}

	private static string CompletenessLabel(GestureMotionDomainCompleteness? completeness)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected I4, but got Unknown
		return completeness switch
		{
			(GestureMotionDomainCompleteness)0L => "有限域精确证书", 
			(GestureMotionDomainCompleteness)1L => "有限域保守证书", 
			(GestureMotionDomainCompleteness)2L => "证书不完整", 
			_ => "即时操作", 
		};
	}

	private static string ReasonOrFallback(string reason, string fallback)
	{
		if (!string.IsNullOrWhiteSpace(reason))
		{
			return reason;
		}
		return fallback;
	}

	private static void LogProfileTiming(string label, double elapsedMs)
	{
		_ = 1.0;
	}
}
