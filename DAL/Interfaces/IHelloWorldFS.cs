using System.Drawing;

namespace DAL.Interfaces
{
	/// <summary>
	/// HelloWorldFS class is responsible to access the FileSystem 
	/// </summary>
	public interface IHelloWorldFS
	{
		bool SaveToTextFile(string message);
		string ReadtextFile();
		byte[] ReadImage();
		bool SaveToImage(byte[] image);
	}
}