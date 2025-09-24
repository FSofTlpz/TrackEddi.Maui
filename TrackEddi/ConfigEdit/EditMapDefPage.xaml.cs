using FSofTUtils.OSInterface.Page;
using GMap.NET.FSofTExtented.MapProviders;
using System.Collections.ObjectModel;
using TrackEddi.Common;

namespace TrackEddi.ConfigEdit {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class EditMapDefPage : ContentPage {
      /// <summary>
      /// zur Datenübergabe, wenn erfolgreich (gespeichert)
      /// </summary>
      public MapProviderDefinition? MapProviderDefinition { get; protected set; }

      public bool Ok { get; protected set; } = false;

      /// <summary>
      /// Neue Karte oder Bearbeitung einer bestehenden Karte?
      /// </summary>
      public bool IsNewMapProviderDefinition = false;

      /// <summary>
      /// Karte für einen <see cref="MultiMapProvider"/>?
      /// </summary>
      public bool IsSubLayer = false;

      List<string> providernames = new List<string>();


      MultiMapProvider.MultiMapDefinition? multimapDef = null;

      class SpecialListView {

         public event EventHandler? OnSelectedIdxChanged;


         public readonly ObservableCollection<ListViewItem> Items = new ObservableCollection<ListViewItem>();

         int _selectedIdx = -1;

         public int SelectedIdx {
            get => _selectedIdx;
            set {
               if (_selectedIdx != value) {
                  if (0 <= _selectedIdx && _selectedIdx < Items.Count)
                     Items[_selectedIdx].IsSelected = false;

                  _selectedIdx = 0 <= value && value < Items.Count ?
                                          value :
                                          -1;

                  if (0 <= _selectedIdx && _selectedIdx < Items.Count)
                     Items[_selectedIdx].IsSelected = true;

                  OnSelectedIdxChanged?.Invoke(this, EventArgs.Empty);
               }
            }
         }


         public bool ItemExists(string txt) => ItemHasIdx(txt) >= 0;

         public int ItemHasIdx(string txt) {
            for (int i = 0; i < Items.Count; i++)
               if (Items[i].Text == txt)
                  return i;
            return -1;
         }

         public void ItemMoveUp(int idx) {
            string itemtxt = Items[idx].Text;
            Items.RemoveAt(idx);
            Items.Insert(idx - 1, new ListViewItem(itemtxt));
            SelectedIdx = idx - 1;
         }

         public void ItemMoveDown(int idx) {
            string itemtxt = Items[idx].Text;
            Items.RemoveAt(idx);
            Items.Insert(idx + 1, new ListViewItem(itemtxt));
            SelectedIdx = idx + 1;
         }

         public void ItemRemove(int idx) {
            Items.RemoveAt(idx);
            if (SelectedIdx <= idx)
               SelectedIdx--;
         }

         public void ItemInsert(int idx, string txt) {
            if (0 <= idx && idx <= Items.Count) {
               Items.Insert(idx, new ListViewItem(txt));
               SelectedIdx = idx;
            } else {
               Items.Add(new ListViewItem(txt));
               SelectedIdx = Items.Count - 1;
            }
         }

      }

      SpecialListView maplist;

      enum FileTypeRegister {
         KMZ,
         TDB,
         TYP,
      }

      /// <summary>
      /// Standardeigenschaft überschreiben
      /// </summary>
      new bool IsBusy {
         get => BusyIndicator.IsRunning;
         set {
            if (MainThread.IsMainThread)
               BusyIndicator.IsRunning = value;
            else
               MainThread.BeginInvokeOnMainThread(() => BusyIndicator.IsRunning = value);
         }
      }


      public EditMapDefPage(MapProviderDefinition definition, bool newmap) {
         InitializeComponent();

         IsNewMapProviderDefinition = newmap;
         maplist = new SpecialListView();
         maplist.OnSelectedIdxChanged += (s, e) => {
            SpecialListView? slv = (SpecialListView?)s;
            if (slv != null) {
               btnDelete.IsEnabled = slv.SelectedIdx >= 0;
               btnUp.IsEnabled = slv.SelectedIdx > 0;
               btnDown.IsEnabled = 0 <= slv.SelectedIdx && slv.SelectedIdx < slv.Items.Count - 1;
            }
         };
         lstMaps.ItemsSource = maplist.Items;

         init(definition);
      }

      async void init(MapProviderDefinition definition) {
         await showMainpageBusy();

         // nur Kopie der Kartendef. anlegen
         if (definition is GarminProvider.GarminMapDefinition)
            MapProviderDefinition = new GarminProvider.GarminMapDefinition(definition as GarminProvider.GarminMapDefinition);
         else if (definition is GarminKmzProvider.KmzMapDefinition)
            MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(definition as GarminKmzProvider.KmzMapDefinition);
         else if (definition is WMSProvider.WMSMapDefinition)
            MapProviderDefinition = new WMSProvider.WMSMapDefinition(definition as WMSProvider.WMSMapDefinition);
         else if (definition is HillshadingProvider.HillshadingMapDefinition)
            MapProviderDefinition = new HillshadingProvider.HillshadingMapDefinition(definition as HillshadingProvider.HillshadingMapDefinition);
         else if (definition is MultiMapProvider.MultiMapDefinition)
            MapProviderDefinition = new MultiMapProvider.MultiMapDefinition(definition as MultiMapProvider.MultiMapDefinition);
         else
            MapProviderDefinition = new MapProviderDefinition(definition);

         // Providerliste füllen
         providernames.Add(GarminProvider.Instance.Name);
         providernames.Add(GarminKmzProvider.Instance.Name);
         providernames.Add(WMSProvider.Instance.Name);
         if (IsSubLayer)
            providernames.Add(HillshadingProvider.Instance.Name);
         if (!IsSubLayer)
            providernames.Add(MultiMapProvider.Instance.Name);
         foreach (var item in GMap.NET.MapProviders.GMapProviders.List)
            providernames.Add(item.Name);

         pickerProvider.ItemsSource = providernames;
         pickerProvider.SelectedItem = MapProviderDefinition.ProviderName;
         pickerProvider.IsEnabled = IsNewMapProviderDefinition;

         entryMapname.Text = MapProviderDefinition.MapName;
         entryZoomFrom.Text = MapProviderDefinition.MinZoom.ToString();
         entryZoomTo.Text = MapProviderDefinition.MaxZoom.ToString();

         if (IsSubLayer) {
            entryZoomFrom.IsEnabled =
            entryZoomTo.IsEnabled = false;
         }

         // i.A. ohne Hillshading
         cbHillshading.IsEnabled =
         entryHillshadingalpha.IsEnabled = false;

         if (MapProviderDefinition is GarminProvider.GarminMapDefinition) {

            GarminProvider.GarminMapDefinition? specmpd = (GarminProvider.GarminMapDefinition)MapProviderDefinition;
            if (specmpd != null) {
               lblTdbFile.Text = specmpd.TDBfile[0];
               lblTypFile.Text = specmpd.TYPfile[0];
               entryTextFactor.Text = IsNewMapProviderDefinition ? "1" : specmpd.TextFactor.ToString();                 // IsNewMapProviderDefinition nötig?
               entrySymbolFactor.Text = IsNewMapProviderDefinition ? "1" : specmpd.SymbolFactor.ToString();             // IsNewMapProviderDefinition nötig?
               entryLineFactor.Text = IsNewMapProviderDefinition ? "1" : specmpd.LineFactor.ToString();                 // IsNewMapProviderDefinition nötig?
               cbHillshading.IsChecked = IsNewMapProviderDefinition ? false : specmpd.HillShading;                      // IsNewMapProviderDefinition nötig?
               entryHillshadingalpha.Text = IsNewMapProviderDefinition ? "100" : specmpd.HillShadingAlpha.ToString();   // IsNewMapProviderDefinition nötig?
            }
            entryTextFactor.IsEnabled =
            entrySymbolFactor.IsEnabled =
            entryLineFactor.IsEnabled =
            entryHillshadingalpha.IsEnabled =
            cbHillshading.IsEnabled = true;

         } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

            GarminKmzProvider.KmzMapDefinition? specmpd = (GarminKmzProvider.KmzMapDefinition)MapProviderDefinition;
            if (specmpd != null) {
               lblKmzFile.Text = specmpd.KmzFile;
               cbHillshading.IsChecked = IsNewMapProviderDefinition ? false : specmpd.HillShading;                      // IsNewMapProviderDefinition nötig?
               entryHillshadingalpha.Text = IsNewMapProviderDefinition ? "100" : specmpd.HillShadingAlpha.ToString();   // IsNewMapProviderDefinition nötig?
            }
            entryHillshadingalpha.IsEnabled =
            cbHillshading.IsEnabled = true;

         } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

            WMSProvider.WMSMapDefinition? specmpd = (WMSProvider.WMSMapDefinition)MapProviderDefinition;
            if (specmpd != null) {
               entryUrl.Text = specmpd.URL;
               entryVersion.Text = specmpd.Version;
               entrySrs.Text = specmpd.SRS;
               pickerWmsPictFormat.SelectedItem = specmpd.PictureFormat.ToUpper();
               entryLayer.Text = specmpd.Layer;
               entryExtParams.Text = specmpd.ExtendedParameters;
               cbHillshading.IsChecked = IsNewMapProviderDefinition ? false : specmpd.HillShading;                      // IsNewMapProviderDefinition nötig?
               entryHillshadingalpha.Text = IsNewMapProviderDefinition ? "100" : specmpd.HillShadingAlpha.ToString();   // IsNewMapProviderDefinition nötig?
            }
            entryHillshadingalpha.IsEnabled =
            cbHillshading.IsEnabled = true;

         } else if (MapProviderDefinition is HillshadingProvider.HillshadingMapDefinition) {

            HillshadingProvider.HillshadingMapDefinition? specmpd = MapProviderDefinition as HillshadingProvider.HillshadingMapDefinition;
            if (specmpd != null) {
               entryHillshadingalpha.Text = IsNewMapProviderDefinition ? "100" : specmpd.Alpha.ToString();   // IsNewMapProviderDefinition nötig?
            }
            entryHillshadingalpha.IsEnabled = true;
            //cbHillshading.IsEnabled = true;

         } else if (MapProviderDefinition is MultiMapProvider.MultiMapDefinition) {

            multimapDef = MapProviderDefinition as MultiMapProvider.MultiMapDefinition;
            if (multimapDef != null) {
               for (int i = 0; i < multimapDef.MapProviderDefinitions.Length; i++)
                  maplist.Items.Add(new ListViewItem(item4ListBoxMaps(multimapDef.MapProviderDefinitions[i])));
               if (multimapDef.MapProviderDefinitions.Length > 0) {
                  maplist.SelectedIdx = 0;
               } else {
                  btnDelete.IsEnabled =
                  btnUp.IsEnabled =
                  btnDown.IsEnabled = false;
               }
            }

         }

         IsBusy = false;
      }

      #region MultiMap-Karte

      private void btnUp_Tapped(object sender, EventArgs e) {
         int idx = maplist.SelectedIdx;
         if (0 < idx &&
             multimapDef != null) {
            maplist.ItemMoveUp(idx);

            MapProviderDefinition tmpdef = multimapDef.MapProviderDefinitions[idx];
            multimapDef.MapProviderDefinitions[idx] = multimapDef.MapProviderDefinitions[idx - 1];
            multimapDef.MapProviderDefinitions[idx - 1] = tmpdef;
         }
      }

      private void btnDown_Tapped(object sender, EventArgs e) {
         int idx = maplist.SelectedIdx;
         if (0 <= idx &&
             idx < maplist.Items.Count - 1 &&
             multimapDef != null) {
            maplist.ItemMoveDown(idx);

            MapProviderDefinition tmpdef = multimapDef.MapProviderDefinitions[idx];
            multimapDef.MapProviderDefinitions[idx] = multimapDef.MapProviderDefinitions[idx + 1];
            multimapDef.MapProviderDefinitions[idx + 1] = tmpdef;
         }
      }

      private async void btnDelete_Tapped(object sender, EventArgs e) {
         int idx = maplist.SelectedIdx;
         if (0 <= idx &&
             multimapDef != null) {
            await deleteMultimap(multimapDef, idx);
         }
      }

      private async void btnEdit_Tapped(object sender, EventArgs e) {
         int idx = maplist.SelectedIdx;
         if (0 <= idx &&
             multimapDef != null) {
            await editMultimap(multimapDef, idx);
         }
      }

      private async void btnAdd_Tapped(object sender, EventArgs e) {
         await addMultimap(maplist.SelectedIdx < 0 ? -1 : maplist.SelectedIdx + 1);
      }

      private void lblText_Tapped(object sender, TappedEventArgs e) {
         int idx = maplist.ItemHasIdx(((Label)sender).Text);
         if (idx >= 0 &&
             idx != maplist.SelectedIdx) {
            maplist.SelectedIdx = idx;
         }
      }

      async Task deleteMultimap(MultiMapProvider.MultiMapDefinition multimapdef, int idx) {
         if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                     "Die Ebene '" + maplist.Items[idx] + "' wirklich löschen?",
                                                     "Löschen")) {
            maplist.ItemRemove(idx);
            multimapdef.RemoveLevel(idx);
         }
      }

      async Task editMultimap(MultiMapProvider.MultiMapDefinition multimapdef, int idx) {
         MapProviderDefinition leveldef = multimapdef.MapProviderDefinitions[idx];
         if (leveldef != null) {
            await showMainpageBusy();

            leveldef.MinZoom = getInt(entryZoomFrom);
            leveldef.MaxZoom = getInt(entryZoomTo);
            EditMapDefPage page = new EditMapDefPage(leveldef, true) {
               IsNewMapProviderDefinition = false,
               IsSubLayer = true,
            };
            page.Disappearing += async (s, ea) => {
               if (page.Ok) {
                  try {
                     if (page.MapProviderDefinition != null &&
                         multimapdef.RemoveLevel(idx)) {
                        multimapdef.InsertLevel(page.MapProviderDefinition, idx);
                        string item = item4ListBoxMaps(page.MapProviderDefinition);
                        if (item != maplist.Items[idx].Text)
                           maplist.Items[idx].Text = item;
                     }
                  } catch (Exception ex) {
                     IsBusy = false;
                     await UIHelper.ShowExceptionMessage(this, "Fehler", ex, null, false);
                  }
               }
            };
            await FSofTUtils.OSInterface.Helper.GoTo(page);
            IsBusy = false;
         }
      }

      async Task addMultimap(int idx) {
         await showMainpageBusy();
         EditMapDefPage page = new EditMapDefPage(new MapProviderDefinition(string.Empty,
                                                                            GMap.NET.MapProviders.GMapProviders.OpenStreetMap.Name,
                                                                            getInt(entryZoomFrom),
                                                                            getInt(entryZoomTo)),
                                                  true) {
            IsNewMapProviderDefinition = true,
            IsSubLayer = true,
         };
         page.Disappearing += async (s, ea) => {
            if (page.Ok) {
               try {
                  if (page.MapProviderDefinition != null &&
                      multimapDef != null) {
                     string item = item4ListBoxMaps(page.MapProviderDefinition);
                     while (maplist.ItemExists(item)) {  // eindeutigen Namen erzeugen
                        page.MapProviderDefinition.MapName += "~";
                        item = item4ListBoxMaps(page.MapProviderDefinition);
                     }

                     if (idx < 0) {
                        multimapDef.InsertLevel(page.MapProviderDefinition);
                        maplist.ItemInsert(int.MaxValue, item);
                     } else {
                        multimapDef.InsertLevel(page.MapProviderDefinition, idx);
                        maplist.ItemInsert(idx, item);
                     }
                  }

               } catch (Exception ex) {
                  IsBusy = false;
                  await UIHelper.ShowExceptionMessage(this, "Fehler", ex, null, false);
               }
            }
         };
         await FSofTUtils.OSInterface.Helper.GoTo(page);
         IsBusy = false;
      }

      string item4ListBoxMaps(MapProviderDefinition def) {
         string mapitem = def.ProviderName + ": " +
                          def.MapName;
         if (def is GarminProvider.GarminMapDefinition) {
            mapitem += " - TDB=" + ((GarminProvider.GarminMapDefinition)def).TDBfile[0];
         } else if (def is GarminKmzProvider.KmzMapDefinition) {
            mapitem += " - KMZ=" + ((GarminKmzProvider.KmzMapDefinition)def).KmzFile;
         } else if (def is WMSProvider.WMSMapDefinition) {
            mapitem += " - URL=" + ((WMSProvider.WMSMapDefinition)def).URL;
         } else if (def is HillshadingProvider.HillshadingMapDefinition) {
            mapitem += " - Alpha=" + ((HillshadingProvider.HillshadingMapDefinition)def).Alpha.ToString();
         }
         return mapitem;
      }

      #endregion

      private async void ChooseKmzFile_Clicked(object sender, EventArgs e) => await registerFile(FileTypeRegister.KMZ);

      private async void ChooseTdbFile_Clicked(object sender, EventArgs e) => await registerFile(FileTypeRegister.TDB);

      private async void ChooseTypFile_Clicked(object sender, EventArgs e) => await registerFile(FileTypeRegister.TYP);

      /// <summary>
      /// kann nur bei einer neuen Definition aufgerufen werden (sonst ist die Auswahl gesperrt)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pickerProvider_SelectedIndexChanged(object sender, EventArgs e) {
         string mapname = MapProviderDefinition != null ? MapProviderDefinition.MapName : "neu Karte";

         frameGarmin.IsVisible =
         frameGarminKmz.IsVisible =
         frameWms.IsVisible =
         frameMulti.IsVisible = false;

         frameHillshading.IsVisible = false;

         string? providername = ((Picker)sender).SelectedItem.ToString();
         if (providername != null) {
            entryMapname.Text = providername;

            if (providername == GarminProvider.Instance.Name) {
               frameGarmin.IsVisible = true;
               frameHillshading.IsVisible = true;

               entryTextFactor.Text = "1";
               entrySymbolFactor.Text = "1";
               entryLineFactor.Text = "1";
               cbHillshading.IsEnabled = true;
               cbHillshading.IsChecked = false;
               entryHillshadingalpha.IsEnabled = true;
               entryHillshadingalpha.Text = "100";
            } else if (providername == GarminKmzProvider.Instance.Name) {
               frameGarminKmz.IsVisible = true;
               frameHillshading.IsVisible = true;

               cbHillshading.IsEnabled = true;
               cbHillshading.IsChecked = false;
               entryHillshadingalpha.IsEnabled = true;
               entryHillshadingalpha.Text = "100";
            } else if (providername == WMSProvider.Instance.Name) {
               frameWms.IsVisible = true;
               frameHillshading.IsVisible = true;

               cbHillshading.IsEnabled = true;
               cbHillshading.IsChecked = false;
               entryHillshadingalpha.IsEnabled = true;
               entryHillshadingalpha.Text = "100";
            } else if (providername == HillshadingProvider.Instance.Name) {
               frameHillshading.IsVisible = true;

               cbHillshading.IsChecked = true;
               cbHillshading.IsEnabled = false;
               entryHillshadingalpha.IsEnabled = true;
               entryHillshadingalpha.Text = "100";
            } else if (providername == MultiMapProvider.Instance.Name) {
               frameMulti.IsVisible = true;
               if (IsNewMapProviderDefinition) {
                  MapProviderDefinition = multimapDef = new MultiMapProvider.MultiMapDefinition(
                                                               mapname,
                                                               MapProviderDefinition != null ? MapProviderDefinition.MinZoom : 10,
                                                               MapProviderDefinition != null ? MapProviderDefinition.MaxZoom : 24,
                                                               []);
               }
            } else {

            }
         }
      }

      private async void btnSave_Clicked(object sender, EventArgs e) {
         await showMainpageBusy();

         string providername = getText(pickerProvider);

         if (!hasText(providername)) {
            IsBusy = false;
            await showError("Ein Kartenprovider muss ausgewählt sein.");
            return;
         }
         if (!hasText(entryMapname) &&
             !IsSubLayer) {
            IsBusy = false;
            await showError("Ein Kartenname muss angegeben sein.");
            return;
         }

         if (providername == GarminProvider.Instance.Name) {
            if (!hasText(lblTdbFile)) {
               IsBusy = false;
               await showError("Eine TDB-Datei muss angegeben sein.");
               return;
            }
            if (!hasText(lblTypFile)) {
               IsBusy = false;
               await showError("Eine TYP-Datei muss angegeben sein.");
               return;
            }
            if (getBool(cbHillshading) && getInt(entryHillshadingalpha) <= 0) {
               IsBusy = false;
               await showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == GarminKmzProvider.Instance.Name) {
            if (!hasText(lblKmzFile)) {
               IsBusy = false;
               await showError("Eine KMZ-Datei muss angegeben sein.");
               return;
            }
            if (getBool(cbHillshading) && getInt(entryHillshadingalpha) <= 0) {
               IsBusy = false;
               await showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == WMSProvider.Instance.Name) {
            if (!hasText(entryUrl)) {
               IsBusy = false;
               await showError("Eine URL muss angegeben sein.");
               return;
            }
            if (!hasText(entrySrs)) {
               IsBusy = false;
               await showError("Eine SRS (Koordinatensystem) muss angegeben sein.");
               return;
            }
            if (!hasText(entryVersion)) {
               IsBusy = false;
               await showError("Eine WMS-Version muss angegeben sein.");
               return;
            }
            if (getBool(cbHillshading) && getInt(entryHillshadingalpha) <= 0) {
               IsBusy = false;
               await showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == HillshadingProvider.Instance.Name) {
            if (getInt(entryHillshadingalpha) <= 0) {
               IsBusy = false;
               await showError("Ein Alpha-Wert größer als 0 muss angegeben sein.");
               return;
            }
         } else if (providername == MultiMapProvider.Instance.Name) {
            if (maplist.Items.Count == 0) {
               IsBusy = false;
               await showError("Mindestens 1 Ebene muss angegeben sein.");
               return;
            }
         }

         Ok = true;

         // ACHTUNG!  Wenn bestimmte Daten geändert werden muss DbIdDelta neu ermittelt werden, d.h. eine neue Def. ist nötig!
         if (!IsNewMapProviderDefinition &&
             MapProviderDefinition != null) {
            if (providername == GarminProvider.Instance.Name) {
               GarminProvider.GarminMapDefinition mapDefinitionData = (GarminProvider.GarminMapDefinition)MapProviderDefinition;
               if (getText(entryMapname) != MapProviderDefinition.MapName ||
                   getText(lblTdbFile) != mapDefinitionData.TDBfile[0] ||
                   getText(lblTypFile) != mapDefinitionData.TYPfile[0]) {
                  IsNewMapProviderDefinition = true;
               }
            } else if (providername == GarminKmzProvider.Instance.Name) {
               if (getText(entryMapname) != MapProviderDefinition.MapName ||
                   getText(lblKmzFile) != ((GarminKmzProvider.KmzMapDefinition)MapProviderDefinition).KmzFile) {
                  IsNewMapProviderDefinition = true;
               }
            } else if (providername == WMSProvider.Instance.Name) {
               WMSProvider.WMSMapDefinition mapDefinitionData = (WMSProvider.WMSMapDefinition)MapProviderDefinition;
               if (getText(entryMapname) != MapProviderDefinition.MapName ||
                   getText(entryLayer) != mapDefinitionData.Layer ||
                   getText(entryUrl) != mapDefinitionData.URL ||
                   getText(entrySrs) != mapDefinitionData.SRS ||
                   getText(entryVersion) != mapDefinitionData.Version ||
                   getText(pickerWmsPictFormat) != mapDefinitionData.PictureFormat ||
                   getText(entryExtParams) != mapDefinitionData.ExtendedParameters) {
                  IsNewMapProviderDefinition = true;
               }
            }
         }

         // Übernahme der Werte nach MapProviderDefinition bzw. Erzeugung einer neuen MapProviderDefinition

         if (IsNewMapProviderDefinition) {
            if (providername == GarminProvider.Instance.Name) {
               MapProviderDefinition = new GarminProvider.GarminMapDefinition(
                                                getText(entryMapname),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                [getText(lblTdbFile),],
                                                [getText(lblTypFile),],
                                                getDouble(entryTextFactor),
                                                getDouble(entryLineFactor),
                                                getDouble(entrySymbolFactor),
                                                getBool(cbHillshading),
                                                getByte(entryHillshadingalpha));
            } else if (providername == GarminKmzProvider.Instance.Name) {
               MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(
                                                getText(entryMapname),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                getText(lblKmzFile),
                                                getBool(cbHillshading),
                                                getByte(entryHillshadingalpha));
            } else if (providername == WMSProvider.Instance.Name) {
               MapProviderDefinition = new WMSProvider.WMSMapDefinition(
                                                getText(entryMapname),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                getText(entryLayer),
                                                getText(entryUrl),
                                                getText(entrySrs),
                                                getText(entryVersion),
                                                getText(pickerWmsPictFormat),
                                                getText(entryExtParams),
                                                getBool(cbHillshading),
                                                getByte(entryHillshadingalpha));
            } else if (providername == HillshadingProvider.Instance.Name) {

               MapProviderDefinition = new HillshadingProvider.HillshadingMapDefinition(
                                                getText(entryMapname),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                null,
                                                getByte(entryHillshadingalpha));

            } else if (providername == MultiMapProvider.Instance.Name) {

               if (MapProviderDefinition != null) {
                  MultiMapProvider.MultiMapDefinition mmdef = (MultiMapProvider.MultiMapDefinition)MapProviderDefinition;
                  if (mmdef != null)
                     for (int i = 0; i < mmdef.MapProviderDefinitions.Length; i++) {
                        mmdef.MapProviderDefinitions[i].MinZoom = MapProviderDefinition.MinZoom;
                        mmdef.MapProviderDefinitions[i].MaxZoom = MapProviderDefinition.MaxZoom;
                     }
               }

            } else {
               MapProviderDefinition = new MapProviderDefinition();
               MapProviderDefinition.ProviderName = providername;
               MapProviderDefinition.MapName = getText(entryMapname);
               MapProviderDefinition.MinZoom = getInt(entryZoomFrom);
               MapProviderDefinition.MaxZoom = getInt(entryZoomTo);
            }

         } else {       // nur Daten verändert

            if (MapProviderDefinition != null) {
               MapProviderDefinition.ProviderName = providername;
               MapProviderDefinition.MapName = getText(entryMapname);
               MapProviderDefinition.MinZoom = getInt(entryZoomFrom);
               MapProviderDefinition.MaxZoom = getInt(entryZoomTo);

               if (MapProviderDefinition is GarminProvider.GarminMapDefinition) {

                  GarminProvider.GarminMapDefinition specmpd = (GarminProvider.GarminMapDefinition)MapProviderDefinition;
                  if (specmpd != null) {
                     specmpd.TDBfile[0] = getText(lblTdbFile);
                     specmpd.TYPfile[0] = getText(lblTypFile);
                     specmpd.TextFactor = getDouble(entryTextFactor);
                     specmpd.SymbolFactor = getDouble(entrySymbolFactor);
                     specmpd.LineFactor = getDouble(entryLineFactor);
                     specmpd.HillShading = getBool(cbHillshading);
                     specmpd.HillShadingAlpha = getByte(entryHillshadingalpha);
                  }

               } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

                  GarminKmzProvider.KmzMapDefinition specmpd = (GarminKmzProvider.KmzMapDefinition)MapProviderDefinition;
                  if (specmpd != null) {
                     specmpd.KmzFile = getText(lblKmzFile);
                     specmpd.HillShading = getBool(cbHillshading);
                     specmpd.HillShadingAlpha = getByte(entryHillshadingalpha);
                  }

               } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

                  WMSProvider.WMSMapDefinition specmpd = (WMSProvider.WMSMapDefinition)MapProviderDefinition;
                  if (specmpd != null) {
                     specmpd.URL = getText(entryUrl);
                     specmpd.Version = getText(entryVersion);
                     specmpd.SRS = getText(entrySrs);
                     specmpd.PictureFormat = getText(pickerWmsPictFormat);
                     specmpd.Layer = getText(entryLayer);
                     specmpd.ExtendedParameters = getText(entryExtParams);
                     specmpd.HillShading = getBool(cbHillshading);
                     specmpd.HillShadingAlpha = getByte(entryHillshadingalpha);
                  }

               } else if (MapProviderDefinition is HillshadingProvider.HillshadingMapDefinition) {

                  HillshadingProvider.HillshadingMapDefinition specmpd = (HillshadingProvider.HillshadingMapDefinition)MapProviderDefinition;
                  if (specmpd != null) {
                     MapProviderDefinition = new HillshadingProvider.HillshadingMapDefinition(specmpd.MapName,
                                                                                              specmpd.MinZoom,
                                                                                              specmpd.MaxZoom,
                                                                                              specmpd.DEM,
                                                                                              getByte(entryHillshadingalpha));
                  }

               } else if (MapProviderDefinition is MultiMapProvider.MultiMapDefinition) {

                  MapProviderDefinition = multimapDef;
                  //if (specmpd != null) {


                  //} else {

                  //}

               }
            }

            IsBusy = false;
         }

         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }

      #region Hilfsfunktionen

      async private Task registerFile(FileTypeRegister type) {
         string typ = type == FileTypeRegister.TDB ? "TDB" :
                      type == FileTypeRegister.TYP ? "TYP" :
                                                     "KMZ";
         try {
            string? path = type == FileTypeRegister.TDB ? lblTdbFile.Text :
                           type == FileTypeRegister.TYP ? lblTypFile.Text :
                                                          lblKmzFile.Text;

            if (string.IsNullOrEmpty(path)) {
               if (type == FileTypeRegister.TDB)
                  path = lblTypFile.Text;
               else if (type == FileTypeRegister.TYP)
                  path = lblTdbFile.Text;
            }
            await showMainpageBusy();
            path = path != string.Empty ? Path.GetDirectoryName(path) : string.Empty;
            // allerletzter Versuch:
            if (string.IsNullOrEmpty(path))
               path = ChooseFilePage.LastChoosedPath;

            ChooseFilePage chooseFilePage = new ChooseFilePage() {
               AndroidActivity = FSofTUtils.OSInterface.DirtyGlobalVars.AndroidActivity,
               Path = path,
               Filename = string.Empty,
               OnlyExistingFile = true,   // ohne Eingabefeld für Namen
               Match4Filenames = new System.Text.RegularExpressions.Regex(@"\.(" + typ.ToLower() + ")$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
               Title = typ + "-Datei auswählen",
            };

            chooseFilePage.ChooseFileReadyEvent += (object? sender,
                                                    FSofTUtils.OSInterface.Control.ChooseFile.ChoosePathAndFileEventArgs e) => {
                                                       if (e.OK) {
                                                          string fullfilename = Path.Combine(e.Path, e.Filename);
                                                          switch (type) {
                                                             case FileTypeRegister.KMZ: lblKmzFile.Text = fullfilename; break;
                                                             case FileTypeRegister.TDB: lblTdbFile.Text = fullfilename; break;
                                                             case FileTypeRegister.TYP: lblTypFile.Text = fullfilename; break;
                                                          }
                                                       }
                                                    };

            await FSofTUtils.OSInterface.Helper.GoTo(chooseFilePage);
         } catch (Exception ex) {
            IsBusy = false;
            await UIHelper.ShowExceptionMessage(this,
                                                "Fehler beim Ermitteln der " + typ + "-Datei",
                                                ex,
                                                null,
                                                false);
         } finally {
            IsBusy = false;
         }
      }

      async Task showError(string message) => await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", message);

      bool hasText(Entry ctrl) => hasText(ctrl.Text);

      bool hasText(Label ctrl) => hasText(ctrl.Text);

      bool hasText(string txt) => !string.IsNullOrEmpty(txt) && txt.Trim().Length > 0;

      double getDouble(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? 0 : Convert.ToDouble(ctrl.Text);

      int getInt(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? 0 : Convert.ToInt32(ctrl.Text);

      byte getByte(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? (byte)0 : Convert.ToByte(ctrl.Text);

      bool getBool(CheckBox ctrl) => ctrl.IsChecked;

      string getText(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? string.Empty : ctrl.Text.Trim();

      string getText(Label ctrl) => string.IsNullOrEmpty(ctrl.Text) ? string.Empty : ctrl.Text.Trim();

      string getText(Picker ctrl) {
         if (ctrl.SelectedItem != null) {
            string? txt = ctrl.SelectedItem.ToString();
            if (txt != null)
               return txt.Trim();
         }
         return string.Empty;
      }

      async Task showMainpageBusy() {
         IsBusy = true;
         await Task.Delay(10);      // "Trick": der ActivityIndicator erscheint schneller
      }

      #endregion

   }
}