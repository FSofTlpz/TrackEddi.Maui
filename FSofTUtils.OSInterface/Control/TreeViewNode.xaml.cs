﻿using System.Reflection;

namespace FSofTUtils.OSInterface.Control {

   public partial class TreeViewNode : ContentView {

      static ImageSource imgSrcStdItem = ImageSource.FromResource("FSofTUtils.OSInterface.Resource.Item.png", typeof(TreeViewNode).GetTypeInfo().Assembly);
      static ImageSource imgSrcStdExpanded = ImageSource.FromResource("FSofTUtils.OSInterface.Resource.OpenGlyph.png", typeof(TreeViewNode).GetTypeInfo().Assembly);
      static ImageSource imgSrcStdCollapsed = ImageSource.FromResource("FSofTUtils.OSInterface.Resource.CollpsedGlyph.png", typeof(TreeViewNode).GetTypeInfo().Assembly);


      #region Events

      public class BoolResultEventArgs : EventArgs {

         /// <summary>
         /// falls true Abbruch der Aktion
         /// </summary>
         public bool Cancel { get; set; }

         public BoolResultEventArgs() {
            Cancel = false;
         }

      }

      /// <summary>
      /// Bevor sich der <see cref="Checked"/>-Status ändert.
      /// </summary>
      public event EventHandler<BoolResultEventArgs>? OnBeforeCheckedChanged;

      /// <summary>
      /// Der <see cref="Checked"/>-Status hat sich geändert.
      /// </summary>
      public event EventHandler<EventArgs>? OnCheckedChanged;

      /// <summary>
      /// Bevor sich der <see cref="Expanded"/>-Status ändert.
      /// </summary>
      public event EventHandler<BoolResultEventArgs>? OnBeforeExpandedChanged;

      /// <summary>
      /// Der <see cref="Expanded"/>-Status hat sich geändert.
      /// </summary>
      public event EventHandler<EventArgs>? OnExpandedChanged;

      /// <summary>
      /// Es wurde auf den Text getippt.
      /// </summary>
      public event EventHandler<EventArgs>? OnTapped;

      /// <summary>
      /// Es wurde auf den Text doppelt getippt.
      /// </summary>
      public event EventHandler<EventArgs>? OnDoubleTapped;

      /// <summary>
      /// Der Text wurde gewischt.
      /// </summary>
      public event EventHandler<EventArgs>? OnSwipe;

      #endregion


      #region  Binding-Var Text

      public static readonly BindableProperty TextProperty = BindableProperty.Create(
         "Text",
         typeof(string),
         typeof(TreeViewNode),
         "");

      /// <summary>
      /// Text
      /// </summary>
      public string Text {
         get => (string)GetValue(TextProperty);
         set => SetValue(TextProperty, value);
      }

      #endregion

      #region  Binding-Var BackcolorText

      public static readonly BindableProperty BackcolorTextProperty = BindableProperty.Create(
         nameof(BackcolorText),
         typeof(Color),
         typeof(TreeViewNode),
         Colors.White);

      /// <summary>
      /// Hintergrundfarbe des Textes
      /// </summary>
      public Color BackcolorText {
         get => (Color)GetValue(BackcolorTextProperty);
         set => SetValue(BackcolorTextProperty, value);
      }

      #endregion

      #region  Binding-Var Textcolor

      public static readonly BindableProperty TextcolorProperty = BindableProperty.Create(
         nameof(Textcolor),
         typeof(Color),
         typeof(TreeViewNode),
         Colors.Black);

      /// <summary>
      /// Hintergrundfarbe des Controls
      /// </summary>
      public Color Textcolor {
         get => (Color)GetValue(TextcolorProperty);
         set => SetValue(TextcolorProperty, value);
      }

      #endregion

      #region  Binding-Var BackcolorNode

      public static readonly BindableProperty BackcolorNodeProperty = BindableProperty.Create(
         nameof(BackcolorNode),
         typeof(Color),
         typeof(TreeViewNode),
         Colors.White);

      /// <summary>
      /// Hintergrundfarbe des gesamten TreeNode-Items
      /// </summary>
      public Color BackcolorNode {
         get => (Color)GetValue(BackcolorNodeProperty);
         set => SetValue(BackcolorNodeProperty, value);
      }

      #endregion

      #region  Binding-Var ChildNodeIndent

      public static readonly BindableProperty ChildIndentMarginProperty = BindableProperty.Create(
         nameof(ChildNodeIndent),
         typeof(Thickness),
         typeof(TreeViewNode),
         new Thickness(50, 0, 0, 0));

      public Thickness ChildNodeIndentMargin {
         get => (Thickness)GetValue(ChildIndentMarginProperty);
         set => SetValue(ChildIndentMarginProperty, value);
      }

      /// <summary>
      /// linker Einzug für den Child-Bereich
      /// </summary>
      public double ChildNodeIndent {
         get => ChildNodeIndentMargin.Left;
         set => SetValue(ChildIndentMarginProperty, new Thickness(value, 0, 0, 0));  // ChildIndentMargin = new Thickness(value, 0, 0, 0);
      }

      #endregion

      #region  Binding-Var FontSize

      public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
         nameof(FontSize),
         typeof(double),
         typeof(TreeViewNode),
         new FontSizeConverter().ConvertFromInvariantString("Medium"));

      /// <summary>
      /// Textgröße
      /// </summary>
      public double FontSize {
         get => (double)GetValue(FontSizeProperty);
         set => SetValue(FontSizeProperty, value);
      }

      #endregion

      #region  Binding-Var ImageExpanded

      public static readonly BindableProperty ImageExpandedProperty = BindableProperty.Create(
         nameof(ImageExpanded),
         typeof(ImageSource),
         typeof(TreeViewNode),
         imgSrcStdExpanded);

      /// <summary>
      /// Bild für einen expandierten <see cref="TreeViewNode"/>
      /// </summary>
      public ImageSource ImageExpanded {
         get => (ImageSource)GetValue(ImageExpandedProperty);
         set => SetValue(ImageExpandedProperty, value);
      }

      #endregion

      #region  Binding-Var ImageCollapsed

      public static readonly BindableProperty ImageCollapsedProperty = BindableProperty.Create(
         nameof(ImageCollapsed),
         typeof(ImageSource),
         typeof(TreeViewNode),
         imgSrcStdCollapsed);

      /// <summary>
      /// Bild für einen nicht-expandierten <see cref="TreeViewNode"/>
      /// </summary>
      public ImageSource ImageCollapsed {
         get => (ImageSource)GetValue(ImageCollapsedProperty);
         set => SetValue(ImageCollapsedProperty, value);
      }

      #endregion

      #region  Binding-Var ImageStandard

      public static readonly BindableProperty ImageStandardProperty = BindableProperty.Create(
         nameof(ImageStandard),
         typeof(ImageSource),
         typeof(TreeViewNode),
         imgSrcStdItem);

      /// <summary>
      /// Bild für einen <see cref="TreeViewNode"/> ohne Childs
      /// </summary>
      public ImageSource ImageStandard {
         get => (ImageSource)GetValue(ImageStandardProperty);
         set => SetValue(ImageStandardProperty, value);
      }

      #endregion


      public StackLayout XamlChildContainer => childrenStackLayout;

      /// <summary>
      /// für beliebige Zusatzdaten
      /// </summary>
      public object? ExtendedData { get; set; }

      /// <summary>
      /// Anzahl der Child-Nodes
      /// </summary>
      public int ChildNodes => XamlChildContainer.Children != null ?
                                    XamlChildContainer.Children.Count :
                                    0;

      /// <summary>
      /// Gibt es Child-Nodes?
      /// </summary>
      public bool HasChildNodes {
         get => ChildNodes > 0;
      }

      /// <summary>
      /// Child-Nodes sichtbar?
      /// </summary>
      public bool Expanded {
         get => XamlChildContainer.IsVisible;
         set {
            if (XamlChildContainer.IsVisible != value) {
               bool change = false;
               if (value) {
                  if (HasChildNodes)
                     change = true;
               } else
                  change = true;

               if (change) {
                  BoolResultEventArgs args = new BoolResultEventArgs();
                  OnBeforeExpandedChanged?.Invoke(this, args);
                  if (!args.Cancel) {
                     XamlChildContainer.IsVisible = value && HasChildNodes;
                     OnExpandedChanged?.Invoke(this, new EventArgs());
                     changeImage();
                  }
               }
            }
         }
      }

      /// <summary>
      /// Ist die Checkbox sichtbar?
      /// </summary>
      public bool HasCheckbox {
         get => checkbox.IsVisible;
         set => checkbox.IsVisible = value;
      }


      bool checkboxSetInternal = false;
      bool checkboxResetInternal = false;

      /// <summary>
      /// Checked-Status (kann auch ohne sichtbare Checkbox verwendet werden)
      /// </summary>
      public bool Checked {
         get => checkbox.IsChecked;
         set {
            if (checkbox.IsChecked != value) {
               BoolResultEventArgs args = new BoolResultEventArgs();
               OnBeforeCheckedChanged?.Invoke(this, args);
               if (!args.Cancel) {
                  checkboxSetInternal = true;
                  checkbox.IsChecked = value;
                  checkboxSetInternal = false;
                  OnCheckedChanged?.Invoke(this, new EventArgs());
               }
            }
         }
      }

      /// <summary>
      /// liefert den Parent-Node
      /// </summary>
      public TreeViewNode? ParentNode {
         get => Parent != null && Parent is StackLayout &&                             // 1. übergeordnete Element StackLayout 
                Parent.Parent != null && Parent.Parent is StackLayout &&               // 2. übergeordnete Element StackLayout 
                Parent.Parent.Parent != null && Parent.Parent.Parent is TreeViewNode ? // 3. übergeordnete Element TreeViewNode (ContentView)
                           Parent.Parent.Parent as TreeViewNode :
                           null;
      }

      /// <summary>
      /// liefert den zugehörigen <see cref="TreeView"/>
      /// </summary>
      public TreeView? TreeView {
         get {
            TreeViewNode? parent = ParentNode;
            if (parent != null)
               return parent.TreeView;
            else
               return Parent != null && Parent is StackLayout &&                          // 1. übergeordnete Element StackLayout 
                      Parent.Parent != null && Parent.Parent is ScrollView &&             // 2. übergeordnete Element ScrollView 
                      Parent.Parent.Parent != null && Parent.Parent.Parent is TreeView ?  // 3. übergeordnete Element TreeView 
                           Parent.Parent.Parent as TreeView :
                           null;
         }
      }


      public TreeViewNode() {
         InitializeComponent();
         Expanded = false;
      }

      public TreeViewNode(string text, object? data = null) : this() {
         Text = text;
         ExtendedData = data;
         HasCheckbox = false;
      }

      /// <summary>
      /// liefert alle Child-Nodes
      /// </summary>
      /// <returns></returns>
      public List<TreeViewNode> GetChildNodes() => TreeView.GetChildNodes(XamlChildContainer);

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an dieser Position wird geliefert.
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public TreeViewNode? GetChildNode(int idx) => TreeView.GetChildNode(XamlChildContainer, idx);

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird angehängt
      /// </summary>
      /// <param name="node"></param>
      public void AddChildNode(TreeViewNode node) {
         TreeView.AddChildNode(XamlChildContainer, node);
         changeImage();
      }

      /// <summary>
      /// Ein <see cref="TreeViewNode"/> für den Text wird erzeugt und angehängt.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="extdata"></param>
      /// <returns></returns>
      public TreeViewNode AddChildNode(string text, object? extdata = null) {
         TreeViewNode tn = new TreeViewNode(text, extdata);
         AddChildNode(tn);
         return tn;
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird an der Position eingefügt.
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="node"></param>
      public void InsertChildNode(int pos, TreeViewNode node) {
         TreeView.InsertChildNode(XamlChildContainer, pos, node);
         changeImage();
      }

      /// <summary>
      /// Ein <see cref="TreeViewNode"/> für den Text wird erzeugt und eingefügt.
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="text"></param>
      /// <param name="extdata"></param>
      /// <returns></returns>
      public TreeViewNode InsertChildNode(int pos, string text, object? extdata = null) {
         TreeViewNode tn = new TreeViewNode(text, extdata);
         InsertChildNode(pos, tn);
         return tn;
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an der Position wird entfernt.
      /// </summary>
      /// <param name="pos"></param>
      public void RemoveChildNode(int pos) {
         TreeView.RemoveChildNode(XamlChildContainer, pos);
         changeImage();
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird entfernt.
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      public bool RemoveChildNode(TreeViewNode node) {
         bool result = TreeView.RemoveChildNode(XamlChildContainer, node);
         changeImage();
         return result;
      }

      /// <summary>
      /// entfernt alle untergeordneten <see cref="TreeViewNode"/> 
      /// </summary>
      public void RemoveChildNodes() {
         TreeView.RemoveChildNodes(XamlChildContainer);
         changeImage();
      }

      /// <summary>
      /// Expand oder Collapse für den <see cref="TreeViewNode"/> und alle Sub-<see cref="TreeViewNode"/>
      /// </summary>
      /// <param name="expand"></param>
      public void ExpandOrCollapseAll(bool expand) {
         if (HasChildNodes) {
            Expanded = expand;
            foreach (TreeViewNode node in GetChildNodes())
               node.ExpandOrCollapseAll(expand);
         }
      }

      protected void changeImage() {
         ImageSource imageSource = HasChildNodes ?
                                       (Expanded ?
                                             ImageExpanded :
                                             ImageCollapsed) :
                                       ImageStandard;
         if (image.Source == null ||
             !image.Source.Equals(imageSource))
            image.Source = imageSource;
      }

      void select() {
         TreeView? tv = TreeView;
         if (tv != null)
            tv.SelectedNode = this;
      }

      private void checkbox_CheckedChanged(object? sender, CheckedChangedEventArgs e) {
         if (!checkboxSetInternal &&      // dann durch User
             !checkboxResetInternal) {    // kein Reset
            BoolResultEventArgs args = new BoolResultEventArgs();
            OnBeforeCheckedChanged?.Invoke(this, args);
            if (!args.Cancel) {
               OnCheckedChanged?.Invoke(this, new EventArgs());
            } else {
               checkboxResetInternal = true;
               checkbox.IsChecked = !checkbox.IsChecked;
               checkboxResetInternal = false;
            }
         }
      }

      private void FrameTapGestureRecognizer_SingleTapped(object sender, TappedEventArgs e) {
         select();
         OnTapped?.Invoke(this, new EventArgs());
      }

      private void FrameTapGestureRecognizer_DoubleTapped(object sender, TappedEventArgs e) {
         select();
         OnDoubleTapped?.Invoke(this, new EventArgs());
      }

      private void ImageTapGestureRecognizer_SingleTapped(object sender, TappedEventArgs e) {
         Expanded = !Expanded;
      }

      public override string ToString() {
         return Text;
      }

   }
}