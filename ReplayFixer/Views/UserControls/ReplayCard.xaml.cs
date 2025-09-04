using ReplayFixer.Library.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReplayFixer.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ReplayCard.xaml
    /// </summary>
    public partial class ReplayCard : UserControl
    {
        public static readonly DependencyProperty ReplayProperty = DependencyProperty.Register("Replay", typeof(Replay), typeof(ReplayCard));

        public Replay Replay
        {
            get { return (Replay)GetValue(ReplayProperty); }
            set { SetValue(ReplayProperty, value); }
        }
        public ReplayCard()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
