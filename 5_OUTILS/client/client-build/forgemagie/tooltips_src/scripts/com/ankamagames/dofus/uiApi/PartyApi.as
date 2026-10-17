package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.internalDatacenter.arena.ArenaRankInfosWrapper;
   
   public class PartyApi
   {
      
      public function PartyApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getPartyMembers(typeId:int = 0) : Object
      {
         return null;
      }
      
      public function getPartyLeaderId(partyId:int) : Number
      {
         return 0;
      }
      
      public function isInParty(pPlayerId:Number) : Boolean
      {
         return false;
      }
      
      public function getPartyId() : int
      {
         return 0;
      }
      
      public function isArenaRegistered() : Boolean
      {
         return false;
      }
      
      public function getArenaCurrentStatus() : int
      {
         return 0;
      }
      
      public function getArenaPartyId() : int
      {
         return 0;
      }
      
      public function getArenaLeader() : Object
      {
         return null;
      }
      
      public function getArenaReadyPartyMemberIds() : Object
      {
         return null;
      }
      
      public function getArenaAlliesIds() : Object
      {
         return null;
      }
      
      public function getArenaRankSoloInfos() : ArenaRankInfosWrapper
      {
         return null;
      }
      
      public function getArenaRankGroupInfos() : ArenaRankInfosWrapper
      {
         return null;
      }
      
      public function getArenaRankDuelInfos() : ArenaRankInfosWrapper
      {
         return null;
      }
      
      public function getAllMemberFollowPlayerId(partyId:int) : Number
      {
         return 0;
      }
      
      public function getPartyLoyalty(partyId:int) : Boolean
      {
         return false;
      }
      
      public function getAllSubscribedDungeons() : Object
      {
         return null;
      }
   }
}

