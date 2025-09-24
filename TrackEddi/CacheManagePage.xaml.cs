using SpecialMapCtrl;
using System.Collections.ObjectModel;
using System.Text;
using TrackEddi.Common;

namespace TrackEddi;

public partial class CacheManagePage : ContentPage {

   SpecialMapCtrl.SpecialMapCtrl? map;

   int mapProviderDefIdx = -1;
   int actualListIdx = -1;

   FilecacheManager.FilecacheInfo? filecacheInfo;

   readonly ObservableCollection<string> cachelst = new ObservableCollection<string>();


   public CacheManagePage() {
      InitializeComponent();

      BindingContext = this;
      lstCacheAct.ItemsSource = cachelst;
   }

   public CacheManagePage(SpecialMapCtrl.SpecialMapCtrl? map, int mapproviderdefidx) : this() {
      this.map = map;
      mapProviderDefIdx = mapproviderdefidx;
   }

   public async Task ActualizeContent() => await showData();

   async Task<FilecacheManager.FilecacheInfo?> getData() {
      FilecacheManager.FilecacheInfo? cacheInfo = null;
      await Task.Run(() => {
         cacheInfo = map?.GetFilecacheInfo();
      });
      return cacheInfo;
   }

   /// <summary>
   /// akt. Daten ermitteln und anzeigen
   /// </summary>
   /// <returns></returns>
   async Task showData() {
      await showPageBusy();

      if (map != null) {
         lblPath.Text = map.M_CacheLocation;

         filecacheInfo = map.GetFilecacheInfo();

         lblCachesizeAct.Text =
         lblCachesizeAll.Text = string.Empty;
         cachelst.Clear();
         actualListIdx = -1;

         filecacheInfo = await getData();
         showSumBytes(filecacheInfo);

         if (filecacheInfo != null) {
            foreach (var mi in filecacheInfo.CacheInfos) {
               StringBuilder sb = new StringBuilder();
               sb.Append("(" + mi.CacheName + ")");
               if (mi.DbIdDelta > 0)
                  sb.Append(" (" + mi.DbIdDelta + ")");
               sb.Append(" \"" + mi.Mapname + "\"");
               sb.Append(" (" + mi.ProviderName + "):");
               if (mi.CacheExists)
                  sb.Append(" " + (mi.Bytes / 1024.0 / 1024.0).ToString("f2") + " MB in " +
                            mi.TileCount + " Kartenteil" + (mi.TileCount == 1 ? string.Empty : "en") + " und " +
                            mi.ZoomLevelsCount + " Zoomstufe" + (mi.ZoomLevelsCount == 1 ? string.Empty : "n"));

               if (!mi.CacheExists || !mi.IsUsed) {
                  sb.Append(" [CACHE ");
                  if (!mi.CacheExists) {
                     sb.Append("NICHT VORHANDEN");
                     if (!mi.IsUsed)
                        sb.Append(" und ");
                  }
                  if (!mi.IsUsed)
                     sb.Append("NICHT VERWENDET");
                  sb.Append("]");
               }

               cachelst.Add(sb.ToString());

               if (mi.MapProviderDefIdx == mapProviderDefIdx) {
                  lblActualMapname.Text = map.M_ProviderDefinitions[mapProviderDefIdx].MapName;
                  showActualMapBytes(mi.Bytes);
                  actualListIdx = cachelst.Count - 1;
                  lstCacheAct.SelectedItem = cachelst[actualListIdx];
               }
            }
         }
      }

      btnClearActualCache.IsEnabled = actualListIdx >= 0 && filecacheInfo != null ?
                                             filecacheInfo.CacheInfos[actualListIdx].Bytes > 0 :
                                             false;
      btnClearAllCache.IsEnabled = filecacheInfo?.Bytes > 0;

      IsBusy = false;
   }

   string getBytesText(long bytes) => (bytes / 1024.0 / 1024.0).ToString("f2") + " MB";

   void showSumBytes(FilecacheManager.FilecacheInfo? cacheInfo) => lblCachesizeAll.Text = getBytesText(cacheInfo != null ? cacheInfo.Bytes : 0);

   void showActualMapBytes(long bytes) => lblCachesizeAct.Text = getBytesText(bytes);

   /// <summary>
   /// Cache der akt. Karte löschen
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private async void clearActualCache_Clicked(object sender, EventArgs e) => await clear(actualListIdx);

   /// <summary>
   /// gesamten Cache löschen
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private async void clearAllCache_Clicked(object sender, EventArgs e) => await clear(-1);

   /// <summary>
   /// Cache der Karte in der Liste löschen
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   private async void clearCacheList_Tapped(object sender, EventArgs e) {
      if (sender is ImageButton ib) {
         int idx = getIdx(ib, cachelst);
         if (idx >= 0)
            await clear(idx);
      }
   }

   int getIdx(ImageButton ib, ObservableCollection<string> cachelst) {
      string? txt = ib.CommandParameter.ToString();
      for (int i = 0; i < cachelst.Count; i++)
         if (cachelst[i] == txt)
            return i;
      return -1;
   }

   async Task clear(int listidx) {
      if (map != null && filecacheInfo != null)
         if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                             listidx >= 0 ?
                                    "Cache für die Karte " + Environment.NewLine + Environment.NewLine +
                                       "'(" + filecacheInfo.CacheInfos[listidx].CacheName + ") " + filecacheInfo.CacheInfos[listidx].Mapname + "'" + Environment.NewLine + Environment.NewLine +
                                       " löschen?" :
                                    "Gesamten Cache löschen?",
                             "Cache löschen")) {
            await showPageBusy();

            map.M_ClearMemoryCache();

            string error;
            int count = 0;
            bool multiuseremove = false;
            if (listidx >= 0)
               (count, multiuseremove, error) = await map.ClearFileCache(filecacheInfo.CacheInfos[listidx]);
            else
               (count, multiuseremove, error) = await map.ClearFileCache(filecacheInfo.CacheInfos);

            IsBusy = false;

            if (error != string.Empty)
               await UIHelper.ShowErrorMessage(this,
                                               "Fehler beim Löschen: " + System.Environment.NewLine + System.Environment.NewLine + error,
                                               "Ergebnis");
            else {
               await UIHelper.ShowInfoMessage(this, count + " Kartenteile gelöscht", "Ergebnis");
               if (listidx >= 0) {
                  cachelst.RemoveAt(listidx);
                  filecacheInfo.CacheInfos.RemoveAt(listidx);
                  if (actualListIdx == listidx)
                     showActualMapBytes(0);
                  showSumBytes(filecacheInfo);
                  if (listidx < filecacheInfo.CacheInfos.Count)
                     lstCacheAct.SelectedItem = cachelst[listidx];
               } else {
                  cachelst.Clear();
                  filecacheInfo.CacheInfos.Clear();
                  await showData();
               }
            }
            map.M_Refresh(true, true, false, false);    // auch den Memory-Cache löschen
         }
   }

   async Task showPageBusy() {
      IsBusy = true;
      await Task.Delay(10);      // "Trick": der ActivityIndicator erscheint schneller
   }

}