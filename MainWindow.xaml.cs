using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;

namespace CardFilePBX
{
	///	14. На междугородной телефонной станции картотека абонентов, содержащая сведения о телефонах и их владельцах, организована в виде линейного списка.
	/// Написать программу, которая:
	///	- обеспечивает начальное формирование картотеки в виде линейного списка;
	///	- производит вывод всей картотеки;
	///	- вводит номер телефона и время разговора;
	///	- выводит извещение на оплату телефонного разговора.
	///	Программа должна обеспечивать диалог с помощью меню и контроль ошибок при вводе.
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;

			
		}

		private void AbonentAddExpander_Collapsed(object sender, RoutedEventArgs e)
		{
			var expander = sender as Expander;
			AbonentAddRow.Height = new GridLength(30, GridUnitType.Pixel);
		}

		private void AbonentAddExpander_Expanded(object sender, RoutedEventArgs e)
		{
			var expander = sender as Expander;
			AbonentAddRow.Height = GridLength.Auto;
		}
	}
}