using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using GMap.NET;
using GMap.NET.Internals;
//using GMap.NET.ObjectModel;
using System.Globalization;
using GMap.NET.CacheProviders;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Data.Common;
using System.Diagnostics;
//using System.Windows.Forms.Control;
//using Utilities; //başka bir yolu olmalı

namespace map_den
{
    /*public class sinif1 // !!!!!!!
    {
       public double Prev_lat,Prev_lng;
        
    }*/
    public partial class Form1 : Form
    {
        /*Türkiye sınırları sag:45 , sol:25.8 alt:35.91, üst:42.12*/
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public GMapOverlay OverlayMarkers = new GMapOverlay("Markers");

        // Ankara
        public double lat_guney = 39.925533;
        public double lat = 39.925533;
        public double lng_dogu = 33.866287;
        public double lat_kuzey = 39.5548;
        public double lng_bati = 32.866287, lng = 32.866287;
        public double prev_lat, prev_lng;
        /*public double lat_guney , lat;
        public double lng_dogu;
        public double lat_kuzey;
        public double lng_bati, lng;*/
        // private string Prev_lat,Prev_lng;

        double[,] Zoomvalues = new double[12, 2] { 
        /*18*/{ 0.013684,0.005011},
        /*17*/{ 0.02738, 0.009972},
        /*16*/{ 0.0547, 0.0202 },
        /*15*/{ 0.10875, 0.053026-0.0125 },
        /*14*/{ 0.215, 0.07 }, 
        /*13*/{ 0.4319, 0.1822-0.0075 },
        /*12*/{ 0.8636, 0.3453-0.0125 },
        /*11*/{ 1.600, 0.6815-0.055 }, 
        /*10*/{ 3.4621, 1.7985 }, 
        /*9*/{ 6.8939, 3.5626-0.75 },
        /*8*/{ 13.8923, 7.1232-2.1 }, 
        /*7*/{ 27.6416-2, 14.1022-5 }}; // boylam(lng) , enlem(lat) 14 e -4 çok fazla

        int toplam_tarama_miktari = 0;
        int point_index = 0;
        int value = 0;
        double increase = 1;
        double heading = 90;
        int zoom = 10;
        int c = 0;
        int server_check = 0;
        //int comboBox = 0;
        int timer_internal = 30;
        /// her kaymada degerler checkpoint.txt'ye yazılıyor


        GMapOverlay polygons = new GMapOverlay("polygons");
        GMapOverlay markersOverlay = new GMapOverlay("markers");
        public List<PointLatLng> _points = new List<PointLatLng>();
        public GMapPolygon poligon;
        private double[] limits = new double[4] { 400, 400, 400, 400 }; // max_lat, max_lng, min_lat, min_lng
        GMap.NET.WindowsForms.Markers.GMarkerGoogle marker;


        // Zoom 14 için boylamların farkı 0.265 ,enlemlerin farkı 0.07
        // Zoom 13 için boylamların farkı [0.4316, 0.4321] ,enlemlerin farkı [0.1835 ,0.181] 
        // Zoom 12 için boylamların farkı [0.8628, 0.8552,0.8645], enlemlerin farkı [0.3483 ,1.5934,0.3423]
        // Zoom 11 için boylamların farkı [1,7351,1.7379,1.7324],  enlemlerin farkı [0.6722, 0.6899,0,9053]
        // Zoom 10 için boylamların farkı [3.4621],  enlemlerin farkı [1.7985]
        // Zoom 9 için boylamların farkı [6.8939],  enlemlerin farkı [3.5626]
        // Zoom 8 için boylamların farkı [13.8923,13,8646],  enlemlerin farkı [7.1232 , 6,4079]
        // Zoom 7 için boylamların farkı [27.5976,27.6416,27.7624],  enlemlerin farkı [14.1022,11.5673,14.335]

        //https://github.com/mdbtools/gmdb2



        bool sag = true;    //// sag true ise saga gidiyor false ise sola gidiyor.   /// sonradan devam edilecekse checkpointte yazan yöne göre burası düzenlenmeli
        string path = @"checkpoint.txt";       /// her kaymada degerler checkpoint.txt'ye yazılıyor
        string yon = "";

        bool flag = false; // Programa başlar başlamaz tarama yapmaya başlamasın
        //bool flag2 = true;
        bool flag_bar = true;
        bool flag_marker = true;
        int click_count = 0;
        bool mouse_left = false;
        bool mouse_right = false;
        bool addpoint_kullanildi = false;
        

        /// <summary>
        ///
        /// </summary>
        public static Form1 instance;

        public Form1()
        {
            InitializeComponent();
            instance = this;

            //this.ActiveControl = textBox1;
            //textBox1.Focus();
            checkBox2.Checked = true;
            checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            int zoom = 18 - Convert.ToInt32(comboBox1.SelectedIndex);

            string yol = "D:\\map_den - Copy\\map_den\\bin\\Debug\\cacheSa3\\TileDBv5\\en";
            DirectoryInfo di = new DirectoryInfo(yol);
            FileInfo[] fiArr = di.GetFiles();
            foreach (FileInfo f in fiArr){
                label15.Text = (f.Length.ToString("#,##0")+" bytes"); 
            }
            fiArr = null;
            /*try
            {
                using (var client = new WebClient())
                //using (var stream = client.OpenRead("http://www.google.com"))
                using (var stream2 = client.OpenRead("https://www.youtube.com"))
                {
                    label16.BackColor = Color.GreenYellow;
                    label16.Text = "Ağa Bağlı";
                }
            }
            catch
            {
                DialogResult result = MessageBox.Show("İnternete bağlı değilsiniz!  ", "Gmap", MessageBoxButtons.OK);
                //return;
                label16.BackColor = Color.Red;
                return;
            }*/

            
            

        }
        // Create a method for a delegate.


        public void Form1_Load(object sender, EventArgs e)
        {
            gmap.Overlays.Add(OverlayMarkers);
            gmap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            //gmap.CacheLocation = @"cacheSa3";
            //gmap.CacheLocation = "D:/map_den/map_den/bin/Debug/cacheSa3";
            gmap.CacheLocation = "D:/map_den - Copy/map_den/bin/Debug/cacheSa3";

            gmap.Overlays.Add(OverlayMarkers);
            gmap.MarkersEnabled = true;
            gmap.NegativeMode = false;
            gmap.RetryLoadTile = 0;
            gmap.ShowTileGridLines = false;
            gmap.AllowDrop = true;
            gmap.IgnoreMarkerOnMouseWheel = true;
            gmap.DragButton = MouseButtons.Right;
            gmap.DisableFocusOnMouseEnter = true;
            //gmap.SetPositionByKeywords("Ankara, Turkey");

            gmap.MinZoom = 5;
            gmap.ShowCenter = false;
            gmap.MaxZoom = 18;
            gmap.Zoom = 10;

            button4.Enabled = false; // stop
            textBox5.Text = "30";

            comboBox1.Enabled = true;
            comboBox1.DisplayMember = "14";
            comboBox1.SelectedIndex = -1;  // ???
            comboBox1.Text = "";//(14).ToString();

            
            

            /*
            Label mylab = new Label();
            mylab.Text = "Wifi durumu";
            mylab.Location = new Point(2486, 248);
            mylab.Size = new Size(120, 25);
            mylab.BorderStyle = BorderStyle.FixedSingle;
            mylab.BackColor = Color.LightBlue;
            mylab.Font = new Font("Calibri", 12);
            mylab.ForeColor = Color.DarkBlue;

            // Adding this control to the form
            this.Controls.Add(mylab);*/

            label14.Text = "Tahmini tarama süresi";

            this.pictureBox1.Size = new Size(1000, 800);

            PointLatLng point = new PointLatLng(lat, lng);
            setMarker(point);

            GMaps.Instance.Mode = AccessMode.ServerAndCache;


            label14.Click += new EventHandler(label14_Click); //??

            label15.Click += new EventHandler(label15_Click);


            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                using (var stream2 = client.OpenRead("https://www.youtube.com"))
                {
                    label16.BackColor = Color.GreenYellow;
                    label16.Text = "Ağa Bağlı";
                }
            }
            catch
            {
                DialogResult result = MessageBox.Show("İnternete bağlı değilsiniz!  ", "Gmap", MessageBoxButtons.OK);
                label16.BackColor = Color.Red;
            }


            
                /*
                DialogResult result = MessageBox.Show("Tarama " + timer_internal.ToString() + " saniye sonra başlayacak  ", "Gmap", MessageBoxButtons.OKCancel);
                switch (result)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.OK:
                        break;
                    default:
                        break;
                }*/
            
            
            /*
            button4.Enabled = false;
            button3.Enabled = false;
            button2.Enabled = false;
            //comboBox1.Enabled = false;
            checkBox2.Checked = true;
            comboBox1.SelectedIndex = 0;
            */


            /*
            textBox4.Text = lng_bati.ToString(); // write latitude
            lng = lng_bati;

            textBox5.Text = lat_guney.ToString(); // write longitude
            lat = lat_guney;

            textBox6.Text = lng_dogu.ToString();

            textBox7.Text = lat_kuzey.ToString();

            label3.Text = gmap.Zoom.ToString(); // write zoom level
            
            */
            /*gmap.OnMapZoomChanged += zoomLevel; // delegate to function
            gmap.MouseClick += map_click; // delegate to function


            setMarker(point);
            timer1.Start();*/

            //DialogResult result2 = DialogResult.OK ;
            //result = map_den.Form2.Show ;

        }


        

        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                //using (var stream = client.OpenRead("http://www.google.com"))
                using (var stream2 = client.OpenRead("https://www.youtube.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        public void zoomLevel()
        {
            label3.Text = gmap.Zoom.ToString();
        }


        public void map_click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                flag = false;
                flag_bar = false;
                mouse_left = true;

                var point = gmap.FromLocalToLatLng(e.X, e.Y);
                lat = point.Lat;
                lng = point.Lng;

                setMarker(point);
                if (click_count == 0)
                {
                    prev_lat = lat;
                    prev_lng = lng;
                    click_count++;
                }
            }

            else if (e.Button == MouseButtons.Right)
            {
                flag = false;
                flag_bar = false;
                mouse_right = true;
                if (click_count == 0)
                {
                    prev_lat = lat;
                    prev_lng = lng;
                    click_count++;
                }
                /*value = progressBar1.Value;
                progressBar1.Value = 0;*/
            }


        }
        public Bitmap RotateImg(Bitmap image, float angle)
        {
            Bitmap rotatedBmp = new Bitmap(image.Width, image.Height);
            rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            Graphics g = Graphics.FromImage(rotatedBmp);
            PointF offset = new PointF(image.Width / 2, image.Height / 2);
            g.TranslateTransform(offset.X, offset.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-offset.X, -offset.Y);
            g.DrawImage(image, new PointF(0, 0));

            return rotatedBmp;



        }

        public void setMarker(PointLatLng point)
        {
            gmap.Position = point;    // konum değiştikçe odaklansın isteniyorsa açık bırakılmalı. odak istenmiyorsa silinebilir

            // var marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(point, GMap.NET.WindowsForms.Markers.GMarkerGoogleType.arrow);

            Bitmap bmpMarker = (Bitmap)Image.FromFile("D:/map_den - Copy/map_den_2/map_den/airplane.png");

            marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(point, RotateImg(bmpMarker, (float)heading));
            marker.Offset = new Point(-bmpMarker.Width / 2, -bmpMarker.Height / 2);
            OverlayMarkers.Clear();
            gmap.Overlays.Add(OverlayMarkers);
            OverlayMarkers.Markers.Add(marker);

        }


        public void server_function(object sender, EventArgs e)
        {
            if (server_check > 0)
            {
                GMaps.Instance.Mode = AccessMode.ServerAndCache;
            }
            else
            {
                GMaps.Instance.Mode = AccessMode.CacheOnly;
            }
        }

        private void gmap_MouseMove(object sender, MouseEventArgs e)
        {
            var point = gmap.FromLocalToLatLng(e.X, e.Y);
            double lat = point.Lat;
            double lng = point.Lng;
            label5.Text = lat.ToString();
            label6.Text = lng.ToString();
        }

        private void gmap_Load(object sender, EventArgs e)
        {
            //Form1.instance.gm = gmap;

            gmap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            //gmap.CacheLocation = @"cacheSa3";
            //gmap.CacheLocation = "D:/map_den/map_den/bin/Debug/cacheSa3";
            gmap.CacheLocation = "D:/map_den - Copy/map_den/bin/Debug/cacheSa3";


            gmap.Overlays.Add(OverlayMarkers);
            gmap.MarkersEnabled = true;
            gmap.NegativeMode = false;
            gmap.RetryLoadTile = 0;
            gmap.ShowTileGridLines = false;
            gmap.AllowDrop = true;
            gmap.IgnoreMarkerOnMouseWheel = true;
            gmap.DragButton = MouseButtons.Right;
            gmap.DisableFocusOnMouseEnter = true;

            gmap.MinZoom = 5;
            gmap.ShowCenter = false;
            gmap.MaxZoom = 18;
            gmap.Zoom = 10;

            PointLatLng point = new PointLatLng(lat, lng);
            gmap.Position = point;

            gmap.OnMapZoomChanged += zoomLevel; // delegate to function
            gmap.MouseClick += map_click; // delegate to function

            /*Form2 form2 = new Form2();
            setMarker(point);
            form2.timer1.Start();*/
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        public void button2_Click(object sender, EventArgs e) // Add Point
        {
            /*Taranacak bölge haritadan seçilirse, point'ler bir array'e atanır. 
             * Array batı-doğu boylamlarını ve guney-kuzey enlemlerini
             * limits[4,1] = { bati boylamı, güney enlemi, doğu boylamı, kuzey enlemi}
             * = { lng_bati, lat_guney, lng_dogu, lat_kuzey'e } tutar
             default değer olarak {400,400,400,400} değerleri verilir. point değerleri girildikçe bu değerler değişir. */

            addpoint_kullanildi = true;
            point_index++;
            // Yeşil konumlar
            markersOverlay = new GMapOverlay("markers");
            marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(new PointLatLng(lat, lng), GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green_big_go);

            markersOverlay.Markers.Add(marker);
            gmap.Overlays.Add(markersOverlay);

            if (lat > limits[3] || limits[3] == 400) // girilen veya tıklanan lat değeri limits[3]'daki lat değerinden daha büyükse 
            {
                limits[3] = lat;
                this.lat_kuzey = lat;

                //textBox2.Text = lat_kuzey.ToString();
            }
            if (lat < limits[1] || limits[1] == 400) // "" lat değeri limits[1]'deki lat değerinden daha küçükse
            {
                limits[1] = lat;
                this.lat_guney = lat;

                //MessageBox.Show("lat_guney"+lat_guney.ToString());
                //textBox3.Text = lat_guney.ToString();
            }


            if (lng > limits[2] || limits[2] == 400) // "" lng değeri limits[2] deki değerden daha büyükse
            {
                limits[2] = lng;
                this.lng_dogu = lng;

                //textBox4.Text = lng_dogu.ToString();
            }
            if (lng < limits[0] || limits[0] == 400) // "" lng değeri limits[0]'deki değerden daha küçükse
            {
                limits[0] = lng;
                this.lng_bati = lng;

                //textBox1.Text = lng.ToString();
            }
            if (point_index > 1)
            {

                _points.Add(new PointLatLng(lat_kuzey, lng_bati));
                _points.Add(new PointLatLng(lat_kuzey, lng_dogu));
                _points.Add(new PointLatLng(lat_guney, lng_dogu));
                _points.Add(new PointLatLng(lat_guney, lng_bati));
                GMapPolygon polygon = new GMapPolygon(_points, "polygon")
                {
                    Stroke = new Pen(Color.Red, 1),
                    Fill = new SolidBrush(Color.FromArgb(50, Color.Red))
                };
                var polygons = new GMapOverlay("polygons");
                polygons.Polygons.Add(polygon);
                gmap.Overlays.Add(polygons);
                lat = lat_guney;
                lng = lng_bati;
                int focusSquare = Convert.ToInt32(gmap.Zoom);
                gmap.Zoom = focusSquare - 1; // seçilen bölgenin daha rahat görünebilmesi için ?????

                textBox3.Text = lng_bati.ToString();
                textBox2.Text = lat_guney.ToString();
                textBox1.Text = lng_dogu.ToString();
                textBox4.Text = lat_kuzey.ToString();

                //label5.Text = metin
                //label14.Text += new EventHandler(button2_Click);

                //label14.Text += metin.ToString(); 
                //textBox6.Text += new EventHandler(textBox6_TextChanged);
                /*
                TextBox tb = new TextBox();
                tb.TextChanged += new EventHandler(textBox6_TextChanged);
                label14.Controls.Add(tb);*/
                //label14.Controls.IndexOfKey(metin);
                //button2.Click += new EventHandler(label14_Click);
                //textBox6.Text = metin;
            }

        }

        private void button3_Click(object sender, EventArgs e) // Start
        {
            PointLatLng point = new PointLatLng(lat_guney, lng_bati);
            setMarker(point);

            timer1.Enabled = true;
            if (!CheckInternetConnection())
            {
                //DialogResult result = MessageBox.Show("İnternete bağlı değilsiniz!  ", "Gmap", MessageBoxButtons.OK);
                DialogResult result = MessageBox.Show("İnternete bağlanılamıyor, Taramayı başlat? (Harita bilgilerini Data.gmdb'e yükleyebilmek için interneti açınız)  ", "Gmap", MessageBoxButtons.YesNo);
                switch (result)
                {
                    case DialogResult.OK:
                        break;
                    case DialogResult.Cancel:
                        flag = false;
                        return;
                    default:
                        flag = false;
                        break;
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Tarama " + timer_internal.ToString() + " saniye sonra başlayacak  ", "Gmap", MessageBoxButtons.OK );
                switch (result)
                {
                    case DialogResult.OK:
                        break;
                    default:
                        break;
                }


            }


            timer1.Start();
            lat = lat_guney;
            lng = lng_bati;
            flag = true;
            flag_bar = true;
            button3.Enabled = true;
            button4.Enabled = true;
            comboBox1.Enabled = false;

            //gmap.Zoom = 18 - comboBox1.SelectedIndex; ??
            gmap.Zoom = 18 - Convert.ToInt32(comboBox1.SelectedIndex); ;
            gmap.MaxZoom = 18;
            gmap.MinZoom = 5;
            click_count = 0;
            c = 0;
            mouse_left = false;
            mouse_right = false;
            //MessageBox.Show("lat_guney =" + frm1.lat_guney.ToString() + " lng_bati =" + frm1.lng_bati.ToString() );

            progressBar1.Value = 0;
            progressBar1.BackColor = Color.Red;
            toplam_tarama_miktari = 0;

            string path = "D:\\map_den - Copy\\map_den\\bin\\Debug\\cacheSa3\\TileDBv5\\en";
            
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fiArr = di.GetFiles();
            long bit = 0;
            //Console.WriteLine("The directory {0} contains the following files:", di.Name);
            foreach (FileInfo f in fiArr){
                label15.Text = (f.Length.ToString("#,##0")+" bytes"); 
                //string stri = Convert.ToString(f.Length);
            }
            //MessageBox.Show(Convert.ToString(bit)+"bytes");
            fiArr = null;
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            /* Tarama için */
            /*Program, ilk girilen lat ve lng değerlerinde ise bu değerlerde değişiklik yapılmıyor.
              *   Tarama daima soldan sağa doğru ilerler. En doğu boylam'a gelince zoomValue'suna göre bir üst enleme atlar. 
              *   (lng_bati, lat_kuzey)-> -> -> -> -> -> -> -> -> -> -> -> -> -> -> -> -> -> ->(lng_dogu, lat_kuzey)
              *  .
              *  .
              *  .
              *  .
              * ikinci tarama - (lng_bati, lat_guney+Zoomvalues[,])-> -> -> -> -> -> -> -> -> -> -> ->(lng_dogu, lat_guney+Zoomvalues[,])                                                   
              * ilk tarama - (lng_bati, lat_guney) -> -> -> -> -> -> -> -> -> -> -> -> -> -> -> -> (lng_dogu, lat_guney)  
             */
            /*bool ag = true;
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.youtube.com"))
                {
                    ag = true;
                }
            }
            catch
            {
                ag = false;
            }

            if (ag)
            {
                label16.BackColor = Color.Red;
                label16.Text = "Ağa Bağlanılamıyor";
                DialogResult result = MessageBox.Show("İnternete bağlanılamıyor, Tarama durdurulsunsun mu? (Harita bilgilerini yükleyebilmek için interneti açınız)  ", "Gmap", MessageBoxButtons.YesNo);
                switch (result)
                {
                    case DialogResult.Yes:
                        timer1.Stop();
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        break;
                }
            }*/
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                using (var stream2 = client.OpenRead("https://www.youtube.com"))
                {
                    label16.BackColor = Color.GreenYellow;
                    label16.Text = "Ağa Bağlı";
                }
            }
            catch
            {
                label16.BackColor = Color.Red;
                label16.Text = "Ağa Bağlanılamıyor";

                DialogResult result = MessageBox.Show("İnternete bağlanılamıyor, Tarama devam etsin mi? (Harita bilgilerini Data.gmdb'e yükleyebilmek için interneti açınız)  ", "Gmap", MessageBoxButtons.YesNo);
                switch (result)
                {
                    case DialogResult.OK:
                        break;
                    case DialogResult.Cancel:
                        //timer1.Enabled = false;
                        //timer1.Stop();
                        return;
                    default:
                        flag = false;
                        break;
                }
            }
            
            string path = "D:\\map_den - Copy\\map_den\\bin\\Debug\\cacheSa3\\TileDBv5\\en";
            //string str;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fiArr = di.GetFiles();
            foreach (FileInfo f in fiArr)
            {
                label15.Text = (f.Length.ToString("#,##0") + " bytes");
                //str = (f.Length.ToString("#,##0") + " bytes");

                //MessageBox.Show(str);
            }
            fiArr = null;

            //MessageBox.Show(str);
            //MessageBox.Show("outside of ifs");
            if (18 - Convert.ToInt32(comboBox1.SelectedIndex) != Convert.ToInt32(gmap.Zoom)) //|| gmap.DragButton == System.Windows.Forms.MouseButtons.Right) 
            {
                //MessageBox.Show("timer/if-1");
                flag = false;
                value = progressBar1.Value;
                progressBar1.Value = 0;

            }
            if (lng > lng_dogu && lat > lat_kuzey) // (sağ-üst) , son köşedeysek
            {
                flag = false;

            }

            if (flag) /* taramaya start button'u ile başlanmış ise */
            {
                //MessageBox.Show("Inside of timer1");
                progressBar1.Maximum = 100;
                progressBar1.Step = 1;
                comboBox1.Enabled = false;
                prev_lat = lat;
                prev_lng = lng;

                if ((lng > lng_dogu && lat > lat_kuzey) && flag) // son köşedeysek (sağ-üst) BU İŞLEMİYOR 
                {
                    flag = false;      //MessageBox.Show("Son köşede olmalı");
                    progressBar1.Value = 100;
                    progressBar1.Value = 0;

                    comboBox1.Enabled = true;
                    button3.Enabled = false;
                }
                else
                {
                    // Progressbar kontrolü ||~~~~~~~~||

                    if ((progressBar1.Value + Convert.ToInt32(Math.Ceiling(increase))) > 100) // Arttırılacak olan değer 100 den büyük ise barı % 95 yap
                    {
                        progressBar1.Value = 95;
                    }
                    else if (progressBar1.Value == value && !flag_bar) // Bir önceki adımdaki bar'ın value'su ile şimdiki value aynı ve flagbar false ise( ekrana tıklandığında değer false olur)
                    {
                        if (value + 20 <= 100)
                        {
                            progressBar1.Value = value + 20;
                        }
                        else
                        {
                            progressBar1.Value = 95;
                        }
                    }
                    else
                    {
                        progressBar1.Value += Convert.ToInt32(Math.Ceiling(increase));
                        if (progressBar1.Value + Convert.ToInt32(Math.Ceiling(increase)) >= 100)
                        {
                            progressBar1.Value = 95;
                            flag_bar = true;
                        }
                        value = progressBar1.Value;
                    }

                    // ||~~~~~~~~||

                    int zoomValue = 18 - Convert.ToInt32(comboBox1.SelectedIndex); //zoom;


                    // zoom değeri 7 den küçük ise 7'ye , 14 ten büyük  ise 14 e yuvarlanır 
                    if (zoomValue < 7)
                    {
                        zoomValue = 7;
                        gmap.Zoom = 7;
                    }
                    else if (zoomValue > 18)
                    {
                        zoomValue = 18;
                        gmap.Zoom = 18;
                    }

                    if (toplam_tarama_miktari == 0)
                    {
                        sag = true;
                        yon = "SAĞ";
                        NewMethod1(zoomValue);
                        toplam_tarama_miktari++;
                    }

                    else
                    {
                        NewMethod2(zoomValue);

                    }

                    PointLatLng point = new PointLatLng(lat, lng);
                    setMarker(point);


                    string text = "latitude: " + lat.ToString() + "\nlongitude: " + lng.ToString() + "\n yön: " + yon;
                    //File.WriteAllText(path, text);
                }
            }
            else if (!flag && !flag_bar)
            {
                timer1.Stop(); // Bu bir yerde patlar mı ?
                progressBar1.Value = value;
                progressBar1.Value = 0;
                comboBox1.Enabled = true;

            }
        }

        private void button5_Click(object sender, EventArgs e) // Refresh
        {
            if (gmap.Overlays.Count > 0)
            {
                gmap.Overlays.RemoveAt(0);
                gmap.Refresh();

                Application.Restart();
                Environment.Exit(0);
            }
        }

        private void button4_Click(object sender, EventArgs e) // Stop
        {
            /*double lat = frm1.lat;
            double lng = frm1.lng;
            double lng_bati = frm1.lng_bati;
            double lat_guney = frm1.lat_guney;
            double lng_dogu = frm1.lng_dogu;
            double lat_kuzey = frm1.lat_kuzey;*/
            if (c == 0)
            {
                prev_lat = lat;
                prev_lng = lng;
                c++;
            }
            button3.Enabled = true;
            comboBox1.Enabled = true;
            value = progressBar1.Value;
            progressBar1.Value = 0;

            timer1.Stop();
            /*lat = Convert.ToDouble(textBox1.Text);
            lng = Convert.ToDouble(textBox2.Text);
            heading = Convert.ToDouble(textBox3.Text);*/
            /*
            PointLatLng point = new PointLatLng(frm1.lat, frm1.lng);
            frm1.setMarker(point);*/
            
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string f = Metin();

            label14.Text = f;

            gmap.Zoom = 18 - Convert.ToInt32(comboBox1.SelectedIndex);
            

        }
        private double calc_seconds(double lat_kuzey, double lat_guney, double lng_bati, double lng_dogu)
        {
            //MessageBox.Show("lat_kuzey: "+Convert.ToString(lat_kuzey) + "lat_guney: "+Convert.ToString(lat_guney)+"lng_bati: "+ Convert.ToString(lng_bati)+"lng_dogu: "+Convert.ToString(lng_dogu));
            double genislik;
            //MessageBox.Show(Convert.ToString(comboBox1.SelectedIndex ));
            zoom = 18 - Convert.ToInt32(comboBox1.SelectedIndex);
            switch (zoom)//zoom
            {
                case 18:
                    genislik = Zoomvalues[0, 1];
                    break;
                case 17:
                    genislik = Zoomvalues[1, 1];
                    break;
                case 16:
                    genislik = Zoomvalues[2, 1];
                    break;
                case 15:
                    genislik = Zoomvalues[3, 1];
                    break;
                case 14:
                    genislik = Zoomvalues[4, 1];
                    break;
                case 13:
                    genislik = Zoomvalues[5, 1];
                    break;
                case 12:
                    genislik = Zoomvalues[6, 1];
                    break;
                case 11:
                    genislik = Zoomvalues[7, 1];
                    break;
                case 10:
                    genislik = Zoomvalues[8, 1];
                    break;
                case 9:
                    genislik = Zoomvalues[9, 1];
                    break;
                case 8:
                    genislik = Zoomvalues[10, 1];
                    break;
                case 7:
                    genislik = Zoomvalues[11, 1];
                    break;
                default:
                    genislik = 0.014;
                    break;

            }
            double uzunluk;
            switch (zoom)//zoom
            {
                case 18:
                    uzunluk = Zoomvalues[0, 0];
                    break;
                case 17:
                    uzunluk = Zoomvalues[1, 0];
                    break;
                case 16:
                    uzunluk = Zoomvalues[2, 0];
                    break;
                case 15:
                    uzunluk = Zoomvalues[3, 0];
                    break;
                case 14:
                    uzunluk = Zoomvalues[4, 0];
                    break;
                case 13:
                    uzunluk = Zoomvalues[5, 0];
                    break;
                case 12:
                    uzunluk = Zoomvalues[6, 0];
                    break;
                case 11:
                    uzunluk = Zoomvalues[7, 0];
                    break;
                case 10:
                    uzunluk = Zoomvalues[8, 0];
                    break;
                case 9:
                    uzunluk = Zoomvalues[9, 0];
                    break;
                case 8:
                    uzunluk = Zoomvalues[10, 0];
                    break;
                case 7:
                    uzunluk = Zoomvalues[11, 0];
                    break;
                default:
                    uzunluk = 0.014;
                    break;

            }
            double farklat = Math.Abs(lat_kuzey - lat_guney);

            if (farklat % uzunluk > 0.007)
            {
                farklat += uzunluk / (2.24);
            }

            double farklng = Math.Abs(lng_dogu - lng_bati);
            if (farklng % genislik > 0.01)
            {
                farklng += genislik / (2.24);
            }
            double carpim = farklat * farklng;
            double sayi = genislik * uzunluk;

            increase = (sayi / carpim) * 100;

            double konum_fark = sayi;
            double bolum = carpim / konum_fark;
            double result = bolum * timer_internal;

            string a = ("genişik:"+ genislik.ToString() + "\nuzunluk:"+uzunluk.ToString()+"\nfarklat"+farklat.ToString()+"\nfarklng:"+farklng.ToString()+"\ncarpim:"+carpim.ToString()+"\nsayi:"+sayi.ToString()+"\nincrease:"+increase.ToString()+"\nkonum_fark:"+konum_fark.ToString()+"\nbolum"+bolum.ToString()+"\nresult"+result.ToString());

            string yol = "D:\\map_den - Copy\\map_den\\map_den\\diff.txt";
            //File.WriteAllText(yol,a);

            string text = " farklat=" + String.Format("--{0:0.##}--", farklat) + " farklng=" + String.Format("--{0:0.##}--", farklng) + " carpim=" + String.Format("--{0:0.##}--", carpim) + " sayi=" + String.Format("--{0:0.##}--", sayi);
            File.WriteAllText(path, text);
            return result;
        }

        private void NewMethod2(int zoomValue) // Zoom'a göre ne kadar ilerleyecegini düzenler
        {
            //zoomValue
            //MessageBox.Show("index in degeri" + comboBox1.SelectedIndex.ToString());
            switch (18 - comboBox1.SelectedIndex)
            {
                case 18:
                    if ((lng + Zoomvalues[0, 0] / 2 > lng_dogu) && (lat + Zoomvalues[0, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[0, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[0, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[0, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[0, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[0, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[0, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[0, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[0, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;
                case 17:
                    if ((lng + Zoomvalues[1, 0] / 2 > lng_dogu) && (lat + Zoomvalues[1, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[1, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[1, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[1, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[1, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[1, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[1, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[1, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[1, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;
                case 16:
                    if ((lng + Zoomvalues[2, 0] / 2 > lng_dogu) && (lat + Zoomvalues[2, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[2, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[2, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[2, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[2, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[2, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[2, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[2, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[2, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;
                case 15:
                    if ((lng + Zoomvalues[3, 0] / 2 > lng_dogu) && (lat + Zoomvalues[3, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[3, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[3, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[3, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[3, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[3, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[3, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[3, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[3, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;

                case 14:

                    if ((lng + Zoomvalues[4, 0] / 2 > lng_dogu) && (lat + Zoomvalues[4, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                        //MessageBox.Show("Buraya giriyor mu? ");
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[4, 0] / 2 > lng_dogu) // ekrandaki alan en dogudaki boylami zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[4, 0] * (0.5); // boylam batiya gitsin
                            sag = true;
                            lat += Zoomvalues[4, 1]; // enlem arttirilir
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[4, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[4, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[4, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[4, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[4, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;

                case 13:
                    if ((lng + Zoomvalues[5, 0] / 2 > lng_dogu) && (lat + Zoomvalues[5, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[5, 0] / 2 > lng_dogu) // en doğuyu zaten kapsıyorsa yukarı çıksın
                        {
                            lng = lng_bati + Zoomvalues[5, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[5, 1] * (0.4);
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[5, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu) // boylam dogudan daha dogudaysa
                    {
                        if (lat + Zoomvalues[5, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[5, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[5, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[5, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;

                case 12:
                    if ((lng + Zoomvalues[6, 0] / 2 > lng_dogu) && (lat + Zoomvalues[6, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[6, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[6, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[6, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[6, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[6, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[6, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[6, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[6, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;

                case 11:
                    if ((lng + Zoomvalues[7, 0] / 2 > lng_dogu) && (lat + Zoomvalues[7, 1] / 2 > lat_kuzey))
                    {
                        //MessageBox.Show("Is it here?");
                        flag = false;
                        progressBar1.Value = 100;
                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[7, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[7, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[7, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[7, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[7, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[7, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[7, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[7, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }

                    break;

                case 10:
                    if ((lng + Zoomvalues[8, 0] / 2 > lng_dogu) && (lat + Zoomvalues[8, 1] / 2 > lat_kuzey)) // lng den biraz daha doguya gidince lat_doguyu asmiyor ve lat tan biraz yukari çikinca lat_kuzeyi asmiyor isek
                    {
                        flag = false;
                        progressBar1.Value = 100;

                    }
                    else if (lng <= lng_dogu) // lng, lng_dogunun batisinda ise
                    {
                        if (lng + Zoomvalues[8, 0] / 2 > lng_dogu) // lng den biraz daha doguya gidince lng_doguyu asmiyor ise
                        {
                            lng = lng_bati + Zoomvalues[8, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[8, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[8, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[8, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[8, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[8, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[8, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }
                    break;


                case 9:
                    if ((lng + Zoomvalues[9, 0] / 2 > lng_dogu) && (lat + Zoomvalues[9, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;
                    }

                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[9, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[9, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[9, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[9, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[9, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[9, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[9, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[9, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }
                    break;

                case 8:
                    if ((lng + Zoomvalues[10, 0] / 2 > lng_dogu) && (lat + Zoomvalues[10, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;

                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[10, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[10, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[10, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[10, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[10, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[10, 0] * (0.5);
                            sag = true;
                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[10, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[10, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }
                    break;

                case 7:
                    if ((lng + Zoomvalues[11, 0] / 2 > lng_dogu) && (lat + Zoomvalues[11, 1] / 2 > lat_kuzey))
                    {
                        flag = false;
                        progressBar1.Value = 100;

                    }
                    else if (lng <= lng_dogu)
                    {
                        if (lng + Zoomvalues[11, 0] / 2 > lng_dogu)
                        {
                            lng = lng_bati + Zoomvalues[11, 0] * (0.4);
                            sag = true;
                            lat += Zoomvalues[11, 1];
                        }
                        else
                        {
                            sag = true;
                            lng += Zoomvalues[11, 0];
                            yon = "SAG";
                        }
                    }
                    else if (lng > lng_dogu)
                    {
                        if (lat + Zoomvalues[11, 1] / 2 > lat_kuzey) // ekrandaki alan en kuzeydeki alani zaten kapsiyorsa
                        {
                            lng = lng_bati + Zoomvalues[11, 0] * (0.5);
                            sag = true;

                        }
                        else                          // kapsamiyorsa
                        {
                            lng = lng_bati + Zoomvalues[11, 0] * (0.5);
                            sag = true;
                            lat += Zoomvalues[11, 1] * (0.6); // enlem degeri arttirilir
                        }
                    }
                    break;

                default:
                    break;
            }
            if (progressBar1.Value == 100)
            {
                //int saniye = Convert.ToInt32(textBox5.Text);
                //Thread.Sleep(TimeSpan.FromSeconds(saniye));

                DialogResult result = MessageBox.Show("Tarama bitti, harita yeniden başlatılsın mı? ", "Gmap", MessageBoxButtons.YesNo);
                switch (result)
                {
                    case DialogResult.Yes:
                        gmap.Overlays.RemoveAt(0);
                        gmap.Refresh();

                        Application.Restart();
                        Environment.Exit(0);
                        break;
                    case DialogResult.No:
                        break;
                }
            }
        }


        private void NewMethod1(int zoomValue)
        {
            //zoomValue;
            switch (18 - comboBox1.SelectedIndex)
            {
                case 18:
                    if (!(lng + Zoomvalues[0, 0] * (0.4) > lng_dogu) && !(lat + Zoomvalues[0, 1] * (0.4) > lat_kuzey))
                    {
                        lng += Zoomvalues[0, 0] / 2;
                        lat += Zoomvalues[0, 1] * (0.4);
                    }
                    break;
                case 17:
                    if (!(lng + Zoomvalues[1, 0] * (0.4) > lng_dogu) && !(lat + Zoomvalues[1, 1] * (0.4) > lat_kuzey))
                    {
                        lng += Zoomvalues[1, 0] / 2;
                        lat += Zoomvalues[1, 1] * (0.4);
                    }
                    break;
                case 16:
                    if (!(lng + Zoomvalues[2, 0] * (0.4) > lng_dogu) && !(lat + Zoomvalues[2, 1] * (0.4) > lat_kuzey))
                    {
                        lng += Zoomvalues[2, 0] / 2;
                        lat += Zoomvalues[2, 1] * (0.4);
                    }
                    break;
                case 15:
                    if (!(lng + Zoomvalues[3, 0] * (0.4) > lng_dogu) && !(lat + Zoomvalues[3, 1] * (0.4) > lat_kuzey))
                    {
                        lng += Zoomvalues[3, 0] / 2;
                        lat += Zoomvalues[3, 1] * (0.4);
                    }
                    break;
                case 14:
                    //MessageBox.Show("case 14: ");
                    if (!(lng + Zoomvalues[4, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[4, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[4, 0] / 2;
                        lat += Zoomvalues[4, 1] * (0.4);
                    }
                    break;
                case 13:
                    if (!(lng + Zoomvalues[5, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[5, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[5, 0] / 2;
                        lat += Zoomvalues[5, 1] * (0.4);
                    }
                    break;
                case 12:
                    if (!(lng + Zoomvalues[6, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[6, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[6, 0] / 2;
                        lat += Zoomvalues[6, 1] * (0.4);
                    }
                    break;
                case 11:
                    if (!(lng + Zoomvalues[7, 0] * (0.4) > lng_dogu) && !(lat + Zoomvalues[7, 1] * (0.4) > lat_kuzey))
                    {
                        lng += Zoomvalues[7, 0] / 2;
                        lat += Zoomvalues[7, 1] * (0.4);
                    }
                    break;
                case 10:
                    if (!(lng + Zoomvalues[8, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[8, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[8, 0] / 2;
                        lat += Zoomvalues[8, 1] * (0.4);
                    }
                    break;
                case 9:
                    if (!(lng + Zoomvalues[9, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[9, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[9, 0] / 2;
                        lat += Zoomvalues[9, 1] * (0.4);
                    }
                    break;
                case 8:
                    if (!(lng + Zoomvalues[10, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[10, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[10, 0] / 2;
                        lat += Zoomvalues[10, 1] * (0.4);
                    }
                    break;
                case 7:
                    if (!(lng + Zoomvalues[11, 0] / 2 > lng_dogu) && !(lat + Zoomvalues[11, 1] / 2 > lat_kuzey))
                    {
                        lng += Zoomvalues[11, 0] / 2;
                        lat += Zoomvalues[11, 1] * (0.4);
                    }
                    break;

                default:
                    break;
            }
        }




        /*private void button7_Click(object sender, EventArgs e) // Time
        {
            double second = calc_seconds(lat_kuzey, lat_guney, lng_bati, lng_dogu); // Toplam saniye süresini hesaplar

            
            textBox1.Text = lat_kuzey.ToString();


            if (second > 3600)
            {
                int i, j = -1;
                for (i = 0; i < second; i = i + 3600, j++) ;
                int saat = j;
                int dakika = Convert.ToInt32(((second - (j * 3600))) / 60);
                metin = "Tahmini işlem süresi: " + Convert.ToString(saat) + " saat " + Convert.ToString(dakika) + " dakika";

            }
            else if (second > 60)
            {
                int dakika = Convert.ToInt32(second / 60);
                metin = "Tahmini işlem süresi: " + Convert.ToString(dakika) + " dakika";
            }
            else
            {
                metin = "Tahmini işlem süresi: " + String.Format("{0:0.##}", second) + " saniye";
            }
            MessageBox.Show(metin);


            return;
        }*/

        private void checkBox2_CheckedChanged(object sender, EventArgs e) // Server and Cache
        {
            if (checkBox2.Checked)
            {
                server_check++;
                checkBox1.Checked = false;

                GMaps.Instance.Mode = AccessMode.ServerAndCache;
                button3.Enabled = true;
            }
            else
            {
                //checkBox1.Enabled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                GMaps.Instance.Mode = AccessMode.CacheOnly;
                button3.Enabled = true;
            }
            else
            {
                //checkBox2.Enabled = true;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e) // kuzey enlemi 
        {
            textBox4.TextChanged += new EventHandler(this.textBox4_TextChanged);

        }

        private void textBox3_TextChanged(object sender, EventArgs e) // bati boylami
        {
            textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);

        }

        private void textBox2_TextChanged(object sender, EventArgs e)// güney enlemi
        {
            textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged); //Overload erroru bunu yazınca gitti
        }

        private void textBox1_TextChanged(object sender, EventArgs e) // doğu boylamı
        {
            textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);

        }

        private void button6_Click(object sender, EventArgs e) // Taramaya devam et
        {
            timer1.Enabled = true;
            flag = true;
            flag_bar = true;
            //gmap.Zoom = 18 - comboBox1.SelectedIndex; ?????
            gmap.Zoom = zoom;
            click_count = 0;
            mouse_left = false;
            mouse_right = false;
            timer1.Start();
            c = 0;
            progressBar1.Value = value;
            PointLatLng point = new PointLatLng(prev_lat, prev_lng);
            lat = point.Lat;
            lng = point.Lng;

            setMarker(point);

            click_count = 0;
        }

        private void textBox5_TextChanged(object sender, EventArgs e) // Timer1 interval
        {
            
            try 
	        {	        
		        timer_internal = Convert.ToInt32(textBox5.Text);
	        }
	        catch (Exception)
	        {
                timer_internal = 30;
		        //throw;  ????
	        }

            string f = Metin();

            label14.Text = f;       

        }
        private void textbox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.None)
            {
                timer_internal = Convert.ToInt32(textBox5.Text);
                this.timer1.Interval = timer_internal * 1000;
                this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            }
            

        }
        

        

        private void textbox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                timer_internal = Convert.ToInt32(textBox5.Text);
                this.timer1.Interval = timer_internal * 1000;
                this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            if (progressBar1.Value == 100)
            {
                comboBox1.Enabled = true;
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }


        public string Metin() 
        {
            double second = calc_seconds(lat_kuzey, lat_guney, lng_bati, lng_dogu); // Toplam saniye süresini hesaplar
                
            if (second > 3600)
            {
                int i, j = -1;
                for (i = 0; i < second; i = i + 3600, j++) ;
                int saat = j;
                int dakika = Convert.ToInt32(((second - (j * 3600))) / 60);
                return Convert.ToString(saat) + " saat " + Convert.ToString(dakika) + " dakika";

            }
            else if (second > 60)
            {
                int dakika = Convert.ToInt32(second / 60);
                return Convert.ToString(dakika) + " dakika";
            }
            else if (second < 30)
            {
                return "30 saniye";
            }
            else if (second > 30 && second < 61)
            {
                return "1 dakika";
            }
            else
            {
                return String.Format("{0:0.##}", second) + " saniye";
            }



        }

        private void label14_Click(object sender, EventArgs e) // Tahmini tarama süresi
        {
            label14.Text = Metin();
            return;
        }

        private void label15_Click(object sender, EventArgs e) // Data Size
        {
            string path = "D:\\map_den - Copy\\map_den\\bin\\Debug\\cacheSa3\\TileDBv5\\en";
            

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fiArr = di.GetFiles();
            foreach (FileInfo f in fiArr){
                label15.Text = (f.Length.ToString("#,##0")+" bytes"); 
            }
            fiArr = null;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                using (var stream2 = client.OpenRead("https://www.youtube.com"))
                {
                    label16.BackColor = Color.GreenYellow;
                    label16.Text = "Ağa Bağlı";
                }
            }
            catch
            {       label16.BackColor = Color.Red;
                    label16.Text = "Ağa Bağlanılamıyor";

                    DialogResult result = MessageBox.Show("İnternete bağlanılamıyor, Tarama durdurulsunsun mu? (Harita bilgilerini Data.gmdb'e yükleyebilmek için interneti açınız)  ", "Gmap", MessageBoxButtons.YesNo);
                    switch (result)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.OK:
                            break;
                        default:
                            break;
                    }
            }
            
          
        }
















    }
    
}

