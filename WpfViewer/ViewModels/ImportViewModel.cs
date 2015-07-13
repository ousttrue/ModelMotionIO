using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WpfViewer.ViewModels
{
    class ImportViewModel: ViewModelBase
    {
        Single m_scale = 1.0f;
        public Single Scaling
        {
            get { return m_scale; }
            set
            {
                if (m_scale == value) return;
                m_scale = value;
                RaisePropertyChanged(() => this.Scaling);
            }
        }

        public IEnumerable<Models.Axis> Axises
        {
            get
            {
                return new[]
                {
                    Models.Axis.None,
                    Models.Axis.X,
                    Models.Axis.Y,
                    Models.Axis.Z,
                };
            }
        }

        Models.Axis m_flipAxis = Models.Axis.None;
        public Models.Axis FlipAxis
        {
            get { return m_flipAxis; }
            set
            {
                if (m_flipAxis == value) return;
                m_flipAxis = value;
                RaisePropertyChanged(() => this.FlipAxis);
            }
        }

        bool m_yRotate = false;
        public Boolean YRotate
        {
            get { return m_yRotate; }
            set
            {
                if (m_yRotate == value) return;
                m_yRotate = value;
                RaisePropertyChanged(() => this.YRotate);
            }
        }

        Livet.Commands.ViewModelCommand m_saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                {
                    m_saveCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                    /*
                    Name = InputName;
                    Birthday = DateTime.Parse(InputBirthday);
                    Memo = InputMemo;

                    if (!m_member.IsIncludedInMainCollection())
                    {
                        m_member.AddThisToMainCollection();
                    }
                    */

                    // Viewに画面遷移用メッセージを送信しています。
                    // Viewは対応するメッセージキーを持つInteractionTransitionMessageTriggerでこのメッセージを受信します。
                    Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
                    });
                }
                return m_saveCommand;
            }
        }

        Livet.Commands.ViewModelCommand m_cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (m_cancelCommand == null)
                {
                    m_cancelCommand = new Livet.Commands.ViewModelCommand(() =>
                    {
                        // 入力情報初期化
                        //InitializeInput();

                        // Viewに画面遷移用メッセージを送信しています。
                        // Viewは対応するメッセージキーを持つInteractionTransitionMessageTriggerでこのメッセージを受信します。
                        Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
                    });
                }
                return m_cancelCommand;
            }
        }
    }
}
