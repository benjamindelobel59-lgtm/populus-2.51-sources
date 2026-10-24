package com.ankamagames.atouin.data.elements.subtypes
{
   import flash.utils.IDataInput;
   
   public class AnimatedGraphicalElementData extends NormalGraphicalElementData
   {
      
      public var minDelay:uint;
      
      public var maxDelay:uint;
      
      public function AnimatedGraphicalElementData(param1:int, param2:int)
      {
         super(param1,param2);
      }
      
      override public function fromRaw(param1:IDataInput, param2:int) : void
      {
         super.fromRaw(param1,param2);
         if(param2 == 4)
         {
            this.minDelay = param1.readInt();
            this.maxDelay = param1.readInt();
         }
      }
   }
}

