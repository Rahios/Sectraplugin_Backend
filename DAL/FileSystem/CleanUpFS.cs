using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;

namespace DAL.FileSystem
{
	public class CleanUpFS : ICleanUpFS
	{
		// A T T R I B U T E S
		private string PATH;

		// D E P E N D E N C I E S   I N J E C T I O N
		public CleanUpFS(string directoryPath)
		{
			PATH = directoryPath;
		}

		// M E T H O D S 
		public bool CleanUp()
		{
			try
			{
				if(Directory.Exists(PATH))
				{
					Directory.Delete(PATH, true);
				}
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error cleaning up the directory in : " + PATH +"\n\nException is : " + e);
				return false;
			}
		}
	}
}
