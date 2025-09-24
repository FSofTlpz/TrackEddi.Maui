using FSofTUtils.Geography.PoorGpx;
using SpecialMapCtrl;
using System.ComponentModel;
using System.Diagnostics;
using TrackEddi.Common;

namespace TrackEddi {
   public class WorkbenchContentPage_ListViewObjectItem : INotifyPropertyChanged {

      public event PropertyChangedEventHandler? PropertyChanged;

      public Track? Track { get; protected set; }

      public Marker? Marker { get; protected set; }


      /// <summary>
      /// Ist <see cref="Track"/> oder <see cref="Marker"/>.
      /// </summary>
      public bool IsTrack => Track != null;

      public bool IsVisible {
         get => IsTrack ?
                     Track != null ? Track.IsVisible : false :
                     Marker != null ? Marker.IsVisible : false;
         set {
            if (IsTrack) {
               if (Track != null && Track.IsVisible != value) {
                  Track.IsVisible = value;
                  Notify4PropChanged(nameof(IsVisible));
               }
            } else {
               if (Marker != null && Marker.IsVisible != value) {
                  Marker.IsVisible = value;
                  Notify4PropChanged(nameof(IsVisible));
               }
            }
         }
      }

      public string Text1 => IsTrack ?
                                 Track != null ? Track.Trackname : string.Empty :
                                 Marker != null ? Marker.Text : string.Empty;

      public string Text2 {
         get {
            if (IsTrack) {
               if (Track == null)
                  return string.Empty;
               try {
                  double len = Track.LengthTS();
                  string minDT = BaseElement.ValueIsUsed(Track.StatMinDateTime) &&
                                 BaseElement.ValueIsValid(Track.StatMinDateTime) ?
                                       Track.StatMinDateTime.ToString("g") + " UTC" :
                                       string.Empty;
                  string maxDT = BaseElement.ValueIsUsed(Track.StatMaxDateTime) &&
                                 BaseElement.ValueIsValid(Track.StatMaxDateTime) ?
                                       Track.StatMaxDateTime.ToString("g") + " UTC" :
                                       string.Empty;

                  return (len < 1000 ?
                              (len.ToString("f0") + "m") :
                              ((len / 1000).ToString("f1") + "km"))
                            + (minDT.Length != 0 || maxDT.Length != 0 ? ", " : string.Empty)
                            + minDT
                            + (maxDT.Length != 0 ? " .. " : string.Empty)
                            + maxDT;
               } catch (Exception ex) {
                  string msg = UIHelper.GetExceptionMessage(ex);
                  UIHelper.Message2Logfile(nameof(WorkbenchContentPage_ListViewObjectItem.Text2), msg, null);
               }
            }

            return Marker != null ? Marker.Symbolname : string.Empty;
         }
      }

      /// <summary>
      /// nur für <see cref="Track"/>
      /// </summary>
      public Color TrackColor {
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
         get => IsTrack ?
                     WinHelper.ConvertColor(Track.LineColor) :
                     Colors.DarkGray;      // Dummy
         set {
            if (IsTrack) {
               System.Drawing.Color col = WinHelper.ConvertColor(value);
               if (col != Track.LineColor) {
                  Track.LineColor = col;
                  Notify4PropChanged(nameof(TrackColor));
               }
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
            }
         }
      }

      /// <summary>
      /// nur für <see cref="Marker"/>
      /// </summary>
      public ImageSource? Picture { get; protected set; }

      //ImageSource? _picture;

      //public ImageSource? Picture {
      //   get => _picture;
      //   protected set => _picture = value;
      //}


      /// <summary>
      /// akt. Bilddaten für <see cref="Picture"/>
      /// </summary>
      byte[]? pictdata;


      public WorkbenchContentPage_ListViewObjectItem(Track t) {
         Track = t;
      }

      public WorkbenchContentPage_ListViewObjectItem(Marker m) {
         Marker = m;
         if (Marker.Bitmap != null) {
            pictdata = WinHelper.GetImageSource4WindowsBitmap(Marker.Bitmap, out ImageSource picture);
            Picture = picture;
         }
      }

      /// <summary>
      /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern" möglich!)
      /// </summary>
      /// <param name="propname"></param>
      public void Notify4PropChanged(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

      public void SetMarkerPicture(string newsymbolname) {
         if (Marker != null) {
            Marker.Symbolname = newsymbolname;
            if (Marker.Bitmap != null) {
               pictdata = WinHelper.GetImageSource4WindowsBitmap(Marker.Bitmap, out ImageSource picture);
               Picture = picture;
               Notify4PropChanged(nameof(Picture));
            }
         }
      }

      public override string ToString() {
         return string.Format("{0}: Text1={1}, Text2={2}",
                              IsTrack ? "Track" : "Waypoint",
                              Text1,
                              Text2);
      }

   }

}
