using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;


namespace PhotoSelector
{
    public partial class PhotoSelectorDialog : Form
    {
        public PhotoSelectorDialog()
        {
            InitializeComponent();
            listView1.Columns.Add("Name");
            listView1.Columns.Add("Date");

            listView1.CheckBoxes = true;
            listView1.View = View.Details;
            listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
            listView1.SmallImageList = new ImageList();
            listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView1.SmallImageList.ImageSize = new Size(75, 75);
            listView1.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;

            listView1.ItemSelectionChanged += ListView1_ItemSelectionChanged;
            listView1.MultiSelect = false;

            listView1.DrawItem += ListView1_DrawItem;

            listView1.ItemChecked += ListView1_ItemChecked;

            photoBox.CheckBox_CheckChanged += PhotoBox_CheckBox_CheckChanged;
            photoBox.NextButton_Clicked += PhotoBox_NextButton_Clicked;
            photoBox.PreviousButton_Clicked += PhotoBox_PreviousButton_Clicked;
        }

        private void PhotoBox_PreviousButton_Clicked(object sender, EventArgs e)
        {
            PreviousPhoto();
        }

        private void PhotoBox_NextButton_Clicked(object sender, EventArgs e)
        {
            NextPhoto();
        }

        private void PhotoBox_CheckBox_CheckChanged(object sender, CheckChangedEventArgs e)
        {
            if (listView1.SelectedItems != null)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    listView1.SelectedItems[0].Checked = e.Checked;
                }
            }
           
        }

        private void ListView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            photoBox.Checked = e.Item.Checked;
        }


        private void NextPhoto()
        {
            int oldIndex = -1;
            if (listView1.SelectedItems != null)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    oldIndex = listView1.SelectedItems[0].Index;
                }
            }
            if (oldIndex != -1 && oldIndex < listView1.Items.Count - 1)
            {
                listView1.Items[oldIndex + 1].Selected = true;
            }
        }

        private void PreviousPhoto()
        {
            int oldIndex = -1;
            if (listView1.SelectedItems != null)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    oldIndex = listView1.SelectedItems[0].Index;
                }
            }
            if (oldIndex != -1 && oldIndex > 0)
            {
                listView1.Items[oldIndex - 1].Selected = true;
            }
        }
        private void NextButton_Click(object sender, EventArgs e)
        {
            NextPhoto();
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            PreviousPhoto();
        }
        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //ListViewItemPhoto listViewItemPhoto = e.Item as ListViewItemPhoto;
            //e.Graphics.DrawImage(Image.FromFile(listViewItemPhoto.FilePath), new Rectangle(0, 0, 64, 64));
        }

        private void ListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems != null)
            {
                if (listView1.SelectedItems.Count>0)
                {
                    ListViewItemPhoto item = listView1.SelectedItems[0] as ListViewItemPhoto;
                    photoBox.Image = Image.FromFile(item.FilePath);
                    photoBox.Checked = item.Checked;
                }
            }
        }
        public bool ThumbnailCallback()
        {
            return true;
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = true
            };



            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var progress = new Progress<double>(percent =>
                {
                    progressBar1.Value = (int)percent;
                });

                int count = 0;
                foreach (var directory in dialog.FileNames)
                {
                    count+= Directory.GetFiles(directory, "*.jpeg", SearchOption.AllDirectories).Length;
                    count += Directory.GetFiles(directory, "*.jpg", SearchOption.AllDirectories).Length;
                    count += Directory.GetFiles(directory, "*.png", SearchOption.AllDirectories).Length;
                }

                this.Enabled = false;
                await Task.Run(() =>
                {
                    LoadFiles(dialog.FileNames, count, progress);
                });
                progressBar1.Value = 0;
                this.Enabled = true;


            }


        }

        private void LoadFiles(IEnumerable<string> folderNames,int totalSteps, IProgress<double> progress)
        {
            int step = 0;
            try
            {
                List<ListViewItemPhoto> allPhotos = new List<ListViewItemPhoto>();

                foreach (var folder in folderNames)
                {
                    foreach (var item in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                    {
                        if (item.ToLower().EndsWith(".jpg") || item.ToLower().EndsWith(".jpeg") || item.ToLower().EndsWith(".png"))
                        {
                            double percentage = (double)Math.Min(decimal.Divide(step, totalSteps), 1);
                            progress.Report((double)(percentage * 100));
                            step++;
                            ListViewItemPhoto listViewItemPhoto = new ListViewItemPhoto();
                            listViewItemPhoto.FilePath = item;
                            try
                            {
                                listViewItemPhoto.Date = GetDateTakenFromImage(item);
                            }
                            catch
                            {
                                listViewItemPhoto.Date = new FileInfo(item).LastWriteTime;
                            }
                            allPhotos.Add(listViewItemPhoto);
                        }
                    }
                }

                List<ListViewItemPhoto> sortedPhotos = allPhotos.OrderBy(o => o.Date).ToList();
                Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                Invoke(new Action(() =>
                {
                    listView1.SuspendLayout();
                    listView1.SmallImageList = new ImageList();
                    listView1.SmallImageList.ImageSize = new Size(75, 75);
                    listView1.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;

                    for (int i = 0; i < sortedPhotos.Count; i++)
                    {
                        Image thumbNail = GetThumbnail(sortedPhotos[i].FilePath);
                        if (thumbNail == null)
                        {
                            continue;
                        }
                        listView1.SmallImageList.Images.Add(thumbNail);

                        ListViewItemPhoto item = sortedPhotos[i];
                        item.ImageIndex = i;
                        item.Text = Path.GetFileName(sortedPhotos[i].FilePath);
                        item.SubItems.Add(item.Date.ToString("dd-MM-yyyy HH:mm:ss"));

                        listView1.Items.Add(item);


                    }

                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    if (listView1.Items.Count > 0)
                    {
                        listView1.Items[0].Selected = true;
                    }
                    listView1.ResumeLayout();

                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong: \n\n"+ex.Message);
            }
        }

        private  Regex r = new Regex(":");
        public DateTime GetDateTakenFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                PropertyItem propItem = myImage.GetPropertyItem(36867);
                string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                return DateTime.Parse(dateTaken);
            }
        }

        private const int THUMBNAIL_DATA = 0x501B;


        /// <summary>
        /// Gets the thumbnail from the image metadata. Returns null of no thumbnail
        /// is stored in the image metadata
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Image GetThumbnail(string path)
        {
            FileStream fs = File.OpenRead(path);
            // Last parameter tells GDI+ not the load the actual image data
            Image img = Image.FromStream(fs, false, false);


            // GDI+ throws an error if we try to read a property when the image
            // doesn't have that property. Check to make sure the thumbnail property
            // item exists.
            bool propertyFound = false;
            for (int i = 0; i < img.PropertyIdList.Length; i++)
                if (img.PropertyIdList[i] == THUMBNAIL_DATA)
                {
                    propertyFound = true;
                    break;
                }

            if (!propertyFound)
                return null;

            PropertyItem p = img.GetPropertyItem(THUMBNAIL_DATA);
            fs.Close();
            img.Dispose();


            // The image data is in the form of a byte array. Write all 
            // the bytes to a stream and create a new image from that stream
            byte[] imageBytes = p.Value;
            MemoryStream stream = new MemoryStream(imageBytes.Length);
            stream.Write(imageBytes, 0, imageBytes.Length);

            return Image.FromStream(stream);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = true;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems == null)
            {
                MessageBox.Show("Nothing selected.");
                return;
            }
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Nothing selected.");
                return;
            }
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var progress = new Progress<double>(percent =>
                {
                    progressBar1.Value = (int)percent;
                });

                int count = 0;

                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Checked)
                    {
                        count++;
                    }
                }



                this.Enabled = false;
                await Task.Run(() =>
                {
                    CopyFiles(dialog.FileName, count, progress);
                });

                progressBar1.Value = 0;
                this.Enabled = true;

                MessageBox.Show($"Copied {count} photos to {dialog.FileName}");
            }
        }

        private void CopyFiles(string path, int totalSteps, IProgress<double> progress)
        {
            int step = 0;
            UniqueName uniqueName = new UniqueName();
            Invoke(new Action(() =>
            {
                foreach (ListViewItemPhoto item in listView1.Items)
                {
                    if (item.Checked)
                    {
                        double percentage = (double)Math.Min(decimal.Divide(step, totalSteps), 1);
                        progress.Report((double)(percentage * 100));
                        step++;

                        string sourceFileName = Path.GetFileName(item.FilePath);
                        string ext = Path.GetExtension(item.FilePath);
                        string cleanFileName = Path.GetFileNameWithoutExtension(item.FilePath);
                        string newPath = path + @"\" + sourceFileName;

                        while (true)
                        {
                            if (File.Exists(newPath))
                            {
                                cleanFileName = uniqueName.GetNext(cleanFileName);
                                newPath = path + @"\" + cleanFileName + ext;
                            }
                            else break;
                        }
                        File.Copy(item.FilePath, newPath);
                    }
                }

            }));
        }


    }

    public class UniqueName
    {
        private Dictionary<string, int> values { get; set; }
    public UniqueName()
        {
            values = new Dictionary<string, int>();
            Separator = '-';
        }
        public char Separator { get; set; }
        public string GetNext(string name)
        {
            if (values.ContainsKey(name))
            {
                values[name]++;
            }
            else
            {
                values.Add(name, 1);
            }
            return name + Separator + values[name];

        }
    }

}
