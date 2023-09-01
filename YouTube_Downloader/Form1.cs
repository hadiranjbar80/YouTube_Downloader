using System.Net;
using System.Net.NetworkInformation;
using YoutubeExplode;

namespace YouTube_Downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbl = new FolderBrowserDialog();
            fbl.Description = "Select a path to save the video";
            if (fbl.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = fbl.SelectedPath;
            }

        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            if (string.IsNullOrWhiteSpace(txtPath.Text))
                errorProvider1.SetError(txtPath, "Please specify storage path.");
            else if (string.IsNullOrWhiteSpace(txtUrl.Text))
                errorProvider1.SetError(txtUrl, "Please specify video url.");
            else
            {
                btnDownload.Enabled = false;
                MessageBox.Show("Started downloading.", "Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                string outputDirectory = txtPath.Text;

                List<string> videoUrls = new List<string>()
                {

                txtUrl.Text
                };

                try
                {
                    foreach (var videoUrl in videoUrls)
                    {
                        await DownloadYouTubeVideo(videoUrl, outputDirectory);
                    }
                    btnDownload.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was a problem downloading the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        static async Task DownloadYouTubeVideo(string videourl, string outputDirectory)
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videourl);

            string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var mixedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();

            if (mixedStreams.Any())
            {
                var streamInfo = mixedStreams.FirstOrDefault();
                using var httpClient = new HttpClient();
                var stream = await httpClient.GetStreamAsync(streamInfo.Url);
                var datetime = DateTime.Now;

                string outputFilePath = Path.Combine(outputDirectory, $"{sanitizedTitle}.{streamInfo.Container}");
                using var outputStream = File.Create(outputFilePath);
                await stream.CopyToAsync(outputStream);

                MessageBox.Show("File has been downloaded successfuly.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}