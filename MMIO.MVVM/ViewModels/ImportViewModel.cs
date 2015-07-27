using Livet.Messaging.Windows;
using SharpDXScene;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MMIO.ViewModels
{
    class ImportViewModel : ViewModelBase
    {
        bool m_isDone;
        public bool IsDone
        {
            get { return m_isDone; }
            set
            {
                if (m_isDone == value) return;
                m_isDone = value;
                RaisePropertyChanged(() => this.IsDone);
            }
        }

        LoadParams m_loadParams;
        public LoadParams LoadParams
        {
            get {
                if (m_loadParams == null)
                {
                    m_loadParams = new LoadParams(1.0f);
                }
                return m_loadParams;
            }
        }

        public ImportViewModel(LoadParams param)
        {
            m_loadParams = param;
        }

        public IEnumerable<Axis> Axises
        {
            get
            {
                return new[]
                {
                    Axis.None,
                    Axis.X,
                    Axis.Y,
                    Axis.Z,
                };
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

                        IsDone = true;

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
