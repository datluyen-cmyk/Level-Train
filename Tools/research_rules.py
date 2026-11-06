from pathlib import Path
import brotli,zlib,lzma,lz4.frame
import json,dnfile,shutil
import struct
from dncil.cil.body import CilMethodBody
from dncil.cil.body.reader import CilMethodBodyReaderBytes

ROOT=Path(r'D:\RotateRings\Research\extracted\assets\yoo\DefaultPackage')
OUT=Path(r'D:\RotateRings\Research\rules')
OUT.mkdir(exist_ok=True)

def decompress_probe():
    p=next(ROOT.glob('*hot_hot*'))
    b=p.read_bytes()
    funcs={'brotli':brotli.decompress,'zlib':zlib.decompress,'gzip':lambda d:zlib.decompress(d,31),'lz4':lz4.frame.decompress,'lzma':lzma.decompress}
    for off in range(16):
        for key in [0,0xaa,0xff]:
            d=bytes(x^key for x in b[off:])
            for name,fn in funcs.items():
                try:
                    r=fn(d)
                    print(name,off,key,len(r),r[:80])
                    (OUT/(name+str(off)+str(key)+'.bin')).write_bytes(r)
                except Exception: pass

def parse_configs():
    meta=(OUT/'game_config_meta_bytes.bin').read_bytes()
    entries=[struct.unpack_from('<Biqii',meta,4+i*21) for i in range(struct.unpack_from('<i',meta)[0])]
    raw=next(ROOT.glob('*config_default*')).read_bytes()
    tables={}
    for i in range(34,55):
        a=entries[i]
        b=brotli.decompress(raw[a[2]:a[2]+a[4]])
        cur=0
        def integer():
            nonlocal cur
            n=struct.unpack_from('<i',b,cur)[0];cur+=4
            return n
        def string():
            nonlocal cur
            n=integer()
            if n==-1:return None
            if n<0:
                ln=~n; utf16len=integer();s=b[cur:cur+ln].decode('utf8');cur+=ln;return s
            s=b[cur:cur+n*2].decode('utf16le');cur+=n*2;return s
        rows=[]
        for j in range(integer()):
            key=integer(); size=b[cur];cur+=1
            assert size==3,(i,j,size)
            lev=integer();physics=string();core=string()
            rows.append({'key':key,'position':lev,'physics':physics,'core':core})
        assert cur==len(b)
        name=['0','1','10','11','12','13','14','15','16','17','18','19','2','20','3','4','5','6','7','8','9'][i-34]
        tables[name]=rows
    (OUT/'main_progressions.json').write_text(json.dumps({'tables':tables},indent=2),encoding='utf8')
    print('PROGRESSIONS', {k:len(v) for k,v in tables.items()})

if __name__=='__main__':
    for p in ROOT.glob('*.rawfile'):
        try:
            result=brotli.decompress(p.read_bytes())
            name=p.name.split('_assets_02_res_')[-1].rsplit('_',1)[0]
            (OUT/(name+('.dll' if result[:2]==b'MZ' else '.bin'))).write_bytes(result)
            print(name,len(result),result[:80])
        except Exception: pass
    parse_configs()
    p=dnfile.dnPE(str(OUT/'game_assembly_hot_hot_bytes.dll'))
    records=[]
    for t in p.net.mdtables.TypeDef:
        if any(x in str(t.TypeName).lower() for x in ['ring','level','collect','rotate','physic','library','config']):
            records.append({'namespace':str(t.TypeNamespace),'type':str(t.TypeName),'methods':[{'name':str(m.row.Name),'rva':m.row.Rva} for m in t.MethodList]})
    (OUT/'managed_members.json').write_text(json.dumps(records,indent=2))
    method_owners={m.row_index:str(t.TypeNamespace)+'.'+str(t.TypeName) for t in p.net.mdtables.TypeDef for m in t.MethodList}
    field_owners={f.row_index:str(t.TypeNamespace)+'.'+str(t.TypeName) for t in p.net.mdtables.TypeDef for f in t.FieldList}
    def resolve(v):
        try:
            n=v.value; table=n>>24; idx=(n&0xffffff)-1
            if table==0x70:return repr(str(p.net.user_strings.get(n&0xffffff)))
            tables={1:'TypeRef',2:'TypeDef',4:'Field',6:'MethodDef',10:'MemberRef',27:'TypeSpec',43:'MethodSpec'}
            r=getattr(p.net.mdtables,tables[table])[idx]
            if table==6:return method_owners[idx+1]+'::'+str(r.Name)
            if table==4:return field_owners[idx+1]+'::'+str(r.Name)
            if table==10:
                parent=r.Class.row
                return str(getattr(parent,'TypeNamespace',''))+'.'+str(getattr(parent,'TypeName',''))+'::'+str(r.Name)
            if table in (1,2):return str(r.TypeNamespace)+'.'+str(r.TypeName)
            if table==43:return resolve(r.Method)
            return str(v)
        except Exception:return str(v)
    text=[]
    for t in p.net.mdtables.TypeDef:
        name=str(t.TypeNamespace)+'.'+str(t.TypeName)
        text.append('TYPE '+name)
        for f in t.FieldList:text.append(' FIELD '+str(f.row.Name))
        for m in t.MethodList:
            text.append('\nMETHOD '+name+'::'+str(m.row.Name)+' RVA '+str(m.row.Rva))
            if not m.row.Rva:continue
            try:
                b=CilMethodBody(CilMethodBodyReaderBytes(p.get_data(m.row.Rva,100000)))
                for ins in b.instructions:
                    v=ins.operand
                    text.append(f' {ins.offset:04x} {ins.opcode.name:15} {resolve(v) if hasattr(v,"value") else v}')
            except Exception as e:text.append('ERROR '+str(e))
    (OUT/'hot_all.il').write_text('\n'.join(text),encoding='utf8')
    print('wrote IL',len(text))
