import sys
from PySide2.QtWidgets import QApplication, QMainWindow, QPushButton, QDoubleSpinBox
from PySide2.QtCore import Qt, Signal, Slot, QThread
from UIView import Ui_UIView
import vgamepad as vg
import json
import time
from pynput.keyboard import Key, KeyCode
from pynput.keyboard import Listener, Controller
import os
from PIL import ImageGrab
from cv2 import VideoWriter, VideoWriter_fourcc, cvtColor, COLOR_RGB2BGR
import numpy as np
import pyaudiowpatch as pyaudio
import wave


class UIFunc(QMainWindow, Ui_UIView):
    key_press_signal = Signal(Key)
    key_release_signal = Signal(Key)
    keymap = {
        'Pick': Key.space,
        'Dash': KeyCode.from_char('.'),
        'Use': KeyCode.from_char('/'),
        'Emote': KeyCode.from_char('c')
    }
    replay_file_path = 'D:\\Steam\\steamapps\\common\\Overcooked! 2\\Mods\\replay.json'
    author = 'GUA'
    q_coord = [
        (-1.0, 0.5),  # q
        (0.5, 1.0)    # e
    ]

    def __init__(self, app):
        super(UIFunc, self).__init__()
        self.app = app
        self.setupUi(self)
        self.setWindowFlags(Qt.WindowStaysOnTopHint | Qt.FramelessWindowHint)
        self.label.setStyleSheet('background-color: rgb(135,206,235)')
        self.label_3.setStyleSheet('background-color: rgb(255,99,71)')
        self.label_2.setStyleSheet('background-color: rgb(0,201,87)')
        self.label_4.setStyleSheet('background-color: rgb(255,215,0)')

        self.label_12.setStyleSheet('background-color: lightgray')
        self.label_13.setStyleSheet('background-color: lightgray')
        self.label_14.setStyleSheet('background-color: lightgray')
        self.label_15.setStyleSheet('background-color: lightgray')

        self.setStyleSheet('QPushButton:checked {background-color: forestgreen;}')
        self.label_5.setStyleSheet('color:brown')
        self.move(QApplication.desktop().width()-self.width(), 0)

        for widget in self.findChildren(QPushButton):
            widget.clicked.connect(self.on_change)
        for widget in self.findChildren(QDoubleSpinBox):
            widget.valueChanged.connect(self.on_change)

        self.gamepad_list = [
            UIFunc.Gamepad({
                'Pick': self.pushButton,
                'Use': self.pushButton_5,
                'Dash': self.pushButton_9,
                'X': self.doubleSpinBox,
                'Y': self.doubleSpinBox_5,
                'Emote': self.pushButton_13
            }, self),
            UIFunc.Gamepad({
                'Pick': self.pushButton_2,
                'Use': self.pushButton_6,
                'Dash': self.pushButton_10,
                'X': self.doubleSpinBox_2,
                'Y': self.doubleSpinBox_6,
                'Emote': self.pushButton_14
            }, self),
            UIFunc.Gamepad({
                'Pick': self.pushButton_3,
                'Use': self.pushButton_7,
                'Dash': self.pushButton_11,
                'X': self.doubleSpinBox_3,
                'Y': self.doubleSpinBox_7,
                'Emote': self.pushButton_15
            }, self),
            UIFunc.Gamepad({
                'Pick': self.pushButton_4,
                'Use': self.pushButton_8,
                'Dash': self.pushButton_12,
                'X': self.doubleSpinBox_4,
                'Y': self.doubleSpinBox_8,
                'Emote': self.pushButton_16
            }, self)
        ]
        self.active_gamepad = self.gamepad_list[0]
        self.label_12.setStyleSheet('background-color: gray')
        self.record = []
        self.cached_record = []
        self.cached_flag = [False] * 4
        self.first_frame_cached = False
        self.level = ""
        self.menu = []
        self.position_correction = []
        self.state = 'playing'  # in ['playing', 'input_recording', 'video']
        self.label_5.setText('PLAY')

        self.ignore_key = False
        self.key_press_signal.connect(self.key_press)
        self.key_release_signal.connect(self.key_release)

        def on_press(key):
            if key not in [Key.enter, Key.space,
                           Key.shift_l, Key.shift_r, Key.ctrl_l, Key.ctrl_r, Key.alt_l, Key.alt_r,
                           Key.f1, Key.f3, Key.f5, Key.f10, Key.f11]:
                key = self.listener.canonical(key)
            self.key_press_signal.emit(key)

        def on_release(key):
            if key not in [Key.enter, Key.space,
                           Key.shift_l, Key.shift_r, Key.ctrl_l, Key.ctrl_r, Key.alt_l, Key.alt_r,
                           Key.f1, Key.f3, Key.f5, Key.f10, Key.f11]:
                key = self.listener.canonical(key)
            self.key_release_signal.emit(key)

        self.listener = Listener(on_press=on_press, on_release=on_release)
        self.listener.start()
        self.video_thread = None

    @Slot(Key)
    def key_release(self, key):
        if self.state != 'playing' or self.ignore_key:
            return
        if key in [KeyCode.from_char('q'), KeyCode.from_char('e'),
                   KeyCode.from_char('w'), KeyCode.from_char('a'), KeyCode.from_char('s'), KeyCode.from_char('d'),
                   *UIFunc.keymap.values(), Key.enter]:
            self.active_gamepad.release_playing(key)

    @Slot(Key)
    def key_press(self, key):
        if key == KeyCode.from_char('0'):
            self.ignore_key = not self.ignore_key
            if self.ignore_key:
                self.label_5.setText('PAUSE')
            else:
                if self.state == 'playing':
                    self.label_5.setText('PLAY')
                elif self.state == 'video':
                    self.label_5.setText('VIDEO')
                else:
                    self.label_5.clear()
            return
        if self.ignore_key:
            return
        if key in [KeyCode.from_char('1'), KeyCode.from_char('2'), KeyCode.from_char('3'), KeyCode.from_char('4')] \
                and self.state in ['playing', 'input_recording']:
            self.label_12.setStyleSheet('background-color: lightgray')
            self.label_13.setStyleSheet('background-color: lightgray')
            self.label_14.setStyleSheet('background-color: lightgray')
            self.label_15.setStyleSheet('background-color: lightgray')
            if key == KeyCode.from_char('1'):
                self.label_12.setStyleSheet('background-color: gray')
                self.active_gamepad = self.gamepad_list[0]
            elif key == KeyCode.from_char('2'):
                self.label_13.setStyleSheet('background-color: gray')
                self.active_gamepad = self.gamepad_list[1]
            elif key == KeyCode.from_char('3'):
                self.label_14.setStyleSheet('background-color: gray')
                self.active_gamepad = self.gamepad_list[2]
            elif key == KeyCode.from_char('4'):
                self.label_15.setStyleSheet('background-color: gray')
                self.active_gamepad = self.gamepad_list[3]
        elif key == KeyCode.from_char('r') and self.state in ['playing', 'input_recording']:
            self.reset()
        elif key in [KeyCode.from_char('q'), KeyCode.from_char('e'),
                     KeyCode.from_char('w'), KeyCode.from_char('a'), KeyCode.from_char('s'), KeyCode.from_char('d'),
                     *UIFunc.keymap.values(), Key.enter,
                     Key.alt_l] and self.state in ['playing', 'input_recording']:
            if self.state == 'input_recording':
                self.active_gamepad.press_recording(key)
            else:
                self.active_gamepad.press_playing(key)
        elif key == Key.f3 and self.state in ['playing', 'input_recording']:
            if self.state == 'input_recording':
                self.save()
                self.record = []
                self.cached_record = []
                self.cached_flag = [False] * 4
                self.first_frame_cached = False
                self.reset()
                self.label_5.setText('PLAY')
                self.label_17.setText("FRAME 00000")
                label_list = [self.label_18, self.label_19, self.label_20, self.label_21]
                for label in label_list:
                    label.setStyleSheet('background-color: lightgray')
                self.state = 'playing'
            else:
                self.label_5.clear()
                self.state = 'input_recording'
        elif key == Key.f5 and self.state == 'input_recording':
            self.save()
            self.label_5.setText('SAVED')
        elif key == Key.f10 and self.state == 'playing':
            if os.path.exists(UIFunc.replay_file_path):
                with open(UIFunc.replay_file_path, 'r') as f:
                    record = json.load(f)
                    pickup_flag = record['pickup_flag']
                    self.level = record['level'] if 'level' in record.keys() else ""
                    self.menu = record['menu'] if 'menu' in record.keys() else []
                    self.position_correction = record['position_correction'] \
                        if 'position_correction' in record.keys() else []
                    self.position_correction.sort(key=lambda x: x[1])
                    record = record['state']
                    for i, rf in enumerate(record):
                        flag = [True] * 4
                        for j, rg in enumerate(rf):
                            if None in rg:
                                flag[j] = False
                        if not all(flag):
                            self.record = record[:i]
                            self.cached_record = record[i:]
                            self.cached_flag = flag
                            self.first_frame_cached = True
                            break
                    else:
                        self.record = record
                    self.label_17.setText("FRAME {:05d}".format(len(self.record)))
                    self.label_5.clear()
                    self.pushButton.setText("{:05d}".format(pickup_flag[0]))
                    self.pushButton_2.setText("{:05d}".format(pickup_flag[1]))
                    self.pushButton_3.setText("{:05d}".format(pickup_flag[2]))
                    self.pushButton_4.setText("{:05d}".format(pickup_flag[3]))
                    time.sleep(0.5)
                    if len(self.record) > 0:
                        for j, rg in enumerate(self.record[-1]):
                            self.gamepad_list[j].set_state(rg)
                    for i, gamepad in enumerate(self.gamepad_list):
                        dash_frame = [j for j, rf in enumerate(self.record) if rf[i][2] is True]
                        k = len(dash_frame)
                        while k > 1 and dash_frame[k-2] == dash_frame[k-1] - 1:
                            k -= 1
                        last_dash_frame = dash_frame[k-1] if k > 0 else 0
                        gamepad.button_dict['Dash'].setText("{:05d}".format(last_dash_frame))
                    label_list = [self.label_18, self.label_19, self.label_20, self.label_21]
                    for j in range(4):
                        if self.cached_flag[j]:
                            label_list[j].setStyleSheet('background-color: green')
                    self.state = 'input_recording'

        elif key == Key.f11 and self.state == 'input_recording':
            if not self.first_frame_cached:
                state = [gamepad.get_state() for gamepad in self.gamepad_list]
                self.record.append(state)
            self.label_5.clear()
            self.label_17.setText("FRAME {:05d}".format(len(self.record)))
            self.first_frame_cached = False
            if self.cached_record:
                time.sleep(0.3)
                rf = self.cached_record.pop(0)
                label_list = [self.label_18, self.label_19, self.label_20, self.label_21]
                for j, rg in enumerate(rf):
                    if None in rg:
                        self.cached_flag[j] = False
                        label_list[j].setStyleSheet('background-color: lightgray')
                    if self.cached_flag[j]:
                        self.gamepad_list[j].set_state(rg)
                if not self.cached_record:
                    for label in label_list:
                        label.setStyleSheet('background-color: lightgray')

        elif key == Key.f11 and self.state == 'video' and self.video_thread.video_stage == 0:
            time.sleep(0.1)
            self.video_thread.frame_cnt += 1
            screenshot = ImageGrab.grab()
            screenshot = cvtColor(np.array(screenshot), COLOR_RGB2BGR)
            self.video_thread.video.write(screenshot)
        elif key == Key.f1 and self.state in ['video', 'playing']:
            if self.state == 'video':
                if self.video_thread.video_stage == 0:
                    self.video_thread.video_stage = 1
                else:
                    self.label_5.setText('PLAY')
                    self.label_17.setText("FRAME 00000")
                    self.state = 'playing'
            elif os.path.exists(UIFunc.replay_file_path):
                self.state = 'video'
                self.label_5.setText('VIDEO')
                self.video_thread = self.VideoThread(self, audio=False)
                self.video_thread.signal.connect(self.replay_frame)
                self.video_thread.start()

    @Slot(list)
    def replay_frame(self, rf):
        for j, rg in enumerate(rf):
            self.gamepad_list[j].set_state(rg)
        self.record.append(rf)
        self.label_17.setText("FRAME {:05d}".format(len(self.record)))

    class AudioThread(QThread):
        def __init__(self, parent, path):
            super().__init__()
            self.parent = parent
            self.path = path

        def run(self):
            with pyaudio.PyAudio() as p:
                try:
                    wasapi_info = p.get_host_api_info_by_type(pyaudio.paWASAPI)
                except OSError:
                    print("Looks like WASAPI is not available on the system. Exiting...")
                    raise
                default_speakers = p.get_device_info_by_index(wasapi_info["defaultOutputDevice"])
                if not default_speakers["isLoopbackDevice"]:
                    for loopback in p.get_loopback_device_info_generator():
                        if default_speakers["name"] in loopback["name"]:
                            default_speakers = loopback
                            break
                    else:
                        print("Default loopback output device not found.")
                        raise OSError
                print(f"Recording from: ({default_speakers['index']}){default_speakers['name']}")
                wave_file = wave.open(self.path+'.wav', 'wb')
                wave_file.setnchannels(default_speakers["maxInputChannels"])
                wave_file.setsampwidth(pyaudio.get_sample_size(pyaudio.paInt16))
                wave_file.setframerate(int(default_speakers["defaultSampleRate"]))

                def callback(in_data, frame_count, time_info, status):
                    wave_file.writeframes(in_data)
                    return in_data, pyaudio.paContinue

                with p.open(format=pyaudio.paInt16,
                            channels=default_speakers["maxInputChannels"],
                            rate=int(default_speakers["defaultSampleRate"]),
                            frames_per_buffer=pyaudio.get_sample_size(pyaudio.paInt16),
                            input=True,
                            input_device_index=default_speakers["index"],
                            stream_callback=callback
                            ) as stream:
                    while self.parent.state == 'video':
                        time.sleep(0.1)  # Blocking execution while playing

                wave_file.close()

    class VideoThread(QThread):
        signal = Signal(list)

        def __init__(self, parent, audio=False):
            super().__init__()
            with open(UIFunc.replay_file_path, 'r') as f:
                record = json.load(f)
                self.pickup_flag = record['pickup_flag']
                self.record = record['state']
            self.parent = parent
            self.controller = Controller()
            self.video_stage = 0
            self.audio_thread = None
            self.frame_cnt = 0
            width, height = QApplication.desktop().width(), QApplication.desktop().height()
            fourcc = VideoWriter_fourcc(*'mp4v')
            path = 'D:\\TAS output\\output_' + time.strftime("%Y%m%d_%H%M%S", time.localtime())
            self.video = VideoWriter(path+'.mp4', fourcc, 50, (width, height))
            if audio:
                self.audio_thread = UIFunc.AudioThread(parent, path)
                self.audio_thread.start()

        def run(self):
            while self.video_stage == 0 and self.parent.state == 'video':
                time.sleep(0.1)
            time.sleep(0.5)
            for i, rf in enumerate(self.record):
                if self.parent.state != 'video':
                    break
                # self.signal.emit(rf)
                # time.sleep(0.1)
                self.controller.press(Key.f11)
                self.controller.release(Key.f11)
                self.frame_cnt += 1
                # if self.frame_cnt % 5 == 1 and self.audio_thread:
                #     time.sleep(0.5)
                # else:
                time.sleep(0.1)
                if self.video:
                    screenshot = ImageGrab.grab()
                    screenshot = cvtColor(np.array(screenshot), COLOR_RGB2BGR)
                    self.video.write(screenshot)

            self.parent.state = 'playing'
            self.parent.label_5.setText('PLAY')
            self.parent.label_17.setText("FRAME 00000")
            self.video.release()

    class Gamepad:
        def __init__(self, button_dict, parent):
            self.gamepad = vg.VX360Gamepad()
            self.button_dict = button_dict
            self.wasd_state = [False] * 4
            self.q_state = [False] * 2
            self.parent = parent

        def press_recording(self, key):
            if key == UIFunc.keymap['Pick'] or key == Key.enter:
                self.button_dict['Pick'].click()
            if key == Key.alt_l:
                self.button_dict['Pick'].setText("{:05d}".format(len(self.parent.record)))
            if key == UIFunc.keymap['Use']:
                self.button_dict['Use'].click()
            if key == UIFunc.keymap['Dash']:
                self.button_dict['Dash'].click()
                if self.button_dict['Dash'].isChecked():
                    self.button_dict['Dash'].setText("{:05d}".format(len(self.parent.record)))
            if key == UIFunc.keymap['Emote']:
                self.button_dict['Emote'].click()
            if key == KeyCode.from_char('d'):
                self.button_dict['X'].setValue(self.button_dict['X'].value() + 1.0)
            if key == KeyCode.from_char('a'):
                self.button_dict['X'].setValue(self.button_dict['X'].value() - 1.0)
            if key == KeyCode.from_char('w'):
                self.button_dict['Y'].setValue(self.button_dict['Y'].value() + 1.0)
            if key == KeyCode.from_char('s'):
                self.button_dict['Y'].setValue(self.button_dict['Y'].value() - 1.0)
            if key == KeyCode.from_char('q'):
                self.button_dict['X'].setValue(UIFunc.q_coord[0][0])
                self.button_dict['Y'].setValue(UIFunc.q_coord[0][1])
            if key == KeyCode.from_char('e'):
                self.button_dict['X'].setValue(UIFunc.q_coord[1][0])
                self.button_dict['Y'].setValue(UIFunc.q_coord[1][1])
            self.update()

        def press_playing(self, key):
            if key == UIFunc.keymap['Pick'] or key == Key.enter:
                self.button_dict['Pick'].setChecked(True)
            if key == UIFunc.keymap['Use']:
                self.button_dict['Use'].setChecked(True)
            if key == UIFunc.keymap['Dash']:
                self.button_dict['Dash'].setChecked(True)
            if key == UIFunc.keymap['Emote']:
                self.button_dict['Emote'].setChecked(True)
            if key == KeyCode.from_char('d'):
                self.wasd_state[3] = True
            if key == KeyCode.from_char('a'):
                self.wasd_state[1] = True
            if key == KeyCode.from_char('w'):
                self.wasd_state[0] = True
            if key == KeyCode.from_char('s'):
                self.wasd_state[2] = True
            if key == KeyCode.from_char('q'):
                self.q_state[0] = True
            if key == KeyCode.from_char('e'):
                self.q_state[1] = True
            if self.q_state[0]:
                self.button_dict['X'].setValue(UIFunc.q_coord[0][0])
                self.button_dict['Y'].setValue(UIFunc.q_coord[0][1])
            elif self.q_state[1]:
                self.button_dict['X'].setValue(UIFunc.q_coord[1][0])
                self.button_dict['Y'].setValue(UIFunc.q_coord[1][1])
            else:
                self.button_dict['X'].setValue(self.wasd_state[1] * -1.0 + self.wasd_state[3] * 1.0)
                self.button_dict['Y'].setValue(self.wasd_state[2] * -1.0 + self.wasd_state[0] * 1.0)
            self.update()

        def release_playing(self, key):
            if key == UIFunc.keymap['Pick'] or key == Key.enter:
                self.button_dict['Pick'].setChecked(False)
            if key == UIFunc.keymap['Use']:
                self.button_dict['Use'].setChecked(False)
            if key == UIFunc.keymap['Dash']:
                self.button_dict['Dash'].setChecked(False)
            if key == UIFunc.keymap['Emote']:
                self.button_dict['Emote'].setChecked(False)
            if key == KeyCode.from_char('d'):
                self.wasd_state[3] = False
            if key == KeyCode.from_char('a'):
                self.wasd_state[1] = False
            if key == KeyCode.from_char('w'):
                self.wasd_state[0] = False
            if key == KeyCode.from_char('s'):
                self.wasd_state[2] = False
            if key == KeyCode.from_char('q'):
                self.q_state[0] = False
            if key == KeyCode.from_char('e'):
                self.q_state[1] = False
            if self.q_state[0]:
                self.button_dict['X'].setValue(UIFunc.q_coord[0][0])
                self.button_dict['Y'].setValue(UIFunc.q_coord[0][1])
            elif self.q_state[1]:
                self.button_dict['X'].setValue(UIFunc.q_coord[1][0])
                self.button_dict['Y'].setValue(UIFunc.q_coord[1][1])
            else:
                self.button_dict['X'].setValue(self.wasd_state[1] * -1.0 + self.wasd_state[3] * 1.0)
                self.button_dict['Y'].setValue(self.wasd_state[2] * -1.0 + self.wasd_state[0] * 1.0)
            self.update()

        def update(self):
            for key, button in zip(['Pick', 'Use', 'Dash', 'Emote'],
                                   [vg.XUSB_BUTTON.XUSB_GAMEPAD_A,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_X,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_B,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_Y]):
                if self.button_dict[key].isChecked():
                    self.gamepad.press_button(button=button)
                else:
                    self.gamepad.release_button(button=button)
            x = self.button_dict['X'].value()
            y = self.button_dict['Y'].value()
            self.gamepad.left_joystick_float(x, y)
            self.gamepad.update()

        def get_state(self):
            return [self.button_dict['Pick'].isChecked(),
                    self.button_dict['Use'].isChecked(),
                    self.button_dict['Dash'].isChecked(),
                    self.button_dict['Emote'].isChecked(),
                    self.button_dict['X'].value(),
                    self.button_dict['Y'].value()]

        def set_state(self, state):
            self.button_dict['Pick'].setChecked(state[0])
            self.button_dict['Use'].setChecked(state[1])
            flag_dash = self.button_dict['Dash'].isChecked()
            self.button_dict['Dash'].setChecked(state[2])
            if not flag_dash and self.button_dict['Dash'].isChecked():
                self.button_dict['Dash'].setText("{:05d}".format(len(self.parent.record)))
            self.button_dict['Emote'].setChecked(state[3])
            self.button_dict['X'].setValue(state[4])
            self.button_dict['Y'].setValue(state[5])
            self.update()

    def on_change(self):
        for gamepad in self.gamepad_list:
            gamepad.update()

    def save(self):
        save_record = []
        save_record += self.record
        if self.cached_record and not self.first_frame_cached:
            state = [gamepad.get_state() for gamepad in self.gamepad_list]
            for j in range(4):
                if not self.cached_flag[j]:
                    state[j][0] = None
            save_record.append(state)
            save_record += self.cached_record
        path = 'records/record_' + time.strftime("%Y%m%d_%H%M%S_", time.localtime()) + \
               str(len(save_record)) + 'f.json'
        with open(path, 'w') as f:
            f.write('{\n')
            f.write('  \"author\": \"{}\",\n'.format(UIFunc.author))
            f.write('  \"level\": \"{}\",\n'.format(self.level))
            f.write('  \"menu\": {},\n'.format(self.menu))
            f.write('  \"pickup_flag\": [{},{},{},{}],\n'.format(
                int(self.pushButton.text()),
                int(self.pushButton_2.text()),
                int(self.pushButton_3.text()),
                int(self.pushButton_4.text()))
            )
            if not self.position_correction:
                f.write('  \"position_correction\": [],\n')
            else:
                f.write('  \"position_correction\": \n')
                f.write('  [\n')
                for pc in self.position_correction:
                    if len(pc) == 5:
                        f.write('    [\"{}\", {}, {:.4f}, {:.4f}, {:.4f}]'.format(*pc))
                    elif len(pc) == 6:
                        f.write('    [\"{}\", {}, {:.4f}, {:.4f}, {:.4f}'.format(*pc[:5])
                                + ', {}]'.format(pc[5]).lower())
                    else:
                        f.write('    [\"Error\", -1, 0.0, 0.0, 0.0]')
                    f.write('\n' if pc is self.position_correction[-1] else ',\n')
                f.write('  ],\n')

            f.write('  \"state\": \n')
            f.write('  [\n')
            for rf in save_record:
                f.write('    [\n')
                for rg in rf:
                    f.write('      '+str(rg).lower().replace("none", "null"))
                    f.write('\n' if rg is rf[-1] else ',\n')
                f.write('    ]\n' if rf is save_record[-1] else '    ],\n')
            f.write('  ]\n')
            f.write('}\n')

    def reset(self):
        for widget in self.findChildren(QPushButton):
            widget.setChecked(False)
        for widget in self.findChildren(QDoubleSpinBox):
            widget.setValue(0.0)
        for gamepad in self.gamepad_list:
            gamepad.wasd_state = [False] * 4
            gamepad.q_state = [False] * 2
            gamepad.update()


if __name__ == '__main__':
    app = QApplication(sys.argv)
    ui = UIFunc(app)
    ui.setFixedSize(ui.width(), ui.height())
    ui.show()
    sys.exit(app.exec_())
