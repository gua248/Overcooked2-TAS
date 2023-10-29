# -*- coding: utf-8 -*-

################################################################################
## Form generated from reading UI file 'UIView.ui'
##
## Created by: Qt User Interface Compiler version 5.15.2
##
## WARNING! All changes made in this file will be lost when recompiling UI file!
################################################################################

from PySide2.QtCore import *
from PySide2.QtGui import *
from PySide2.QtWidgets import *


class Ui_UIView(object):
    def setupUi(self, UIView):
        if not UIView.objectName():
            UIView.setObjectName(u"UIView")
        UIView.resize(571, 251)
        icon = QIcon()
        icon.addFile(u"games.ico", QSize(), QIcon.Normal, QIcon.Off)
        UIView.setWindowIcon(icon)
        self.centralwidget = QWidget(UIView)
        self.centralwidget.setObjectName(u"centralwidget")
        self.groupBox = QGroupBox(self.centralwidget)
        self.groupBox.setObjectName(u"groupBox")
        self.groupBox.setGeometry(QRect(9, 19, 551, 191))
        self.groupBox.setAlignment(Qt.AlignLeading|Qt.AlignLeft|Qt.AlignVCenter)
        self.groupBox.setFlat(False)
        self.gridLayout = QGridLayout(self.groupBox)
        self.gridLayout.setObjectName(u"gridLayout")
        self.gridLayout.setContentsMargins(11, 11, 11, 11)
        self.pushButton_4 = QPushButton(self.groupBox)
        self.pushButton_4.setObjectName(u"pushButton_4")
        font = QFont()
        font.setBold(True)
        font.setWeight(75)
        self.pushButton_4.setFont(font)
        self.pushButton_4.setFocusPolicy(Qt.NoFocus)
        self.pushButton_4.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_4, 5, 1, 1, 1)

        self.pushButton_5 = QPushButton(self.groupBox)
        self.pushButton_5.setObjectName(u"pushButton_5")
        self.pushButton_5.setFocusPolicy(Qt.NoFocus)
        self.pushButton_5.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_5, 2, 2, 1, 1)

        self.doubleSpinBox_4 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_4.setObjectName(u"doubleSpinBox_4")
        self.doubleSpinBox_4.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_4.setMinimum(-1.000000000000000)
        self.doubleSpinBox_4.setMaximum(1.000000000000000)
        self.doubleSpinBox_4.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_4, 5, 4, 1, 1)

        self.label_4 = QLabel(self.groupBox)
        self.label_4.setObjectName(u"label_4")
        font1 = QFont()
        font1.setFamily(u"\u5fae\u8f6f\u96c5\u9ed1")
        font1.setBold(True)
        font1.setWeight(75)
        self.label_4.setFont(font1)
        self.label_4.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_4, 5, 0, 1, 1)

        self.doubleSpinBox_2 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_2.setObjectName(u"doubleSpinBox_2")
        self.doubleSpinBox_2.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_2.setMinimum(-1.000000000000000)
        self.doubleSpinBox_2.setMaximum(1.000000000000000)
        self.doubleSpinBox_2.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_2, 3, 4, 1, 1)

        self.label_6 = QLabel(self.groupBox)
        self.label_6.setObjectName(u"label_6")
        font2 = QFont()
        font2.setFamily(u"\u5fae\u8f6f\u96c5\u9ed1")
        font2.setBold(True)
        font2.setItalic(False)
        font2.setUnderline(False)
        font2.setWeight(75)
        self.label_6.setFont(font2)
        self.label_6.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_6, 1, 1, 1, 1)

        self.pushButton_12 = QPushButton(self.groupBox)
        self.pushButton_12.setObjectName(u"pushButton_12")
        self.pushButton_12.setFont(font)
        self.pushButton_12.setFocusPolicy(Qt.NoFocus)
        self.pushButton_12.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_12, 5, 3, 1, 1)

        self.pushButton_3 = QPushButton(self.groupBox)
        self.pushButton_3.setObjectName(u"pushButton_3")
        self.pushButton_3.setFont(font)
        self.pushButton_3.setFocusPolicy(Qt.NoFocus)
        self.pushButton_3.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_3, 4, 1, 1, 1)

        self.pushButton_11 = QPushButton(self.groupBox)
        self.pushButton_11.setObjectName(u"pushButton_11")
        self.pushButton_11.setFont(font)
        self.pushButton_11.setFocusPolicy(Qt.NoFocus)
        self.pushButton_11.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_11, 4, 3, 1, 1)

        self.pushButton_16 = QPushButton(self.groupBox)
        self.pushButton_16.setObjectName(u"pushButton_16")
        self.pushButton_16.setFocusPolicy(Qt.NoFocus)
        self.pushButton_16.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_16, 5, 6, 1, 1)

        self.pushButton_8 = QPushButton(self.groupBox)
        self.pushButton_8.setObjectName(u"pushButton_8")
        self.pushButton_8.setFocusPolicy(Qt.NoFocus)
        self.pushButton_8.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_8, 5, 2, 1, 1)

        self.label_2 = QLabel(self.groupBox)
        self.label_2.setObjectName(u"label_2")
        self.label_2.setFont(font1)
        self.label_2.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_2, 4, 0, 1, 1)

        self.pushButton_13 = QPushButton(self.groupBox)
        self.pushButton_13.setObjectName(u"pushButton_13")
        self.pushButton_13.setFocusPolicy(Qt.NoFocus)
        self.pushButton_13.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_13, 2, 6, 1, 1)

        self.label_3 = QLabel(self.groupBox)
        self.label_3.setObjectName(u"label_3")
        self.label_3.setFont(font1)
        self.label_3.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_3, 3, 0, 1, 1)

        self.label_9 = QLabel(self.groupBox)
        self.label_9.setObjectName(u"label_9")
        self.label_9.setFont(font2)
        self.label_9.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_9, 1, 4, 1, 1)

        self.label_10 = QLabel(self.groupBox)
        self.label_10.setObjectName(u"label_10")
        self.label_10.setFont(font2)
        self.label_10.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_10, 1, 5, 1, 1)

        self.pushButton_6 = QPushButton(self.groupBox)
        self.pushButton_6.setObjectName(u"pushButton_6")
        self.pushButton_6.setFocusPolicy(Qt.NoFocus)
        self.pushButton_6.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_6, 3, 2, 1, 1)

        self.doubleSpinBox_6 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_6.setObjectName(u"doubleSpinBox_6")
        self.doubleSpinBox_6.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_6.setMinimum(-1.000000000000000)
        self.doubleSpinBox_6.setMaximum(1.000000000000000)
        self.doubleSpinBox_6.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_6, 3, 5, 1, 1)

        self.pushButton_10 = QPushButton(self.groupBox)
        self.pushButton_10.setObjectName(u"pushButton_10")
        self.pushButton_10.setFont(font)
        self.pushButton_10.setFocusPolicy(Qt.NoFocus)
        self.pushButton_10.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_10, 3, 3, 1, 1)

        self.label_8 = QLabel(self.groupBox)
        self.label_8.setObjectName(u"label_8")
        self.label_8.setFont(font2)
        self.label_8.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_8, 1, 3, 1, 1)

        self.doubleSpinBox_3 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_3.setObjectName(u"doubleSpinBox_3")
        self.doubleSpinBox_3.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_3.setMinimum(-1.000000000000000)
        self.doubleSpinBox_3.setMaximum(1.000000000000000)
        self.doubleSpinBox_3.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_3, 4, 4, 1, 1)

        self.doubleSpinBox_8 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_8.setObjectName(u"doubleSpinBox_8")
        self.doubleSpinBox_8.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_8.setMinimum(-1.000000000000000)
        self.doubleSpinBox_8.setMaximum(1.000000000000000)
        self.doubleSpinBox_8.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_8, 5, 5, 1, 1)

        self.label = QLabel(self.groupBox)
        self.label.setObjectName(u"label")
        self.label.setFont(font1)
        self.label.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label, 2, 0, 1, 1)

        self.pushButton_14 = QPushButton(self.groupBox)
        self.pushButton_14.setObjectName(u"pushButton_14")
        self.pushButton_14.setFocusPolicy(Qt.NoFocus)
        self.pushButton_14.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_14, 3, 6, 1, 1)

        self.pushButton = QPushButton(self.groupBox)
        self.pushButton.setObjectName(u"pushButton")
        self.pushButton.setFont(font)
        self.pushButton.setFocusPolicy(Qt.NoFocus)
        self.pushButton.setCheckable(True)
        self.pushButton.setChecked(False)

        self.gridLayout.addWidget(self.pushButton, 2, 1, 1, 1)

        self.label_7 = QLabel(self.groupBox)
        self.label_7.setObjectName(u"label_7")
        self.label_7.setFont(font2)
        self.label_7.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_7, 1, 2, 1, 1)

        self.doubleSpinBox_5 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_5.setObjectName(u"doubleSpinBox_5")
        self.doubleSpinBox_5.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_5.setMinimum(-1.000000000000000)
        self.doubleSpinBox_5.setMaximum(1.000000000000000)
        self.doubleSpinBox_5.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_5, 2, 5, 1, 1)

        self.label_11 = QLabel(self.groupBox)
        self.label_11.setObjectName(u"label_11")
        self.label_11.setFont(font2)
        self.label_11.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_11, 1, 6, 1, 1)

        self.doubleSpinBox = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox.setObjectName(u"doubleSpinBox")
        self.doubleSpinBox.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox.setMinimum(-1.000000000000000)
        self.doubleSpinBox.setMaximum(1.000000000000000)
        self.doubleSpinBox.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox, 2, 4, 1, 1)

        self.doubleSpinBox_7 = QDoubleSpinBox(self.groupBox)
        self.doubleSpinBox_7.setObjectName(u"doubleSpinBox_7")
        self.doubleSpinBox_7.setFocusPolicy(Qt.NoFocus)
        self.doubleSpinBox_7.setMinimum(-1.000000000000000)
        self.doubleSpinBox_7.setMaximum(1.000000000000000)
        self.doubleSpinBox_7.setSingleStep(0.050000000000000)

        self.gridLayout.addWidget(self.doubleSpinBox_7, 4, 5, 1, 1)

        self.pushButton_7 = QPushButton(self.groupBox)
        self.pushButton_7.setObjectName(u"pushButton_7")
        self.pushButton_7.setFocusPolicy(Qt.NoFocus)
        self.pushButton_7.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_7, 4, 2, 1, 1)

        self.pushButton_9 = QPushButton(self.groupBox)
        self.pushButton_9.setObjectName(u"pushButton_9")
        self.pushButton_9.setFont(font)
        self.pushButton_9.setFocusPolicy(Qt.NoFocus)
        self.pushButton_9.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_9, 2, 3, 1, 1)

        self.pushButton_2 = QPushButton(self.groupBox)
        self.pushButton_2.setObjectName(u"pushButton_2")
        self.pushButton_2.setFont(font)
        self.pushButton_2.setFocusPolicy(Qt.NoFocus)
        self.pushButton_2.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_2, 3, 1, 1, 1)

        self.label_5 = QLabel(self.groupBox)
        self.label_5.setObjectName(u"label_5")
        font3 = QFont()
        font3.setFamily(u"\u5fae\u8f6f\u96c5\u9ed1")
        font3.setPointSize(8)
        font3.setBold(True)
        font3.setItalic(False)
        font3.setWeight(75)
        self.label_5.setFont(font3)
        self.label_5.setAlignment(Qt.AlignCenter)

        self.gridLayout.addWidget(self.label_5, 1, 0, 1, 1)

        self.pushButton_15 = QPushButton(self.groupBox)
        self.pushButton_15.setObjectName(u"pushButton_15")
        self.pushButton_15.setFocusPolicy(Qt.NoFocus)
        self.pushButton_15.setCheckable(True)

        self.gridLayout.addWidget(self.pushButton_15, 4, 6, 1, 1)

        self.groupBox_2 = QGroupBox(self.centralwidget)
        self.groupBox_2.setObjectName(u"groupBox_2")
        self.groupBox_2.setGeometry(QRect(9, 19, 551, 191))
        self.gridLayout_2 = QGridLayout(self.groupBox_2)
        self.gridLayout_2.setSpacing(0)
        self.gridLayout_2.setObjectName(u"gridLayout_2")
        self.gridLayout_2.setContentsMargins(0, 40, 0, 8)
        self.label_13 = QLabel(self.groupBox_2)
        self.label_13.setObjectName(u"label_13")

        self.gridLayout_2.addWidget(self.label_13, 1, 0, 1, 1)

        self.label_15 = QLabel(self.groupBox_2)
        self.label_15.setObjectName(u"label_15")

        self.gridLayout_2.addWidget(self.label_15, 3, 0, 1, 1)

        self.label_12 = QLabel(self.groupBox_2)
        self.label_12.setObjectName(u"label_12")

        self.gridLayout_2.addWidget(self.label_12, 0, 0, 1, 1)

        self.label_14 = QLabel(self.groupBox_2)
        self.label_14.setObjectName(u"label_14")

        self.gridLayout_2.addWidget(self.label_14, 2, 0, 1, 1)

        self.label_17 = QLabel(self.centralwidget)
        self.label_17.setObjectName(u"label_17")
        self.label_17.setGeometry(QRect(440, 220, 111, 21))
        font4 = QFont()
        font4.setPointSize(10)
        font4.setBold(True)
        font4.setWeight(75)
        self.label_17.setFont(font4)
        self.groupBox_3 = QGroupBox(self.centralwidget)
        self.groupBox_3.setObjectName(u"groupBox_3")
        self.groupBox_3.setGeometry(QRect(10, 10, 12, 191))
        self.groupBox_3.setStyleSheet(u"border: none")
        self.gridLayout_3 = QGridLayout(self.groupBox_3)
        self.gridLayout_3.setSpacing(0)
        self.gridLayout_3.setObjectName(u"gridLayout_3")
        self.gridLayout_3.setContentsMargins(0, 40, 0, 8)
        self.label_19 = QLabel(self.groupBox_3)
        self.label_19.setObjectName(u"label_19")

        self.gridLayout_3.addWidget(self.label_19, 1, 0, 1, 1)

        self.label_21 = QLabel(self.groupBox_3)
        self.label_21.setObjectName(u"label_21")

        self.gridLayout_3.addWidget(self.label_21, 3, 0, 1, 1)

        self.label_18 = QLabel(self.groupBox_3)
        self.label_18.setObjectName(u"label_18")

        self.gridLayout_3.addWidget(self.label_18, 0, 0, 1, 1)

        self.label_20 = QLabel(self.groupBox_3)
        self.label_20.setObjectName(u"label_20")

        self.gridLayout_3.addWidget(self.label_20, 2, 0, 1, 1)

        UIView.setCentralWidget(self.centralwidget)
        self.groupBox_2.raise_()
        self.groupBox.raise_()
        self.label_17.raise_()
        self.groupBox_3.raise_()
        QWidget.setTabOrder(self.pushButton, self.pushButton_5)
        QWidget.setTabOrder(self.pushButton_5, self.pushButton_9)
        QWidget.setTabOrder(self.pushButton_9, self.doubleSpinBox)
        QWidget.setTabOrder(self.doubleSpinBox, self.doubleSpinBox_5)
        QWidget.setTabOrder(self.doubleSpinBox_5, self.pushButton_13)
        QWidget.setTabOrder(self.pushButton_13, self.pushButton_2)
        QWidget.setTabOrder(self.pushButton_2, self.pushButton_6)
        QWidget.setTabOrder(self.pushButton_6, self.pushButton_10)
        QWidget.setTabOrder(self.pushButton_10, self.doubleSpinBox_2)
        QWidget.setTabOrder(self.doubleSpinBox_2, self.doubleSpinBox_6)
        QWidget.setTabOrder(self.doubleSpinBox_6, self.pushButton_14)
        QWidget.setTabOrder(self.pushButton_14, self.pushButton_3)
        QWidget.setTabOrder(self.pushButton_3, self.pushButton_7)
        QWidget.setTabOrder(self.pushButton_7, self.pushButton_11)
        QWidget.setTabOrder(self.pushButton_11, self.doubleSpinBox_3)
        QWidget.setTabOrder(self.doubleSpinBox_3, self.doubleSpinBox_7)
        QWidget.setTabOrder(self.doubleSpinBox_7, self.pushButton_15)
        QWidget.setTabOrder(self.pushButton_15, self.pushButton_4)
        QWidget.setTabOrder(self.pushButton_4, self.pushButton_8)
        QWidget.setTabOrder(self.pushButton_8, self.pushButton_12)
        QWidget.setTabOrder(self.pushButton_12, self.doubleSpinBox_4)
        QWidget.setTabOrder(self.doubleSpinBox_4, self.doubleSpinBox_8)
        QWidget.setTabOrder(self.doubleSpinBox_8, self.pushButton_16)

        self.retranslateUi(UIView)

        QMetaObject.connectSlotsByName(UIView)
    # setupUi

    def retranslateUi(self, UIView):
        UIView.setWindowTitle(QCoreApplication.translate("UIView", u"OC2TAS", None))
        self.groupBox.setTitle("")
        self.pushButton_4.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.pushButton_5.setText("")
        self.label_4.setText(QCoreApplication.translate("UIView", u"P4", None))
        self.label_6.setText(QCoreApplication.translate("UIView", u"PICKUP", None))
        self.pushButton_12.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.pushButton_3.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.pushButton_11.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.pushButton_16.setText("")
        self.pushButton_8.setText("")
        self.label_2.setText(QCoreApplication.translate("UIView", u"P3", None))
        self.pushButton_13.setText("")
        self.label_3.setText(QCoreApplication.translate("UIView", u"P2", None))
        self.label_9.setText(QCoreApplication.translate("UIView", u"X", None))
        self.label_10.setText(QCoreApplication.translate("UIView", u"Y", None))
        self.pushButton_6.setText("")
        self.pushButton_10.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.label_8.setText(QCoreApplication.translate("UIView", u"DASH", None))
        self.label.setText(QCoreApplication.translate("UIView", u"P1", None))
        self.pushButton_14.setText("")
        self.pushButton.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.label_7.setText(QCoreApplication.translate("UIView", u"USE", None))
        self.label_11.setText(QCoreApplication.translate("UIView", u"EMOTE", None))
        self.pushButton_7.setText("")
        self.pushButton_9.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.pushButton_2.setText(QCoreApplication.translate("UIView", u"00000", None))
        self.label_5.setText("")
        self.pushButton_15.setText("")
        self.groupBox_2.setTitle("")
        self.label_13.setText("")
        self.label_15.setText("")
        self.label_12.setText("")
        self.label_14.setText("")
        self.label_17.setText(QCoreApplication.translate("UIView", u"FRAME 00000", None))
        self.groupBox_3.setTitle("")
        self.label_19.setText("")
        self.label_21.setText("")
        self.label_18.setText("")
        self.label_20.setText("")
    # retranslateUi

