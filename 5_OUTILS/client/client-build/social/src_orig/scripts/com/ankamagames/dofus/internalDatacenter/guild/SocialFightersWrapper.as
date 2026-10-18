package com.ankamagames.dofus.internalDatacenter.guild
{
   import com.ankamagames.dofus.network.types.game.character.CharacterMinimalPlusLookInformations;
   
   public class SocialFightersWrapper
   {
      
      public function SocialFightersWrapper()
      {
         super();
      }
      
      public function get ally() : uint
      {
         return new uint();
      }
      
      public function get playerCharactersInformations() : CharacterMinimalPlusLookInformations
      {
         return new CharacterMinimalPlusLookInformations();
      }
      
      public function get entityLook() : Object
      {
         return new Object();
      }
   }
}

