//#define EXTSHOWINFO
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;
using System.Text;

namespace FSofTUtils.OSInterface.Touch {
   /// <summary>
   /// Hilfsklasse, die die "Rohdaten" von <see cref="TouchEffect"/> aufbereitet und als Events liefert
   /// </summary>
   public class TouchPointEvaluator {

      /*
      Vom betriebssystemspezifischen Teil werden Punktkoordinaten mit ID und einer "Aktion" geliefert.
      Alle Aktionen/Koordinaten mit der gleichen ID gehören zum gleichen Finger.

      Normalerweise sollte jede Fingergeste mit Pressed beginnen. Danach folgen weitere Aktionen/Koordinaten.
      Normalerweise wird jede Fingergeste mit Released beendet (oder abgebrochen).

      Eine Move-Fingergeste könnte prinzipiell auch ohne Pressed beginnen.

      Folgen nach dem Start einer Fingergeste nur geringfügige Moves (Delta um Pressed), sollten sie nicht 
      berücksichtigt werden.
      Fogt daraus effektiv nur eine Fingergeste aus Pressed und Released, wird sie als Tap gewertet. Ob es ein 
      short (normaler) oder long Tap ist ergibt sich aus der Zeitdiff. zwischen Pressed und Released;

      Problematisch sind Multitaps. Sollten innerhalb einer max. Zeitspanne und eines max. Delta mehrere short Taps
      erfolgen, sollen sie zusätzlich zu den Einzeltaps auch Multitaps ergeben.


       */

      #region Events

      #region Event-Args

      public class BaseGestureEventArgs {

         public readonly Point Point;

         public readonly long ID;

         public BaseGestureEventArgs(long id, Point point) {
            Point = point;
            ID = id;
         }

         public override string ToString() {
            return string.Format("ID={0}, Point={1}", ID, Point);
         }

      }

      public class TappedEventArgs : BaseGestureEventArgs {

         public readonly bool LongTap;

         public readonly double Delay;

         public readonly int TapCount;

         public TappedEventArgs(long id, Point startPoint, bool longtap, double delay, int tapcount) :
            base(id, startPoint) {
            LongTap = longtap;
            Delay = delay;
            TapCount = tapcount;
         }

         public TappedEventArgs(long id, Point startPoint) :
            this(id, startPoint, false, 0, 1) { }

         public TappedEventArgs(long id, Point startPoint, double delay) :
            this(id, startPoint, true, delay, 1) { }

         public TappedEventArgs(long id, Point startPoint, int tapcount) :
            this(id, startPoint, false, 0, tapcount) { }

         public override string ToString() {
            return base.ToString() + string.Format(", LongTap={0}, Delay={1}ms, TapCount={2}",
                                                   LongTap,
                                                   Delay,
                                                   TapCount);
         }

      }


      public class MoveEventArgs : BaseGestureEventArgs {

         public readonly Point Delta2Startpoint;

         public readonly Point Delta2Lastpoint;

         public readonly bool MovingEnded;

         public MoveEventArgs(long id, Point startPoint, Point delta2start, Point delta2last, bool ended) :
            base(id, startPoint) {
            Delta2Startpoint = delta2start;
            Delta2Lastpoint = delta2last;
            MovingEnded = ended;
         }

         public override string ToString() {
            return base.ToString() + string.Format(", MovingEnded={0}, Delta2Startpoint={1}, Delta2Lastpoint={2}",
                                                   MovingEnded,
                                                   Delta2Startpoint,
                                                   Delta2Lastpoint);
         }
      }

      #endregion

      /// <summary>
      /// vermutlich nicht sehr sinnvoll
      /// </summary>
      public event EventHandler<TappedEventArgs>? TapDownEvent;

      /// <summary>
      /// ein Tap (kurz oder lang) ist erfolgt
      /// </summary>
      public event EventHandler<TappedEventArgs>? TappedEvent;

      /// <summary>
      /// ein mehrfaches (kurzes) Tap ist erfolgt (folgt nach jedem dazugehörigen <see cref="TappedEvent"/>)
      /// </summary>
      public event EventHandler<TappedEventArgs>? MultiTappedEvent;

      /// <summary>
      /// eine Bewegung ist erfolgt oder beendet
      /// </summary>
      public event EventHandler<MoveEventArgs>? MoveEvent;


      #endregion

      protected enum TouchAction {
         nothing,
         Pressed,
         Moved,
         Released,
         Cancelled,
         Entered,
         Exited
      }

      /// <summary>
      /// max. Punktdifferenz eines Tap zwischen Pressed und Released (i.A. 0)
      /// </summary>
      public Point Delta4Tapped = new Point(0, 0);
      /// <summary>
      /// max. Punktdifferenz zwischen Taps eines Multi-Tap
      /// </summary>
      public Point Delta4MultiTapped = new Point(40, 40);
      /// <summary>
      /// max. Zeit zwischen Pressed und Released für einen (kurzen) Tap (sonst langer Tap)
      /// </summary>
      public double MaxDelay4ShortTapped = 300;
      /// <summary>
      /// max. Zeit zwischen 2 Released bei aufeinanderfolgenden Taps eines Multi-Tap
      /// </summary>
      public double MaxDelay4MultiTapped = 500;

      #region interne Hilfsklassen

      protected class PointData {

         public readonly TouchAction Action;
         public readonly Point Point;
         public readonly DateTime DateTime;
         public TouchAction EvaluatedAction;

         public PointData(Point pt, TouchAction action) {
            Action = EvaluatedAction = action;
            Point = pt;
            DateTime = DateTime.Now;
         }

      }

      /// <summary>
      /// Hilfsklasse zur Registrierung der Punktdaten zu jeder ID
      /// </summary>
      protected class FingerGesture4ID {

         Dictionary<long, List<PointData>> pd4id = new Dictionary<long, List<PointData>>();

         Dictionary<long, List<PointData>> release4id = new Dictionary<long, List<PointData>>();


         public FingerGesture4ID() { }

         /// <summary>
         /// Alle Daten löschen.
         /// </summary>
         public void Clear() {
            pd4id.Clear();
            release4id.Clear();
         }

         /// <summary>
         /// Punktanzahl zur ID
         /// </summary>
         /// <param name="id"></param>
         /// <returns></returns>
         public int Count(long id) => count(pd4id, id);

         /// <summary>
         /// Daten zu dieser ID löschen
         /// </summary>
         /// <param name="id"></param>
         public void Clear(long id) => clear(pd4id, id);

         /// <summary>
         /// Punkt anhängen
         /// </summary>
         /// <param name="id"></param>
         /// <param name="pd"></param>
         public void Append(long id, PointData pd) => append(pd4id, id, pd);

         public PointData? First(long id) => first(pd4id, id);

         public PointData? Last(long id) => last(pd4id, id);

         public PointData? SecondLast(long id) => secondlast(pd4id, id);

         public long[] IDList() {
            long[] ids = new long[pd4id.Keys.Count];
            pd4id.Keys.CopyTo(ids, 0);
            return ids;
         }

         public int ReleaseCount(long id) => count(release4id, id);

         public void ReleaseClear(long id) => clear(release4id, id);

         public void ReleaseAppend(long id, PointData pd) => append(release4id, id, pd);

         public PointData? ReleaseLast(long id) => last(release4id, id);


         #region protected

         protected List<PointData>? list4ID(Dictionary<long, List<PointData>> dict, long id) =>
            dict.TryGetValue(id, out List<PointData>? lst) ? lst : null;

         protected PointData? first(Dictionary<long, List<PointData>> dict, long id) {
            List<PointData>? lst = list4ID(dict, id);
            return lst == null || lst.Count == 0 ? null : lst[0];
         }

         protected PointData? last(Dictionary<long, List<PointData>> dict, long id) {
            List<PointData>? lst = list4ID(dict, id);
            return lst == null || lst.Count == 0 ? null : lst[lst.Count - 1];
         }

         protected PointData? secondlast(Dictionary<long, List<PointData>> dict, long id) {
            List<PointData>? lst = list4ID(dict, id);
            return lst == null || lst.Count < 2 ? null : lst[lst.Count - 2];
         }

         protected void clear(Dictionary<long, List<PointData>> dict, long id) {
            if (dict.ContainsKey(id)) {
               dict[id].Clear();
               dict.Remove(id);
            }
         }

         /// <summary>
         /// Punkt anhängen
         /// </summary>
         /// <param name="dict"></param>
         /// <param name="id"></param>
         /// <param name="pd"></param>
         protected void append(Dictionary<long, List<PointData>> dict, long id, PointData pd) {
            if (dict.TryGetValue(id, out List<PointData>? lst))
               lst.Add(pd);
            else
               dict.Add(id, new List<PointData> { pd });
         }

         protected int count(Dictionary<long, List<PointData>> dict, long id) =>
            dict.TryGetValue(id, out List<PointData>? lst) ? lst.Count : 0;

         #endregion
      }

      #endregion

      protected FingerGesture4ID data4id = new FingerGesture4ID();


      public TouchPointEvaluator() {
         //Matrix m = GetIsotropicZoomMatrix(new Point(164.556361607143, 93.6875),
         //                                  new Point(244.938616071429, 57.5044642857143),
         //                                  new Point(246.081194196429, 57.5044642857143));

      }

      public void Evaluate(TouchEffect.TouchActionEventArgs args) {
         TouchAction touchAction = TouchAction.nothing;
         switch (args.Type) {
            // Android.Views.MotionEventActions.Down           A pressed gesture has started, the motion contains the initial starting location.
            // Android.Views.MotionEventActions.PointerDown    A non-primary pointer has gone down.
            case TouchEffect.TouchActionEventArgs.TouchActionType.Pressed:
               touchAction = TouchAction.Pressed;
               break;

            // Android.Views.MotionEventActions.Move:
            case TouchEffect.TouchActionEventArgs.TouchActionType.Moved:
               touchAction = TouchAction.Moved;
               break;

            // Android.Views.MotionEventActions.Up:
            // Android.Views.MotionEventActions.Pointer1Up:
            case TouchEffect.TouchActionEventArgs.TouchActionType.Released:
               touchAction = TouchAction.Released;
               break;

            // Android.Views.MotionEventActions.Cancel         The current gesture has been aborted. You will not receive any more points in it.
            case TouchEffect.TouchActionEventArgs.TouchActionType.Cancelled:
               touchAction = TouchAction.Cancelled;
               break;

            case TouchEffect.TouchActionEventArgs.TouchActionType.Entered:
               touchAction = TouchAction.Pressed;
               break;

            case TouchEffect.TouchActionEventArgs.TouchActionType.Exited:
               touchAction = TouchAction.Released;
               break;
         }
         evaluateAction(args.Id, new PointData(args.Location, touchAction));
      }

      void evaluateAction(long id, PointData pdActual) {
         // Es kann nur Pressed-, Released-, Moved- und Cancelled-Aktionen geben.
         switch (pdActual.Action) {
            case TouchAction.Pressed:
               doPressed(id, pdActual);
               break;

            case TouchAction.Moved:
               doAppend(id, pdActual, false);
               break;

            case TouchAction.Released:
               doAppend(id, pdActual, true);
               break;

            case TouchAction.Cancelled:
            default:
#if EXTSHOWINFO
               Debug.WriteLine(">>> TouchPointEvaluator.evaluateAction: all CLEAR");
#endif
               data4id.Clear(id);
               break;
         }

      }

      /// <summary>
      /// eine Fingergeste wird gestartet
      /// </summary>
      /// <param name="id"></param>
      /// <param name="pdActual"></param>
      void doPressed(long id, PointData pdActual) {
         pdActual.EvaluatedAction = TouchAction.Pressed;
         data4id.Clear(id);               // neue Fingergeste
         data4id.ReleaseClear(id);
         data4id.Append(id, pdActual);

#if EXTSHOWINFO
         Debug.WriteLine(">>> TouchPointEvaluator.doPressed: TapDownEvent id={0}, Point={1}", id, pdActual.Point);
#endif

         TapDownEvent?.Invoke(this, new TappedEventArgs(id, pdActual.Point));
      }

      void doAppend(long id, PointData pdActual, bool ended) {
         PointData? pdLastPt = data4id.Last(id);
         if (pdLastPt == null) {       // noch kein Punkt vorhanden -> erster Punkt

            doPressed(id, pdActual);

            if (ended)                 // als short-Tap werten (sollte eigentlich nicht vorkommen)
               doTap(id, pdActual, null);

         } else {

            Point delta = getDelta(pdLastPt.Point, pdActual.Point);     // Entfernung zum letzten Punkt

            if (ended) {         // Fingergeste auf jeden Fall beendet -> bewerten

               if (data4id.Count(id) == 1 &&    // erst 1 Punkt vorhanden und
                   isDelta4Tap(delta)) {        // Abstand klein genug -> Tap
                  doTap(id, pdActual, pdLastPt);
               } else {                         // schon min. 2 Punkte vorhanden oder Abstand zu groß -> Move
                  doMove(id, pdActual, delta, true);
               }

            } else {             // Fingergeste noch NICHT beendet

               if (!isDelta4Tap(delta)) {  // sonst zu dicht
                  pdActual.EvaluatedAction = TouchAction.Moved;
                  doMove(id, pdActual, delta, false);
               }

            }
         }
      }

      void doMove(long id, PointData pdActual, Point delta, bool ended) {
         data4id.Append(id, pdActual);
         PointData? pdStart = data4id.First(id);    // u.U. identisch zu pdActual
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
         invokeMoveEvent(id,
                         pdActual.Point,
                         getDelta(pdStart.Point, pdActual.Point),
                         delta,
                         ended);    // noch in Bewegung?
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
         if (ended) {
            data4id.Clear(id);
            data4id.ReleaseClear(id);
         }
      }

      /// <summary>
      /// es wird noch auf long oder short und auf multi getestet
      /// </summary>
      /// <param name="id"></param>
      /// <param name="pdActual"></param>
      /// <param name="pdLastPt"></param>
      void doTap(long id, PointData pdActual, PointData? pdLastPt) {
         if (pdLastPt != null) {
            double ms = getDelay(pdLastPt.DateTime, pdActual.DateTime);
            if (isDelay4ShortTap(ms))
               invokeTapEvent(id,
                              pdActual.Point,
                              true);   // ein short Tap
            else
               invokeTapEvent(id,
                              pdActual.Point,
                              false,
                              ms);     // ein long Tap
         } else {
            invokeTapEvent(id,
                           pdActual.Point,
                           true);   // ein short Tap
         }
         doMultiShortTap(id, pdActual);
      }

      void doMultiShortTap(long id, PointData pdActual) {
         PointData? pdLastRelease = data4id.ReleaseLast(id);
         if (pdLastRelease != null &&                                                  // gab es schon Releases
             isDelta4MultiTap(getDelta(pdLastRelease.Point, pdActual.Point)) &&        // innerhalb eines max. Abstandes
             isDelay4MultiTap(getDelay(pdLastRelease.DateTime, pdActual.DateTime))) {  // innerhalb einer max. Zeit
            invokeTapEvent(id, pdActual.Point, true, 0, data4id.ReleaseCount(id) + 1);    // dann auch Multitap (mehrere short Tap)
         } else
            data4id.ReleaseClear(id);
         data4id.ReleaseAppend(id, pdActual);
      }

      void invokeMoveEvent(long id, Point pt, Point deltastart, Point deltaend, bool ended) {
#if EXTSHOWINFO
         Debug.WriteLine(">>> TouchPointEvaluator.evaluateAction: MoveEvent id={0}, pt={1}, delta2start={2}, delta2end={3}, ended={4}",
                         id,
                         pt,
                         deltastart,
                         deltaend,
                         ended);
#endif
         MoveEvent?.Invoke(this, new MoveEventArgs(id,
                                                   pt,
                                                   deltastart,
                                                   deltaend,
                                                   ended));   // Bewegung beendet?
         if (ended)
            data4id.ReleaseClear(id);
      }

      void invokeTapEvent(long id, Point pt, bool shorttap, double ms = 0, int count = 0) {
#if EXTSHOWINFO
         Debug.WriteLine(">>> TouchPointEvaluator.evaluateAction: multi TappedEvent id={0}, Point={1}, count=2",
                         id,
                         pt,
                         count);
#endif
         if (count > 0) {
            MultiTappedEvent?.Invoke(this, new TappedEventArgs(id, pt, count));  // mehrere short Tap
         } else {
#if EXTSHOWINFO
            Debug.WriteLine(">>> TouchPointEvaluator.evaluateAction: {2} TappedEvent id={0}, Point={1}",
                            id,
                            pt,
                            shorttap ? "short" : "long");
#endif
            if (shorttap)
               TappedEvent?.Invoke(this, new TappedEventArgs(id, pt));           // ein short Tap
            else
               TappedEvent?.Invoke(this, new TappedEventArgs(id, pt, ms));       // ein long Tap (in TappedEventArgs gesetzt)
         }
      }

      #region public Matrix-Funktionen

      Point getLastPoint(long id, out bool empty) {
         empty = true;
         PointData? pd = data4id.Last(id);
         if (pd != null)
            return pd.Point;
         return Point.Zero;
      }

      Point getSecondLastPoint(long id, out bool empty) {
         empty = true;
         PointData? pd = data4id.SecondLast(id);
         if (pd != null)
            return pd.Point;
         return Point.Zero;
      }

      /// <summary>
      /// Bewegungsmatrix anfügen
      /// </summary>
      /// <param name="matrix"></param>
      /// <param name="newmove"></param>
      public void AppendMatrix(Matrix matrix, Matrix newmove) => matrix.Append(newmove);

      /// <summary>
      /// liefert die Bewegung der letzten beiden Punkte als Verschiebung
      /// </summary>
      /// <param name="moveID">ID des "beweglichen" Fingers"</param>
      /// <returns></returns>
      public Matrix GetTranslateMatrix(long moveID) {
         Point last = getLastPoint(moveID, out bool empty);
         if (!empty) {
            Point secondlast = getSecondLastPoint(moveID, out empty);
            if (!empty)
               return getTranslateMatrix(secondlast, last);
         }
         return new Matrix();
      }

      /// <summary>
      /// liefert die Zoommatrix
      /// </summary>
      /// <param name="pivotID">ID des "festen" Fingers</param>
      /// <param name="moveID">ID des "beweglichen" Fingers"</param>
      /// <returns></returns>
      public Matrix GetAnisotropicZoomMatrix(long pivotID, long moveID) {
         Point ptPivotLast = getLastPoint(pivotID, out bool empty);
         if (!empty) {
            Point ptMoveLast = getLastPoint(moveID, out empty);
            if (!empty) {
               Point ptMoveSecondLast = getSecondLastPoint(moveID, out empty);
               if (!empty)
                  return getAnisotropicZoomMatrix(ptPivotLast,
                                                  ptMoveSecondLast,
                                                  ptMoveLast);
            }
         }
         return new Matrix();
      }

      /// <summary>
      /// liefert die Zoommatrix
      /// </summary>
      /// <param name="pivotID">ID des "festen" Fingers</param>
      /// <param name="moveID">ID des "beweglichen" Fingers"</param>
      /// <returns></returns>
      public Matrix GetIsotropicZoomMatrix(long pivotID, long moveID) {
         Point ptPivotLast = getLastPoint(pivotID, out bool empty);
         if (!empty) {
            Point ptMoveLast = getLastPoint(moveID, out empty);
            if (!empty) {
               Point ptMoveSecondLast = getSecondLastPoint(moveID, out empty);
               if (!empty)
                  return getIsotropicZoomMatrix(ptPivotLast,
                                                ptMoveSecondLast,
                                                ptMoveLast);
            }
         }
         return new Matrix();
      }

      /// <summary>
      /// liefert die Rotationsmatrix
      /// </summary>
      /// <param name="pivotID">ID des "festen" Fingers</param>
      /// <param name="moveID">ID des "Drehfingers"</param>
      /// <param name="angle">Drehwinkel</param>
      /// <returns></returns>
      public Matrix GetRotateMatrix(long pivotID, long moveID, out double angle) {
         Point ptPivotLast = getLastPoint(pivotID, out bool empty);
         if (!empty) {
            Point ptMoveLast = getLastPoint(moveID, out empty);
            if (!empty) {
               Point ptMoveSecondLast = getSecondLastPoint(moveID, out empty);
               if (!empty)
                  return getRotateMatrix(ptPivotLast,
                                         ptMoveSecondLast,
                                         ptMoveLast,
                                         out angle);
            }
         }
         angle = 0;
         return new Matrix();
      }

      /// <summary>
      /// Es dürfen intern nur für 2 Finger Punkte registriert sein. Der "feste" Punkt wird vom gerade nicht "bewegten" Finger geliefert.
      /// </summary>
      /// <param name="moveID">ID des "bewegten" Fingers</param>
      /// <returns></returns>
      public Matrix Get2FingerAnisotropicZoomMatrix(long moveID) {
         long[] id = data4id.IDList();
         if (id.Length == 2)
            return GetAnisotropicZoomMatrix(id[0] == moveID ? id[1] : id[0], moveID);
         return new Matrix();
      }

      /// <summary>
      /// Es dürfen intern nur für 2 Finger Punkte registriert sein. Der "feste" Punkt wird vom gerade nicht "bewegten" Finger geliefert.
      /// </summary>
      /// <param name="moveID">ID des "bewegten" Fingers</param>
      /// <returns></returns>
      public Matrix Get2FingerIsotropicZoomMatrix(long moveID) {
         long[] id = data4id.IDList();
         if (id.Length == 2)
            return GetIsotropicZoomMatrix(id[0] == moveID ? id[1] : id[0], moveID);
         return new Matrix();
      }

      /// <summary>
      /// transformiert einen einzelnen Punkt mit der Matrix
      /// </summary>
      /// <param name="m"></param>
      /// <param name="org"></param>
      /// <returns>neuer Punkt</returns>
      public static Point Transform(Matrix m, Point org) => m.Transform(org);

      /// <summary>
      /// transformiert ein Punkt-Array mit der Matrix
      /// </summary>
      /// <param name="m"></param>
      /// <param name="org"></param>
      public static void Transform(Matrix m, Point[] org) => m.Transform(org);

      /// <summary>
      /// liefert eine Zeichenkette für die ersten 2 Zeilen der Matrix (für debuggen)
      /// </summary>
      /// <param name="m"></param>
      /// <returns></returns>
      public static string MatrixToString(Matrix m) {
         StringBuilder sb = new StringBuilder();
         sb.AppendFormat(CultureInfo.InvariantCulture, "[{0,15:F3}", m.M11);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}", m.M21);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}", m.OffsetX);
         sb.AppendFormat(CultureInfo.InvariantCulture, " / {0,15:F3}", m.M12);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}", m.M22);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}]", m.OffsetY);
         return sb.ToString();
      }

      #endregion

      #region protected-Funktionen

      #region Matrix-Funktionen

      protected static Matrix getTranslateMatrix(Point ptFrom, Point ptTo) {
         return new Matrix(0, 0, 0, 0, ptTo.X - ptFrom.X, ptTo.Y - ptFrom.Y);
      }

      /// <summary>
      /// liefert die Zoommatrix
      /// <para>ACHTUNG: Der Zoom horizontal und vertikal muss NICHT identisch sein.</para>
      /// </summary>
      /// <param name="ptPivot">"fester" Finger</param>
      /// <param name="ptFrom">Startpunkt "beweglicher" Finger</param>
      /// <param name="ptTo">Endpunkt "beweglicher" Finger</param>
      /// <returns></returns>
      protected static Matrix getAnisotropicZoomMatrix(Point ptPivot, Point ptFrom, Point ptTo) {
         double scx = (ptTo.X - ptPivot.X) / (ptFrom.X - ptPivot.X);
         double scy = (ptTo.Y - ptPivot.Y) / (ptFrom.Y - ptPivot.Y);
         if (double.IsNaN(scx) || double.IsInfinity(scx) ||
             double.IsNaN(scy) || double.IsInfinity(scy))
            return new Matrix();

         Matrix m = new Matrix(1, 0, 0, 1, -ptPivot.X, -ptPivot.Y);  // Verschiebung, so dass Pivot im Koordinatneursprung liegt
         m.Append(new Matrix(scx, 0, 0, scy, 0, 0));                 // Streckung
         m.Append(new Matrix(1, 0, 0, 1, ptPivot.X, ptPivot.Y));     // Zurück-Verschiebung

         //Point pve = Transform(m1, ptPivot);
         //Point p1e = Transform(m1, ptFrom);
         //Debug.WriteLine("::: " + ptPivot + " <-> " + pve);
         //Debug.WriteLine("::: " + ptTo + " <-> " + p1e);

         return m;
      }

      /// <summary>
      /// liefert die Zoommatrix (aus der Relation der Entfernungen zum festen Punkt)
      /// </summary>
      /// <param name="ptPivot">"fester" Finger</param>
      /// <param name="ptFrom">Startpunkt "beweglicher" Finger</param>
      /// <param name="ptTo">Endpunkt "beweglicher" Finger</param>
      /// <returns></returns>
      protected static Matrix getIsotropicZoomMatrix(Point ptPivot, Point ptFrom, Point ptTo) {
         double scale = length(ptPivot, ptTo) / length(ptPivot, ptFrom);
         if (double.IsNaN(scale) || double.IsInfinity(scale))
            return new Matrix();
         Matrix m = new Matrix(1, 0, 0, 1, -ptPivot.X, -ptPivot.Y);  // Verschiebung, so dass Pivot im Koordinatneursprung liegt
         m.Append(new Matrix(scale, 0, 0, scale, 0, 0));             // Streckung
         m.Append(new Matrix(1, 0, 0, 1, ptPivot.X, ptPivot.Y));     // Zurück-Verschiebung
         return m;
      }

      /// <summary>
      /// liefert die Rotationsmatrix
      /// </summary>
      /// <param name="ptPivot">"fester" Finger</param>
      /// <param name="ptFrom">Startpunkt "beweglicher" Finger</param>
      /// <param name="ptTo">Endpunkt "beweglicher" Finger</param>
      /// <param name="angle">Drehwinkel</param>
      /// <returns></returns>
      protected static Matrix getRotateMatrix(Point ptPivot, Point ptFrom, Point ptTo, out double angle) {
         // Calculate two vectors
         Point oldVector = subtract(ptFrom, ptPivot);
         Point newVector = subtract(ptTo, ptPivot);

         // Find angles from pivot point to touch points
         double oldAngle = Math.Atan2(oldVector.Y, oldVector.X);
         double newAngle = Math.Atan2(newVector.Y, newVector.X);

         // Calculate rotation matrix
         angle = newAngle - oldAngle;
         Matrix touchMatrix = new Matrix();
         touchMatrix.RotateAt(angle, ptPivot.X, ptPivot.Y);

         // Effectively rotate the old vector
         double magnitudeRatio = length(oldVector) / length(newVector);
         oldVector.X = magnitudeRatio * newVector.X;
         oldVector.Y = magnitudeRatio * newVector.Y;

         // Isotropic scaling!
         double scale = 1 / magnitudeRatio;

         if (!double.IsNaN(scale) && !double.IsInfinity(scale))
            touchMatrix.Append(new Matrix(scale, 0, 0, scale, ptPivot.X, ptPivot.Y));

         return touchMatrix;
      }

      #endregion

      /// <summary>
      /// Entfernung zum Koordinatenursprung (Länge des Vektors)
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      protected static double length(Point pt) {
         return pt.Distance(Point.Zero);
      }

      /// <summary>
      /// Entfernung zwischen den beiden Punkten (Länge des Vektors)
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns></returns>
      protected static double length(Point pt1, Point pt2) {
         return new Point(pt2.X - pt1.X, pt2.Y - pt1.Y).Distance(Point.Zero);
      }

      /// <summary>
      /// pt1 - pt2
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns></returns>
      protected static Point subtract(Point pt1, Point pt2) {
         return pt1.Offset(-pt2.X, -pt2.Y);
      }

      protected static Point getDelta(Point pstart, Point pend) {
         return new Point(pend.X - pstart.X,
                          pend.Y - pstart.Y);
      }

      protected static double getDelay(DateTime start, DateTime end) {
         return end.Subtract(start).TotalMilliseconds;
      }

      protected bool isDelta4Tap(Point delta) {
         return Math.Abs(delta.X) <= Delta4Tapped.X &&
                Math.Abs(delta.Y) <= Delta4Tapped.Y;
      }

      protected bool isDelta4MultiTap(Point delta) {
         return Math.Abs(delta.X) <= Delta4MultiTapped.X &&
                Math.Abs(delta.Y) <= Delta4MultiTapped.Y;
      }

      protected bool isDelay4ShortTap(double ms) {
         return Math.Abs(ms) <= MaxDelay4ShortTapped;
      }

      protected bool isDelay4MultiTap(double ms) {
         return Math.Abs(ms) <= MaxDelay4MultiTapped;
      }

      protected bool isSamePosition(Point pt1, Point pt2) {
         return pt1.X == pt2.X && pt1.Y == pt2.Y;
      }

      #endregion

   }
}
