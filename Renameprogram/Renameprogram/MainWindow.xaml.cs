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


		//General =========================================================================

		private void FileListView_Drop(object sender, DragEventArgs e)
		{
			List<string> filesToAdd = ((string[])e.Data.GetData(DataFormats.FileDrop, true)).ToList();

			if (sortAlphabeticCheckBox.IsChecked == true)
			{
				filesToAdd.Sort();
			}

			AddNewFiles(filesToAdd);
		}


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
		

		///<summary>Check for empty fileList or newFileNameTextBox and enabble or disable buttons.</summary>
		private void EnableButtons()
		{
			bool activate = files2.Count() > 0;

			moveUpButton.IsEnabled = activate;
			moveDownButton.IsEnabled = activate;
			deleteButton.IsEnabled = activate;
			clearButton.IsEnabled = activate;
			normal_renameButton.IsEnabled = activate && (normal_newFileNameTextBox.Text != "");
			series_renameButton.IsEnabled = activate && (series_newFileNameSeriesNameTextBox.Text != "");
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
					Series_updateFileName(files2[i], i);
				}
			}
			else if (ReferenceEquals(mainTabControl.SelectedItem, replaceTabItem))
			{
				for (int i = 0; i < files2.Count; i++)
				{
					//Replace_updateFileName(files2[i], i);
				}
			}

			fileListView.Items.Refresh();
		}


		private void RenameButton_Click(object sender, RoutedEventArgs e)
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



		//Normal Tab =========================================================================

		///<summary>Updates the newFilemane Atrubute of the given FileElement.</summary>
		///<param name="file">the FileElement to update newFilename.</param>
		///<param name="pos">the position index in the files List.</param>
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



		//Series Tab =========================================================================

		static bool series_EnableEpisodeTitleControlElementsenabled = true;
		///<summary>Check for checked CheckBoxes and enable or disable episode title control elements.</summary>
		private void Series_EnableEpisodeTitleControlElements(object sender, RoutedEventArgs e)
		{
			if (series_EnableEpisodeTitleControlElementsenabled && finishedInitializeComponent)
			{
				series_EnableEpisodeTitleControlElementsenabled = false;

				if (series_newFileNameMinusCheckBox.IsChecked == true)
				{
					//enable CheckBox
					series_newFileNameEpisodeTitleCheckBox.IsEnabled = true;
				}
				else
				{
					//disable CheckBox
					series_newFileNameEpisodeTitleCheckBox.IsChecked = false;
					series_newFileNameEpisodeTitleCheckBox.IsEnabled = false;
				}

				if (series_newFileNameEpisodeTitleCheckBox.IsChecked == true)
				{
					//enable RadioButtons
					series_episodeTitleSourceRandarisRadioButton.IsEnabled = true;
					series_episodeTitleSourceBSRadioButton.IsEnabled = true;
					series_episodeTitleSourceAniSearchRadioButton.IsEnabled = true;
				}
				else
				{
					//disable RadioButtons
					series_episodeTitleSourceRandarisRadioButton.IsChecked = false;
					series_episodeTitleSourceRandarisRadioButton.IsEnabled = false;
					series_episodeTitleSourceBSRadioButton.IsChecked = false;
					series_episodeTitleSourceBSRadioButton.IsEnabled = false;
					series_episodeTitleSourceAniSearchRadioButton.IsChecked = false;
					series_episodeTitleSourceAniSearchRadioButton.IsEnabled = false;
				}

				if ((series_episodeTitleSourceRandarisRadioButton.IsChecked == true) || (series_episodeTitleSourceBSRadioButton.IsChecked == true) || (series_episodeTitleSourceAniSearchRadioButton.IsChecked == true))
				{
					//enable Button
					series_episodeTitleButton.IsEnabled = true;
				}
				else
				{
					//disable Button
					series_episodeTitleButton.IsEnabled = false;
				}

				UpdateNewFileNames();

				series_EnableEpisodeTitleControlElementsenabled = true;
			}
		}


		///<summary>Updates the newFilemane Atrubute of the given FileElement.</summary>
		///<param name="file">the FileElement to update newFilename.</param>
		///<param name="pos">the position index in the files List.</param>
		private void Series_updateFileName(FileElement file, int pos)
		{
			string fileName; //der neue Name der einer Datei zugewiesen werden soll
			int episodeCounter = 0; //counter der die Episodennummer beschreibt (wird für den Episodentitel benötigt)
			string episodeNumber; //Zeicenfolde der Episodennummer + Vornullen
			string LanguageString; //Zeicenfolde des Sprachenzuweisung
			string episodeTitle = ""; //Zeicenfolde des Namens der Episode

			//calculate episodeNumber
			episodeCounter = pos * (int)series_counterStepsizeNumericUpDown.Value + (int)series_counterStartvalueNumericUpDown.Value;
			episodeNumber = episodeCounter.ToString();

			while (episodeNumber.Length < series_counterLeadingzerosLengthNumericUpDown.Value && !ReferenceEquals((ComboBoxItem)series_counterLeadingzerosTypeComboBox.SelectedItem, series_counterLeadingzerosTypeNoneComboBoxItem))
			{
				if (ReferenceEquals((ComboBoxItem)series_counterLeadingzerosTypeComboBox.SelectedItem, series_counterLeadingzerosTypeZeroComboBoxItem))
				{
					episodeNumber = "0" + episodeNumber;
				}
				else if (ReferenceEquals((ComboBoxItem)series_counterLeadingzerosTypeComboBox.SelectedItem, series_counterLeadingzerosTypeSpaceComboBoxItem))
				{
					episodeNumber = " " + episodeNumber;
				}
				else if (ReferenceEquals((ComboBoxItem)series_counterLeadingzerosTypeComboBox.SelectedItem, series_counterLeadingzerosTypeUnderscoreComboBoxItem))
				{
					episodeNumber = "_" + episodeNumber;
				}
			}

			//calculate Language
			if (ReferenceEquals((ComboBoxItem)series_newFileNameLangugageComboBox.SelectedItem, series_newFileNameLangugageNoneComboBoxItem))
			{
				LanguageString = "";
			}
			else
			{
				LanguageString = " " + ((ComboBoxItem)series_newFileNameLangugageComboBox.SelectedItem).Content;				
			}

			//calculate EpisodeTitle
			if ((series_newFileNameEpisodeTitleCheckBox.IsChecked == true) && EpisodeTitleBlock != null)
			{
				//Randaris
				if (series_episodeTitleSourceRandarisRadioButton.IsChecked == true)
				{
					if (EpisodeTitleBlock.IndexOf("Episode " + episodeCounter.ToString() + "  ") != -1)
					{
						episodeTitle = EpisodeTitleBlock.Remove(0, EpisodeTitleBlock.IndexOf("Episode " + episodeCounter.ToString() + " ") + 10 + episodeCounter.ToString().Length);

						if (episodeTitle.IndexOf(Environment.NewLine) != -1)
						{
							episodeTitle = episodeTitle.Remove(episodeTitle.IndexOf(Environment.NewLine));
						}
					}
					else
					{
						episodeTitle = "";
					}
				}

				//Ani Search
				else if (series_episodeTitleSourceAniSearchRadioButton.IsChecked == true)
				{
					if (EpisodeTitleBlock.IndexOf(episodeCounter.ToString() + "\t") != -1)
					{
						episodeTitle = EpisodeTitleBlock.Remove(0, EpisodeTitleBlock.IndexOf(episodeCounter.ToString() + "\t"));
						episodeTitle = episodeTitle.Remove(0, episodeTitle.IndexOf("\r\n") + 2); //remove "Episode"-Column
						if (episodeTitle.IndexOf("Filler\t\r\n") == 0)
						{
							episodeTitle = episodeTitle.Remove(0, episodeTitle.IndexOf("\r\n") + 2); //remove "filler" banner
						}
						episodeTitle = episodeTitle.Remove(0, episodeTitle.IndexOf("\r\n") + 2); //remove "Laufzeit"-Column
						episodeTitle = episodeTitle.Remove(0, episodeTitle.IndexOf("\r\n") + 2); //remove ""-Column
						episodeTitle = episodeTitle.Remove(0, 2); //remove flag

						if (episodeTitle.IndexOf(Environment.NewLine) != -1)
						{
							episodeTitle = episodeTitle.Remove(episodeTitle.IndexOf("\r\n"));
						}
					}
					else
					{
						episodeTitle = "";
					}
				}


				//BS
				else if (series_episodeTitleSourceBSRadioButton.IsChecked == true)
				{
					if (EpisodeTitleBlock.IndexOf(episodeCounter.ToString() + " " + "\t") != -1)
					{
						episodeTitle = EpisodeTitleBlock.Remove(0, EpisodeTitleBlock.IndexOf(episodeCounter.ToString() + " " + "\t") + 2 + episodeCounter.ToString().Length);

						if (episodeTitle.IndexOf(" " + "\t") != -1)
						{
							episodeTitle = episodeTitle.Remove(episodeTitle.IndexOf("\t"));
						}
					}
					else
					{
						episodeTitle = "";
					}
				}
				else
				{
					episodeTitle = "";
				}
			}
			else
			{
				episodeTitle = "";
			}

			//join fileName
			fileName = series_newFileNameSeriesNameTextBox.Text;
			fileName += " Folge ";
			fileName += episodeNumber;
			fileName += LanguageString;
			if (series_newFileNameMinusCheckBox.IsChecked == true)
			{
				fileName += " - ";
			}
			fileName += episodeTitle;

			//remove illegal Characters
			fileName = RemoveIllegalChars(fileName);

			//Dateiendung hinzufügen
			//fileName += System.IO.Path.GetExtension(file.GetFullPath());

			file.newFilename = fileName;
		}



		//Debug =========================================================================
		private void DebugButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(files2[0].GetFullPath());
		}
	}
}
