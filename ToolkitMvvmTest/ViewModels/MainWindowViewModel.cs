using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolkitMvvmTest.Services;
using ToolkitMvvmTest.Share;
using ToolkitMvvmTest.Views;

namespace ToolkitMvvmTest.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region 属性
        private string? dataBinding1;

        public string? DataBinding1//数据绑定
        {
            get => dataBinding1;
            set => SetProperty(ref dataBinding1, value);
        }
        private ObservableCollection<string> collectionData = new ObservableCollection<string>();

        public ObservableCollection<string> CollectionData//集合的绑定
        {
            get => collectionData;
            set => SetProperty(ref collectionData, value);
        }

        private readonly IFileService _fileService;

        #endregion
        #region 方法绑定
        public ICommand Button1ClickCommand { get; }
        public ICommand Button2ClickCommand { get; }
        public ICommand AppLoadedEventCommand { get; }
        #endregion
        #region 构造函数
        public MainWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
            Button1ClickCommand = new RelayCommand<object>(DoSomething);
            Button2ClickCommand = new RelayCommand(DoSomething2);
            AppLoadedEventCommand = new AsyncRelayCommand(AppLoaded);
        }

        #endregion
        #region 方法
        private void DoSomething(object? obj)
        {
            WeakReferenceMessenger.Default.Send(new TMessage { Type = "Test", Content = DateTime.Now.ToString("HH:mm:ss") });
        }
        private void DoSomething2()
        {
            WeakReferenceMessenger.Default.Send(new TMessage { Type = "Test", Content = DateTime.Now.ToString("HH:mm:ss") }, "AAA");
        }
        private Task AppLoaded()
        {
            SubWindow1 subWindow1 = new SubWindow1();
            subWindow1.Show();
            return Task.Run(() =>
            {
                //System.Threading.Thread.Sleep(3000);
                Task.Delay(3000).GetAwaiter().GetResult();
                DataBinding1 = "3456";
                //多线程更新UI内容
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    CollectionData.Add("789");
                }));

            });
        }
        #endregion

    }
}
