import sys
from PySide2.QtWidgets import QApplication, QMainWindow, QPushButton, QDoubleSpinBox
from PySide2.QtCore import Qt, Signal, Slot
from UIView import Ui_UIView
import vgamepad as vg
import json
import time
from pynput.keyboard import Key, KeyCode
from pynput.keyboard import Listener
import os


class UIFunc(QMainWindow, Ui_UIView):
    key_press_signal = Signal(Key)
    key_release_signal = Signal(Key)
    keymap = {
        'Pick': Key.space,
        'Dash': KeyCode.from_char('.'),
        'Use': KeyCode.from_char('/'),
        'Emote': KeyCode.from_char('c'),
        'Shift': Key.shift_r,
    }
    replay_file_path = 'D:\\Steam\\steamapps\\common\\Overcooked! 2\\BepInEx\\plugins\\replay.json'
    author = 'GUA'

    def __init__(self, app):
        super(UIFunc, self).__init__()
        self.app = app
        self.setupUi(self)
        self.setWindowFlags(Qt.WindowStaysOnTopHint)
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
        self.move(app.primaryScreen().availableGeometry().right() - self.width(),
                  app.primaryScreen().availableGeometry().top())

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
                'Emote': self.pushButton_13,
                'Shift': self.pushButton_17
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
        self.num_player = 4
        self.cached_record = []
        self.cached_flag = [False] * 4
        self.first_frame_cached = False
        self.level = ""
        self.menu = []
        self.position_correction = []
        self.rng_assigner = []
        self.state = 'playing'  # in ['playing', 'input_recording']
        self.label_5.setText('PLAY')

        self.ignore_key = False
        self.key_press_signal.connect(self.key_press)
        self.key_release_signal.connect(self.key_release)

        def on_press(key):
            if key not in [Key.enter, Key.space,
                           Key.shift_l, Key.shift_r, Key.ctrl_l, Key.ctrl_r, Key.alt_l, Key.alt_r,
                           Key.f3, Key.f5, Key.f10, Key.f11]:
                key = self.listener.canonical(key)
            self.key_press_signal.emit(key)

        def on_release(key):
            if key not in [Key.enter, Key.space,
                           Key.shift_l, Key.shift_r, Key.ctrl_l, Key.ctrl_r, Key.alt_l, Key.alt_r,
                           Key.f3, Key.f5, Key.f10, Key.f11]:
                key = self.listener.canonical(key)
            self.key_release_signal.emit(key)

        self.listener = Listener(on_press=on_press, on_release=on_release)
        self.listener.start()

    @Slot(Key)
    def key_release(self, key):
        if self.state != 'playing' or self.ignore_key:
            return
        if key in [KeyCode.from_char('w'), KeyCode.from_char('a'), KeyCode.from_char('s'), KeyCode.from_char('d'),
                   *UIFunc.keymap.values(), Key.enter]:
            self.active_gamepad.release_playing(key)

    @Slot(Key)
    def key_press(self, key):
        if key == KeyCode.from_char('`'):
            self.ignore_key = not self.ignore_key
            if self.ignore_key:
                self.label_5.setText('PAUSE')
            else:
                if self.state == 'playing':
                    self.label_5.setText('PLAY')
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
        elif key in [KeyCode.from_char('w'), KeyCode.from_char('a'), KeyCode.from_char('s'), KeyCode.from_char('d'),
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
                self.label_17.setText("FRAME 00000 ")
                label_list = [self.label_18, self.label_19, self.label_20, self.label_21]
                for label in label_list:
                    label.setStyleSheet('background-color: none')
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
                    self.rng_assigner = record['rng'] if 'rng' in record.keys() else []
                    record = record['state']
                    self.num_player = len(record[0]) if len(record) > 0 else 4
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
                    self.label_17.setText("FRAME {:05d} ".format(len(self.record)))
                    self.label_5.clear()
                    self.pushButton.setText("{:05d}".format(pickup_flag[0]))
                    self.pushButton_2.setText("{:05d}".format(pickup_flag[1]))
                    self.pushButton_3.setText("{:05d}".format(pickup_flag[2]))
                    self.pushButton_4.setText("{:05d}".format(pickup_flag[3]))
                    time.sleep(0.5)
                    if len(self.record) > 0:
                        for j, rg in enumerate(self.record[-1]):
                            self.gamepad_list[j].set_state(rg)
                    for j in range(self.num_player):
                        dash_frame = [i for i, rf in enumerate(self.record) if rf[j][2] is True]
                        k = len(dash_frame)
                        while k > 1 and dash_frame[k-2] == dash_frame[k-1] - 1:
                            k -= 1
                        last_dash_frame = dash_frame[k-1] if k > 0 else 0
                        self.gamepad_list[j].button_dict['Dash'].setText("{:05d}".format(last_dash_frame))
                    label_list = [self.label_18, self.label_19, self.label_20, self.label_21]
                    for j in range(4):
                        if self.cached_flag[j]:
                            label_list[j].setStyleSheet('background-color: green')
                        else:
                            label_list[j].setStyleSheet('background-color: lightgray')
                    self.state = 'input_recording'

        elif key == Key.f11 and self.state == 'input_recording':
            if not self.first_frame_cached:
                state = [self.gamepad_list[j].get_state() for j in range(self.num_player)]
                self.record.append(state)
            self.label_5.clear()
            self.label_17.setText("FRAME {:05d} ".format(len(self.record)))
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

    class Gamepad:
        def __init__(self, button_dict, parent):
            self.gamepad = vg.VX360Gamepad()
            self.button_dict = button_dict
            self.wasd_state = [False] * 4
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
            if key == UIFunc.keymap['Shift'] and 'Shift' in self.button_dict.keys():
                self.button_dict['Shift'].click()
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
            if key == UIFunc.keymap['Shift'] and 'Shift' in self.button_dict.keys():
                self.button_dict['Shift'].setChecked(True)
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
            if key == UIFunc.keymap['Shift'] and 'Shift' in self.button_dict.keys():
                self.button_dict['Shift'].setChecked(False)
            self.button_dict['X'].setValue(self.wasd_state[1] * -1.0 + self.wasd_state[3] * 1.0)
            self.button_dict['Y'].setValue(self.wasd_state[2] * -1.0 + self.wasd_state[0] * 1.0)
            self.update()

        def update(self):
            for key, button in zip(['Pick', 'Use', 'Dash', 'Emote', 'Shift'],
                                   [vg.XUSB_BUTTON.XUSB_GAMEPAD_A,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_X,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_B,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_Y,
                                    vg.XUSB_BUTTON.XUSB_GAMEPAD_LEFT_SHOULDER]):
                if key in self.button_dict.keys():
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
                    self.button_dict['Y'].value(),
                    'Shift' in self.button_dict.keys() and self.button_dict['Shift'].isChecked()]

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
            if 'Shift' in self.button_dict.keys():
                self.button_dict['Shift'].setChecked(state[6] if len(state) > 6 else False)
            self.update()

    def on_change(self):
        for gamepad in self.gamepad_list:
            gamepad.update()

    def save(self):
        save_record = []
        save_record += self.record
        if self.cached_record and not self.first_frame_cached:
            state = [self.gamepad_list[j].get_state() for j in range(self.num_player)]
            for j in range(self.num_player):
                if not self.cached_flag[j]:
                    state[j][0] = None
            save_record.append(state)
            save_record += self.cached_record
        path = './records/record_' + time.strftime("%Y%m%d_%H%M%S_", time.localtime()) + \
               str(len(save_record)) + 'f.json'
        with open(path, 'w') as f:
            f.write('{\n')
            f.write('  \"author\": \"{}\",\n'.format(UIFunc.author))
            f.write('  \"level\": \"{}\",\n'.format(self.level))
            f.write('  \"menu\": {},\n'.format(self.menu))
            if not self.rng_assigner:
                f.write('  \"rng\": [],\n')
            else:
                f.write('  \"rng\": \n')
                f.write('  [\n')
                for rng in self.rng_assigner:
                    f.write('    [\"{}\", {}]'.format(*rng))
                    f.write('\n' if rng is self.rng_assigner[-1] else ',\n')
                f.write('  ],\n')

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
                    if len(pc) >= 5:
                        f.write('    [\"{}\", {}, {:.4f}, {:.4f}, {:.4f}]'.format(*pc[:5]))
                    else:
                        f.write('    [\"Error\", -1, 0.0, 0.0, 0.0]')
                    f.write('\n' if pc is self.position_correction[-1] else ',\n')
                f.write('  ],\n')

            f.write('  \"state\": \n')
            f.write('  [\n')
            for rf in save_record:
                if self.num_player > 1:
                    f.write('    [\n')
                    for rg in rf:
                        f.write('      '+str(rg).lower().replace("none", "null"))
                        f.write('\n' if rg is rf[-1] else ',\n')
                    f.write('    ]\n' if rf is save_record[-1] else '    ],\n')
                else:
                    f.write('    [')
                    f.write(str(rf[0]).lower().replace("none", "null"))
                    f.write(']\n' if rf is save_record[-1] else '],\n')
            f.write('  ]\n')
            f.write('}\n')

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
