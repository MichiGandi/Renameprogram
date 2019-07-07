using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renameprogram
{
	public class FileElement
	{
		//Atributes
		public string directory { get; set; }
		public string filename { get; set; }
		public string newFilename { get; set; }

		//Construcor
		public FileElement(string fullPath)
		{
			directory = System.IO.Path.GetDirectoryName(fullPath);
			filename = System.IO.Path.GetFileName(fullPath);
			newFilename = "-";
		}

		//Method
		public string GetFullPath()
		{
			return System.IO.Path.Combine(directory, filename);
		}

		public string GetFileNameWithoutExtension()
		{
			return System.IO.Path.GetFileNameWithoutExtension(GetFullPath());
		}


		public string GetExtension()
		{
			return System.IO.Path.GetExtension(GetFullPath());
		}

		public static bool operator ==(FileElement file1, FileElement file2)
		{
			if (ReferenceEquals(file1, file2))
			{
				return true;
			}
			else if (ReferenceEquals(file1, null) || ReferenceEquals(file2, null))
			{
				return false;
			}
			else
			{
				return (file1.directory == file2.directory) && (file1.filename == file2.filename);
			}
			
		}

		public static bool operator !=(FileElement file1, FileElement file2)
		{
			return !(file1 == file2);
		}

		override public bool Equals(object obj)
		{
			return this == obj as FileElement;
		}
		public override int GetHashCode()
		{
			return (directory + filename).GetHashCode();
		}
	}
}
