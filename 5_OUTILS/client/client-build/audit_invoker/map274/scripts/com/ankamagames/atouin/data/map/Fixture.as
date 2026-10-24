package com.ankamagames.atouin.data.map
{
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.geom.Point;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class Fixture
   {
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(Fixture));
      
      public var fixtureId:int;
      
      public var offset:Point;
      
      public var hue:int;
      
      public var redMultiplier:int;
      
      public var greenMultiplier:int;
      
      public var blueMultiplier:int;
      
      public var alpha:uint;
      
      public var xScale:int;
      
      public var yScale:int;
      
      public var rotation:int;
      
      private var _map:Map;
      
      public function Fixture(param1:Map)
      {
         super();
         this._map = param1;
      }
      
      public function get map() : Map
      {
         return this._map;
      }
      
      public function fromRaw(param1:IDataInput) : void
      {
         try
         {
            this.fixtureId = param1.readInt();
            this.offset = new Point();
            this.offset.x = param1.readShort();
            this.offset.y = param1.readShort();
            this.rotation = param1.readShort();
            this.xScale = param1.readShort();
            this.yScale = param1.readShort();
            this.redMultiplier = param1.readByte();
            this.greenMultiplier = param1.readByte();
            this.blueMultiplier = param1.readByte();
            this.hue = this.redMultiplier | this.greenMultiplier | this.blueMultiplier;
            this.alpha = param1.readUnsignedByte();
         }
         catch(e:*)
         {
            throw e;
         }
      }
   }
}

