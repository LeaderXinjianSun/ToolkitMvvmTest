using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitMvvmTest.Share;

namespace ToolkitMvvmTest.ViewModels
{
    public class SubWindow1ViewModel : ObservableObject
    {
        private string? receiveText;

        public string? ReceiveText
        {
            get => receiveText;
            set => SetProperty(ref receiveText, value);
        }
        public SubWindow1ViewModel()
        {
            WeakReferenceMessenger.Default.Register<TMessage>(this, Receive);
            WeakReferenceMessenger.Default.Register<TMessage,string>(this, "AAA", ReceiveWithToken);
        }
        public void Receive(object recipient, TMessage message)
        {
            ReceiveText = $"Receive Type:{message.Type} Content:{message.Content}";
        }
        public void ReceiveWithToken(object recipient, TMessage message)
        {
            ReceiveText = $"ReceiveWithToken Type:{message.Type} Content:{message.Content}";
        }
    }
}
