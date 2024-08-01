using BLL.Interfaces;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL.Managers
{
    public class HistolungManager : IHistolungManager
	{
		// A T T R I B U T E S
		private readonly IHistolungFS _histolungFS;

		// C O N S T R U C T O R
		public HistolungManager(IHistolungFS histolungFS)
		{
			_histolungFS = histolungFS;
		}


		// M E T H O D S
		public HistolungResponse AnalyseImage(HistolungRequest request)
		{
			return _histolungFS.AnalyzeImage(request);
		}

		public byte[] GetHeatmap()
		{
			return _histolungFS.GetHeatmap();
		}

		public HistolungResponse GetLastAnalysis()
		{
			return _histolungFS.GetLastAnalysis();
		}
	}
}
