namespace BLL.Interfaces
{
	/// <summary>
	/// HelloWorldManager class is responsible for generating and saving 
	/// - an image 200x200 pixels, half red other half blue 
	/// - and a text file with the text "Hello World"
	/// </summary>
	public interface IHelloWorldManager
	{
		/// <summary>
		/// Generates a text file with the text "Hello World" in the FileSystem
		/// </summary>
		bool SendHelloWorld();

		/// <summary>
		/// Reads the text file with the text "Hello World" from the FileSystem
		/// </summary>
		/// <returns>The content of the text file</returns>
		string GetHelloWorld();

		/// <summary>
		/// Generates an image 200x200 pixels, half red other half blue
		/// </summary>
		bool GeneratAndSaveImage();

		/// <summary>
		/// Reads the image from the FileSystem
		/// </summary>
		/// <returns>The image as a byte array to download</returns>
		byte[] GetImage();

	}
}