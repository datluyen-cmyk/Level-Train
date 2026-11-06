from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[1]/'RotateRings Test/LevelTraining'
tools=root/'tools'
(tools/'generate_batch.py').write_text('''"""Saved-model generator entry point used by the bundled Tk interface."""
import argparse,contextlib,pathlib,re,sys
from train_reconstruction import generate
class Progress:
    def __init__(self):self.buffer=''
    def write(self,text):
        self.buffer+=text
        while '\\n' in self.buffer:
            line,self.buffer=self.buffer.split('\\n',1)
            match=re.match(r'^(\\d+) (PASS|INCOMPLETE) (\\d+) / (\\d+)$',line)
            if match:line=f'{match[1]}/{match[4]}: {match[2]} ({match[3]} accepted)'
            sys.__stdout__.write(line+'\\n');sys.__stdout__.flush()
        return len(text)
    def flush(self):sys.__stdout__.flush()
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--config',required=True);p.add_argument('--output',required=True);p.add_argument('--count',type=int,default=300);p.add_argument('--seed',type=int,default=42);p.add_argument('--exclude',action='append',default=[]);args=p.parse_args()
    first=pathlib.Path(args.config).resolve().parent/'batches/final-300/generated_levels.json'
    exclude=list(args.exclude)
    if first.exists() and str(first) not in exclude:exclude.append(str(first))
    with contextlib.redirect_stdout(Progress()):code=generate(args.config,args.output,args.count,args.seed,exclude)
    raise SystemExit(code)
''',encoding='utf8')
(tools/'level_training.py').write_text('''"""Frozen game-specific training entry point; see train_reconstruction.py."""
import argparse,datetime,pathlib
from train_reconstruction import fit,generate
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--config',required=True);p.add_argument('--output');p.add_argument('--count',type=int,default=300);p.add_argument('--train-only',action='store_true');args=p.parse_args()
    fit(args.config)
    if not args.train_only:
        output=args.output or str(pathlib.Path(args.config).resolve().parent/'batches'/('retrain-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S')))
        raise SystemExit(generate(args.config,output,args.count))
''',encoding='utf8')
p=tools/'level_generator_gui.py';s=p.read_text(encoding='utf8');s=s.replace("report['complete']","report.get('complete',report.get('passed',False))");p.write_text(s,encoding='utf8')
p=root/'Open Generator.pyw';s=p.read_text(encoding='utf8');s=s.replace("str(root/'batches')]","str(root/'batches'),'--count','300']");p.write_text(s,encoding='utf8')
p=root/'Open Generator.vbs';s=p.read_text(encoding='utf-16');s=s.replace('& q & output & q, 1, False','& q & output & q & " --count 300", 1, False');p.write_text(s,encoding='utf-16')
print('Packaged saved-model generator')
