package com.ankamagames.berilia.components
{
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   
   public class SwfApplication extends GraphicContainer
   {
      
      public function SwfApplication()
      {
         super();
      }
      
      public function set uri(v:Object) : void
      {
      }
      
      public function get uri() : Object
      {
         return null;
      }
      
      public function setByte(bytes:Object, accessKey:Object) : void
      {
      }
      
      public function get loadedHandler() : Function
      {
         return new Function();
      }
      
      public function set loadedHandler(v:Function) : void
      {
      }
      
      public function get loadErrorHandler() : Function
      {
         return new Function();
      }
      
      public function set loadErrorHandler(v:Function) : void
      {
      }
      
      public function get loadProgressHandler() : Function
      {
         return new Function();
      }
      
      public function set loadProgressHandler(v:Function) : void
      {
      }
      
      public function get autoResize() : Boolean
      {
         return new Boolean();
      }
      
      public function set autoResize(v:Boolean) : void
      {
      }
      
      public function bindApi(propertyName:String, value:*) : Boolean
      {
         return false;
      }
      
      public function dispatchEventToApp(eventName:String, param:Object) : void
      {
      }
   }
}

