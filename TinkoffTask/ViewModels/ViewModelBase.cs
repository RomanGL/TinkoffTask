using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TinkoffTask.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual Task OnNavigatedToAsync(object parameter) => Task.CompletedTask;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
