using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ParallelLab3.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private IEventAggregator _eventAggregator;
        public ShellViewModel()
        {
            _eventAggregator = new EventAggregator();
            Task.Run(async () =>
            {
                await ActivateItemAsync(new MainViewModel(_eventAggregator));
            });
        }
    }
}
