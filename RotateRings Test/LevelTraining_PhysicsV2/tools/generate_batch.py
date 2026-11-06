import argparse
from train_native_physics import generate
if __name__=="__main__":
 p=argparse.ArgumentParser();p.add_argument("--config",required=True);p.add_argument("--output",required=True);p.add_argument("--count",type=int,default=300);p.add_argument("--seed",type=int,default=20261105);p.add_argument("--exclude",action="append",default=[]);a=p.parse_args();raise SystemExit(generate(a.config,a.output,a.count,a.seed,a.exclude))
