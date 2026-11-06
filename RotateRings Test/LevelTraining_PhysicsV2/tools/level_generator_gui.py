"""Local desktop UI for the saved-model batch generator (Python + Tkinter)."""
from __future__ import annotations
import argparse
import datetime
import json
import os
import pathlib
import queue
import re
import subprocess
import sys
import threading
import tkinter as tk
from tkinter import filedialog, messagebox, ttk


class GeneratorApp:
    def __init__(self, window, config='', output_root=''):
        self.window = window
        self.process = None
        self.events = queue.Queue()
        self.cancelled = False
        self.last_output = None
        self.config = tk.StringVar(value=config)
        self.output = tk.StringVar(value=output_root or str(pathlib.Path.home() / 'Level Generator Output'))
        self.count = tk.StringVar(value='100')
        self.seed = tk.StringVar(value='42')
        self.status = tk.StringVar(value='Chọn cấu hình của game đã train để bắt đầu.')
        self.model_info = tk.StringVar(value='Chưa chọn mô hình')
        self.exclusions = []
        self.controls = []
        window.title('Level Generator • Sinh level hàng loạt')
        window.geometry('900x770')
        window.minsize(760, 680)
        window.configure(bg='#f2f5f9')
        style = ttk.Style()
        if 'clam' in style.theme_names():
            style.theme_use('clam')
        style.configure('TFrame', background='#f2f5f9')
        style.configure('TLabel', background='#f2f5f9', font=('Segoe UI', 10))
        style.configure('Title.TLabel', font=('Segoe UI', 23, 'bold'), foreground='#18243b')
        style.configure('Sub.TLabel', foreground='#526078')
        style.configure('TButton', font=('Segoe UI', 10), padding=(12, 7))
        style.configure('Accent.TButton', foreground='white', background='#2866cf', font=('Segoe UI', 11, 'bold'))
        frame = ttk.Frame(window, padding=26)
        frame.pack(fill='both', expand=True)
        frame.columnconfigure(0, weight=1)
        ttk.Label(frame, text='Level Generator', style='Title.TLabel').grid(row=0, column=0, sticky='w')
        ttk.Label(frame, text='Nạp mô hình đã train · Sinh theo độ khó tham chiếu · Kiểm tra độ mới', style='Sub.TLabel').grid(row=1, column=0, sticky='w', pady=(3, 20))
        self.path_row(frame, 2, '1. Cấu hình game đã train', self.config, self.choose_config)
        ttk.Label(frame, textvariable=self.model_info, style='Sub.TLabel', wraplength=800).grid(row=3, column=0, sticky='w', pady=(4, 15))
        self.path_row(frame, 4, '2. Thư mục chứa các đợt sinh', self.output, self.choose_output)
        ttk.Label(frame, text='Mỗi lần chạy tự tạo một thư mục mới, không ghi đè bộ cũ.', style='Sub.TLabel').grid(row=5, column=0, sticky='w', pady=(4, 15))
        settings = ttk.Frame(frame)
        settings.grid(row=6, column=0, sticky='ew', pady=(0, 16))
        for column, (label, variable) in enumerate([('Số level', self.count), ('Seed', self.seed)]):
            ttk.Label(settings, text=label).grid(row=0, column=column * 2, padx=(0, 8))
            entry = ttk.Entry(settings, textvariable=variable, width=12)
            entry.grid(row=0, column=column * 2 + 1, padx=(0, 25))
            self.controls.append(entry)
        ttk.Label(settings, text='Đổi seed để tạo đợt mới.', style='Sub.TLabel').grid(row=0, column=4)
        exclusion_header = ttk.Frame(frame)
        exclusion_header.grid(row=7, column=0, sticky='ew')
        ttk.Label(exclusion_header, text='3. Bộ level cũ cần tránh trùng (tùy chọn)').pack(side='left')
        for label, callback in [('Thêm JSON', self.add_exclusions), ('Bỏ chọn', self.remove_exclusions)]:
            button = ttk.Button(exclusion_header, text=label, command=callback)
            button.pack(side='right', padx=(6, 0))
            self.controls.append(button)
        self.exclusion_list = tk.Listbox(frame, height=3, selectmode='extended', font=('Segoe UI', 9), relief='solid', bd=1)
        self.exclusion_list.grid(row=8, column=0, sticky='ew', pady=(7, 15))
        actions = ttk.Frame(frame)
        actions.grid(row=9, column=0, sticky='ew')
        self.start_button = ttk.Button(actions, text='Sinh level', style='Accent.TButton', command=self.start)
        self.start_button.pack(side='left')
        self.cancel_button = ttk.Button(actions, text='Dừng', command=self.cancel, state='disabled')
        self.cancel_button.pack(side='left', padx=8)
        self.open_button = ttk.Button(actions, text='Mở kết quả', command=self.open_output, state='disabled')
        self.open_button.pack(side='right')
        self.progress = ttk.Progressbar(frame, mode='determinate')
        self.progress.grid(row=10, column=0, sticky='ew', pady=(16, 7))
        ttk.Label(frame, textvariable=self.status, wraplength=800).grid(row=11, column=0, sticky='w')
        log_frame = ttk.Frame(frame)
        log_frame.grid(row=12, column=0, sticky='nsew', pady=(12, 0))
        frame.rowconfigure(12, weight=1)
        self.log = tk.Text(log_frame, height=8, font=('Consolas', 10), bg='#142037', fg='#dae7fa', relief='flat', state='disabled', wrap='word')
        scroll = ttk.Scrollbar(log_frame, command=self.log.yview)
        self.log.configure(yscrollcommand=scroll.set)
        scroll.pack(side='right', fill='y')
        self.log.pack(fill='both', expand=True)
        self.config.trace_add('write', lambda *_: self.inspect_config())
        self.inspect_config()
        window.protocol('WM_DELETE_WINDOW', self.close)
        window.after(100, self.poll)

    def path_row(self, parent, row, label, variable, callback):
        box = ttk.Frame(parent)
        box.grid(row=row, column=0, sticky='ew')
        box.columnconfigure(0, weight=1)
        ttk.Label(box, text=label).grid(row=0, column=0, sticky='w', pady=(0, 6))
        entry = ttk.Entry(box, textvariable=variable)
        entry.grid(row=1, column=0, sticky='ew')
        button = ttk.Button(box, text='Chọn…', command=callback)
        button.grid(row=1, column=1, padx=(8, 0))
        self.controls.extend([entry, button])

    def choose_config(self):
        path = filedialog.askopenfilename(title='Chọn level_training.json', filetypes=[('Cấu hình JSON', '*.json')])
        if path:
            self.config.set(path)

    def choose_output(self):
        path = filedialog.askdirectory(title='Chọn thư mục chứa các đợt sinh')
        if path:
            self.output.set(path)

    def inspect_config(self):
        try:
            path = pathlib.Path(self.config.get().strip())
            config = json.loads(path.read_text(encoding='utf8'))
            refs = json.loads((path.parent / config['references']).read_text(encoding='utf-8-sig'))['levels']
            ready = all((path.parent / 'models' / name).is_file() for name in ['manifest.json', 'difficulty.joblib', 'proposal.joblib'])
            display=config.get('gameName',path.parent.name)
            workspace=config.get('workspaceName',path.parent.name)
            self.window.title(f'Level Generator • {display}')
            self.model_info.set(f"{display} / {workspace} · {len(refs)} level tham chiếu · " + ('Đã có mô hình; sẽ kiểm tra tính toàn vẹn khi chạy.' if ready else 'Thiếu mô hình/manifest: cần train bằng phiên bản skill hiện tại.'))
        except (OSError, ValueError, KeyError, TypeError):
            self.model_info.set('Chọn file level_training.json trong workspace đã train trên máy.')

    def add_exclusions(self):
        for path in filedialog.askopenfilenames(title='Chọn các bộ level cũ', filetypes=[('Level JSON', '*.json')]):
            if path not in self.exclusions:
                self.exclusions.append(path)
                self.exclusion_list.insert('end', path)

    def remove_exclusions(self):
        for index in reversed(self.exclusion_list.curselection()):
            self.exclusions.pop(index)
            self.exclusion_list.delete(index)

    def append_log(self, text):
        self.log.configure(state='normal')
        self.log.insert('end', text + '\n')
        self.log.see('end')
        self.log.configure(state='disabled')

    def start(self):
        if self.process is not None:
            return
        try:
            count, seed = int(self.count.get()), int(self.seed.get())
            if count < 1:
                raise ValueError('Số level phải lớn hơn 0.')
            config = pathlib.Path(self.config.get().strip()).resolve()
            if not config.is_file():
                raise ValueError('Hãy chọn file cấu hình game đã train.')
            if not self.output.get().strip():
                raise ValueError('Hãy chọn thư mục lưu.')
            output = pathlib.Path(self.output.get().strip()).resolve() / ('batch-' + datetime.datetime.now().strftime('%Y%m%d-%H%M%S-%f'))
            args = [sys.executable, '-u', str(pathlib.Path(__file__).with_name('generate_batch.py')), '--config', str(config), '--output', str(output), '--count', str(count), '--seed', str(seed)]
            for path in self.exclusions:
                args.extend(['--exclude', path])
            env = {**os.environ, 'PYTHONIOENCODING':'utf-8', 'PYTHONUNBUFFERED':'1'}
            self.process = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding='utf8', errors='replace', env=env, creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
        except (OSError, ValueError) as error:
            messagebox.showerror('Chưa thể sinh level', str(error))
            return
        self.cancelled = False
        self.last_output = output
        self.progress.configure(maximum=count, value=0)
        self.status.set('Đang nạp mô hình và kiểm tra dữ liệu…')
        self.start_button.configure(state='disabled')
        self.open_button.configure(state='disabled')
        self.cancel_button.configure(state='normal')
        for control in self.controls:
            control.configure(state='disabled')
        self.append_log(f'\nBẮT ĐẦU · {count} level · seed {seed}\nLưu tại: {output}')
        threading.Thread(target=self.read_process, args=(self.process,), daemon=True).start()

    def read_process(self, process):
        try:
            for line in process.stdout:
                self.events.put(('line', line.rstrip()))
            self.events.put(('done', process.wait()))
        except Exception as error:
            self.events.put(('line', str(error)))
            self.events.put(('done', process.wait()))

    def poll(self):
        try:
            while True:
                kind, value = self.events.get_nowait()
                if kind == 'line':
                    self.append_log(value)
                    match = re.match(r'^(\d+)/(\d+): (PASS|INCOMPLETE)', value)
                    if match:
                        self.progress['value'] = int(match[1])
                        self.status.set(f'Đã kiểm tra {match[1]}/{match[2]} vị trí. ' + ('Đạt.' if match[3] == 'PASS' else 'Chưa tìm được level đạt tiêu chí.'))
                else:
                    self.finish(value)
        except queue.Empty:
            pass
        self.window.after(100, self.poll)

    def finish(self, code):
        self.process = None
        self.start_button.configure(state='normal')
        self.cancel_button.configure(state='disabled')
        for control in self.controls:
            control.configure(state='normal')
        if self.last_output and self.last_output.is_dir():
            self.open_button.configure(state='normal')
        if self.cancelled:
            self.status.set('Đã dừng. Các level đã lưu nằm trong partial_levels.json nếu có.')
        elif code in (0, 2):
            try:
                report = json.loads((self.last_output / 'validation_report.json').read_text(encoding='utf8'))
                self.status.set(f"{'Hoàn tất' if report['complete'] else 'Chưa đủ số lượng'}: {report['generatedCount']}/{report['requestedCount']} level đạt. " + ('Mở kết quả để lấy JSON.' if report['complete'] else 'Đã giữ phần đạt; xem báo cáo lý do loại.'))
            except (OSError, ValueError, KeyError):
                self.status.set('Không đọc được báo cáo. Xem nhật ký bên dưới.')
        else:
            self.status.set('Không thể sinh level. Xem lỗi trong nhật ký bên dưới.')

    def cancel(self):
        if self.process and self.process.poll() is None:
            self.cancelled = True
            self.process.terminate()
            self.cancel_button.configure(state='disabled')
            self.status.set('Đang dừng…')

    def open_output(self):
        if self.last_output and self.last_output.is_dir():
            if os.name == 'nt':
                os.startfile(str(self.last_output))
            elif sys.platform == 'darwin':
                subprocess.Popen(['open', str(self.last_output)])
            else:
                subprocess.Popen(['xdg-open', str(self.last_output)])

    def close(self):
        if self.process and self.process.poll() is None:
            if not messagebox.askyesno('Đang sinh level', 'Dừng lượt sinh hiện tại và đóng giao diện?'):
                return
            self.process.terminate()
        self.window.destroy()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--config', default='')
    parser.add_argument('--output-root', default='')
    parser.add_argument('--count', type=int, default=100)
    parser.add_argument('--seed', type=int, default=42)
    args = parser.parse_args()
    window = tk.Tk()
    app = GeneratorApp(window, args.config, args.output_root)
    app.count.set(str(args.count))
    app.seed.set(str(args.seed))
    window.mainloop()


if __name__ == '__main__':
    main()
