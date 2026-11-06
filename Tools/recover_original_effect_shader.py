"""Read/export original GLES3 particle programs that UnityPy's default export omits.

No Unity project files are changed. The first four Unity 6000 table entries are
parameter records; the last four contain complete combined VERTEX/FRAGMENT GLSL.
"""
from pathlib import Path
import hashlib,json,struct
import UnityPy
from UnityPy.helpers import CompressionHelper
from UnityPy.streams import EndianBinaryReader
from UnityPy.export.ShaderConverter import ShaderSubProgram

ROOT=Path(__file__).resolve().parents[1]
BUNDLES=ROOT/'Research/extracted/assets/yoo/DefaultPackage'
OUT=ROOT/'Research/shaders'
def main():
    OUT.mkdir(exist_ok=True)
    bundle=next(BUNDLES.glob('*prefab_maingameplay_*'))
    env=UnityPy.load(str(bundle));shader=next(o.read() for o in env.objects if o.path_id==838372116047587976)
    blob=bytes(shader.compressedBlob);programs=[]
    for platform,offsets,compressed,decompressed in zip(shader.platforms,shader.offsets,shader.compressedLengths,shader.decompressedLengths):
        for chunk,(offset,csize,dsize) in enumerate(zip(offsets,compressed,decompressed)):
            raw=CompressionHelper.decompress_lz4(blob[offset:offset+csize],dsize)
            (OUT/f'HsAppVFXAllEffect_platform{platform}_{chunk}.bin').write_bytes(raw)
            if platform!=9:continue
            count=struct.unpack_from('<I',raw)[0]
            for i in range(count):
                entry_offset,entry_size,entry_segment=struct.unpack_from('<III',raw,4+i*12)
                if i<4:continue
                reader=EndianBinaryReader(raw,endian='<');reader.Position=entry_offset
                sub=ShaderSubProgram(reader)
                code=sub.m_ProgramCode.decode('utf8')
                assert code.startswith('#ifdef VERTEX') and '#ifdef FRAGMENT' in code
                target=OUT/f'HsAppVFXAllEffect_GLES3_program{i}.glsl';target.write_bytes(sub.m_ProgramCode)
                programs.append({'index':i,'keywords':sub.m_Keywords,'entryOffset':entry_offset,'entrySize':entry_size,'entrySegment':entry_segment,'bytes':len(sub.m_ProgramCode),'file':target.name,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
    report={'bundle':bundle.name,'bundleSha256':hashlib.sha256(bundle.read_bytes()).hexdigest(),'shaderPathId':838372116047587976,'shaderName':'HsApp/Particles/HsAppVFXAllEffect','method':'UnityPy lz4 decompress platform9; little-endian ShaderSubProgram table entries4..7; no invented shader code','programs':programs,'activeProgram':4,'limits':'GLSL is original compiler output, not authored HLSL source. Direct ShaderLab cross-platform loading needs a semantic port. Platform18 decompressed data retained but SPIRV not decompiled here.'}
    (OUT/'effect_shader_recovery_manifest.json').write_text(json.dumps(report,indent=2))
    print(json.dumps(report,indent=2))
if __name__=='__main__':main()
