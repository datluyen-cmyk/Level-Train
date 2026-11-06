using System;

namespace XingYun.Track.Events;

[Serializable]
public class GDeviceInfo : BaseTrackConfig
{
	public const string EventName = "g_device_info";

	public string gpu_name;

	public string gpu_vendor;

	public string gpu_api;

	public int gpu_shader_level;

	public int gpu_memory_mb;

	public string gpu_version;

	public string render_threading_mode;

	public int supports_multithreaded_rendering;

	public string npot_support;

	public int supports_async_gpu_readback;

	public int supports_compute_shaders;

	public int supports_instancing;

	public int supports_mip_streaming;

	public int uses_reversed_z_buffer;

	public int max_texture_size;

	public string copy_texture_support;

	public string game_key;

	public int device_level;

	public int supports_astc;

	public int supports_etc;

	public int supports_texture_array;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		gpu_name = null;
		gpu_vendor = null;
		gpu_api = null;
		gpu_shader_level = 0;
		gpu_memory_mb = 0;
		gpu_version = null;
		render_threading_mode = null;
		supports_multithreaded_rendering = 0;
		npot_support = null;
		supports_async_gpu_readback = 0;
		supports_compute_shaders = 0;
		supports_instancing = 0;
		supports_mip_streaming = 0;
		uses_reversed_z_buffer = 0;
		max_texture_size = 0;
		copy_texture_support = null;
		game_key = null;
		device_level = 0;
		supports_astc = 0;
		supports_etc = 0;
		supports_texture_array = 0;
	}
}
