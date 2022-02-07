using Prism.Commands;
using Prism.Mvvm;

namespace BlankCoreApp1.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "My Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private DelegateCommand testButtonCommand;
        public DelegateCommand TestButtonCommand =>
            testButtonCommand ?? (testButtonCommand = new DelegateCommand(ExecuteTestButtonCommand));

        void ExecuteTestButtonCommand()
        {
            
        }
        public MainWindowViewModel()
        {

        }
    }
}
