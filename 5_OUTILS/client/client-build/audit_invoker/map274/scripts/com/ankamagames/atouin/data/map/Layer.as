package com.ankamagames.atouin.data.map
{
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class Layer
   {
      
      public static const LAYER_GROUND:uint = 0;
      
      public static const LAYER_ADDITIONAL_GROUND:uint = 1;
      
      public static const LAYER_DECOR:uint = 2;
      
      public static const LAYER_ADDITIONAL_DECOR:uint = 3;
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(Layer));
      
      public var layerId:int;
      
      public var cellsCount:int;
      
      public var cells:Vector.<Cell>;
      
      private var _map:Map;
      
      public function Layer(param1:Map)
      {
         super();
         this._map = param1;
      }
      
      public function get map() : Map
      {
         return this._map;
      }
      
      public function fromRaw(param1:IDataInput, param2:int) : void
      {
         var _loc6_:int = 0;
         var _loc3_:Cell = null;
         var _loc4_:int = 0;
         var _loc5_:Cell = null;
         try
         {
            if(param2 >= 9)
            {
               this.layerId = param1.readByte();
            }
            else
            {
               this.layerId = param1.readInt();
            }
            this.cellsCount = param1.readShort();
            this.cells = new Vector.<Cell>(this.cellsCount,true);
            if(this.cellsCount > 0)
            {
               _loc6_ = 0;
               while(_loc6_ < this.cellsCount)
               {
                  _loc3_ = new Cell(this);
                  _loc3_.fromRaw(param1,param2);
                  this.cells[_loc6_] = _loc3_;
                  _loc6_ += 1;
               }
               _loc4_ = 560 - 1;
               if(_loc3_.cellId < _loc4_)
               {
                  _loc5_ = Cell.createEmptyCell(this,_loc4_);
                  this.cells.fixed = false;
                  this.cells.push(_loc5_);
                  this.cells.fixed = true;
               }
            }
         }
         catch(e:*)
         {
            throw e;
         }
      }
   }
}

