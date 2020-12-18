using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTool.Test
{
    [TestClass]
    public class ImgurAlbumFactoryTest
    {
        private const string IMGUR_ALBUM_ID = "cbLdh";
        private const string IMGUR_ALBUM_RESPONSE = "ImgurAlbumResponse.json";
        private ImgurAlbumFactory imgurAlbumFactory;

        [TestInitialize]
        public void initialise()
        {
            imgurAlbumFactory = new ImgurAlbumFactory();
        }

        [TestMethod]
        public void ConvertImgurAlbum()
        {
            var contents = GetFileContents(IMGUR_ALBUM_RESPONSE);
            var album = imgurAlbumFactory.ConvertPlaylist(contents);
            Assert.AreEqual("STS display", album.title);
            Assert.AreEqual(4, album.images_count);

            Assert.AreEqual("lAWVKKW", album.images[0].id);
            Assert.AreEqual("AtbOyto", album.images[1].id);
            Assert.AreEqual("ldaISC3", album.images[2].id);
            Assert.AreEqual("YAJkHLA", album.images[3].id);
        }

        [TestMethod]
        [Ignore("Imgur API is returning 4xx errors. Further investigation requred.")]
        public void DownloadImgurAlbumCheck()
        {
            var album = imgurAlbumFactory.DownloadAlbum(IMGUR_ALBUM_ID);
            Assert.AreEqual(4, album.images.Count());
        }

        private string GetFileContents(string filename)
        {
            using (var resultStream = File.Open(filename, FileMode.Open))
            using (var reader = new StreamReader(resultStream, Encoding.UTF8))
            {
                var resultJson = reader.ReadToEnd();
                return resultJson;
            }
        }

    }
}
