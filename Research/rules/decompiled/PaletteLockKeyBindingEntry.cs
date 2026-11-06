using System;
using RotateRings.GamePlay.Services;

[Serializable]
public struct PaletteLockKeyBindingEntry
{
	public string keyAssemblyName;

	public string[] lockedAssemblyNames;

	public PaletteLockKeyColor keyColor;
}
