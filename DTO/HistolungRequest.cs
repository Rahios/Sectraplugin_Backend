using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
	/// <summary>
	/// Request model for the Histolung controller analysis operation
	/// </summary>
	public class HistolungRequest 
	{
		// For the ANALYSIS REQUEST
		/// <summary>
		/// The name of the image to analyse as a ".tif" file
		/// </summary>
		public string ImageName { get; set; }
	}
}
