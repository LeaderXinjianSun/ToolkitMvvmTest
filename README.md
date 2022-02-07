~~Prism还没有支持dotnet6~~，找到一个支持dotnet的MVVM框架[Microsoft.Toolkit.Mvvm](https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)。B站视频教程: [WPF应用开发中的轻型级MVVM框架-MVVM Toolkit](https://www.bilibili.com/video/BV1Zb4y1b77i?spm_id_from=333.1007.top_right_bar_window_custom_collection.content.click)。
后来我发现Prism是支持dotnet core框架的，只是[Prism Template Pack](https://marketplace.visualstudio.com/items?itemName=BrianLagunas.PrismTemplatePack)目前只支持vs2019。
#### 数据绑定
```xml
    <Grid>
        <StackPanel>
            <TextBlock Text="{Binding DataBinding1,FallbackValue=123}"></TextBlock>
            <ListBox ItemsSource="{Binding CollectionData}"></ListBox>
        </StackPanel>
    </Grid>
```
```csharp
public class MainWindowViewModel : ObservableObject
    {
        #region 属性
        private string dataBinding1;

        public string DataBinding1//数据绑定
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

        #endregion
        #region 构造函数
        public MainWindowViewModel()
        {
            CollectionData.Add("123");
            CollectionData.Add("456");
            Task.Run(() => {
                System.Threading.Thread.Sleep(3000);
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
```
#### 方法绑定

   - 这里写了，测试发现，如果不传参数，obj可为null，即不带参数的也可以用[IRelayCommand<T>](https://docs.microsoft.com/en-us/dotnet/api/microsoft.toolkit.mvvm.input.IRelayCommand-1)处理。
```csharp
        #region 方法绑定
        public ICommand Button1ClickCommand { get; }
        #endregion
        #region 构造函数
        public MainWindowViewModel()
        {
            Button1ClickCommand = new RelayCommand<object>(DoSomething);
        }
        #endregion
        #region 方法
        private void DoSomething(object? obj)
        {
            CollectionData.Add(DateTime.Now.ToString("HH:mm:ss"));
        }
```

   - 控件原生事件的绑定，需要添加一个nuget包[Microsoft.Xaml.Behaviors.Wpf](https://www.nuget.org/packages/Microsoft.Xaml.Behaviors.Wpf/)。
```xml
xmlns:i="http://schemas.microsoft.com/xaml/behaviors"    
<i:Interaction.Triggers>
    <i:EventTrigger EventName="Loaded">
        <i:InvokeCommandAction Command="{Binding AppLoadedEventCommand}" />
    </i:EventTrigger>
    <i:EventTrigger EventName="Closed">
        <i:InvokeCommandAction Command="{Binding AppClosedEventCommand}" />
    </i:EventTrigger>
</i:Interaction.Triggers>
```

   - 关于AsyncRelayCommand，就是多了一个异步方法的写法（只能传Task类型的进去）
#### 消息

- 不同的模块（ViewModel），可以通过Messager进行通信。
   - 消息的一般写法，和带Token的用法。
```csharp
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
    
//以下是发送
        private void DoSomething(object? obj)
        {
            WeakReferenceMessenger.Default.Send(new TMessage { Type = "Test", Content = DateTime.Now.ToString("HH:mm:ss") });
        }
        private void DoSomething2()
        {
            WeakReferenceMessenger.Default.Send(new TMessage { Type = "Test", Content = DateTime.Now.ToString("HH:mm:ss") }, "AAA");
        }
```

   - 自动注册→ObservableRecipient。PS: 需要this.IsActive = true;
```csharp
    public class SubWindow1ViewModel : ObservableRecipient, IRecipient<TMessage>
    {
        private string? receiveText;

        public string? ReceiveText
        {
            get => receiveText;
            set => SetProperty(ref receiveText, value);
        }
        public SubWindow1ViewModel()
        {
            this.IsActive = true;
        }
        public void Receive(TMessage message)
        {
            ReceiveText = $"Type:{message.Type} Content:{message.Content}";
        }
    }
```
#### IoC处理

- Toolkit.Mvvm本身并不包含IoC内容，但是可以通过Microsoft.Extensions.DependencyInjection Nuget包实现依赖注入功能
- 在App里面注册服务
```csharp
public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
        }
        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }
        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<IFileService, FilesService>();
            //services.AddSingleton<ISettingsService, SettingsService>();
            //services.AddSingleton<IClipboardService, ClipboardService>();
            //services.AddSingleton<IShareService, ShareService>();
            //services.AddSingleton<IEmailService, EmailService>();

            // Viewmodels
            services.AddTransient<MainWindowViewModel>();

            return services.BuildServiceProvider();
        }
    }
```

- 使用服务
   - 服务
   - Viewmodels(构造函数)
```csharp
//使用服务        
public MainWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
            Button1ClickCommand = new RelayCommand<object>(DoSomething);
            Button2ClickCommand = new RelayCommand(DoSomething2);
            AppLoadedEventCommand = new AsyncRelayCommand(AppLoaded);
        }
```
```csharp
//使用依赖注入连接viewmodel        
public MainWindow()
        {
            InitializeComponent();
            this.DataContext = App.Current.Services.GetService(typeof(MainWindowViewModel));
            //this.DataContext = App.Current.Services.GetService<MainWindowViewModel>();
        }
```
#### .net core项目引用dll

- 添加程序集

![image.png](https://cdn.nlark.com/yuque/0/2022/png/22023506/1644225355303-0ff5d26e-b9c5-4a05-b9b7-ed5f0d77e27e.png#clientId=u4ce9d044-9ddc-4&crop=0&crop=0&crop=1&crop=1&from=paste&height=434&id=u2b8a57d2&margin=%5Bobject%20Object%5D&name=image.png&originHeight=543&originWidth=786&originalType=binary&ratio=1&rotation=0&showTitle=false&size=20978&status=done&style=none&taskId=u55cd0536-62b1-4ca7-a30b-eff14f251da&title=&width=628.8)
![image.png](https://cdn.nlark.com/yuque/0/2022/png/22023506/1644225389786-781e472c-31c8-4bce-afa6-6d6d6b862fbf.png#clientId=u4ce9d044-9ddc-4&crop=0&crop=0&crop=1&crop=1&from=paste&height=201&id=ud30b58f2&margin=%5Bobject%20Object%5D&name=image.png&originHeight=251&originWidth=242&originalType=binary&ratio=1&rotation=0&showTitle=false&size=11694&status=done&style=none&taskId=u4d4e11f8-60ef-456d-9f4f-52796c592e7&title=&width=193.6)

- 正常写控件代码
```xml
xmlns:halconviewer="clr-namespace:HalconViewer;assembly=HalconViewer"
<halconviewer:ImageViewer
                          AppendHMessage="{Binding CameraAppendHMessage0}"
                          AppendHObject="{Binding CameraAppendHObject0}"
                          AutoRepaint="True"
                          GCStyle="{Binding CameraGCStyle0}"
                          Image="{Binding CameraIamge0}"
                          ROIList="{Binding CameraROIList0}"
                          Repaint="{Binding CameraRepaint0}" />   
```

- 如何使用System.Windows.Forms？
   - 编辑项目文件ToolkitMvvmTest.csprog
   - 开启对应功能
```xml
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
```

- 测试可用(打开图片)

![image.png](https://cdn.nlark.com/yuque/0/2022/png/22023506/1644225213765-ed463da8-8830-4338-85e8-3317b03eed10.png#clientId=u4ce9d044-9ddc-4&crop=0&crop=0&crop=1&crop=1&from=paste&height=474&id=udb3157cb&margin=%5Bobject%20Object%5D&name=image.png&originHeight=593&originWidth=786&originalType=binary&ratio=1&rotation=0&showTitle=false&size=112046&status=done&style=none&taskId=u73359398-d985-4a36-8dff-fed68715ea8&title=&width=628.8)
