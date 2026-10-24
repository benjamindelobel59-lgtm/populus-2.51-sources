package com.ankamagames.atouin.data.map
{
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class CellData
   {
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(CellData));
      
      public var id:uint;
      
      public var speed:int;
      
      public var mapChangeData:uint;
      
      public var moveZone:uint;
      
      private var _losmov:int = 3;
      
      private var _floor:int;
      
      private var _map:Map;
      
      private var _arrow:int = 0;
      
      private var _linkedZone:int;
      
      private var _mov:Boolean;
      
      private var _los:Boolean;
      
      private var _nonWalkableDuringFight:Boolean;
      
      private var _red:Boolean;
      
      private var _blue:Boolean;
      
      private var _farmCell:Boolean;
      
      private var _havenbagCell:Boolean;
      
      private var _visible:Boolean;
      
      private var _nonWalkableDuringRP:Boolean;
      
      public function CellData(param1:Map, param2:uint)
      {
         super();
         this.id = param2;
         this._map = param1;
      }
      
      public function get map() : Map
      {
         return this._map;
      }
      
      public function get mov() : Boolean
      {
         return this._mov;
      }
      
      public function get los() : Boolean
      {
         return this._los;
      }
      
      public function get nonWalkableDuringFight() : Boolean
      {
         return this._nonWalkableDuringFight;
      }
      
      public function get red() : Boolean
      {
         return this._red;
      }
      
      public function get blue() : Boolean
      {
         return this._blue;
      }
      
      public function get farmCell() : Boolean
      {
         return this._farmCell;
      }
      
      public function get havenbagCell() : Boolean
      {
         return this._havenbagCell;
      }
      
      public function get visible() : Boolean
      {
         return this._visible;
      }
      
      public function get nonWalkableDuringRP() : Boolean
      {
         return this._nonWalkableDuringRP;
      }
      
      public function get floor() : int
      {
         return this._floor;
      }
      
      public function get useTopArrow() : Boolean
      {
         return (this._arrow & 1) != 0;
      }
      
      public function get useBottomArrow() : Boolean
      {
         return (this._arrow & 2) != 0;
      }
      
      public function get useRightArrow() : Boolean
      {
         return (this._arrow & 4) != 0;
      }
      
      public function get useLeftArrow() : Boolean
      {
         return (this._arrow & 8) != 0;
      }
      
      public function hasLinkedZoneRP() : Boolean
      {
         return this.mov && !this.farmCell;
      }
      
      public function get linkedZoneRP() : int
      {
         return (this._linkedZone & 0xF0) >> 4;
      }
      
      public function hasLinkedZoneFight() : Boolean
      {
         return this.mov && !this.nonWalkableDuringFight && !this.farmCell && !this.havenbagCell;
      }
      
      public function get linkedZoneFight() : int
      {
         return this._linkedZone & 0x0F;
      }
      
      public function fromRaw(param1:IDataInput) : void
      {
         var _loc5_:int = 0;
         var _loc2_:Boolean = false;
         var _loc4_:Boolean = false;
         var _loc7_:Boolean = false;
         var _loc6_:Boolean = false;
         var _loc3_:int = 0;
         try
         {
            this._floor = param1.readByte() * 10;
            if(this._floor == -1280)
            {
               return;
            }
            if(this._map.mapVersion >= 9)
            {
               _loc5_ = param1.readShort();
               this._mov = (_loc5_ & 1) == 0;
               this._nonWalkableDuringFight = (_loc5_ & 2) != 0;
               this._nonWalkableDuringRP = (_loc5_ & 4) != 0;
               this._los = (_loc5_ & 8) == 0;
               this._blue = (_loc5_ & 0x10) != 0;
               this._red = (_loc5_ & 0x20) != 0;
               this._visible = (_loc5_ & 0x40) != 0;
               this._farmCell = (_loc5_ & 0x80) != 0;
               if(this.map.mapVersion >= 10)
               {
                  this._havenbagCell = (_loc5_ & 0x0100) != 0;
                  _loc2_ = (_loc5_ & 0x0200) != 0;
                  _loc4_ = (_loc5_ & 0x0400) != 0;
                  _loc7_ = (_loc5_ & 0x0800) != 0;
                  _loc6_ = (_loc5_ & 0x1000) != 0;
               }
               else
               {
                  _loc2_ = (_loc5_ & 0x0100) != 0;
                  _loc4_ = (_loc5_ & 0x0200) != 0;
                  _loc7_ = (_loc5_ & 0x0400) != 0;
                  _loc6_ = (_loc5_ & 0x0800) != 0;
               }
               if(_loc2_)
               {
                  this._map.topArrowCell.push(this.id);
               }
               if(_loc4_)
               {
                  this._map.bottomArrowCell.push(this.id);
               }
               if(_loc7_)
               {
                  this._map.rightArrowCell.push(this.id);
               }
               if(_loc6_)
               {
                  this._map.leftArrowCell.push(this.id);
               }
            }
            else
            {
               this._losmov = param1.readUnsignedByte();
               this._los = (this._losmov & 2) >> 1 == 1;
               this._mov = (this._losmov & 1) == 1;
               this._visible = (this._losmov & 0x40) >> 6 == 1;
               this._farmCell = (this._losmov & 0x20) >> 5 == 1;
               this._blue = (this._losmov & 0x10) >> 4 == 1;
               this._red = (this._losmov & 8) >> 3 == 1;
               this._nonWalkableDuringRP = (this._losmov & 0x80) >> 7 == 1;
               this._nonWalkableDuringFight = (this._losmov & 4) >> 2 == 1;
            }
            this.speed = param1.readByte();
            this.mapChangeData = param1.readByte();
            if(this._map.mapVersion > 5)
            {
               this.moveZone = param1.readUnsignedByte();
            }
            if(this._map.mapVersion > 10 && (this.hasLinkedZoneRP() || this.hasLinkedZoneFight()))
            {
               this._linkedZone = param1.readUnsignedByte();
            }
            if(this._map.mapVersion > 7 && this.map.mapVersion < 9)
            {
               _loc3_ = param1.readByte();
               this._arrow = 0x0F & _loc3_;
               if(this.useTopArrow)
               {
                  this._map.topArrowCell.push(this.id);
               }
               if(this.useBottomArrow)
               {
                  this._map.bottomArrowCell.push(this.id);
               }
               if(this.useLeftArrow)
               {
                  this._map.leftArrowCell.push(this.id);
               }
               if(this.useRightArrow)
               {
                  this._map.rightArrowCell.push(this.id);
               }
            }
         }
         catch(e:*)
         {
            throw e;
         }
      }
      
      public function toString() : String
      {
         return "map : " + this._map.id + " CellId : " + this.id + " mov : " + this._mov + " los : " + this._los + " nonWalkableDuringFight : " + this._nonWalkableDuringFight + " nonWalkableDuringRp : " + this._nonWalkableDuringRP + " farmCell : " + this._farmCell + " havenbagCell: " + this._havenbagCell + " visbile : " + this._visible + " speed: " + this.speed + " moveZone: " + this.moveZone + " linkedZoneId: " + this._linkedZone;
      }
   }
}

