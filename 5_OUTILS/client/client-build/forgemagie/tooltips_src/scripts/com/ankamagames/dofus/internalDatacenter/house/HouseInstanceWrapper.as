package com.ankamagames.dofus.internalDatacenter.house
{
   import com.ankamagames.dofus.internalDatacenter.guild.GuildWrapper;
   
   public class HouseInstanceWrapper
   {
      
      public function HouseInstanceWrapper()
      {
         super();
      }
      
      public function get id() : int
      {
         return new int();
      }
      
      public function get ownerName() : String
      {
         return new String();
      }
      
      public function get price() : Number
      {
         return new Number();
      }
      
      public function get isOnSale() : Boolean
      {
         return new Boolean();
      }
      
      public function get isSaleLocked() : Boolean
      {
         return new Boolean();
      }
      
      public function get isLocked() : Boolean
      {
         return new Boolean();
      }
      
      public function get guildIdentity() : GuildWrapper
      {
         return new GuildWrapper();
      }
      
      public function get indexOfThisInstanceForTheOwner() : int
      {
         return new int();
      }
      
      public function get label() : String
      {
         return null;
      }
      
      public function get isMine() : Boolean
      {
         return false;
      }
      
      public function get belongsToMyGuild() : Boolean
      {
         return false;
      }
   }
}

