package com.ankamagames.dofus.internalDatacenter.guild
{
   public class PaddockInstanceWrapper
   {
      
      public function PaddockInstanceWrapper()
      {
         super();
      }
      
      public function get price() : Number
      {
         return new Number();
      }
      
      public function get guildIdentity() : GuildWrapper
      {
         return new GuildWrapper();
      }
      
      public function get isSaleLocked() : Boolean
      {
         return new Boolean();
      }
      
      public function get isAbandonned() : Boolean
      {
         return new Boolean();
      }
      
      public function get priceString() : String
      {
         return null;
      }
      
      public function get guildNameForSorting() : String
      {
         return null;
      }
   }
}

