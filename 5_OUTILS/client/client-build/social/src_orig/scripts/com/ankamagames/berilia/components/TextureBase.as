package com.ankamagames.berilia.components
{
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   
   public class TextureBase extends GraphicContainer
   {
      
      public function TextureBase()
      {
         super();
      }
      
      public function get useCache() : Boolean
      {
         return false;
      }
      
      public function set useCache(value:Boolean) : void
      {
      }
      
      public function get showLoadingError() : Boolean
      {
         return false;
      }
      
      public function set showLoadingError(value:Boolean) : void
      {
      }
      
      public function get autoCenterBitmap() : Boolean
      {
         return false;
      }
      
      public function set autoCenterBitmap(value:Boolean) : void
      {
      }
      
      public function get smooth() : Boolean
      {
         return false;
      }
      
      public function set smooth(value:Boolean) : void
      {
      }
      
      public function get uri() : Object
      {
         return null;
      }
      
      public function set uri(uri:Object) : void
      {
      }
      
      public function loadBitmapData(bmpdt:Object) : void
      {
      }
   }
}

