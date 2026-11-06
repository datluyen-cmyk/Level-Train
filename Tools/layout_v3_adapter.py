"""V3 workspace adapter with an explicit native-parity generation gate."""
import hashlib,json
def default_settings():return {'nativeParityRequired':True,'generationEnabled':False,'phase':'composition model evaluation; native physics parity pending','acceptance':{'requestedCount':300,'sourceCountMinimum':500,'canonicalDuplicatesAllowed':False,'nativeUnmodeledMechanics':'retain source, withhold labels and generation'},'policies':[{'strategy':'native_contract','maxSeconds':30}]}
def validate_level(l):return [] if l.get('state',{}).get('nativeSourceLevelId') else ['missing native source index']
def fingerprint(l):raise RuntimeError('Native whole-layout canonicalization gate pending; source index IDs are not novelty fingerprints')
def features(l):return {'sourcePosition':l['state']['position']}
def solve(l,policy=None):return {'solved':False,'solution':[],'cost':0,'effort':0,'reason':'Native mixed-shape source replay unverified. This record is a source index, not a solved game label.'}
def replay(l,solution):return False
def difficulty(l,result):raise RuntimeError('No verified native source difficulty labels yet')
def fit_proposal(levels,seed):raise RuntimeError('Use tools/layout_learning_v3.py for composition-only training. Native generation gate remains closed.')
def propose(reference,rng,proposal_model):raise RuntimeError('Generation blocked until native physics and original-source parity gates pass')
def novelty_distance(a,b):raise RuntimeError('Native layout near-copy validation pending; no generation enabled')
