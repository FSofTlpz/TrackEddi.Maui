using FSofTUtils.OSInterface.Control;
using GMap.NET.FSofTExtented.MapProviders;
using TrackEddi.Common;
using TrackEddi.ConfigEdit;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class MapChoosingPage : ContentPage {


      public SpecialMapCtrl.SpecialMapCtrl? MapControl;

      public List<int[]>? ProvIdxPaths;

      public Config? Config;

      public AppData? AppData;

      List<int> lastusedmapsidx = new List<int>();



      class ListViewObjectItem {

         MapProviderDefinition mpd;

         public string Text {
            get => mpd.MapName + " (" + mpd.ProviderName + ")";
         }

         public ListViewObjectItem(MapProviderDefinition def) {
            mpd = def;
         }

         public override string ToString() {
            return string.Format("{0}", Text);
         }

      }

      List<ListViewObjectItem> listViewObjectItems = new List<ListViewObjectItem>();


      public class MapChoosingEventArgs : EventArgs {
         /// <summary>
         /// aktueller Index der <see cref="MapProviderDefinition"/>
         /// </summary>
         public int ActualIdx { get; private set; }

         /// <summary>
         /// Index der ausgewählten <see cref="MapProviderDefinition"/>
         /// </summary>
         public int NewIdx { get; private set; }

         public MapChoosingEventArgs(int newidx, int actualidx) {
            NewIdx = newidx;
            ActualIdx = actualidx;
         }

      }

      /// <summary>
      /// Auswahl ist beendet
      /// </summary>
      public event EventHandler<MapChoosingEventArgs>? MapChoosingEvent;



      public MapChoosingPage() {
         InitializeComponent();

         //tv.OnBeforeCheckedChanged += Tv_OnBeforeCheckedChanged;
         //tv.OnCheckedChanged += Tv_OnCheckedChanged;

         //tv.OnBeforeExpandedChanged += Tv_OnBeforeExpandedChanged;
         //tv.OnExpandedChanged += TV_OnExpandedChanged;

         //tv.OnSelectedNodeChanged += TV_OnSelectedNodeChanged;
         tv.OnNodeTapped += TV_OnNodeTapped;
         //tv.OnNodeDoubleTapped += TV_OnNodeDoubleTapped;

         //tv.OnNodeSwiped += TV_OnNodeSwiped;

      }

      protected override void OnAppearing() {
         base.OnAppearing();
         if (MapControl != null)
            buildTreeViewContent(Config, AppData, tv, MapControl.M_ProviderDefinitions, ProvIdxPaths);
      }

      //void clearTreeViewNodes(IList<TreeViewNode> nodes) {
      //   if (nodes != null) {
      //      foreach (TreeViewNode node in nodes)
      //         if (node.HasChildNodes)
      //            clearTreeViewNodes(node.GetChildNodes());
      //      nodes.Clear();
      //   }
      //}

      //void clearTreeViewNodes(TreeView tv) {
      //   if (tv.HasChildNodes) {
      //      IList<TreeViewNode> nodes = tv.GetChildNodes();
      //      clearTreeViewNodes(nodes);
      //      nodes.Clear();
      //   }
      //}

      int getIdx4Mapname(string mapname, IList<MapProviderDefinition> providerdefs) {
         for (int j = 0; j < providerdefs.Count; j++)
            if (providerdefs[j].MapName == mapname)
               return j;
         return -1;
      }

      void buildTreeViewContent(Config? config,
                                AppData? appData,
                                TreeView tv,
                                IList<MapProviderDefinition> providerdefs,
                                IList<int[]>? providxpaths) {
         if (MapControl != null &&
             config != null &&
             appData != null &&
             providxpaths != null) {
            MapTreeViewHelper.BuildTreeViewContent(config, tv, providerdefs, providxpaths, MapControl.M_ActualMapIdx);

            int lastusedmapsmax = Math.Max(0, config.LastUsedMapsCount);
            if (lastusedmapsmax > 0) {
               tv.InsertChildNode(0, new TreeViewNode("⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯") { IsEnabled = false });
               lastusedmapsidx.Clear();
               List<string> mapnames = appData.LastUsedMapnames;
               if (mapnames != null) {
                  while (lastusedmapsmax < mapnames.Count)
                     mapnames.RemoveAt(0);
                  for (int i = 0; i < mapnames.Count; i++) {
                     int idx = getIdx4Mapname(mapnames[i], providerdefs);
                     if (0 <= idx) {
                        tv.InsertChildNode(0, new TreeViewNode(mapnames[i], idx));
                        lastusedmapsidx.Add(idx);
                     }
                  }
               }
            }
         }
      }
      //private void Tv_OnCheckedChanged(object sender, TreeView.TreeViewNodeEventArgs e) { }

      //private void Tv_OnBeforeCheckedChanged(object sender, TreeView.TreeViewNodeStatusChangedEventArgs e) { }

      //private void Tv_OnBeforeExpandedChanged(object sender, TreeView.TreeViewNodeStatusChangedEventArgs e) { }

      //private void TV_OnNodeSwiped(object sender, TreeView.TreeViewNodeEventArgs e) { }

      //private void TV_OnNodeDoubleTapped(object sender, TreeView.TreeViewNodeEventArgs e) { }

      //private void TV_OnSelectedNodeChanged(object sender, TreeView.TreeViewNodeChangedEventArgs e) { }

      //private void TV_OnExpandedChanged(object sender, TreeView.TreeViewNodeEventArgs e) { }

      /// <summary>
      /// ev. Auswahl einer Karte
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void TV_OnNodeTapped(object? sender, TreeView.TreeViewNodeEventArgs e) {
         if (e.TreeViewNode != null)
            if (MapControl != null &&
                Config != null &&
                AppData != null &&
                e.TreeViewNode.ExtendedData != null) {
               string mapname = e.TreeViewNode.Text;
               int newprovidx = getIdx4Mapname(mapname, MapControl.M_ProviderDefinitions);
               if (newprovidx != MapControl.M_ActualMapIdx) {
                  List<string> mapnames = AppData.LastUsedMapnames;
                  int m = mapnames.IndexOf(mapname);
                  if (0 <= m)
                     mapnames.RemoveAt(m);
                  mapnames.Add(mapname);
                  while (Config.LastUsedMapsCount < mapnames.Count)
                     mapnames.RemoveAt(0);
                  AppData.LastUsedMapnames = mapnames;
                  AppData.LastMapname = mapname;
                  await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
                  MapChoosingEvent?.Invoke(this, new MapChoosingEventArgs(newprovidx, MapControl.M_GetActiveProviderIdx()));
               }
            } else {
               if (e.TreeViewNode.HasChildNodes)
                  e.TreeViewNode.Expanded = !e.TreeViewNode.Expanded;
            }
      }


   }
}