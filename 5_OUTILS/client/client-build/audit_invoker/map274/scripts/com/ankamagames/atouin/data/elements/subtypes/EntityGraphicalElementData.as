package com.ankamagames.atouin.data.elements.subtypes
{
   import com.ankamagames.atouin.data.elements.GraphicalElementData;
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import flash.utils.IDataInput;
   import flash.utils.getQualifiedClassName;
   
   public class EntityGraphicalElementData extends GraphicalElementData
   {
      
      protected static const _log:Logger = Log.getLogger(getQualifiedClassName(EntityGraphicalElementData));
      
      public var entityLook:String;
      
      public var horizontalSymmetry:Boolean;
      
      public var playAnimation:Boolean;
      
      public var playAnimStatic:Boolean;
      
      public var minDelay:uint;
      
      public var maxDelay:uint;
      
      public function EntityGraphicalElementData(param1:int, param2:int)
      {
         super(param1,param2);
      }
      
      override public function fromRaw(param1:IDataInput, param2:int) : void
      {
         var _loc3_:uint = uint(param1.readInt());
         this.entityLook = param1.readUTFBytes(_loc3_);
         this.horizontalSymmetry = param1.readBoolean();
         if(param2 >= 7)
         {
            this.playAnimation = param1.readBoolean();
         }
         if(param2 >= 6)
         {
            this.playAnimStatic = param1.readBoolean();
         }
         if(param2 >= 5)
         {
            this.minDelay = param1.readInt();
            this.maxDelay = param1.readInt();
         }
      }
   }
}

