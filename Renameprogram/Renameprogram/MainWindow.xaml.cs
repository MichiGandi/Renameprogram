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
using System.IO;

using Microsoft.Win32;

namespace Renameprogram
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//class atributes
		public List<FileElement> files2 { get; set; }
		//List<string> files = new List<string>();
		//List<string> newFileNames = new List<string>();
		public string EpisodeTitleBlock;
		private bool finishedInitializeComponent = false;
		public static Visibility IsDebug
		{
#if DEBUG
			get { return Visibility.Visible; }
#else
			get { return Visibility.Collapsed; }
#endif
		}

		public MainWindow()
		{
			InitializeComponent();

			files2 = new List<FileElement>();

			DataContext = this;

			finishedInitializeComponent = true;

			EnableButtons();
		}


		//Initialization =========================================================================

		private void Main_Loaded(object sender, RoutedEventArgs e)
		{
			//setting initial values of Combo Boexes, no need since WPF
			//Series_LangugageComboBox.SelectedIndex = 0;
			//Series_LeadingzerosComboBox.SelectedIndex = 1;
			//normal_leadingzerosTypeComboBox.SelectedIndex = 1;
		}


		//Fileselection =========================================================================

		private void SelectFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog selectFileDialog = new OpenFileDialog();
			selectFileDialog.Multiselect = true;

			if (selectFileDialog.ShowDialog() == true)
			{
				AddNewFiles((selectFileDialog.FileNames).ToList());
			}
		}


		private void MoveUpButton_Click(object sender, RoutedEventArgs e)
		{
			MoveFiles(true);
		}


		private void MoveDownButton_Click(object sender, RoutedEventArgs e)
		{
			MoveFiles(false);
		}


		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			foreach (FileElement selectedFile in fileListView.SelectedItems)
			{
				files2.Remove(selectedFile);
			}

			UpdateNewFileNames();

			EnableButtons();
		}


		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			files2.Clear();

			UpdateNewFileNames();
			
			EnableButtons();
		}

		///<summary>Move all selected Files up or down.</summary>
		///<param name="up">Move up when true, otherwise move down.</param>  
		private void MoveFiles(bool upwards)
		{
			//get indizes of selected items
			List<int> selectedIndizes = new List<int>();
			foreach (FileElement item in fileListView.SelectedItems)
			{
				selectedIndizes.Add(files2.IndexOf(item));
			}
			selectedIndizes.Sort();



			//move items up
			if (upwards)
			{
				int stopRange = 0; //already moved items at the top of the list

				for (int i = 0; i < selectedIndizes.Count; i++)
				{
					if (selectedIndizes[i] > stopRange)
					{
						//insert above and remove original
						files2.Insert(selectedIndizes[i] - 1, files2[selectedIndizes[i]]); //insert item one pos higher
						files2.RemoveAt(selectedIndizes[i] + 1); //remove old item

						//swap Objects
						//eElement fileToMove = files2[selectedIndizes[i]];
						//files2[selectedIndizes[i]] = files2[selectedIndizes[i] - 1];
						//files2[selectedIndizes[i] - 1] = fileToMove;

						//update activeFiles
						selectedIndizes[i]--;
					}
					else if (selectedIndizes[i] == stopRange)
					{
						stopRange++;
					}
				}
			}
			//move items down
			else
			{
				int stopRange = 0; //already moved items at the top of the list

				for (int i = selectedIndizes.Count - 1; i >= 0; i--)
				{
					if (selectedIndizes[i] < (files2.Count - stopRange) - 1)
					{
						//insert above and remove original
						files2.Insert(selectedIndizes[i] + 2, files2[selectedIndizes[i]]); //insert item one pos lower
						files2.RemoveAt(selectedIndizes[i]); //remove old item

						//swap Objects
						//eElement fileToMove = files2[selectedIndizes[i]];
						//files2[selectedIndizes[i]] = files2[selectedIndizes[i] + 1];
						//files2[selectedIndizes[i] + 1] = fileToMove;

						//update activeFiles
						selectedIndizes[i]++;
					}
					else if (selectedIndizes[i] == (files2.Count - stopRange) - 1)
					{
						stopRange++;
					}
				}
			}
			

			//refresh Selection
			fileListView.UnselectAll();

			fileListView.SelectedItems.Clear();
			for (int i = 0; i < selectedIndizes.Count; i++)
			{
				fileListView.SelectedItems.Add(files2[selectedIndizes[i]]); //should work, but doesn't
			}

			UpdateNewFileNames();
		}


		//fileListView DragDrop event
		private void FileListView_Drop(object sender, DragEventArgs e)
		{
			List<string> filesToAdd = ((string[])e.Data.GetData(DataFormats.FileDrop, true)).ToList();

			if (sortAlphabeticCheckBox.IsChecked == true)
			{
				filesToAdd.Sort();
			}

			AddNewFiles(filesToAdd);
		}



		//General Functions =========================================================================

		///<summary>Check for empty fileList or newFileNameTextBox and enabble or disable buttons.</summary>
		private void EnableButtons()
		{
			bool activate = files2.Count() > 0;

			moveUpButton.IsEnabled = activate;
			moveDownButton.IsEnabled = activate;
			deleteButton.IsEnabled = activate;
			clearButton.IsEnabled = activate;
			normal_renameButton.IsEnabled = activate && (normal_newFileNameTextBox.Text != "");
		}


		///<summary>Add new files to the List.</summary>
		////<param name="newFiles">The list to add.</param>  
		private void AddNewFiles(List<string> newFiles)
		{
			foreach (string fileToAdd in newFiles)
			{
				files2.Add(new FileElement(fileToAdd));
			}

			RemoveMultibleAccuringFiles(files2);

			UpdateNewFileNames();

			EnableButtons();
		}


		///<summary>Remove doubled files in a list.</summary>
		////<param name="fileList">The list to remove doubled files from.</param>  
		private void RemoveMultibleAccuringFiles(List<FileElement> fileList)
		{
			for (int i = fileList.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < i; j++)
				{
					if (fileList[i] == fileList[j])
					{
						fileList.RemoveAt(i); //remove last occuring element
						break;
					}
				}
			}
		}

		///<summary>Remove doubled files in a list.</summary>
		////<param name="fileList">The list to remove doubled files from.</param>
		private void UpdateNewFileNames()
		{
			if (ReferenceEquals(mainTabControl.SelectedItem, normalTabItem))
			{
				for (int i = 0; i < files2.Count; i++)
				{
					Normal_updateFileName(files2[i], i);
				}
			}
			else if (ReferenceEquals(mainTabControl.SelectedItem, seriesTabItem))
			{
				for (int i = 0; i < files2.Count; i++)
				{
					//files[i].newFilename = Series_updateFileName(files[i].filename);
				}
			}
			else if (ReferenceEquals(mainTabControl.SelectedItem, replaceTabItem))
			{
				for (int i = 0; i < files2.Count; i++)
				{
					//files[i].newFilename = Replace_updateFileName(files[i].filename);
				}
			}

			fileListView.Items.Refresh();
		}


		private void ControlElementChanged(object sender, EventArgs e)
		{
			if (finishedInitializeComponent)
			{
				//if sender is a TextBox
				if (!ReferenceEquals(sender as TextBox, null))
				{
					((TextBox)sender).Text = RemoveIllegalChars(((TextBox)sender).Text);
					EnableButtons();
				}

				UpdateNewFileNames();
			}
		}



		//Normal Tab =========================================================================

		private void Normal_renameButton_Click(object sender, RoutedEventArgs e)
		{
			//rename files

			for (int i = 0; i < files2.Count; i++)
			{
				System.IO.File.Move(files2[i].GetFullPath(), System.IO.Path.Combine(files2[i].directory, files2[i].newFilename));
			}

			//Clear file list
			files2.Clear();
			UpdateNewFileNames();

			//Show confirmation MessageBox
			MessageBox.Show("Dateinamen wurden erfolgreich geändert", "Abgeschlossen");
		}


		///<summary>Returns the Filename of File number [Counter].</summary>
		///<param name="file">the string to remove illegal chars from.</param>
		private void Normal_updateFileName(FileElement file, int pos)
		{
			string newFileName; //der neue Name der einer Datei zugewiesen werden soll
			string counterString; //Zeichenfolge der Dateinummer + Vornullen

			//calculate counter
			counterString = ((int)normal_counterStartvalueNumericUpDown.Value + pos * (int)normal_counterStepsizeNumericUpDown.Value).ToString();
			while (counterString.Length < normal_counterLeadingzerosLengthNumericUpDown.Value && !ReferenceEquals((ComboBoxItem)normal_counterLeadingzerosTypeComboBox.SelectedItem, normal_counterLeadingzerosTypeNoneComboBoxItem))
			{
				if (ReferenceEquals((ComboBoxItem)normal_counterLeadingzerosTypeComboBox.SelectedItem, normal_counterLeadingzerosTypeZeroComboBoxItem))
				{
					counterString = "0" + counterString;
				}
				else if (ReferenceEquals((ComboBoxItem)normal_counterLeadingzerosTypeComboBox.SelectedItem, normal_counterLeadingzerosTypeSpaceComboBoxItem))
				{
					counterString = " " + counterString;
				}
				else if (ReferenceEquals((ComboBoxItem)normal_counterLeadingzerosTypeComboBox.SelectedItem, normal_counterLeadingzerosTypeUnderscoreComboBoxItem))
				{
					counterString = "_" + counterString;
				}
			}

			//Dateinamen Ermitteln
			newFileName = normal_newFileNameTextBox.Text;

			//[NAME] ersetzen
			while (newFileName.IndexOf("[NAME]") != -1)
			{
				newFileName = newFileName.Replace("[NAME]", file.GetFileNameWithoutExtension());
			}

			//[C] ersetzen
			while (newFileName.IndexOf("[C]") != -1)
			{
				newFileName = newFileName.Replace("[C]", counterString);
			}

			//Dateiendung hinzufügen
			newFileName += file.GetExtension();

			file.newFilename = newFileName;
		}


		//General =========================================================================

		///<summary>Remove chars, that are illegal for file names from a string.</summary>
		////<param name="fileList">the string to remove illegal chars from.</param>
		public string RemoveIllegalChars(string stringToRemoveFrom)
		{
			//unzulässige zeichen entfernen:  \ / : * ? " < > | Tabstop
			stringToRemoveFrom = stringToRemoveFrom.Replace("\\", ""); // \
			stringToRemoveFrom = stringToRemoveFrom.Replace("/", "");  // /
			stringToRemoveFrom = stringToRemoveFrom.Replace(":", "");  // :
			stringToRemoveFrom = stringToRemoveFrom.Replace("*", "");  // *
			stringToRemoveFrom = stringToRemoveFrom.Replace("?", "");  // ?
			stringToRemoveFrom = stringToRemoveFrom.Replace("\"", ""); // "
			stringToRemoveFrom = stringToRemoveFrom.Replace("<", "");  // <
			stringToRemoveFrom = stringToRemoveFrom.Replace(">", "");  // >
			stringToRemoveFrom = stringToRemoveFrom.Replace("|", "");  // |
			stringToRemoveFrom = stringToRemoveFrom.Replace("\t", ""); // Tabstop

			return stringToRemoveFrom;
		}

		//Debug =========================================================================
		private void DebugButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(files2[0].GetFullPath());
		}
	}
}
