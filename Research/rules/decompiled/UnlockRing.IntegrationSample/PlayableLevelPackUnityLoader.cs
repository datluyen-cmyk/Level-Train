using System;
using System.Collections.Generic;
using System.IO;
using UnlockRing.Core.Runtime;
using UnlockRing.Serialization;

namespace UnlockRing.IntegrationSample;

public sealed class PlayableLevelPackUnityLoader
{
	private readonly PlayableLevelPack _pack;

	public IReadOnlyList<PlayableLevelPackIndexEntry> Levels => _pack.Levels;

	public PlayableLevelPackUnityLoader(byte[] trustedPackBytes)
	{
		_pack = PlayableLevelPackReader.Read(trustedPackBytes ?? throw new ArgumentNullException("trustedPackBytes"));
	}

	public static PlayableLevelPackUnityLoader FromTrustedFile(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentException("Path must not be empty.", "path");
		}
		return new PlayableLevelPackUnityLoader(File.ReadAllBytes(path));
	}

	public LoadedPlayableLevel Load(string levelId)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		PlayableLevelPackLevel val = _pack.LoadLevel(levelId);
		return new LoadedPlayableLevel(new GameSession(val.CertifiedInitialState), val);
	}
}
