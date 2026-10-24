package com.ankamagames.atouin.data.elements.subtypes
{
   import com.ankamagames.atouin.data.elements.GraphicalElementData;
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.geom.Point;
   import flash.utils.Dictionary;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class NormalGraphicalElementData extends GraphicalElementData
   {
      
      public static var MEMORY_LOG:Dictionary = new Dictionary(true);
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(NormalGraphicalElementData));
      
      public var gfxId:int;
      
      public var height:uint;
      
      public var horizontalSymmetry:Boolean;
      
      public var origin:Point;
      
      public var size:Point;
      
      public function NormalGraphicalElementData(param1:int, param2:int)
      {
         super(param1,param2);
         MEMORY_LOG[this] = 1;
      }
      
      override public function fromRaw(param1:IDataInput, param2:int) : void
      {
         this.gfxId = param1.readInt();
         this.height = param1.readByte();
         this.horizontalSymmetry = param1.readBoolean();
         this.origin = new Point(param1.readShort(),param1.readShort());
         this.size = new Point(param1.readShort(),param1.readShort());
      }
   }
}

