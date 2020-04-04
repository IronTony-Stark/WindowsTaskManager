using System.Windows.Controls;
using TaskManager.Tools.Navigation;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskGridView : UserControl, INavigatable
    {
        public TaskGridView()
        {
            InitializeComponent();
            DataContext = new TaskGridViewModel();
        }
    }
}