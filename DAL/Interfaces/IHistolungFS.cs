namespace DAL.Interfaces
{
	public interface IHistolungFS
	{
		global::DTO.HistolungResponse AnalyzeImage(global::DTO.HistolungRequest request);
	}
}