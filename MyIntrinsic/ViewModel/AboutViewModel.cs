using System.Windows.Input;

namespace MyIntrinsic.ViewModel
{
    public partial class AboutViewModel : BaseViewModel
    {
        public string VersionInfo => $"Version {AppInfo.VersionString}";
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://www.intrinsicpower.com/"));
        }

        public ICommand OpenWebCommand { get; }
    }
}