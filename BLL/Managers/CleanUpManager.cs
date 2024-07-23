using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Interfaces;
using DAL.Interfaces;

namespace BLL.Managers
{
	public class CleanUpManager : ICleanUpManager
	{
		// * * * D E P E N D E N C I E S   I N J E C T I O N * * *
		private readonly ICleanUpFS _cleanUpFS;

		public CleanUpManager(ICleanUpFS cleanUpFS)
		{
			_cleanUpFS = cleanUpFS;
		}

		// * * * M E T H O D S * * *
		public bool CleanUp()
		{
			return _cleanUpFS.CleanUp();
		}
	}
}
