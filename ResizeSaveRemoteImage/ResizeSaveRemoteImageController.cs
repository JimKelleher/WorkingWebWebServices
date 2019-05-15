using System.IO;
using System.Net;
using System.Web.Http;

using System.Web.Hosting;

using System;

namespace WorkingWebWebServices.ResizeSaveRemoteImage
{
    public class ResizeSaveRemoteImageController : ApiController
    {
        // Test with:
        // http://localhost:60133/api/ResizeSaveRemoteImage/?imageUrl=https://upload.wikimedia.org/wikipedia/commons/d/d1/Elton_John_2011_Shankbone_2.JPG&fileName=Elton%20John&width=200&height=200

        // http://www.workingweb.info/utility/WorkingWebWebServices/api/ResizeSaveRemoteImage/?imageUrl=https://upload.wikimedia.org/wikipedia/commons/d/d1/Elton_John_2011_Shankbone_2.JPG&fileName=Elton%20John&width=200&height=200

        // GET: api/ResizeSaveRemoteImage/? etc.
        public void Get(string imageUrl, string fileName, int width, int height)
        {
            // Get the image pointed to by the url:
            string imageUrlString = imageUrl.ToString();
            byte[] imageDataBytes = new WebClient().DownloadData(imageUrlString);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(imageUrlString);
            WebResponse webResponse = httpWebRequest.GetResponse();
            BinaryReader binaryReader = new BinaryReader(webResponse.GetResponseStream());
            byte[] imageDataBytes2 = binaryReader.ReadBytes(imageDataBytes.Length);

            // Terminate:
            binaryReader.Close();
            webResponse.Close();

            // The first instantiation of the image will be in its original size as a temp file.
            // NOTE: This file will be created on the GoDaddy server, eg:
            // d:\temp\tmp\tmpF2D9.tmp
            string tempFileName = Path.GetTempFileName();

            FileStream fileStream = new FileStream(tempFileName, FileMode.Create);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(imageDataBytes2);

            // Terminate:
            binaryWriter.Close();
            binaryWriter.Dispose();

            // The second instantiation of the image will be to the specified size.
            // NOTE: System.Drawing doesn't work as a using so we must fully qualify here:
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.DrawImage(System.Drawing.Image.FromFile(tempFileName), 0, 0, (width), (height));

            // Terminate:
            fileStream.Close();
            fileStream.Dispose();

            // Save to thw final directory:
            bitmap.Save(HostingEnvironment.ApplicationPhysicalPath + fileName + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            // Terminate:
            bitmap.Dispose();

        }

    }
}
