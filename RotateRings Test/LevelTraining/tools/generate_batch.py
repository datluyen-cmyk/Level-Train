"""Saved-model generator entry point used by the bundled Tk interface."""
import argparse,contextlib,pathlib,re,sys
from train_reconstruction import generate
class Progress:
    def __init__(self):self.buffer=''
    def write(self,text):
        self.buffer+=text
        while '\n' in self.buffer:
            line,self.buffer=self.buffer.split('\n',1)
            match=re.match(r'^(\d+) (PASS|INCOMPLETE) (\d+) / (\d+)$',line)
            if match:line=f'{match[1]}/{match[4]}: {match[2]} ({match[3]} accepted)'
            sys.__stdout__.write(line+'\n');sys.__stdout__.flush()
        return len(text)
    def flush(self):sys.__stdout__.flush()
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--config',required=True);p.add_argument('--output',required=True);p.add_argument('--count',type=int,default=300);p.add_argument('--seed',type=int,default=42);p.add_argument('--exclude',action='append',default=[]);args=p.parse_args()
    first=pathlib.Path(args.config).resolve().parent/'batches/final-300/generated_levels.json'
    exclude=list(args.exclude)
    if first.exists() and str(first) not in exclude:exclude.append(str(first))
    with contextlib.redirect_stdout(Progress()):code=generate(args.config,args.output,args.count,args.seed,exclude)
    raise SystemExit(code)
