using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AstRevitTool.ViewModels
{
    public sealed class AstRevitToolViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}