namespace TrackEddi {
   public class SymbolGroupList : List<SymbolObjectItem> {
      public string Groupname { get; set; } = string.Empty;

      public List<SymbolObjectItem> Symbols => new List<SymbolObjectItem>();

      public override string ToString() {
         return Groupname + " (" + Count + ")";
      }

   }
}
