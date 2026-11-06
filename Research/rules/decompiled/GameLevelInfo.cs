using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnlockRing.Core.Geometry;

public class GameLevelInfo : MonoBehaviour
{
	public Camera levelCaptureCamera;

	public WeakGuideManager weakGuideManager;

	public bool isUseCoreLevel;

	public bool isModelLevel;

	public bool specifyColor;

	[SerializeField]
	private string coreLevelJsonHash = string.Empty;

	[SerializeField]
	[HideInInspector]
	private double boundsMinX;

	[SerializeField]
	[HideInInspector]
	private double boundsMinY;

	[SerializeField]
	[HideInInspector]
	private double boundsMaxX;

	[SerializeField]
	[HideInInspector]
	private double boundsMaxY;

	[SerializeField]
	private RingLinkEntry[] ringLinks = Array.Empty<RingLinkEntry>();

	[FormerlySerializedAs("pullLinks")]
	[SerializeField]
	private GuideLinkEntry[] guideLinks = Array.Empty<GuideLinkEntry>();

	[SerializeField]
	private PaletteLockKeyBindingEntry[] paletteLockKeyBindings = Array.Empty<PaletteLockKeyBindingEntry>();

	public string CoreLevelJsonHash => coreLevelJsonHash ?? string.Empty;

	public IReadOnlyList<RingLinkEntry> RingLinks => ringLinks ?? Array.Empty<RingLinkEntry>();

	public IReadOnlyList<GuideLinkEntry> GuideLinks => guideLinks ?? Array.Empty<GuideLinkEntry>();

	public IReadOnlyList<PaletteLockKeyBindingEntry> PaletteLockKeyBindings => paletteLockKeyBindings ?? Array.Empty<PaletteLockKeyBindingEntry>();

	public Aabb Bounds
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			return new Aabb(new Vector2(boundsMinX, boundsMinY), new Vector2(boundsMaxX, boundsMaxY));
		}
		set
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = ((Aabb)(ref value)).Min;
			boundsMinX = ((Vector2)(ref val)).X;
			val = ((Aabb)(ref value)).Min;
			boundsMinY = ((Vector2)(ref val)).Y;
			val = ((Aabb)(ref value)).Max;
			boundsMaxX = ((Vector2)(ref val)).X;
			val = ((Aabb)(ref value)).Max;
			boundsMaxY = ((Vector2)(ref val)).Y;
		}
	}

	public void SetRingLinks(IReadOnlyList<RingLinkEntry> links)
	{
		if (links == null || links.Count == 0)
		{
			ringLinks = Array.Empty<RingLinkEntry>();
			return;
		}
		RingLinkEntry[] array = new RingLinkEntry[links.Count];
		for (int i = 0; i < links.Count; i++)
		{
			array[i] = links[i];
		}
		ringLinks = array;
	}

	public void SetGuideLinks(IReadOnlyList<GuideLinkEntry> links)
	{
		if (links == null || links.Count == 0)
		{
			guideLinks = Array.Empty<GuideLinkEntry>();
			return;
		}
		GuideLinkEntry[] array = new GuideLinkEntry[links.Count];
		for (int i = 0; i < links.Count; i++)
		{
			array[i] = links[i];
		}
		guideLinks = array;
	}

	public void SetPaletteLockKeyBindings(IReadOnlyList<PaletteLockKeyBindingEntry> bindings)
	{
		if (bindings == null || bindings.Count == 0)
		{
			paletteLockKeyBindings = Array.Empty<PaletteLockKeyBindingEntry>();
			return;
		}
		PaletteLockKeyBindingEntry[] array = new PaletteLockKeyBindingEntry[bindings.Count];
		for (int i = 0; i < bindings.Count; i++)
		{
			PaletteLockKeyBindingEntry paletteLockKeyBindingEntry = bindings[i];
			string[] lockedAssemblyNames = paletteLockKeyBindingEntry.lockedAssemblyNames;
			if (lockedAssemblyNames == null || lockedAssemblyNames.Length == 0)
			{
				array[i] = new PaletteLockKeyBindingEntry
				{
					keyAssemblyName = paletteLockKeyBindingEntry.keyAssemblyName,
					lockedAssemblyNames = Array.Empty<string>(),
					keyColor = paletteLockKeyBindingEntry.keyColor
				};
			}
			else
			{
				string[] array2 = new string[lockedAssemblyNames.Length];
				Array.Copy(lockedAssemblyNames, array2, lockedAssemblyNames.Length);
				array[i] = new PaletteLockKeyBindingEntry
				{
					keyAssemblyName = paletteLockKeyBindingEntry.keyAssemblyName,
					lockedAssemblyNames = array2,
					keyColor = paletteLockKeyBindingEntry.keyColor
				};
			}
		}
		paletteLockKeyBindings = array;
	}

	public void SetCoreLevelJsonHash(string hash)
	{
		coreLevelJsonHash = (string.IsNullOrEmpty(hash) ? string.Empty : hash);
	}

	public Camera GetInputCamera()
	{
		if ((Object)(object)levelCaptureCamera != (Object)null)
		{
			return levelCaptureCamera;
		}
		return FindCaptureCameraInChildren(((Component)this).gameObject);
	}

	public static Camera ResolveInputCamera(GameObject levelRoot, out GameLevelInfo gameLevelInfo)
	{
		gameLevelInfo = null;
		if ((Object)(object)levelRoot == (Object)null)
		{
			return null;
		}
		gameLevelInfo = levelRoot.GetComponent<GameLevelInfo>();
		if ((Object)(object)gameLevelInfo != (Object)null)
		{
			return gameLevelInfo.GetInputCamera();
		}
		return FindCaptureCameraInChildren(levelRoot);
	}

	private static Camera FindCaptureCameraInChildren(GameObject root)
	{
		Camera[] componentsInChildren = root.GetComponentsInChildren<Camera>(true);
		foreach (Camera val in componentsInChildren)
		{
			if ((Object)(object)val != (Object)null && ((Object)((Component)val).gameObject).name == "LevelCaptureCamera")
			{
				return val;
			}
		}
		return null;
	}

	private void Awake()
	{
		if ((Object)(object)levelCaptureCamera != (Object)null)
		{
			((Behaviour)levelCaptureCamera).enabled = false;
		}
	}
}
