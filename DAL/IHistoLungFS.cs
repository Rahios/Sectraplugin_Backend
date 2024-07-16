
namespace DAL
{
	public interface IHistoLungFS
	{
		void CommandAIToAnalyze(string fileName);
		IEnumerable<string> GetImages();
		bool ImageExists(string fileName);
	}
}