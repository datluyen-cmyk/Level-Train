using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingSolveMaterialPool : IDisposable
{
	private readonly Dictionary<Material, Stack<Material>> inPool = new Dictionary<Material, Stack<Material>>();

	private readonly Dictionary<Material, Material> inUse = new Dictionary<Material, Material>();

	public Material Get(Material template)
	{
		if ((Object)(object)template == (Object)null)
		{
			throw new ArgumentNullException("template");
		}
		if (!inPool.TryGetValue(template, out var value))
		{
			value = new Stack<Material>();
			inPool.Add(template, value);
		}
		Material val = ((value.Count > 0) ? value.Pop() : Create(template));
		Reset(val, template);
		inUse.Add(val, template);
		return val;
	}

	public void Release(Material instance)
	{
		if (!((Object)(object)instance == (Object)null) && inUse.TryGetValue(instance, out var value))
		{
			inUse.Remove(instance);
			if ((Object)(object)value == (Object)null)
			{
				Destroy((Object)(object)instance);
				return;
			}
			Reset(instance, value);
			inPool[value].Push(instance);
		}
	}

	public void Dispose()
	{
		foreach (Stack<Material> value in inPool.Values)
		{
			foreach (Material item in value)
			{
				Destroy((Object)(object)item);
			}
		}
		foreach (Material key in inUse.Keys)
		{
			Destroy((Object)(object)key);
		}
		inPool.Clear();
		inUse.Clear();
	}

	private static Material Create(Material template)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		return new Material(template)
		{
			name = ((Object)template).name + " (RingSolve Pooled)",
			hideFlags = (HideFlags)52
		};
	}

	private static void Reset(Material instance, Material template)
	{
		instance.CopyPropertiesFromMaterial(template);
		instance.renderQueue = template.renderQueue;
	}

	private static void Destroy(Object target)
	{
		if (!(target == (Object)null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy(target);
			}
			else
			{
				Object.DestroyImmediate(target);
			}
		}
	}
}
