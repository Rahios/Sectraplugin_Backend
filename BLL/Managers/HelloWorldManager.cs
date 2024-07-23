using BLL.Interfaces;
using DAL.Interfaces;
using SkiaSharp;


namespace BLL.Managers
{

	public class HelloWorldManager : IHelloWorldManager
    {
        // * * * D E P E N D E N C I E S   I N J E C T I O N * * *
        private readonly IHelloWorldFS _helloWorldFS;

        public HelloWorldManager(IHelloWorldFS helloWorldFS)
        {
            _helloWorldFS = helloWorldFS;
        }


        // * * * M E T H O D S * * *
        public bool SendHelloWorld()
        {
            var message = "Hello World".ToUpper();
            return _helloWorldFS.SaveToTextFile(message);
        }

        public string GetHelloWorld()
        {
            return _helloWorldFS.ReadtextFile();
        }

        public bool GeneratAndSaveImage()
        {
            try
            {
				// Create the image with SkiaSharp (Linux compatible)
				Console.WriteLine("Generating the image to save on the FileSystem");
				var info = new SKImageInfo(200, 200);
				using var surface = SKSurface.Create(info);
				var canvas = surface.Canvas;

				// Fill the left half with red
                Console.WriteLine("Filling the left half with red");
				var redPaint = new SKPaint { Color = SKColors.Red };
				canvas.DrawRect(new SKRect(0, 0, 100, 200), redPaint);

				// Fill the right half with blue
                Console.WriteLine("Filling the right half with blue");
				var bluePaint = new SKPaint { Color = SKColors.Blue };
				canvas.DrawRect(new SKRect(100, 0, 200, 200), bluePaint);

                // Save the image
                Console.WriteLine("Saving the image on the FileSystem");
				using var image = surface.Snapshot();
				using var data = image.Encode(SKEncodedImageFormat.Png, 100);
				return _helloWorldFS.SaveToImage(data.ToArray());
			}
            catch (Exception e)
			{
				Console.WriteLine("Error generating the image to save on the FileSystem. " + e);
				return false;
			}
			
		}

        public byte[] GetImage()
        {
            return _helloWorldFS.ReadImage();
        }


    }
}
