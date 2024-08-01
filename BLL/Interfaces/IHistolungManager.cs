﻿using DTO;

namespace BLL.Interfaces
{
	public interface IHistolungManager
	{
		global::DTO.HistolungResponse AnalyseImage(global::DTO.HistolungRequest request);
		byte[] GetHeatmap();
		string[] GetImagesList();
		HistolungResponse GetLastAnalysis();
	}
}