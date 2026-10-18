package utils
{
   import com.ankamagames.dofus.network.types.game.character.CharacterMinimalPlusLookInformations;
   import d2actions.GuildFightJoinRequest;
   import d2actions.GuildFightLeaveRequest;
   import d2actions.GuildFightTakePlaceRequest;
   import d2actions.PrismFightJoinLeaveRequest;
   import d2actions.PrismFightSwapRequest;
   
   public class JoinFightUtil
   {
      
      private static const TYPE_TAX_COLLECTOR:int = 0;
      
      private static const TYPE_PRISM:int = 1;
      
      public function JoinFightUtil()
      {
         super();
      }
      
      public static function join(pFightType:int, pFightId:Number) : void
      {
         if(Api.social.isPlayerDefender(pFightType,Api.player.id(),pFightId))
         {
            leave(pFightType,pFightId);
         }
         else if(pFightType == TYPE_TAX_COLLECTOR)
         {
            Api.system.sendAction(new GuildFightJoinRequest(pFightId));
         }
         else if(pFightType == TYPE_PRISM)
         {
            Api.system.sendAction(new PrismFightJoinLeaveRequest(pFightId,true));
         }
      }
      
      public static function leave(pFightType:int, pFightId:Number) : void
      {
         if(pFightType == TYPE_TAX_COLLECTOR)
         {
            Api.system.sendAction(new GuildFightLeaveRequest(pFightId,Api.player.id()));
         }
         else if(pFightType == TYPE_PRISM)
         {
            Api.system.sendAction(new PrismFightJoinLeaveRequest(pFightId,false));
         }
      }
      
      public static function swapPlaces(pFightType:int, pFightId:Number, pTarget:CharacterMinimalPlusLookInformations) : void
      {
         if(pFightType == TYPE_TAX_COLLECTOR)
         {
            Api.system.sendAction(new GuildFightTakePlaceRequest(pFightId,pTarget.id));
         }
         else if(pFightType == TYPE_PRISM)
         {
            Api.system.sendAction(new PrismFightSwapRequest(pFightId,pTarget.id));
         }
      }
   }
}

