package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.internalDatacenter.communication.BasicChatSentence;
   import com.ankamagames.dofus.internalDatacenter.conquest.PrismSubAreaWrapper;
   import com.ankamagames.dofus.internalDatacenter.dare.DareWrapper;
   import com.ankamagames.dofus.internalDatacenter.guild.AllianceWrapper;
   import com.ankamagames.dofus.internalDatacenter.guild.GuildFactSheetWrapper;
   import com.ankamagames.dofus.internalDatacenter.guild.GuildWrapper;
   import com.ankamagames.dofus.internalDatacenter.guild.SocialEntityInFightWrapper;
   import com.ankamagames.dofus.internalDatacenter.guild.TaxCollectorWrapper;
   import com.ankamagames.dofus.internalDatacenter.people.SpouseWrapper;
   import com.ankamagames.dofus.network.types.game.prism.PrismInformation;
   
   public class SocialApi
   {
      
      public function SocialApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function hasSocialFrame() : Boolean
      {
         return false;
      }
      
      public function getFriendsList() : Array
      {
         return null;
      }
      
      public function getContactsList() : Array
      {
         return null;
      }
      
      public function isFriend(playerName:String) : Boolean
      {
         return false;
      }
      
      public function isContact(playerName:String) : Boolean
      {
         return false;
      }
      
      public function isFriendOrContact(playerName:String) : Boolean
      {
         return false;
      }
      
      public function getEnemiesList() : Array
      {
         return null;
      }
      
      public function isEnemy(playerName:String) : Boolean
      {
         return false;
      }
      
      public function getIgnoredList() : Array
      {
         return null;
      }
      
      public function isIgnored(name:String, accountId:int = 0) : Boolean
      {
         return false;
      }
      
      public function getAccountName(name:String) : String
      {
         return null;
      }
      
      public function getWarnOnFriendConnec() : Boolean
      {
         return false;
      }
      
      public function getShareStatus() : Boolean
      {
         return false;
      }
      
      public function getWarnOnMemberConnec() : Boolean
      {
         return false;
      }
      
      public function getWarnWhenFriendOrGuildMemberLvlUp() : Boolean
      {
         return false;
      }
      
      public function getWarnWhenFriendOrGuildMemberAchieve() : Boolean
      {
         return false;
      }
      
      public function getWarnOnHardcoreDeath() : Boolean
      {
         return false;
      }
      
      public function getSpouse() : SpouseWrapper
      {
         return null;
      }
      
      public function hasSpouse() : Boolean
      {
         return false;
      }
      
      public function getAllowedGuildEmblemSymbolCategories() : int
      {
         return 0;
      }
      
      public function hasGuild() : Boolean
      {
         return false;
      }
      
      public function getGuild() : GuildWrapper
      {
         return null;
      }
      
      public function getGuildMembers() : Object
      {
         return null;
      }
      
      public function getGuildRights() : Array
      {
         return null;
      }
      
      public function getGuildByid(id:int) : GuildFactSheetWrapper
      {
         return null;
      }
      
      public function hasGuildRight(pPlayerId:Number, pRightId:String) : Boolean
      {
         return false;
      }
      
      public function hasGuildRank(pPlayerId:Number, rankId:int) : Boolean
      {
         return false;
      }
      
      public function getGuildHouses() : Object
      {
         return null;
      }
      
      public function guildHousesUpdateNeeded() : Boolean
      {
         return false;
      }
      
      public function getGuildPaddocks() : Object
      {
         return null;
      }
      
      public function getMaxGuildPaddocks() : int
      {
         return 0;
      }
      
      public function isGuildNameInvalid() : Boolean
      {
         return false;
      }
      
      public function getMaxCollectorCount() : uint
      {
         return 0;
      }
      
      public function getTaxCollectors() : Object
      {
         return null;
      }
      
      public function getTaxCollector(id:Number) : TaxCollectorWrapper
      {
         return null;
      }
      
      public function getGuildFightingTaxCollectors() : Object
      {
         return null;
      }
      
      public function getGuildFightingTaxCollector(pFightId:Number) : SocialEntityInFightWrapper
      {
         return null;
      }
      
      public function getAllFightingTaxCollectors() : Object
      {
         return null;
      }
      
      public function getAllFightingTaxCollector(pFightId:Number) : SocialEntityInFightWrapper
      {
         return null;
      }
      
      public function isPlayerDefender(pType:int, pPlayerId:Number, pSocialFightId:Number) : Boolean
      {
         return false;
      }
      
      public function hasAlliance() : Boolean
      {
         return false;
      }
      
      public function getAlliance() : AllianceWrapper
      {
         return null;
      }
      
      public function getAllianceById(id:int) : AllianceWrapper
      {
         return null;
      }
      
      public function getAllianceGuilds() : Object
      {
         return null;
      }
      
      public function isAllianceNameInvalid() : Boolean
      {
         return false;
      }
      
      public function isAllianceTagInvalid() : Boolean
      {
         return false;
      }
      
      public function getAllianceNameAndTag(pPrismInfo:PrismInformation) : String
      {
         return null;
      }
      
      public function getPrismSubAreaById(id:int) : PrismSubAreaWrapper
      {
         return null;
      }
      
      public function getFightingPrisms() : Object
      {
         return null;
      }
      
      public function getFightingPrism(pFightId:uint) : SocialEntityInFightWrapper
      {
         return null;
      }
      
      public function isPlayerPrismDefender(pPlayerId:Number, pSubAreaId:int) : Boolean
      {
         return false;
      }
      
      public function getChatSentence(timestamp:Number, fingerprint:String) : BasicChatSentence
      {
         return null;
      }
      
      public function getDareList() : Object
      {
         return null;
      }
      
      public function getDareById(dareId:Number) : DareWrapper
      {
         return null;
      }
      
      public function getTotalGuildDares() : uint
      {
         return 0;
      }
      
      public function getTotalAllianceDares() : uint
      {
         return 0;
      }
      
      public function getTotalDaresInSubArea(subAreaId:uint) : uint
      {
         return 0;
      }
      
      public function getTargetedMonsterIdsInSubArea(subAreaId:uint) : Object
      {
         return null;
      }
      
      public function getFilteredDareList(subscribableByPlayer:Boolean = true, createdByPlayer:Boolean = false, subscribedByPlayer:Boolean = false, searchQuery:String = "", searchOnId:Boolean = false, searchOnCreatorName:Boolean = false, searchOnMonsterName:Boolean = false, searchOnCriteria:Boolean = false, searchOnSubArea:Boolean = false, searchOnGuildName:Boolean = false, searchOnAllianceName:Boolean = false) : Array
      {
         return null;
      }
      
      public function getDareRewards() : Object
      {
         return null;
      }
   }
}

