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
        self.state = 'playing'
        self.label_5.setText('PLAY')

        self.key_press_signal.connect(self.key_press)
        self.key_release_signal.connect(self.key_release)

        def on_press(key):
            if key not in [Key.enter, Key.space,
                           Key.shift_l, Key.shift_r, Key.ctrl_l, Key.ctrl_r, Key.alt_l, Key.alt_r,
                           Key.f3, Key.f5, Key.f10]:
                key = self.listener.canonical(key)
            self.key_press_signal.emit(key)

        def on_release(key):
            if key not in [Key.enter, Key.space,
                           Key.shift_l, Key.shift_r, Key.ctrl_l, Key.ctrl_r, Key.alt_l, Key.alt_r,
                           Key.f3, Key.f5, Key.f10]:
                key = self.listener.canonical(key)
            self.key_release_signal.emit(key)

        self.listener = Listener(on_press=on_press, on_release=on_release)
        self.listener.start()
        self.replay_thread = None

    @Slot(Key)
    def key_release(self, key):
        if self.state != 'playing':
            return
        if key in [KeyCode.from_char('w'), KeyCode.from_char('a'), KeyCode.from_char('s'), KeyCode.from_char('d'),
                   Key.space, KeyCode.from_char('.'), KeyCode.from_char('/'), KeyCode.from_char('c'), Key.enter]:
            self.active_gamepad.release_playing(key)

    @Slot(Key)
    def key_press(self, key):
        if key in [KeyCode.from_char('1'), KeyCode.from_char('2'), KeyCode.from_char('3'), KeyCode.from_char('4')]:
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
        elif key in [KeyCode.from_char('w'), KeyCode.from_char('a'), KeyCode.from_char('s'), KeyCode.from_char('d'),
                     Key.space, KeyCode.from_char('.'), KeyCode.from_char('/'), KeyCode.from_char('c'), Key.enter,
                     Key.alt_l] and self.state in ['playing', 'input_recording']:
            if self.state == 'input_recording':
                self.active_gamepad.press_recording(key)
            else:
                self.active_gamepad.press_playing(key)
        elif key == Key.f3 and self.state in ['playing', 'input_recording']:
            if self.state == 'input_recording':
                self.save()
                self.record = []
                self.reset()
                self.label_5.setText('PLAY')
                self.label_17.setText("FRAME 00000")
                self.state = 'playing'
            else:
                self.label_5.clear()
                self.state = 'input_recording'
        elif key == Key.f10 and self.state == 'input_recording':
            state = [gamepad.get_state() for gamepad in self.gamepad_list]
            self.record.append(state)
            self.label_5.clear()
            self.label_17.setText("FRAME {:05d}".format(len(self.record)))
        elif key == Key.f10 and self.state == 'replaying' and self.replay_thread.video_stage == 0:
            time.sleep(0.05)
            self.replay_thread.frame_cnt += 1
            screenshot = ImageGrab.grab()
            screenshot = cvtColor(np.array(screenshot), COLOR_RGB2BGR)
            self.replay_thread.video.write(screenshot)
        elif key == Key.f5 and self.state == 'input_recording':
            self.save()
            self.label_5.setText('SAVED')
        elif key == KeyCode.from_char('p') and self.state in ['replaying', 'playing']:
            if self.state == 'replaying':
                if self.replay_thread.video_stage == 0:
                    self.replay_thread.video_stage = 1
                else:
                    self.state = 'input_recording'
            elif os.path.exists('replay.json'):
                self.state = 'replaying'
                self.label_5.setText('REPLAY')
                self.replay_thread = self.ReplayThread(self)
                self.replay_thread.signal.connect(self.replay_frame)
                self.replay_thread.start()
        elif key == KeyCode.from_char('o') and self.state in ['replaying', 'playing']:
            if self.state == 'replaying':
                self.state = 'input_recording'
            elif os.path.exists('replay.json'):
                self.state = 'replaying'
                self.label_5.setText('VIDEO')
                self.replay_thread = self.ReplayThread(self, video=True)
                self.replay_thread.signal.connect(self.replay_frame)
                self.replay_thread.start()

    @Slot(list)
    def replay_frame(self, rf):
        if isinstance(rf[0], int):
            self.label_5.clear()
            self.pushButton.setText("{:05d}".format(rf[0]))
            self.pushButton_2.setText("{:05d}".format(rf[1]))
            self.pushButton_3.setText("{:05d}".format(rf[2]))
            self.pushButton_4.setText("{:05d}".format(rf[3]))
        else:
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
                    while self.parent.state == 'replaying':
                        time.sleep(0.1)  # Blocking execution while playing

                wave_file.close()

    class ReplayThread(QThread):
        signal = Signal(list)

        def __init__(self, parent, video=False, audio=False):
            super().__init__()
            with open('replay.json', 'r') as f:
                record = json.load(f)
                self.pickup_flag = record[0]
                self.record = record[1:]
            self.parent = parent
            self.parent.record = []
            self.controller = Controller()
            self.video = None
            self.video_stage = 0 if video else -1
            self.audio_thread = None
            self.frame_cnt = 0
            if video:
                width, height = QApplication.desktop().width(), QApplication.desktop().height()
                fourcc = VideoWriter_fourcc(*'mp4v')
                path = 'D:/TAS output/output_' + time.strftime("%Y%m%d_%H%M%S", time.localtime())
                self.video = VideoWriter(path+'.mp4', fourcc, 50, (width, height))
            if audio:
                self.audio_thread = UIFunc.AudioThread(parent, path)
                self.audio_thread.start()

        def run(self):
            while self.video_stage == 0 and self.parent.state == 'replaying':
                time.sleep(0.1)
            for i, rf in enumerate(self.record):
                if self.parent.state != 'replaying':
                    break
                self.signal.emit(rf)
                sleep_time = 0.1 if self.video else 0.05
                time.sleep(sleep_time)
                self.controller.press(Key.f10)
                self.controller.release(Key.f10)
                self.frame_cnt += 1
                if self.frame_cnt % 5 == 1 and self.video:
                    time.sleep(0.5)
                else:
                    time.sleep(sleep_time)
                if self.video:
                    screenshot = ImageGrab.grab()
                    screenshot = cvtColor(np.array(screenshot), COLOR_RGB2BGR)
                    self.video.write(screenshot)

            self.signal.emit(self.pickup_flag)
            self.parent.state = 'input_recording'
            if self.video:
                self.video.release()

    class Gamepad:
        def __init__(self, button_dict, parent):
            self.gamepad = vg.VX360Gamepad()
            self.button_dict = button_dict
            self.wasd_state = [False] * 4
            self.parent = parent

        def press_recording(self, key):
            if key == Key.space or key == Key.enter:
                self.button_dict['Pick'].click()
            if key == Key.alt_l:
                self.button_dict['Pick'].setText("{:05d}".format(len(self.parent.record)))
            if key == KeyCode.from_char('/'):
                self.button_dict['Use'].click()
            if key == KeyCode.from_char('.'):
                self.button_dict['Dash'].click()
                if self.button_dict['Dash'].isChecked():
                    self.button_dict['Dash'].setText("{:05d}".format(len(self.parent.record)))
            if key == KeyCode.from_char('c'):
                self.button_dict['Emote'].click()
            if key == KeyCode.from_char('d'):
                self.button_dict['X'].setValue(self.button_dict['X'].value() + 1.0)
            if key == KeyCode.from_char('a'):
                self.button_dict['X'].setValue(self.button_dict['X'].value() - 1.0)
            if key == KeyCode.from_char('w'):
                self.button_dict['Y'].setValue(self.button_dict['Y'].value() + 1.0)
            if key == KeyCode.from_char('s'):
                self.button_dict['Y'].setValue(self.button_dict['Y'].value() - 1.0)
            self.update()

        def press_playing(self, key):
            if key == Key.space or key == Key.enter:
                self.button_dict['Pick'].setChecked(True)
            if key == KeyCode.from_char('/'):
                self.button_dict['Use'].setChecked(True)
            if key == KeyCode.from_char('.'):
                self.button_dict['Dash'].setChecked(True)
            if key == KeyCode.from_char('c'):
                self.button_dict['Emote'].setChecked(True)
            if key == KeyCode.from_char('d'):
                self.wasd_state[3] = True
            if key == KeyCode.from_char('a'):
                self.wasd_state[1] = True
            if key == KeyCode.from_char('w'):
                self.wasd_state[0] = True
            if key == KeyCode.from_char('s'):
                self.wasd_state[2] = True
            self.button_dict['X'].setValue(self.wasd_state[1] * -1.0 + self.wasd_state[3] * 1.0)
            self.button_dict['Y'].setValue(self.wasd_state[2] * -1.0 + self.wasd_state[0] * 1.0)
            self.update()

        def release_playing(self, key):
            if key == Key.space or key == Key.enter:
                self.button_dict['Pick'].setChecked(False)
            if key == KeyCode.from_char('/'):
                self.button_dict['Use'].setChecked(False)
            if key == KeyCode.from_char('.'):
                self.button_dict['Dash'].setChecked(False)
            if key == KeyCode.from_char('c'):
                self.button_dict['Emote'].setChecked(False)
            if key == KeyCode.from_char('d'):
                self.wasd_state[3] = False
            if key == KeyCode.from_char('a'):
                self.wasd_state[1] = False
            if key == KeyCode.from_char('w'):
                self.wasd_state[0] = False
            if key == KeyCode.from_char('s'):
                self.wasd_state[2] = False
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
        path = 'records/record_' + time.strftime("%Y%m%d_%H%M%S_", time.localtime()) + \
               str(len(self.record)) + 'f.json'
        with open(path, 'w') as f:
            f.write('[\n')
            f.write('  [{},{},{},{}],\n'.format(int(self.pushButton.text()),
                                                int(self.pushButton_2.text()),
                                                int(self.pushButton_3.text()),
                                                int(self.pushButton_4.text())))
            for rf in self.record:
                f.write('  [\n')
                for rg in rf:
                    f.write('    '+str(rg).lower())
                    f.write('\n' if rg is rf[-1] else ',\n')
                f.write('  ]\n' if rf is self.record[-1] else '  ],\n')
            f.write(']\n')

    def reset(self):
        for widget in self.findChildren(QPushButton):
            widget.setChecked(False)
        for widget in self.findChildren(QDoubleSpinBox):
            widget.setValue(0.0)
        for gamepad in self.gamepad_list:
            gamepad.wasd_state = [False] * 4
            gamepad.update()


if __name__ == '__main__':
    app = QApplication(sys.argv)
    ui = UIFunc(app)
    ui.setFixedSize(ui.width(), ui.height())
    ui.show()
    sys.exit(app.exec_())

