
namespace BLL
{
	public interface IHistoLungManager
	{
		IEnumerable<string> ListImageFiles();
		bool ProcessImage(string fileName);
	}
}