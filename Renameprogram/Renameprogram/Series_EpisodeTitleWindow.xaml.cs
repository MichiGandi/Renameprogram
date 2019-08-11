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
using System.Windows.Shapes;

namespace Renameprogram
{
	/// <summary>
	/// Interaktionslogik für Series_EpisodeTitleWindow.xaml
	/// </summary>
	public partial class Series_EpisodeTitleWindow : Window
	{
		//global variables
		public MainWindow MainWindowInstance;

		public Series_EpisodeTitleWindow(MainWindow MainWindowInstance)
		{
			this.MainWindowInstance = MainWindowInstance;
			InitializeComponent();
		}

		private void Series_EpisodeTitleWindow_Loaded(object sender, RoutedEventArgs e)
		{
			series_episodeTitleBlockTextBox.Text = MainWindowInstance.series_episodeTitleBlock;
		}

		private void Series_episodeTitleBlockTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//remove illegal characters: \ / : * ? " < > | Tabstop
			series_episodeTitleBlockTextBox.Text = MainWindowInstance.RemoveIllegalChars(series_episodeTitleBlockTextBox.Text);
		}

		private void Series_clearButton_Click(object sender, RoutedEventArgs e)
		{
			series_episodeTitleBlockTextBox.Text = "";
		}

		private void Series_abbroatButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Series_oKButton_Copy_Click(object sender, RoutedEventArgs e)
		{
			MainWindowInstance.series_episodeTitleBlock = series_episodeTitleBlockTextBox.Text;
			Close();
		}
	}
}
