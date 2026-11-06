import pathlib,runpy,sys
root=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(root/'tools'))
sys.argv=[str(root/'tools/level_generator_gui.py'),'--config',str(root/'level_training.json'),'--output-root',str(root/'batches'),'--count','300']
runpy.run_path(sys.argv[0],run_name='__main__')
