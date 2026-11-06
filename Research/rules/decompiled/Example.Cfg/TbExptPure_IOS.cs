using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using MemoryPack;
using MemoryPack.Compression;
using UnityEngine;
using XingYun.Cfg;
using XingYun.Config;

namespace Example.Cfg;

public class TbExptPure_IOS : ConfigVoBase
{
	private readonly Dictionary<string, ExptPure> _dic;

	private List<ExptPure> _dataList;

	private int _tableRowCount;

	private FileStream _fileStream;

	private BrotliDecompressor _dcp;

	public int TableRowCount => _tableRowCount;

	public List<ExptPure> DataList
	{
		get
		{
			if (_dataList == null && _tableRowCount > 0)
			{
				_dataList = new List<ExptPure>(_tableRowCount);
				foreach (ExptPure value in _dic.Values)
				{
					_dataList.Add(value);
				}
			}
			else if (_dataList == null && _tableRowCount == 0)
			{
				_dataList = new List<ExptPure>(0);
			}
			return _dataList;
		}
	}

	public Dictionary<string, ExptPure> DataMap => _dic;

	public ExptPure this[string key] => _dic[key];

	public TbExptPure_IOS(TableInfo tableInfo, ref FileStream contentFileStream, ref BrotliDecompressor dcp)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_tableRowCount = -1;
		if (contentFileStream != null)
		{
			if (tableInfo.rowCount != 0)
			{
				_dcp = dcp;
				_tableRowCount = tableInfo.rowCount;
				_fileStream = contentFileStream;
				byte[] array = ArrayPool<byte>.Shared.Rent(tableInfo.contentLength);
				try
				{
					_fileStream.Seek(tableInfo.contentBegin, SeekOrigin.Begin);
					int num = _fileStream.Read(array, 0, tableInfo.contentLength);
					if (num == tableInfo.contentLength)
					{
						ReadOnlySequence<byte> readOnlySequence = ((BrotliDecompressor)(ref _dcp)).Decompress((ReadOnlySpan<byte>)array);
						_dic = MemoryPackSerializer.Deserialize<Dictionary<string, ExptPure>>(ref readOnlySequence, (MemoryPackSerializerOptions)null);
						((BrotliDecompressor)(ref _dcp)).Dispose();
					}
					else
					{
						Debug.LogError((object)("TbExptPure_IOS contentDataLength " + tableInfo.contentLength + " length " + num));
					}
					return;
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(array);
				}
			}
			_tableRowCount = 0;
		}
		else
		{
			Debug.LogError((object)"contentFileStream = null TbExptPure_IOS contentDataLength ");
		}
	}

	public ExptPure Get(string key)
	{
		if (_dic.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public ExptPure GetOrDefault(string key)
	{
		return Get(key);
	}

	public void DisposeLogic()
	{
		if (_dataList != null)
		{
			_dataList.Clear();
		}
		_dataList = null;
	}
}
