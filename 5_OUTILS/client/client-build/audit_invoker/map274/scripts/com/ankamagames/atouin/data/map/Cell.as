package com.ankamagames.atouin.data.map
{
   import com.ankamagames.atouin.data.map.elements.BasicElement;
   import com.ankamagames.atouin.utils.CellIdConverter;
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.geom.Point;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class Cell
   {
      
      private static var _cellCoords:Point;
      
      public static const EMPTY_ELEMENTS_LIST:Vector.<BasicElement> = new Vector.<BasicElement>(0,true);
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(Cell));
      
      public var cellId:int;
      
      public var elementsCount:int;
      
      public var elements:Vector.<BasicElement>;
      
      private var _layer:Layer;
      
      public function Cell(param1:Layer)
      {
         super();
         this._layer = param1;
      }
      
      public static function cellCoords(param1:uint) : Point
      {
         if(_cellCoords == null)
         {
            _cellCoords = new Point();
         }
         _cellCoords.x = param1 % 14;
         _cellCoords.y = Math.floor(param1 / 14);
         return _cellCoords;
      }
      
      public static function cellId(param1:Point) : uint
      {
         return CellIdConverter.coordToCellId(param1.x,param1.y);
      }
      
      public static function cellIdByXY(param1:int, param2:int) : uint
      {
         return CellIdConverter.coordToCellId(param1,param2);
      }
      
      public static function cellPixelCoords(param1:uint) : Point
      {
         var _loc2_:Point = cellCoords(param1);
         _loc2_.x = _loc2_.x * 86 + (_loc2_.y % 2 == 1 ? 43 : 0);
         _loc2_.y *= 21.5;
         return _loc2_;
      }
      
      public static function createEmptyCell(param1:Layer, param2:int) : Cell
      {
         var _loc3_:Cell = new Cell(param1);
         _loc3_.cellId = param2;
         _loc3_.elementsCount = 0;
         _loc3_.elements = EMPTY_ELEMENTS_LIST;
         return _loc3_;
      }
      
      public function get layer() : Layer
      {
         return this._layer;
      }
      
      public function get coords() : Point
      {
         return CellIdConverter.cellIdToCoord(this.cellId);
      }
      
      public function get pixelCoords() : Point
      {
         return cellPixelCoords(this.cellId);
      }
      
      public function fromRaw(param1:IDataInput, param2:int) : void
      {
         var _loc3_:BasicElement = null;
         var _loc4_:int = 0;
         try
         {
            this.cellId = param1.readShort();
            this.elementsCount = param1.readShort();
            this.elements = new Vector.<BasicElement>(this.elementsCount,true);
            _loc4_ = 0;
            while(_loc4_ < this.elementsCount)
            {
               _loc3_ = BasicElement.getElementFromType(param1.readByte(),this);
               _loc3_.fromRaw(param1,param2);
               this.elements[_loc4_] = _loc3_;
               _loc4_ += 1;
            }
         }
         catch(e:*)
         {
            throw e;
         }
      }
   }
}

