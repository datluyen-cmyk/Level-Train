"""Exercise the actual saved-model Tk GUI with labeled, isolated QA batches.

Does not alter models or the delivered final pack. A cancelled run must retain
its last completed partial checkpoint. The success run excludes the final pack
through the real wrapper's normal default behavior.
"""
from pathlib import Path
import importlib.util,json,time,tkinter as tk,hashlib,traceback

ROOT=Path(__file__).resolve().parents[1]
BUNDLE=ROOT/'RotateRings Test/LevelTraining'
OUT=ROOT/'Research/generator_gui_qa.json'

def save(report):OUT.write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    report={'testKind':'actual Tkinter app methods with real saved-model subprocess; window withdrawn',
      'bundle':str(BUNDLE),'finalPackExcludedByDefault':True,'tests':{},'passed':False}
    final=BUNDLE/'batches/final-300/generated_levels.json'
    if not final.exists():
        report['status']='waiting for completed final pack';save(report)
        raise SystemExit('Final pack is not ready; re-run after final generation')
    final_hash=sha(final);report['finalPackSha256']=final_hash
    spec=importlib.util.spec_from_file_location('actual_generator_gui',BUNDLE/'tools/level_generator_gui.py')
    gui=importlib.util.module_from_spec(spec);spec.loader.exec_module(gui)
    window=tk.Tk();window.withdraw()
    errors=[]
    gui.messagebox.showerror=lambda title,message:errors.append({'title':title,'message':message})
    app=gui.GeneratorApp(window,str(BUNDLE/'level_training.json'),str(BUNDLE/'batches/gui-qa'))
    report['tests']['config']={'info':app.model_info.get(),'title':window.title(),'modelsRecognized':'Đã có mô hình' in app.model_info.get()}
    def pump_until(condition,timeout):
        start=time.monotonic()
        while time.monotonic()-start<timeout:
            window.update()
            if condition():return True
            time.sleep(.025)
        return False
    app.count.set('300');app.seed.set('118');app.start()
    started=app.process is not None
    checkpoint_ready=pump_until(lambda:bool(app.last_output and (app.last_output/'partial_levels.json').exists()),60)
    output=app.last_output
    app.cancel();stopped=pump_until(lambda:app.process is None,15)
    partial=json.loads((output/'partial_levels.json').read_text()) if checkpoint_ready else None
    report['tests']['cancellation']={'started':started,'checkpointWritten':checkpoint_ready,'stopped':stopped,
      'cancelledFlag':app.cancelled,'status':app.status.get(),'output':str(output),
      'retainedAcceptedCount':len(partial['levels']) if partial else 0,
      'partialStillExists':bool(output and (output/'partial_levels.json').exists()),
      'controlsRestored':str(app.start_button.cget('state'))=='normal'}
    print('cancellation',json.dumps(report['tests']['cancellation'],ensure_ascii=True),flush=True)
    app.count.set('1');app.seed.set('117');app.start()
    finished=pump_until(lambda:app.process is None,120)
    if not finished:
        app.cancel()
        terminated=pump_until(lambda:app.process is None,15)
        if not terminated:
            app.process.kill()
            app.process.wait(timeout=5)
            pump_until(lambda:app.process is None,5)
    output=app.last_output;path=output/'last_run_report.json'
    result=json.loads(path.read_text()) if path.exists() else None
    report['tests']['generation']={'finished':finished,'output':str(output),'status':app.status.get(),
      'reportPresent':result is not None,'requestedCount':result.get('requestedCount') if result else None,
      'generatedCount':result.get('generatedCount') if result else None,'passed':result.get('passed') if result else False,
      'packPresent':(output/'generated_levels.json').exists(),'progressValue':float(app.progress['value']),
      'openResultsEnabled':str(app.open_button.cget('state'))=='normal','errors':errors}
    # Exercise INCOMPLETE presentation with a labeled report fixture; generation
    # above remains a real model run. This verifies report fallback separately.
    fixture=BUNDLE/'batches/gui-qa/incomplete-status-fixture';fixture.mkdir(parents=True,exist_ok=True)
    (fixture/'last_run_report.json').write_text(json.dumps({'passed':False,'requestedCount':2,'generatedCount':1}))
    app.last_output=fixture;app.cancelled=False;app.finish(2)
    report['tests']['incompletePresentation']={'fixtureOnly':True,'status':app.status.get(),
      'correctlyReportsPartial':'Chưa đủ số lượng: 1/2' in app.status.get()}
    report['finalPackUnchanged']=sha(final)==final_hash
    report['passed']=all([report['tests']['config']['modelsRecognized'],started,checkpoint_ready,stopped,
      report['tests']['cancellation']['partialStillExists'],finished,bool(result and result.get('passed')),
      report['tests']['generation']['packPresent'],report['tests']['incompletePresentation']['correctlyReportsPartial'],
      report['finalPackUnchanged'],not errors])
    report['status']='complete';save(report);window.destroy()
    print(json.dumps(report,ensure_ascii=True),flush=True)
    if not report['passed']:raise SystemExit(2)

if __name__=='__main__':
    try:main()
    except Exception as exc:
        save({'passed':False,'error':str(exc),'traceback':traceback.format_exc()});raise
