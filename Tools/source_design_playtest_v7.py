"""Drive and inspect the live original-board native-gesture research harness."""
from pathlib import Path
import argparse
import json
import math
from unity_mcp import McpClient, DEFAULT_URL
from run_native_virtual_playtest_v5 import execute


def compact(value):
    if not isinstance(value, dict):
        return value
    if 'outcome' in value:
        fields = ('index', 'partId', 'action', 'requestedAmount', 'reachedBefore', 'reachedAfter',
                  'accepted', 'outcome', 'rejectionReason', 'blockerOwner', 'blockerRole', 'removedDelta', 'won')
        result = {k:value.get(k) for k in fields}
        for phase in ('before', 'after'):
            obs = value.get(phase) or {}
            result[phase] = {k:obs.get(k) for k in ('remainingParts', 'moves', 'activeAuthoredBindingCount')}
        return result
    return {k:value.get(k) for k in ('levelId', 'position', 'remainingParts', 'moves', 'activeAuthoredBindingCount', 'won')} | {
        'poses':[{k:p.get(k) for k in ('ownerId', 'kind', 'x', 'y', 'angle', 'slide', 'gap', 'gapDirectionDeg', 'locked', 'hasActiveOutgoing')}
                 for p in value.get('poses', []) if not p.get('removed')]}


def call(client, operation, **kwargs):
    q = lambda value:json.dumps(str(value))
    if operation == 'load':
        workspace = Path(kwargs['workspace']).resolve()
        code = 'return SourceDesignPlaytestV7.Load('+','.join((str(kwargs['id']), q(workspace/'data/source500_native.json'),
            q(workspace/'data/source_templates.json'),q(Path(kwargs['report']).resolve()),q(kwargs.get('label','source research'))))+');'
    elif operation == 'step':
        amount = float(kwargs['amount'])
        if not math.isfinite(amount):
            raise ValueError('Finite native gesture amount required')
        code = 'return SourceDesignPlaytestV7.Step('+str(int(kwargs['part']))+','+q(kwargs['kind'])+','+format(amount,'.9g')+'f);'
    else:
        method = {'observe':'Observe', 'undo':'Undo', 'restart':'Restart'}[operation]
        code = 'return SourceDesignPlaytestV7.'+method+'();'
    value = execute(client, code)
    if isinstance(value,str):
        try:return json.loads(value)
        except json.JSONDecodeError:return value
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('operation',choices=('load','observe','step','undo','restart'))
    parser.add_argument('--workspace',default=str(Path(__file__).resolve().parents[1]/'RotateRings Test/LevelTraining_SourceLayoutV3/runs/ordinary_v4'))
    parser.add_argument('--id',type=int)
    parser.add_argument('--report')
    parser.add_argument('--label',default='source research')
    parser.add_argument('--part',type=int)
    parser.add_argument('--kind',choices=('rotate','slide'),default='rotate')
    parser.add_argument('--amount',type=float)
    args = vars(parser.parse_args())
    value = call(McpClient(DEFAULT_URL), **args)
    print(json.dumps(compact(value),ensure_ascii=False,indent=2))


if __name__ == '__main__':
    main()
