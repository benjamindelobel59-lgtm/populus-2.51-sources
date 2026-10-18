package com.ankamagames.berilia.components
{
   public class TextureBitmap extends TextureBase
   {
      
      public function TextureBitmap()
      {
         super();
      }
      
      public function get dynamicRender() : Boolean
      {
         return false;
      }
      
      public function set dynamicRender(value:Boolean) : void
      {
      }
      
      public function get tileRender() : Boolean
      {
         return false;
      }
      
      public function set tileRender(value:Boolean) : void
      {
      }
      
      public function get margin() : Object
      {
         return null;
      }
      
      public function set margin(value:Object) : void
      {
      }
      
      public function set align(str:String) : void
      {
      }
      
      public function get align() : String
      {
         return null;
      }
      
      public function invalidate() : void
      {
      }
   }
}

