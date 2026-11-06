"""Frozen game-specific training entry point; see train_reconstruction.py."""
import argparse,datetime,pathlib
from train_reconstruction import fit,generate
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--config',required=True);p.add_argument('--output');p.add_argument('--count',type=int,default=300);p.add_argument('--train-only',action='store_true');args=p.parse_args()
    fit(args.config)
    if not args.train_only:
        output=args.output or str(pathlib.Path(args.config).resolve().parent/'batches'/('retrain-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S')))
        raise SystemExit(generate(args.config,output,args.count))
