using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hangman.WPF.ViewModels
{
    // Den här klassen implementerar ett interface som låter oss
    // meddela UI:t när en egenskap har ändrats.
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}