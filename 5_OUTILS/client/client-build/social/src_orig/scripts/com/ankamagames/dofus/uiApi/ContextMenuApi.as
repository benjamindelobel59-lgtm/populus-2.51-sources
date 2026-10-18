package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.berilia.types.data.ContextMenuData;
   
   public class ContextMenuApi
   {
      
      public function ContextMenuApi()
      {
         super();
      }
      
      public function registerMenuMaker(makerName:String, makerClass:Class) : void
      {
      }
      
      public function create(data:*, makerName:String = null, makerParams:Array = null) : ContextMenuData
      {
         return null;
      }
      
      public function getMenuMaker(makerName:String) : Object
      {
         return null;
      }
   }
}

