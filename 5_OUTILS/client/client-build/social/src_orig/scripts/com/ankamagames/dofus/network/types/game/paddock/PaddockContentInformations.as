package com.ankamagames.dofus.network.types.game.paddock
{
   public class PaddockContentInformations extends PaddockInformations
   {
      
      public function PaddockContentInformations()
      {
         super();
      }
      
      public function get paddockId() : Number
      {
         return new Number();
      }
      
      public function get worldX() : int
      {
         return new int();
      }
      
      public function get worldY() : int
      {
         return new int();
      }
      
      public function get mapId() : Number
      {
         return new Number();
      }
      
      public function get subAreaId() : uint
      {
         return new uint();
      }
      
      public function get abandonned() : Boolean
      {
         return new Boolean();
      }
      
      public function get mountsInformations() : Vector.<MountInformationsForPaddock>
      {
         return new Vector.<MountInformationsForPaddock>();
      }
   }
}

