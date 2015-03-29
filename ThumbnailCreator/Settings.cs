using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ThumbnailCreator
{
    class Settings
    {
        string filePath;
        Log log;
        public Settings() { }
        public Settings(string applicationPath, Log log)
        {
            this.log = log;
            this.filePath = applicationPath + @"\settings.json";
        }
        public string LastWorkingDirectory { get; set; }
        public int SplitContainerImagePosition { get; set; }
        public List<string> PictureExtensions { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public string Dimension { get; set; }
        public bool KeepRatio { get; set; }
        public Thumbnail.ThumbnailQuality Quality { get; set; }
        public string Suffix { get; set; }

        public void Load()
        {
            try
            {
                Settings s = (Settings)JsonSerializer.Deserialize(File.ReadAllText(filePath), this.GetType());
                this.LastWorkingDirectory = s.LastWorkingDirectory;
                this.SplitContainerImagePosition = s.SplitContainerImagePosition;
                this.PictureExtensions = s.PictureExtensions;
                this.WindowWidth = s.WindowWidth;
                this.WindowHeight = s.WindowHeight;
                this.ThumbnailWidth = s.ThumbnailWidth;
                this.ThumbnailHeight = s.ThumbnailHeight;
                this.Dimension = s.Dimension;
                this.KeepRatio = s.KeepRatio;
                this.Quality = s.Quality;
                this.Suffix = s.Suffix;
                log.AppendMessage("Settings successfully loaded.");
            }
            catch
            {
                log.AppendMessage("An error occured while loading the settings, loading default ones.");
                this.LastWorkingDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                this.SplitContainerImagePosition = 200;
                this.PictureExtensions = new List<string>();
                this.PictureExtensions.Add(".jpg");
                this.PictureExtensions.Add(".bmp");
                this.PictureExtensions.Add(".png");
                this.WindowWidth = 850;
                this.WindowHeight = 566;
                this.ThumbnailWidth = 200;
                this.ThumbnailHeight = 175;
                this.Dimension = "Breite";
                this.KeepRatio = true;
                this.Quality = Thumbnail.ThumbnailQuality.Normal;
                this.Suffix = "_thumbnail";
            }
        }
        public void Save()
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(this));
            log.AppendMessage("Saved settings successfully.");
        }
    }
}
